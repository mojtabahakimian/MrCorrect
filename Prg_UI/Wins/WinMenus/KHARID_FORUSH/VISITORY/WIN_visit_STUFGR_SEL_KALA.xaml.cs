using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for WIN_visit_STUFGR_SEL_KALA.xaml
    /// </summary>
    public partial class WIN_visit_STUFGR_SEL_KALA : Window
    {
        public ObservableCollection<TCODE_MENUITEM_MODEL> ItemsData { get; set; } = new ObservableCollection<TCODE_MENUITEM_MODEL>();
        public WIN_visit_STUFGR_SEL_KALA(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
            }

            //LBL1 , DARSAD
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
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            FILL_ALL_COMBOBOXES();

            ReGetData();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                // با زدن اینتر، لیست انتخاب شده‌ها پردازش می‌شود
                OpenVistedadKala();
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
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
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }
        private void ReGetData()
        {
            try
            {
                // فقط کد و نام را می‌خوانیم، تیک را نادیده می‌گیریم چون می‌خواهیم در حافظه مدیریت کنیم
                string sql = "SELECT CODE, NAMES FROM TCODE_MENUITEM ORDER BY CODE";
                var data = dbms.DoGetDataSQL<TCODE_MENUITEM_MODEL>(sql);

                ItemsData.Clear();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        // مقدار اولیه تیک را فالس می‌گذاریم تا لیست همیشه پاک باشد
                        item.TIC = false;
                        ItemsData.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری اطلاعات").ShowDialog();
            }
        }

        // Simulating the "Bound Checkbox" behavior. 
        // In legacy Access, clicking the box updates the DB immediately or on record move.
        // We handle this via CellEditEnding to capture the change.
        private void DG_List_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column is DataGridCheckBoxColumn && e.Row.Item is TCODE_MENUITEM_MODEL item)
            {
                var checkBox = e.EditingElement as CheckBox;
                bool isChecked = checkBox?.IsChecked ?? false;

                try
                {
                    // Update DB directly
                    string sql = $"UPDATE TCODE_MENUITEM SET TIC = {(isChecked ? 1 : 0)} WHERE CODE = '{item.CODE}'";
                    dbms.DoExecuteSQL(sql);

                    // Update local model to reflect state
                    item.TIC = isChecked;
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در ذخیره انتخاب").ShowDialog();
                }
            }
        }

        private void OpenVistedadKala()
        {
            // 1. فیلتر کردن داده‌ها از روی حافظه (ItemsData)
            // بدون نیاز به مراجعه به UI (DataGrid) یا Database
            var selectedItems = ItemsData.Where(x => x.TIC == true).ToList();

            if (selectedItems.Count == 0)
            {
                new Msgwin(false, "هیچ گروه کالایی انتخاب نشده است").ShowDialog();
                return;
            }

            // 2. آماده‌سازی برای ارسال به فرم بعدی
            // فرض: فرم بعدی (WIN_VISTEDAD_KALA) یک لیست از مدل‌ها یا کدها را در سازنده دریافت می‌کند

            // var win = new WIN_VISTEDAD_KALA(selectedItems);
            // win.ShowDialog();

            // جهت نمایش موقت خروجی برای تست:
            // var codes = string.Join(", ", selectedItems.Select(x => x.CODE));
            // new Msgwin(true, $"تعداد {selectedItems.Count} گروه انتخاب شد: \n{codes}").ShowDialog();
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(DARSAD.Text, out double p))
            {
                SelectedPercent = p;
            }
            else
            {
                SelectedPercent = 0;
            }
            this.Close();
        }
        public double SelectedPercent { get; private set; } = 0;
        private void DARSAD_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DARSAD.Text))
            {
                var (isvalid, msg) = CL_LMethods.IsValidPercentage(DARSAD.Text);
                if (!isvalid)
                {
                    new Msgwin(false, msg).ShowDialog();
                }
            }
        }
    }

    public class TCODE_MENUITEM_MODEL : INotifyPropertyChanged
    {
        private string _code;
        private string _names;
        private bool? _tic;
        public string CODE
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }
        public string NAMES
        {
            get => _names;
            set
            {
                if (_names != value)
                {
                    _names = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? TIC
        {
            get => _tic;
            set
            {
                if (_tic != value)
                {
                    _tic = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
