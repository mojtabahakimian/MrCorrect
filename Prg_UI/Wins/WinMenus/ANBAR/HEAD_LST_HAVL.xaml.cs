using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dapper;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using Wins.WinMenus.KHARID_FORUSH;
using System.ComponentModel;
using Functions;
using static Prg_UI.HelperWins.Msgwin;
using Rpts;
using Wins.WinOther;
using static Interfaces.INavigator;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using System.Windows.Data;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class HEAD_LST_HAVL : Window, ISearchableWindow
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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

        #region MyFunctions
        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        /// <summary>
        /// On DataObject Pasting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Prevent_UnNumberPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        /// <summary>
        /// On Preveiw KeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SpaceRemvo(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// On Preveiw Text Input
        /// </summary>
        /// <param name="TXBX"></param>
        /// <param name="e"></param>
        /// <param name="CanEnterDecimals"></param>
        public void AccepterOnlyNumber(TextBox TXBX, TextCompositionEventArgs e, bool CanEnterDecimals = false)
        {
            if (CanEnterDecimals)
            {
                if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text))
                && (Convert.ToChar(e.TextComposition.Text) != '.'))
                {
                    e.Handled = true;
                }
                // only allow one decimal point
                if ((Convert.ToChar(e.TextComposition.Text) == '.') && (TXBX.Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text)))
                {
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region MODELS
        class CMB_TAH
        {
            public string TAH { get; set; }
        }
        class CMB_MOLAH
        {
            public string MOLAH { get; set; }
        }
        public class Custom_VAHEDK
        {
            public int? VAHED { get; set; }
            public string NAMES { get; set; }
            public string CODE { get; set; }
        }
        public class EMZAMODEL
        {
            private byte[] _emza1;
            public byte[] EMZA1 { get => _emza1; set { if (_emza1 == value) return; _emza1 = value; } }

            private byte[] _emza2;
            public byte[] EMZA2 { get => _emza2; set { if (_emza2 == value) return; _emza2 = value; } }

            private byte[] _emza3;
            public byte[] EMZA3 { get => _emza3; set { if (_emza3 == value) return; _emza3 = value; } }

        }
        public class CODE_VAHED_STUFDEF
        {
            public string CODE { get; set; }
            public int? VAHED { get; set; }
        }
        public class HAVL_QRE1
        {
            public double? NUMBER { get; set; }
            public string SHARAYET { get; set; }
        }
        public class HAVL_QRE2
        {
            public double? FNUMCO { get; set; }
            public double? NUMBER { get; set; }
        }
        public class PRT1
        {
            public double? MABL_F { get; set; }
            public double? B_SEF { get; set; }
        }
        public class PRT2
        {
            public double? MABL { get; set; }
            public double? DATE_N { get; set; }
        }
        #endregion

        public HEAD_LST_HAVL(double? _NUMBER_HAVL = null, bool _isAutomasion_ = false)
        {
            if (_NUMBER_HAVL != null && _NUMBER_HAVL > -1)
            {
                NUMBER_TO_OPEN = (double)_NUMBER_HAVL;
                IsOpenedFromAutomation = _isAutomasion_;
            }
            InitializeComponent();
            this.DataContext = this;
        }
        public bool IsOpenedFromAutomation { get; } = false;
        public ObservableCollection<INVO_LST_FACTOR22> HAVALEH_INVO_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

        List<COMBOPERSONEL> rst_personel = null;

        /// <summary>
        /// یک فیلد استاتیک برای اینکه بتونم آیدی این پنجره رو در دسترس قرار بدم
        /// </summary>
        public static Visual I_AM_HEAD_LST_HAVLAH { get; set; }
        /// <summary>
        /// متغیر برای داشتن شماره حواله ایکه میخاویم داخل این فرم باز کنیم
        /// </summary>
        public double NUMBER_TO_OPEN { get; set; } = -1;
        //برای پیش فرض انبار در مشخصات سیستم
        private int ANBARDefaultValue = 0;
        string BEFOREDATEN = "";
        private bool NowIsReady = false;
        private bool chek;
        private double min;

        //برای ردیابی اینکه آیا دیتاگرید رو سیو نکرده داره میبنده ؟
        public bool IS_SAVED { get; set; } = false;
        public bool ChangeIsHappend { get; set; } = false;

        //جلوگیری از خطا های غیر استاندارد و شناخته نشده

        /// <summary>
        /// متغیری برای فانکشن سکیوریتی که میره چک میکنه آیا دسترسی داره و اگر نداشت این رو فالس میکنه و ایندعه میتونیم از انجا جلو رفتن ش و بگیرم و بببندمش
        /// </summary>
        public bool MAY_OPEN_HAVALE { get; set; } = true;
        public bool CANCEL { get; private set; }
        public int RDD { get; set; }
        public double Meidnum { get; set; }
        /// <summary>
        /// Me.CODE.TAG برای اینکه مقدار قبل از اصلاح در دیتاگرید رو داشته باشیم
        /// </summary>
        public INVO_LST_FACTOR22 WAS_ROW_ITEM { get; set; }
        /// <summary>
        /// برای گرفتن آخرین ستونی که انتخاب شده بوده
        /// </summary>
        public int CURRENT_COLUMN_INDEX { get; set; }
        /// <summary>
        /// برای گرفتن آخرین سطری که انتخاب شده بوده
        /// </summary>
        public int CURRENT_ROW_INDEX { get; set; }
        public string NameOfCurrentColumn { get; set; }
        /// <summary>
        /// تک مقداری که توی سطر الان وارد کرده
        /// </summary>
        public object ENTERED_VALUE_ROW { get; set; }
        /// <summary>
        /// سلول جاری در حال اصلاح
        /// </summary>
        public DataGridCell CURRENT_CELL_ROW { get; set; }
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_HAVL_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
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
        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();

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

                if (Baseknow.UPDDATE ?? false)
                {
                    DATE_N.IsReadOnly = !ican;
                }
                else
                {
                    DATE_N.IsReadOnly = true;
                }

                FNUMCO.IsReadOnly = !ican;
                ANBAR.IsReadOnly = !ican;
                SHARAYET.IsReadOnly = !ican;

                MAS.IsEnabled = ican;
                SADER.IsEnabled = ican;
                TAH.IsEnabled = ican;
                MOLAH.IsEnabled = ican;

                if (!CL_HESABDARI.LETSGO("DEFA"))
                {
                    DEPATMAN.IsEnabled = false;
                }
                else
                {
                    DEPATMAN.IsEnabled = ican;
                }

                CUST_NO.IsEnabled = ican;
                BUTTON_SAVE_HAVALE.IsEnabled = ican;
                DELETE_HAVALE.IsEnabled = ican;

                //if (!CL_HESABDARI.LETSGO("EHANBAR")) //!کلید اصلاح حواله انبار بازرگانی
                //{
                //    ESLAH.IsEnabled = false;
                //}
                //else
                //{
                //    ESLAH.IsEnabled = ican;
                //}

                //TAMIR.UpdateLayout(); var _TAMIR_ = Convert.ToBoolean(TAMIR.IsChecked ?? false); //تایید بارگیری
                if (!_navigationManager.IsNewRecord)
                {
                    INVO_LST_HAVL_SUB.IsReadOnly = !ican;
                }
            }
        }

        public bool NewRecord { get; set; } = false;
        public string rptname { get; set; } = "";
        public long OKDATE { get; private set; }
        public int OKTIME { get; private set; }
        public bool IsDataGrid_SUB_IsFocused { get; private set; }

        private double sum_of_megh_k;
        public double SUM_OF_MEGH_K
        {
            get => HAVALEH_INVO_DATA?.Sum(r => r?.MEGHk) ?? 0;
            set => sum_of_megh_k = value;
        }

        private NavigationManager<HEAD_LST> _navigationManager;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_HEAD_LST_HAVLAH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "HAVL", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = Baseknow.UUSER;

            FILL_ALL_COMBOBOXES();

            byte FTAG = 2;
            string WhereCondition = FTAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) " : "  ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(FTAG, WhereCondition);

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = {FTAG} ";
            }

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
            x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {FTAG}", //On Change for One Record
            Convert.ToDouble(NUMBER_TO_OPEN)
            );


            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            if (!CL_HESABDARI.LETSGO("ESLAHH")) //کلید اصلاح حواله انبار فروش
            {
                this.ESLAH.Visibility = Visibility.Hidden;//false
            }
            else
            {
                this.ESLAH.Visibility = Visibility.Visible;//true;

            }
            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {
                this.JAY.Visibility = Visibility.Visible;
            }
            else
            {
                this.JAY.Visibility = Visibility.Hidden;//false
            }
            if ((bool)Baseknow.SIGN)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
            }

            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";


            GetDefaultFocus();

            CL_LMethods.SetTabIndexes(
                 DATE_N,
                 MAS,
                 TAH,
                 MOLAH,
                 CUST_NO,
                 SHARAYET,
                 BUTTON_SAVE_HAVALE,
                 INVO_LST_HAVL_SUB
                 );
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_HAVL_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (BUTTON_SAVE_HAVALE.IsFocused)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null);
                        return;
                    }
                    else
                    {
                        if (INVO_LST_HAVL_SUB.IsKeyboardFocusWithin)
                        {
                            if (DG.CurrentColumn != null)
                            {
                                int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                                bool isLastColumn = currentColumnIndex == 9;
                                bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                                if (isLastColumn)
                                {
                                    // If it's the last column, move focus to the first cell of next row
                                    if (isLastRow)
                                    {
                                        // Add focus to new row if needed
                                        DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[INVO_LST_SUB_DEF_INDEX_COL]);

                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            DG.BeginEdit();
                                        }), DispatcherPriority.Background);

                                        //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                        var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                        if (focusedWindow != null)
                                        {
                                            Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                focusedWindow.Activate();
                                                focusedWindow.Focus();
                                            }), DispatcherPriority.Background);
                                        }
                                        return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                    }
                                }
                            }
                        }
                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                }
                else
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G) //Just another method
                    {
                        e.Handled = true; //Mark the event as handled to prevent further processing

                        if (!_navigationManager.IsNewRecord)
                        {
                            DateTime dt = dt = DateTime.Now;
                            CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                            CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);

                            //BUTTON_SAVE_HAVALE_Click(null, null);

                            if (!_navigationManager.IsNewRecord)
                            {
                                OTHER_DTL win = new OTHER_DTL(2, CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle));
                                win.NUMBER = Convert.ToInt64(NUMBER.Text);
                                win.Show();
                            }
                        }
                    }

                    if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        if (IsDataGrid_SUB_IsFocused)
                        {
                            //DELETE_HAVALE_Click(null, null);
                        }
                    }

                }
            }
            catch { /*ignore*/ }

            if (!INVO_LST_HAVL_SUB.IsKeyboardFocusWithin && !INVO_LST_HAVL_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }

            ///////در این روش تابع فوکوس توسط سی شارپ اجرا میشود منتها از لحاظ ظاهری در صورت لود مجدد دیتاگرید فوکوس به کار خود ادامه میدهد اما در ظاهر برنامه فوکوس مثلا بالای سطر اول مانده 
            ///////در این حالت فوکوس روی ادیت شده نـــیست 
            ////مشکلی نداره فقط ایندفعه دیگه مثلا تو واحد باید حتما انتخاب کنی یه چیزی تا نمایش بده چون پیریپیر سل اند ادیت اتفاق نمی افتد چون توی حالت ادیت جلو نمیره
            //if (dg != null)
            //{
            //    if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            //    {
            //        //میخوایم بگیم ایندکس آخرین ستون موجود مهم نیست چه ستونیه چون من یکسری ستون های مخفی هم بعد از این ستون ایندکس 8 دارم
            //        //if (dg.Columns.IndexOf(dg.CurrentColumn) == dg.Columns.Count - 1 && dg.SelectedIndex == dg.Items.Count - 2)
            //        if (dg.Columns.IndexOf(dg.CurrentColumn) == 8 && dg.SelectedIndex == dg.Items.Count - 2)
            //        {
            //            //change the selected item to the last row
            //            dg.SelectedItem = dg.Items[dg.Items.Count - 1];

            //            //change the current cell to the first cell in the last row
            //            dg.CurrentCell = new DataGridCellInfo(dg.SelectedItem, dg.Columns[0]);
            //        }
            //        else
            //        {
            //            uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //        }
            //        e.Handled = true;
            //        //dg.BeginEdit();
            //    }
            //}

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
                if (focused != null && (IsInside<TextBoxBase>(focused) || IsInside<ComboBox>(focused) || IsInside<CheckBox>(focused)))
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

        private void OnCurrentRecordChanged(HEAD_LST QRE_HED)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
            }
            else if (QRE_HED == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                NUMBER.Text = QRE_HED.NUMBER.ToString();
                NUMBER.UpdateLayout();
                //HEAD_TOP

                string thevalue = QRE_HED.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();
                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue) && data != null)
                {
                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                }
                if (data != null)
                {
                    CUST_NO.SelectedValue = QRE_HED.CUST_NO; //مشتری
                    CUST_NO.Items.Refresh();
                }

                DATE_N.Text = QRE_HED.DATE_N.ToString();
                FNUMCO.Text = QRE_HED?.FNUMCO?.ToString(); // شماره داخلی
                MAS.SelectedValue = QRE_HED.MAS; // مقصد - کمبوباکس
                SADER.SelectedValue = QRE_HED.SADER; // نوع فروش
                TAH.Text = QRE_HED.TAH;
                MOLAH.Text = QRE_HED.MOLAH; // توسط
                ANBAR.Text = QRE_HED.ANBAR.ToString();// شماره برگه
                DEPATMAN.SelectedValue = QRE_HED.DEPATMAN; // واحد 
                SHARAYET.Text = QRE_HED.SHARAYET; // ملاحظات
                USER_NAME.Text = QRE_HED.USER_NAME;
                OKF.IsChecked = Convert.ToBoolean(QRE_HED.OKF);

                if (QRE_HED?.TAMIR != null)
                {
                    TAMIR.IsChecked = Convert.ToBoolean(GetTamirLikeAccess(QRE_HED?.TAMIR.ToString()));
                }
                else
                {
                    TAMIR.IsChecked = false;
                }

                INVO_LST_HAVL_SUB.IsReadOnly = true;

                //DATAGRID_SUB
                ReGetdata();

                //FOOTER
                SGN1.IsChecked = Convert.ToBoolean(QRE_HED.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(QRE_HED.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(QRE_HED.SGN3);

                SGN1usid.Tag = Convert.ToInt32(QRE_HED.sgn1usid);
                SGN2usid.Tag = Convert.ToInt32(QRE_HED.sgn2usid);
                SGN3usid.Tag = Convert.ToInt32(QRE_HED.sgn3usid);

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == QRE_HED?.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == QRE_HED?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == QRE_HED?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                MakeOKFReady();

                Summer();

                Form_Current();
            }
        }
        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = 2").FirstOrDefault();
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
            NewRecord = false;
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = 2").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void Summer()
        {
            Text59.Text = SUM_OF_MEGH_K.ToString();
        }

        public void ANBAR_LOADITEM()
        {
            ////ALL ANBARS:
            //var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR ORDER BY TCOD_ANBAR.CODE").ToList();
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
                if (rst.Count > 0)
                {
                    ANBARDefaultValue = (int)rst.FirstOrDefault();

                    Baseknow.anbardef = ANBARDefaultValue;
                }
                else
                {
                    Baseknow.anbardef = Baseknow.DEFANB;
                }
            }
            else
            {
                Baseknow.anbardef = Baseknow.DEFANB;
            }
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            ANBAR_COL.ItemsSource = ARST;
            //ANBAR_COL.EditingElementStyle.
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع مشتری
            CUST_KIND.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND.DisplayMemberPath = "CUSTKNAME";
            CUST_KIND.SelectedValuePath = "CUST_COD";
            CUST_KIND.SelectedIndex = 0;

            //نام مشتریان
            //try
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
            //}
            //catch (Exception)
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT hes, NAME  FROM CUST_HESAB OPTION (ORDER GROUP, FAST 1)").ToList();
            //}

            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";
            CUST_NO.SelectedItem = null;


            //واحد ها
            DEPATMAN.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectedIndex = 0;
            DEPATMAN.SelectedItem = 0;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;

            ANBAR_LOADITEM();
            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_COL.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //شیفت
            SHIFT.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT.SHIFT_ID, SHIFT.SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            SHIFT.DisplayMemberPath = "SHNAME";
            SHIFT.SelectedValuePath = "SHIFT_ID";

            //مقصد
            MAS.ItemsSource = dbms.DoGetDataSQL<TheMAGHSAD1_1>("SELECT TCOD_CITY.CITYCODE, TCOD_CITY.CITYNAME+N' - '+TCOD_OSTAN.OSNAME AS Expr1 FROM TCOD_CITY INNER JOIN TCOD_OSTAN ON TCOD_CITY.OSCODE=TCOD_OSTAN.OSCODE ORDER BY TCOD_CITY.OSCODE, TCOD_CITY.CITYCODE").ToList();
            MAS.SelectedValuePath = "CITYCODE";
            MAS.DisplayMemberPath = "Expr1";

            //تحویل دهنده
            TAH.ItemsSource = dbms.DoGetDataSQL<CMB_TAH>("SELECT HEAD_LST.TAH FROM HEAD_LST GROUP BY HEAD_LST.TAH ORDER BY HEAD_LST.TAH").ToList();
            TAH.SelectedValuePath = "TAH";
            TAH.DisplayMemberPath = "TAH";

            //توسط
            MOLAH.ItemsSource = dbms.DoGetDataSQL<CMB_MOLAH>("SELECT MOLAH FROM HEAD_LST GROUP BY MOLAH ORDER BY MOLAH").ToList();
            MOLAH.SelectedValuePath = "MOLAH";
            MOLAH.DisplayMemberPath = "MOLAH";

            //نحوه پرداخت و مدت
            MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO UNION SELECT 0, 'آزاد', 0").ToList();
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";

            SADER.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "داخلی" },
                new COMBOYMODEL { ID = 1, NAME = "خارجی" }
            };

            //کبموباکس مجری پرسنل
            string sql = @"
               SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD
               FROM SALA_DTL sd
               LEFT JOIN USER_PERSONEL_ORDER uo 
                    ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
               WHERE sd.ENABL = 0
               ORDER BY
                    CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                    uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);

            //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.SelectedValue = Baseknow.USERCOD;
            //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            //List<INVO_LST_FACTOR22> FRESHLIST = new List<INVO_LST_FACTOR22>();
            //INVO_LST_HAVL_SUB.ItemsSource = FRESHLIST;

        }
        private void GetDefaultFocus()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }
        private void MakeOKFReady()
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                OKF.IsChecked = true;
            }
            else
            {
                OKF.IsChecked = false;
            }
        }
        private void Form_Current()
        {
            bool ghat;
            this.PERSONEL.Visibility = Visibility.Visible;

            var IsExistingInvoice = CL_LMethods.IsNumeric(NUMBER.Text) && NUMBER.Text != "0";

            if (IsExistingInvoice) // Equivalent of Me.NewRecord
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
            }
            else
            {
                if (HAVALEH_INVO_DATA.Count == 0) // Unicode Persian check
                {
                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                }
                else
                {
                    this.AllowDeletions = false;
                }
            }

            //یعنی حواله از قبل ثبت شده باز شده
            if (OKF.IsChecked == true && IsExistingInvoice) // Not (!) NewRecord
            {
                AllowDeletions = false;
                AllowEdits = false;

                this.ESLAH.IsEnabled = true;
            }
            else
            {
                AllowDeletions = true;
                AllowEdits = true;

                this.ESLAH.IsEnabled = false;

            }
            if (this.TAMIR.IsChecked == true || OKF.IsChecked == true)
            {
                this.INVO_LST_HAVL_SUB.IsReadOnly = true;
            }

            if (Baseknow.SIGN ?? false)
            {
                if (this.SGN1.IsChecked == true || this.SGN3.IsChecked == true)
                {
                    this.Command106.IsEnabled = true;
                    this.Command111.IsEnabled = true;
                    if (CL_HESABDARI.LETSGO("BARGI"))
                    {
                        this.Command122.IsEnabled = true;
                    }
                    else
                    {
                        this.Command122.IsEnabled = false;
                    }
                    this.Command123.IsEnabled = true;
                    this.Command124.IsEnabled = true;
                    this.Command125.IsEnabled = true;
                }
                else
                {
                    this.Command106.IsEnabled = false;
                    this.Command111.IsEnabled = false;
                    this.Command122.IsEnabled = false;
                    this.Command123.IsEnabled = false;
                    this.Command124.IsEnabled = false;
                    this.Command125.IsEnabled = false;
                }
            }
            else if (CL_HESABDARI.LETSGO("BARGI"))
            {
                this.Command122.IsEnabled = true;
            }
            else
            {
                this.Command122.IsEnabled = false;
            }


            if (CL_LMethods.IsNumeric(NUMBER.Text) && NUMBER.Text != "0") //NUMBER > 0
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 2, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }

            this.esl.IsChecked = false;
        }
        private void HAVL_BeforeUpdate()
        {
            byte HTAG = 2;
            if (CUST_NO.SelectedValue is null || CUST_NO.SelectedIndex < 0)
            {
                Msgwin msgwin = new Msgwin(false, "مشتری مشخص نشده لطفا اصلاح کنید"); msgwin.ShowDialog();
                CANCEL = true;
                return;
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                Msgwin msgwin = new Msgwin(false, "حساب مشتری مسدود است لطفا با مالی تماس بگیرید"); msgwin.ShowDialog();
                CANCEL = true;
                CUST_NO.SelectedValue = null;
                return;
            }

            if (this.NUMBER.Text == "0")
            {
                var RST1 = dbms.DoGetDataSQL<int?>("SELECT     MAX(ANBAR) AS MaxOfNUMBER FROM dbo.HEAD_LST WHERE     (TAG = 2) AND (SADER = " + this.SADER.SelectedValue + ")").FirstOrDefault();
                if (RST1 is null || RST1 < 0)
                {
                    this.ANBAR.Text = Baseknow.STHFR.ToStringNullSafe();
                }
                else
                {
                    this.ANBAR.Text = Convert.ToString(RST1 + 1);
                }

                int I = 1;
                var RST = dbms.DoGetDataSQL<HAVL_QRE1>("SELECT     TOP 100 PERCENT NUMBER, SHARAYET FROM         dbo.HEAD_LST WHERE     (TAG = 2) ORDER BY NUMBER DESC").ToList();
                if (RST.Count > 0)
                {
                    if (!string.IsNullOrEmpty(RST[I]?.SHARAYET))
                    {
                        if (RST != null && RST[I]?.SHARAYET != null)
                        {
                            int maxLength = RST[I]?.SHARAYET.Length ?? 0;
                            while (I < maxLength && I < 500)
                            {
                                string currentChar = Strings.Mid(RST[I]?.SHARAYET, I + 1, 1);
                                if (currentChar != "!")
                                {
                                    I++;  // Increment only if conditions are met
                                }
                                else
                                {
                                    break;  // Exit if "!" is encountered.
                                }
                            }
                        }
                    }

                    if (I < 500)
                    {
                        if (RST[I]?.SHARAYET == null)
                        {
                            return;
                        }

                        int remainingLength = (int)RST[I]?.SHARAYET?.Length - I;
                        if (remainingLength > 0) // Ensure that the length is not negative
                        {
                            if (IsNull(this.SHARAYET.Text))
                            {
                                this.SHARAYET.Text = Strings.Mid(System.Convert.ToString(RST[I].SHARAYET), I, remainingLength);
                            }
                            else
                            {
                                this.SHARAYET.Text = this.SHARAYET.Text + '\r' + Strings.Mid(System.Convert.ToString(RST[I].SHARAYET), I, remainingLength);
                            }
                        }
                    }
                }
            }
        }

        private void HAVL_AfterUpdate()
        {
            long num = 0;
            if (!IsNull(this.NUMBER.Text) && !IsNull(this.CUST_NO.SelectedValue))
            {
                var RST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("SELECT * FROM HEAD_LST WHERE TAG = 13 and NUMBER =  " + this.NUMBER.Text).FirstOrDefault();
                string where_qre = " WHERE TAG = 13 AND NUMBER =  " + this.NUMBER.Text;
                if (!(RST is null))
                {
                    if (RST.CUST_NO != CUST_NO.SelectedValue.ToString() || this.esl.IsChecked == true)
                    {
                        num = System.Convert.ToInt64(RST.NUMBER);
                        //RST.update;
                        dbms.DoExecuteSQL($" UPDATE HEAD_LST SET CUST_NO = N'{CUST_NO.SelectedValue}' {where_qre} ");

                        CL_HESABDARI.UpdateGHeymat(Convert.ToInt32(NUMBER.Text), 2, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked));

                        AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(Convert.ToInt64(num), Convert.ToInt64(num), false);
                    }
                }
            }
            if (Convert.ToDouble(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 2, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                //this.SGN1.Locked = true;
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
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

        /// <summary>
        ///  تابعی برای قفل کردن فیلد های سربرگ حواله به جز کلید های چاپ - اصلاح - تایید بارگیری
        /// </summary>
        /// <param name="YN"></param>


        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null; PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (PERSONEL.SelectedIndex > -1 && !(PERSONEL.SelectedValue is null))
            {
                Meidnum = CL_HESABDARI.PERSONELUpdate(2, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'حواله شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'");
                universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            else
            {
                //Not in List
                if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده نادیده بگیر
                string personel_tex = ((TextBox)PERSONEL.Template.FindName("PART_EditableTextBox", PERSONEL)).Text;
                //if (Information.IsNumeric(NewData))
                if (int.TryParse(personel_tex, out _))
                {
                    var RST = dbms.DoGetDataSQL<int?>("select idd from sala_dtl where idd = " + personel_tex).FirstOrDefault();
                    if (!(RST is null))
                    {
                        this.PERSONEL.SelectedValue = RST;
                    }
                }
                else
                {
                    //DoCmd.OpenForm("SelectUser", acFormDS, default, "sal_name like N'%" + CODESAL(NewData) + "%' or sal_name like N'%" + CODESAL(Fixp(NewData)) + "%' or sal_name like N'%" + CODESAL(Fixpi(NewData)) + "%'", default, acDialog, 3);
                    SelectUser selectUser = new SelectUser("sal_name like N'%" + CL_HESABDARI.CODESAL(personel_tex) + "%' or sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(personel_tex)) + "%' or sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixpi(personel_tex)) + "%'", new WindowInteropHelper(this).Handle);
                    selectUser.ShowDialog();
                }
            }
        }
        private void ReGetdata()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {
                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
                                                                          dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
                                                                          dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
                                                                          dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
                                                                       FROM   dbo.INVO_LST LEFT OUTER JOIN
                                                                                                dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE
                                                                       WHERE        (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                HAVALEH_INVO_DATA?.Clear();
                foreach (var item in QRE_LST)
                    HAVALEH_INVO_DATA.Add(item);

                //INVO_LST_HAVL_SUB.ItemsSource = HAVALEH_INVO_DATA;


                //Re Focus On Last Row was Focused - 1
                #region Way1
                //INVO_LST_HAVL_SUB.Focus();
                //INVO_LST_HAVL_SUB.SelectedIndex = CURRENT_ROW_INDEX;
                //INVO_LST_HAVL_SUB.CurrentCell = new DataGridCellInfo(INVO_LST_HAVL_SUB.SelectedItem, INVO_LST_HAVL_SUB.Columns[0]);
                #endregion


                //Re Focus On Last Row was Focused - 2
                #region Way2
                if (INVO_LST_HAVL_SUB.Items.Count > 0)
                {
                    return;
                    INVO_LST_HAVL_SUB.Focus();
                    DataGridRow row = INVO_LST_HAVL_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX) as DataGridRow;
                    if (row is null)
                    {
                        object item = INVO_LST_HAVL_SUB.Items[CURRENT_ROW_INDEX];
                        INVO_LST_HAVL_SUB.ScrollIntoView(INVO_LST_HAVL_SUB.Items[CURRENT_ROW_INDEX]);
                        row = (DataGridRow)INVO_LST_HAVL_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
                        INVO_LST_HAVL_SUB.SelectedItem = item;

                        //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                        var col_index = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "CODE").DisplayIndex;
                        DataGridCell cell = CL_LMethods.GetCell(INVO_LST_HAVL_SUB, row, Convert.ToInt32(col_index));
                        if (cell != null)
                            cell.Focus();
                    }
                    else
                    {
                        object item = INVO_LST_HAVL_SUB.Items[CURRENT_ROW_INDEX];
                        INVO_LST_HAVL_SUB.SelectedItem = item;
                        INVO_LST_HAVL_SUB.ScrollIntoView(item);
                        //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                        var col_index = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "CODE").DisplayIndex;
                        DataGridCell cell = CL_LMethods.GetCell(INVO_LST_HAVL_SUB, row, Convert.ToInt32(col_index));
                        if (cell != null)
                            cell.Focus();
                    }
                }
                #endregion

                //Text59.Text = Convert.ToString(AllInvo.Sum(x => x.MABL_K));

            }
        }
        private bool IsNull(object hTAF2)
        {
            if (hTAF2 is null)
            {
                return true;
            }
            if (!(hTAF2 is null))
            {
                return false;
            }
            return true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void DATE_N_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE_N.Text.ToRawTarikh();
        }

        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = BEFOREDATEN;
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    e.Handled = true; //Cancel Leaving Focus
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = BEFOREDATEN;
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        e.Handled = true; //Cancel Leaving Focus
                        return;
                    }
                }
            }
            else
            {
                DATE_N.Text = BEFOREDATEN;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true; //Cancel Leaving Focus
                return;
            }
        }

        private void SADER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                Msgwin msgwin = new Msgwin(false, "نوع حواله بعد از گرفتن شماره برگه قابل تغيير نمي باشد.");
                msgwin.ShowDialog();
                CANCEL = true;
            }
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (CUST_NO.SelectedValue is not null)
            {
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"مشتری نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }

            if (CUST_NO.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    CUST_NO.SelectedValue = null;
                }
                if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.CUST_NO.SelectedValue.ToString())) == false)
                    {
                        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                        msgwin.ShowDialog();
                        CUST_NO.SelectedValue = null;
                    }
                }
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO2.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
        }

        private void JAY_Click(object sender, RoutedEventArgs e)
        {
            //JAY_AfterUpdate
            if (JAY.IsChecked is true)
            {
                dbms.DoExecuteSQL("DELETE FROM INVO_LST  WHERE JAY <> 0 AND TAG = 2 AND NUMBER = " + this.NUMBER.Text);
                var Jrst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text).ToList();
                var RST = dbms.DoGetDataSQL<JAYMD>("SELECT dbo.INVO_LST.VAHED_K, dbo.invo_edam.idd, dbo.INVO_LST.CODE, dbo.invo_edam.VAHED, dbo.invo_edam.MEGHTA, dbo.invo_edam.MEGHJAY, dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.JAY , dbo.INVO_LST.JAYO, dbo.INVO_LST.id FROM dbo.INVO_LST INNER JOIN dbo.invo_edam ON dbo.INVO_LST.id = dbo.invo_edam.idd WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.JAY = 0) AND dbo.INVO_LST.NUMBER = " + this.NUMBER.Text).ToList();
                //while (!RST.EOF)
                for (int i = 0; i < RST.Count;)
                {
                    if (IsNull(RST[i].JAYO))
                    {
                        if (RST[i].MEGHTA > 0 & RST[i].MEGHJAY > 0)
                        {
                            if (RST[i].MEGHk / RST[i].MEGHTA >= 1)
                            {
                                var _megh = (double)(Math.Truncate((double)(RST[i].MEGHk / RST[i].MEGHTA)) * RST[i].MEGHJAY / CL_HESABDARI.GETVAHEDN(RST[i].CODE, (int)RST[i].VAHED));
                                var _meghk = (double)(Math.Truncate((double)(RST[i].MEGHk / RST[i].MEGHTA)) * RST[i].MEGHJAY);

                                dbms.DoExecuteSQL($@"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,JAY,CODE,SANAD_NO,RADIF,VAHED_K,MEGH,MEGHk)
                                                    VALUES ({RST[i].NUMBER},2,{RST[i].ANBAR},{RST[i].id},{RST[i].CODE},0,{RST[i].RADIF + 1},{RST[i].VAHED},{_megh},{_meghk})");
                                //INVO_LST iNVO_LST = new INVO_LST()
                                //{
                                //    NUMBER = (double)RST[i].NUMBER,
                                //    TAG = 2,
                                //    ANBAR = (int)RST[i].ANBAR,
                                //    JAY = RST[i].id,
                                //    CODE = RST[i].CODE,
                                //    SANAD_NO = 0,
                                //    RADIF = RST[i].RADIF + 1,
                                //    VAHED_K = RST[i].VAHED,
                                //    MEGH = (double)(Math.Truncate((double)(RST[i].MEGHk / RST[i].MEGHTA)) * RST[i].MEGHJAY / CL_HESABDARI.GETVAHEDN(RST[i].CODE, (int)RST[i].VAHED)),
                                //    MEGHk = (double)(Math.Truncate((double)(RST[i].MEGHk / RST[i].MEGHTA)) * RST[i].MEGHJAY)
                                //};
                                //dbms.INVO_LST.Add(iNVO_LST);
                                //dbms.SaveChanges();
                                //Jrst.AddNew();
                                //Jrst.update;
                            }
                        }
                    }
                    //i++;//RST.MoveNext();
                }
                var Jrst2 = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text + " ORDER BY CODE, megh DESC").ToList();
                RDD = 1;
                //while (!Jrst2.EOF)
                for (int i = 0; i < Jrst2.Count;)
                {
                    Jrst2[i].RADIF = RDD;
                    RDD = RDD + 1;
                    dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET RADIF = {RDD} WHERE NUMBER = {NUMBER.Text} AND TAG = 2 AND id = {Jrst2[i].id} "); //Jrst2.update();
                    i++;// Jrst2.MoveNext();
                }
            }
            else
            {
                dbms.DoExecuteSQL("DELETE FROM INVO_LST  WHERE JAY <> 0 AND TAG = 2 AND NUMBER = " + this.NUMBER.Text);
            }
            //this.INVO_LST_HAVL_SUB.Requery();
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (SGN1.IsChecked is true || SGN2.IsChecked is true || SGN3.IsChecked is true)
            {
                ////طبق اکسس این خط ها کامنت شد
                //Msgwin msgwin = new Msgwin(false, "اول باید امضا ها را بردارید سپس مجددا تلاش کنید");
                //msgwin.ShowDialog();
                //return;
            }

            DateTime dt = Convert.ToDateTime("#01/01/0001 12:00:00 AM#");
            dt = DateTime.Now;
            CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);
            CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);

            if (TAMIR.IsChecked is true)
            {
                ////طبق اکسس این خط ها کامنت شد
                //Msgwin msgwin = new Msgwin(false, "اول تایید بارگیری را بردارید");
                //msgwin.ShowDialog();
                //if (CL_HESABDARI.LETSGO("BARGI"))
                //{
                //    this.Command122.IsEnabled = true;
                //}
                //return;
            }

            if (!IsNull(NUMBER.Text))
            {
                var RST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("SELECT * FROM HEAD_LST WHERE TAG = 13 and NUMBER =  " + NUMBER.Text).FirstOrDefault();
                //if (RST is null || Strings.Left(System.Convert.ToString(CL_HESABDARI.UCurrentUser()), 10) == (char)1605 + System.Convert.ToString((char)1583) + System.Convert.ToString((char)1740) + System.Convert.ToString((char)1585) + System.Convert.ToString((char)1587) + System.Convert.ToString((char)1740) + System.Convert.ToString((char)1587) + System.Convert.ToString((char)1578) + System.Convert.ToString((char)1605))
                if (RST is null)
                {
                    AllowEdits = true;
                    AllowDeletions = true;
                }
                else
                {
                    if (CL_HESABDARI.Signed(13, Convert.ToInt64(NUMBER.Text)))
                    {
                        Msgwin msgwin = new Msgwin(false, "براي اين حواله فاکتور صادر شده و به امضاء رسيده است .اگر ميخواهيد آنرا اصلاح کنيد بايد امضاء دوم و سوم فاکتور  برداشته شود . به کارتابل ارسال کنيد و براي مدير مالي جهت برداشتن امضا ارسال کنيد");
                        msgwin.ShowDialog();
                    }
                    else
                    {
                        dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                        AllowEdits = true;
                        Msgwin msgwin = new Msgwin(false, " براي اين حواله فاكتور صادر شده است و تغيرات داده شده در فاکتور نيز اعمال ميشود دقت کنيد که بعداز اصلاح حواله , فاکتور را هم کنترل کنيد ....!");
                        msgwin.ShowDialog();
                    }
                }
            }

        }

        private int? GetTamirLikeAccess(string _VALUE_)
        {
            bool _RESULT_ = false;

            if (CL_LMethods.IsNumeric(_VALUE_))
            {
                _RESULT_ = Convert.ToBoolean(Convert.ToInt32(_VALUE_));
            }
            else
            {
                _RESULT_ = Convert.ToBoolean(_VALUE_);
            }

            if (_RESULT_ == true)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        private void Command122_Click(object sender, RoutedEventArgs e)
        {
            //var _SGN1_ = Convert.ToBoolean(SGN1.IsChecked);
            //var _SGN2_ = Convert.ToBoolean(SGN2.IsChecked);
            //var _SGN3_ = Convert.ToBoolean(SGN3.IsChecked);
            //if (_SGN1_ || _SGN2_ || _SGN3_)
            //{
            //    new Msgwin(false, "ابتدا امضا ها را بردارید").ShowDialog();
            //    e.Handled = true;
            //    return;
            //}

            bool _ChangeHappend_ = false;
            DateTime dt;
            dt = DateTime.Now;
            CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
            CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);



            if (TAMIR.IsChecked is true)
            {
                //اجازه نداره تیک بارگیری رو برداره // تيک بارگيري را نتواند بردارد
                if (!CL_HESABDARI.LETSGO("TICBARGILES")) //اجازه دارد تیک را بردارد:
                {

                    TAMIR.IsChecked = false;
                    //var RST = dbms.DoGetDataSQL<HEAD_LST_LOG1>("SELECT * FROM HEAD_LST_LOG WHERE TAGG = 2 AND FIELDNAME = 'BARGIRI'  AND NUMBER =" + NUMBER.Text + " ORDER BY IDD DESC").FirstOrDefault();

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST_LOG(UP_DATE, NUMBER, TAGG, RESERVED, UP_USER_NAME, fieldname, UDATEF)
                                                       VALUES({Tarikh.GetMiladiDateTimeForSQL(false, true)},
                                                       {this.NUMBER.Text} ,
                                                       2 ,
                                                       {Convert.ToByte(TAMIR.IsChecked)} ,
                                                       N'{CL_HESABDARI.UCurrentUser()}' ,
                                                       N'BARGIRILES' ,
                                                       {CL_HESABDARI.FARSIDATE()})");

                    if (TAMIR.IsChecked == true)
                    {
                        INVO_LST_HAVL_SUB.IsReadOnly = true;
                    }
                    else
                    {
                        INVO_LST_HAVL_SUB.IsReadOnly = false;
                    }
                    _ChangeHappend_ = true;
                }
                else
                {
                    universControl.PopNotifyShowUp("شما مجوز لازم برای بــرداشتن تیک بارگیری را ندارید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    return;
                }
            }
            else
            {   //TAMIR = -1 == True
                if (CL_HESABDARI.LETSGO("BARGI")) //اجازه ثبت بارگیری دارد
                {
                    TAMIR.IsChecked = true;

                    DATE_N.Text = CL_HESABDARI.FARSIDATE().ToString();

                    //dbms.DoExecuteSQL("UPDATE HEAD_LST SET TAMIR = -1 WHERE TAG = 2 AND NUMBER = " + NUMBER.Text);

                    //var RST = dbms.DoGetDataSQL<head_lst_log>("SELECT * FROM HEAD_LST_LOG WHERE TAGG = 2 AND FIELDNAME = 'BARGIRI'  AND NUMBER =" + this.NUMBER.Text + " ORDER BY IDD DESC").FirstOrDefault();

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST_LOG(UP_DATE, NUMBER, TAGG, RESERVED, UP_USER_NAME, fieldname, UDATEF)
                                                       VALUES(CAST('{Tarikh.GetMiladiDateTimeForSQL()}' AS DATETIME),
                                                       {NUMBER.Text} ,
                                                       2 ,
                                                       {Convert.ToByte(TAMIR.IsChecked)} ,
                                                       N'{CL_HESABDARI.UCurrentUser()}' ,
                                                       N'BARGIRI' ,
                                                       {CL_HESABDARI.FARSIDATE()})");

                    if (TAMIR.IsChecked == true)
                    {
                        INVO_LST_HAVL_SUB.IsReadOnly = true;
                    }
                    else
                    {
                        INVO_LST_HAVL_SUB.IsReadOnly = false;
                    }
                    _ChangeHappend_ = true;
                }
                else
                {
                    universControl.PopNotifyShowUp("شما مجوز لازم برای ثـــبـــت تیک بارگیری را ندارید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    return;
                }
            }
            if (_ChangeHappend_)
            {
                BUTTON_SAVE_HAVALE_Click(null, null);
            }
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN1.IsChecked);

                SGN1.IsChecked = !SGN_WAS;
                return;
            }

            double MID = 0;
            string SHARH = null;
            //DateTime td;
            string td = "";
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 2);
            if (MID > 0)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد1 " : ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));

                var SAL = DATE_N.Text.ToRawTarikh().Substring(0, 4);
                var MAH = DATE_N.Text.ToRawTarikh().Substring(4, 2);
                var ROOZ = DATE_N.Text.ToRawTarikh().Substring(6, 2);
                var DTEN = $"{SAL}/{MAH}/{ROOZ}";

                SHARH = "'حواله شماره: " + this.NUMBER.Text + " مورخ " + DTEN + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";

                string testqre = $"insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2, GETDATE() ," + Baseknow.USERCOD + " )";

                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2, GETDATE() ," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 2);
                string testqre2 = $"insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد1 " : ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )";
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد1 " : ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
            }
            //this.PERSONEL.Visible = true;
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!Convert.ToBoolean(OKF.IsChecked))
            {
                this.OKF.IsChecked = true;
            }
            if (SGN1.IsChecked == true)
            {
                SGN1usid.Tag = Baseknow.USERCOD;
                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;
            }
            else
            {
                //SGN1usid.Tag = null;
                //SGN1usid.Text = null;
            }

            if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
            {
                this.Command106.IsEnabled = true;
                this.Command111.IsEnabled = true;
                this.Command123.IsEnabled = true;
                this.Command124.IsEnabled = true;
                this.Command125.IsEnabled = true;
                //this.Command122.IsEnabled = true; //تایید بارگیری
            }
            else
            {
                this.Command106.IsEnabled = false;
                this.Command111.IsEnabled = false;
                this.Command123.IsEnabled = false;
                this.Command124.IsEnabled = false;
                this.Command125.IsEnabled = false;
                //this.Command122.IsEnabled = false; //تایید بارگیری
            }

            if ((sender as CheckBox).IsChecked is true) //LOCK EDIT
            {
                //چون امضا شده قفل کن
                AllowEdits = false;

                //دسترسی دکمه اصلاح
                if (CL_HESABDARI.LETSGO("ESLAHH"))
                {
                    ESLAH.Visibility = Visibility.Visible;
                    ESLAH.IsEnabled = true;
                }
                else
                    ESLAH.IsEnabled = false;
            }

            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(SGN1.IsChecked == true, 1, 0) + $" WHERE TAG = 2 AND NUMBER = " + NUMBER.Text);
        }

        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN2.IsChecked);
                SGN2.IsChecked = !SGN_WAS;
                return;
            }

            double MID = 0;
            string SHARH = null;
            string td = "";
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 2);
            if (MID > 0)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد2 " : ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + " WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                var SAL = DATE_N.Text.ToRawTarikh().Substring(0, 4);
                var MAH = DATE_N.Text.ToRawTarikh().Substring(4, 2);
                var ROOZ = DATE_N.Text.ToRawTarikh().Substring(6, 2);
                var DTEN = $"{SAL}/{MAH}/{ROOZ}";

                SHARH = "'حواله شماره: " + this.NUMBER.Text + " مورخ " + DTEN + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2, GETDATE() ," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 2);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد2 " : ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
            }
            //this.PERSONEL.Visible = true;
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!Convert.ToBoolean(OKF.IsChecked))
            {
                this.OKF.IsChecked = true;
            }

            SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;
            if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
            {
                this.Command106.IsEnabled = true;
                this.Command111.IsEnabled = true;
                this.Command123.IsEnabled = true;
                this.Command124.IsEnabled = true;
                this.Command125.IsEnabled = true;
                this.Command122.IsEnabled = true;
            }
            else
            {
                this.Command106.IsEnabled = false;
                this.Command111.IsEnabled = false;
                this.Command123.IsEnabled = false;
                this.Command124.IsEnabled = false;
                this.Command125.IsEnabled = false;
                this.Command122.IsEnabled = false;
            }


            if ((sender as CheckBox).IsChecked is true) //LOCK EDIT
            {
                //چون امضا شده قفل کن
                AllowEdits = false;

                //دسترسی دکمه اصلاح
                if (CL_HESABDARI.LETSGO("ESLAHH"))
                {
                    ESLAH.Visibility = Visibility.Visible;
                    ESLAH.IsEnabled = true;
                }
                else
                    ESLAH.IsEnabled = false;
            }
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(SGN2.IsChecked == true, 1, 0) + $" WHERE TAG = 2 AND NUMBER = " + NUMBER.Text);
        }

        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN3.IsChecked);
                SGN3.IsChecked = !SGN_WAS;
                return;
            }

            double MID = 0;
            string SHARH = null;
            string td = "";
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 2);
            if (MID > 0)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد3 " : ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + " WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                var SAL = DATE_N.Text.ToRawTarikh().Substring(0, 4);
                var MAH = DATE_N.Text.ToRawTarikh().Substring(4, 2);
                var ROOZ = DATE_N.Text.ToRawTarikh().Substring(6, 2);
                var DTEN = $"{SAL}/{MAH}/{ROOZ}";

                SHARH = "'حواله شماره: " + this.NUMBER.Text + " مورخ " + DTEN + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2, GETDATE() ," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 2);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد3 " : ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
            }
            //this.PERSONEL.Visible = true;
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!Convert.ToBoolean(OKF.IsChecked))
            {
                this.OKF.IsChecked = true;
            }
            //this.SGN3usid.SelectedValue = Baseknow.USERCOD;
            SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;
            if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
            {
                this.Command106.IsEnabled = true;
                this.Command111.IsEnabled = true;
                this.Command123.IsEnabled = true;
                this.Command124.IsEnabled = true;
                this.Command125.IsEnabled = true;
                this.Command122.IsEnabled = true;
            }
            else
            {
                this.Command106.IsEnabled = false;
                this.Command111.IsEnabled = false;
                this.Command123.IsEnabled = false;
                this.Command124.IsEnabled = false;
                this.Command125.IsEnabled = false;
                this.Command122.IsEnabled = false;
            }

            if ((sender as CheckBox).IsChecked is true) //LOCK EDIT
            {
                //چون امضا شده قفل کن

                AllowEdits = false;

                //دسترسی دکمه اصلاح
                if (CL_HESABDARI.LETSGO("ESLAHH"))
                {
                    ESLAH.Visibility = Visibility.Visible;
                    ESLAH.IsEnabled = true;
                }
                else
                    ESLAH.IsEnabled = false;
            }
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(SGN3.IsChecked == true, 1, 0) + $" WHERE TAG = 2 AND NUMBER = " + NUMBER.Text);
        }
        private void INVO_LST_HAVL_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(INVO_LST_HAVL_SUB.Items.Count < 1) && !(INVO_LST_HAVL_SUB.SelectedItem is null))
                {
                    if (INVO_LST_HAVL_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        //WAS_ROW_ITEM = (INVO_LST_FACTOR22)INVO_LST_HAVL_SUB.SelectedItem;

                        if (!(INVO_LST_HAVL_SUB.CurrentCell.Column is null))
                            CURRENT_COLUMN_INDEX = INVO_LST_HAVL_SUB.CurrentCell.Column.DisplayIndex;

                        CURRENT_ROW_INDEX = INVO_LST_HAVL_SUB.SelectedIndex;
                    }
                }
            }
        }
        private void MEGH_AfterUpdate(INVO_LST_FACTOR22 CURRENT_ITMES_ROW, int row_index)
        {
            long Temp;
            double MAND;
            if (CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.ANBAR is null || CURRENT_ITMES_ROW.MEGH is null || CURRENT_ITMES_ROW.MEGHk is null)
            {
                return;
            }
            CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
            CURRENT_ITMES_ROW.MEGH_R = CURRENT_ITMES_ROW.MEGHk;
            if (CURRENT_ITMES_ROW.ANBAR != 0)
            {
                CURRENT_ITMES_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITMES_ROW.CODE, Convert.ToInt64(CURRENT_ITMES_ROW.ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);

                var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                if (RST is null)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                }
                else if (Baseknow.RMOG is true && !IsNull(Baseknow.RMOG))
                {
                    var RST1 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").FirstOrDefault();
                    if (!(RST1 is null))
                    {
                        //RST1.Fields("MAND")
                        MAND = (double)RST1;
                        // If Math.Round(rst.Fields("MAND") - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)), 2) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                        if (Math.Round((double)(RST1 - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                            msgwin.ShowDialog();
                            CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                            CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                            CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                            CURRENT_ITMES_ROW.MABL = WAS_ROW_ITEM.MABL;
                            chek = true;
                            var RST1_1 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                            string where_qre = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                            if (!(RST1_1 is null))
                            {
                                //RST1_1.Fields("MOGODI") = MAND;
                                //RST1_1.Fields("MOGODI_A") = 0;
                                dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {MAND} , MOGODI_A = {0} {where_qre} ");
                            }
                        }
                        else
                        {
                            var RST2_1 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                            string where_qre = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                            if (!(RST2_1 is null))
                            {
                                var _MOGODI = MAND - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR));
                                //RST2_1.Fields("MOGODI_A") = 0;
                                //RST2_1.update();
                                dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {_MOGODI} , MOGODI_A = {0} {where_qre} ");
                            }
                        }
                    }
                }
                else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                {
                    if (Math.Round((double)(RST.MOGODI + RST.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                    {
                        // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();

                        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                        CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                        CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                        chek = true;
                    }
                }
                // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                else if (Math.Round((double)(RST.MOGODI + RST.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                {
                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                    CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                    CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                    CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                    CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                    chek = true;
                }
            }
            if (CURRENT_ITMES_ROW.MABL == 0)
            {
                var TheCol = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                var DGCInf = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                //MABL_K.TabStop = true;
                if (!(THECELL is null))
                    THECELL.IsTabStop = true;
            }
            else
            {
                var TheCol = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                var DGCInf = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = false;
                CURRENT_ITMES_ROW.MABL_K = Math.Round((double)(CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk));
            }
            if (CURRENT_ITMES_ROW.N_MOIN != Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100))) * CURRENT_ITMES_ROW.TKHN / 100)))
            {
                CURRENT_ITMES_ROW.N_MOIN = Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100))) * CURRENT_ITMES_ROW.TKHN / 100));
            }
            if (TICMBAA.IsChecked is true)
            {
                var RST = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "'").FirstOrDefault();
                if (!(RST is null))
                {
                    if ((bool)RST.CMBAA)
                    {
                        if (CURRENT_ITMES_ROW.IMBAA != Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - CURRENT_ITMES_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITMES_ROW.CODE.ToString()) / 100)))
                        {
                            CURRENT_ITMES_ROW.IMBAA = Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - CURRENT_ITMES_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITMES_ROW.CODE.ToString()) / 100));
                        }
                    }
                    else if (CURRENT_ITMES_ROW.IMBAA != 0)
                    {
                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult is true)
                        {
                            CURRENT_ITMES_ROW.IMBAA = 0;
                        }
                    }
                }
            }
            else
            {
                CURRENT_ITMES_ROW.IMBAA = 0;
            }
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is HEAD_LST item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER.Equals(Convert.ToDouble(item.NUMBER)));
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
                 new SearchableProperty { DisplayName = "شماره حواله", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                 new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                 new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                 new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                 new SearchableProperty { DisplayName = "تحویل گیرنده", PropertyPath = "TAH", PropertyType = typeof(string) },
                 new SearchableProperty { DisplayName = "تحویل دهنده", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                 new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "SHARAYET", PropertyType = typeof(string) },
                 // Add other searchable properties
            };
        }

        #endregion

        private void INVO_LST_HAVL_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                e.Handled = true;

            if (NowIsReady && e.Key == Key.Delete && DELETE_HAVALE.IsEnabled)
            {
                e.Handled = true;
                DELETE_HAVALE_Click(null, null);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G)
            {
                e.Handled = true;
                if (Convert.ToDouble(NUMBER.Text ?? "0") > 0 && INVO_LST_HAVL_SUB.Items.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از باز کردن پنجره سایر اطلاعات مطمئن هستید؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null);

                        if (!_navigationManager.IsNewRecord)
                        {
                            OTHER_DTL win = new OTHER_DTL(2, CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle));
                            win.NUMBER = Convert.ToInt64(NUMBER.Text);
                            win.Show();
                        }
                    }
                }
            }
        }
        private void INVO_LST_HAVL_SUB_GotFocus(object sender, RoutedEventArgs e)
        {
            string tah_tex = ((TextBox)TAH.Template.FindName("PART_EditableTextBox", TAH)).Text;
            string molah_tex = ((TextBox)MOLAH.Template.FindName("PART_EditableTextBox", MOLAH)).Text;
            string mas_tex = ((TextBox)MAS.Template.FindName("PART_EditableTextBox", MAS)).Text;

            //بررسی کلیه مقادیر این درست انختاب یا وارد شدند یا نه 
            if ((!Tarikh.IsValidedDate(DATE_N.Text.ToRawTarikh()) || CUST_KIND.SelectedIndex == -1 || CUST_NO.SelectedIndex == -1 || DEPATMAN.SelectedIndex == -1))
            {
                universControl.PopNotifyShow("بعضی آیتم های بالای  حواله را  خالی گذاشتید یا درست وارد نکردید لطفا بررسی کنید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }


        }
        private void INVO_LST_HAVL_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_HAVL_SUB.SelectedItem != null && INVO_LST_HAVL_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (INVO_LST_HAVL_SUB.Items.Count > 0)
                {
                    ///WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_HAVL_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                    if (!(INVO_LST_HAVL_SUB.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = INVO_LST_HAVL_SUB.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = INVO_LST_HAVL_SUB.SelectedIndex;
                }
            }
        }
        private void INVO_LST_HAVL_SUB_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            //var NI = new INVO_LST_FACTOR22 { CODE = ANBARDefaultValue };
            //e.NewItem = NI;
        }
        private void INVO_LST_HAVL_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as INVO_LST_FACTOR22;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || CurrentRow?.ANBAR == null || string.IsNullOrEmpty(CurrentRow?.CODE))
            {
                return;
            }

            int? LastSelectedVahed = null; //پیش فرض واحد کالا انتخاب شده از قبل 
            if (CurrentRow?.VAHED_K != null)
            {
                LastSelectedVahed = (int)CurrentRow.VAHED_K;
            }

            if (e.Column.SortMemberPath == "VAHED_K") //اگر کاربر داخل واحد کالا بود
            {
                var COMBOBOX_VAHED_K = e.EditingElement as ComboBox;
                if (COMBOBOX_VAHED_K == null) return;

                // دریافت واحدهای فرعی کالا
                var filteredUnits = dbms.DoGetDataSQL<Custom_VAHEDK>(@$"SELECT DISTINCT VAHED, NAMES
                                                                        FROM (
                                                                            SELECT dbo.TCOD_VAHEDS.CODE AS VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                            FROM dbo.TCOD_VAHEDS
                                                                            INNER JOIN dbo.STUF_DEF ON dbo.TCOD_VAHEDS.CODE = dbo.STUF_DEF.VAHED
                                                                            WHERE dbo.STUF_DEF.CODE = N'{CurrentRow.CODE}'
                                                                            UNION ALL
                                                                            SELECT dbo.MODULE_D.VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                            FROM dbo.MODULE_D
                                                                            INNER JOIN dbo.TCOD_VAHEDS ON dbo.MODULE_D.VAHED = dbo.TCOD_VAHEDS.CODE
                                                                            WHERE dbo.MODULE_D.CODE = N'{CurrentRow.CODE}'
                                                                        ) AS Combined").ToList();

                RST_KALAVAHED_LST = filteredUnits;

                // تنظیم آیتم‌های کمبوباکس
                COMBOBOX_VAHED_K.ItemsSource = RST_KALAVAHED_LST;

                // تنظیم مقدار انتخاب شده
                if (LastSelectedVahed.HasValue)
                {
                    COMBOBOX_VAHED_K.SelectedValue = LastSelectedVahed;
                }
                else if (filteredUnits.Any())
                {
                    COMBOBOX_VAHED_K.SelectedValue = filteredUnits.FirstOrDefault().VAHED;
                }

                // رفرش کردن آیتم‌ها
                COMBOBOX_VAHED_K.Items.Refresh();
            }
        }
        private void INVO_LST_HAVL_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (NowIsReady && INVO_LST_HAVL_SUB != null)
            {
                if (INVO_LST_HAVL_SUB.Items.Count > 0)
                {
                    NameOfCurrentColumn = e.Column.SortMemberPath;

                    DataGridColumn col1 = e.Column;
                    DataGridRow row1 = e.Row;
                    int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
                    int col_index = col1.DisplayIndex;
                    var DGCellinfo = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[col_index]);
                    var CurrentDGCell = CL_LMethods.GetDataGridCell(DGCellinfo);

                    CURRENT_ROW_INDEX = row_index;
                    CURRENT_COLUMN_INDEX = e.Column.DisplayIndex;


                    //CELL
                    var rowContainer = INVO_LST_HAVL_SUB.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
                    DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);

                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    if (cell == null)
                    {
                        INVO_LST_HAVL_SUB.ScrollIntoView(rowContainer, INVO_LST_HAVL_SUB.Columns[CURRENT_COLUMN_INDEX]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    }
                    CURRENT_CELL_ROW = cell;
                    //CELL

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

                    var CURRENT_ITMES_ROW = e.Row.Item as INVO_LST_FACTOR22;




                    // (INVO_LST_HAVL_SUB.Items[row_index] as INVO_LST_FACTOR22)

                    //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MABL_K").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));

                    //انبار
                    if (e.Column.SortMemberPath == "ANBAR")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //(e.EditingElement as ComboBox).SelectedValue = WAS_ROW_ITEM.ANBAR;
                            return;
                        }
                        else
                        {
                            if ((e.Row.Item as INVO_LST_FACTOR22).CODE != null)
                            {
                                var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = N'{(e.Row.Item as INVO_LST_FACTOR22).CODE}' AND ANBAR = {(e.EditingElement as ComboBox).SelectedValue}").ToList();
                                if (Rst1.Count == 0)
                                {
                                    universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                                    (e.Row.Item as INVO_LST_FACTOR22).CODE = WAS_ROW_ITEM.CODE;
                                    (e.Row.Item as INVO_LST_FACTOR22).NAME_CODE = WAS_ROW_ITEM.NAME_CODE;

                                    //CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K

                                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);//Undo and go back to normal

                                }
                            }
                        }
                        ////ANBAR_BeforeUpdate
                        if (CL_HESABDARI.Isbargiried(Convert.ToInt64(NUMBER.Text)))
                        {
                            Msgwin msgwin = new Msgwin(false, "ركورد جاري قابل تغيير نيست زيرا واحد فروش آنرا تائيد نموده است.");
                            msgwin.ShowDialog();
                            CANCEL = true;
                        }
                        ////ANBAR_AfterUpdate
                        if (!(IsNull((e.Row.Item as INVO_LST_FACTOR22).ANBAR) || !string.IsNullOrEmpty((e.Row.Item as INVO_LST_FACTOR22).CODE)))
                        {
                            //------------------
                            //MEGH_AfterUpdate;
                            //if (chek)
                            //{
                            //    this.Undo;
                            //}
                            MEGH_AfterUpdate(CURRENT_ITMES_ROW, row_index);
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }

                    }
                    //نام کالا
                    if (e.Column.SortMemberPath == "NAME_CODE")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //Cleaning
                            CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null)
                        {
                            return;
                        }


                        #region BEFORE_UPDATE_SEARCH_FOR_VALUE_ENTERED
                        //اگر عدد وارد کرده برم سرغ کد کالا
                        if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //اگر کد کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                            var str = $"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {WAS_ROW_ITEM.ANBAR})";
                            var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {WAS_ROW_ITEM.ANBAR})").FirstOrDefault();
                            if (!ReferenceEquals(FoundKala, null))
                            {
                                (e.Row.Item as INVO_LST_FACTOR22).CODE = FoundKala.CODE;

                                CURRENT_ITMES_ROW.NAME_CODE = FoundKala.NAME;
                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);


                                //CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                // INVO_LST_HAVL_SUB_PreparingCellForEdit(null, e.EditingElement);

                                //(e.Row.Item as INVO_LST_FACTOR22).NAME = FoundKala.NAME;
                                //CL_LMethods.CellTexSet(INVO_LST_HAVL_SUB, "NAME", CURRENT_ROW_INDEX, FoundKala.NAME);
                                //CL_LMethods.CellTexSet(INVO_LST_HAVL_SUB, "CODE", CURRENT_ROW_INDEX, FoundKala.CODE);
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.CODE = null;
                                CURRENT_ITMES_ROW.NAME_CODE = null;
                                CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                universControl.PopNotifyShow("چنین کد کالایی وجود ندارد لطفا اصلاح کنید", Pop1, Pop1Text1, Pop_Border1);
                                return;
                            }
                            var test = (e.Row.Item as INVO_LST_FACTOR22);
                        }
                        else
                        {
                            var Test0 = WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe();
                            var Test1 = ENTERED_VALUE_ROW.ToString();
                            //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                            if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim())
                            {
                                //الکی نره روی گات فوکوس دیتاگرید
                                INVO_LST_HAVL_SUB.GotFocus -= INVO_LST_HAVL_SUB_GotFocus;

                                //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                                //OpenSearchKala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.CODE.ToString(), null);
                                if (ENTERED_VALUE_ROW.ToString() == "+")
                                {
                                    SERCHK sERCHK = new SERCHK(I_AM_HEAD_LST_HAVLAH, CURRENT_ITMES_ROW.ANBAR.ToString());
                                    sERCHK.ShowDialog();
                                }
                                else
                                {
                                    CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.ANBAR.ToString(), I_AM_HEAD_LST_HAVLAH);
                                }


                                INVO_LST_HAVL_SUB.GotFocus += INVO_LST_HAVL_SUB_GotFocus;

                                if (FROM_SAERCH_KAL.CODE is null)
                                {
                                    //CURRENT_ITMES_ROW.CODE = null;
                                    //CURRENT_ITMES_ROW.NAME_CODE = null;

                                    //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                    var TheCol = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[TheCol]);
                                    var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                                    TheDGCell_MABL_K.Focus();

                                    CURRENT_ITMES_ROW.CODE = null;
                                    CURRENT_ITMES_ROW.NAME_CODE = null;
                                    CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                    return;
                                }
                                else
                                {
                                    CURRENT_ITMES_ROW.CODE = FROM_SAERCH_KAL.CODE;
                                    CURRENT_ITMES_ROW.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;
                                    //CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                    CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                                    //Cleaning
                                    FROM_SAERCH_KAL.CODE = null;
                                    FROM_SAERCH_KAL.NAME_CODE = null;
                                }
                            }
                        }
                        #endregion

                        #region CODE_AfterUpdate
                        double min;
                        double MAND;

                        var Rst0 = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE,ANBAR,MOGODI_A,MOGODI,MABL_M FROM STUF_STK WHERE CODE = '{CURRENT_ITMES_ROW.CODE}' AND ANBAR = {(e.Row.Item as INVO_LST_FACTOR22).ANBAR}").FirstOrDefault();
                        if (Rst0 is null)
                        {
                            MOGU.Text = "0";
                        }
                        else
                        {
                            MOGU.Text = (Rst0.MOGODI + Rst0.MOGODI_A).ToString();
                        }
                        //var RST = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM dbo.STUF_DEF WHERE CODE = N'{CURRENT_ITMES_ROW.CODE}'").FirstOrDefault();
                        //if (RST is null)
                        //{
                        //}
                        //else
                        //{
                        //    (e.Row.Item as INVO_LST_FACTOR22).VAHED_K = Convert.ToInt32(RST.VAHED);
                        //}
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR != 0)
                        {
                            min = CL_HESABDARI.Getmin((int)(e.Row.Item as INVO_LST_FACTOR22).ANBAR, (string)ENTERED_VALUE_ROW);

                            //if (!this.NewRecord)
                            var IsNewRow = (e.Row.Item as INVO_LST_FACTOR22).id is null or 0;
                            if (!IsNewRow)
                            {
                                var RST1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE,ANBAR,MOGODI_A,MOGODI,MABL_M FROM STUF_STK WHERE CODE = '{CURRENT_ITMES_ROW.CODE}' AND ANBAR = {(e.Row.Item as INVO_LST_FACTOR22).ANBAR}").ToList();
                                if (RST1.Count == 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                    msgwin.ShowDialog();
                                }
                                else if (Baseknow.RMOG is true && !IsNull(Baseknow.RMOG))
                                {
                                    var RST2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + (e.Row.Item as INVO_LST_FACTOR22).ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + (e.Row.Item as INVO_LST_FACTOR22).ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + (e.Row.Item as INVO_LST_FACTOR22).ANBAR + ")").FirstOrDefault();
                                    if (!(RST2 is null))
                                    {
                                        MAND = (double)RST2;
                                        // If Math.Round(rst.Fields("MAND") - (Me.MEGHk), 2) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then

                                        var kala = Math.Round((double)((double)RST2 - (e.Row.Item as INVO_LST_FACTOR22).MEGHk), (int)Baseknow.DIG);

                                        var test = Math.Round(min, (int)Baseknow.DIG);

                                        if (Math.Round((double)((double)RST2 - (e.Row.Item as INVO_LST_FACTOR22).MEGHk), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && (e.Row.Item as INVO_LST_FACTOR22).ANBAR != 0 && Baseknow.MOJU)
                                        {
                                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                            msgwin.ShowDialog();

                                            INVO_LST_SUB_CANCEL_EDIT();
                                            //CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                            //INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                            chek = true;
                                            //return;
                                        }
                                        else
                                        {
                                            //#Check Matter
                                            //var RST3 = dbms.DoGetDataSQL<STUF_FSK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + (e.Row.Item as INVO_LST_FACTOR22).ANBAR).FirstOrDefault();
                                            //string where_qre = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + (e.Row.Item as INVO_LST_FACTOR22).ANBAR;
                                            //if (!(RST3 is null))
                                            //{
                                            //    dbms.DoExecuteSQL($"UPDATE dbo.STUF_FSK SET MOGODI = {MAND - (e.Row.Item as INVO_LST_FACTOR22).MEGHk} , MOGODI_A = {0} {where_qre} ");
                                            //}
                                        }
                                    }
                                }
                                //else if (this.CODE == this.CODE.TAG)
                                else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                                {
                                    // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                    if (Math.Round((double)(Rst0.MOGODI + Rst0.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();

                                        INVO_LST_SUB_CANCEL_EDIT();

                                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                        //INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                        chek = true;
                                    }
                                }
                                // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                else if (Math.Round((double)(Rst0.MOGODI + Rst0.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                    msgwin.ShowDialog();
                                    INVO_LST_SUB_CANCEL_EDIT();
                                    CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                    //INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                    chek = true;
                                    //return;
                                }
                            }
                            //VAHED_K_AfterUpdate();
                            #region VAHED_K_AfterUpdate
                            if (CURRENT_ITMES_ROW.MEGH is null || CURRENT_ITMES_ROW.VAHED_K is null)
                            {
                            }
                            else
                            {
                                var RECOMPUTE_MEGHk_K = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                                if (CURRENT_ITMES_ROW.MEGHk != RECOMPUTE_MEGHk_K)
                                {
                                    CURRENT_ITMES_ROW.MEGHk = RECOMPUTE_MEGHk_K;
                                    dbms.DoExecuteSQL($"UPDATE INVO_LST SET MEGHk = {RECOMPUTE_MEGHk_K} WHERE TAG = 2 AND id = {CURRENT_ITMES_ROW.id}");
                                    //DoCmd.RunCommand(acCmdSaveRecord);
                                }
                                MEGH_AfterUpdate(CURRENT_ITMES_ROW, row_index);
                            }
                            #endregion
                        }
                        #endregion
                    }
                    //واحد کالا
                    if (e.Column.SortMemberPath == "VAHED_K")
                    {
                        if ((CURRENT_ITMES_ROW.VAHED_K is null) ||
                            (CURRENT_ITMES_ROW.VAHED_K < 1) ||
                            ((CURRENT_ITMES_ROW.CODE is null))
                            || (CURRENT_ITMES_ROW.CODE is null))
                        {
                            CURRENT_ITMES_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                            INVO_LST_SUB_CANCEL_EDIT();
                            return;
                        }

                        #region VAHED_K_NotInList
                        if (!(CURRENT_ITMES_ROW.VAHED_K is null) && !(CURRENT_ITMES_ROW.VAHED_K < 0))
                        {
                            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").FirstOrDefault();
                            if (RST is null)
                            {
                                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                                msgwin.ShowDialog();
                                CURRENT_ITMES_ROW.VAHED_K = null;
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * RST.NESBAT;
                            }
                        }
                        #endregion

                        if (CURRENT_ITMES_ROW.VAHED_K == null)
                        {
                            return;
                        }

                        #region VAHED_K_AfterUpdate
                        var RECOMPUTE_MEGHk_K = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                        if (CURRENT_ITMES_ROW.MEGHk != RECOMPUTE_MEGHk_K)
                        {
                            CURRENT_ITMES_ROW.MEGHk = RECOMPUTE_MEGHk_K;
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET MEGHk = {RECOMPUTE_MEGHk_K} WHERE TAG = 2 AND id = {CURRENT_ITMES_ROW.id}");
                            //DoCmd.RunCommand(acCmdSaveRecord);
                        }
                        MEGH_AfterUpdate(CURRENT_ITMES_ROW, row_index);
                        #endregion
                    }
                    //مقدار MEGH
                    if (e.Column.SortMemberPath == "MEGH")
                    {
                        if (CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.VAHED_K is null)
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MEGH").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));
                            CURRENT_ITMES_ROW.MEGH = 0;
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null)
                        {
                            return;
                        }
                        else
                        {
                            //LostFocusMegh
                            var tmegh = ((TextBox)e.EditingElement).Text;
                            if (string.IsNullOrEmpty(tmegh.ToStringNullSafe()))
                            {
                                CURRENT_ITMES_ROW.MEGH = 0;
                                tmegh = "0";
                            }
                            if ((e.Row.Item as INVO_LST_FACTOR22).CODE != null)
                            {
                                #region MEGH_AfterUpdate
                                long Temp;
                                double MAND;
                                CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                                CURRENT_ITMES_ROW.MEGH_R = CURRENT_ITMES_ROW.MEGHk;
                                if (CURRENT_ITMES_ROW.ANBAR != 0)
                                {
                                    CURRENT_ITMES_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITMES_ROW.CODE, Convert.ToInt64(CURRENT_ITMES_ROW.ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                                    min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);

                                    var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                                    if (RST is null)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                        msgwin.ShowDialog();
                                    }
                                    else if (Baseknow.RMOG is true && !IsNull(Baseknow.RMOG))
                                    {
                                        var RST1 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").FirstOrDefault();
                                        if (!(RST1 is null))
                                        {
                                            //RST1.Fields("MAND")
                                            MAND = (double)RST1;
                                            // If Math.Round(rst.Fields("MAND") - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)), 2) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                            if (Math.Round((double)(RST1 - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                            {
                                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                                msgwin.ShowDialog();
                                                CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                                CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                                                CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                                                CURRENT_ITMES_ROW.MABL = WAS_ROW_ITEM.MABL;
                                                chek = true;
                                                var RST1_1 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                                                string where_qre = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                                                if (!(RST1_1 is null))
                                                {
                                                    //RST1_1.Fields("MOGODI") = MAND;
                                                    //RST1_1.Fields("MOGODI_A") = 0;
                                                    dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {MAND} , MOGODI_A = {0} {where_qre} ");
                                                }
                                            }
                                            else
                                            {
                                                var RST2_1 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).FirstOrDefault();
                                                string where_qre = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                                                if (!(RST2_1 is null))
                                                {
                                                    var _MOGODI = MAND - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR));
                                                    //RST2_1.Fields("MOGODI_A") = 0;
                                                    //RST2_1.update();
                                                    dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {_MOGODI} , MOGODI_A = {0} {where_qre} ");
                                                }
                                            }
                                        }
                                    }
                                    else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                                    {
                                        if (Math.Round((double)(RST.MOGODI + RST.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                        {
                                            // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                            msgwin.ShowDialog();

                                            CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                            CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                                            CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                                            CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                                            chek = true;
                                        }
                                    }
                                    // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                    else if (Math.Round((double)(RST.MOGODI + RST.MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                                        CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                                        CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                                        chek = true;
                                    }
                                }
                                if (CURRENT_ITMES_ROW.MABL == 0)
                                {
                                    var TheCol = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[TheCol]);
                                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                                    //MABL_K.TabStop = true;
                                    if (!(THECELL is null))
                                        THECELL.IsTabStop = true;
                                }
                                else
                                {
                                    var TheCol = INVO_LST_HAVL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_HAVL_SUB.Items[row_index], INVO_LST_HAVL_SUB.Columns[TheCol]);
                                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                                    if (!(THECELL is null))
                                        THECELL.IsTabStop = false;
                                    CURRENT_ITMES_ROW.MABL_K = Math.Round((double)(CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk));
                                }
                                if (CURRENT_ITMES_ROW.N_MOIN != Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100))) * CURRENT_ITMES_ROW.TKHN / 100)))
                                {
                                    CURRENT_ITMES_ROW.N_MOIN = Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - Math.Round((double)(CURRENT_ITMES_ROW.N_KOL * CURRENT_ITMES_ROW.MABL_K / 100))) * CURRENT_ITMES_ROW.TKHN / 100));
                                }
                                if (TICMBAA.IsChecked is true)
                                {
                                    var RST = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "'").FirstOrDefault();
                                    if (!(RST is null))
                                    {
                                        if ((bool)RST.CMBAA)
                                        {
                                            if (CURRENT_ITMES_ROW.IMBAA != Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - CURRENT_ITMES_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITMES_ROW.CODE.ToString()) / 100)))
                                            {
                                                CURRENT_ITMES_ROW.IMBAA = Math.Round((double)((CURRENT_ITMES_ROW.MABL_K - CURRENT_ITMES_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITMES_ROW.CODE.ToString()) / 100));
                                            }
                                        }
                                        else if (CURRENT_ITMES_ROW.IMBAA != 0)
                                        {
                                            Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                            msgwin.ShowDialog();
                                            if (msgwin.DialogResult is true)
                                            {
                                                CURRENT_ITMES_ROW.IMBAA = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    CURRENT_ITMES_ROW.IMBAA = 0;
                                }

                                #endregion
                            }
                        }
                    }
                    //مقدار تحویلی
                    if (e.Column.SortMemberPath == "MEGH_R")
                    {
                        #region BeforeUpdate
                        if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
                        {
                            if (Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToString())) == false)
                            {
                                Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                                CANCEL = true;
                            }
                        }
                        #endregion

                        #region AfterUpdate
                        if (CURRENT_ITMES_ROW.MABL == 0)
                        {
                            CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH_R;
                            CURRENT_ITMES_ROW.MEGH = CURRENT_ITMES_ROW.MEGH_R / CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                            MEGH_AfterUpdate(CURRENT_ITMES_ROW, row_index);
                        }
                        Command106.IsEnabled = true;
                        #endregion
                    }

                    //WAS_ROW_ITEM = (e.Row.Item as INVO_LST_FACTOR22);
                    ChangeIsHappend = true;
                }
            }
        }
        private void INVO_LST_HAVL_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var ROW = e.Row.Item as INVO_LST_FACTOR22;
            if (ConstructorRowDetector.IsPristine(ROW)) { INVO_LST_SUB_CANCEL_EDIT(); return; }


            if (!BodyIsValid(ROW))
            {
                INVO_LST_SUB_CANCEL_EDIT();
                return;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            //اگر نال هست اصلاح کن به صفر
            _ = (ROW.MEGH_MAR is null) ? ROW.MEGH_MAR = 0 : ROW.MEGH_MAR;
            _ = (ROW.MEGH_R is null) ? ROW.MEGH_R = 0 : ROW.MEGH_R;
            _ = (ROW.MABL is null) ? ROW.MABL = 0 : ROW.MABL;
            _ = (ROW.MABL_K is null) ? ROW.MABL_K = 0 : ROW.MABL_K;

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************
            bool CurrentRowisNew = true;
            string _qre = "";
            if (ROW.id is null || ROW.id <= 0) //INSERT
            {
                _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                                OUTPUT INSERTED.id
                                VALUES({NUMBER.Text},2,{ROW.ANBAR},NULL,N'{ROW.CODE}',{ROW.MEGH},{ROW.MEGHk},{ROW.MEGH_MAR},N'{ROW.MANDAH}',{ROW.MABL},{ROW.MABL_K},{Convert.ToByte(ROW.FROM_A)},N'{ROW.N_RASID}',
                                {ROW.MEGH_R},{ROW.SANAD_NO},NULL,{ROW.ANBARF},{ROW.VAHED_K},{ROW.N_KOL},{ROW.N_MOIN},{ROW.N_TAF},{ROW.AVRAGE},{ROW.AVRAGE2},{ROW.IMBAA},{ROW.TOTALARZ},N'{ROW.VISITOR}',{ROW.TKHN},
                                {(ROW.JAY is null ? "NULL" : ROW.JAY)},{(ROW.JAYO is null ? "NULL" : ROW.JAYO)})";

                var (errorMsgs, infoMsgs, invDetails, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { ROW }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);

                if (queryOutputs.Any())
                {
                    ROW.id = queryOutputs.FirstOrDefault(); // Update the list with the new ID
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG=2) FROM dbo.INVO_LST WHERE id = {ROW.id}");
                }
            }
            else //UPDATE
            {
                CurrentRowisNew = false;
                _qre = $@"UPDATE dbo.INVO_LST
                          SET ANBAR={ROW.ANBAR},
                              CODE=N'{ROW.CODE}',	
                              MEGH={ROW.MEGH},
                              MEGHk={ROW.MEGHk},
                              MEGH_MAR={ROW.MEGH_MAR},
                              MANDAH=N'{ROW.MANDAH}',
                              MABL={ROW.MABL},
                              MABL_K={ROW.MABL_K},
                              FROM_A={Convert.ToByte(ROW.FROM_A)},
                              N_RASID=N'{ROW.N_RASID}',
                              MEGH_R={ROW.MEGH_R},
                              SANAD_NO={ROW.SANAD_NO},
                              ANBARF={ROW.ANBARF},
                              VAHED_K={ROW.VAHED_K},
                              N_KOL={ROW.N_KOL},
                              N_MOIN={ROW.N_MOIN},
                              N_TAF={ROW.N_TAF},
                              AVRAGE={ROW.AVRAGE},
                              AVRAGE2={ROW.AVRAGE2},
                              IMBAA={ROW.IMBAA},
                              TOTALARZ={ROW.TOTALARZ},
                              VISITOR=N'{ROW.VISITOR}',
                              TKHN={ROW.TKHN},
                              JAY={(ROW.JAY is null ? "NULL" : ROW.JAY)},
                              JAYO={(ROW.JAYO is null ? "NULL" : ROW.JAYO)}	
                            WHERE id = {ROW.id} AND TAG = 2";

                var (errorMsgs, _, _, _) = IVM.CheckInventoryAndExecuteQuery<int>(new List<object> { ROW }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);
            }

            // Handle error messages for row
            if (ErrosMessages.Any())
            {
                IVM.RollbackTransaction(); // Rollback the transaction if there are errors
                if (CurrentRowisNew)
                {
                    ROW.id = null; //Bring Back to null (New State because of Rollback Transaction)
                }
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(ErrosMessages);
                return;
            }
            else
            {
                IVM.CommitTransaction(); // Commit the transaction if no errors
            }
            // End of the transaction again if is disposed before ****************************************************************

            HAVL_AfterUpdate();
            Summer();
            ChangeIsHappend = false;
        }

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله
        private int datagridname_tbox_def_index_col;

        private void BUTTON_SAVE_HAVALE_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            if (!DoCmdSaveHeader())
            {
                return;
            }

            INVO_LST_HAVL_SUB.IsReadOnly = false;

            universControl.PopNotifyShow(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            if (HAVALEH_INVO_DATA.Count == 0)
            {
                INVO_LST_HAVL_SUB.Focus();
                var DEFINDX = (INVO_LST_HAVL_SUB.SelectedIndex < 0) ? 0 : INVO_LST_HAVL_SUB.SelectedIndex;
                CL_LMethods.FocusCellReadyToEdit(INVO_LST_HAVL_SUB, "ANBAR", DEFINDX, true);
            }
        }

        private bool DoCmdSaveHeader()
        {
            try
            {
                var _FNUMCO_ = string.IsNullOrEmpty(FNUMCO.Text) ? "NULL" : FNUMCO.Text;

                //Frist New Fresh Insert Add   
                if (_navigationManager.IsNewRecord)
                {
                    double num = 0;
                    using (IDbConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                        {
                            //Fake Query for Lock Table
                            db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                            //Fake Query for Lock Table

                            var rst_11 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=2))", null, transaction).FirstOrDefault();
                            if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                            {
                                num = Baseknow.STHFR;
                                NUMBER.Text = num.ToString();
                                NUMBER.UpdateLayout();
                            }
                            else
                            {
                                num = Convert.ToInt64(rst_11 + 1);
                                NUMBER.Text = num.ToString();
                                NUMBER.UpdateLayout();
                            }
                            string QRE_HEADINSUP = $@"INSERT INTO dbo.HEAD_LST(NUMBER, TAG, ANBAR, DATE_N, TAH, MAS, VAS, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID, sgn1usid, sgn2usid, sgn3usid)
                                              VALUES({num},
                                              	  2 ,
                                              	  {(string.IsNullOrEmpty(ANBAR.Text) ? "0" : ANBAR.Text)}   ,
                                              	  {DATE_N.Text.ToRawTarikh()}   ,
                                              	  N'{TAH.Text}' ,
                                              	  {((MAS.SelectedValue is null) ? "0" : MAS.SelectedValue)} ,
                                              	  0,
                                              	  N'{CUST_NO.SelectedValue}' ,
                                              	  N'{MOLAH.Text}' ,
                                              	  0,
                                              	  0,
                                              	  N'',
                                              	  0,
                                              	  N'',
                                              	  0,
                                              	  N'',
                                              	  0,
                                              	  N'',
                                              	  0,
                                              	  {(_FNUMCO_)} ,
                                              	  {DEPATMAN.SelectedValue},
                                              	  {CL_Generaly.SHIFT_OF_USER},
                                              	  {CUST_KIND.SelectedValue},
                                              	  N'{USER_NAME.Text}',
                                              	  N'{SHARAYET.Text}',
                                              	  {Convert.ToByte(SGN1.IsChecked)},
                                              	  {Convert.ToByte(SGN2.IsChecked)},
                                              	  {Convert.ToByte(SGN3.IsChecked)},
                                              	  NULL,
                                              	  0,
                                              	  N'',
                                              	  {GetTamirLikeAccess(Convert.ToBoolean(TAMIR.IsChecked).ToString())},
                                              	  NULL,
                                              	  NULL,
                                              	  {Convert.ToByte(OKF.IsChecked)},
                                              	  {SADER.SelectedValue},
                                              	  0,
                                              	  0,
                                              	  {Baseknow.dt},
                                              	  {Tarikh.GET_OADATE_DAO()},
                                              	  0,
                                              	  0,
                                              	  NULL,
                                              	  {((MODAT_PPID.SelectedValue is null) ? "NULL" : MODAT_PPID.SelectedValue)}   ,
                                              	  NULL ,
                                              	  NULL   ,
                                              	  {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}   ,
                                              	  {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}   ,
                                              	  {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                                                  )";
                            db.Execute(QRE_HEADINSUP, null, transaction);

                            transaction.Commit();
                            db?.Close();

                            _navigationManager.IsNewRecord = false;
                            RefreshAfterUpdate();
                        }
                    }
                }
                //Update Edit
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET DATE_N={DATE_N.Text.ToRawTarikh()}, FNUMCO={_FNUMCO_}, MAS={(MAS.SelectedValue is null ? "0" : MAS.SelectedValue)}, SADER={SADER.SelectedValue}, TAH=N'{TAH.Text}', MOLAH=N'{MOLAH.Text}', ANBAR={(string.IsNullOrEmpty(ANBAR.Text) ? "0" : ANBAR.Text)}, DEPATMAN={DEPATMAN.SelectedValue}, CUST_NO=N'{CUST_NO.SelectedValue}', SHARAYET=N'{SHARAYET.Text}', OKF={Convert.ToByte(OKF.IsChecked)}, TAMIR={GetTamirLikeAccess(TAMIR.IsChecked.ToString())},SGN1usid={(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)},SGN2usid={(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)},SGN3usid={(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)} WHERE TAG=2 AND NUMBER={NUMBER.Text}");
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره سربرگ حواله").Show();
                return false;
            }

            return true;
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = null;
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = null;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                DATE_N.Text = null;
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (DEPATMAN.SelectedValue is null)  //واحد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد نمیتواند خالی باشد." });
            }
            if (CUST_KIND.SelectedValue is null) //نوع مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع مشتری نمیتواند خالی باشد." });
            }
            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری نمیتواند خالی باشد." });
            }

            if (!string.IsNullOrEmpty(DATE_N.Text?.ToRawTarikh()))
            {
                if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                {
                    //تاریخ صحیح نیست
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ فاکتور را بررسی کنید" });
                }
            }

            if (IsNull(this.CUST_NO.SelectedValue) || this.CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " مشتري مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
            }

            if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToStringNullSafe())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToStringNullSafe())) == false)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!" });
                }
            }

            if (IsNull(this.CUST_KIND.SelectedValue) || CUST_KIND.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع  مشتري مشخص نشده است ....!" });
            }

            if (IsNull(this.DEPATMAN.SelectedValue) || DEPATMAN.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد فروش مشخص نشده است ....!" });
            }

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
        private bool BodyIsValid(INVO_LST_FACTOR22 TheRow)
        {
            var errors = (from object i in INVO_LST_HAVL_SUB.ItemsSource
                          let c = INVO_LST_HAVL_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // Validate ANBAR
            if (!int.TryParse(TheRow.ANBAR?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار صحیح انتخاب نشده" });
            }
            // Validate CODE
            if (string.IsNullOrEmpty(TheRow.CODE) || TheRow.CODE.Length > 15)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.NAME_CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کالا صحیح وارد نشده" });
            }
            // Validate MEGH
            if (!double.TryParse(TheRow.MEGH?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            // Validate MANDAH
            if (TheRow.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (TheRow?.id != null && TheRow?.CODE != null && TheRow?.VAHED_K != null && TheRow?.MABL != null && TheRow?.MEGH != null)
            {
                double nesba;
                //محاسبه نسب باتوجه به واحد و کالا
                nesba = CL_HESABDARI.GETNESBAT(TheRow.CODE, (int)TheRow.VAHED_K);
                //محاسبه و ثبت مجددا مقدار کل
                TheRow.MEGHk = Convert.ToDouble(TheRow.MEGH * nesba);
                TheRow.MABL_K = Math.Round((double)(TheRow.MEGHk * TheRow.MABL));

                //بررسی موجودی و حداقل موجودی
                if (Strings.Mid(Baseknow.OPTIONSS, 59, 1) == "5")
                {
                    TheRow.AVRAGE = CL_HESABDARI.LASTAVRAGE(TheRow.CODE, Convert.ToInt64(TheRow.ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                    min = CL_HESABDARI.Getmin((int)TheRow.ANBAR, TheRow.CODE);
                    var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT CODE, ANBAR, MOGODI_A, MOGODI, MABL_M FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).FirstOrDefault();
                    if (RST is null)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {TheRow.CODE} به انبار فوق تعلق ندارد." });
                    }
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

        private void INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_HAVL_SUB.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_HAVL_SUB.CellEditEnding -= INVO_LST_HAVL_SUB_CellEditEnding;
                INVO_LST_HAVL_SUB.RowEditEnding -= INVO_LST_HAVL_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVO_LST_HAVL_SUB.CancelEdit();
                }
                else
                {
                    INVO_LST_HAVL_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_HAVL_SUB.RowEditEnding += INVO_LST_HAVL_SUB_RowEditEnding;
                INVO_LST_HAVL_SUB.CellEditEnding += INVO_LST_HAVL_SUB_CellEditEnding;
            });
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ANBAR.Text)) { ANBAR.Text = "0"; }
        }

        private void DELETE_HAVALE_Click(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_HAVL_SUB.IsReadOnly || !INVO_LST_HAVL_SUB.IsEnabled)
            {
                return;
            }

            var IsVisible = DELETE_HAVALE.Visibility == Visibility.Visible;
            if (!DELETE_HAVALE.IsEnabled || !IsVisible) { return; }

            if (Convert.ToBoolean(TAMIR.IsChecked))
            {
                new Msgwin(false, "این حواله تایید بارگیری شده , نمیتوان آنرا حذف کرد").ShowDialog();
                return;
            }

            var editableCollectionView = INVO_LST_HAVL_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem) { editableCollectionView.CommitEdit(); }

            //if (INVO_LST_HAVL_SUB.IsEditing()) return;

            _ = AuditLogger.LogActionAsync(
             actionType: "DELETE",
             tableName: "حواله انبار فروش",
             recordId: NUMBER.Text,
             oldValue: "TAG = 2",
             newValue: null,
             additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            if (HAVALEH_INVO_DATA.Count > 0 && INVO_LST_HAVL_SUB.SelectedItem != null)
            {
                if (INVO_LST_HAVL_SUB.SelectedItems is null) return;

                List<MsgModel> ErrosMessages = new List<MsgModel>();
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult != true) return;

                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 2)", dt, 1);

                for (int i = 0; i < INVO_LST_HAVL_SUB.SelectedItems.Count; i++)
                {
                    var item = INVO_LST_HAVL_SUB.SelectedItems[i];

                    if (CL_LMethods.IsNewPlaceHolder(INVO_LST_HAVL_SUB, item))
                    {
                        continue; // Skip deletion for new placeholder items
                    }

                    var _id_ = item.GetType().GetProperty("id").GetValue(item);

                    if (_id_ is null)
                    {
                        HAVALEH_INVO_DATA.Remove(item as INVO_LST_FACTOR22);
                    }
                    else
                    {
                        try
                        {
                            var items = new List<object> { item }; // Wrap the item in a list
                            var (errorMessages, infoMessages, inventoryDetails, queryOutputs) =
                                IVM.CheckInventoryAndExecuteQuery<int>(items, $@"DELETE FROM dbo.INVO_LST WHERE id = {_id_} AND TAG = 2");

                            ErrosMessages.AddRange(errorMessages);
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

                if (ErrosMessages.Any())
                {
                    IVM.ShowErrorMessages(ErrosMessages);
                    //return;
                }
                else
                {
                    ReGetdata();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = 2");

                        _navigationManager.DeleteCurrentRecord(); //Refresh Record Source
                    }
                    catch (SqlException ex)
                    {
                        e.Handled = true;

                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این حواله دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
                else
                {
                    universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
                }
            }
        }

        private void SHARAYET_GotFocus(object sender, RoutedEventArgs e)
        {
            HAVL_BeforeUpdate();
            //HAVL_AfterUpdate();
        }

        /// <summary>
        /// چاپ حواله انبار
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();
            BUTTON_SAVE_HAVALE_Click(null, null);

            if (!!_navigationManager.IsNewRecord)
            {
                ProcLoader.Stop(Prc);
                return;
            }
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            #region SET_IMAGE_IN_REPORT_FROM_BINARY_BYTE_IN_CODE
            //var EMZAHA = dbms.DoGetDataSQL<EMZAMODEL>("SELECT TOP(1) EMZA1,EMZA2,EMZA3  FROM dbo.RASID_ANBAR WHERE TAG = 2 AND NUMBER = 1").FirstOrDefault();
            //////#region FROM_PHISICAL_FILE
            //////StiImage IMG_MYREPORT = report.GetComponents()["Image1v"] as StiImage;
            //////System.Drawing.Image myImage = System.Drawing.Image.FromFile(PATH_FILE);
            //////IMG_MYREPORT.Image = myImage;
            //////#endregion

            //StiImage RPT_SGN1 = report.GetComponents()["SGN1"] as StiImage;
            //StiImage RPT_SGN2 = report.GetComponents()["SGN2"] as StiImage;
            //StiImage RPT_SGN3 = report.GetComponents()["SGN3"] as StiImage;

            //byte[] IMG_ByteArray1 = EMZAHA.EMZA1;
            //byte[] IMG_ByteArray2 = EMZAHA.EMZA2;
            //byte[] IMG_ByteArray3 = EMZAHA.EMZA3;

            ////SGN1
            //using (MemoryStream ms = new MemoryStream(IMG_ByteArray1))
            //{
            //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            //    RPT_SGN1.Image = image;
            //}
            ////SGN2
            //using (MemoryStream ms = new MemoryStream(IMG_ByteArray2))
            //{
            //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            //    RPT_SGN2.Image = image;
            //}
            ////SGN3
            //using (MemoryStream ms = new MemoryStream(IMG_ByteArray3))
            //{
            //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            //    RPT_SGN3.Image = image;
            //}
            #endregion

            double min = 0;
            bool NOTPR = false;
            if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO2.SelectedValue.ToString())) == false)
                {
                    Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                    return;
                }
            }
            //    DoCmd.RunCommand acCmdSaveRecord
            NOTPR = false;
            if (OKF.IsChecked is true)
            {
                if (CL_HESABDARI.LETSGO("CHAPM"))
                {
                    aa(ref min, ref NOTPR, "havlah_anbar", 5);
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where  num = 5 and NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                    if (rst.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "شما اجازه گرفتن بيش از يك چاپ را نداريد"); msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        aa(ref min, ref NOTPR, "havlah_anbar", 5);
                    }
                }
            }
            else
            {
                aa(ref min, ref NOTPR, "havlah_anbar", 5);
            }
            ProcLoader.Stop(Prc);
        }
        /// <summary>
        /// چاپ 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command111_Click(object sender, RoutedEventArgs e)
        {
            BUTTON_SAVE_HAVALE_Click(null, null);

            if (!!_navigationManager.IsNewRecord)
            {
                return;
            }
            double min = 0;
            bool NOTPR = false;
            double JAMFACT = 0;
            if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO2.SelectedValue.ToString())) == false)
                {
                    Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                    return;
                }
            }
            //    DoCmd.RunCommand acCmdSaveRecord
            NOTPR = false;
            if (OKF.IsChecked is true)
            {
                if (CL_HESABDARI.LETSGO("CHAPM"))
                {
                    aa(ref min, ref NOTPR, "havlah_anbar", 6);
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where  num = 5 and NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                    if (rst.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "شما اجازه گرفتن بيش از يك چاپ را نداريد"); msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        aa(ref min, ref NOTPR, "havlah_anbar", 6);
                    }
                }
            }
            else
            {
                aa(ref min, ref NOTPR, "havlah_anbar", 6);
            }
        }
        /// <summary>
        /// چاپ اصلاحیه
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command123_Click(object sender, RoutedEventArgs e)
        {
            BUTTON_SAVE_HAVALE_Click(null, null);

            if (!!_navigationManager.IsNewRecord)
            {
                return;
            }
            string rptname;
            double min = 0;
            bool NOTPR;
            double JAMFACT;
            if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO2.SelectedValue.ToString())) == false)
                {
                    Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                    return;
                }
            }
            //DoCmd.RunCommand(acCmdSaveRecord);
            NOTPR = false;
            if (OKF.IsChecked is true)
            {
                if (CL_HESABDARI.LETSGO("CHAPM"))
                {
                    aa(ref min, ref NOTPR, "havlah_anbar", 8);
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where  num = 5 and NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                    if (rst.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "شما اجازه گرفتن بيش از يك چاپ را نداريد"); msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        aa(ref min, ref NOTPR, "havlah_anbar", 8);
                    }
                }
            }
            else
            {
                aa(ref min, ref NOTPR, "havlah_anbar", 8);
            }

        }
        private void aa(ref double min, ref bool NOTPR, string reportname, byte NUM, bool IsHChap = false)
        {
            if (Baseknow.RMOG is true && !IsNull(Baseknow.RMOG))
            {
                var RST2 = dbms.DoGetDataSQL<INVO_LST>("select * from invo_lst where NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                //while (!RST2.EOF)
                foreach (var RST2EOF in RST2)
                {
                    min = CL_HESABDARI.Getmin(RST2EOF.ANBAR, RST2EOF.CODE);
                    var rstin = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + RST2EOF.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + RST2EOF.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + RST2EOF.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + RST2EOF.ANBAR + ")").FirstOrDefault();
                    if (!(rstin is null))
                    {
                        if (Math.Round((double)rstin, (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && RST2EOF.ANBAR != 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالاي  " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(RST2EOF.CODE)) + " از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.برگه قابل چاپ نيست" + "حداقل موجودي تعريف شده در اف دو :" + min); msgwin.ShowDialog();
                            NOTPR = true;
                        }
                    }
                }//Wend;
            }
            if (NOTPR == false)
            {
                //DoCmd.OpenReport
                if (OKF.IsChecked is true && NUM != 5)
                {
                    Msgwin msgwin = new Msgwin(false, "قبلا اين حواله چاپ گرفته شده است خيلي دقت كنيد كه دوبار بارگيري نشود...!"); msgwin.ShowDialog();
                }
                if (Baseknow.LOCKFAP is true)
                {
                    OKF.IsChecked = true;
                }
                if (Baseknow.OPTIONSS.Substring(9, 1) == "5")
                {
                    rptname = "HAVLAH_ANBAR_" + Baseknow.OPTIONSS.Substring(10, 2);
                }
                else
                {
                    rptname = "HAVLAH_ANBAR";
                }
                //Open The Report
                #region OpenReport
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.{reportname}.mrt");
                report.Load(pathreport);
                ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

                report["NUM_PARAM"] = NUMBER.Text;

                //Report_OnOpen: چاپ  حواله - چاپ 2 - چاپ اصلاحیه
                if (NUM is 5 || NUM is 6 || NUM is 8)
                {
                    if (Strings.Left(Convert.ToString(this.SHARAYET.Text), 1) != "." || this.SHARAYET.Text == "" || IsNull(SHARAYET.Text))
                    {
                        //this.DATE_N.Visible = true;
                        (report.GetComponentByName("TA_DT") as StiText).Enabled = true;
                    }
                    else
                    {
                        // this.DATE_N.Visible = false;
                        (report.GetComponentByName("TA_DT") as StiText).Enabled = false;
                    }
                    var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM         dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").FirstOrDefault();
                    if (!(rst is null))
                        (report.GetComponentByName("vazn") as StiText).Text = "وزن كل به كيلو : " + Strings.Format(Math.Round((double)rst), "#,##");

                    DateTime dt = DateTime.Now;
                    (report.GetComponentByName("zaman") as StiText).Text = $"{Tarikh.SlashyFullDate} - {Tarikh.GetMiladiDateTimeForSQL(true)}";
                }
                //برای چاپ اچ
                if (IsHChap)
                {
                    if (IsNull(Baseknow.ISO_FROOSH) || Strings.Trim(Convert.ToString(Baseknow.ISO_FROOSH)) == "")
                    {
                        (report.GetComponentByName("Label195") as StiText).Enabled = false;
                        //this.Label195.Visible = false;
                    }
                    else
                    {
                        (report.GetComponentByName("Label195") as StiText).Text = $"كد فرم : {Baseknow.ISO_FROOSH}";
                    }
                    var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM         dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").FirstOrDefault();
                    if (!(rst is null))
                        (report.GetComponentByName("vazn") as StiText).Text = "وزن كل به كيلو : " + Strings.Format(Math.Round((double)rst), "#,##");

                    DateTime dt = DateTime.Now;
                    (report.GetComponentByName("zaman") as StiText).Text = $"{Tarikh.SlashyFullDate} - {Tarikh.GetMiladiDateTimeForSQL(true)}";
                }

                //report.Compile();
                //report.Render();
                //report.Show();

                new WINRPT(report, "حواله انبار فروش").Show();
                #endregion
                //rptname =  "NUMBER =" + this.NUMBER.Text + " AND TAG =" + this.HTAG;

                if ((bool)!this.OKF.IsChecked)
                {
                    OKDATE = Baseknow.dt;
                    OKTIME = DateTime.Now.Hour * 10000 + DateTime.Now.Minute * 100 + DateTime.Now.Second;
                }
                if (OKF.IsChecked is true)
                {
                    AllowDeletions = false;
                    AllowEdits = false;
                    //this.AllowEdits = false;
                    //INVO_LST_HAVL_SUB.IsReadOnly = true;
                    //this.INVO_LST_HAVL_SUB.Form.AllowDeletions = false;
                    if (!CL_HESABDARI.LETSGO("EHANBAR"))
                    {
                        ESLAH.IsEnabled = false;
                    }
                }
                if (Baseknow.SMSACT is true || IsNull(Baseknow.SMSACT) is true)
                {
                    //#Check ارسال اس ام اس درست نشده SMS Sned
                    //ersal_sms(this.CUST_NO, CREATE_SMSFR(this.NUMBER.Text), this.NUMBER.Text, 2);
                }
            }
            //var rstOpen = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where NUM = 0 AND NUMBER  = 0").FirstOrDefault();
            dbms.DoExecuteSQL($"UPDATE dbo.CHAPNUM SET NUM={NUM},USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' WHERE NUMBER={NUMBER.Text} AND TAG=2");
            //rst.Fields["NUMBER"] = this.NUMBER.Text;
            //rst.Fields["TAG"] = 2;
            //rst.Fields["NUM"] = 6;
            //rst.Fields["USER_NAME"] = UCurrentUser();
            //rst.update;
        }
        /// <summary>
        /// چاپ H
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command124_Click(object sender, RoutedEventArgs e)
        {
            BUTTON_SAVE_HAVALE_Click(null, null);
            if (!!_navigationManager.IsNewRecord)
            {
                return;
            }
            double min = 0;
            bool NOTPR;
            if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO2.SelectedValue.ToString())) == false)
                {
                    Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                    return;
                }
            }
            //DoCmd.RunCommand(acCmdSaveRecord);
            NOTPR = false;
            if (OKF.IsChecked is true)
            {
                if (CL_HESABDARI.LETSGO("CHAPM"))
                {
                    aa(ref min, ref NOTPR, "HAVLAH_ANBARh", 7, true);
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where  num = 5 and NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                    if (rst.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "شما اجازه گرفتن بيش از يك چاپ را نداريد"); msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        aa(ref min, ref NOTPR, "HAVLAH_ANBARh", 7, true);
                    }
                }
            }
            else
            {
                aa(ref min, ref NOTPR, "HAVLAH_ANBARh", 7, true);
            }
        }
        /// <summary>
        /// چاپ D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command125_Click(object sender, RoutedEventArgs e)
        {
            BUTTON_SAVE_HAVALE_Click(null, null);
            if (!!_navigationManager.IsNewRecord)
            {
                return;
            }
            double min = 0;
            bool NOTPR;
            if (Baseknow.SAGHF is true || Baseknow.SAGHF2 is true)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO2.SelectedValue.ToString())) == false)
                {
                    Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                    return;
                }
            }
            //DoCmd.RunCommand(acCmdSaveRecord);
            NOTPR = false;
            if (OKF.IsChecked is true)
            {
                if (CL_HESABDARI.LETSGO("CHAPM"))
                {
                    aa(ref min, ref NOTPR, "havlah_anbar", 7, false);
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<QRE_22>("select * from chapnum where  num = 5 and NUMBER  = " + NUMBER.Text + " and tag = 2").ToList();
                    if (rst.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "شما اجازه گرفتن بيش از يك چاپ را نداريد"); msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        aa(ref min, ref NOTPR, "havlah_anbar", 7, false);
                    }
                }
            }
            else
            {
                aa(ref min, ref NOTPR, "havlah_anbar", 7, false);
            }
        }

        #region MakingOnlyNumbericFileds
        private void ANBAR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(ANBAR, e);
        }
        private void ANBAR_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void ANBAR_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            Prevent_UnNumberPaste(sender, e);
        }
        #endregion

        private void SGN1usid_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;
        }

        private void SGN2usid_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;
        }

        private void SGN3usid_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;

        }

        private void INVO_LST_HAVL_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null) && INVO_LST_HAVL_SUB.SelectedItem != null)
            {
                if (INVO_LST_HAVL_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_HAVL_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }

        private void FNUMCO_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FNUMCO.Text)) { return; }

            var RST = dbms.DoGetDataSQL<HAVL_QRE2>("SELECT     FNUMCO,NUMBER FROM dbo.HEAD_LST WHERE     (TAG = 2) AND (FNUMCO = " + this.FNUMCO.Text + " )").FirstOrDefault();
            if (!(RST is null))
            {
                if (RST.NUMBER != Convert.ToDouble(NUMBER.Text))
                {
                    Msgwin msgwin = new Msgwin(false, "شماره داخلي تكراري است اين شماره در حواله " + RST.NUMBER + " ثبت شده است!");
                    msgwin.ShowDialog();
                }
            }
        }

        private void BTN_INVOCES_Click(object sender, RoutedEventArgs e)
        {
            new FACTORS_LST(2).Show();
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void BTN_NEW_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeIsHappend)
            {
                ClearFreshAll();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "ذخیره را انجام نداده ای آیا از ادامه مطمئن هستید؟");
                if (msgwin.DialogResult != true)
                {
                    return;
                }
            }
        }

        private void ClearFreshAll()
        {
            //HAVL_OnCurrent_HAVL();

            NUMBER.Text = null; //شماره حواله
            NUMBER.Tag = null; //شماره حواله
            NUMBER.Text = "0"; //شماره حواله

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedValue = null; CUST_NO.Items.Refresh();
            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 

            FNUMCO.Text = "0"; //شماره داخلی
            SADER.SelectedValue = 0; SADER.Items.Refresh();

            TAH.SelectedValue = null; TAH.Items.Refresh(); TAH.Text = null;
            MOLAH.SelectedValue = null; MOLAH.Items.Refresh(); MOLAH.Text = null;
            MAS.SelectedValue = null; MAS.Items.Refresh();
            SHARAYET.Text = null;
            ANBAR.Text = "0"; //شماره برگه

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد

            OKF.IsChecked = false; //تایید فاکتور
            TICMBAA.IsChecked = false; //مالیات ب.ا.ا
            TAMIR.IsChecked = false;
            JAY.IsChecked = false; //جایزه

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOGU.Text = null; //موجودی

            HAVALEH_INVO_DATA?.Clear(); //سایر

            //Command106.IsEnabled = false; // چاپ حواله انبار
            //Command125.IsEnabled = false; //D
            //Command124.IsEnabled = false; //H
            //Command111.IsEnabled = false; //چاپ2
            //Command123.IsEnabled = false; //چاپ اصلاحیه

            DELETE_HAVALE.IsEnabled = false;
            INVO_LST_HAVL_SUB.IsReadOnly = true;

            Form_Current();
            AllowEdits = true;

            TAH.Focus();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ChangeIsHappend = false;
        }

        private void INVO_LST_HAVL_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                IsDataGrid_SUB_IsFocused = false;
            }
            else
            {
                IsDataGrid_SUB_IsFocused = true;
            }
        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_HAVL_SUB.Items.Count > 0)
            {
                if (INVO_LST_HAVL_SUB.SelectedItem is not null)
                {
                    var Row = INVO_LST_HAVL_SUB.SelectedItem as INVO_LST_FACTOR22;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void INVO_LST_sub_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
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
                // Select the row under the mouse
                dataGrid.SelectedItem = row.Item;

                // Show the context menu
                dataGrid.ContextMenu.IsOpen = true;

                // Mark the event as handled to prevent the default context menu behavior
                e.Handled = true;
            }
            else
            {
                // No valid row, don't show context menu
                e.Handled = true;
            }
        }
    }
}
