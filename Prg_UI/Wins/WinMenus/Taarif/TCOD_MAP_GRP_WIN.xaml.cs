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
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
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
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.MPP.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("کد مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MPP = WAS_ROW_ITEM?.MPP;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
            if (e.Column.SortMemberPath == "MPNAME")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.MPNAME.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.MPNAME = WAS_ROW_ITEM?.MPNAME;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
            if (e.Column.SortMemberPath == "SIZEF")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.SIZEF.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("سايز كد در مپ نمیتوان خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.SIZEF = WAS_ROW_ITEM?.SIZEF;
                    TCOD_MAP_GRP_SUB_CANCEL_EDIT();
                }
            }
        }
        private void TCOD_MAP_GRP_SUB_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            var ROW = e.Row.Item as TCOD_MAP_GRP;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            try
            {
                if (ROW?.ID is null) //INSERT
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TCOD_MAP_GRP(MPP, MPNAME, SIZEF, STARTF)
                                         VALUES({ROW.MPP},
                                         N'{ROW.MPNAME}' ,
                                         {ROW.SIZEF} ,
                                         {(ROW.STARTF is null ? "NULL" : ROW.STARTF)} )");
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
                    new Msgwin(false, "این کد تکراری است !");
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            ReGetData();
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
            TCOD_MAP_GRP_SUB.Dispatcher.InvokeAsync(() =>
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
