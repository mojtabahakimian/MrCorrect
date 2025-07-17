using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Stimulsoft.Base;
using Stimulsoft.Data.Extensions;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
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
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Stimulsoft.Report.Components;

namespace Wins.WinMenus.HESABDARI
{
    public partial class MOGHAYERAT : Window
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
        #region LOCAL
        public class MYQRE1
        {
            public double? SBED { get; set; }
            public double? SBES { get; set; }
        }
        #endregion

        public MOGHAYERAT(int? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (int)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<MOGH_SUB_MODEL> MOGH_SUB_DATA { get; set; } = new ObservableCollection<MOGH_SUB_MODEL>();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public int? NUMBER_TO_OPEN { get; set; } = 1;
        public bool NowIsReady { get; private set; }
        public bool MOGH_DG_IsFocused { get; private set; }
        public bool NewRecord { get; set; }
        public long? CURRENT_ROW_MOGH_SUB_MODEL_INDEX { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int MOGH_DG_DEF_INDEX_COL
        {
            get
            {
                if (MOGH_DG.Columns.Count > 0)
                {
                    int? defaultcolumnindex = MOGH_DG.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "DATE_S")?.DisplayIndex;
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
        public string? MOGH_SUB_MODEL_ENTERED_VALUE_ROW { get; private set; }
        public MOGH_SUB_MODEL? MOGH_SUB_MODEL_CURRENT_ROW_ITEMS { get; private set; }
        public MOGH_SUB_MODEL? MOGH_SUB_MODEL_WAS_ROW_ITEM { get; private set; }

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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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
        private bool ican = false;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                MODATE.IsReadOnly = !ican;
                ONVAN.IsReadOnly = !ican;
                FDATE.IsReadOnly = !ican;
                TDATE.IsReadOnly = !ican;

                //MOGH_DG.IsReadOnly = !ican; //DataGrid

                HES.IsEnabled = ican;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MOGHA", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            CL_HESABDARI.SETSECURITYSUB(MOGH_DG, "MOGHA");

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            MONUM.Focus();
        }

        private void Summing()
        {
            //جمع کل
            SBED.Text = MOGH_SUB_DATA.Sum(i => i.BED).ToStringNullSafe();
            SBES.Text = MOGH_SUB_DATA.Sum(i => i.BES).ToStringNullSafe();

            //جمع تيک خورده
            var TICKED_ROWS = MOGH_SUB_DATA.Where(i => i.TICK == true).ToList();
            STBED.Text = TICKED_ROWS.Sum(x => x.BED).ToStringNullSafe();
            STBES.Text = TICKED_ROWS.Sum(x => x.BES).ToStringNullSafe();

            //مانده تيک نخورده
            UTICK_SBED.Text = Convert.ToString(Convert.ToDouble(SBED.Text) - Convert.ToDouble(STBED.Text));
            UTICK_SBES.Text = Convert.ToString(Convert.ToDouble(SBES.Text) - Convert.ToDouble(STBES.Text));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = MOGH_DG;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (MOGH_DG_IsFocused)
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

                                DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[MOGH_DG_DEF_INDEX_COL]);

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
        private void FILL_ALL_COMBOBOXES()
        {
            HES.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
        }
        private void MOGH_DG_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && MOGH_DG.SelectedItem != null)
            {
                if (MOGH_DG.Items.Count > 0)
                    CURRENT_ROW_MOGH_SUB_MODEL_INDEX = MOGH_DG.SelectedIndex;

                if (!(e is null) && MOGH_DG.SelectedItem is not null)
                {
                    if (MOGH_DG.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        MOGH_SUB_MODEL_WAS_ROW_ITEM = ((MOGH_SUB_MODEL)MOGH_DG.SelectedItem).Clone() as MOGH_SUB_MODEL;
                    }
                }
            }
        }
        private void MOGH_DG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(MOGH_DG.Items.Count < 1) && !(MOGH_DG.SelectedItem is null))
                {
                    CURRENT_ROW_MOGH_SUB_MODEL_INDEX = MOGH_DG.SelectedIndex;
                }
            }
        }
        private void MOGH_DG_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }
        private void MOGH_DG_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                MOGH_DG_IsFocused = false;
            }
            else
            {
                MOGH_DG_IsFocused = true;
            }
        }
        public void Form_Current()
        {
            var RST = dbms.DoGetDataSQL<MYQRE1>("SELECT  SUM(BED) AS SBED, SUM(BES) AS SBES FROM dbo.MO_DTL WHERE  (MONUM = " + MONUM.Text + ") AND (TICK = 1 )").FirstOrDefault();
            if (RST.SBED is null)
            {
                STBED.Text = "0";
            }
            else
            {
                STBED.Text = RST.SBED.ToString();
            }
            if (RST.SBES is null)
            {
                STBES.Text = "0";
            }
            else
            {
                STBES.Text = RST.SBES.ToString();
            }
        }
        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<MOGHHEAD>($"SELECT MONUM, MODATE, ONVAN, HES, FDATE, TDATE, CRT, UID FROM dbo.MOGHHEAD").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                //AllowEdits = false; //IsOpened
                var item = RecordsData.View.Cast<MOGHHEAD>().FirstOrDefault(x => x.MONUM.Equals(NUMBER_TO_OPEN));
                if (item != null)
                {
                    RecordsData.View.MoveCurrentTo(item);
                    MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                }
            }
            else
            {
                MoveReGetData(INavigator.Jahat.LastItem);
            }
        }

        public void MoveReGetData(INavigator.Jahat jahat, int? custom_postiion = null)
        {
            int RecordCount()
            {
                return ((System.Windows.Data.ListCollectionView)RecordsData.View)?.Count ?? 0;
            }
            void DisplayCounts()
            {
                var RVC = RecordsData.View?.CurrentPosition;
                if (RVC is not null && RecordsData.View?.CurrentItem is not null)
                {
                    //Current Record
                    if (RecordsData.View.CurrentPosition + 1 <= RecordCount())
                    {
                        Current_Rec.Text = Convert.ToString(RVC + 1); // to display number of record in normal way to user, not displaying zero (1)
                    }
                    else
                    {
                        Current_Rec.Text = RVC.ToString();
                    }
                }

                RecCount.Text = (RecordCount()).ToString(); //Record Count
            }

            if (NewRecord && !ConfirmExitWithoutSaving())
            {
                return;
            }

            switch (jahat)
            {
                case INavigator.Jahat.FirstItem: //اولین
                    NewRecord = false;
                    RecordsData.View.MoveCurrentToFirst();
                    break;
                case INavigator.Jahat.BackItem: //قبلی
                    if (RecordsData.View.CurrentPosition > 0) //Possible To Back
                    {
                        if (NewRecord)
                        {
                            jahat = INavigator.Jahat.LastItem;
                            RecordsData.View.MoveCurrentToLast();
                        }
                        else
                        {
                            RecordsData.View.MoveCurrentToPrevious();
                        }
                        NewRecord = false;
                    }
                    break;

                case INavigator.Jahat.NextItem: //بعدی
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1)  //[ RecordCount() - 1 ] : just ensure that stand on existing real item
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToNext();
                    }
                    break;

                case INavigator.Jahat.LastItem: //آخرین
                    RecordsData.View.MoveCurrentToLast();
                    break;

                case INavigator.Jahat.CustomPosition:
                    if (custom_postiion > -1)
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToPosition((int)custom_postiion);
                    }
                    break;

                case INavigator.Jahat.NewItem: //جدید خالی
                    NewRecord = true;
                    RecordsData.View.MoveCurrentToLast();
                    ClearFreshNew();
                    break;
            }

            //Update CurrentViewItem
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as MOGHHEAD;
                var DBData = dbms.DoGetDataSQL<MOGHHEAD>($" SELECT MONUM, MODATE, ONVAN, HES, FDATE, TDATE, CRT, UID FROM dbo.MOGHHEAD WHERE MONUM = {HEADER.MONUM} ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(MOGHHEAD).GetProperties();
                    foreach (var property in properties)
                    {
                        if (property.CanWrite)
                        {
                            var value = property.GetValue(DBData);
                            property.SetValue(HEADER, value);
                        }
                    }
                    RecordsData.View.Refresh();
                }
            }


            DisplayCounts();

            if (RecordCount() == 0)
                NEWRECORD_BTN.IsEnabled = false;
            else
                NEWRECORD_BTN.IsEnabled = true;

            int RDCount = RecordsData.View != null ? RecordsData.View.Cast<object>().Count() : 0;
            if (jahat == INavigator.Jahat.NewItem || RDCount == 0)
            {
                ClearFreshNew();

                Form_Current();
            }
            else
            {
                UiDataUpdate();
            }
        }

        public void ClearFreshNew()
        {
            MOGH_SUB_DATA.Clear();
            //Clearing Elemts Values
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as MOGHHEAD;
                MONUM.Text = HEADER.MONUM.ToStringNullSafe();
                MODATE.Text = HEADER.MODATE.ToStringNullSafe();
                ONVAN.Text = HEADER.ONVAN.ToStringNullSafe();
                FDATE.Text = HEADER.FDATE.ToStringNullSafe();
                TDATE.Text = HEADER.TDATE.ToStringNullSafe();

                HES.IsEnabled = ican;
                HES.SelectedValue = HEADER.HES; HES.Items.Refresh();

                MOGH_DG_ReGetData(); //Load DataGrid's data

                Form_Current();
            }
        }
        public bool ConfirmExitWithoutSaving()
        {
            Msgwin msgwin = new Msgwin(true, "آیتم جدید را ذخیره نکرده اید , آیا از خروج از این آیتم اطمینان دارید ؟");
            msgwin.ShowDialog();
            return msgwin.DialogResult == true;
        }
        public void RefreshAfterDelete()
        {
            var LastCurrentPosition = RecordsData.View.CurrentPosition;

            if (RecordsData.View.CurrentItem != null)
            {
                var itemToRemove = RecordsData.View.CurrentItem as MOGHHEAD;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<MOGHHEAD>;
                    if (underlyingCollection != null)
                    {
                        underlyingCollection.Remove(itemToRemove);
                        RecordsData.View.Refresh(); // Refresh the view to reflect the removal
                    }
                }
            }

            //Move to next exiting item
            if (LastCurrentPosition - 1 > 0)
            {
                MoveReGetData(INavigator.Jahat.CustomPosition, LastCurrentPosition - 1);
                //MoveReGetData(INavigator.Jahat.BackItem);
            }
            else if (LastCurrentPosition + 1 <= ((System.Windows.Data.ListCollectionView)RecordsData.View).Count - 1)
            {
                //MoveReGetData(INavigator.Jahat.NextItem);
                MoveReGetData(INavigator.Jahat.CustomPosition, LastCurrentPosition + 1);
            }
            else
            {
                MoveReGetData(INavigator.Jahat.NewItem);
            }
        }
        public void RefreshAfterInsert()
        {
            var itemtoadd = dbms.DoGetDataSQL<MOGHHEAD>($"SELECT MONUM, MODATE, ONVAN, HES, FDATE, TDATE, CRT, UID FROM dbo.MOGHHEAD WHERE MONUM = {MONUM.Text} ").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<MOGHHEAD>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                ////MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
            }
        }
        public void MOGH_DG_ReGetData()
        {
            MOGH_SUB_DATA?.Clear();
            var data = dbms.DoGetDataSQL<MOGH_SUB_MODEL>($"SELECT DATE_S, MONUM, N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, TICK, N_SERI, BANK, NUMBER, TAG, IDDH, ID FROM dbo.MOGH_SUB WHERE MONUM = {MONUM.Text}").ToList();
            foreach (var item in data)
            {
                MOGH_SUB_DATA.Add(item);
            }

            Summing();

            CL_LMethods.FocusCellReadyToEdit(MOGH_DG,"TICK", MOGH_DG.SelectedIndex,false);
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

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

        private void MOGH_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (MOGH_DG.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = MOGH_DG.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "")
                {
                    e.Handled = true;
                    var text = "000";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
            if (e.Key == Key.Subtract)
            {
                if (CURRENT_COLUMN_NAME is "")
                {
                    e.Handled = true;
                    var text = "00";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
        }
        private void MOGH_DG_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            MOGH_DG.Dispatcher.InvokeAsync(() =>
            {
                MOGH_DG.CellEditEnding -= MOGH_DG_CellEditEnding;
                MOGH_DG.RowEditEnding -= MOGH_DG_RowEditEnding;
                if (_RC_ is null)
                {
                    MOGH_DG.CancelEdit();
                }
                else
                {
                    MOGH_DG.CancelEdit((DataGridEditingUnit)_RC_);
                }
                MOGH_DG.RowEditEnding += MOGH_DG_RowEditEnding;
                MOGH_DG.CellEditEnding += MOGH_DG_CellEditEnding;
            });
        }
        private void MOGH_DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            #region REFILL_CURRENTS
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
                MOGH_SUB_MODEL_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                MOGH_SUB_MODEL_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            MOGH_SUB_MODEL_CURRENT_ROW_ITEMS = e.Row.Item as MOGH_SUB_MODEL;
            #endregion

            if (e.Column.SortMemberPath == "TICK")
            {
                Summing();
            }

        }
        private void MOGH_DG_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && MOGH_DG.SelectedItem is not null)
            {
                if (MOGH_DG.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    MOGH_SUB_MODEL_WAS_ROW_ITEM = ((MOGH_SUB_MODEL)MOGH_DG.SelectedItem).Clone() as MOGH_SUB_MODEL;
                }
            }
        }
        private void MOGH_DG_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as MOGH_SUB_MODEL;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            int? ID = null;
            try
            {
                if (ROW?.ID == null) //INSERT
                {
                    //OUTPUT INSERTED.ID
                    ID = dbms.DoGetDataSQL<int?>(@$"").FirstOrDefault();
                    if (ID != null)
                    {
                        ROW.ID = ID;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"");
                }
            }
            catch (SqlException ex)
            {
                MOGH_DG_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " تکراری است!").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog(); return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
        }

        private bool BodyIsValid(MOGH_SUB_MODEL _row)
        {
            var ROW = _row;

            var errors = (from object i in MOGH_DG.ItemsSource
                          let c = MOGH_DG.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();


            //if (string.IsNullOrEmpty(ROW?.CUST_COD.ToStringNullSafe()))
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "گروه مشتری نمیتواند خالی باشد" });
            //}
            //else if (!double.TryParse(ROW?.CUST_COD.ToStringNullSafe(), out _))
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "گروه مشتری وارد شده در محدوده مجاز نیست" });
            //}

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            //Here Save
            //RefreshAfterInsert();

            foreach (var row in MOGH_SUB_DATA)
            {
                dbms.DoExecuteSQL($@"UPDATE MOGH_SUB SET TICK = {Convert.ToByte(row.TICK)} WHERE ID = {row.ID}");
            }

            MOGH_DG_ReGetData();

            ChangeIsHappend = false;

            universControl.PopNotifyShow($"اطلاعات ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
   
        private void CHATP_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MONUM.Text) || MONUM.Text == "0")
            {
                return;
            }

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.HESABDARI.MOGHAYERAT.mrt");
            report.Load(pathreport);

            StiSqlSource sqlSource = new StiSqlSource();
            sqlSource.CommandTimeout = 6000; // 5 minutes
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=6000";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
            ((StiSqlSource)report.Dictionary.DataSources["MOGHAYERAT"]).CommandTimeout = 6000;

            report["NUMBER_PARAM"] = MONUM.Text;
            (report.GetComponentByName("WIDTH_D") as StiText).Text = Baseknow.WIDTH_D;

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "مغایرت گیری").Show();
            //DoCmd.OpenReport "MOGHAYERAT", acViewPreview,, "MONUM = " + this.MONUM;
        }
        private void BAZKHANI_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MONUM.Text) || MONUM.Text == "0")
            {
                return;
            }

            int NF = 0;
            NF = Convert.ToInt32(MONUM.Text);
            dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = 'MODTL')   DROP Table MODTL");
            dbms.DoExecuteSQL("SELECT MO_DTL.MONUM, MO_DTL.N_S, MO_DTL.HES_K, MO_DTL.HES_M, MO_DTL.HES_T, MO_DTL.SHARH, MO_DTL.BED, MO_DTL.BES, MO_DTL.TICK, MO_DTL.N_SERI, MO_DTL.BANK, MO_DTL.NUMBER, MO_DTL.TAG,MO_DTL.ID INTO MODTL FROM MO_DTL WHERE (((MO_DTL.MONUM)= " + MONUM.Text + "))");
            dbms.DoExecuteSQL("DELETE FROM MO_DTL WHERE (((MO_DTL.MONUM)=" + MONUM.Text + "))");
            dbms.DoExecuteSQL("INSERT INTO dbo.MO_DTL (MONUM, N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, NUMBER, TAG,ID) SELECT " + MONUM.Text + " AS MMM, dbo.DEED_DTL.N_S, dbo.DEED_DTL.HES_K, dbo.DEED_DTL.HES_M, dbo.DEED_DTL.HES_T, dbo.DEED_DTL.SHARH, dbo.DEED_DTL.BED , dbo.DEED_DTL.BES, dbo.DEED_DTL.N_SERI, dbo.DEED_DTL.BANK, dbo.DEED_DTL.NUMBER, dbo.DEED_DTL.TAG, dbo.DEED_DTL.ID FROM  dbo.DEED_HED INNER JOIN  dbo.DEED_DTL ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S AND dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S AND dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S And dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S WHERE     (dbo.DEED_DTL.HES_K = " + CL_HESABDARI.GETKOL(HES.SelectedValue.ToString()) + ") AND (dbo.DEED_DTL.HES_M = " + CL_HESABDARI.GETMOIN(HES.SelectedValue.ToString()) + ") AND (dbo.DEED_DTL.HES_T = " + CL_HESABDARI.GETTAF(HES.SelectedValue.ToString()) + ") AND (dbo.DEED_HED.DATE_S BETWEEN " + FDATE.Text.ToRawTarikh() + " AND " + TDATE.Text.ToRawTarikh() + ")");
            dbms.OpenStoredProcedure("UPDATETICKS");
            MOGH_DG_ReGetData();
        }

        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NewItem);
            //Focus();
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(INavigator.Jahat.LastItem);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NextItem);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.BackItem);
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(INavigator.Jahat.FirstItem);
        }
        private void SERVERRELOAD_Btn_Click(object sender, RoutedEventArgs e)
        {
            ReGetMasterData();
        }

        private void TICK_COLUMN_Click(object sender, RoutedEventArgs e)
        {
            var ROW = MOGH_DG.SelectedItem as MOGH_SUB_MODEL;

            if (ROW != null)
            {
                dbms.DoExecuteSQL($@"UPDATE MOGH_SUB SET TICK = {Convert.ToByte(ROW.TICK)} WHERE ID = {ROW.ID}");
            }

            Summing();
        }
    
    }
}
