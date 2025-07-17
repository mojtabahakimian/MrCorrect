using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FuzzySharp;
using System.Globalization;
using System.Windows.Threading;
using Prg_UI.Functions;

namespace Prg_UI.CUC
{
    public partial class ACT : UserControl
    {
        #region Constructor / Initialization
        private bool _suppressNextTab;
        private bool isHidedOnLoad = true;

        public ACT()
        {
            InitializeComponent();

            // رویداد را حتی اگر قبلاً Handled شده باشد، می‌گیریم
            AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnGlobalPreviewKeyDown), /* handledEventsToo */ true);

            if (IsNumberic)
            {
                _lastValidNumericValue = 0;
            }

            // Attach event handlers
            this.Loaded += OnUserControlLoaded;
            this.SizeChanged += OnSizeChanged;

        }
        /// <summary>
        /// هندلری که همیشه قبل از رسیدن به کنترل‌های داخلی اجرا می‌شود
        /// (حتی اگر رویداد قبلاً Handled شده باشد).
        /// </summary>
        private void OnGlobalPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //--------------------------------------------------
            // مرحلهٔ ۱: کاربر Enter زده ولی پنجره آن را بلعیده
            //--------------------------------------------------
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                // منطق دلخواه کنترل (مثلاً انتخاب آیتم از لیست)
                if (SEARCH_LIST.Visibility == Visibility.Visible && SEARCH_LIST.SelectedItem != null)
                {
                    SuppressAutoCompleteTemporarily();
                    AcceptSelectedItem();
                }

                // علامت می‌زنیم که Tab تزریقی را خنثی کنیم
                _suppressNextTab = true;

                // دیگر نیازی نیست e.Handled را دست‌کاری کنیم
                // چون همین الآن Handled شده است.
                return;
            }

            //--------------------------------------------------
            // مرحلهٔ ۲: پنجره یک Tab مصنوعی فرستاده است
            //--------------------------------------------------
            if (e.Key == Key.Tab && _suppressNextTab)
            {
                e.Handled = true;      // اجازهٔ جابجایی فوکوس نمی‌دهیم
                _suppressNextTab = false;
            }
        }
        private void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                // Existing logic to reposition popup when the window moves.
                window.LocationChanged += delegate
                {
                    var offset = Pop1.HorizontalOffset;
                    Pop1.HorizontalOffset = offset + 1;
                    Pop1.HorizontalOffset = offset;
                };

                // Close the popup when the window loses activation.
                window.Deactivated += (s, args) =>
                {
                    Pop1.IsOpen = false;
                };

                // Also close the popup when the window is minimized.
                window.StateChanged += (s, args) =>
                {
                    if (window.WindowState == WindowState.Minimized)
                    {
                        Pop1.IsOpen = false;
                    }
                };
            }

            isHidedOnLoad = true;

            TextBox1.CaretIndex = TextBox1.Text.Length;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Pop1.IsOpen)
            {
                var offset = Pop1.HorizontalOffset;
                Pop1.HorizontalOffset = offset + 1;
                Pop1.HorizontalOffset = offset;
            }
        }



        #endregion

        #region Dependency Properties

        /// <summary>
        /// If true, when the control loses focus with empty text, 
        /// it will restore the last valid item instead of clearing.
        /// </summary>
        public static readonly DependencyProperty RestoreValidItemLeaveProperty =
            DependencyProperty.Register(
                nameof(RestoreValidItemLeave),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool RestoreValidItemLeave
        {
            get => (bool)GetValue(RestoreValidItemLeaveProperty);
            set => SetValue(RestoreValidItemLeaveProperty, value);
        }

        /// <summary>
        /// Auto-suggest the first found item as soon as the user starts typing.
        /// </summary>
        public static readonly DependencyProperty AutoSuggestFirstItemProperty =
            DependencyProperty.Register(
                nameof(AutoSuggestFirstItem),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool AutoSuggestFirstItem
        {
            get => (bool)GetValue(AutoSuggestFirstItemProperty);
            set => SetValue(AutoSuggestFirstItemProperty, value);
        }

        /// <summary>
        /// If true, only numeric input is accepted in TextBox.
        /// </summary>
        public static readonly DependencyProperty IsNumbericProperty =
            DependencyProperty.Register(
                nameof(IsNumberic),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool IsNumberic
        {
            get => (bool)GetValue(IsNumbericProperty);
            set => SetValue(IsNumbericProperty, value);
        }

        /// <summary>
        /// If true, digit grouping (i.e. thousand separators) is applied on numeric LostFocus.
        /// </summary>
        public static readonly DependencyProperty IsDigitGroupActiveProperty =
            DependencyProperty.Register(
                nameof(IsDigitGroupActive),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool IsDigitGroupActive
        {
            get => (bool)GetValue(IsDigitGroupActiveProperty);
            set => SetValue(IsDigitGroupActiveProperty, value);
        }

        /// <summary>
        /// If true, grouping with commas is only applied on Enter, not necessarily on LostFocus.
        /// (Implementation detail can be adapted as needed.)
        /// </summary>
        public static readonly DependencyProperty DigitGroupOnEnterProperty =
            DependencyProperty.Register(
                nameof(DigitGroupOnEnter),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool DigitGroupOnEnter
        {
            get => (bool)GetValue(DigitGroupOnEnterProperty);
            set => SetValue(DigitGroupOnEnterProperty, value);
        }

        /// <summary>
        /// If true, then pressing '+' or '-' can inject "000" or "00" respectively at the caret.
        /// (Called 'ThreeTwoZero' as per your initial code.)
        /// </summary>
        public static readonly DependencyProperty ThreeTwoZeroProperty =
            DependencyProperty.Register(
                nameof(ThreeTwoZero),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false));

        public bool ThreeTwoZero
        {
            get => (bool)GetValue(ThreeTwoZeroProperty);
            set => SetValue(ThreeTwoZeroProperty, value);
        }

        /// <summary>
        /// If true, the internal TextBox is read-only.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(false, ReadOnlyChangedCallback));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set
            {
                if (IsReadOnly != value)
                {
                    SetValue(IsReadOnlyProperty, value);
                    TextBox1.IsReadOnly = value;
                }
            }
        }

        private static void ReadOnlyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ACT ctrl)
            {
                ctrl.TextBox1.IsReadOnly = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// If true, decimal values are accepted. If false, only integers are valid.
        /// </summary>
        public static readonly DependencyProperty DoesAcceptDoubleProperty =
            DependencyProperty.Register(
                nameof(DoesAcceptDouble),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(true));

        public bool DoesAcceptDouble
        {
            get => (bool)GetValue(DoesAcceptDoubleProperty);
            set => SetValue(DoesAcceptDoubleProperty, value);
        }

        /// <summary>
        /// A comma-separated list of property names for searching (e.g., "Code,Name").
        /// </summary>
        public static readonly DependencyProperty SearchMemberPathsProperty =
            DependencyProperty.Register(
                nameof(SearchMemberPaths),
                typeof(string),
                typeof(ACT),
                new PropertyMetadata(string.Empty));

        public string SearchMemberPaths
        {
            get => (string)GetValue(SearchMemberPathsProperty);
            set => SetValue(SearchMemberPathsProperty, value);
        }

        /// <summary>
        /// If true, uses fuzzy searching (FuzzySharp). Otherwise uses a simpler 'contains' or 'starts with' approach.
        /// </summary>
        public static readonly DependencyProperty IsFuzzySearchProperty =
            DependencyProperty.Register(
                nameof(IsFuzzySearch),
                typeof(bool),
                typeof(ACT),
                new PropertyMetadata(true));

        public bool IsFuzzySearch
        {
            get => (bool)GetValue(IsFuzzySearchProperty);
            set => SetValue(IsFuzzySearchProperty, value);
        }

        /// <summary>
        /// The displayed text in the TextBox, two-way bound.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(ACT),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// The MaxLength for the TextBox input.
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(
                nameof(MaxLength),
                typeof(int),
                typeof(ACT),
                new PropertyMetadata(default(int)));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        /// <summary>
        /// The property name used to get the 'value' for the selected item.
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                nameof(SelectedValuePath),
                typeof(string),
                typeof(ACT),
                new PropertyMetadata(default(string)));

        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        /// <summary>
        /// The value selected from the ListBox (based on SelectedValuePath).
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                nameof(SelectedValue),
                typeof(object),
                typeof(ACT),
                new PropertyMetadata(default(object)));

        public object SelectedValue
        {
            get => (object)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        /// <summary>
        /// The entire row (object) that is currently selected in the ListBox.
        /// </summary>
        public static readonly DependencyProperty SelectedRowProperty =
            DependencyProperty.Register(
                nameof(SelectedRow),
                typeof(object),
                typeof(ACT),
                new PropertyMetadata(default(object)));

        public object SelectedRow
        {
            get => (object)GetValue(SelectedRowProperty);
            set => SetValue(SelectedRowProperty, value);
        }

        /// <summary>
        /// The ItemSource for the ListBox's search.
        /// </summary>
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<object>),
                typeof(ACT),
                new PropertyMetadata(default(IEnumerable<object>), OnItemSourceChanged));

        public IEnumerable<object> ItemSource
        {
            get => (IEnumerable<object>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        /// <summary>
        /// The property name used to display text in the ListBox.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(ACT),
                new PropertyMetadata(default(string)));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty ListBoxBackgroundProperty =
            DependencyProperty.Register(
                nameof(ListBoxBackground),
                typeof(Brush),
                typeof(ACT),
                new PropertyMetadata(Brushes.Transparent)); // Default to White or any default Brush you prefer

        public Brush ListBoxBackground
        {
            get => (Brush)GetValue(ListBoxBackgroundProperty);
            set => SetValue(ListBoxBackgroundProperty, value);
        }


        private static void OnItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ACT control && e.NewValue is IEnumerable<object> newSource)
            {
                control.CacheSource(newSource);
            }
        }

        #endregion

        #region Private Fields

        // Keeps track of the last valid selection. If text is cleared and RestoreValidItemLeave = true,
        // we restore these.
        private object _lastValidSelectedRow;
        private object _lastValidSelectedValue;
        private object _lastValidSelectedItem;
        private string _lastValidText;

        // For numeric
        private double _lastValidNumericValue;

        // For searching concurrency
        private CancellationTokenSource _searchCancellationTokenSource;

        // When we pick an item from the list or do something that changes TextBox text programmatically,
        // we may want to suppress auto-search momentarily.
        private bool _suppressAutoComplete;

        // Holds a cached version of the item source and string-lowercased representation for searching.
        private IReadOnlyList<object> _sourceCache;
        private IReadOnlyList<string[]> _displayValuesCache;

        // For ESC key handling
        private byte _escapeKeyCounter = 0;

        #endregion

        #region Data Caching / Searching

        private void CacheSource(IEnumerable<object> source)
        {
            _sourceCache = source.ToList(); // Prevent multiple enumeration.

            var searchMembers = !string.IsNullOrWhiteSpace(SearchMemberPaths)
                ? SearchMemberPaths.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(m => m.Trim())
                                   .ToArray()
                : Array.Empty<string>();

            // For each item in source, we create an array of the relevant string properties (lowercased).
            _displayValuesCache = _sourceCache
                .Select(item =>
                    searchMembers
                        .Select(member => item?.GetType()
                                               .GetProperty(member)?
                                               .GetValue(item, null)?
                                               .ToString()
                                               .ToLower() ?? string.Empty)
                        .ToArray())
                .ToList();
        }

        /// <summary>
        /// A simpler, non-fuzzy search. 
        /// If AutoSuggestFirstItem is true, uses StartsWith; otherwise uses Contains.
        /// </summary>
        /// <param name="searchTerm">The search term typed by user.</param>
        private IEnumerable<object> RegularSearch(string searchTerm)
        {
            if (_sourceCache == null || string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<object>();

            var lowerTerm = searchTerm.ToLowerInvariant();
            IEnumerable<object> results;

            if (AutoSuggestFirstItem)
            {
                results = _sourceCache
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Where((item, index) =>
                        _displayValuesCache[index].Any(val => val != null && val.StartsWith(lowerTerm)))
                    .ToList();
            }
            else
            {
                results = _sourceCache
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Where((item, index) =>
                        _displayValuesCache[index].Any(val => val != null && val.Contains(lowerTerm)))
                    .ToList();
            }

            return results;
        }

        /// <summary>
        /// A fuzzy search using FuzzySharp. 
        /// </summary>
        private async Task<IEnumerable<object>> FuzzySearchInDataAsync(string searchTerm, CancellationToken ct)
        {
            const int MinimumScoreThreshold = 50;
            if (string.IsNullOrWhiteSpace(searchTerm) || _sourceCache == null)
            {
                return Enumerable.Empty<object>();
            }

            var lowerTerm = searchTerm.ToLower();
            var matches = await Task.Run(() =>
            {
                return _displayValuesCache
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Select((searchValues, index) => new
                    {
                        Item = _sourceCache[index],
                        Score = searchValues.Max(value => Fuzz.PartialTokenSetRatio(lowerTerm, value))
                    })
                    .Where(x => x.Score >= MinimumScoreThreshold)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item)
                    .ToList();
            }, ct);

            return matches;
        }

        /// <summary>
        /// Gets the display value of an item using DisplayMemberPath.
        /// </summary>
        private string GetDisplayValue(object item)
        {
            if (item == null || string.IsNullOrEmpty(DisplayMemberPath))
                return string.Empty;

            var propInfo = item.GetType().GetProperty(DisplayMemberPath);
            return propInfo != null ? (propInfo.GetValue(item, null)?.ToString() ?? string.Empty) : string.Empty;
        }

        #endregion

        #region TextBox TextChanged (Search Trigger)

        private async void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressAutoComplete)
                return;

            // Cancel any ongoing search
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = null;

            // Start a fresh search
            _searchCancellationTokenSource = new CancellationTokenSource();
            var ct = _searchCancellationTokenSource.Token;

            try
            {
                // Debounce
                await Task.Delay(500, ct);
                if (ct.IsCancellationRequested) return;

                string searchTerm = TextBox1.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    // Clear
                    SEARCH_LIST.Visibility = Visibility.Collapsed;
                    SearchStatusTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Show "Searching..." feedback
                    Dispatcher.Invoke(() =>
                    {
                        if (!isHidedOnLoad)
                        {
                            Pop1.IsOpen = true;
                            SearchStatusTextBlock.Visibility = Visibility.Visible;
                            SearchStatusTextBlock.Content = "در حال جستجو...";
                        }

                        isHidedOnLoad = false;

                        SEARCH_LIST.Visibility = Visibility.Hidden;
                    });

                    // Get **all** matching results
                    IEnumerable<object> allResults;
                    if (IsFuzzySearch)
                        allResults = await FuzzySearchInDataAsync(searchTerm, ct);
                    else
                        allResults = RegularSearch(searchTerm);

                    if (ct.IsCancellationRequested) return;

                    // We only show up to 30 items
                    var displayedResults = allResults.ToList();
                    //var displayedResults = allResults.Take(30).ToList();

                    // Bind them all to the ListBox
                    Dispatcher.Invoke(() =>
                    {
                        SEARCH_LIST.ItemsSource = displayedResults;

                        if (displayedResults.Any())
                        {
                            SEARCH_LIST.Visibility = Visibility.Visible;
                            SearchStatusTextBlock.Visibility = Visibility.Collapsed;
                            SearchStatusTextBlock.Content = "";

                            // Auto-suggest the FIRST item, but still show all 30 in the list
                            if (AutoSuggestFirstItem)
                            {
                                var first = displayedResults.First();
                                SEARCH_LIST.SelectedItem = first;
                                SelectedRow = first;
                                SelectedValue = SEARCH_LIST.SelectedValue;

                                string suggestion = GetDisplayValue(first);
                                if (!string.Equals(suggestion, TextBox1.Text, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Temporarily detach to avoid re-triggering
                                    TextBox1.TextChanged -= TextBox1_TextChanged;

                                    var originalLength = TextBox1.Text.Length;
                                    TextBox1.Text = suggestion;
                                    // Highlight the auto-completed portion
                                    TextBox1.Select(originalLength, suggestion.Length - originalLength);

                                    TextBox1.TextChanged += TextBox1_TextChanged;
                                }
                            }
                        }
                        else
                        {
                            SEARCH_LIST.Visibility = Visibility.Collapsed;
                            SearchStatusTextBlock.Visibility = Visibility.Visible;
                            SearchStatusTextBlock.Content = "موردی یافت نشد.";
                        }
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore canceled tasks
            }
            finally
            {
                if (!ct.IsCancellationRequested)
                {
                    _searchCancellationTokenSource?.Dispose();
                    _searchCancellationTokenSource = null;

                }
            }
        }

        #endregion


        #region Keyboard Events on UserControl / TextBox / ListBox

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _escapeKeyCounter++;

                // Double ESC to hide the list
                if (_escapeKeyCounter % 2 == 0)
                {
                    SEARCH_LIST.Visibility = Visibility.Hidden;
                    _searchCancellationTokenSource?.Cancel();
                    _escapeKeyCounter = 0;
                }
                else
                {
                    TextBox1.CaretIndex = TextBox1.Text.Length;
                    TextBox1.Focus();
                }
            }
        }

        private void TextBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // "ThreeTwoZero" logic:  pressing + or - to insert "000" or "00"
            if (ThreeTwoZero && IsNumberic)
            {
                if (e.Key == Key.Add)
                {
                    e.Handled = true;
                    InsertTextAtCaret("000");
                }
                else if (e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    InsertTextAtCaret("00");
                }
            }

            // Handle Down key
            if (e.Key == Key.Down)
            {
                if (SEARCH_LIST.Visibility == Visibility.Visible && SEARCH_LIST.Items.Count > 0)
                {
                    e.Handled = true;

                    // Ensure popup is open
                    Pop1.IsOpen = true;

                    // Set selected index if none selected
                    if (SEARCH_LIST.SelectedIndex < 0)
                    {
                        SEARCH_LIST.SelectedIndex = 0;
                    }

                    // Focus on the selected item
                    var item = SEARCH_LIST.ItemContainerGenerator.ContainerFromIndex(SEARCH_LIST.SelectedIndex) as ListBoxItem;
                    if (item != null)
                    {
                        item.Focus();
                        item.BringIntoView();
                    }

                }
            }

            // Handle Alt + Down
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Down))
            {
                e.Handled = true;

                // Show the popup and list
                Pop1.IsOpen = true;
                SEARCH_LIST.Visibility = Visibility.Visible;

                // Set selected index if none selected
                if (SEARCH_LIST.SelectedIndex < 0)
                {
                    SEARCH_LIST.SelectedIndex = 0;
                }

                // Ensure the item is generated and focused
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var item = SEARCH_LIST.ItemContainerGenerator.ContainerFromIndex(SEARCH_LIST.SelectedIndex) as ListBoxItem;
                    if (item != null)
                    {
                        item.Focus();
                        item.BringIntoView();
                    }
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private void SEARCH_LIST_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SEARCH_LIST.SelectedItem != null)
            {
                e.Handled = true;
                SuppressAutoCompleteTemporarily();
                AcceptSelectedItem();
            }
            else if (e.Key == Key.Up && SEARCH_LIST.SelectedIndex == 0)
            {
                e.Handled = true;
                TextBox1.Focus();
            }
            else
            {
                // Ensure the newly selected item is brought into view
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var item = SEARCH_LIST.ItemContainerGenerator.ContainerFromIndex(SEARCH_LIST.SelectedIndex) as ListBoxItem;
                    item?.BringIntoView();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }

            //if (e.Key == Key.Enter && SEARCH_LIST.SelectedItem != null)
            //{
            //    e.Handled = true;
            //    SuppressAutoCompleteTemporarily();
            //    AcceptSelectedItem();
            //}
        }

        private void SEARCH_LIST_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SEARCH_LIST.SelectedItem != null)
            {
                SuppressAutoCompleteTemporarily();
                AcceptSelectedItem();
            }
        }

        /// <summary>
        /// Temporarily suppress the TextChanged-based auto-search to avoid recursion
        /// when we programmatically update TextBox.
        /// </summary>
        private void SuppressAutoCompleteTemporarily()
        {
            _suppressAutoComplete = true;
            _searchCancellationTokenSource?.Cancel();

            Task.Delay(300)
                .ContinueWith(_ => _suppressAutoComplete = false,
                              CancellationToken.None,
                              TaskContinuationOptions.None,
                              TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Putting SelectedItem into TextBox

        private void AcceptSelectedItem()
        {
            if (SEARCH_LIST.SelectedItem == null) return;

            TextBox1.TextChanged -= TextBox1_TextChanged;
            SelectedRow = SEARCH_LIST.SelectedItem;
            SelectedValue = SEARCH_LIST.SelectedValue;

            var selectedObj = SEARCH_LIST.SelectedItem;
            var displayText = GetDisplayValue(selectedObj);
            Text = displayText;
            TextBox1.Text = displayText;

            TextBox1.TextChanged += TextBox1_TextChanged;
            TextBox1.CaretIndex = TextBox1.Text.Length;
            TextBox1.Focus();

            SEARCH_LIST.Visibility = Visibility.Hidden;

            // Save last valid
            _lastValidSelectedRow = SelectedRow;
            _lastValidSelectedValue = SelectedValue;
            _lastValidSelectedItem = selectedObj;
            _lastValidText = displayText;
        }

        #endregion

        #region Focus / LostFocus Events

        private void TextBox1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // When the TextBox loses keyboard focus, hide the popup.
            //Pop1.IsOpen = false;
        }

        private void TextBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            // Numeric formatting
            if (IsNumberic)
            {
                HandleNumericOnLostFocus();
            }

            // If user isn't pressing Down, hide the list
            if (AutoSuggestFirstItem && !Keyboard.IsKeyDown(Key.Down))
            {
                SEARCH_LIST.Visibility = Visibility.Hidden;
            }
        }

        private bool IsFocusInsideThisControlOrPopup(DependencyObject newFocus)
        {
            // Edge case: if no new focus, do not close
            if (newFocus == null)
                return true;

            // We climb up the visual or logical tree to see if we are still inside 
            // the UserControl or the popup's child elements.
            while (newFocus != null)
            {
                if (newFocus == this || newFocus == Pop1 || newFocus == SEARCH_LIST || newFocus is ListBoxItem)
                    return true;

                // If you prefer, you can climb "LogicalTreeHelper" or "VisualTreeHelper" 
                // depending on your situation:
                newFocus = LogicalTreeHelper.GetParent(newFocus);
            }
            return false;
        }
        private void UserControl_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // If the new focus is inside the UserControl or inside the Popup’s child, do nothing.
            // Otherwise, close the Popup.

            if (IsFocusInsideThisControlOrPopup(e.NewFocus as DependencyObject))
            {
                // Remain open – the user is just going from TextBox -> ListBox, etc.
                //e.Handled = true;
                Pop1.IsOpen = true;
                return;
            }
            else
            {
                Pop1.IsOpen = false;

                // If the user leaves the control with an empty string
                // then restore last valid item or clear selection.
                if (string.IsNullOrEmpty(TextBox1.Text.Trim()))
                {
                    if (RestoreValidItemLeave)
                    {

                        SelectedRow = _lastValidSelectedRow;
                        SelectedValue = _lastValidSelectedValue;
                        SEARCH_LIST.SelectedItem = _lastValidSelectedItem;

                        // Temporarily detach to avoid re-triggering
                        TextBox1.TextChanged -= TextBox1_TextChanged;

                        Text = _lastValidText;
                        TextBox1.Text = _lastValidText;

                        TextBox1.TextChanged += TextBox1_TextChanged;

                    }
                    else
                    {
                        SEARCH_LIST.SelectedItem = null;
                        SelectedRow = null;
                        SelectedValue = null;
                    }

                    SEARCH_LIST.Visibility = Visibility.Hidden;
                }
            }
        }

        #endregion

        #region Numeric Helper Methods

        private void HandleNumericOnLostFocus()
        {
            // If the Text is empty, revert to last known good or 0
            if (string.IsNullOrEmpty(TextBox1.Text))
            {
                if (string.IsNullOrEmpty(_lastValidNumericValue.ToString(CultureInfo.CurrentCulture)))
                {
                    TextBox1.Text = "0";
                }
                else
                {
                    TextBox1.Text = _lastValidNumericValue.ToString(CultureInfo.CurrentCulture);
                }
                FormatNumericValue();
                return;
            }

            // Validate the numeric input
            if (!double.TryParse(UnformatText(TextBox1.Text), out double newValue))
            {
                // Revert to last valid
                TextBox1.Text = _lastValidNumericValue.ToString(CultureInfo.CurrentCulture);
                FormatNumericValue();
            }
            else
            {
                _lastValidNumericValue = newValue;
                FormatNumericValue();
            }
        }

        /// <summary>
        /// Removes all commas from the text for numeric parsing.
        /// </summary>
        private string UnformatText(string formattedText)
        {
            return formattedText?.Replace(",", "") ?? "0";
        }

        /// <summary>
        /// Applies digit grouping if configured, e.g. "1234" -> "1,234".
        /// </summary>
        private void FormatNumericValue()
        {
            if (IsDigitGroupActive &&
                double.TryParse(TextBox1.Text, out double numericValue))
            {
                // If user typed just a dot or something partial, skip. (Guard)
                if (TextBox1.Text.EndsWith("."))
                    return;

                // Format with or without decimal. 
                // Adjust this format as desired (0.##, 0.###, etc.).
                if (DoesAcceptDouble)
                {
                    TextBox1.Text = string.Format("{0:#,##0.###}", numericValue);
                }
                else
                {
                    // Integer-based formatting
                    TextBox1.Text = string.Format("{0:#,##0}", numericValue);
                }

                // Move caret to end
                TextBox1.CaretIndex = TextBox1.Text.Length;
            }
        }

        private void TextBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsNumberic)
            {
                if (!IsValidNumericInput(e.Text))
                {
                    e.Handled = true;
                }
            }
        }

        private bool IsValidNumericInput(string input)
        {
            // If we accept doubles, we allow decimal separator
            if (DoesAcceptDouble)
            {
                return double.TryParse(input, out _) ||
                       IsDecimalSeparator(input);
            }
            else
            {
                // If integer only
                return long.TryParse(input, out _) ||
                       IsDecimalSeparator(input); // Decide if you want to allow decimals at all
            }
        }

        private bool IsDecimalSeparator(string input)
        {
            return input.Equals(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void TextBox1_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (IsNumberic)
            {
                if (e.DataObject.GetDataPresent(typeof(string)))
                {
                    string pastedText = (string)e.DataObject.GetData(typeof(string));
                    if (!IsValidNumericInput(pastedText))
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    e.CancelCommand();
                }
            }
        }

        /// <summary>
        /// Inserts given text at the current caret position in the TextBox.
        /// </summary>
        private void InsertTextAtCaret(string textToInsert)
        {
            var target = Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;
            target.RaiseEvent(
                new TextCompositionEventArgs(
                    InputManager.Current.PrimaryKeyboardDevice,
                    new TextComposition(InputManager.Current, target, textToInsert))
                {
                    RoutedEvent = routedEvent
                });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Selects all text in the internal TextBox.
        /// </summary>
        public void SelectAllText()
        {
            TextBox1.SelectAll();
        }

        #endregion
    }
}
