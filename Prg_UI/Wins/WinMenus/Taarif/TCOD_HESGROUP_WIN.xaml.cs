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
using System.Windows.Interop;

namespace Wins.WinMenus.Taarif
{
    public partial class TCOD_HESGROUP_WIN : Window
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

        public ObservableCollection<TCOD_HESGROUP> HESGROUP_DATA { get; set; } = new ObservableCollection<TCOD_HESGROUP>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public TCOD_HESGROUP? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TCOD_HESGROUP? WAS_ROW_ITEM { get; private set; }
        public bool NowIsReady { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        UniversControl universControl = new UniversControl();
        public TCOD_HESGROUP_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "GHES", new WindowInteropHelper(this).Handle, this.GetType().Name); //دسترسی
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            ReGetData();
        }
        private void ReGetData()
        {
            HESGROUP_DATA?.Clear();
            var _DATA_ = dbms.DoGetDataSQL<TCOD_HESGROUP>("SELECT CODE, NAMES, CRT, UID FROM dbo.TCOD_HESGROUP ORDER BY CODE").ToList();
            foreach (var item in _DATA_)
            {
                HESGROUP_DATA.Add(item);
            }
        }

        private void TCOD_HESGROUP_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TCOD_HESGROUP_SUB.Dispatcher.InvokeAsync(() =>
            {
                TCOD_HESGROUP_SUB.CellEditEnding -= TCOD_HESGROUP_SUB_CellEditEnding;
                TCOD_HESGROUP_SUB.RowEditEnding -= TCOD_HESGROUP_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TCOD_HESGROUP_SUB.CancelEdit();
                }
                else
                {
                    TCOD_HESGROUP_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TCOD_HESGROUP_SUB.RowEditEnding += TCOD_HESGROUP_SUB_RowEditEnding;
                TCOD_HESGROUP_SUB.CellEditEnding += TCOD_HESGROUP_SUB_CellEditEnding;
            });
        }
        private bool BodyIsValid(TCOD_HESGROUP _row)
        {
            var ROW = _row;

            var errors = (from object i in TCOD_HESGROUP_SUB.ItemsSource
                          let c = TCOD_HESGROUP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.NAMES))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام گروه نمی تواند خالی باشد" });
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

        private void TCOD_HESGROUP_SUB_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TCOD_HESGROUP_SUB.SelectedItem is not null)
            {
                if (TCOD_HESGROUP_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TCOD_HESGROUP)TCOD_HESGROUP_SUB.SelectedItem).Clone() as TCOD_HESGROUP;
                }
            }
        }
        private void TCOD_HESGROUP_SUB_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
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
            {
            }
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TCOD_HESGROUP;
            #endregion

            if (e.Column.SortMemberPath == "NAMES")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام گروه نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    TCOD_HESGROUP_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.NAMES = WAS_ROW_ITEM?.NAMES;
                }
            }
        }
        private void TCOD_HESGROUP_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as TCOD_HESGROUP;

            if (!BodyIsValid(ROW))
            {
                TCOD_HESGROUP_SUB_CANCEL_EDIT();
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            double? THECODE = null;

            if (string.IsNullOrEmpty(ROW?.CODE.ToStringNullSafe()))
            {
                THECODE = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(CODE + 1) FROM dbo.TCOD_HESGROUP").FirstOrDefault();
                if (THECODE is null || THECODE.ToStringNullSafe() == "NULL")
                {
                    THECODE = 1;
                }
            }
            else
            {
                THECODE = ROW.CODE;
            }

            try
            {
                if (ROW?.CODE is null)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TCOD_HESGROUP(CODE, NAMES)
                                VALUES({THECODE}, N'{ROW.NAMES}' ) ");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCOD_HESGROUP
                                SET NAMES = N'{ROW.NAMES}'
                                WHERE CODE = {ROW.CODE}");
                }
            }
            catch (SqlException ex)
            {
                TCOD_HESGROUP_SUB_CANCEL_EDIT();

                if (ex.Number == 547)
                {
                    new Msgwin(false, "نام این گروه دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    new Msgwin(false, "نام تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            ReGetData();

            CL_LMethods.MovingDG(TCOD_HESGROUP_SUB, null, CURRENT_ROW_INDEX);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void TCOD_HESGROUP_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TCOD_HESGROUP_SUB.Items.Count > 0 && TCOD_HESGROUP_SUB.SelectedItem != null)
                {
                    if (!(TCOD_HESGROUP_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعريف  گروه حسابها",
                                    recordId: TCOD_HESGROUP_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TCOD_HESGROUP_SUB.SelectedItems.Count; i++)
                            {
                                var item = TCOD_HESGROUP_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("CODE").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _code = item.GetType().GetProperty("CODE").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_HESGROUP WHERE CODE = {_code}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var BANKNAME = item.GetType().GetProperty("NAMES").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"نام این گروه ({BANKNAME}) دارای گردش است و نمی توان آنرا حذف کرد" });
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

        private void CODE_Dbl_Click(object sender, RoutedEventArgs e)
        {
            //DoCmd.OpenForm("TOTA_HES_SHEET", acFormDS, default, "GROUP = " + this.CODE);

            //SELECT* FROM "dbo"."TOTA_HES" WHERE(GROUP = 11)
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }

        private void TCOD_HESGROUP_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(TCOD_HESGROUP_SUB.Items.Count < 1) && !(TCOD_HESGROUP_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = TCOD_HESGROUP_SUB.SelectedIndex;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
    }
}
