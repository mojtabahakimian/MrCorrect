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
using static Prg_UI.HelperWins.Msgwin;

namespace Wins.WinMenus.Taarif
{
    public partial class TDETA_HES_SHEET4 : Window
    {
        public TDETA_HES_SHEET4(int _NKOL_, int _NUMBER_, int _TNUMBER_, int _TNUMBER2_, int _TNUMBER3_)
        {
            InitializeComponent();

            N_KOL = _NKOL_;
            NUMBER = _NUMBER_;
            TNUMBER = _TNUMBER_;
            TNUMBER2 = _TNUMBER2_;
            TNUMBER3 = _TNUMBER3_;

            this.DataContext = this;
        }

        public int N_KOL { get; private set; }
        public int NUMBER { get; private set; }
        public int TNUMBER { get; private set; }
        public int TNUMBER2 { get; private set; }
        public int TNUMBER3 { get; private set; }

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

        public ObservableCollection<TDETA_HES4> TDETA_HES4_DATA { get; set; } = new ObservableCollection<TDETA_HES4>();

        public bool TDETA_HES4_SUB_IsFocused { get; private set; }
        public TDETA_HES4? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TDETA_HES4? WAS_ROW_ITEM { get; private set; }
        public int CURRENT_ROW_INDEX { get; set; }
        public Visual I_AM_TDETA_HES4_SHEET { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (TDETA_HES4_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TDETA_HES4_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "TNUMBER4")?.DisplayIndex;
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
            I_AM_TDETA_HES4_SHEET = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();

            ReGetData();

            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = TDETA_HES4_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                try
                {
                    if (uie is DataGridCell || TDETA_HES4_SUB_IsFocused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int DefaultColumnIndex = CL_LMethods.GetLastColumn(TDETA_HES4_SUB).DisplayIndex;
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
                new TobItem { CODE = 2, NAMES = "حقوقی" }
            };

        }
        private ICollectionView DataViewPal;
        private void ReGetData(bool GOTOLAST = false)
        {
            TDETA_HES4_DATA?.Clear();

            var RST = dbms.DoGetDataSQL<TDETA_HES4>($@" SELECT N_KOL, NUMBER, TNUMBER,TNUMBER2,TNUMBER3,TNUMBER4, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, IDD, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, CRT, UID, tob FROM dbo.TDETA_HES4 WHERE (N_KOL = {N_KOL} AND NUMBER = {NUMBER} AND TNUMBER = {TNUMBER} AND TNUMBER2 = {TNUMBER2} AND TNUMBER3 = {TNUMBER3}) ORDER BY TNUMBER4").ToList();
            foreach (var item in RST)
            {
                TDETA_HES4_DATA.Add(item);
            }

            var _DataGrid_ = TDETA_HES4_SUB;
            string _SORTPATH_ = "TNUMBER4";
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

            DataViewPal = CollectionViewSource.GetDefaultView(TDETA_HES4_DATA);
            TDETA_HES4_SUB.ItemsSource = DataViewPal;
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
                    if (obj is TDETA_HES4 model)
                    {
                        return !string.IsNullOrEmpty(model.NAME) &&
                               model.NAME.ToLower().Contains(query);
                    }
                    return false;
                };
            }
            DataViewPal.Refresh();
        }

        private void TDETA_HES4_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TDETA_HES4_SUB_IsFocused = false;
            }
            else
            {
                TDETA_HES4_SUB_IsFocused = true;
            }
        }
        private void TDETA_HES4_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TDETA_HES4_SUB.SelectedItem is not null)
            {
                if (TDETA_HES4_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TDETA_HES4)TDETA_HES4_SUB.SelectedItem).Clone() as TDETA_HES4;
                }
                var editableCollectionView = TDETA_HES4_SUB.Items as IEditableCollectionView;
                if (!editableCollectionView.IsAddingNew) { }
            }
        }
        private void TDETA_HES4_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TDETA_HES4_SUB.Items.Count > 0 && TDETA_HES4_SUB.SelectedItem != null)
                {
                    IEditableCollectionView itemsView = TDETA_HES4_SUB.Items as IEditableCollectionView;
                    if (!itemsView.IsAddingNew && !itemsView.IsEditingItem)
                    {
                        if (!(TDETA_HES4_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {
                                _ = AuditLogger.LogActionAsync(
                                        actionType: "DELETE",
                                        tableName: "سرفصل حسابهاي تفضیلی سطح 4",
                                        recordId: TDETA_HES4_SUB.SelectedItem.ToStringNullSafe(),
                                        oldValue: null,
                                        newValue: null,
                                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                                for (int i = 0; i < TDETA_HES4_SUB.SelectedItems.Count; i++)
                                {
                                    var item = TDETA_HES4_SUB.SelectedItems[i];

                                    if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                    {
                                        if (item.GetType().GetProperty("IDD").GetValue(item) is null)
                                        {
                                        }
                                        else
                                        {
                                            var _idd = item.GetType().GetProperty("IDD").GetValue(item);
                                            var _tnumber2 = item.GetType().GetProperty("TNUMBER2").GetValue(item);
                                            var _tnumber3 = item.GetType().GetProperty("TNUMBER3").GetValue(item);
                                            var _tnumber4 = item.GetType().GetProperty("TNUMBER4").GetValue(item);
                                            var _hes = item.GetType().GetProperty("HESAB").GetValue(item);


                                            var RST = dbms.DoGetDataSQL<string?>("SELECT HES FROM DEED_DTL WHERE HES LIKE '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + _tnumber2 + "-" + _tnumber3 + "-" + _tnumber4 + "%'").ToList();
                                            if (RST.Count > 0)
                                            {
                                                e.Handled = true;

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"حساب داراي گردش مي باشد قابل حذف نيست {_hes}" });
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    IsDeletedSomething = true;

                                                    ESLAH_ROW((int?)_tnumber4);

                                                    dbms.DoExecuteSQL($@" DELETE FROM dbo.TDETA_HES4 WHERE IDD = {_idd} ");
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
                DataGridExtension.HandleKeyPress(sender, e, TDETA_HES4_SUB);
            }
        }
        private void TDETA_HES4_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && TDETA_HES4_SUB.SelectedItem != null && TDETA_HES4_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (TDETA_HES4_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = TDETA_HES4_SUB.SelectedIndex;
                }
            }
        }
        private void TDETA_HES4_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            int DefVale = 0;
            var OSTAN = ((TDETA_HES4)e.Row.Item).OSTANID;

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
                    { }
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
        private void TDETA_HES4_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (NowIsReady && TDETA_HES4_SUB != null)
            {
                if (TDETA_HES4_SUB.Items.Count > 0)
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

                    CURRENT_ROW_ITEMS = e.Row.Item as TDETA_HES4;
                    #endregion

                    if (e.Column.SortMemberPath == "TNUMBER4") //کد حساب
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
                            CURRENT_ROW_ITEMS.TNUMBER4 = WAS_ROW_ITEM?.TNUMBER4;
                            TDETA_HES4_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                            TDETA_HES4_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                    }

                    if (e.Column.SortMemberPath == "ROUTE_NAME") //مسیر ویزیت
                    {
                        if (CURRENT_ROW_ITEMS?.TNUMBER4 > 0)
                        {
                            if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                            {
                                return;
                            }

                            var routeExists = dbms.DoGetDataSQL<int?>($"SELECT COUNT(1) FROM dbo.Visit_route WHERE ROUTE_NAME = N'{ENTERED_VALUE_ROW}'").FirstOrDefault();
                            if (routeExists == null || routeExists == 0)
                            {
                                new Msgwin(false, "مسیر ویزیت وارد شده در سیستم موجود نیست").ShowDialog();
                                TDETA_HES4_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                return;
                            }

                            //ROUTE_NAME_BeforeUpdate
                            var RST2 = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>("SELECT * FROM VISIT_ROUTE_DTL WHERE COUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + CURRENT_ROW_ITEMS.TNUMBER4 + "'").ToList();
                            var RST = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>("SELECT * FROM VISIT_ROUTE_DTL where COUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + CURRENT_ROW_ITEMS.TNUMBER4 + "' AND ROUTE_NAME = '" + ENTERED_VALUE_ROW /*ROUTE_NAME*/ + "'").ToList();
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
                                        var _COUST_NO_ = N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + CURRENT_ROW_ITEMS.TNUMBER4;
                                        //RST.Fields("RACTIVE") = true;
                                        //RST.update();
                                        dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                     VALUES(N'{ENTERED_VALUE_ROW}', N'{_COUST_NO_}',1)");
                                    }
                                }
                                else
                                {
                                    TDETA_HES4_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                }
                            }
                            else
                            {
                                //RST2.AddNew();
                                //RST2.Fields("ROUTE_NAME") = this.ROUTE_NAME;
                                var COUST_NO = N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + CURRENT_ROW_ITEMS.TNUMBER4;
                                //RST2.Fields("RACTIVE") = true;
                                //RST2.update();
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                     VALUES(N'{ENTERED_VALUE_ROW}', N'{COUST_NO}',1)");
                            }
                        }

                    }
                }
            }
        }
        private void TDETA_HES4_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            if (!BodyIsValid(e.Row.Item as TDETA_HES4))
            {
                TDETA_HES4_SUB.CellEditEnding -= TDETA_HES4_SUB_CellEditEnding;
                TDETA_HES4_SUB.RowEditEnding -= TDETA_HES4_SUB_RowEditEnding;

                e.Cancel = true;
                TDETA_HES4_SUB.CancelEdit(DataGridEditingUnit.Cell);

                TDETA_HES4_SUB.RowEditEnding += TDETA_HES4_SUB_RowEditEnding;
                TDETA_HES4_SUB.CellEditEnding += TDETA_HES4_SUB_CellEditEnding;
                return;
            }

            var ROW = e.Row.Item as TDETA_HES4;

            int? idd = null;
            try
            {
                if (ROW?.IDD is null) //INSERT
                {
                    idd = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.TDETA_HES4 (N_KOL, NUMBER, TNUMBER,TNUMBER2,TNUMBER3,TNUMBER4, NAME, TOZIH, ADDRESS, TEL, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                         OUTPUT INSERTED.IDD
                                         VALUES({N_KOL},
                                         {NUMBER} ,
                                         {TNUMBER} ,
                                         {TNUMBER2} ,
                                         {TNUMBER3} ,
                                         {ROW.TNUMBER4} ,
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
                    ESLAH_ROW(ROW.TNUMBER4);

                    dbms.DoExecuteSQL(@$" UPDATE dbo.TDETA_HES4
                      SET TNUMBER4 = {ROW.TNUMBER4}, NAME = N'{ROW.NAME.FixPersianChars().Trim()}', 
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

                Form_AfterUpdate((int)ROW.TNUMBER4, (int)WAS_ROW_ITEM.TNUMBER4);
            }
            catch (SqlException ex)
            {
                TDETA_HES4_SUB_CANCEL_EDIT();

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


            if (idd != null) //So Much Important
            {
                ROW.IDD = idd;
            }

            ROW.N_KOL = N_KOL;
            ROW.NUMBER = NUMBER;
            ROW.TNUMBER = TNUMBER;
            ROW.TNUMBER2 = TNUMBER2;
            ROW.TNUMBER3 = TNUMBER3;

            ChangeIsHappend = false;
            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void TDETA_HES4_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL برای بروز رسانی کارنت رو و کارنت کالمن
                if (!(TDETA_HES4_SUB.Items.Count < 1) && !(TDETA_HES4_SUB.SelectedItem is null))
                {
                    if (TDETA_HES4_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        CURRENT_ROW_INDEX = TDETA_HES4_SUB.SelectedIndex;
                    }
                }
            }
        }
        private void TDETA_HES4_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TDETA_HES4_SUB.Dispatcher.InvokeAsync(() =>
            {
                TDETA_HES4_SUB.CellEditEnding -= TDETA_HES4_SUB_CellEditEnding;
                TDETA_HES4_SUB.RowEditEnding -= TDETA_HES4_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TDETA_HES4_SUB.CancelEdit();
                }
                else
                {
                    TDETA_HES4_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TDETA_HES4_SUB.RowEditEnding += TDETA_HES4_SUB_RowEditEnding;
                TDETA_HES4_SUB.CellEditEnding += TDETA_HES4_SUB_CellEditEnding;
            });
        }
        private void ESLAH_ROW(int? TNUMBER4)
        {
            if (TNUMBER4 is not null)
            {
                //NAME_DblClick
                CL_HESABDARI.TR("TDETA_HES4", "(N_KOL = " + N_KOL + " ) AND (NUMBER = " + NUMBER + " ) AND (TNUMBER = " + TNUMBER + " ) AND (TNUMBER2 = " + TNUMBER2 + ") AND (TNUMBER3 = " + TNUMBER3 + " ) AND (TNUMBER4 = " + TNUMBER4 + " )", DateTime.Now, 1);
            }
        }
        private bool BodyIsValid(TDETA_HES4 _row)
        {
            var ROW = _row;

            var errors = (from object i in TDETA_HES4_SUB.ItemsSource
                          let c = TDETA_HES4_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.TNUMBER4.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند خالی باشد" });
            }
            else if (ROW?.TNUMBER4 <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد حساب نمی تواند صفر یا منفی باشد" });
            }
            else if (!int.TryParse(ROW?.TNUMBER4.ToStringNullSafe(), out _))
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
        private void Form_AfterUpdate(int TNUMBER4, int TNUMBER4_TAG)
        {
            //Form_AfterUpdate
            if (TNUMBER4 != TNUMBER4_TAG)
            {
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  THES_T4 = " + TNUMBER4 + " , THES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE (THES_K = " + N_KOL + " ) AND  (THES_M = " + NUMBER + " ) AND  (THES_T = " + TNUMBER + " ) AND  (THES_T2 = " + TNUMBER2 + " ) AND  (THES_T3 = " + TNUMBER3 + " ) And (THES_T4 = " + TNUMBER4_TAG + ")  ");
                // سطح 1 دريافت و پرداخت
                dbms.DoExecuteSQL("UPDATE  dbo.PGET_LST SET  FHES_T4 = " + TNUMBER4 + " , FHES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "'  WHERE (FHES_K = " + N_KOL + " ) AND  (FHES_M = " + NUMBER + " ) AND  (FHES_T = " + TNUMBER + " ) AND  (FHES_T2 = " + TNUMBER2 + " ) AND  (FHES_T3 = " + TNUMBER3 + " ) And (FHES_T4 = " + TNUMBER4_TAG + ")  ");
                // درفاكتورها
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  CUST_NO = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE     (dbo.GETKOL(CUST_NO) = " + N_KOL + ") AND (dbo.GETMOIN(CUST_NO) = " + NUMBER + ") AND (dbo.GETTAF(CUST_NO) = " + TNUMBER + ") AND (dbo.GETTAF2(CUST_NO) = N'" + TNUMBER2 + "') AND (dbo.GETTAF3(CUST_NO) = N'" + TNUMBER3 + "') AND (dbo.GETTAF4(CUST_NO) = N'" + TNUMBER4_TAG + "')");
                // MOIN_VAR
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_VAR = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE     (dbo.GETKOL(MOIN_VAR) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_VAR) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_VAR) = " + TNUMBER + ") AND (dbo.GETTAF2(MOIN_VAR) = N'" + TNUMBER2 + "') AND (dbo.GETTAF3(MOIN_VAR) = N'" + TNUMBER3 + "') AND (dbo.GETTAF4(MOIN_VAR) = N'" + TNUMBER4_TAG + "')");
                // MOIN_HAV
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAV = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE     (dbo.GETKOL(MOIN_HAV) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAV) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAV) = " + TNUMBER + ") AND (dbo.GETTAF2(MOIN_HAV) = N'" + TNUMBER2 + "') AND (dbo.GETTAF3(MOIN_HAV) = N'" + TNUMBER3 + "') AND (dbo.GETTAF4(MOIN_HAV) = N'" + TNUMBER4_TAG + "')");
                // MOIN_HAZ
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  MOIN_HAZ = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE     (dbo.GETKOL(MOIN_HAZ) = " + N_KOL + ") AND (dbo.GETMOIN(MOIN_HAZ) = " + NUMBER + ") AND (dbo.GETTAF(MOIN_HAZ) = " + TNUMBER + ") AND (dbo.GETTAF2(MOIN_HAZ) = N'" + TNUMBER2 + "') AND (dbo.GETTAF3(MOIN_HAZ) = N'" + TNUMBER3 + "') AND (dbo.GETTAF4(MOIN_HAZ) = N'" + TNUMBER4_TAG + "')");
                // HMBAA
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST Set  HMBAA = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE     (dbo.GETKOL(HMBAA) = " + N_KOL + ") AND (dbo.GETMOIN(HMBAA) = " + NUMBER + ") AND (dbo.GETTAF(HMBAA) = " + TNUMBER + ") AND (dbo.GETTAF2(HMBAA) = N'" + TNUMBER2 + "') AND (dbo.GETTAF3(HMBAA) = N'" + TNUMBER3 + "') AND (dbo.GETTAF4(HMBAA) = N'" + TNUMBER4_TAG + "')");
                // در اسناد حسابداري
                dbms.DoExecuteSQL("UPDATE dbo.DEED_DTL Set HES_T4 = " + TNUMBER4 + " , HES = '" + N_KOL + "-" + NUMBER + "-" + TNUMBER + "-" + TNUMBER2 + "-" + TNUMBER3 + "-" + TNUMBER4 + "' WHERE (HES_K = " + N_KOL + " ) AND  (HES_M = " + NUMBER + " ) AND  (HES_T = " + TNUMBER + " ) AND  (HES_T2 = " + TNUMBER2 + " ) AND  (HES_T3 = " + TNUMBER3 + " )" + " And (HES_T4 = " + TNUMBER4_TAG + ")");
            }
        }
        private void MOREINFOBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as TDETA_HES4)?.TNUMBER2 is not null)
                    {
                        var Row = btn.Tag as TDETA_HES4;
                        if (Row != null && Row?.IDD > 0)
                        {
                            if (CL_HESABDARI.LETSGO("moreinfo"))
                            {
                                var CUST_NO_HES = Row.N_KOL + "-" + Row.NUMBER + "-" + Row.TNUMBER + "-" + Row.TNUMBER2 + "-" + Row.TNUMBER3 + "-" + Row.TNUMBER4;
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
    }
}
