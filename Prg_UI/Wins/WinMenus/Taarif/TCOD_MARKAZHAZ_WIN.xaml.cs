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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Wins.WinMenus.Taarif
{
    public partial class TCOD_MARKAZHAZ_WIN : Window
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
        public TCOD_MARKAZHAZ_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<TCOD_MARKAZHAZ> TCOD_MARKAZHAZ_DATA { get; set; } = new ObservableCollection<TCOD_MARKAZHAZ>();
        public bool ChangeIsHappend { get; private set; } = false;
        public int CURRENT_ROW_INDEX { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TCOD_MARKAZHAZ? CURRENT_ROW_ITEMS { get; private set; }
        public TCOD_MARKAZHAZ? WAS_ROW_ITEM { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (TCOD_MARKAZHAZ_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TCOD_MARKAZHAZ_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "MHAZNAME")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _name_code_index = 0;
                    }
                    else
                    {
                        _name_code_index = (int)defaultcolumnindex;
                    }
                }
                return _name_code_index;
            }
        }

        public bool TCOD_MARKAZHAZ_SUB_IsFocused { get; private set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "DEPART", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            ReGetData(true);

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = TCOD_MARKAZHAZ_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                try
                {
                    if (uie is DataGridCell || TCOD_MARKAZHAZ_SUB_IsFocused)
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

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[NAME_CODE_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        //DG.BeginEdit();
                                        //TCOD_MARKAZHAZ_SUB_CANCEL_EDIT(); //New Just Test
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                }
                catch { /*ignore*/ }

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
            }
        }

        private void TCOD_MARKAZHAZ_SUB_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TCOD_MARKAZHAZ_SUB.SelectedItem is not null)
            {
                if (TCOD_MARKAZHAZ_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TCOD_MARKAZHAZ)TCOD_MARKAZHAZ_SUB.SelectedItem).Clone() as TCOD_MARKAZHAZ;
                }
            }
        }
        private void TCOD_MARKAZHAZ_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TCOD_MARKAZHAZ_SUB.Items.Count > 0 && TCOD_MARKAZHAZ_SUB.SelectedItem != null)
                {
                    if (!(TCOD_MARKAZHAZ_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {

                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف مراکز هزینه",
                                    recordId: TCOD_MARKAZHAZ_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TCOD_MARKAZHAZ_SUB.SelectedItems.Count; i++)
                            {
                                var item = TCOD_MARKAZHAZ_SUB.SelectedItems[i];


                                var editableCollectionView = TCOD_MARKAZHAZ_SUB.Items as IEditableCollectionView;
                                if (editableCollectionView != null && editableCollectionView.IsEditingItem)
                                {
                                    editableCollectionView.CommitEdit();
                                }
                                if (CL_LMethods.IsNewPlaceHolder(TCOD_MARKAZHAZ_SUB, item)) // Check if the item is a new placeholder Row
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _id = item.GetType().GetProperty("ID").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_MARKAZHAZ WHERE ID = {_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;
                                                var _MHAZNAME_ = item.GetType().GetProperty("MHAZNAME").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این مرکز مصرف ({_MHAZNAME_}) دارای گردش است و نمی توان آنرا حذف کرد" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                  .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }

                        //After Opration:
                        if (IsDeletedSomething)
                        {
                            ReGetData(false);
                        }
                    }
                }
            }
        }
        private void TCOD_MARKAZHAZ_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS_
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

            CURRENT_ROW_INDEX = row_index;

            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TCOD_MARKAZHAZ;
            #endregion

            if (e.Column.SortMemberPath == "MHAZ_NO")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.MHAZ_NO.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("کد نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MHAZ_NO = WAS_ROW_ITEM?.MHAZ_NO;
                    TCOD_MARKAZHAZ_SUB_CANCEL_EDIT();
                }
            }

            if (e.Column.SortMemberPath == "MHAZNAME")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.MHAZNAME.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MHAZNAME = WAS_ROW_ITEM?.MHAZNAME;
                    TCOD_MARKAZHAZ_SUB_CANCEL_EDIT();
                }
            }
        }
        private void TCOD_MARKAZHAZ_SUB_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            var ROW = e.Row.Item as TCOD_MARKAZHAZ;
            if (!BodyIsValid(ROW))
            {
                TCOD_MARKAZHAZ_SUB_CANCEL_EDIT(); return;
            }

            long? _id_ = null;
            try
            {
                if (ROW?.ID > 0) //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCOD_MARKAZHAZ SET MHAZ_NO = {ROW.MHAZ_NO}, MHAZNAME = N'{ROW.MHAZNAME}' WHERE ID = {ROW.ID}");
                }
                else //INSERT
                {
                    //Form_BeforeUpdate
                    if (ROW?.MHAZ_NO is null)
                    {
                        var RST = dbms.DoGetDataSQL<int?>("SELECT Max(MHAZ_NO) AS MaxOfMHAZ_NO FROM TCOD_MARKAZHAZ").FirstOrDefault();
                        if (RST != null)
                        {
                            ROW.MHAZ_NO = Convert.ToInt32(RST) + 1;
                        }
                        else
                        {
                            ROW.MHAZ_NO = 1;
                        }
                    }

                    _id_ = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.TCOD_MARKAZHAZ(MHAZ_NO, MHAZNAME)
                                                       OUTPUT INSERTED.ID
                                                       VALUES({ROW.MHAZ_NO},
                                                       N'{ROW.MHAZNAME}') ").FirstOrDefault();

                    if (_id_ != null)
                    {
                        ROW.ID = _id_;
                    }

                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "ذخیره به دلیل خطا انجام نشد!").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void ReGetData(bool GOTOLAST)
        {
            TCOD_MARKAZHAZ_DATA?.Clear();
            var RST = dbms.DoGetDataSQL<TCOD_MARKAZHAZ>("SELECT ID,MHAZ_NO, MHAZNAME, CRT, UID FROM dbo.TCOD_MARKAZHAZ").ToList();
            foreach (var item in RST)
            {
                TCOD_MARKAZHAZ_DATA.Add(item);
            }

            var _DataGrid_ = TCOD_MARKAZHAZ_SUB;
            string _SORTPATH_ = "MHAZNAME";
            int lastindexrow = _DataGrid_.Items.Count - 1;

            if (GOTOLAST)
            {
                CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
            }
            else
            {
                lastindexrow = _DataGrid_.Items.IndexOf(_DataGrid_?.CurrentItem);
                if (lastindexrow > 0)
                {
                    CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, lastindexrow, false);
                }
                else
                {
                    CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
                }
            }
        }
        private bool BodyIsValid(TCOD_MARKAZHAZ _row)
        {
            var ROW = _row;

            var errors = (from object i in TCOD_MARKAZHAZ_SUB.ItemsSource
                          let c = TCOD_MARKAZHAZ_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.MHAZNAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام نمی تواند خالی باشد" });
            }

            if ((bool)(ROW?.MHAZ_NO.HasValue))
            {
                if (!int.TryParse(ROW?.MHAZ_NO.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "کد مجاز نیست" });
                }
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void TCOD_MARKAZHAZ_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TCOD_MARKAZHAZ_SUB.Dispatcher.InvokeAsync(() =>
            {
                TCOD_MARKAZHAZ_SUB.CellEditEnding -= TCOD_MARKAZHAZ_SUB_CellEditEnding;
                TCOD_MARKAZHAZ_SUB.RowEditEnding -= TCOD_MARKAZHAZ_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TCOD_MARKAZHAZ_SUB.CancelEdit();
                }
                else
                {
                    TCOD_MARKAZHAZ_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TCOD_MARKAZHAZ_SUB.RowEditEnding += TCOD_MARKAZHAZ_SUB_RowEditEnding;
                TCOD_MARKAZHAZ_SUB.CellEditEnding += TCOD_MARKAZHAZ_SUB_CellEditEnding;
            });
        }

        private void TCOD_MARKAZHAZ_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TCOD_MARKAZHAZ_SUB_IsFocused = false;
            }
            else
            {
                TCOD_MARKAZHAZ_SUB_IsFocused = true;
            }
        }
    }
}
