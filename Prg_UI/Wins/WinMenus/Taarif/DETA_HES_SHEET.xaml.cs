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
using Syncfusion.UI.Xaml.Grid;
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
using System.Windows.Media;
using System.Windows.Threading;

namespace Wins.WinMenus.Taarif
{
    public partial class DETA_HES_SHEET : Window
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
        public DETA_HES_SHEET(string _openargs)
        {
            InitializeComponent();

            N_KOL = _openargs;

            this.DataContext = this;
        }
        public ObservableCollection<DETA_HES> DETA_HES_DATA { get; set; } = new ObservableCollection<DETA_HES>();

        UniversControl universControl = new UniversControl();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool ChangeIsHappend { get; private set; } = false;
        public bool DETA_HES_IsFocused { get; private set; }
        public DETA_HES? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public DETA_HES? WAS_ROW_ITEM { get; private set; }
        public Visual I_AM_DETA_HES_SHEET { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (DETA_HES_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = DETA_HES_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NUMBER")?.DisplayIndex;
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

        public string N_KOL { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_DETA_HES_SHEET = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            ReGetData();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = DETA_HES_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (DETA_HES_IsFocused)
                {
                    try
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                            if (isLastColumn)
                            {
                                if (isLastRow)
                                {
                                    DG.SelectedIndex++;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[NAME_CODE_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    catch { /*ignore*/ }
                }

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
        private void DETA_HES_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                DETA_HES_IsFocused = false;
            }
            else //Is Focus inside of TOTA_HES_SUB_IsFocused
            {
                DETA_HES_IsFocused = true;
            }
        }
        private void DETA_HES_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            if (!BodyIsValid(e.Row.Item as DETA_HES))
            {
                DETA_HES_SUB.CellEditEnding -= DETA_HES_SUB_CellEditEnding;
                DETA_HES_SUB.RowEditEnding -= DETA_HES_SUB_RowEditEnding;

                e.Cancel = true;
                DETA_HES_SUB.CancelEdit(DataGridEditingUnit.Cell);

                DETA_HES_SUB.RowEditEnding += DETA_HES_SUB_RowEditEnding;
                DETA_HES_SUB.CellEditEnding += DETA_HES_SUB_CellEditEnding;

                return;
            }

            var ROW = e.Row.Item as DETA_HES;

            int? id = null;
            try
            {
                if (ROW?.ID is null) //INSERT
                {
                    id = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, TOZIH, USERCO, USER_NAME)
                                                    OUTPUT INSERTED.ID
                                                    VALUES({N_KOL},
                                                    {ROW.NUMBER} ,
                                                    N'{ROW.NAME.FixPersianChars().Trim()}' ,
                                                    N'{ROW.TOZIH.FixPersianChars()}' ,
                                                    {Baseknow.USERCOD} ,
                                                    N'{CL_HESABDARI.UCurrentUser()}')").FirstOrDefault();
                }
                else //UPDATE
                {
                    ESLAH_ROW(ROW.NUMBER);

                    dbms.DoExecuteSQL(@$" UPDATE dbo.DETA_HES
                                          SET NUMBER = {ROW.NUMBER}, NAME = N'{ROW.NAME.FixPersianChars().Trim()}', TOZIH = N'{ROW.TOZIH.FixPersianChars()}'
                                          WHERE ID = {ROW.ID} ");
                }

                Form_AfterUpdate((double)ROW.NUMBER, (double)WAS_ROW_ITEM.NUMBER);
            }
            catch (SqlException ex)
            {
                DETA_HES_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد حساب تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }

                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            if (id != null) //So Much Important
            {
                ROW.ID = id;
            }

            ROW.N_KOL = Convert.ToInt32(N_KOL);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void DETA_HES_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && DETA_HES_SUB.SelectedItem is not null)
            {
                if (DETA_HES_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((DETA_HES)DETA_HES_SUB.SelectedItem).Clone() as DETA_HES;
                }
            }
        }
        private void DETA_HES_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS_
            CURRENT_ROW_ITEMS = e.Row.Item as DETA_HES;
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
            else
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();
            #endregion

            if (e.Column.SortMemberPath == "NUMBER") //کد حساب
            {
                bool anyerror = false;
                int parsedValue;
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("کد حساب نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }
                else if (!int.TryParse(ENTERED_VALUE_ROW?.ToStringNullSafe(), out parsedValue))
                {
                    universControl.PopNotifyShow("کد وارد شده در محدوده مجاز نیست !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }
                else if (parsedValue <= 0)
                {
                    universControl.PopNotifyShow("کد حساب نمی تواند صفر یا منفی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }
                if (anyerror)
                {
                    CURRENT_ROW_ITEMS.NUMBER = WAS_ROW_ITEM?.NUMBER;
                    DETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "NAME") //نام حساب
            {
                bool anyerror = false;
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام حساب نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }

                if (anyerror)
                {
                    CURRENT_ROW_ITEMS.NAME = WAS_ROW_ITEM?.NAME;
                    DETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }
        }
        private void DETA_HES_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (DETA_HES_SUB.Items.Count > 0 && DETA_HES_SUB.SelectedItem != null)
                {
                    IEditableCollectionView itemsView = DETA_HES_SUB.Items as IEditableCollectionView;
                    if (!itemsView.IsAddingNew && !itemsView.IsEditingItem)
                    {
                        if (!(DETA_HES_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {
                                _ = AuditLogger.LogActionAsync(
                                        actionType: "DELETE",
                                        tableName: "تعريف سرفصل حسابهاي معين",
                                        recordId: DETA_HES_SUB.SelectedItem.ToStringNullSafe(),
                                        oldValue: null,
                                        newValue: null,
                                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                                for (int i = 0; i < DETA_HES_SUB.SelectedItems.Count; i++)
                                {
                                    var item = DETA_HES_SUB.SelectedItems[i];

                                    if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                    {
                                        if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                        {
                                        }
                                        else
                                        {
                                            var _id = item.GetType().GetProperty("ID").GetValue(item);
                                            var _NUMBER = item.GetType().GetProperty("NUMBER").GetValue(item);

                                            ESLAH_ROW((int?)_NUMBER);

                                            try
                                            {
                                                var tafChildCount = dbms.DoGetDataSQL<int>($@"SELECT COUNT(*) FROM dbo.TDETA_HES WHERE N_KOL = {N_KOL} AND NUMBER = {_NUMBER}").FirstOrDefault();
                                                var gerdeshCount = dbms.DoGetDataSQL<int>($@"SELECT COUNT(*) FROM dbo.DEED_DTL WHERE HES_K = {N_KOL} AND HES_M = {_NUMBER}").FirstOrDefault();
                                                if (tafChildCount > 0 || gerdeshCount > 0)
                                                {
                                                    e.Handled = true;
                                                    if (tafChildCount > 0)
                                                        ErrosMessages.Add(new MsgModel { MessageText_U = $"این حساب دارای {tafChildCount} زیرحساب تفضیلی است - ابتدا زیرحساب‌ها را حذف کنید." });
                                                    if (gerdeshCount > 0)
                                                    {
                                                        var snadNums = string.Join("، ", dbms.DoGetDataSQL<double>($@"SELECT DISTINCT TOP 5 N_S FROM dbo.DEED_DTL WHERE HES_K = {N_KOL} AND HES_M = {_NUMBER} ORDER BY N_S").Select(s => ((long)s).ToString()));
                                                        string moreTxt = gerdeshCount > 5 ? " و ..." : "";
                                                        ErrosMessages.Add(new MsgModel { MessageText_U = $"این حساب در {gerdeshCount} ردیف از اسناد حسابداری استفاده شده است (شماره سند: {snadNums}{moreTxt}) و نمیتوان آنرا حذف کرد!" });
                                                    }
                                                }
                                                else
                                                {
                                                    dbms.DoExecuteSQL($@"DELETE FROM dbo.DETA_HES WHERE ID = {_id}");
                                                    IsDeletedSomething = true;
                                                }
                                            }
                                            catch (SqlException ex)
                                            {
                                                if (ex.Number == 547)
                                                {
                                                    e.Handled = true;

                                                    ErrosMessages.Add(new MsgModel { MessageText_U = $"این حساب دارای گردش است و نمیتوان آنرا حذف کرد!" });
                                                }
                                                else
                                                {
                                                    ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
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
                                ReGetData(true);
                                universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                            }
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }

            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                DataGridExtension.HandleKeyPress(sender, e, DETA_HES_SUB);
            }
        }
        private void DETA_HES_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //NUMBER_DblClick
            //DoCmd.OpenForm("TDETA_HES_SHEET", acFormDS, default, "N_KOL = " + this.N_KOL + " AND NUMBER = " + this.NUMBER);
        }


        private ICollectionView DataViewPal;
        private void ReGetData(bool GOTOLAST = false)
        {
            DETA_HES_DATA?.Clear();

            var RST = dbms.DoGetDataSQL<DETA_HES>($@"SELECT ID,N_KOL, NUMBER, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, USERCO, USER_NAME, CRT, UID FROM 
                                                     dbo.DETA_HES WHERE N_KOL = {N_KOL}  ORDER BY NUMBER").ToList();
            foreach (var item in RST)
            {
                DETA_HES_DATA.Add(item);
            }

            var _DataGrid_ = DETA_HES_SUB;
            string _SORTPATH_ = "NUMBER";
            int lastindexrow = _DataGrid_.Items.Count - 1;

            //if (GOTOLAST)
            //{
            //    CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
            //}
            //else
            //{
            //    lastindexrow = _DataGrid_.Items.IndexOf(_DataGrid_?.CurrentItem);
            //    if (lastindexrow > 0)
            //    {
            //        CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, lastindexrow, false);
            //    }
            //    else
            //    {
            //        CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
            //    }
            //}

            DataViewPal = CollectionViewSource.GetDefaultView(DETA_HES_DATA);
            DETA_HES_SUB.ItemsSource = DataViewPal;
        }
        private void ApplyDataGridItems()
        {
            try
            {
                if (DETA_HES_SUB.Items is IEditableCollectionView editableCollectionView)
                {
                    if (editableCollectionView.IsAddingNew)
                    {
                        editableCollectionView.CancelNew(); // discard the new item
                    }
                    if (editableCollectionView.IsEditingItem)
                    {
                        editableCollectionView.CommitEdit(); // commit the edit transaction
                    }
                }
            }
            catch { }
        }
        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyDataGridItems();
            string query = SearchText.Text?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                DataViewPal.Filter = null;
            }
            else
            {
                DataViewPal.Filter = obj =>
                {
                    if (obj is DETA_HES model)
                    {
                        return !string.IsNullOrEmpty(model.NAME) && model.NAME.ToLower().Contains(query);
                    }
                    return false;
                };
            }
            DataViewPal.Refresh();
        }
        private void DETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DETA_HES_SUB.Dispatcher.InvokeAsync(() =>
            {
                DETA_HES_SUB.CellEditEnding -= DETA_HES_SUB_CellEditEnding;
                DETA_HES_SUB.RowEditEnding -= DETA_HES_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    DETA_HES_SUB.CancelEdit();
                }
                else
                {
                    DETA_HES_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DETA_HES_SUB.RowEditEnding += DETA_HES_SUB_RowEditEnding;
                DETA_HES_SUB.CellEditEnding += DETA_HES_SUB_CellEditEnding;
            });
        }
        private bool BodyIsValid(DETA_HES _row)
        {
            var ROW = _row;

            var errors = (from object i in DETA_HES_SUB.ItemsSource
                          let c = DETA_HES_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.NUMBER.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند خالی باشد" });
            }
            else if (ROW?.NUMBER <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند صفر یا منفی باشد" });
            }
            else if (!int.TryParse(ROW?.NUMBER.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد وارد شده در محدوده مجاز نیست" });
            }

            if (string.IsNullOrEmpty(ROW?.NAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام حساب نمی تواند خالی باشد" });
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
        private void ESLAH_ROW(int? NUMBER)
        {
            //NAME_DblClick
            if (NUMBER is not null)
            {
                var dt = DateTime.Now;
                //if ((bool)Baseknow.TRANSF) {}
                CL_HESABDARI.TR("DETA_HES", "(N_KOL = " + N_KOL + " ) AND (NUMBER = " + NUMBER + " )", dt, 1);
            }
        }
        private void Form_AfterUpdate(double NUMBER, double NUMBER_TAG)
        {
            //Form_AfterUpdate
            if (NUMBER != NUMBER_TAG)
            {
                // سطح 2 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_MOIN2 = " + NUMBER + " WHERE  (N_KOL2 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER_TAG + ")");
                // سطح 3 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_MOIN3 = " + NUMBER + " WHERE  (N_KOL3 = " + N_KOL + ") AND (N_MOIN3 = " + NUMBER_TAG + ")");
                // سطح 2 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_MOIN2 = " + NUMBER + " WHERE  (N_KOL2 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER_TAG + ")");
                // سطح 3 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_MOIN3 = " + NUMBER + " WHERE  (N_KOL3 = " + N_KOL + ") AND (N_MOIN3 = " + NUMBER_TAG + ")");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_M = " + NUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-' + CAST(THES_T AS NVARCHAR)   WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER_TAG + " ) AND  (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_M = " + NUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_M = " + NUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_M = " + NUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR) + '-' + CAST(THES_T4 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND NOT (THES_T4 IS NULL)");
                // سطح 1 دريافت و پرداخت درطرف بستانكار دريافت پرداخت حساب تفصيلي به صورت خودكارآبديت مي شود
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-' + CAST(FHES_T AS NVARCHAR)   WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR) + '-' + CAST(FHES_T4 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND NOT (FHES_T4 IS NULL)");
                // درفاكتورها
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  CUST_NO = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)  WHERE dbo.HEAD_LST.CUST_NO = '" + N_KOL + "-" + NUMBER_TAG + "-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL))  AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (dbo.GETTAF3(CUST_NO) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)+ '-' + dbo.GETTAF4(CUST_NO) WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (NOT (dbo.GETTAF4(CUST_NO) IS NULL)) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                // MOIN_VAR
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_VAR = '" + N_KOL + "-" + NUMBER_TAG + "-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL))  AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (dbo.GETTAF3(MOIN_VAR) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)+ '-' + dbo.GETTAF4(MOIN_VAR) WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_VAR) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                // MOIN_HAV
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAV = '" + N_KOL + "-" + NUMBER_TAG + "-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (dbo.GETTAF3(MOIN_HAV) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)+ '-' + dbo.GETTAF4(MOIN_HAV) WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAV) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                // MOIN_HAZ
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAZ = '" + N_KOL + "-" + NUMBER_TAG + "-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (dbo.GETTAF3(MOIN_HAZ) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)+ '-' + dbo.GETTAF4(MOIN_HAZ) WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAZ) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                // HMBAA
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  HMBAA = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)  WHERE dbo.HEAD_LST.HMBAA = '" + N_KOL + "-" + NUMBER_TAG + "-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(HMBAA) IS NULL))  AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (dbo.GETTAF3(HMBAA) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)+ '-' + dbo.GETTAF4(HMBAA) WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (NOT (dbo.GETTAF4(HMBAA) IS NULL)) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                // در اسناد حسابداري
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-' + CAST(HES_T AS NVARCHAR)  WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND   (HES_T2 IS NULL) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-' + CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " )  AND  (NOT (HES_T2 IS NULL)) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-' + CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-' + CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR)+ '-' + CAST(HES_T4 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (NOT (HES_T4 IS NULL))");
            }
        }
        private void SubSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as DETA_HES)?.ID is not null)
                    {
                        var Row = btn.Tag as DETA_HES;
                        if (Row != null && Row?.ID > 0)
                        {
                            if (Row?.NUMBER != null)
                            {
                                new TDETA_HES_SHEET((int)Row.N_KOL, (int)Row.NUMBER).Show(); //تفضیلی
                            }
                        }
                    }
                }
            }
        }

        private void DETA_HES_SUB_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
        }
        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null) return;

            try
            {
                // اطمینان از فوکوس بودن DataGrid قبل از باز کردن Context Menu
                if (!dataGrid.IsKeyboardFocusWithin)
                {
                    dataGrid.Focus();
                }

                // Find the row under the mouse
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;
                if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
                {

                    if (dataGrid?.SelectedItems.Count <= 1)
                    {
                        // Select the row under the mouse
                        dataGrid.SelectedItem = row.Item;
                    }

                    // تنظیم فوکوس روی سطر
                    row.Focus();

                    // Show the context menu
                    dataGrid.ContextMenu.IsOpen = true;

                    // Mark the event as handled to prevent the default context menu behavior
                    e.Handled = true;
                }
                else
                {
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            if (!DETA_HES_DATA.Any())
            {
                return;
            }

            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(DETA_HES_SUB, "DGExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            // پیدا کردن سطری که روی آن کلیک شده
            var row = CL_LMethods.FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);

            if (row != null)
            {
                // انتخاب سطر
                if (!row.IsSelected)
                {
                    row.IsSelected = true;
                    dataGrid.SelectedItem = row.Item;
                }

                // تنظیم فوکوس روی سطر و DataGrid
                row.Focus();

                // اطمینان از اینکه DataGrid خودش هم فوکوس دارد
                dataGrid.Focus();

                // اگر سلول خاصی زیر موس است، آن را هم فوکوس کنیم
                var cell = CL_LMethods.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                if (cell != null)
                {
                    cell.Focus();
                }
            }
            else
            {
                // اگر روی header یا جای دیگری کلیک شد، حداقل DataGrid را فوکوس کنیم
                dataGrid.Focus();
            }
        }
        private void DG_SUB_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid? dg = sender as DataGrid;
            if (dg == null) return;

            // اطمینان از فوکوس بودن DataGrid
            if (!dg.IsKeyboardFocusWithin)
            {
                dg.Focus();
            }

            if (dg.CurrentItem == null || dg.CurrentItem == CollectionView.NewItemPlaceholder)
            {
                e.Handled = true; // Cancel opening the menu. Avoids the crash.
                return;
            }
            if (dg?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            else if (dg?.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            base.OnContextMenuOpening(e);
        }
    }
}
