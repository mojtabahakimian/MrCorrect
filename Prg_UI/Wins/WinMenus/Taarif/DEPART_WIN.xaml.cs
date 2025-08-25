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
using static Wins.WinMenus.Taarif.TCOD_ANBAR_WIN;

namespace Wins.WinMenus.Taarif
{
    public partial class DEPART_WIN : Window
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
        public DEPART_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public bool ChangeIsHappend { get; private set; } = false;
        public bool NowIsReady { get; private set; }
        public ObservableCollection<DEPART_MODEL> DEPART_DATA { get; set; } = new ObservableCollection<DEPART_MODEL>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public DEPART_MODEL? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public DEPART_MODEL? WAS_ROW_ITEM { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; } = 0;

        UniversControl universControl = new UniversControl();
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "DEPART", new WindowInteropHelper(this).Handle, this.GetType().Name); //دسترسی
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            ReGetData();
        }
        private void ReGetData()
        {
            DEPART_DATA?.Clear();
            var _DATA_ = dbms.DoGetDataSQL<DEPART_MODEL>("SELECT DEPATMAN, DEPNAME, IDD, DEPART, CRT, UID, PCODE, BBC FROM dbo.DEPART ORDER BY IDD").ToList();
            foreach (var item in _DATA_)
            {
                DEPART_DATA.Add(item);
            }
        }
        private void DEPART_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DEPART_SUB.Dispatcher.InvokeAsync(() =>
            {
                DEPART_SUB.CellEditEnding -= DEPART_SUB_CellEditEnding;
                DEPART_SUB.RowEditEnding -= DEPART_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    DEPART_SUB.CancelEdit();
                }
                else
                {
                    DEPART_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DEPART_SUB.RowEditEnding += DEPART_SUB_RowEditEnding;
                DEPART_SUB.CellEditEnding += DEPART_SUB_CellEditEnding;
            });
        }
        private bool BodyIsValid(DEPART_MODEL _row)
        {
            var ROW = _row;

            var errors = (from object i in DEPART_SUB.ItemsSource
                          let c = DEPART_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.DEPNAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام دپارتمان نمی تواند خالی باشد" });
            }

            bool IsChangingExsiting = ROW?.IDD > 0 && WAS_ROW_ITEM?.DEPATMAN != null && WAS_ROW_ITEM?.DEPATMAN < 20;
            if (IsChangingExsiting)
            {
                if (ROW?.DEPATMAN < 20 && WAS_ROW_ITEM?.DEPNAME != ROW.DEPNAME)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نی توان نام کد رزرو شده را تغییر داد" });
                }
            }
            else
            {
                if (ROW?.DEPATMAN < 20)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "كد بايد بزرگتراز 20 باشد بقيه كدها رزرو است" });
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

        private void DEPART_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && DEPART_SUB.SelectedItem is not null)
            {
                if (DEPART_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((DEPART_MODEL)DEPART_SUB.SelectedItem).Clone() as DEPART_MODEL;
                }
            }
        }
        private void DEPART_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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

            CURRENT_ROW_ITEMS = e.Row.Item as DEPART_MODEL;
            #endregion

            //کد واحد
            if (e.Column.SortMemberPath == "DEPATMAN")
            {
            }

            //نام
            if (e.Column.SortMemberPath == "DEPNAME")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام دپارتمان نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    DEPART_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    CURRENT_ROW_ITEMS.DEPNAME = WAS_ROW_ITEM?.DEPNAME;
                }
            }

            //آدرس
            if (e.Column.SortMemberPath == "DEPART")
            {

            }
        }
        private void DEPART_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as DEPART_MODEL;

            if (!BodyIsValid(ROW))
            {
                DEPART_SUB_CANCEL_EDIT();
                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            double? THECODE = null;

            if (string.IsNullOrEmpty(ROW?.DEPATMAN.ToStringNullSafe()))
            {
                THECODE = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(DEPART.DEPATMAN + 1) FROM dbo.DEPART").FirstOrDefault();
                if (THECODE is null || THECODE.ToStringNullSafe() == "NULL")
                {
                    THECODE = 1;
                }
            }
            else
            {
                THECODE = ROW.DEPATMAN;
            }

            try
            {
                if (ROW?.DEPATMAN is null)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEPART(DEPATMAN, DEPNAME, DEPART,PCODE,BBC)
                                         VALUES({THECODE},
                                         N'{ROW?.DEPNAME}',
                                         N'{ROW?.DEPART}' , N'{ROW?.PCODE}' , N'{ROW?.BBC}' )");

                    ROW.DEPATMAN = (int?)THECODE;
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.DEPART
                                         SET DEPATMAN = {ROW.DEPATMAN}, DEPNAME = N'{ROW.DEPNAME}', DEPART = N'{ROW.DEPART}' , PCODE = N'{ROW.PCODE}', BBC = N'{ROW.BBC}'
                                         WHERE DEPATMAN = {WAS_ROW_ITEM.DEPATMAN}");
                }

                #region Form_AfterUpdate
                if (true) //Format
                {
                    var rst = dbms.DoGetDataSQL<ANBAR_MODEL_QRE1>($"SELECT TOP 1 N_KOL, NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={Baseknow.DARAM} AND NUMBER={ROW.DEPATMAN}").FirstOrDefault();
                    var _N_KOL = Baseknow.DARAM;
                    var _NUMBER = ROW.DEPATMAN;
                    var _NAME = ROW.DEPNAME;
                    var _BED_BES_ = -1;

                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME,BED_BES)
                                             VALUES({_N_KOL}, {_NUMBER}, N'{_NAME}',{_BED_BES_})");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES SET N_KOL = {_N_KOL}, NUMBER = {_NUMBER}, NAME = N'{_NAME}' , BED_BES = N'{_BED_BES_}'  
                                             WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER}");
                    }
                }

                if (true)
                {
                    // تخفيفات درآمد
                    var rst = dbms.DoGetDataSQL<ANBAR_MODEL_QRE1>($"SELECT TOP 1 N_KOL, NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={Baseknow.HDARAM} AND NUMBER={ROW.DEPATMAN}").FirstOrDefault();
                    var _N_KOL = Baseknow.HDARAM;
                    var _NUMBER = ROW.DEPATMAN;
                    var _NAME = ROW.DEPNAME;
                    var _BED_BES_ = -1;

                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME,BED_BES)
                                             VALUES({_N_KOL}, {_NUMBER}, N'{_NAME}',{_BED_BES_})");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES SET N_KOL = {_N_KOL}, NUMBER = {_NUMBER}, NAME = N'{_NAME}' , BED_BES = N'{_BED_BES_}'  
                                             WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER}");
                    }
                }

                if (true)
                {
                    var rst = dbms.DoGetDataSQL<ANBAR_MODEL_QRE1>($"SELECT TOP 1 N_KOL, NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={Baseknow.SANDOGH} AND NUMBER={ROW.DEPATMAN}").FirstOrDefault();
                    var _N_KOL = Baseknow.SANDOGH;
                    var _NUMBER = ROW.DEPATMAN;
                    var _NAME = ROW.DEPNAME;
                    var _BED_BES_ = -1;

                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME,BED_BES)
                                             VALUES({_N_KOL}, {_NUMBER}, N'{_NAME}',{_BED_BES_})");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES SET N_KOL = {_N_KOL}, NUMBER = {_NUMBER}, NAME = N'{_NAME}' , BED_BES = N'{_BED_BES_}'  
                                             WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER}");
                    }
                }

                if (true)
                {
                    //RST2.Open("SHIFT");
                    var RST2 = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID, SHNAME FROM SHIFT").ToList();
                    foreach (var item in RST2) // while (!RST2.EOF())
                    {
                        var _N_KOL_ = Baseknow.SANDOGH;
                        var _NUMBER_ = ROW.DEPATMAN;
                        var _TNUMBER_ = item.SHIFT_ID;
                        var _NAME_ = item.SHNAME;
                        var _BED_BES_ = -1;

                        var rst = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TOP 1 N_KOL, NUMBER,TNUMBER, NAME FROM dbo.TDETA_HES WHERE N_KOL={Baseknow.SANDOGH} AND NUMBER={ROW.DEPATMAN} AND TNUMBER = {item.SHIFT_ID}").FirstOrDefault();
                        if (rst is null)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_},
                                                 {_TNUMBER_},
                                                 N'{_NAME_}',
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_},
                                                 TNUMBER = {_TNUMBER_}, NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                 WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER} AND TNUMBER = {rst.TNUMBER}");
                        }
                    }
                }
                #endregion
            }
            catch (SqlException ex)
            {
                DEPART_SUB_CANCEL_EDIT();
                if (ex.Number == 547)
                {
                    new Msgwin(false, "نام این دپارتمان دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    new Msgwin(false, "نام دپارتمان تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            CL_LMethods.MovingDG(DEPART_SUB, null, CURRENT_ROW_INDEX);
            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void DEPART_SUB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && DEPART_SUB.SelectedItem != null && DEPART_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (DEPART_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = DEPART_SUB.SelectedIndex;
                }
            }
        }
        private void DEPART_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL برای بروز رسانی کارنت رو و کارنت کالمن
                if (!(DEPART_SUB.Items.Count < 1) && !(DEPART_SUB.SelectedItem is null))
                {
                    if (DEPART_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        CURRENT_ROW_INDEX = DEPART_SUB.SelectedIndex;
                    }
                }
            }
        }
        private void DEPART_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (DEPART_SUB.Items.Count > 0 && DEPART_SUB.SelectedItem != null)
                {
                    if (!(DEPART_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف واحد های زیر مجموعه سازمان",
                                    recordId: DEPART_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < DEPART_SUB.SelectedItems.Count; i++)
                            {
                                var item = DEPART_SUB.SelectedItems[i];

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

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.DEPART WHERE IDD = {_idd}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var _DEPNAME_ = item.GetType().GetProperty("DEPNAME").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این دپارتمان ({_DEPNAME_}) دارای گردش است و نمی توان آنرا حذف کرد" });
                                            }
                                            else
                                            {
                                                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog();
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
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

            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                //DEPART_SUB_CANCEL_EDIT();
                DataGridExtension.HandleKeyPress(sender, e, DEPART_SUB);
            }
        }

    }
}
