using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using Wins.WinMenus.CONFIGS;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_TCOD_ZARMALIAT : Window
    {
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
        public WIN_TCOD_ZARMALIAT()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<TCOD_ZARMALIAT_MODEL> TaxBrackets { get; set; } = new ObservableCollection<TCOD_ZARMALIAT_MODEL>();

        private bool _isNewRecord;
        private TCOD_ZARMALIAT_MODEL _originalRowData;
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

            FILL_ALL_COMBOBOXES();

            ReGetData();
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
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        private void ReGetData()
        {
            try
            {
                var data = dbms.DoGetDataSQL<TCOD_ZARMALIAT_MODEL>("SELECT MABL_H, ZARIB, MALIAT, CRT, UID FROM dbo.TCOD_ZARMALIAT ORDER BY MABL_H");
                TaxBrackets.Clear();
                foreach (var item in data)
                {
                    TaxBrackets.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری اطلاعات: {ex.Message}").ShowDialog();
            }
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_MAIN.Dispatcher.InvokeAsync(() =>
            {
                DG_MAIN.CellEditEnding -= DG_MAIN_CellEditEnding;
                DG_MAIN.RowEditEnding -= DG_MAIN_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_MAIN.CancelEdit();
                }
                else
                {
                    DG_MAIN.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DG_MAIN.RowEditEnding += DG_MAIN_RowEditEnding;
                DG_MAIN.CellEditEnding += DG_MAIN_CellEditEnding;
            });
        }
        private void DG_MAIN_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.EditingElement == null || e.Column == null) { return; }

            if (e.EditAction == DataGridEditAction.Cancel || e.Column.SortMemberPath != "MABL_H")
            {
                return;
            }

            // Translation of the VBA MABL_H_AfterUpdate logic
            var dataItem = e.Row.Item as TCOD_ZARMALIAT_MODEL;
            if (dataItem != null && TaxBrackets.IndexOf(dataItem) == 0)
            {
                var sazmanWindow = Application.Current.Windows.OfType<WIN_SAZMAN>().FirstOrDefault();
                if (sazmanWindow != null)
                {
                    sazmanWindow.ChangeIsHappend = true;
                    sazmanWindow.SAGHFH.Text = dataItem.MABL_H.ToString();
                }
            }
        }
        private void DG_MAIN_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var TheRow = e.Row.Item as TCOD_ZARMALIAT_MODEL;
            if (ConstructorRowDetector.IsPristine(TheRow)) { DG_SUB_CANCEL_EDIT(); return; }


            var data = e.Row.Item as TCOD_ZARMALIAT_MODEL;
            if (data == null) return;

            _isNewRecord = (DG_MAIN.Items.IndexOf(data) == DG_MAIN.Items.Count - 2);

            if (data.MABL_H <= 0)
            {
                new Msgwin(false, "مبلغ حقوق باید یک عدد مثبت باشد.").ShowDialog();
                DG_SUB_CANCEL_EDIT();
                return;
            }

            if (e.Row.IsNewItem)
            {
                bool pkExists = dbms.DoGetDataSQL<int>("SELECT 1 FROM TCOD_ZARMALIAT WHERE MABL_H = @MABL_H", new { data.MABL_H }).Any();
                if (pkExists)
                {
                    new Msgwin(false, "این مبلغ حقوق قبلا ثبت شده و قابل تکرار نیست.").ShowDialog();
                    DG_SUB_CANCEL_EDIT();
                    return;
                }
            }

            // --- Save Stage ---
            try
            {
                if (e.Row.IsNewItem)
                {
                    const string insertSql = @"
                            INSERT INTO dbo.TCOD_ZARMALIAT (MABL_H, ZARIB, MALIAT, UID) 
                            VALUES (@MABL_H, @ZARIB, @MALIAT, @UID)";
                    dbms.DoExecuteSQL(insertSql, new { data.MABL_H, data.ZARIB, data.MALIAT, UID = Baseknow.USERCOD });
                }
                else
                {
                    const string updateSql = @"
                            UPDATE dbo.TCOD_ZARMALIAT 
                            SET ZARIB = @ZARIB, MALIAT = @MALIAT, UID = @UID
                            WHERE MABL_H = @MABL_H";
                    dbms.DoExecuteSQL(updateSql, new { data.ZARIB, data.MALIAT, UID = Baseknow.USERCOD, data.MABL_H });
                }
            }
            catch (System.Exception ex)
            {
                new Msgwin(false, $"خطا در ذخیره سازی اطلاعات: {ex.Message}").ShowDialog();
                DG_SUB_CANCEL_EDIT();
            }
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            //if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }
            var editableCollectionView = DG_MAIN.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
            {
                try { editableCollectionView.CancelEdit(); } catch { }
            }

            var selectedItem = DG_MAIN.SelectedItem as TCOD_ZARMALIAT_MODEL;
            if (selectedItem == null || DG_MAIN.Items.IndexOf(selectedItem) >= TaxBrackets.Count) // Do not delete the placeholder row
            {
                new Msgwin(false, "لطفا ابتدا یک سطر را برای حذف انتخاب کنید.").ShowDialog();
                return;
            }

            var msg = new Msgwin(true, "آیا از حذف سطر اطمینان دارید؟");
            msg.ShowDialog();

            if (msg.DialogResult == true)
            {
                var Rowefer = ((TCOD_ZARMALIAT_MODEL)DG_MAIN.SelectedItem).Clone() as TCOD_ZARMALIAT_MODEL;
                _ = AuditLogger.LogActionAsync(
                                  actionType: "Delete",
                                  tableName: "مشخصات سیسیتم : ضرائب مالیات",
                                  recordId: default,
                                  oldValue: default,
                                  newValue: Rowefer,
                                  additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                try
                {
                    const string deleteSql = "DELETE FROM dbo.TCOD_ZARMALIAT WHERE MABL_H = @MABL_H";
                    dbms.DoExecuteSQL(deleteSql, new { selectedItem.MABL_H });
                    TaxBrackets.Remove(selectedItem);
                }
                catch (System.Exception ex)
                {
                    new Msgwin(false, $"خطا در عملیات حذف: {ex.Message}").ShowDialog();
                }
            }
        }

        private void DG_MAIN_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Prevent the DataGrid's default delete action
                e.Handled = true;
                // Call our button's logic instead
                BTN_DELETE_Click(null, null);
            }

            if (e == null || DG_MAIN == null || DG_MAIN.CurrentCell == null)
                return;

            string CURRENT_COLUMN_NAME = "";
            if (DG_MAIN.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = DG_MAIN.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME == "MABL_H" || CURRENT_COLUMN_NAME == "MABLK")
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
            }
            if (e.Key == Key.Subtract)
            {
                if (CURRENT_COLUMN_NAME == "MABL_H" || CURRENT_COLUMN_NAME == "MABLK")
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

    }
}
