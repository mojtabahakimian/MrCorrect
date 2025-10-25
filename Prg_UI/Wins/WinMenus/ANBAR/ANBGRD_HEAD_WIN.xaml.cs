using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static AUTO_BAZ.LocalModles;
using static Prg_UI.Functions.CL_LMethods;
using System.Diagnostics;
using Functions;
using Wins.WinOther;
using static Interfaces.INavigator;

namespace Wins.WinMenus.ANBAR
{
    public partial class ANBGRD_HEAD_WIN : Window, ISearchableWindow
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

        public ANBGRD_HEAD_WIN(int? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (int)number_to_open;
            }
        }

        public Visual I_AM_ANBGRD { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        private NavigationManager<ANBGRD_HEAD_LST_MODEL> _navigationManager;

        public ObservableCollection<ANBARGRD_SUB1_MODEL> ANBARGRD_SUB1_MODEL_DATA { get; set; } = new ObservableCollection<ANBARGRD_SUB1_MODEL>();
        public ObservableCollection<ANBARGRD_SUB2_MODEL> ANBARGRD_SUB2_MODEL_DATA { get; set; } = new ObservableCollection<ANBARGRD_SUB2_MODEL>();
        public ObservableCollection<ANBARGRD_SUB3_MODEL> ANBARGRD_SUB3_MODEL_DATA { get; set; } = new ObservableCollection<ANBARGRD_SUB3_MODEL>();

        public int? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool ANBARGRD_SUB_IsFocused { get; private set; }
        public bool NewRecord { get; set; }
        public long? CURRENT_ROW_ANBARGRD_SUB1_MODEL_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int ANBARGRD_SUB_DEF_INDEX_COL
        {
            get
            {
                if (ANBARGRD_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = ANBARGRD_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODE")?.DisplayIndex;
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
        public string? ENTERED_VALUE_ROW { get; private set; }
        public ANBARGRD_SUB1_MODEL? CURRENT_ROW_ITEMS { get; private set; }
        public ANBARGRD_SUB1_MODEL? WAS_ROW_ITEM { get; private set; }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is ANBGRD_HEAD_LST_MODEL item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.GRD_NUM.Equals(Convert.ToInt32(item.GRD_NUM)));
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;

                        // 1) Find its index in the master list
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx < 0)
                        {
                            // not found (perhaps filtered out?), bail out
                            new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                            return;
                        }

                        // 2) Tell the navigation manager to move to that position
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                        //OnCurrentRecordChanged(itemfound);
                    }
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "GRD_NUM", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "GRD_DATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "حساب کسری و اضافات", PropertyPath = "GRD_HES", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "توضیحات", PropertyPath = "COMMENT", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion



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
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
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

        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;
                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;

                GRD_DATE.IsReadOnly = !ican; ; //تاریخ
                COMMENT.IsReadOnly = !ican; ; //توضیحات

                GRD_ANBAR.IsEnabled = ican; //انبار
                GRD_HES.IsEnabled = ican; //حساب کسری و اضافت

                if (!_navigationManager.IsNewRecord)
                {
                    Command19.IsEnabled = ican;
                    BTN_SAVE.IsEnabled = ican;
                }
            }
        }

        public class ANB1
        {
            public string? CODE { get; set; }
        }

        public class ANB2
        {
            public int? GRD_NUM { get; set; }
            public long? GRD_DATE { get; set; }
            public int? GRD_ANBAR { get; set; }
            public string? GRD_HES { get; set; }
            public double? N_S { get; set; }
            public string? COMMENT { get; set; }
            public string? USER_NAME { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }

        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void Fill_ComboBoxes()
        {
            GRD_ANBAR.ItemsSource = dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT CODE, NAMES FROM TCOD_ANBAR").ToList();
            GRD_ANBAR.SelectedValuePath = "CODE";
            GRD_ANBAR.DisplayMemberPath = "NAMES";

            GRD_HES.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB>("SELECT * FROM CUST_HESAB").ToList();
            GRD_HES.SelectedValuePath = "hes";
            GRD_HES.DisplayMemberPath = "hes";
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_ANBGRD = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "ANGD", new WindowInteropHelper(this).Handle);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            Fill_ComboBoxes();

            #region LoadExisting

            _navigationManager = new NavigationManager<ANBGRD_HEAD_LST_MODEL>(
                dbms,
                x => x.GRD_NUM.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM ANBGRD_HEAD ORDER BY GRD_NUM", //All Record of The Table
                x => $"SELECT * FROM ANBGRD_HEAD WHERE GRD_NUM = {x?.GRD_NUM}", //On Change for One Record
                Convert.ToInt32(NUMBER_TO_OPEN)
                );

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();
            #endregion

            Command19.IsEnabled = false;

            Command21.IsEnabled = false;
            Command22.IsEnabled = false;
            Command23.IsEnabled = false;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = ANBARGRD_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (BTN_SAVE.IsFocused)
                    {
                        return;
                    }

                    e.Handled = true;
                    if (ANBARGRD_SUB_IsFocused)
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

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[ANBARGRD_SUB_DEF_INDEX_COL]);

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
            }
            catch { /*ignore*/ }

            if (!ANBARGRD_SUB_IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }

            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (ANBARGRD_SUB_IsFocused)
                {
                    //BTN_DELETE_Click(null,null);
                }
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

        private void OnCurrentRecordChanged(ANBGRD_HEAD_LST_MODEL HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                if (HEADER_FAC is null)
                {
                    new Msgwin(false, "این برگه خالی است").Show();
                    return;
                }

                GRD_NUM.Text = HEADER_FAC.GRD_NUM.ToStringNullSafe(); //شماره
                GRD_DATE.Text = HEADER_FAC.GRD_DATE.ToStringNullSafe(); //تاریخ
                GRD_ANBAR.SelectedValue = HEADER_FAC.GRD_ANBAR; //انبار
                GRD_HES.SelectedValue = HEADER_FAC.GRD_HES; //حساب کسری و اضافت
                COMMENT.Text = HEADER_FAC.COMMENT; //توضیحات
                N_S.Text = HEADER_FAC?.N_S?.ToStringNullSafe(); //شماره سند
                USER_NAME.Text = HEADER_FAC?.USER_NAME; //کاربر
                AllowEdits = false;

                ReGetData();
                ReGetData2();
                ReGetData3();

                Form_Current();

                GetDefaultFocus();
            }
        }

        private void GetDefaultFocus()
        {
            GRD_DATE.Focus();
            GRD_DATE.SelectAll();
        }

        private bool OnInsertRecord(ANBGRD_HEAD_LST_MODEL record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<ANBGRD_HEAD_LST_MODEL>($"SELECT * FROM ANBGRD_HEAD WHERE GRD_NUM = {GRD_NUM.Text}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<ANBGRD_HEAD_LST_MODEL>($"SELECT * FROM ANBGRD_HEAD WHERE GRD_NUM = {GRD_NUM.Text}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }


        public void Form_Current()
        {
            if (string.IsNullOrEmpty(GRD_NUM.Text))
            {
                Command21.IsEnabled = false;
                Command22.IsEnabled = false;
                Command23.IsEnabled = false;
            }
            if (this.ANBARGRD_SUB1_MODEL_DATA.Count > 0)
            {
                this.Command19.IsEnabled = false;
            }
            else
            {
                this.Command19.IsEnabled = true;
            }
            if (this.NewRecord)
            {
                this.Command19.IsEnabled = false;
                this.ANBARGRD_SUB.IsReadOnly = false;
                this.ANBARGRD_SUB2.IsReadOnly = false;
                this.ANBARGRD_SUB3.IsReadOnly = false;
            }
            else
            {
                this.Command19.IsEnabled = true;
                this.ANBARGRD_SUB.IsReadOnly = true;
                this.ANBARGRD_SUB2.IsReadOnly = true;
                this.ANBARGRD_SUB3.IsReadOnly = true;
            }
        }
        public void Form_Before_Update()
        {
            if (IsNull(this.GRD_NUM.Text))
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT  MAX(GRD_NUM) AS mgrd FROM dbo.ANBGRD_HEAD").ToList();
                if (IsNull(RST.FirstOrDefault()) || RST.Count == 0)
                {
                    this.GRD_NUM.Text = "1";
                }
                else
                {
                    this.GRD_NUM.Text = Convert.ToString(RST.FirstOrDefault() + 1);
                }
            }
        }

        private void ANBARGRD_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && ANBARGRD_SUB.SelectedItem != null)
            {
                if (ANBARGRD_SUB.Items.Count > 0)
                    CURRENT_ROW_ANBARGRD_SUB1_MODEL_INDEX = ANBARGRD_SUB.SelectedIndex;

                if (!(e is null) && ANBARGRD_SUB.SelectedItem is not null)
                {
                    if (ANBARGRD_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((ANBARGRD_SUB1_MODEL)ANBARGRD_SUB.SelectedItem).Clone() as ANBARGRD_SUB1_MODEL;
                    }
                }
            }
        }
        private void ANBARGRD_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(ANBARGRD_SUB.Items.Count < 1) && !(ANBARGRD_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_ANBARGRD_SUB1_MODEL_INDEX = ANBARGRD_SUB.SelectedIndex;
                }
            }
        }
        private void ANBARGRD_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }
        private void ANBARGRD_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                ANBARGRD_SUB_IsFocused = false;
            }
            else
            {
                ANBARGRD_SUB_IsFocused = true;
            }
        }
        private void ANBARGRD_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (ANBARGRD_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = ANBARGRD_SUB.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
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
        private void ANBARGRD_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && ANBARGRD_SUB.SelectedItem is not null)
            {
                if (ANBARGRD_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((ANBARGRD_SUB1_MODEL)ANBARGRD_SUB.SelectedItem).Clone() as ANBARGRD_SUB1_MODEL;
                }
            }
        }

        private void ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            ANBARGRD_SUB.Dispatcher.InvokeAsync(() =>
            {
                ANBARGRD_SUB.CellEditEnding -= ANBARGRD_SUB_CellEditEnding;
                ANBARGRD_SUB.RowEditEnding -= ANBARGRD_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    ANBARGRD_SUB.CancelEdit();
                }
                else
                {
                    ANBARGRD_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                ANBARGRD_SUB.RowEditEnding += ANBARGRD_SUB_RowEditEnding;
                ANBARGRD_SUB.CellEditEnding += ANBARGRD_SUB_CellEditEnding;
            });
        }
        private void ANBARGRD_SUB2_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            ANBARGRD_SUB.Dispatcher.InvokeAsync(() =>
            {
                ANBARGRD_SUB2.CellEditEnding -= ANBARGRD_SUB2_CellEditEnding;
                ANBARGRD_SUB2.RowEditEnding -= ANBARGRD_SUB2_RowEditEnding;
                if (_RC_ is null)
                {
                    ANBARGRD_SUB2.CancelEdit();
                }
                else
                {
                    ANBARGRD_SUB2.CancelEdit((DataGridEditingUnit)_RC_);
                }
                ANBARGRD_SUB2.RowEditEnding += ANBARGRD_SUB2_RowEditEnding;
                ANBARGRD_SUB2.CellEditEnding += ANBARGRD_SUB2_CellEditEnding;
            });
        }
        private void ANBARGRD_SUB3_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            ANBARGRD_SUB.Dispatcher.InvokeAsync(() =>
            {
                ANBARGRD_SUB3.CellEditEnding -= ANBARGRD_SUB3_CellEditEnding;
                ANBARGRD_SUB3.RowEditEnding -= ANBARGRD_SUB3_RowEditEnding;
                if (_RC_ is null)
                {
                    ANBARGRD_SUB3.CancelEdit();
                }
                else
                {
                    ANBARGRD_SUB3.CancelEdit((DataGridEditingUnit)_RC_);
                }
                ANBARGRD_SUB3.RowEditEnding += ANBARGRD_SUB3_RowEditEnding;
                ANBARGRD_SUB3.CellEditEnding += ANBARGRD_SUB3_CellEditEnding;
            });
        }

        private void ANBARGRD_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as ANBARGRD_SUB1_MODEL;
            #endregion

            #region NUM1_After_Update
            if (e.Column.SortMemberPath == "NUM1")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون شمارش اول فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        CURRENT_ROW_ITEMS.NUM1 = WAS_ROW_ITEM.NUM1;
                    }
                    else
                    {
                        if (Convert.ToDecimal(ENTERED_VALUE_ROW) - CURRENT_ROW_ITEMS.MOG == 0)
                        {
                            CURRENT_ROW_ITEMS.NUM2 = Convert.ToDouble(ENTERED_VALUE_ROW);
                            CURRENT_ROW_ITEMS.NUM3 = Convert.ToDouble(ENTERED_VALUE_ROW);
                        }
                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش اول نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion

            #region MOG_After_Update
            if (e.Column.SortMemberPath == "MOG")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون موجودی فعلی فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        CURRENT_ROW_ITEMS.MOG = WAS_ROW_ITEM.MOG;
                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش اول نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion


        }
        private void ANBARGRD_SUB2_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as ANBARGRD_SUB1_MODEL;
            #endregion

            #region NUM1_After_Update
            if (e.Column.SortMemberPath == "NUM2")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون شمارش دوم فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB2_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        CURRENT_ROW_ITEMS.NUM2 = WAS_ROW_ITEM.NUM1;
                    }
                    else
                    {
                        if (Convert.ToDecimal(ENTERED_VALUE_ROW) - CURRENT_ROW_ITEMS.MOG == 0)
                        {
                            CURRENT_ROW_ITEMS.NUM3 = Convert.ToDouble(ENTERED_VALUE_ROW);
                        }
                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش دوم نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB2_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion

            #region MOG_After_Update
            if (e.Column.SortMemberPath == "MOG")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون موجودی فعلی فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        CURRENT_ROW_ITEMS.MOG = WAS_ROW_ITEM.MOG;
                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش اول نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion
        }
        private void ANBARGRD_SUB3_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as ANBARGRD_SUB1_MODEL;
            #endregion

            #region NUM1_After_Update
            if (e.Column.SortMemberPath == "NUM2")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون شمارش سوم فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB3_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    }
                    else
                    {

                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش سوم نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB3_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion

            #region MOG_After_Update
            if (e.Column.SortMemberPath == "MOG")
            {
                if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (!double.TryParse(ENTERED_VALUE_ROW, out double _))
                    {
                        universControl.PopNotifyShow("در ستون موجودی فعلی فقط باید عدد وارد گردد !", Pop1, Pop1Text1, Pop_Border1);
                        ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        CURRENT_ROW_ITEMS.MOG = WAS_ROW_ITEM.MOG;
                    }

                }
                else
                {
                    universControl.PopNotifyShow("شمارش سوم نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    ANBARGRD_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //ANBARGRD_SUB1_MODEL_CURRENT_ROW_ITEMS.MOGODI_A = ANBARGRD_SUB1_MODEL_WAS_ROW_ITEM?.MOGODI_A;
                }
            }
            #endregion
        }

        private void ANBARGRD_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as ANBARGRD_SUB1_MODEL;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            long? ID = null;
            try
            {

                dbms.DoExecuteSQL($@"UPDATE ANBGRD_LST
					                                SET NUM1 = {CURRENT_ROW_ITEMS.NUM1},
					                                	MOG = {CURRENT_ROW_ITEMS.MOG}
					                                WHERE GRD_NUM = {GRD_NUM.Text} AND CODE = N'{CURRENT_ROW_ITEMS.CODE}'");

            }
            catch (SqlException ex)
            {
                ANBARGRD_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " لطفا مقادیر را درست وارد کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        private void ANBARGRD_SUB2_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as ANBARGRD_SUB1_MODEL;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            long? ID = null;
            try
            {

                dbms.DoExecuteSQL($@"UPDATE ANBGRD_LST
					                                SET NUM2 = {CURRENT_ROW_ITEMS.NUM2},
					                                	MOG = {CURRENT_ROW_ITEMS.MOG}
					                                WHERE GRD_NUM = {GRD_NUM.Text} AND CODE = N'{CURRENT_ROW_ITEMS.CODE}'");

            }
            catch (SqlException ex)
            {
                ANBARGRD_SUB2_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " لطفا مقادیر را درست وارد کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void ANBARGRD_SUB3_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as ANBARGRD_SUB1_MODEL;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            long? ID = null;
            try
            {

                dbms.DoExecuteSQL($@"UPDATE ANBGRD_LST
					                                SET NUM3 = {CURRENT_ROW_ITEMS.NUM3},
					                                	MOG = {CURRENT_ROW_ITEMS.MOG}
					                                WHERE GRD_NUM = {GRD_NUM.Text} AND CODE = N'{CURRENT_ROW_ITEMS.CODE}'");

            }
            catch (SqlException ex)
            {
                ANBARGRD_SUB3_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " لطفا مقادیر را درست وارد کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool BodyIsValid(ANBARGRD_SUB1_MODEL _row)
        {
            var ROW = _row;

            var errors = (from object i in ANBARGRD_SUB.ItemsSource
                          let c = ANBARGRD_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
        public bool VALIDATION()
        {
            if (IsNull(this.GRD_DATE.Text.ToRawTarikh()))
            {
                Msgwin msgwin = new Msgwin(false, " تاریخ نمی تواند خالی باشد ....!");
                msgwin.ShowDialog();
                return false;
            }

            if (IsNull(this.GRD_NUM.Text))
            {
                Msgwin msgwin = new Msgwin(false, " شماره نمی تواند خالی باشد ....!");
                msgwin.ShowDialog();
                return false;
            }

            if (IsNull(this.GRD_HES.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, " حساب کسری و اضافات نمی تواند خالی باشد ....!");
                msgwin.ShowDialog();
                return false;
            }

            if (IsNull(this.GRD_ANBAR.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, " انبار نمی تواند خالی باشد ....!");
                msgwin.ShowDialog();
                return false;
            }

            var S_ANBAR_DATE = dbms.DoGetDataSQL<ANB2>($"SELECT * FROM ANBGRD_HEAD WHERE GRD_ANBAR = {GRD_ANBAR.SelectedValue} AND GRD_DATE = {GRD_DATE.Text.ToRawTarikh()}").ToList();

            bool DateChanged = false;
            if (!string.IsNullOrEmpty(GRD_DATE.Text.ToRawTarikh()))
            {
                DateChanged = GRD_DATE.Text.ToRawTarikh() != _navigationManager?.CurrentRecord?.GRD_DATE?.ToStringNullSafe();
            }

            if ((_navigationManager?.IsNewRecord ?? false) || DateChanged)
            {
                
                if (S_ANBAR_DATE.Count >= 1)
                {
                    Msgwin msgwin = new Msgwin(false, $"در حال حاضر انبار گردانی ثبت شده به شماره {S_ANBAR_DATE.FirstOrDefault().GRD_NUM} با همین تاریخ و همین انبار وجود دارد ، لطفا تاریخ را تغییر دهید");
                    msgwin.ShowDialog();
                    return false;
                }
            }

            return true;
        }
        public void DATE_VALIDATION()
        {
            bool Date_Is_Valid = true;

            var DATE = GRD_DATE.Text.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    GRD_DATE.Text = null;
                    GRD_DATE.Focus();
                    Date_Is_Valid = false;
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        GRD_DATE.Text = null;
                        GRD_DATE.Focus();
                        Date_Is_Valid = false;
                        return;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                GRD_DATE.Focus();
                Date_Is_Valid = false;
                return;
            }
        }
        public void ReGetData()
        {
            ANBARGRD_SUB1_MODEL_DATA?.Clear();
            if (GRD_NUM.Text is not null && GRD_NUM.Text != "")
            {
                var ANBARGRD_SUB1_MODEL_DATA_TEMP = dbms.DoGetDataSQL<ANBARGRD_SUB1_MODEL>($@"SELECT EKH, GRD_NUM, CODE, MOG, NUM1, NUM2, NUM3, MABL, NAMES, nam, N_FANI, grp FROM ANBARGRD_SUB1 WHERE GRD_NUM = {GRD_NUM.Text}").ToList();

                // INVO_RASIDA_DATA?.Clear();

                foreach (var item in ANBARGRD_SUB1_MODEL_DATA_TEMP)
                {
                    ANBARGRD_SUB1_MODEL_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }
        public void ReGetData2()
        {
            ANBARGRD_SUB2_MODEL_DATA?.Clear();
            if (GRD_NUM.Text is not null && GRD_NUM.Text != "")
            {
                var ANBARGRD_SUB2_MODEL_DATA_TEMP = dbms.DoGetDataSQL<ANBARGRD_SUB2_MODEL>($@"SELECT EKH, GRD_NUM, CODE, nam, MOG, NUM1, NUM2, NUM3, MABL, NAMES, N_FANI, grp FROM ANBARGRD_SUB2 WHERE GRD_NUM = {GRD_NUM.Text}").ToList();

                // INVO_RASIDA_DATA?.Clear();

                foreach (var item in ANBARGRD_SUB2_MODEL_DATA_TEMP)
                {
                    ANBARGRD_SUB2_MODEL_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }
        public void ReGetData3()
        {
            ANBARGRD_SUB3_MODEL_DATA?.Clear();
            if (GRD_NUM.Text is not null && GRD_NUM.Text != "")
            {
                var ANBARGRD_SUB3_MODEL_DATA_TEMP = dbms.DoGetDataSQL<ANBARGRD_SUB3_MODEL>($@"SELECT EKH, GRD_NUM, CODE, nam, MOG, NUM1, NUM2, NUM3, MABL, NAMES, N_FANI, grp FROM ANBARGRD_SUB3 WHERE GRD_NUM = {GRD_NUM.Text}").ToList();

                // INVO_RASIDA_DATA?.Clear();

                foreach (var item in ANBARGRD_SUB3_MODEL_DATA_TEMP)
                {
                    ANBARGRD_SUB3_MODEL_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (VALIDATION() is false)
            {
                return;
            }
            //Here Save
            var number = dbms.DoGetDataSQL<double?>("SELECT MAX(GRD_NUM)+1 FROM ANBGRD_HEAD").FirstOrDefault();
            if (_navigationManager.IsNewRecord)
            {
                if (number is null)
                {
                    number = 1;
                    GRD_NUM.Text = number.ToString();
                }
                else
                {
                    GRD_NUM.Text = number.ToString();
                }

                //INSERT
                dbms.DoExecuteSQL(@$"INSERT INTO dbo.ANBGRD_HEAD (       GRD_NUM,                     GRD_DATE,                GRD_ANBAR,                    GRD_HES,          COMMENT,           USER_NAME) 
                                                                   VALUES ({GRD_NUM.Text},{GRD_DATE.Text.ToRawTarikh()},{GRD_ANBAR.SelectedValue}, N'{GRD_HES.SelectedValue}',N'{COMMENT.Text}', N'{USER_NAME.Text}')");

                RefreshAfterUpdate();
            }
            else
            {
                //UPDATE
                dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_HEAD
                                                      SET 
                                                          GRD_DATE = {GRD_DATE.Text.ToRawTarikh()},
                                                          GRD_ANBAR = {GRD_ANBAR.SelectedValue},
                                                          GRD_HES = N'{GRD_HES.SelectedValue}',
                                                          COMMENT = N'{COMMENT.Text}',
                                                          USER_NAME = N'{USER_NAME.Text}'
                                                      WHERE 
                                                          GRD_NUM = {GRD_NUM.Text}");
            }

            universControl.PopNotifyShowUp("ذخیره انجام شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);

            ChangeIsHappend = false;
            Command19.IsEnabled = true;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!string.IsNullOrEmpty(GRD_NUM.Text))
            {
                DateTime dt = DateTime.Now;
                CL_HESABDARI.TR("ANBGRD_HEAD", "(GRD_NUM = " + this.GRD_NUM.Text + ")", dt, 1);
                CL_HESABDARI.TR("ANBGRD_LST", "(GRD_NUM = " + this.GRD_NUM.Text + ")", dt, 1);

                AllowEdits = true;
                this.ANBARGRD_SUB.IsReadOnly = false;
                this.ANBARGRD_SUB2.IsReadOnly = false;
                this.ANBARGRD_SUB3.IsReadOnly = false;
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_DELETE.IsEnabled || _navigationManager.IsNewRecord) { return; }

            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (string.IsNullOrEmpty(GRD_NUM.Text) || string.IsNullOrWhiteSpace(GRD_NUM.Text) || GRD_NUM.Text == "0")
            {
                new Msgwin(false, "هنوز انبار گردانی شماره نگرفته , ابتدا آنرا ذخیره کنید").ShowDialog();
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "انبار گردانی",
                    recordId: GRD_NUM.Text,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                var dt = DateTime.Now;
                CL_HESABDARI.TR("ANBGRD_HEAD", "(GRD_NUM = " + this.GRD_NUM.Text + ")", dt, 1);
                CL_HESABDARI.TR("ANBGRD_LST", "(GRD_NUM = " + this.GRD_NUM.Text + ")", dt, 1);


                if (ANBARGRD_SUB1_MODEL_DATA.Count > 0 && ANBARGRD_SUB_IsFocused && ANBARGRD_SUB.SelectedItems.Count > 0) //Any Sub Items ?
                {
                    if (!(ANBARGRD_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        var editableCollectionView = ANBARGRD_SUB.Items as IEditableCollectionView;
                        if (editableCollectionView != null && editableCollectionView.IsEditingItem) { editableCollectionView.CommitEdit(); }

                        for (int i = 0; i < ANBARGRD_SUB.SelectedItems.Count; i++)
                        {
                            var item = ANBARGRD_SUB.SelectedItems[i];

                            if (CL_LMethods.IsNewPlaceHolder(ANBARGRD_SUB, item))
                            {
                                continue; // Skip deletion for new placeholder items
                            }

                            var _CODE_ = item.GetType()?.GetProperty("CODE")?.GetValue(item);

                            if (_CODE_ != null)
                            {
                                try
                                {
                                    IsDeletedSomething = true;

                                    ESLAH_Click(null, null);

                                    dbms.DoExecuteSQL($@"DELETE FROM dbo.ANBGRD_LST WHERE (GRD_NUM = " + this.GRD_NUM.Text + ") AND (CODE = N'" + _CODE_ + "')");
                                }
                                catch (SqlException ex)
                                {
                                    if (ex.Number == 547)
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                                    }
                                    else
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                                    }
                                }
                                catch (Exception)
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                                }
                            }
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                  .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }

                        if (IsDeletedSomething)
                        {
                            ReGetData();
                        }
                    }

                }
                else
                {
                    //dbms.DoExecuteSQL($"DELETE FROM ANBGRD_HEAD WHERE GRD_NUM = {GRD_NUM.Text}");
                    if (!string.IsNullOrEmpty(GRD_NUM.Text) && GRD_NUM.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM ANBGRD_HEAD WHERE GRD_NUM = {GRD_NUM.Text}");

                            SANAD();

                            _navigationManager.DeleteCurrentRecord(); //Refresh Record Source
                        }
                        catch (SqlException ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این برگه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
                }

            }
        }

        private void COMMENT_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string co;
            COMMENT.Text = Strings.Trim(COMMENT.Text);
            //ERROR
            if (COMMENT.Text.StartsWith("#"))
            {
                co = COMMENT.Text.Substring(1);

                var RST = dbms.DoGetDataSQL<ANB1>("SELECT CODE FROM STUF_DEF WHERE CODE = '" + co + "'").ToList();
                if (RST.Count > 0)
                {
                    var RST2 = dbms.DoGetDataSQL<ANBGRD_LST>("SELECT * FROM ANBGRD_LST").ToList();
                    //RST2.AddNew();
                    RST2.FirstOrDefault().CODE = co;
                    RST2.FirstOrDefault().GRD_NUM = Convert.ToInt32(this.GRD_NUM.Text);

                    dbms.DoExecuteSQL(@$"INSERT INTO ANBGRD_LST (code , GRD_NUM) 
				                                         VALUES({RST2.FirstOrDefault().CODE} ,{RST2.FirstOrDefault().GRD_NUM}) ");
                    //RST2.update();
                }
                //this.ANBGRD_LST_SUB.Requery();
                //this.ANBGRD_LST_SUB2.Requery();
                //this.ANBGRD_LST_SUB3.Requery();
                COMMENT.Text = "";
            }
        }

        private void Command19_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GRD_NUM.Text))
            {
                Msgwin msgwin = new Msgwin(false, "قبل از ذخیره نمی توانید موجودی کالاها را دریافت کنید");
                msgwin.ShowDialog();
            }
            if (Convert.ToInt32(GRD_NUM.Text) > 0 && ANBARGRD_SUB1_MODEL_DATA.Count == 0)
            {
                this.ANBARGRD_SUB.IsReadOnly = false;
                this.ANBARGRD_SUB2.IsReadOnly = false;
                this.ANBARGRD_SUB3.IsReadOnly = false;
            }
            try
            {
                dbms.DoExecuteSQL("INSERT INTO dbo.ANBGRD_LST  (CODE, MOG, GRD_NUM) SELECT CODE, MAND, " + this.GRD_NUM.Text + " AS GN FROM dbo.MOGUDI(" + this.GRD_DATE.Text.ToRawTarikh() + "," + this.GRD_ANBAR.SelectedValue + ") MOGUDI");
            }
            catch (Exception)
            {

                Msgwin msgwin = new Msgwin(false, "اشكالي در انتقال كالاها به وجود آمده است .ممكن است براي يك كالا دوتا فرمول تعريف كرده باشيد لطفا برسي كنيد");
                msgwin.Show();
            }

            ReGetData();
            ReGetData2();
            ReGetData3();

            if (string.IsNullOrEmpty(GRD_NUM.Text))
            {
                Command21.IsEnabled = false;
                Command22.IsEnabled = false;
                Command23.IsEnabled = false;
            }
            else
            {
                Command21.IsEnabled = true;
                Command22.IsEnabled = true;
                Command23.IsEnabled = true;
            }
            ANBARGRD_SUB.CanUserAddRows = false;
            ANBARGRD_SUB.CanUserDeleteRows = false;
            ANBARGRD_SUB2.CanUserAddRows = false;
            ANBARGRD_SUB2.CanUserDeleteRows = false;
            ANBARGRD_SUB3.CanUserAddRows = false;
            ANBARGRD_SUB3.CanUserDeleteRows = false;
        }


        private void Command22_Copy_Click(object sender, RoutedEventArgs e)
        {
           
            if (!IsNull(this.GRD_HES.SelectedValue))
            {
                SANAD();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "اشكالي در انتقال كالاها به وجود آمده است .ممكن است براي يك كالا دوتا فرمول تعريف كرده باشيد لطفا برسي كنيد");
                msgwin.ShowDialog();
            }
        }
        private void SANAD()
        {
            var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADANBARGARD(Convert.ToInt64(GRD_NUM.Text), Convert.ToInt64(GRD_NUM.Text), false);

            if (SanadNumber != null)
            {
                N_S.Text = SanadNumber.ToString();
            }
        }


        private void Command23_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.r_counter1.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = GRD_NUM.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            new Rpts.WINRPT(report, "چاپ لیست شمارش اول").Show();
            //report.Show();
        }

        private void Command21_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.r_counter2.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = GRD_NUM.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "چاپ لیست شمارش دوم").Show();
        }

        private void Command22_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.r_counter3.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = GRD_NUM.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);
            //report.Show();
            new Rpts.WINRPT(report, "چاپ لیست کالا ها جهت شمارش سوم").Show();
        }

        private void Command25_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.r_anbargrd.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = GRD_NUM.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "چاپ انبار گردانی").Show();
        }

        private void Command24_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.R_TAG.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = GRD_NUM.Text;
            report["ANBAR_PARM"] = GRD_ANBAR.SelectedValue;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "چاپ تگ انبار گرادنی").Show();
        }

        private void BTN_LSTANBATGRD_Click(object sender, RoutedEventArgs e)
        {
            new WIN_ANBAR_GRD_LIST().Show();
        }

        private void BTN_NEWABARGRD_Click(object sender, RoutedEventArgs e)
        {
            //Clear for new
            ClearFreshAll();
        }

        private void ClearFreshAll()
        {
            GRD_NUM.Text = null;
            NewRecord = true;
            NUMBER_TO_OPEN = null;

            ANBARGRD_SUB1_MODEL_DATA?.Clear();
            ANBARGRD_SUB2_MODEL_DATA?.Clear();
            ANBARGRD_SUB3_MODEL_DATA?.Clear();

            N_S.Text = null;
            USER_NAME.Text = null;
            COMMENT.Text = null; //توضیحات
            GRD_DATE.Text = null; //تاریخ
            GRD_ANBAR.SelectedValue = null; GRD_ANBAR.Items.Refresh();  //انبار
            GRD_HES.SelectedValue = null; GRD_HES.Items.Refresh();  //حساب کسری و اضافت

            Command19.IsEnabled = false;
            BTN_SAVE.IsEnabled = true;

            AllowEdits = true;

            GetDefaultFocus();
        }
    }
}
