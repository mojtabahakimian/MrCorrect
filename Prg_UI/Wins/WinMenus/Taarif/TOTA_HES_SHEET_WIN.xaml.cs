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
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wins.WinMenus.Taarif
{
    public partial class TOTA_HES_SHEET_WIN : Window
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

        #region MODELS
        public class MODEL_HESY1
        {
            public double? CODE { get; set; }
            public string? NAMES { get; set; }
        }
        #endregion

        public string OpenArgs { get; set; }
        public TOTA_HES_SHEET_WIN(string openArgs = "")
        {
            InitializeComponent();

            this.DataContext = this;

            OpenArgs = openArgs;
        }
        public ObservableCollection<TOTA_HES> TOTA_HES_DATA { get; set; } = new ObservableCollection<TOTA_HES>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public TOTA_HES? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TOTA_HES? WAS_ROW_ITEM { get; private set; }
        public Visual I_AM_TOTA_HES_SHEET { get; set; }

        UniversControl universControl = new UniversControl();
        public bool ChangeIsHappend { get; private set; } = false;
        public bool TOTA_HES_SUB_IsFocused { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (TOTA_HES_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TOTA_HES_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NUMBER")?.DisplayIndex;
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
        public string ServerFilter { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_TOTA_HES_SHEET = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            #region Form_KeyPress
            if (OpenArgs == "1")
            {
                //DoCmd.OpenForm("DETA_HES_SHEET", acFormDS, default, "N_KOL = " + this.NUMBER, acFormReadOnly, default, "1");
            }
            #endregion

            FILL_ALL_COMBOBOXES();

            ReGetData();

            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };

            #region Form_Load
            var SH = default(string);
            var RST2 = dbms.DoGetDataSQL<BLOCK_HES>("SELECT USERCO,HES FROM BLOCK_HES WHERE USERCO = " + Baseknow.USERCOD).ToList();
            if (RST2.Count > 0)
            {
                if (!CL_HESABDARI.ISHESAB3(RST2.FirstOrDefault().HES))
                {
                    SH = "NUMBER NOT LIKE '" + RST2.FirstOrDefault().HES + "'";
                }
                //RST2.MoveNext();
                for (int i = 0; i < RST2.Count; i++) //while (!RST2.EOF)
                {
                    if (!CL_HESABDARI.ISHESAB3(RST2[i].HES))
                    {
                        SH = SH + " AND NUMBER NOT LIKE '" + RST2[i].HES + "'";
                    }
                    //RST2.MoveNext();
                }
            }
            if (!string.IsNullOrEmpty(SH))
            {
                this.ServerFilter = SH;
            }
            #endregion
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = TOTA_HES_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (uie is DataGridCell || TOTA_HES_SUB_IsFocused)
                {
                    if (DG.CurrentColumn != null)
                    {
                        int DefaultColumnIndex = CL_LMethods.GetLastColumn(TOTA_HES_SUB).DisplayIndex;
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
                                    DG.BeginEdit();
                                }), DispatcherPriority.Background);

                                return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                            }
                        }
                    }
                }

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
        private void Form_Current()
        {
            //if (this.NewRecord)
            //{
            //    this.NUMBER.TAG = "";
            //}
            //else
            //{
            //    this.NUMBER.TAG = this.NUMBER;
            //}
            //if (this.OpenArgs == "1")
            //{
            //    DoCmd.RunCommand(acCmdSelectRecord);
            //}
            //if (!IsNull(this.NUMBER))
            //{
            //    this.AllowDeletions = false;
            //    this.AllowEdits = false;
            //}
            //else
            //{
            //    this.AllowDeletions = true;
            //    this.AllowEdits = true;
            //}
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع حساب
            NO_HES_COLUMN.ItemsSource = dbms.DoGetDataSQL<MODEL_HESY1>("SELECT TCOD_HESKIND.CODE, TCOD_HESKIND.NAMES FROM dbo.TCOD_HESKIND GROUP BY TCOD_HESKIND.CODE, TCOD_HESKIND.NAMES ORDER BY TCOD_HESKIND.NAMES").ToList();
            //وضعیت
            M_D_COLUMN.ItemsSource = dbms.DoGetDataSQL<MODEL_HESY1>("SELECT TCOD_HESVAZ.CODE, TCOD_HESVAZ.NAMES FROM dbo.TCOD_HESVAZ GROUP BY TCOD_HESVAZ.CODE, TCOD_HESVAZ.NAMES ORDER BY TCOD_HESVAZ.NAMES").ToList();
            //گروه حساب
            GROUP_COLUMN.ItemsSource = dbms.DoGetDataSQL<MODEL_HESY1>("SELECT TCOD_HESGROUP.CODE, TCOD_HESGROUP.NAMES FROM dbo.TCOD_HESGROUP GROUP BY TCOD_HESGROUP.CODE, TCOD_HESGROUP.NAMES ORDER BY TCOD_HESGROUP.NAMES").ToList();
        }

        private ICollectionView DataViewPal;
        private void ReGetData(bool GOTOLAST = false)
        {
            TOTA_HES_DATA?.Clear();
            string Qry = "SELECT ID,NUMBER, NAME, NO_HES, M_D, [GROUP], CRT, UID FROM dbo.TOTA_HES";
            if (!string.IsNullOrEmpty(ServerFilter))
            {
                Qry = Qry + " WHERE " + ServerFilter;
            }
            var RST = dbms.DoGetDataSQL<TOTA_HES>(Qry + "  ORDER BY NUMBER ").ToList();
            foreach (var item in RST)
            {
                TOTA_HES_DATA.Add(item);
            }


            //if (GOTOLAST)
            //{
            //    CL_LMethods.FocusCellReadyToEdit(TOTA_HES_SUB, "NUMBER", TOTA_HES_SUB.Items.Count - 1, false);
            //}
            //else
            //{
            //    int lastindexrow = 0;
            //    lastindexrow = TOTA_HES_SUB.Items.IndexOf(TOTA_HES_SUB?.CurrentItem);
            //    if (lastindexrow > 0)
            //    {
            //        CL_LMethods.FocusCellReadyToEdit(TOTA_HES_SUB, "NUMBER", lastindexrow, false);
            //    }
            //    else
            //    {
            //        CL_LMethods.FocusCellReadyToEdit(TOTA_HES_SUB, "NUMBER", TOTA_HES_SUB.Items.Count - 1, false);
            //    }
            //}

            DataViewPal = CollectionViewSource.GetDefaultView(TOTA_HES_DATA);
            TOTA_HES_SUB.ItemsSource = DataViewPal;
        }
        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchText.Text?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                DataViewPal.Filter = null;
            }
            else
            {
                DataViewPal.Filter = obj =>
                {
                    if (obj is TOTA_HES model)
                    {
                        return !string.IsNullOrEmpty(model.NAME) && model.NAME.ToLower().Contains(query);
                    }
                    return false;
                };
            }

            DataViewPal.Refresh();
        }

        private void TOTA_HES_SUB_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TOTA_HES_SUB.SelectedItem is not null)
            {
                if (TOTA_HES_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TOTA_HES)TOTA_HES_SUB.SelectedItem).Clone() as TOTA_HES;
                }
                var editableCollectionView = TOTA_HES_SUB.Items as IEditableCollectionView;
                if (!editableCollectionView.IsAddingNew)
                {
                    //TOTA_HES? item = e.Row.Item as TOTA_HES;
                    //if (item != null && !item.IsRowEditable) //if is not editable cancel edit
                    //{
                    //    e.Cancel = true;
                    //    TOTA_HES_SUB_CANCEL_EDIT();
                    //}
                    //else
                    {
                    }
                }
            }
        }
        private void TOTA_HES_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TOTA_HES_SUB.Items.Count > 0 && TOTA_HES_SUB.SelectedItem != null)
                {
                    IEditableCollectionView itemsView = TOTA_HES_SUB.Items as IEditableCollectionView;
                    if (!itemsView.IsAddingNew && !itemsView.IsEditingItem)
                    {
                        if (!(TOTA_HES_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {

                                _ = AuditLogger.LogActionAsync(
                                        actionType: "DELETE",
                                        tableName: "سرفصل حسابهاي كل",
                                        recordId: TOTA_HES_SUB.SelectedItem.ToStringNullSafe(),
                                        oldValue: null,
                                        newValue: null,
                                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                                for (int i = 0; i < TOTA_HES_SUB.SelectedItems.Count; i++)
                                {
                                    var item = TOTA_HES_SUB.SelectedItems[i];

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
                                                IsDeletedSomething = true;

                                                dbms.DoExecuteSQL($@"DELETE FROM dbo.TOTA_HES WHERE ID = {_id}");
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
                DataGridExtension.HandleKeyPress(sender, e, TOTA_HES_SUB);
            }

            //if (e.Key == Key.Escape)
            //{
            //    TOTA_HES_SUB_CANCEL_EDIT();
            //}
        }
        private void TOTA_HES_SUB_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS_
            CURRENT_ROW_ITEMS = e.Row.Item as TOTA_HES;
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

            CURRENT_ROW_ITEMS = e.Row.Item as TOTA_HES;
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
                    TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                    TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "NO_HES") //نوع حساب
            {
                bool anyerror = false;
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نوع حساب نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }

                if (anyerror)
                {
                    CURRENT_ROW_ITEMS.NO_HES = WAS_ROW_ITEM?.NO_HES;
                    TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "M_D") //وضعیت
            {
                bool anyerror = false;
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("وضعیت نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }

                if (anyerror)
                {
                    CURRENT_ROW_ITEMS.M_D = WAS_ROW_ITEM?.M_D;
                    TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "GROUP")
            {
                bool anyerror = false;
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("گروه حساب نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    anyerror = true;
                }

                if (anyerror)
                {
                    CURRENT_ROW_ITEMS.GROUP = WAS_ROW_ITEM?.GROUP;
                    TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }
        }
        private void TOTA_HES_SUB_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (!BodyIsValid(e.Row.Item as TOTA_HES))
            {
                TOTA_HES_SUB.CellEditEnding -= TOTA_HES_SUB_CellEditEnding;
                TOTA_HES_SUB.RowEditEnding -= TOTA_HES_SUB_RowEditEnding;

                e.Cancel = true;
                TOTA_HES_SUB.CancelEdit(DataGridEditingUnit.Cell);

                TOTA_HES_SUB.RowEditEnding += TOTA_HES_SUB_RowEditEnding;
                TOTA_HES_SUB.CellEditEnding += TOTA_HES_SUB_CellEditEnding;
                return;
            }

            var ROW = e.Row.Item as TOTA_HES;

            int? id = null;
            try
            {
                if (ROW?.ID is null) //INSERT
                {
                    id = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.TOTA_HES (NUMBER, NAME, NO_HES, M_D, [GROUP])
                                                    OUTPUT INSERTED.ID
                                                    VALUES ({ROW.NUMBER}, N'{ROW.NAME.FixPersianChars().Trim()}', {ROW.NO_HES}, {ROW.M_D}, {ROW.GROUP})").FirstOrDefault();
                }
                else //UPDATE
                {
                    ESLAH_ROW(ROW.NUMBER);

                    dbms.DoExecuteSQL(@$" UPDATE dbo.TOTA_HES
                                          SET NUMBER = {ROW.NUMBER}, NAME = N'{ROW.NAME.FixPersianChars().Trim()}', NO_HES = {ROW.NO_HES}, M_D = {ROW.M_D}, [GROUP] = {ROW.GROUP}  WHERE ID = {ROW.ID} ");
                }

                Form_AfterUpdate((double)ROW.NUMBER, (double)WAS_ROW_ITEM.NUMBER);
            }
            catch (SqlException ex)
            {
                TOTA_HES_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد حساب تکراری است آنرا اصلاح کنید").ShowDialog();
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

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void TOTA_HES_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (TOTA_HES_SUB.IsEnabled == true)
            //{
            //    var grid = sender as DataGrid;
            //    if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null && TOTA_HES_SUB.SelectedIndex > -1)
            //    {
            //        var CurrentData = TOTA_HES_SUB.Items[TOTA_HES_SUB.SelectedIndex] as TOTA_HES;
            //        if (grid.CurrentCell.Column.SortMemberPath == "NAME")
            //        {
            //            if (CurrentData != null)
            //            {
            //                if (CurrentData.NUMBER != null)
            //                {
            //                    ESLAH_ROW(CurrentData.NUMBER);
            //                }
            //            }
            //        }
            //    }
            //}
        }
        private void TOTA_HES_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TOTA_HES_SUB_IsFocused = false;
            }
            else //Is Focus inside of TOTA_HES_SUB_IsFocused
            {
                TOTA_HES_SUB_IsFocused = true;
            }
        }
        private void TOTA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TOTA_HES_SUB.Dispatcher.InvokeAsync(() =>
            {
                TOTA_HES_SUB.CellEditEnding -= TOTA_HES_SUB_CellEditEnding;
                TOTA_HES_SUB.RowEditEnding -= TOTA_HES_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TOTA_HES_SUB.CancelEdit();
                }
                else
                {
                    TOTA_HES_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TOTA_HES_SUB.RowEditEnding += TOTA_HES_SUB_RowEditEnding;
                TOTA_HES_SUB.CellEditEnding += TOTA_HES_SUB_CellEditEnding;
            });
        }

        private bool BodyIsValid(TOTA_HES _row)
        {
            var ROW = _row;

            var errors = (from object i in TOTA_HES_SUB.ItemsSource
                          let c = TOTA_HES_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
            if (string.IsNullOrEmpty(ROW?.NO_HES.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع حساب نمی تواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(ROW?.M_D.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت نمی تواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(ROW?.GROUP.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه حساب نمی تواند خالی باشد" });
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
        private void Form_AfterUpdate(double NUMBER, double NUMBER_TAG)
        {
            //Form_AfterUpdate
            if (NUMBER != NUMBER_TAG)
            {
                // سطح 2 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_kol2 = " + NUMBER + " WHERE  (N_KOL2 = " + NUMBER_TAG + ")");
                // سطح 3 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_KOL3 = " + NUMBER + " WHERE  (N_KOL3 = " + NUMBER_TAG + ")");
                // سطح 2 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_KOL2 = " + NUMBER + " WHERE  (N_KOL2 = " + NUMBER_TAG + ")");
                // سطح 3 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_KOL3 = " + NUMBER + " WHERE  (N_KOL3 = " + NUMBER_TAG + ")");

                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR)   WHERE (THES_K = " + NUMBER_TAG + " ) AND  (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR) + '-' + CAST(THES_T4 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND NOT (THES_T4 IS NULL)");
                // سطح 1 دريافت و پرداخت درطرف بستانكار دريافت پرداخت حساب تفصيلي به صورت خودكارآبديت مي شود
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR)   WHERE (FHES_K = " + NUMBER + " ) AND  (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR) + '-' + CAST(FHES_T4 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND NOT (FHES_T4 IS NULL)");
                // درفاكتورها
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)  WHERE dbo.HEAD_LST.CUST_NO = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL))  AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (dbo.GETTAF3(CUST_NO) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)+ '-' + dbo.GETTAF4(CUST_NO) WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (NOT (dbo.GETTAF4(CUST_NO) IS NULL)) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                // MOIN_VAR
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_VAR = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL))  AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (dbo.GETTAF3(MOIN_VAR) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)+ '-' + dbo.GETTAF4(MOIN_VAR) WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_VAR) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                // MOIN_HAV
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAV = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (dbo.GETTAF3(MOIN_HAV) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)+ '-' + dbo.GETTAF4(MOIN_HAV) WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAV) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                // MOIN_HAZ
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAZ = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (dbo.GETTAF3(MOIN_HAZ) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)+ '-' + dbo.GETTAF4(MOIN_HAZ) WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAZ) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                // HMBAA
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)  WHERE dbo.HEAD_LST.HMBAA = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(HMBAA) IS NULL))  AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (dbo.GETTAF3(HMBAA) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)+ '-' + dbo.GETTAF4(HMBAA) WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (NOT (dbo.GETTAF4(HMBAA) IS NULL)) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                // در اسناد حسابداري
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  WHERE (HES_K = " + NUMBER + " ) AND   (HES_T2 IS NULL) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND (NOT (HES_T3 IS NULL)) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR)+ '-' + CAST(HES_T4 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (NOT (HES_T4 IS NULL))");

                #region ON_ERROR_RESUME_NEXT
                //// سطح 2 دريافت چك
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_kol2 = " + NUMBER + " WHERE  (N_KOL2 = " + NUMBER_TAG + ")"); } catch { }
                //// سطح 3 دريافت چك
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_KOL3 = " + NUMBER + " WHERE  (N_KOL3 = " + NUMBER_TAG + ")"); } catch { }
                //// سطح 2 پرداخت چك
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_KOL2 = " + NUMBER + " WHERE  (N_KOL2 = " + NUMBER_TAG + ")"); } catch { }
                //// سطح 3 پرداخت چك
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_KOL3 = " + NUMBER + " WHERE  (N_KOL3 = " + NUMBER_TAG + ")"); } catch { }

                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR)   WHERE (THES_K = " + NUMBER_TAG + " ) AND  (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND (THES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_K = " + NUMBER + " , THES = '" + NUMBER + "-' + CAST(THES_M AS NVARCHAR) + '-' + CAST(THES_T AS NVARCHAR) + '-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR) + '-' + CAST(THES_T4 AS NVARCHAR)  WHERE (THES_K = " + NUMBER_TAG + " ) AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND NOT (THES_T4 IS NULL)"); } catch { }
                //// سطح 1 دريافت و پرداخت درطرف بستانكار دريافت پرداخت حساب تفصيلي به صورت خودكارآبديت مي شود
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR)   WHERE (FHES_K = " + NUMBER + " ) AND  (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + NUMBER + "-' + CAST(FHES_M AS NVARCHAR) + '-' + CAST(FHES_T AS NVARCHAR) + '-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR) + '-' + CAST(FHES_T4 AS NVARCHAR)  WHERE (FHES_K = " + NUMBER + " ) AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND NOT (FHES_T4 IS NULL)"); } catch { }
                //// درفاكتورها
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)  WHERE dbo.HEAD_LST.CUST_NO = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL))  AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (dbo.GETTAF3(CUST_NO) IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + NUMBER + "-' + CAST(dbo.GETMOIN(CUST_NO) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(CUST_NO) AS NVARCHAR) + '-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)+ '-' + dbo.GETTAF4(CUST_NO) WHERE     (dbo.GETKOL(CUST_NO) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (NOT (dbo.GETTAF4(CUST_NO) IS NULL)) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))"); } catch { }
                //// MOIN_VAR
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_VAR = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL))  AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (dbo.GETTAF3(MOIN_VAR) IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_VAR) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_VAR) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)+ '-' + dbo.GETTAF4(MOIN_VAR) WHERE     (dbo.GETKOL(MOIN_VAR) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_VAR) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))"); } catch { }
                //// MOIN_HAV
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAV = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (dbo.GETTAF3(MOIN_HAV) IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAV) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAV) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)+ '-' + dbo.GETTAF4(MOIN_HAV) WHERE     (dbo.GETKOL(MOIN_HAV) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAV) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))"); } catch { }
                //// MOIN_HAZ
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)  WHERE dbo.HEAD_LST.MOIN_HAZ = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (dbo.GETTAF3(MOIN_HAZ) IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + NUMBER + "-' + CAST(dbo.GETMOIN(MOIN_HAZ) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(MOIN_HAZ) AS NVARCHAR) + '-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)+ '-' + dbo.GETTAF4(MOIN_HAZ) WHERE     (dbo.GETKOL(MOIN_HAZ) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAZ) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))"); } catch { }
                //// HMBAA
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)  WHERE dbo.HEAD_LST.HMBAA = '" + NUMBER_TAG + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-'+ CAST(dbo.GETTAF(HMBAA) AS NVARCHAR)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(HMBAA) IS NULL))  AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (dbo.GETTAF3(HMBAA) IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ")  AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + NUMBER + "-' + CAST(dbo.GETMOIN(HMBAA) AS NVARCHAR) + '-' + CAST(dbo.GETTAF(HMBAA) AS NVARCHAR) + '-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)+ '-' + dbo.GETTAF4(HMBAA) WHERE     (dbo.GETKOL(HMBAA) = " + NUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (NOT (dbo.GETTAF4(HMBAA) IS NULL)) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))"); } catch { }
                //// در اسناد حسابداري
                //try { dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  WHERE (HES_K = " + NUMBER + " ) AND   (HES_T2 IS NULL) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND (NOT (HES_T3 IS NULL)) AND (HES_T4 IS NULL)"); } catch { }
                //try { dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + NUMBER + "-' + CAST(HES_M AS NVARCHAR) + '-' +  CAST(HES_T AS NVARCHAR)  + '-' + CAST(HES_T2 AS NVARCHAR) + '-' + CAST(HES_T3 AS NVARCHAR)+ '-' + CAST(HES_T4 AS NVARCHAR) WHERE (HES_K = " + NUMBER + " ) AND (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (NOT (HES_T4 IS NULL))"); } catch { }
                #endregion
            }
        }
        private void ESLAH_ROW(int? NUMBER)
        {
            //NAME_DblClick
            if (NUMBER is not null)
            {
                var dt = DateTime.Now;
                //if ((bool)Baseknow.TRANSF) {}
                CL_HESABDARI.TR("TOTA_HES", "(NUMBER = " + NUMBER + " )", dt, 1);
            }
        }
        private void CODE_CC_Click(object sender, RoutedEventArgs e)
        {
            //NUMBER_DblClick
            //DoCmd.OpenForm("DETA_HES_SHEET", acFormDS, default, "N_KOL = " + this.NUMBER);
        }
        private void SubSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as TOTA_HES)?.ID is not null)
                    {
                        var Row = btn.Tag as TOTA_HES;
                        if (Row != null && Row?.ID > 0)
                        {
                            if (Row?.NUMBER != null)
                            {
                                new DETA_HES_SHEET(Row.NUMBER.ToString()).Show(); //معین
                                //var QRE = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 N_KOL FROM dbo.DETA_HES WHERE N_KOL = {Row.NUMBER}").FirstOrDefault();
                                //if (QRE is not null)
                                //{
                                //}
                            }
                        }
                    }
                }
            }
        }

    }
}
