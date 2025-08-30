using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Wins.WinOther;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    public partial class WIN_GRADE_SHART_FUNC_FORM : Window
    {
        #region Header Window Begin
        //Header Window Begin
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
                    //🗖,🗗
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
        public WIN_GRADE_SHART_FUNC_FORM()
        {
            InitializeComponent();
            this.DataContext = this;
            DG_DATA = new ObservableCollection<GRADE_SHART_FUNC>();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<GRADE_SHART_FUNC> DG_DATA { get; set; }

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        public class CMB_FUNCTION_ITEM
        {
            public string? CODE { get; set; }
            public string? NAMES { get; set; }
        }

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

            ReGetData();

            FILL_ALL_COMBOBOXES();

            if (DG_DATA.Count == 0)
            {
                DG_SUB.IsReadOnly = false;
            }

            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };
            GR_NAV_DATAGRID.NewRecordAction = () =>
            {
                DG_SUB.Focus();
                GR_NAV_DATAGRID.GoNewPlaceholder(DG_SUB, GSHNAME_COLUMN);
            };
        }

        private void ReGetData()
        {
            var RST = dbms.DoGetDataSQL<GRADE_SHART_FUNC>("SELECT * FROM GRADE_SHART_FUNC ORDER BY GSHARTID").ToList();
            DG_DATA.Clear();
            foreach (var item in RST)
            {
                DG_DATA.Add(item);
            }

            GetFocusonDataGrid(false);
        }

        private void GetFocusonDataGrid(bool _BeginEdit_)
        {
            var DEFINDX = (DG_SUB.SelectedIndex <= 0) ? DG_SUB.Items.Count - 1 : DG_SUB.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG_SUB, "GSHNAME", DEFINDX, _BeginEdit_);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = DG_SUB;
                UIElement uie = e.OriginalSource as UIElement;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    try
                    {
                        if (DG_SUB.IsKeyboardFocusWithin)
                        {
                            if (DG.CurrentColumn != null)
                            {
                                int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                                bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                                bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                                if (isLastColumn)
                                {
                                    // If it's the last column, move focus to the first cell of next row
                                    if (isLastRow)
                                    {
                                        // Add focus to new row if needed
                                        DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[1]);

                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            DG.BeginEdit();
                                        }), DispatcherPriority.Background);

                                        //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                        var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                        if (focusedWindow != null)
                                        {
                                            Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                focusedWindow.Activate();
                                                focusedWindow.Focus();
                                            }), DispatcherPriority.Background);
                                        }

                                        return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                    }
                                }
                            }
                        }

                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                    catch { /*ignore*/ }
                }
            }
            catch { }


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
            var functions = new List<CMB_FUNCTION_ITEM>
            {
                new CMB_FUNCTION_ITEM { CODE = "ChecketebarSSM",   NAMES = "ChecketebarSSM" },
                new CMB_FUNCTION_ITEM { CODE = "ChecketebarSSMF",  NAMES = "ChecketebarSSMF" },
                new CMB_FUNCTION_ITEM { CODE = "ChecketebarASSSM", NAMES = "ChecketebarASSSM" },
                new CMB_FUNCTION_ITEM { CODE = "MandTop10",        NAMES = "MandTop10" },
                new CMB_FUNCTION_ITEM { CODE = "MandLes10",        NAMES = "MandLes10" },
                new CMB_FUNCTION_ITEM { CODE = "FactCount",        NAMES = "FactCount" },
                new CMB_FUNCTION_ITEM { CODE = "ChekBrgashti",     NAMES = "ChekBrgashti" }
            };

            FunctionComboBoxColumn.ItemsSource = functions;
        }
        private bool BodyIsValid(GRADE_SHART_FUNC ROW, bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(ROW?.GSHNAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "عنوان شرط نمیتواند خالی باشد" });
            }

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }
            return true;
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            DG_SUB.IsReadOnly = false;
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_DELETE.IsEnabled || BTN_DELETE.Visibility != Visibility.Visible) { return; }

            if (DG_DATA.Count > 0)
            {
                if (DG_SUB.IsReadOnly) { return; }

                try
                {
                    var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(DG_SUB.ItemsSource);
                    if (view.IsAddingNew && view.CanCancelEdit)
                    {
                        //view.CancelNew();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else if (view.IsEditingItem && view.CanCancelEdit)
                    {
                        //view.CancelEdit();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else
                    {
                        //Cancel Any Editting to avoid conflict during remove
                        DG_SUB.CommitEdit(DataGridEditingUnit.Cell, true);
                        DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                    }
                }
                catch { }
            }


            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                bool IsDeletedSomething = false;

                if (DG_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
                {
                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                    {
                        var item = DG_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                        {
                            continue; //Skip deletion for new placeholder items
                        }
                        var NewRow = ((GRADE_SHART_FUNC)item).Clone() as GRADE_SHART_FUNC;
                        var _id_ = item.GetType().GetProperty("GSHARTID").GetValue(item);

                        _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "تعریف شروط گریدهای مشتریان",
                            recordId: _id_.ToString(),
                            oldValue: "",
                            newValue: NewRow,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        if (_id_ != null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"DELETE FROM dbo.GRADE_SHART_FUNC WHERE GSHARTID = {_id_}");
                                DG_DATA.Remove((GRADE_SHART_FUNC)item);
                                IsDeletedSomething = true;
                            }
                            catch (SqlException ex)
                            {
                                if (ex.Number == 547)
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                                }
                                else
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                                }
                            }
                            catch (Exception)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                            }
                        }
                    }

                    if (ErrosMessages.Any())
                    {
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();
                    }
                    else if (IsDeletedSomething)
                    {
                        //ReGetData();
                    }
                }
            }
        }

        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
        }
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as GRADE_SHART_FUNC;
            if (e.Row.Item == null || ROW is null) { return; }
            if (ConstructorRowDetector.IsPristine(ROW)) { DG_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(ROW))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            try
            {
                string sql;

                if (e.Row.IsNewItem)
                {
                    var existing = DG_DATA.FirstOrDefault(g => g.GSHARTID == ROW.GSHARTID && g != ROW);
                    if (existing != null)
                    {
                        universControl.PopNotifyShowUp($"شماره شرط {ROW.GSHARTID} تکراری است.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        DG_SUB_CANCEL_EDIT();
                        return;
                    }

                    var idd = CL_HESABDARI.GetLIDD("GRADE_SHART_FUNC", "GSHARTID");

                    ROW.GSHARTID = (int)idd;

                    // INSERT new record
                    sql = @"INSERT INTO GRADE_SHART_FUNC (GSHARTID, GSHNAME, GSHFUNC, TOZIH)
                            OUTPUT INSERTED.GSHARTID
                            VALUES (@GSHARTID, @GSHNAME, @GSHFUNC, @TOZIH)";

                    dbms.DoExecuteSQL(sql, ROW);
                }
                else
                {
                    // UPDATE existing record
                    sql = @"UPDATE GRADE_SHART_FUNC 
                            SET GSHNAME = @GSHNAME, GSHFUNC = @GSHFUNC, TOZIH = @TOZIH
                            WHERE GSHARTID = @GSHARTID";

                    dbms.DoExecuteSQL(sql, ROW);
                }
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    universControl.PopNotifyShowUp($"خطا در ذخیره سازی", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
                return;
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShowUp($"خطا در ذخیره سازی", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                DG_SUB_CANCEL_EDIT();
            }



        }

        private void DG_SUB_CANCEL_EDIT()
        {
            DG_SUB.Dispatcher.Invoke(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;

                DG_SUB.CancelEdit();

                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }

        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            {
                try
                {
                    // 1) اگر داخل یک TextBox در حالت ویرایش هستیم، کاری نکنیم
                    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
                    {
                        // اجازه بدهید Delete عادی متن کارش رو بکنه
                        return;
                    }
                    //else
                    //{
                    //    // اگر داخل حالت ویرایش سلول هستیم، از رفتار پیش‌فرض Delete (حذف کاراکتر) استفاده کن
                    //    var cell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                    //    if (cell != null && cell.IsEditing)
                    //        return;
                    //}
                }
                catch { }

                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
        }
    }
}