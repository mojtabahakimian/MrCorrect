using Syncfusion.Windows.Controls.PivotGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Prg_UI.CUC
{
    public static class ChangeTracker
    {
        // ===== Attached Properties =====
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(ChangeTracker),
                new PropertyMetadata(false, OnEnableChanged));
        public static void SetEnable(DependencyObject d, bool value) => d.SetValue(EnableProperty, value);
        public static bool GetEnable(DependencyObject d) => (bool)d.GetValue(EnableProperty);

        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.RegisterAttached("IsDirty", typeof(bool), typeof(ChangeTracker),
                new PropertyMetadata(false));
        public static void SetIsDirty(DependencyObject d, bool value) => d.SetValue(IsDirtyProperty, value);
        public static bool GetIsDirty(DependencyObject d) => (bool)d.GetValue(IsDirtyProperty);

        // ===== Public API =====
        public static bool HasChanges(Window w) => GetIsDirty(w);
        public static void AcceptChanges(Window w)
        {
            if (w == null) return;
            var map = GetSnapshot(w);
            map.Clear();
            SnapshotAll(w, map); // ذخیره وضعیت فعلی به‌عنوان مبنا
            SetIsDirty(w, false);
        }

        // ===== Internal Snapshot Store =====
        private static readonly DependencyProperty SnapshotProperty =
            DependencyProperty.RegisterAttached("Snapshot", typeof(Dictionary<string, object>), typeof(ChangeTracker),
                new PropertyMetadata(null));
        private static Dictionary<string, object> GetSnapshot(DependencyObject d)
        {
            var dict = (Dictionary<string, object>)d.GetValue(SnapshotProperty);
            if (dict == null)
            {
                dict = new Dictionary<string, object>();
                d.SetValue(SnapshotProperty, dict);
            }
            return dict;
        }

        // ===== Enable/Disable =====
        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Window w) return;

            if ((bool)e.NewValue)
            {
                w.Loaded += Window_Loaded;
                w.Closing += Window_Closing_Mark; // اجازه لغو خروج بستگی به برنامه شما دارد
            }
            else
            {
                w.Loaded -= Window_Loaded;
                w.Closing -= Window_Closing_Mark;
            }
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window w) return;

            // اسنپ‌شات اولیه
            var map = GetSnapshot(w);
            map.Clear();
            SnapshotAll(w, map);

            // اتصال هندلرها به تمام کنترل‌های ورودی موجود در Visual Tree
            foreach (var fe in FindInputs(w))
            {
                Hook(fe, w);
            }
        }

        private static void Window_Closing_Mark(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is not Window w) return;

            // بررسی نهایی اگر تغییراتی اعمال شده باشد
            var map = GetSnapshot(w);
            if (IsAnyChanged(w, map))
                SetIsDirty(w, true);
        }

        // ===== Hook events for value changes =====
        private static void Hook(FrameworkElement fe, Window root)
        {
            // برای جلوگیری از چند‌بار اتصال، ابتدا هندلر برای Unloaded اضافه می‌شود.
            RoutedEventHandler onUnloaded = null!;
            onUnloaded = (s, e) =>
            {
                // در هنگام Unloaded رویدادهای مشترک لغو شوند.
                Unhook(fe);
                fe.Unloaded -= onUnloaded;
            };
            fe.Unloaded += onUnloaded;

            // اتصال رویدادها بر اساس نوع کنترل
            switch (fe)
            {
                case Prg_UI.CUC.NumericTextBox ntb:
                    {
                        // (1) تغییر مقدار (DependencyProperty Text)
                        var dpd = DependencyPropertyDescriptor.FromProperty(
                            Prg_UI.CUC.NumericTextBox.TextProperty,
                            typeof(Prg_UI.CUC.NumericTextBox));
                        EventHandler ntbTextHandler = (_, __) => CompareAndMark(ntb, root);
                        dpd.AddValueChanged(ntb, ntbTextHandler);

                        // (2) رویداد خروج فوکوس اختصاصی
                        RoutedEventHandler ntbLostFocus = (_, __) => CompareAndMark(ntb, root);
                        ntb.NumericLostFocus += ntbLostFocus;

                        // لغو اشتراک در هنگام Unloaded
                        ntb.Unloaded += (_, __) =>
                        {
                            dpd.RemoveValueChanged(ntb, ntbTextHandler);
                            ntb.NumericLostFocus -= ntbLostFocus;
                        };
                        break;
                    }
                case Xceed.Wpf.Toolkit.MaskedTextBox msk:
                    {
                        // برای کنترل‌های Xceed: ثبت رویداد TextChanged
                        TextChangedEventHandler mskHandler = (_, __) => CompareAndMark(msk, root);
                        msk.TextChanged += mskHandler;
                        msk.Unloaded += (_, __) => msk.TextChanged -= mskHandler;
                        break;
                    }
                case TextBox tb:
                    {
                        TextChangedEventHandler tbHandler = (_, __) => CompareAndMark(tb, root);
                        tb.TextChanged += tbHandler;
                        tb.Unloaded += (_, __) => tb.TextChanged -= tbHandler;
                        break;
                    }
                case PasswordBox pb:
                    {
                        RoutedEventHandler pbHandler = (_, __) => CompareAndMark(pb, root);
                        pb.PasswordChanged += pbHandler;
                        pb.Unloaded += (_, __) => pb.PasswordChanged -= pbHandler;
                        break;
                    }
                case CheckBox cb:
                    {
                        RoutedEventHandler cbChecked = (_, __) => CompareAndMark(cb, root);
                        RoutedEventHandler cbUnchecked = (_, __) => CompareAndMark(cb, root);
                        cb.Checked += cbChecked;
                        cb.Unchecked += cbUnchecked;
                        cb.Unloaded += (_, __) => { cb.Checked -= cbChecked; cb.Unchecked -= cbUnchecked; };
                        break;
                    }
                case ToggleButton tgl:
                    {
                        RoutedEventHandler tglChecked = (_, __) => CompareAndMark(tgl, root);
                        RoutedEventHandler tglUnchecked = (_, __) => CompareAndMark(tgl, root);
                        tgl.Checked += tglChecked;
                        tgl.Unchecked += tglUnchecked;
                        tgl.Unloaded += (_, __) => { tgl.Checked -= tglChecked; tgl.Unchecked -= tglUnchecked; };
                        break;
                    }
                case ComboBox cmb:
                    {
                        SelectionChangedEventHandler cmbSelChanged = (_, __) => CompareAndMark(cmb, root);
                        RoutedEventHandler cmbLostFocus = (_, __) => CompareAndMark(cmb, root);
                        cmb.SelectionChanged += cmbSelChanged;
                        cmb.LostFocus += cmbLostFocus;
                        cmb.Unloaded += (_, __) => { cmb.SelectionChanged -= cmbSelChanged; cmb.LostFocus -= cmbLostFocus; };
                        break;
                    }
                case DatePicker dp:
                    {
                        EventHandler dpHandler = (_, __) => CompareAndMark(dp, root);
                        dp.SelectedDateChanged += (s, args) => dpHandler(s, args);
                        dp.Unloaded += (_, __) =>
                        {
                            // لغو اشتراک رویداد SelectedDateChanged به روش زیر:
                            var eventInfo = typeof(DatePicker).GetEvent("SelectedDateChanged");
                            if (eventInfo is not null)
                            {
                                // متأسفانه نمی‌توان به راحتی رویداد اضافه‌شده را لغو کرد،
                                // بنابراین در اینجا فرض می‌کنیم که کنترل DatePicker به ازای هر بار بارگذاری فقط یک اشتراک ایجاد می‌کند.
                            }
                        };
                        break;
                    }
                case Slider sl:
                    {
                        RoutedPropertyChangedEventHandler<double> slHandler = (_, __) => CompareAndMark(sl, root);
                        sl.ValueChanged += slHandler;
                        sl.Unloaded += (_, __) => sl.ValueChanged -= slHandler;
                        break;
                    }
                default:
                    {
                        // کنترل‌های UpDown و یا DataGrid های Syncfusion
                        if (fe.GetType().Name.Contains("UpDown") ||
                            (fe.GetType().FullName?.Contains("Syncfusion.UI.Xaml.Grid.SfDataGrid") ?? false))
                        {
                            // اگر از کنترل UpDown استفاده می‌کنید (مثلاً از WPF Extended)
                            // ابتدا بررسی می‌کنیم که آیا کنترل UpDown است.
                            var type = fe.GetType();
                            if (type.FullName?.Contains("Syncfusion.UI.Xaml.Grid.SfDataGrid") == true)
                            {
                                TryHookSyncfusionGrid(fe, root);
                            }
                            else
                            {
                                // فرض کنید کنترل UpDown دارای رویداد ValueChanged است
                                EventInfo? evt = type.GetEvent("ValueChanged");
                                if (evt != null)
                                {
                                    Delegate handler = Delegate.CreateDelegate(evt.EventHandlerType!,
                                        typeof(ChangeTracker).GetMethod(nameof(GenericValueChanged),
                                            BindingFlags.NonPublic | BindingFlags.Static)!);
                                    evt.AddEventHandler(fe, handler);
                                    fe.Unloaded += (_, __) => evt.RemoveEventHandler(fe, handler);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        // متد لغو اشتراک رویدادها (در صورتی که نیاز به گسترش داشته باشید)
        private static void Unhook(FrameworkElement fe)
        {
            // در اینجا می‌توانید بررسی کنید که آیا اشتراک‌های رویداد باید لغو شوند.
            // از آنجا که در هر بلوک Hook لغو اشتراک در رویداد Unloaded ثبت شده است،
            // معمولا نیازی به انجام کاری نیست.
        }

        private static void TryHookSyncfusionGrid(FrameworkElement fe, Window root)
        {
            var type = fe.GetType();
            if (type.FullName == null) return;
            // پشتیبانی حداقلی بدون وابستگی مستقیم: رویداد CurrentCellEndEdit
            EventInfo? ev = type.GetEvent("CurrentCellEndEdit");
            if (ev != null)
            {
                MethodInfo? mi = typeof(ChangeTracker).GetMethod(nameof(GridCellEndEdit),
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (mi != null)
                {
                    Delegate handler = Delegate.CreateDelegate(ev.EventHandlerType!, mi);
                    ev.AddEventHandler(fe, handler);
                    fe.Unloaded += (_, __) => ev.RemoveEventHandler(fe, handler);
                }
            }
        }

        private static void GenericValueChanged(object? sender, EventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                var w = Window.GetWindow(fe);
                if (w != null) SetIsDirty(w, true);
            }
        }

        private static void GridCellEndEdit(object? sender, EventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                var w = Window.GetWindow(fe);
                if (w != null) SetIsDirty(w, true);
            }
        }

        // ===== Compare logic =====
        private static void CompareAndMark(FrameworkElement fe, Window root)
        {
            var map = GetSnapshot(root);
            var key = MakeKey(fe);
            var current = ReadValue(fe);

            if (!map.TryGetValue(key, out var init))
            {
                map[key] = current; // اگر کنترل تازه وارد شده
                return;
            }

            bool changed = !Equals(Normalize(init), Normalize(current));
            if (changed)
            {
                SetIsDirty(root, true);
            }
            else
            {
                // اگر مقدار به حالت اولیه برگردانده شده و هیچ تغییر دیگری در کنترل‌ها وجود ندارد
                if (!IsAnyChanged(root, map))
                    SetIsDirty(root, false);
            }
        }

        private static bool IsAnyChanged(Window root, Dictionary<string, object> map)
        {
            foreach (var fe in FindInputs(root))
            {
                var key = MakeKey(fe);
                // نادیده گرفتن کنترل‌هایی با نام خاص (مثلاً "PERSONEL")
                if (key.Equals("System.Windows.Controls.ComboBox|PERSONEL"))
                {
                    continue;
                }

                var cur = ReadValue(fe);
                if (map.TryGetValue(key, out var init))
                {
                    if (!Equals(Normalize(init), Normalize(cur)))
                        return true;
                }
                else
                {
                    // اگر کنترل جدید اضافه شده است، آن را اضافه می‌کنیم
                    map[key] = cur;
                }
            }
            return false;
        }

        private static void SnapshotAll(Window root, Dictionary<string, object> map)
        {
            foreach (var fe in FindInputs(root))
            {
                map[MakeKey(fe)] = ReadValue(fe);
            }
        }

        // ===== Helpers =====
        private static IEnumerable<FrameworkElement> FindInputs(DependencyObject root)
        {
            if (root == null)
                yield break;

            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    queue.Enqueue(VisualTreeHelper.GetChild(current, i));
                }

                if (current is FrameworkElement fe)
                {
                    // اگر کنترل ComboBox باشد و نامش "PERSONEL" است، آن را نادیده بگیر
                    if (fe is ComboBox && string.Equals(fe.Name, "PERSONEL", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (fe is ComboBox && string.Equals(fe.Parent.GetName(), "PERSONEL", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (fe is TextBox ||
                        fe is PasswordBox ||
                        fe is CheckBox ||
                        fe is ToggleButton ||
                        fe is ComboBox ||
                        fe is DatePicker ||
                        fe is Slider ||
                        (fe.GetType().Name.Contains("UpDown")) ||
                        (fe.GetType().FullName?.Contains("Syncfusion.UI.Xaml.Grid.SfDataGrid") == true))
                    {
                        yield return fe;
                    }
                }
            }
        }


        private static string MakeKey(FrameworkElement fe)
        {
            string id = fe.Name;
            if (string.IsNullOrWhiteSpace(id))
                id = fe.GetHashCode().ToString();
            return $"{fe.GetType().FullName}|{id}";
        }

        private static object? ReadValue(FrameworkElement fe) =>
            fe switch
            {
                Prg_UI.CUC.NumericTextBox ntb => ntb.Text,
                TextBox tb => tb.Text,
                PasswordBox pb => pb.Password,
                CheckBox cb => cb.IsChecked,
                ToggleButton tgl => tgl.IsChecked,
                ComboBox cmb => cmb.IsEditable ? (cmb.Text ?? cmb.SelectedValue ?? cmb.SelectedItem) : (cmb.SelectedValue ?? cmb.SelectedItem),
                DatePicker dp => dp.SelectedDate,
                Slider sl => sl.Value,
                _ => TryReadCommonDP(fe)
            };

        private static object? TryReadCommonDP(FrameworkElement fe)
        {
            // بررسی کردن binding‌های TwoWay روی خاصیت‌های رایج
            var candidates = new[] {
                TextBox.TextProperty,
                Selector.SelectedItemProperty,
                Selector.SelectedValueProperty,
                ToggleButton.IsCheckedProperty,
                RangeBase.ValueProperty
            };

            foreach (var dp in candidates)
            {
                var be = BindingOperations.GetBindingExpression(fe, dp);
                if ((be?.ParentBinding?.Mode == BindingMode.TwoWay) ||
                    (be?.ParentBinding?.Mode == BindingMode.Default))
                {
                    return fe.GetValue(dp);
                }
            }
            return null;
        }

        private static object? Normalize(object? v)
        {
            switch (v)
            {
                case string s:
                    var trimmed = s.Trim();
                    // اگر متن عددی است، آن را به decimal تبدیل می‌کنیم (مناسب برای مقادیر مالی)
                    if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                        return dec;
                    if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, out dec))
                        return dec;
                    return trimmed;
                case DateTime dt:
                    return dt;
                default:
                    return v;
            }
        }
    }
}
