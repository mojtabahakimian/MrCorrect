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
    public partial class TCOD_BANKS_WIN : Window
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
        public TCOD_BANKS_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<TCOD_BANKS> BAKNS_DATA { get; set; } = new ObservableCollection<TCOD_BANKS>();
        public TCOD_BANKS? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TCOD_BANKS? WAS_ROW_ITEM { get; private set; }
        public bool NowIsReady { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        UniversControl universControl = new UniversControl();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            ReGetData();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void ReGetData()
        {
            BAKNS_DATA?.Clear();
            var DTBNK = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT CODE, NAMES, IDD FROM dbo.TCOD_BANKS").ToList();
            foreach (var item in DTBNK)
            {
                BAKNS_DATA.Add(item);
            }
        }
        private void TCOD_BANKS_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TCOD_BANKS_SUB.Dispatcher.InvokeAsync(() =>
            {
                TCOD_BANKS_SUB.CellEditEnding -= TCOD_BANKS_SUB_CellEditEnding;
                TCOD_BANKS_SUB.RowEditEnding -= TCOD_BANKS_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TCOD_BANKS_SUB.CancelEdit();
                }
                else
                {
                    TCOD_BANKS_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TCOD_BANKS_SUB.RowEditEnding += TCOD_BANKS_SUB_RowEditEnding;
                TCOD_BANKS_SUB.CellEditEnding += TCOD_BANKS_SUB_CellEditEnding;
            });
        }
        private void TCOD_BANKS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TCOD_BANKS;

            //if (e.Column.SortMemberPath == "CODE")
            //{
            //    if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) && CURRENT_ROW_ITEMS?.IDD is not null)
            //    {
            //        var DataBank = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 CODE FROM dbo.TCOD_BANKS WHERE CODE = {ENTERED_VALUE_ROW.ToStringNullSafe()}").ToList();
            //        if (DataBank.Count > 0)
            //        {
            //            universControl.PopNotifyShow("این کد از قبل وجود دارد.", Pop1, Pop1Text1, Pop_Border1);
            //            TCOD_BANKS_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
            //        }
            //    }
            //}

            #endregion
            if (e.Column.SortMemberPath == "NAMES")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام بانک نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.NAMES = WAS_ROW_ITEM?.NAMES;
                    TCOD_BANKS_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }
        }
        private bool BodyIsValid(TCOD_BANKS _row)
        {
            var ROW = _row;

            var errors = (from object i in TCOD_BANKS_SUB.ItemsSource
                          let c = TCOD_BANKS_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام بانک نمی تواند خالی باشد" });
            }

            //if (!string.IsNullOrEmpty(ROW.CODE.ToStringNullSafe()))
            //{
            //    var DataBank = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 CODE FROM dbo.TCOD_BANKS WHERE CODE = {ROW.CODE}").ToList();
            //    if (DataBank.Count > 0)
            //    {
            //        ErrosMessages.Add(new MsgModel { MessageText_U = "این کد از قبل وجود دارد" });
            //    }
            //}

            if (ErrosMessages.Count > 0)
            {
                TCOD_BANKS_SUB_CANCEL_EDIT();

                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void TCOD_BANKS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var ROW = e.Row.Item as TCOD_BANKS;
            if (!BodyIsValid(ROW)) { return; }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            int? THECODE = null;

            if (string.IsNullOrEmpty(ROW?.CODE.ToStringNullSafe()))
            {
                THECODE = (int)dbms.DoGetDataSQL<int?>("SELECT MAX(CODE + 1) FROM dbo.TCOD_BANKS").FirstOrDefault();
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
                if (ROW?.IDD is null)
                {

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TCOD_BANKS(CODE, NAMES)
                                     VALUES({THECODE}, N'{ROW.NAMES}' ) ");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCOD_BANKS
                                     SET NAMES = N'{ROW.NAMES}'
                                     WHERE IDD = {ROW.IDD}");
                }
            }
            catch (SqlException ex)
            {
                TCOD_BANKS_SUB_CANCEL_EDIT();
                if (ex.Number == 547)
                {
                    new Msgwin(false, "صرفا نام این بانک دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            ReGetData();

            CL_LMethods.MovingDG(TCOD_BANKS_SUB, null, CURRENT_ROW_INDEX);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void TCOD_BANKS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TCOD_BANKS_SUB.Items.Count > 0 && TCOD_BANKS_SUB.SelectedItem != null)
                {
                    if (!(TCOD_BANKS_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف بانک",
                                    recordId: TCOD_BANKS_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TCOD_BANKS_SUB.SelectedItems.Count; i++)
                            {
                                var item = TCOD_BANKS_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("IDD").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _idd = item.GetType().GetProperty("IDD").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_BANKS WHERE IDD = {_idd}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var BANKNAME = item.GetType().GetProperty("NAMES").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"صرفا نام این بانک ({BANKNAME}) دارای گردش است و نمی توان آنرا حذف کرد" });
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

                        //After Opration:
                        if (IsDeletedSomething)
                        {
                            ReGetData();
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                  .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }
                    }
                }
            }
        }

        private void TCOD_BANKS_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TCOD_BANKS_SUB.SelectedItem is not null)
            {
                if (TCOD_BANKS_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TCOD_BANKS)TCOD_BANKS_SUB.SelectedItem).Clone() as TCOD_BANKS;
                }
            }
        }

        private void TCOD_BANKS_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(TCOD_BANKS_SUB.Items.Count < 1) && !(TCOD_BANKS_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = TCOD_BANKS_SUB.SelectedIndex;
                }
            }
        }

     
    }
}
