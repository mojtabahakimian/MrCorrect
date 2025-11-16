using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Prg_UI.Functions;

namespace Wins.WinOther
{
    // The searchable window interface – note that GetSearchSource now returns an object.
    public interface ISearchableWindow
    {
        void OnSearchResultSelected(object selectedItem);
        IEnumerable<SearchableProperty> GetSearchableProperties();
        // Can return either a CollectionViewSource or an ObservableCollection (or any IEnumerable)
        object GetSearchSource();

        // Specify the name of the identity property (for example, "EmployeeID" or "UserID")
    }
    public interface IComboLookupProvider
    {
        IEnumerable<ComboLookupSpec> GetComboLookups();
    }
    public class ComboLookupSpec
    {
        public string DisplayName { get; set; }       // نام ستونی که در جستجو نشان داده می‌شود
        public string KeyPropertyPath { get; set; }   // فیلد کلیدی داخل مدل (مثلاً "CODE")
        public ComboBox Combo { get; set; }           // خود ComboBox
    }
    // Defines a searchable property.
    public class SearchableProperty
    {
        public string DisplayName { get; set; }
        public string PropertyPath { get; set; }
        public Type PropertyType { get; set; }

        // --- اختیاری: اگر پر شود یعنی این ستون Lookup است (نمایش/فیلتر روی متنِ ComboBox) ---
        public IDictionary<string, string> LookupMap { get; set; }   // key: normalized key, val: display
        public bool IsLookup => LookupMap != null;
        // سازندهٔ راحت برای ComboBox
        public static SearchableProperty FromCombo(string displayName, string keyPropertyPath, ComboBox combo)
        {
            var sv = combo.SelectedValuePath ?? "Id";
            var dm = combo.DisplayMemberPath ?? string.Empty;
            var items = combo.ItemsSource?.Cast<object>() ?? Enumerable.Empty<object>();

            // کلیدها را string نرمال شده می‌گیریم تا با انواع مختلف (int, long, string) مشکلی نباشد
            string Norm(object o) => CL_LMethods.NormalizeArabicPersian(Convert.ToString(o) ?? string.Empty).Trim();

            var map = items
                .Select(it => new
                {
                    Key = FastPropertyAccess.GetPropertyValue(it, sv),
                    Val = FastPropertyAccess.GetPropertyValue(it, dm)
                })
                .Where(x => x.Key != null)
                .GroupBy(x => Norm(x.Key))                 // اگر آیتم تکراری بود، اولی را نگه داریم
                .ToDictionary(g => g.Key, g => Convert.ToString(g.First().Val) ?? string.Empty);

            return new SearchableProperty
            {
                DisplayName = displayName,
                PropertyPath = keyPropertyPath,            // روی این فیلد کلیدی سرچ می‌کنیم، ولی با LookupMap متن را می‌سازیم
                PropertyType = typeof(string),
                LookupMap = map
            };
        }
    }

    public partial class EnhancedSearchWindow : Window
    {
        #region Header Window Code
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
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
                this.DragMove();
            if (e.ClickCount == 2)
                Btn_Max_Click(null, null);
        }
        #endregion

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;

            _searchBox?.Focus(); _searchBox?.SelectAll();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Additional initialization if needed
        }

        private readonly ISearchableWindow _parentWindow;
        // Use an ICollectionView so that we can work with either a CollectionViewSource or an ObservableCollection.
        private readonly ICollectionView _recordsView;
        private IList<SearchableProperty> _searchableProperties;

        public EnhancedSearchWindow(ISearchableWindow parentWindow)
        {
            _parentWindow = parentWindow;

            // Get the original data source from the parent
            var parentSource = parentWindow.GetSearchSource();
            IEnumerable originalData = null;

            if (parentSource is CollectionViewSource cvs)
                originalData = cvs.Source as IEnumerable;
            else if (parentSource is IEnumerable enumerable)
                originalData = enumerable;

            if (originalData == null)
                throw new InvalidOperationException("The provided search source must be IEnumerable.");

            // Create a local copy of the data (this breaks the synchronization with the parent)
            var localData = new ObservableCollection<object>(originalData.Cast<object>().ToList());

            // Create an isolated view for the search window
            _recordsView = CollectionViewSource.GetDefaultView(localData);

            _searchableProperties = parentWindow.GetSearchableProperties().ToList();

            // اگر فرم ComboBoxهایی برای Lookup معرفی کرده بود، آن‌ها را به لیست ستون‌ها اضافه کن
            if (_parentWindow is IComboLookupProvider comboProvider)
            {
                foreach (var spec in comboProvider.GetComboLookups() ?? Enumerable.Empty<ComboLookupSpec>())
                {
                    if (spec?.Combo?.ItemsSource == null) continue;
                    _searchableProperties.Add(SearchableProperty.FromCombo(spec.DisplayName, spec.KeyPropertyPath, spec.Combo));
                }
            }

            InitializeComponent();
            SetupUI();

        }

        private void SetupUI()
        {
            // Setup property selector.
            _propertySelector.ItemsSource = _searchableProperties;
            _propertySelector.DisplayMemberPath = "DisplayName";
            _propertySelector.SelectedIndex = 0;

            // Bind the DataGrid to the ICollectionView.
            _resultsGrid.ItemsSource = _recordsView;
            _resultsGrid.AutoGenerateColumns = false;
            //foreach (var prop in _searchableProperties)
            //{
            //    _resultsGrid.Columns.Add(new System.Windows.Controls.DataGridTextColumn
            //    {
            //        Header = prop.DisplayName,
            //        Binding = new Binding(prop.PropertyPath),
            //    });
            //}


            foreach (var prop in _searchableProperties)
            {
                var binding = new Binding(prop.PropertyPath);

                // فرمت سادهٔ تاریخ (مثل قبل)
                if (prop.PropertyPath.ToStringNullSafe().ToUpper().Contains("DATE"))
                    binding.StringFormat = "####/##/##";

                var col = new System.Windows.Controls.DataGridTextColumn
                {
                    Header = prop.DisplayName,
                    Binding = binding,
                    MinWidth = 65
                };

                // اگر Lookup بود، مقدار نمایش ستون را با Converter از روی Map نشان بده
                if (prop.IsLookup)
                {
                    col.Binding = new Binding(prop.PropertyPath)
                    {
                        Converter = new LookupConverter { Map = prop.LookupMap }
                    };

                    _resultsGrid.Columns.Insert(1, col);
                }
                else
                {
                    _resultsGrid.Columns.Add(col);
                }

            }

            // Handle double-click on a result.
            _resultsGrid.MouseDoubleClick += (s, e) =>
            {
                if (_resultsGrid.SelectedItem != null)
                {
                    _parentWindow.OnSearchResultSelected(_resultsGrid.SelectedItem);
                    // Optionally, close the window here.
                }
            };
        }

        // Debounce dispatcher for the search.
        private readonly DebounceDispatcher _debouncer = new DebounceDispatcher();
        private void ApplySearchDebounced() =>
            _debouncer.Debounce(250, _ => ApplySearch());

        private void _searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchDebounced();
        }
        private void ApplySearch()
        {
            var searchText = CL_LMethods.NormalizeArabicPersian(_searchBox.Text?.Trim() ?? "");
            var selectedProperty = (SearchableProperty)_propertySelector.SelectedItem;

            if (string.IsNullOrEmpty(searchText))
            {
                _recordsView.Filter = null;
                _recordsView.Refresh();
                return;
            }

            string Norm(object o) => CL_LMethods.NormalizeArabicPersian(Convert.ToString(o) ?? string.Empty).Trim();

            _recordsView.Filter = item =>
            {
                if (item == null) return false;

                var rawValue = FastPropertyAccess.GetPropertyValue(item, selectedProperty.PropertyPath);
                if (rawValue == null) return false;

                // اگر Lookup است، ابتدا مقدار کلید را به متن نمایشی تبدیل کن
                var candidate = selectedProperty.IsLookup
                              ? (selectedProperty.LookupMap.TryGetValue(Norm(rawValue), out var txt) ? txt : string.Empty)
                              : Convert.ToString(rawValue) ?? string.Empty;

                var normalizedValue = Norm(candidate);

                return (CHB_MachCase.IsChecked ?? false)
                     ? normalizedValue.Equals(searchText, StringComparison.CurrentCultureIgnoreCase)
                     : normalizedValue.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
            };

            _recordsView.Refresh();
        }


        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the search box and remove any filter.
            _searchBox.Text = "";
            _recordsView.Filter = null;
            _recordsView.Refresh();
            _parentWindow.OnSearchResultSelected(_resultsGrid.SelectedItem);
        }
    }

    // Helper class for fast property access.
    public class FastPropertyAccess
    {
        private static readonly ConcurrentDictionary<(Type type, string path), PropertyInfo> _cache = new();

        public static object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null) return null;
            var key = (obj.GetType(), propertyPath);
            var pi = _cache.GetOrAdd(key, k => k.type.GetProperty(k.path));
            return pi?.GetValue(obj);
        }
    }

    // Debounce helper.
    public class DebounceDispatcher
    {
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        public void Debounce(int milliseconds, Action<object> action, object param = null)
        {
            _cancelTokenSource.Cancel();
            _cancelTokenSource = new CancellationTokenSource();

            Task.Delay(milliseconds, _cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        Application.Current.Dispatcher.Invoke(() => action(param));
                    }
                }, TaskScheduler.Current);
        }
    }
}
