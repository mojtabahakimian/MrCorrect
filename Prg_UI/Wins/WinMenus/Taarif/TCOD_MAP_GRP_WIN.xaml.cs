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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.Taarif
{
    public partial class TCOD_MAP_GRP_WIN : Window
    {
        public ObservableCollection<TCOD_MAP_GRP> TCOD_MAP_GRP_DATA { get; set; } = new ObservableCollection<TCOD_MAP_GRP>();
        public TCOD_MAP_GRP_WIN()
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; private set; } = false;
        public TCOD_MAP_GRP? CURRENT_ROW_ITEMS { get; private set; }
        public TCOD_MAP_GRP? WAS_ROW_ITEM { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        UniversControl universControl = new UniversControl();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            ReGetData();
        }
        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (TCOD_MAP_GRP_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TCOD_MAP_GRP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "MPP")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        datagridname_tbox_def_index_col = 0;
                    }
                    else
                    {
                        datagridname_tbox_def_index_col = (int)defaultcolumnindex;
                    }
                }
                return datagridname_tbox_def_index_col;
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = TCOD_MAP_GRP_SUB;
                UIElement uie = e.OriginalSource as UIElement;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    try
                    {
                        if (TCOD_MAP_GRP_SUB.IsKeyboardFocusWithin)
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

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[INVO_LST_SUB_DEF_INDEX_COL]);

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
        private void TCOD_MAP_GRP_SUB_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {

        }
        private bool BodyIsValid(TCOD_MAP_GRP _row)
        {
            var ROW = _row;

            var errors = (from object i in TCOD_MAP_GRP_SUB.ItemsSource
                          let c = TCOD_MAP_GRP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            //MPP //کد *
            //MPNAME // نام مپ *
            //SIZEF //سايز كد در مپ *
            //STARTF //شروع كد از موقعيت

            if (string.IsNullOrEmpty(ROW?.MPP.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد نمی تواند خالی باشد" });
            }
            else
            {
                if (!int.TryParse(ROW?.MPP.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "کد مجاز نیست" });
                }
            }
            if (string.IsNullOrEmpty(ROW?.MPNAME.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مپ نمی تواند خالی باشد" });
            }
            else
            {
                if (!int.TryParse(ROW?.MPNAME.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نام مجاز نیست" });
                }
            }
            if (string.IsNullOrEmpty(ROW?.SIZEF.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "سايز كد در مپ نمی تواند خالی باشد" });
            }
            else
            {
                if (!int.TryParse(ROW?.SIZEF.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نام مجاز نیست" });
                }
            }

            if (!string.IsNullOrEmpty(ROW?.STARTF.ToStringNullSafe()))
            {
                if (!int.TryParse(ROW?.STARTF.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شروع کد از موقعیت مجاز نیست" });

                }
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void TCOD_MAP_GRP_SUB_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

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
                ENTERED_VALUE_ROW = (string?)Comboval.SelectedValue;
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TCOD_MAP_GRP;
            #endregion

            if (e.Column.SortMemberPath == "MPP")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToString()))
                {
                    universControl.PopNotifyShow("کد مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MPP = WAS_ROW_ITEM?.MPP;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
            if (e.Column.SortMemberPath == "MPNAME")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToString()))
                {
                    universControl.PopNotifyShow("نام مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MPNAME = WAS_ROW_ITEM?.MPNAME;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
            if (e.Column.SortMemberPath == "SIZEF")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToString()))
                {
                    universControl.PopNotifyShow("سايز كد در مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.SIZEF = WAS_ROW_ITEM?.SIZEF;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
        }
        private void TCOD_MAP_GRP_SUB_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            var ROW = e.Row.Item as TCOD_MAP_GRP;
            if (ConstructorRowDetector.IsPristine(ROW)) { TCOD_MAP_GRP_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(ROW))
            {
                return;
            }


            long? _idd = null;
            try
            {

                if (ROW?.ID is null) //INSERT
                {
                    _idd = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.TCOD_MAP_GRP(MPP, MPNAME, SIZEF, STARTF)
                                         OUTPUT INSERTED.ID
                                         VALUES({ROW.MPP},
                                         N'{ROW.MPNAME}' ,
                                         {ROW.SIZEF} ,
                                         {(ROW.STARTF is null ? "NULL" : ROW.STARTF)} )").FirstOrDefault();
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCOD_MAP_GRP
                                         SET MPP = {ROW.MPP}, MPNAME = N'{ROW.MPNAME}',
                                         SIZEF = {ROW.SIZEF}, STARTF = {(ROW.STARTF is null ? "NULL" : ROW.STARTF)}
                                         WHERE ID = {ROW.ID}");
                }
            }
            catch (SqlException ex)
            {
                TCOD_MAP_GRP_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این کد تکراری است !").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception)
            {
                TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }

            if (_idd != null)
            {
                ROW.ID = _idd;
            }

            //ReGetData();
        }
        private void ReGetData()
        {
            TCOD_MAP_GRP_DATA?.Clear();
            var _DATA_ = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT MPP, MPNAME, SIZEF, STARTF, CRT, UID, ID FROM dbo.TCOD_MAP_GRP").ToList();
            foreach (var item in _DATA_)
            {
                TCOD_MAP_GRP_DATA.Add(item);
            }

            CL_LMethods.MovingDG(TCOD_MAP_GRP_SUB, null, TCOD_MAP_GRP_SUB.Items.Count is 0 ? 0 : TCOD_MAP_GRP_SUB.Items.Count - 1);
        }
        private void TCOD_MAP_GRP_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TCOD_MAP_GRP_SUB.Dispatcher.Invoke(() =>
            {
                TCOD_MAP_GRP_SUB.CellEditEnding -= TCOD_MAP_GRP_SUB_CellEditEnding;
                TCOD_MAP_GRP_SUB.RowEditEnding -= TCOD_MAP_GRP_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TCOD_MAP_GRP_SUB.CancelEdit();
                }
                else
                {
                    TCOD_MAP_GRP_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TCOD_MAP_GRP_SUB.RowEditEnding += TCOD_MAP_GRP_SUB_RowEditEnding;
                TCOD_MAP_GRP_SUB.CellEditEnding += TCOD_MAP_GRP_SUB_CellEditEnding;
            });
        }
        private void TCOD_MAP_GRP_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {


            if (e.Key is Key.Delete)
            {
                try
                {
                    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
                    {
                        return;
                    }
                }
                catch { }

                if (TCOD_MAP_GRP_SUB.Items.Count > 0 && TCOD_MAP_GRP_SUB.SelectedItem != null)
                {
                    if (!(TCOD_MAP_GRP_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "کدینگ مپ",
                                    recordId: TCOD_MAP_GRP_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TCOD_MAP_GRP_SUB.SelectedItems.Count; i++)
                            {
                                var item = TCOD_MAP_GRP_SUB.SelectedItems[i];

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

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_MAP_GRP WHERE ID = {_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این کد دارای گردش است و نمیتوان آنرا پاک کرد!" });
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
                            ReGetData();
                        }
                    }
                }
            }
        }
    }
}
