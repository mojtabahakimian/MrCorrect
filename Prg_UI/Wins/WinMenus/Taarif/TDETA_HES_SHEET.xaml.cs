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
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;

namespace Wins.WinMenus.Taarif
{
    public partial class TDETA_HES_SHEET : Window
    {
        public TDETA_HES_SHEET(int _NKOL_, int _NUMBER_)
        {
            InitializeComponent();

            N_KOL = _NKOL_;
            NUMBER = _NUMBER_;

            this.DataContext = this;
        }

        public int N_KOL { get; set; } //کل
        public int NUMBER { get; set; } //معین

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

        #region LOMODELS
        public class CMB1
        {
            public string? ROUTE_NAME { get; set; }
            public string? Expr1 { get; set; }
        }

        public class TobItem
        {
            public int CODE { get; set; }
            public string NAMES { get; set; }
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();
        public bool ChangeIsHappend { get; private set; } = false;

        public ObservableCollection<TDETA_HES> TDETA_HES_DATA { get; set; } = new ObservableCollection<TDETA_HES>();

        //universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        public bool TDETA_HES_SUB_IsFocused { get; private set; }
        public TDETA_HES? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TDETA_HES? WAS_ROW_ITEM { get; private set; } = new();
        public int CURRENT_ROW_INDEX { get; set; }
        public Visual I_AM_TDETA_HES_SHEET { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (TDETA_HES_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TDETA_HES_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "TNUMBER")?.DisplayIndex;
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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                if (_ican is true) // Is Enable and ReadOnly = False
                {

                }
                else
                {

                }
            }
        }

        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_TDETA_HES_SHEET = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();

            ReGetData();

            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = TDETA_HES_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                try
                {
                    if (uie is DataGridCell || TDETA_HES_SUB_IsFocused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int DefaultColumnIndex = CL_LMethods.GetLastColumn(TDETA_HES_SUB).DisplayIndex;
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
                }
                catch { /*ignore*/ }



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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع مشتری
            CUST_COD_COLUMN.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM dbo.CUSTKIND ORDER BY CUSTKNAME").ToList();



            //مسیر ویزیت
            ROUTE_NAME_COLUMN.ItemsSource = dbms.DoGetDataSQL<CMB1>($@"SELECT Visit_route.ROUTE_NAME, Visit_route.ROUTE_NAME+N' - '+CUST_HESAB.NAME+N' - '+CUST_HESAB.hes AS Expr1
                                                                   FROM Visit_route
                                                                        INNER JOIN CUST_HESAB ON Visit_route.HES=CUST_HESAB.hes
                                                                   WHERE(Visit_route.RACTIVE=1)").ToList();

            //کد استان
            ALL_OSTAN = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();

            foreach (var item in ALL_OSTAN) { item.OSNAME = item.OSNAME?.FixPersianChars(); }

            //کد شهر
            ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();

            OSTANID_COLUMN.ItemsSource = ALL_OSTAN;
            SHAHRID_COLUMN.ItemsSource = ALL_SHAHR;

            //شخصیت
            TOB_COLUMN.ItemsSource = new List<TobItem>
            {
                new TobItem { CODE = 1, NAMES = "حقیقی" },
                new TobItem { CODE = 2, NAMES = "حقوقی" },
                new TobItem { CODE = 3, NAMES = "مشارکت مدنی" },
                new TobItem { CODE = 4, NAMES = "اتباع غیر ایرانی" }
            };

        }

        private ICollectionView DataViewPal;
        private void ReGetData(bool GOTOLAST = false)
        {
            TDETA_HES_DATA?.Clear();

            var RST = dbms.DoGetDataSQL<TDETA_HES>($@" SELECT N_KOL, NUMBER, TNUMBER, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, IDD, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, CRT, UID, tob FROM dbo.TDETA_HES WHERE (N_KOL = {N_KOL} AND NUMBER = {NUMBER}) ORDER BY TNUMBER").ToList();
            foreach (var item in RST)
            {
                TDETA_HES_DATA.Add(item);
            }

            var _DataGrid_ = TDETA_HES_SUB;
            string _SORTPATH_ = "TNUMBER";
            int lastindexrow = _DataGrid_.Items.Count - 1;

            if (GOTOLAST)
            {
                CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
            }
            else
            {
                lastindexrow = _DataGrid_.Items.IndexOf(_DataGrid_?.CurrentItem);
                if (lastindexrow > 0)
                {
                    CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, lastindexrow, false);
                }
                else
                {
                    CL_LMethods.FocusCellReadyToEdit(_DataGrid_, _SORTPATH_, _DataGrid_.Items.Count - 1, false);
                }
            }

            DataViewPal = CollectionViewSource.GetDefaultView(TDETA_HES_DATA);
            TDETA_HES_SUB.ItemsSource = DataViewPal;
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
                    if (obj is TDETA_HES model)
                    {
                        return !string.IsNullOrEmpty(model.NAME) &&
                               model.NAME.ToLower().Contains(query);
                    }
                    return false;
                };
            }
            DataViewPal.Refresh();
        }



        private void TDETA_HES_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TDETA_HES_SUB_IsFocused = false;
            }
            else
            {
                TDETA_HES_SUB_IsFocused = true;
            }
        }
        private void TDETA_HES_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TDETA_HES_SUB.SelectedItem is not null)
            {
                if (TDETA_HES_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TDETA_HES)TDETA_HES_SUB.SelectedItem).Clone() as TDETA_HES;
                }
                //var editableCollectionView = TDETA_HES_SUB.Items as IEditableCollectionView;
                //if (!editableCollectionView.IsAddingNew) { }
            }
        }
        private void TDETA_HES_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TDETA_HES_SUB.Items.Count > 0 && TDETA_HES_SUB.SelectedItem != null)
                {
                    IEditableCollectionView itemsView = TDETA_HES_SUB.Items as IEditableCollectionView;
                    if (!itemsView.IsAddingNew && !itemsView.IsEditingItem)
                    {
                        if (!(TDETA_HES_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {
                                _ = AuditLogger.LogActionAsync(
                                        actionType: "DELETE",
                                        tableName: "سرفصل حسابهاي تفضیلی",
                                        recordId: TDETA_HES_SUB.SelectedItem.ToStringNullSafe(),
                                        oldValue: null,
                                        newValue: null,
                                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                                for (int i = 0; i < TDETA_HES_SUB.SelectedItems.Count; i++)
                                {
                                    var item = TDETA_HES_SUB.SelectedItems[i];

                                    if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                    {
                                        if (item.GetType().GetProperty("IDD").GetValue(item) is null)
                                        {
                                        }
                                        else
                                        {
                                            var _idd = item.GetType().GetProperty("IDD").GetValue(item);
                                            var _tnumber = item.GetType().GetProperty("TNUMBER").GetValue(item);

                                            try
                                            {
                                                IsDeletedSomething = true;

                                                ESLAH_ROW((int?)_tnumber);

                                                dbms.DoExecuteSQL($@" DELETE FROM dbo.TDETA_HES WHERE IDD = {_idd} ");
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
                DataGridExtension.HandleKeyPress(sender, e, TDETA_HES_SUB);
            }
        }
        private void TDETA_HES_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && TDETA_HES_SUB.SelectedItem != null && TDETA_HES_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (TDETA_HES_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = TDETA_HES_SUB.SelectedIndex;
                }
            }
        }
        private void TDETA_HES_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            int DefVale = 0;
            var OSTAN = ((TDETA_HES)e.Row.Item).OSTANID;

            FrameworkElement element = e.EditingElement;
            ComboBox CityCmb = CL_LMethods.GetVisualChild<ComboBox>(element);
            if (element is ComboBox)
                CityCmb = element as ComboBox;

            if (e.Column.SortMemberPath == "SHAHRID")
            {
                if (!(e.EditingElement is null) && element is ComboBox)
                {
                    if (OSTAN != null)
                    {
                        var FILTERRED_CITY = dbms.DoGetDataSQL<TCOD_CITY>($"SELECT  CITYCODE, CITYNAME, OSCODE FROM TCOD_CITY WHERE (OSCODE ={OSTAN}) ORDER BY CITYNAME").ToList();

                        foreach (var item in FILTERRED_CITY) { item.CITYNAME = item.CITYNAME?.FixPersianChars(); }

                        DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);

                        CityCmb.ItemsSource = FILTERRED_CITY;
                        if (DefVale is 0)
                        {
                            (e.EditingElement as ComboBox).SelectedIndex = 0;
                        }
                        else
                        {
                            (e.EditingElement as ComboBox).SelectedValue = DefVale;
                        }
                    }
                }
            }
            else
            {
                if (!(e.EditingElement is null) && element is ComboBox)
                {
                    if (CityCmb.DisplayMemberPath == "OSNAME")
                    {
                    }
                    else
                    {
                        //CityCmb.ItemsSource = ALL_SHAHR;

                        //if (DefVale is 0)
                        //{
                        //    (e.EditingElement as ComboBox).SelectedIndex = 0;
                        //}
                        //else
                        //{
                        //    (e.EditingElement as ComboBox).SelectedValue = DefVale;
                        //}
                    }
                }
            }
        }
        private void TDETA_HES_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (NowIsReady && TDETA_HES_SUB != null)
            {
                if (TDETA_HES_SUB.Items.Count > 0)
                {
                    if (Keyboard.IsKeyDown(Key.Escape))
                    {
                        return;
                    }

                    #region REFILL_CURRENTS_
                    DataGridColumn col1 = e.Column;
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
                    else
                        ENTERED_VALUE_ROW = TexboVal.Text.Trim();

                    CURRENT_ROW_ITEMS = e.Row.Item as TDETA_HES;
                    #endregion

                    if (e.Column.SortMemberPath == "TNUMBER") //کد حساب
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
                            CURRENT_ROW_ITEMS.TNUMBER = WAS_ROW_ITEM?.TNUMBER;
                            TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                            TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                    }

                    if (e.Column.SortMemberPath == "ROUTE_NAME") //مسیر ویزیت
                    {
                        if (CURRENT_ROW_ITEMS?.TNUMBER > 0)
                        {
                            if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                            {
                                return;
                            }

                            var routeExists = dbms.DoGetDataSQL<int?>($"SELECT COUNT(1) FROM dbo.Visit_route WHERE ROUTE_NAME = N'{ENTERED_VALUE_ROW}'").FirstOrDefault();
                            if (routeExists == null || routeExists == 0)
                            {
                                new Msgwin(false, "مسیر ویزیت وارد شده در سیستم موجود نیست").ShowDialog();
                                TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                return;
                            }

                            //ROUTE_NAME_BeforeUpdate
                            var RST2 = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>("SELECT * FROM VISIT_ROUTE_DTL WHERE COUST_NO = '" + N_KOL + "-" + NUMBER + "-" + CURRENT_ROW_ITEMS.TNUMBER + "'").ToList();
                            var RST = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>("SELECT * FROM VISIT_ROUTE_DTL where COUST_NO = '" + N_KOL + "-" + NUMBER + "-" + CURRENT_ROW_ITEMS.TNUMBER + "' AND ROUTE_NAME = '" + ENTERED_VALUE_ROW /*ROUTE_NAME*/ + "'").ToList();
                            try
                            {
                                if (RST2.Count > 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين مشتري زير مجموعه مسير ويزيتوري :" + RST2.FirstOrDefault().ROUTE_NAME + " قبلا ثبت شده است آيا مايليد در آن مسير غير فعال شود و به اين مسير اضافه شود؟ ");
                                    msgwin.ShowDialog();

                                    if (msgwin.DialogResult is true)
                                    {
                                        //RST2.Fields("ractive") = false; RST2.update();
                                        dbms.DoExecuteSQL($@"UPDATE dbo.Visit_route_dtl SET RACTIVE = 0 WHERE IDR = {RST2.FirstOrDefault().IDR}");

                                        if (RST.Count > 0)
                                        {
                                            //RST.Fields("ractive") = true; //RST.update();
                                            dbms.DoExecuteSQL($@"UPDATE dbo.Visit_route_dtl SET RACTIVE = 1 WHERE IDR = {RST.FirstOrDefault().IDR}");
                                        }
                                        else
                                        {
                                            //RST.AddNew();
                                            //RST.Fields("ROUTE_NAME") = this.ROUTE_NAME;
                                            var _COUST_NO_ = N_KOL + "-" + NUMBER + "-" + CURRENT_ROW_ITEMS.TNUMBER;
                                            //RST.Fields("RACTIVE") = true;
                                            //RST.update();
                                            dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                                         VALUES(N'{ENTERED_VALUE_ROW}', N'{_COUST_NO_}',1)");
                                        }
                                    }
                                    else
                                    {
                                        TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                    }
                                }
                                else
                                {
                                    //RST2.AddNew();
                                    //RST2.Fields("ROUTE_NAME") = this.ROUTE_NAME;
                                    var COUST_NO = N_KOL + "-" + NUMBER + "-" + CURRENT_ROW_ITEMS.TNUMBER;
                                    //RST2.Fields("RACTIVE") = true;
                                    //RST2.update();
                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                                         VALUES(N'{ENTERED_VALUE_ROW}', N'{COUST_NO}',1)");
                                }
                            }
                            catch (SqlException sqlEx)
                            {
                                // بررسی شماره خطای SQL و نمایش پیام مناسب
                                switch (sqlEx.Number)
                                {
                                    case 2627: // خطای یکتایی
                                        new Msgwin(false, "مسیر ویزیت وارد شده تکراری است. لطفاً اطلاعات را بررسی کنید و مطمئن شوید که این مقدار قبلاً ثبت نشده است").ShowDialog();
                                        break;

                                    case 547: // خطای کلید خارجی
                                        new Msgwin(false, "امکان ثبت اطلاعات وجود ندارد. به نظر می‌رسد که اطلاعات مرتبط با این رکورد در سیستم موجود نیست").ShowDialog();
                                        break;

                                    case 2601: // خطای کلید تکراری
                                        new Msgwin(false, "این مسیر ویزیت قبلاً در سیستم ثبت شده است. لطفاً مقدار جدیدی وارد کنید").ShowDialog();
                                        break;

                                    default: // سایر خطاهای SQL
                                        new Msgwin(false, "خطا در انجام عملیات مسیر ویزیت").ShowDialog();
                                        break;
                                }

                                TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            }
                            catch (Exception ex)
                            {
                                TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);

                                new Msgwin(false, "خطا در انجام عملیات مسیر ویزیت").ShowDialog();
                            }
                        }

                    }
                }
            }
        }
        private void TDETA_HES_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }

            var ROW = e.Row.Item as TDETA_HES;
            if (ConstructorRowDetector.IsPristine(ROW)) { DG_SUB_CANCEL_EDIT(); return; }


            if (!BodyIsValid(ROW))
            {
                DG_SUB_CANCEL_EDIT(e);
                return;
            }



            int? idd = null;
            try
            {
                if (ROW?.IDD is null) //INSERT
                {
                    idd = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.TDETA_HES (N_KOL, NUMBER, TNUMBER, NAME, TOZIH, ADDRESS, TEL, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                                     OUTPUT INSERTED.IDD
                                                     VALUES({N_KOL},
                                                     {NUMBER} ,
                                                     {ROW.TNUMBER} ,
                                                     N'{ROW.NAME.FixPersianChars().Trim()}' ,
                                                     N'{ROW.TOZIH.FixPersianChars()}' ,
                                                     N'{ROW.ADDRESS}' ,
                                                     N'{ROW.TEL}' ,
                                                     N'{ROW.ECODE}' ,
                                                     N'{ROW.PCODE}' ,
                                                     N'{ROW.MCODEM}' ,
                                                     {(ROW.CUST_COD is null ? "NULL" : ROW.CUST_COD)} ,
                                                     N'{ROW.MOBILE}' ,
                                                     N'{ROW.ROUTE_NAME}' ,
                                                     {(ROW.Longitude is null ? "NULL" : ROW.Longitude)} ,
                                                     {(ROW.Latitude is null ? "NULL" : ROW.Latitude)} ,
                                                     {(ROW.OSTANID is null ? "NULL" : ROW.OSTANID)} ,
                                                     {(ROW.SHAHRID is null ? "NULL" : ROW.SHAHRID)} ,
                                                     {Baseknow.USERCOD} ,
                                                     N'{CL_HESABDARI.UCurrentUser()}',
                                                     {(ROW.tob is null ? 1 : ROW.tob)} )").FirstOrDefault();
                }
                else //UPDATE
                {
                    ESLAH_ROW(ROW.TNUMBER);

                    dbms.DoExecuteSQL(@$" UPDATE dbo.TDETA_HES
                                          SET TNUMBER = {ROW.TNUMBER}, NAME = N'{ROW.NAME.FixPersianChars().Trim()}', 
                                          TOZIH = N'{ROW.TOZIH.FixPersianChars()}', ADDRESS = N'{ROW.ADDRESS}', TEL = N'{ROW.TEL}', ECODE = N'{ROW.ECODE}', 
                                          PCODE = N'{ROW.PCODE}', MCODEM = N'{ROW.MCODEM}', 
                                          CUST_COD = {(ROW.CUST_COD is null ? "NULL" : ROW.CUST_COD)},
                                          MOBILE = N'{ROW.MOBILE}', ROUTE_NAME = N'{ROW.ROUTE_NAME}',
                                          Longitude = {(ROW.Longitude is null ? "NULL" : ROW.Longitude)}, 
                                          Latitude = {(ROW.Latitude is null ? "NULL" : ROW.Latitude)},
                                          OSTANID = {(ROW.OSTANID is null ? "NULL" : ROW.OSTANID)}, 
                                          SHAHRID = {(ROW.SHAHRID is null ? "NULL" : ROW.SHAHRID)}, 
                                          tob = {(ROW.tob is null ? 1 : ROW.tob)}
                                          WHERE IDD = {ROW.IDD} ");
                }

                Form_AfterUpdate(ROW?.TNUMBER ?? 0, WAS_ROW_ITEM?.TNUMBER ?? 0);
            }
            catch (SqlException ex)
            {
                TDETA_HES_SUB_CANCEL_EDIT();

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
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }
            if (idd != null) //So Much Important
            {
                ROW.IDD = idd;
            }

            ROW.N_KOL = N_KOL;
            ROW.NUMBER = NUMBER;


            ChangeIsHappend = false;
            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void DG_SUB_CANCEL_EDIT(DataGridRowEditEndingEventArgs e = null)
        {
            TDETA_HES_SUB.CellEditEnding -= TDETA_HES_SUB_CellEditEnding;
            TDETA_HES_SUB.RowEditEnding -= TDETA_HES_SUB_RowEditEnding;

            if (e != null)
            {
                e.Cancel = true;
                TDETA_HES_SUB.CancelEdit(DataGridEditingUnit.Cell);
            }
            else
            {
                TDETA_HES_SUB.CancelEdit();
            }

            TDETA_HES_SUB.RowEditEnding += TDETA_HES_SUB_RowEditEnding;
            TDETA_HES_SUB.CellEditEnding += TDETA_HES_SUB_CellEditEnding;
        }

        private void TDETA_HES_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL برای بروز رسانی کارنت رو و کارنت کالمن
                if (!(TDETA_HES_SUB.Items.Count < 1) && !(TDETA_HES_SUB.SelectedItem is null))
                {
                    if (TDETA_HES_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        CURRENT_ROW_INDEX = TDETA_HES_SUB.SelectedIndex;
                    }
                }
            }
        }
        private void TDETA_HES_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TDETA_HES_SUB.Dispatcher.InvokeAsync(() =>
            {
                TDETA_HES_SUB.CellEditEnding -= TDETA_HES_SUB_CellEditEnding;
                TDETA_HES_SUB.RowEditEnding -= TDETA_HES_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TDETA_HES_SUB.CancelEdit();
                }
                else
                {
                    TDETA_HES_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TDETA_HES_SUB.RowEditEnding += TDETA_HES_SUB_RowEditEnding;
                TDETA_HES_SUB.CellEditEnding += TDETA_HES_SUB_CellEditEnding;
            });
        }
        private void ESLAH_ROW(int? TNUMBER)
        {
            if (TNUMBER is not null)
            {
                CL_HESABDARI.TR("TDETA_HES", "(N_KOL = " + N_KOL + " ) AND (NUMBER = " + NUMBER + " ) AND (TNUMBER = " + TNUMBER + " )", DateTime.Now, 1);
            }
        }
        private bool BodyIsValid(TDETA_HES _row)
        {
            var ROW = _row;

            var errors = (from object i in TDETA_HES_SUB.ItemsSource
                          let c = TDETA_HES_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.TNUMBER.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند خالی باشد" });
            }
            else if (ROW?.TNUMBER <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند صفر یا منفی باشد" });
            }
            else if (!int.TryParse(ROW?.TNUMBER.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد وارد شده در محدوده مجاز نیست" });
            }

            if (string.IsNullOrEmpty(ROW?.NAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام حساب نمی تواند خالی باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.TOZIH) && ROW.TOZIH.Length > 255)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "توضیحات نمی تواند بیشتر از 255 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.ADDRESS) && ROW.ADDRESS.Length > 100)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "آدرس نمی تواند بیشتر از 100 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.TEL) && ROW.TEL.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره تلفن نمی تواند بیشتر از 50 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.ECODE) && ROW.ECODE.Length > 20)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد اقتصادی نمی تواند بیشتر از 20 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.PCODE) && ROW.PCODE.Length > 10)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد پستی نمی تواند بیشتر از 10 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.MOBILE) && ROW.MOBILE.Length > 55)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره موبایل نمی تواند بیشتر از 55 کاراکتر باشد" });
            }

            if (!string.IsNullOrEmpty(ROW?.Longitude.ToStringNullSafe()) && !double.TryParse(ROW?.Longitude.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول جغرافیایی وارد شده در محدوده مجاز نیست" });
            }

            if (!string.IsNullOrEmpty(ROW?.Latitude.ToStringNullSafe()) && !double.TryParse(ROW?.Latitude.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "عرض جغرافیایی وارد شده در محدوده مجاز نیست" });
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
        private void Form_AfterUpdate(int TNUMBER, int TNUMBER_TAG)
        {
            //Form_AfterUpdate
            if (TNUMBER != TNUMBER_TAG)
            {
                // سطح 2 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_TAF2 = " + TNUMBER + " WHERE  (N_KOL2 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER + ") AND  (N_TAF2 = " + TNUMBER_TAG + ")");
                // سطح 3 دريافت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETD SET N_TAF3 = " + TNUMBER + " WHERE  (N_KOL3 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER + ") AND (N_TAF3 = " + TNUMBER_TAG + ")");
                // سطح 2 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_TAF2 = " + TNUMBER + " WHERE  (N_KOL2 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER + ") AND (N_TAF2 = " + TNUMBER_TAG + ")");
                // سطح 3 پرداخت چك
                dbms.DoExecuteSQL("UPDATE  dbo.PAY_GETP SET N_TAF3 = " + TNUMBER + " WHERE  (N_KOL3 = " + N_KOL + ") AND (N_MOIN2 = " + NUMBER + ") AND (N_TAF3 = " + TNUMBER_TAG + ")");

                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_T = " + TNUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "'  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER + " ) AND  (THES_T = " + TNUMBER_TAG + " )  AND  (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_T = " + TNUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(THES_T2 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER + " ) AND  (THES_T = " + TNUMBER_TAG + " )  AND  NOT (THES_T2 IS NULL) AND  (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_T = " + TNUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER + " ) AND  (THES_T = " + TNUMBER_TAG + " )  AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND (THES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_T = " + TNUMBER + " , THES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(THES_T2 AS NVARCHAR) + '-' + CAST(THES_T3 AS NVARCHAR) + '-' + CAST(THES_T4 AS NVARCHAR)  WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER + " ) AND  (THES_T = " + TNUMBER_TAG + " )  AND  NOT (THES_T2 IS NULL) AND  NOT (THES_T3 IS NULL) AND NOT (THES_T4 IS NULL)");
                // سطح 1 دريافت و پرداخت درطرف بستانكار دريافت پرداخت حساب تفصيلي به صورت خودكارآبديت مي شود
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "'  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T = " + TNUMBER + " )  AND  (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(FHES_T2 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T = " + TNUMBER + " )  AND  NOT (FHES_T2 IS NULL) AND  (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T = " + TNUMBER + " )  AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND (FHES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(FHES_T2 AS NVARCHAR) + '-' + CAST(FHES_T3 AS NVARCHAR) + '-' + CAST(FHES_T4 AS NVARCHAR)  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T = " + TNUMBER + " )  AND  NOT (FHES_T2 IS NULL) AND  NOT (FHES_T3 IS NULL) AND NOT (FHES_T4 IS NULL)");
                // درفاكتورها
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE dbo.HEAD_LST.CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER_TAG + "'");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER + ") AND (dbo.GETTAF(CUST_NO) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL))  AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (dbo.GETTAF3(CUST_NO) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)  WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER + ") AND (dbo.GETTAF(CUST_NO) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (dbo.GETTAF4(CUST_NO) IS NULL) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(CUST_NO)+ '-' + dbo.GETTAF3(CUST_NO)+ '-' + dbo.GETTAF4(CUST_NO) WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER + ") AND (dbo.GETTAF(CUST_NO) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(CUST_NO) IS NULL)) AND  (NOT (dbo.GETTAF4(CUST_NO) IS NULL)) AND (NOT (dbo.GETTAF3(CUST_NO) IS NULL))");
                // MOIN_VAR
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE dbo.HEAD_LST.MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER_TAG + "'");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_VAR) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL))  AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (dbo.GETTAF3(MOIN_VAR) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)  WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_VAR) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (dbo.GETTAF4(MOIN_VAR) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_VAR)+ '-' + dbo.GETTAF3(MOIN_VAR)+ '-' + dbo.GETTAF4(MOIN_VAR) WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_VAR) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_VAR) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_VAR) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_VAR) IS NULL))");
                // MOIN_HAV
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE dbo.HEAD_LST.MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER_TAG + "'");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAV) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (dbo.GETTAF3(MOIN_HAV) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)  WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAV) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAV) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAV)+ '-' + dbo.GETTAF3(MOIN_HAV)+ '-' + dbo.GETTAF4(MOIN_HAV) WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAV) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAV) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAV) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAV) IS NULL))");
                // MOIN_HAZ
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE dbo.HEAD_LST.MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER_TAG + "'");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAZ) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL))  AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (dbo.GETTAF3(MOIN_HAZ) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)  WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAZ) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (dbo.GETTAF4(MOIN_HAZ) IS NULL) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(MOIN_HAZ)+ '-' + dbo.GETTAF3(MOIN_HAZ)+ '-' + dbo.GETTAF4(MOIN_HAZ) WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAZ) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(MOIN_HAZ) IS NULL)) AND  (NOT (dbo.GETTAF4(MOIN_HAZ) IS NULL)) AND (NOT (dbo.GETTAF3(MOIN_HAZ) IS NULL))");
                // HMBAA
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET  HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE dbo.HEAD_LST.HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER_TAG + "'");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER + ") AND (dbo.GETTAF(HMBAA) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL))  AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (dbo.GETTAF3(HMBAA) IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)  WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER + ") AND (dbo.GETTAF(HMBAA) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (dbo.GETTAF4(HMBAA) IS NULL) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + dbo.GETTAF2(HMBAA)+ '-' + dbo.GETTAF3(HMBAA)+ '-' + dbo.GETTAF4(HMBAA) WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER + ") AND (dbo.GETTAF(HMBAA) = " + TNUMBER_TAG + ") AND (NOT (dbo.GETTAF2(HMBAA) IS NULL)) AND  (NOT (dbo.GETTAF4(HMBAA) IS NULL)) AND (NOT (dbo.GETTAF3(HMBAA) IS NULL))");
                // در اسناد حسابداري
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "' WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (HES_T = " + TNUMBER + ")  AND  (HES_T2 IS NULL) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(HES_T2 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (HES_T = " + TNUMBER + ")  AND  (NOT (HES_T2 IS NULL)) AND  (HES_T3 IS NULL) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(HES_T2 AS NVARCHAR) + '-'+ CAST(HES_T3 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (HES_T = " + TNUMBER + " ) AND  (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (HES_T4 IS NULL)");
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL SET HES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-' + CAST(HES_T2 AS NVARCHAR) + '-'+  CAST(HES_T3 AS NVARCHAR)+ '-' + CAST(HES_T4 AS NVARCHAR) WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (HES_T = " + TNUMBER + ") AND  (NOT (HES_T2 IS NULL)) AND  (NOT (HES_T3 IS NULL)) AND (NOT (HES_T4 IS NULL))");
            }
        }

        private void MOREINFOBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as TDETA_HES)?.TNUMBER is not null)
                    {
                        var Row = btn.Tag as TDETA_HES;
                        if (Row != null && Row?.IDD > 0)
                        {
                            if (CL_HESABDARI.LETSGO("moreinfo"))
                            {
                                var CUST_NO_HES = Row.N_KOL + "-" + Row.NUMBER + "-" + Row.TNUMBER;
                                new FCODE_CUSTOMER_MORE(CUST_NO_HES).ShowDialog();
                            }
                            else
                            {
                                new Msgwin(false, "دسترسی ندارید").ShowDialog();
                            }
                        }
                    }
                }
            }
        }
        private void SubSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as TDETA_HES)?.IDD is not null)
                    {
                        var Row = btn.Tag as TDETA_HES;
                        if (Row != null && Row?.IDD > 0)
                        {
                            if (Row?.NUMBER != null)
                            {
                                var _HES_ = Row.N_KOL + "-" + Row.NUMBER + "-" + Row.TNUMBER;

                                if (CL_HESABDARI.ISTAF(_HES_))
                                {
                                    var RST = dbms.DoGetDataSQL<string?>("SELECT HES FROM DEED_DTL WHERE HES = '" + _HES_ + "'").ToList();
                                    if (RST.Count > 0)
                                    {
                                        new Msgwin(false, " اين تفصيلي داراي گردش ميباشد و شما نميتوانيد براي آن تفصيلي بالاتر تعريف كنيد در صورت لزوم ابتدا گردشهاي اين حساب را به يك حساب موقت منتقل كنيد و سپس تفصيلي تعريف كنيد").ShowDialog();
                                        return;
                                    }
                                }

                                new TDETA_HES_SHEET2((int)Row.N_KOL, (int)Row.NUMBER, (int)Row.TNUMBER).Show(); //تفضیلی سطح 2
                            }
                        }
                    }
                }
            }

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
            if (!TDETA_HES_DATA.Any())
            {
                return;
            }

            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(TDETA_HES_SUB, "DGExportedExcel");
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
