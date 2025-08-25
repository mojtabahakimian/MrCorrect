using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Wins.WinMenus.Taarif
{
    public partial class TCOD_ANBAR_WIN : Window
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
        public ObservableCollection<TCOD_ANBAR> ANBARS_DATA { get; set; } = new ObservableCollection<TCOD_ANBAR>();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public int CURRENT_ROW_INDEX { get; set; } = 0;
        public class ANBAR_MODEL_QRE1
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
        }
        public TCOD_ANBAR_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "ANBAR", new WindowInteropHelper(this).Handle, this.GetType().Name); //چک کردن دسترسی
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

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
        private void TCOD_ANBAR_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TCOD_ANBAR_SUB.Items.Count > 0 && TCOD_ANBAR_SUB.SelectedItem != null)
                {
                    if (!(TCOD_ANBAR_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف انبار ها",
                                    recordId: TCOD_ANBAR_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TCOD_ANBAR_SUB.SelectedItems.Count; i++)
                            {
                                var item = TCOD_ANBAR_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("idd").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _idd = item.GetType().GetProperty("idd").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_ANBAR WHERE idd = {_idd}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var ANBARNAME = item.GetType().GetProperty("NAMES").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این انبار ({ANBARNAME}) دارای گردش است و نمی توان آنرا حذف کرد" });
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
        public List<COMBOYMODEL> ANBAR_ITEMS_MODEL { get; set; } = new List<COMBOYMODEL>()
        {
            new COMBOYMODEL { ID = 0, NAME = "خدمات" },
            new COMBOYMODEL { ID = 1, NAME = "مواد مصرفی تولید" },
            new COMBOYMODEL { ID = 2, NAME = "نیمه ساخته" },
            new COMBOYMODEL { ID = 3, NAME = "ساخته شده" },
            new COMBOYMODEL { ID = 4, NAME = "سایر" }
        };
        public TCOD_ANBAR? CURRENT_ROW_ITEMS { get; private set; }
        public TCOD_ANBAR? WAS_ROW_ITEM { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private void FILL_ALL_COMBOBOXES()
        {
            COLUMN_ANBARTYPE.ItemsSource = ANBAR_ITEMS_MODEL;
        }
        private void ReGetData()
        {
            ANBARS_DATA?.Clear();
            var DTANBAR = dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT CODE ,NAMES , KIND , idd FROM dbo.TCOD_ANBAR WHERE(((TCOD_ANBAR.CODE)<>0))").ToList();
            foreach (var item in DTANBAR)
            {
                ANBARS_DATA.Add(item);
            }
        }
        private bool BodyIsValid(TCOD_ANBAR _row)
        {
            var ROW = _row;

            var errors = (from object i in TCOD_ANBAR_SUB.ItemsSource
                          let c = TCOD_ANBAR_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام انبار نمی تواند خالی باشد" });
            }

            if (string.IsNullOrEmpty(ROW?.KIND.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع انبار نمی تواند خالی باشد" });
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
        private void TCOD_ANBAR_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TCOD_ANBAR;
            #endregion

            if (e.Column.SortMemberPath == "NAMES")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.NAMES.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام انبار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.NAMES = WAS_ROW_ITEM?.NAMES;
                    TCOD_ANBAR_SUB_CANCEL_EDIT();
                }
            }
            if (e.Column.SortMemberPath == "KIND")
            {
                if (string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.KIND.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نوع انبار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ROW_ITEMS.KIND = WAS_ROW_ITEM?.KIND;
                    TCOD_ANBAR_SUB_CANCEL_EDIT();
                }
            }
        }
        private void TCOD_ANBAR_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as TCOD_ANBAR;
            if (!BodyIsValid(ROW))
            {
                TCOD_ANBAR_SUB.Dispatcher.BeginInvoke(() =>
                {
                    TCOD_ANBAR_SUB.CellEditEnding -= TCOD_ANBAR_SUB_CellEditEnding;
                    TCOD_ANBAR_SUB.RowEditEnding -= TCOD_ANBAR_SUB_RowEditEnding;

                    TCOD_ANBAR_SUB.CancelEdit();

                    TCOD_ANBAR_SUB.RowEditEnding += TCOD_ANBAR_SUB_RowEditEnding;
                    TCOD_ANBAR_SUB.CellEditEnding += TCOD_ANBAR_SUB_CellEditEnding;
                });

                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            int? THECODE = null;

            if (string.IsNullOrEmpty(ROW?.CODE.ToStringNullSafe()))
            {
                THECODE = (int)dbms.DoGetDataSQL<int?>("SELECT MAX(CODE + 1) FROM dbo.TCOD_ANBAR").FirstOrDefault();
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
                if (ROW?.idd is null)
                {

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TCOD_ANBAR(CODE, NAMES, KIND)
                                         VALUES({THECODE},
                                         N'{ROW.NAMES}' ,
                                         {ROW.KIND})");

                    ROW.CODE = THECODE;
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCOD_ANBAR
                                         SET NAMES = N'{ROW.NAMES}', KIND = {ROW.KIND}
                                         WHERE idd = {ROW.idd}");
                }

                #region Form_AfterUpdate
                var rst = dbms.DoGetDataSQL<ANBAR_MODEL_QRE1>($"SELECT TOP 1 N_KOL, NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={Baseknow.MOGODIA} AND NUMBER={ROW.CODE}").FirstOrDefault();

                var _N_KOL = Baseknow.MOGODIA;
                var _NUMBER = ROW.CODE;
                var _NAME = ROW.NAMES;

                if (rst is null)
                {
                    //rst.AddNew();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME)
                                         VALUES({_N_KOL}, {_NUMBER}, N'{_NAME}')");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES SET N_KOL = {_N_KOL}, NUMBER = {_NUMBER}, NAME = N'{_NAME}' 
                                         WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER}");
                }
                #endregion

            }
            catch (SqlException ex)
            {
                TCOD_ANBAR_SUB_CANCEL_EDIT();
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این انبار دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
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

            int lastindexrow = 0;
            lastindexrow = TCOD_ANBAR_SUB.Items.IndexOf(TCOD_ANBAR_SUB?.CurrentItem);
            CL_LMethods.MovingDG(TCOD_ANBAR_SUB, null, lastindexrow);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void TCOD_ANBAR_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TCOD_ANBAR_SUB.SelectedItem is not null)
            {
                if (TCOD_ANBAR_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TCOD_ANBAR)TCOD_ANBAR_SUB.SelectedItem).Clone() as TCOD_ANBAR;
                }
            }
        }

        private void TCOD_ANBAR_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TCOD_ANBAR_SUB.Dispatcher.InvokeAsync(() =>
            {
                TCOD_ANBAR_SUB.CellEditEnding -= TCOD_ANBAR_SUB_CellEditEnding;
                TCOD_ANBAR_SUB.RowEditEnding -= TCOD_ANBAR_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TCOD_ANBAR_SUB.CancelEdit();
                }
                else
                {
                    TCOD_ANBAR_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TCOD_ANBAR_SUB.RowEditEnding += TCOD_ANBAR_SUB_RowEditEnding;
                TCOD_ANBAR_SUB.CellEditEnding += TCOD_ANBAR_SUB_CellEditEnding;
            });
        }


    }
}
