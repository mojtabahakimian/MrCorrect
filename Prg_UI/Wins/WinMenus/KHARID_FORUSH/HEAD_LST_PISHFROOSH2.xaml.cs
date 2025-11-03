using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Rpts;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
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
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using System.Diagnostics;
using System.Data;
using Dapper;
using Functions;
using Wins.WinOther;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using Rpts;
using static Prg_UI.HelperWins.Msgwin;
using Wins.WinMenus.ANBAR;
using System.Windows.Controls.Primitives;
using static Prg_UI.Rpts.Win_INVOICE_PISHFROOSH2;
using Wins.WinMenus.WinAutomasion;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class HEAD_LST_PISHFROOSH2 : Window, ISearchableWindow
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

        #region LOCALMODEL
        public class PISHQ3
        {
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? MEGHk { get; set; }
        }
        public class PISHQ2
        {
            public double? MABL { get; set; }
            public long? DATE_N { get; set; }
        }
        public class PISHQ1
        {
            public double? MABL_F { get; set; }
            public double? B_SEF { get; set; }
        }
        public class CUSTOM_MAX_Number_Mabl
        {
            public double MMABL { get; set; }
            public double MaxOfNUMBER { get; set; }
            public string CODE { get; set; }
        }
        public class VAHEDKTOM
        {
            public string CODE { get; set; }
            public int? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }
        public class CUSTOM_CMAA_CODE
        {
            public Nullable<bool> CMBAA { get; set; }
            public string CODE { get; set; }
        }
        public class Custom5_INVO
        {
            public double MaxOfNUMBER { get; set; }
            public double MABL { get; set; }
        }
        public class Custom4_INVO
        {
            public Nullable<bool> CMBAA { get; set; }
            public string CODE { get; set; }
        }
        public class CUSTOM1_TAKHPERS
        {
            public int CUST_CO { get; set; }
            public string TAKH_COD { get; set; }
            public short TAFPER { get; set; }
            public Nullable<int> PRICE_M { get; set; }

            public virtual CUSTKIND CUSTKIND { get; set; }
            public virtual STUF_DEF STUF_DEF { get; set; }
        }
        public class Custom6_INVO
        {
            public int ANBAR { get; set; }
            public string CODE { get; set; }
            public double MEGHk { get; set; }
        }
        public class Custom_STUFDEF0
        {
            public double MIN_M { get; set; }
        }
        public class Custom2_CUST_HESAB
        {
            public string hes { get; set; }
            public Int32? CUST_COD { get; set; }
        }
        public class Custom_PRICE_PAYNO
        {
            public int PPID { get; set; }
            public string PPAME { get; set; }
            public int MODAT { get; set; }
        }
        public class Custom_PRICELIST
        {
            public int PEPID { get; set; }
            public string PEPNAME { get; set; }
            public int PEPDATE { get; set; }
            public Nullable<int> PEPDEPART { get; set; }
        }
        public class Custom_PRICELIST_ETF_Takhfif
        {
            public int PEID { get; set; }
            public string PENAME { get; set; }
            public int PEDATE { get; set; }
            public Nullable<int> PEPDEPART { get; set; }
        }
        #endregion

        public HEAD_LST_PISHFROOSH2(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
                IsOpenedFromAutomation = _isAutomasion_;
            }

            if (CL_Generaly.IsGHAYM_7)
            {
                EGHEY_LABEL.Visibility = Visibility.Visible;
                PEPID.Visibility = Visibility.Visible;
                ETAKHF_LABEL.Visibility = Visibility.Visible;
                PEID.Visibility = Visibility.Visible;
                NAHVAH_LABEL.Visibility = Visibility.Visible;
                MODAT_PPID.Visibility = Visibility.Visible;

                okpish.Visibility = Visibility.Visible; estelam.Visibility = Visibility.Visible;
            }
            else
            {
                EGHEY_LABEL.Visibility = Visibility.Hidden;
                PEPID.Visibility = Visibility.Hidden;
                ETAKHF_LABEL.Visibility = Visibility.Hidden;
                PEID.Visibility = Visibility.Hidden;
                NAHVAH_LABEL.Visibility = Visibility.Hidden;
                MODAT_PPID.Visibility = Visibility.Hidden;

                okpish.Visibility = Visibility.Hidden; estelam.Visibility = Visibility.Hidden;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_PISH2_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool INVO_LST_SUB_IsFocused { get; private set; }
        public bool NewRecord { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
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
        public int CURRENT_ROW_INVO_LST_PISH2_INDEX { get; set; } = 0;
        public string? ENTERED_VALUE_ROW { get; private set; }
        public INVO_LST_FACTOR22? CURRENT_ROW_ITEMS { get; private set; }
        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; } = new INVO_LST_FACTOR22();

        List<COMBOPERSONEL> rst_personel = null;

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;
        public INVO_LST_FACTOR22 FROM_SEARCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "FFRP_FROOSHTX");
                    _sgn1_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN1usid.Tag)));
                }
                return _sgn1_info;
            }
        }

        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO
        {
            get
            {
                if (SGN2usid.Tag is not null)
                {
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "FFRP_ANBTX");
                    _sgn2_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN2usid.Tag)));
                }
                return _sgn2_info;
            }
        }

        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO
        {
            get
            {
                if (SGN3usid.Tag is not null)
                {
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "FFRP_HESABTX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        InventoryManager IVM = new InventoryManager();
        //TransactionManagement TM;


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

        private bool can;
        public bool AllowEdits
        {
            get { return can; }
            set
            {
                can = value;

                DATE_N.IsReadOnly = !can;
                MOLAH.IsReadOnly = !can;
                MAS.IsReadOnly = !can;
                TAKHFIF.IsReadOnly = !can;
                MBAA.IsReadOnly = !can;
                MABL_HAZ.IsReadOnly = !can;
                SHARAYET.IsReadOnly = !can;
                INVO_LST_SUB.IsReadOnly = !can;

                //CUST_KIND.IsEnabled = can;

                if (!CL_HESABDARI.LETSGO("CUSTEN"))
                {
                    CUST_KIND.IsEnabled = false;
                }
                else
                {
                    CUST_KIND.IsEnabled = can;
                }

                byte TAMIRVALUE = 250;
                if (TAMIR.SelectedValue != null)
                {
                    TAMIRVALUE = Convert.ToByte(((FrameworkElement)TAMIR.SelectedValue).Tag);
                }
                if (!CL_HESABDARI.LETSGO("RESERV") || TAMIRVALUE == 2)
                {
                    this.TAMIR.IsEnabled = false;
                }
                else
                {
                    this.TAMIR.IsEnabled = can;
                }

                TICMBAA.IsEnabled = can;

                CUST_NO.IsEnabled = can;
                CUST_NO2.IsEnabled = can;
                JAY.IsEnabled = can;
                //TAMIR.IsEnabled = can;

                //cccc.IsEnabled = can;
                //custprint.IsEnabled = can;
                //Command114.IsEnabled = can;
                //Command139.IsEnabled = can;
                //Command100.IsEnabled = can;

                //Command116.IsEnabled = can; //تبدیل به حواله
                //Command113.IsEnabled = can; //تبدیل به فاکتور
                BTN_SAVE.IsEnabled = can;
                //Command118.IsEnabled = can;

                if (!CL_HESABDARI.LETSGO("DEFA"))
                {
                    this.DEPATMAN.IsEnabled = false;
                }
                else
                {
                    DEPATMAN.IsEnabled = can;
                }

                if (CL_Generaly.IsGHAYM_7)
                {
                    MODAT_PPID.IsEnabled = can;

                    if (!CL_HESABDARI.LETSGO("elamghe"))
                    {
                        //اجازه اصلاح اعلامیه قیمت را ندارد
                        this.PEPID.IsEnabled = false; //Locked = true;
                        this.PEID.IsEnabled = false; //Locked = true;
                    }
                    else
                    {
                        this.PEPID.IsEnabled = can; //Locked = false;
                        this.PEID.IsEnabled = can; //Locked = false;
                    }
                }
            }
        }
        public string CDTIME { get; set; }
        public string OKDATE { get; set; }
        public string OKTIME { get; set; }
        public string CDDATE { get; set; }
        public Visual I_AM_PISHFACTOR { get; set; }

        /// <summary>
        /// TAG = 20
        /// </summary>
        const byte TAG = 20; //پیش فاکتور
        public double Meidnum { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "PFACTFR", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            I_AM_PISHFACTOR = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);


            FILL_ALL_COMBOBOXES();

            OnOpenHEADLSTPISHFOROOSH2();

            ReGetMasterData();

            Form_Open_SUB();

            CL_LMethods.SetTabIndexes(
                 CUST_NO,
                 MOLAH,
                 DEPATMAN,
                 MODAT_PPID,
                 MAS,
                 BTN_SAVE,
                 INVO_LST_SUB
                 );

            CUST_NO.Focus();
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is pish_view item)
            {
                if (item != null)
                {
                    var itemfound = RecordsData.View.Cast<pish_view>().FirstOrDefault(x => x.NUMBER.Equals(Convert.ToDouble(item.NUMBER)));
                    if (itemfound != null)
                    {
                        // Set the CurrentItem to the found item
                        RecordsData.View.MoveCurrentTo(itemfound);

                        MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                    }
                }
                else
                {
                    // Update your window with the selected item
                    MoveReGetData(INavigator.Jahat.LastItem);
                }

            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = INVO_LST_SUB;
                UIElement uie = e.OriginalSource as UIElement;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (BTN_SAVE.IsFocused)
                    {
                        BTN_SAVE_Click(null, null);
                        return;
                    }
                    else if (SHARAYET.IsFocused || SHARAYET.IsKeyboardFocusWithin)
                    {
                        //continue out
                    }
                    else
                    {
                        e.Handled = true;

                        try
                        {
                            if (INVO_LST_SUB_IsFocused)
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
                        catch { /*ignore*/ }

                    }
                }
                else
                {
                    //if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F8 || e.SystemKey == Key.F8))
                    //{
                    //    e.Handled = true;
                    //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
                    //}
                }

                if (!INVO_LST_SUB.IsKeyboardFocusWithin && !INVO_LST_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
                {
                    if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        e.Handled = true;
                        var searchWindow = new EnhancedSearchWindow(this);
                        searchWindow.Owner = this;
                        searchWindow.ShowDialog();
                    }
                }
            }
            catch { }


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
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CDOKDATE_AND_TIME();

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

        private void GetFocusOnDefaultCell()
        {
            var DG = INVO_LST_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }

        //برای پیش فرض انبار در مشخصات سیستم
        private int ANBARDefaultValue = 0;
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";

            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            ANBAR_COLUMN.ItemsSource = ARST;

            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT ANBCO FROM dbo.OPANBACCESS WHERE (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
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
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //Noe Moshtari Of Customers
            CUST_KIND.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND.DisplayMemberPath = "CUSTKNAME";
            CUST_KIND.SelectedValuePath = "CUST_COD";
            CUST_KIND.SelectionChanged -= CUST_KIND_SelectionChanged;
            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh();
            CUST_KIND.SelectionChanged += CUST_KIND_SelectionChanged;

            ////Names Of Customers
            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";
            CUST_NO.SelectedItem = null;
            //Codes Of Customers
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //VAHEDJARI
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            DEPATMAN.ItemsSource = RST; DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectionChanged -= DEPATMAN_SelectionChanged;
            DEPATMAN.SelectedIndex = 0;
            DEPATMAN.SelectedItem = 0;
            DEPATMAN.SelectionChanged += DEPATMAN_SelectionChanged;

            //SHIFT
            //SHIFT.ItemsSource = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID,SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            //SHIFT.DisplayMemberPath = "SHNAME";
            //SHIFT.SelectedValuePath = "SHIFT_ID";
            //SHIFT.SelectedIndex = 0;
            //SHIFT.SelectedItem = 0;

            //نحوه پرداخت و مدت
            MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";

            //اعلامیه قیمت
            PEPID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_CSHARP>("SELECT PEPID, PEPNAME, PEPDATE, PEPDEPART FROM PRICE_ELAMIE ORDER BY PEPNAME DESC").ToList();
            PEPID.DisplayMemberPath = "PEPNAME";
            PEPID.SelectedValuePath = "PEPID";

            //اعلامیه تخفیف
            PEID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_ETF_TAKHFIF__CSHARP>("SELECT PEID, PENAME, PEDATE, PEPDEPART FROM PRICE_ELAMIETF").ToList();
            PEID.DisplayMemberPath = "PENAME";
            PEID.SelectedValuePath = "PEID";


            //PEID
            PEID.ItemsSource = dbms.DoGetDataSQL<Custom_PRICELIST_ETF_Takhfif>("SELECT PEID, PENAME, PEDATE, PEPDEPART FROM PRICE_ELAMIETF").ToList();
            PEID.DisplayMemberPath = "PENAME";
            PEID.SelectedValuePath = "PEID";
            PEID.SelectedIndex = 0;
            PEID.SelectedItem = 0;

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;

            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //کبموباکس مجری
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
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.SelectedValue = Baseknow.USERCOD;
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
        }


        private bool IsNull(object? hTAF2)
        {
            string _inputy = hTAF2.ToStringNullSafe();
            if (string.IsNullOrEmpty(_inputy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!Tarikh.IsValidedDate(DATE_N.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
            }
            else
            {
                if (!Tarikh.IsSyncedDateNow(DATE_N.Text, (bool)Baseknow.CTL_DT))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                }
            }

            if (MODAT_PPID.SelectedIndex == 0)
            {
                if (MAS.Text == "0")
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مدت توافق را وارد کنید" });
                }
            }
            //بررسی کلیه مقادیر این درست انختاب یا وارد شدند یا نه 
            if (!Tarikh.IsValidedDate(DATE_N.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
            }
            if (CUST_KIND.SelectedIndex == -1)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع مشتری انتخاب نشده است" });
            }
            if (DEPATMAN.SelectedIndex == -1)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "دپارتمان مشخص نشده است" });
            }
            if (CUST_NO.SelectedIndex == -1)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مشتری مشخص نشده است" });
            }
            if (CUST_NO2.SelectedIndex == -1)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مشتری مشخص نشده است" });
            }
            if (TAMIR.SelectedIndex == -1)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت پیش فاکتور از نظر عادی , رزرو و ... مشخص نشده است" });
            }

            if (MODAT_PPID.SelectedIndex == -1 && CL_Generaly.IsGHAYM_7)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نحوه پرداخت مشخص نشده" });
            }

            if (string.IsNullOrEmpty(MAS.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مدت زمان توافق نمیتواند خالی باشد" });
            }
            if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }
            if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToStringNullSafe())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToStringNullSafe())) == false)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!" });
                }
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
        private bool BodyIsValid(INVO_LST_FACTOR22 _row, bool _DisplayMsg_ = true)
        {
            var ROW = _row;
            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                INVO_LST_SUB_CANCEL_EDIT();
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            if (_row == null)
            {
                INVO_LST_SUB_CANCEL_EDIT();
                universControl.PopNotifyShow("سطر خالی مجاز نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            #region DEFAULT_VALIDATION
            // Validate ANBAR
            if (!int.TryParse(ROW.ANBAR?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار صحیح انتخاب نشده" });
            }
            // Validate CODE
            if (string.IsNullOrEmpty(ROW?.CODE) || ROW?.CODE.Length > 15)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            if (string.IsNullOrEmpty(ROW?.NAME_CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            // Validate MEGH
            if (!double.TryParse(ROW.MEGH?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            else
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                {
                    if (ROW.MEGH == 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صفر نمیتواند باشد" });
                    }
                }
            }
            // Validate MEGHk
            if (!double.TryParse(ROW.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }
            // Validate MANDAH
            if (ROW.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate MABL
            if (!double.TryParse(ROW.MABL?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کالا صحیح وارد نشده" });
            }
            // Validate MABL_K
            if (!double.TryParse(ROW.MABL_K?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کل,  کالا صحیح وارد نشده" });
            }
            // Validate VAHED_K
            if (!int.TryParse(ROW.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }
            // Validate N_KOL
            if (!double.TryParse(ROW.N_KOL?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تخفیف صحیح وارد نشده" });
            }
            if (!(ROW.N_KOL >= 0 && ROW.N_KOL <= 100))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "محدوده وارد شده تخفیف صحیح نیست" });
            }
            // Validate N_MOIN
            if (!double.TryParse(ROW.N_MOIN?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ تخفیف صحیح وارد نشده" });
            }
            // Validate TKHN
            if (!double.TryParse(ROW.TKHN?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف نقدی صحیح وارد نشده" });
            }
            #endregion

            #region COMPUTE_VALIDATION
            //بررسی محاسباتی*
            double MAND;
            var MEGHTAA = default(long);
            var MEGHJAYY = default(long);
            var VAHEDD = default(long);
            long CMABL = default;
            //گرفتن مقادیر جایزه و حداقل موجودی کالا
            var RST = dbms.DoGetDataSQL<MG_MODEL1>("SELECT MEGHJAY,MEGHTA,VAHED FROM STUF_DEF WHERE CODE = '" + ROW.CODE + "'").ToList();
            if (RST.Count > 0)
            {
                MEGHJAYY = (long)RST.FirstOrDefault().MEGHJAY;
                MEGHTAA = (long)RST.FirstOrDefault().MEGHTA;
                VAHEDD = (long)RST.FirstOrDefault().VAHED;
            }

            //انبار خالی نباشد
            if (ROW?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {ROW.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (IsNull(ROW.CODE))
            { }
            else
            {
                var RST_STUF_STK = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + ROW.CODE + "' AND ANBAR = " + ROW.ANBAR).ToList();
                if (RST_STUF_STK.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {ROW.CODE} به انبار {ROW.ANBAR} فوق تعلق ندارد." });
                }

                if (ROW?.VAHED_K != null)
                {
                    //بررسی صحیح بودن واحد کالا نسبت به خود کالا
                    var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + ROW.CODE + "' AND ((VAHEDS.VAHED)= " + ROW.VAHED_K + ")))").ToList();
                    if (RSTV1.Count == 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });
                        ROW.VAHED_K = null;
                    }
                    //واحد کالا بررسی مقدار کل باتوجه به نسبت
                    else
                    {
                        var NesbatMegh = RSTV1.FirstOrDefault().NESBAT * ROW.MEGH;
                        if (NesbatMegh != ROW.MEGHk)
                        {
                            ROW.MEGHk = NesbatMegh;
                            ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است مجددا اینتر را بزنید تا به سطر بعدی بروید" });
                        }
                    }
                }
            }

            //مقدار كالا نمي تواند صفر باشد بر اسا تنظیمات بیشتر
            if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
            {
                if (ROW.MEGH == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار كالا نمي تواند صفر باشد." });
                }
            }

            bool HichGHEYM = Baseknow.GHAYM.ToString() == "3"; //پیش فرض قیمت هیچکدام
            //در زمان تبديل پيش فاكتور به فاكتور مبلغ فروش كنترل گردد كه اگر تغيير كرده اخطار دهد بجز مبالغ صفر
            if (Strings.Mid(Baseknow.OPTIONSS, 51, 1) == "5" && !CL_Generaly.IsGHAYM_7 && !HichGHEYM)
            {
                var rst4 = dbms.DoGetDataSQL<PRT1>("SELECT MABL_F , B_SEF FROM STUF_DEF WHERE code = '" + ROW.CODE + "'").ToList();
                if (rst4.Count == 1)
                {
                    if (Baseknow.GHAYM.ToString() == "2")
                    {
                        CMABL = Convert.ToInt64(rst4.Select(x => x.MABL_F).FirstOrDefault());
                    }
                    else if (Baseknow.GHAYM.ToString() == "5")
                    {
                        CMABL = Convert.ToInt64(rst4.Select(x => x.B_SEF).FirstOrDefault());
                    }
                }
                if (CMABL != ROW.MABL && ROW.MABL != 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " قيمت كالاي " + ROW.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(ROW.CODE)) + " با قيمت سيستم منطبق نيست" });
                }

                var rst44 = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + ROW.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                if (rst44.Count > 0)
                {
                    if (rst44.Select(x => x.MABL).FirstOrDefault() > ROW.MABL)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "قيمت فروش از قيمت خريد كمتر مي باشد. " + "کد کالا : " + ROW.CODE + " نام کالا : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(ROW.CODE)) });
                    }
                }
            }

            //محاسبه مجدد مبلغ تخفیف و در صورت تفاوت بروز کردن آن
            if (ROW?.N_KOL > 0) //درصد تخفیف در دیتاگرید
            {
                var _N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                if (_N_MOIN != ROW.N_MOIN)
                {
                    ROW.N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ تخفیف {ROW.N_MOIN} اصلاح کردم , درصورتی که مورد تایید است مجددا اینتر را بزنید تا به سطر بعدی بروید" });
                }
            }
            else if (ROW?.TKHN > 0) // ت.ن% TKHN_AfterUpdate
            {
                var _N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));

                if (_N_MOIN != ROW.N_MOIN)
                {
                    ROW.N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ تخفیف {ROW.N_MOIN} اصلاح کردم , درصورتی که مورد تایید است مجددا اینتر را بزنید تا به سطر بعدی بروید" });
                }
            }
            #endregion

            if (ErrosMessages.Count > 0)
            {
                //INVO_LST_SUB_CANCEL_EDIT();

                if (_DisplayMsg_)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }
        private void INVO_LST_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_SUB.SelectedItem != null)
            {
                if (INVO_LST_SUB.Items.Count > 0)
                    CURRENT_ROW_INVO_LST_PISH2_INDEX = INVO_LST_SUB.SelectedIndex;

                if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
                {
                    if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                    }
                }
            }
        }
        private void INVO_LST_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(INVO_LST_SUB.Items.Count < 1) && !(INVO_LST_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INVO_LST_PISH2_INDEX = INVO_LST_SUB.SelectedIndex;
                }
            }
        }
        private void INVO_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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
                var filteredUnits = dbms.DoGetDataSQL<Custom_VAHEDK>(@$"(SELECT dbo.TCOD_VAHEDS.CODE AS VAHED,
                                                                    dbo.TCOD_VAHEDS.NAMES
                                                             FROM dbo.TCOD_VAHEDS
                                                                 INNER JOIN dbo.STUF_DEF
                                                                     ON dbo.TCOD_VAHEDS.CODE = dbo.STUF_DEF.VAHED
                                                             WHERE (dbo.STUF_DEF.CODE = N'{CurrentRow.CODE}'))
                                                             
                                                             UNION ALL
                                                             
                                                             (SELECT dbo.MODULE_D.VAHED,
                                                                    dbo.TCOD_VAHEDS.NAMES
                                                             FROM dbo.MODULE_D
                                                                 INNER JOIN dbo.TCOD_VAHEDS
                                                                     ON dbo.MODULE_D.VAHED = dbo.TCOD_VAHEDS.CODE
                                                             WHERE (dbo.MODULE_D.CODE = N'{CurrentRow.CODE}'))").Distinct().ToList();

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
        private void INVO_LST_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                INVO_LST_SUB_IsFocused = false;
            }
            else
            {
                INVO_LST_SUB_IsFocused = true;
            }
        }
        private void Form_Open_SUB()
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
            {
                SANAD_NO_COL.Visibility = Visibility.Visible;
            }
            else
            {
                this.SANAD_NO_COL.Visibility = Visibility.Hidden;

            }
            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) == "5")
            {
                this.TKHN_COL.Visibility = Visibility.Hidden;
            }
            else
            {
                this.TKHN_COL.Visibility = Visibility.Visible;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {
                this.JAY_COL.Visibility = Visibility.Visible;
                this.JAYO_COL.Visibility = Visibility.Visible;
            }
            else
            {
                this.JAY_COL.Visibility = Visibility.Hidden;
                this.JAYO_COL.Visibility = Visibility.Hidden;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) == "5")
            {
                this.TKHN_COL.Visibility = Visibility.Visible;
            }
            else
            {
                this.TKHN_COL.Visibility = Visibility.Visible;
            }
            if (CL_HESABDARI.LETSGO("JAYO"))
            {
                this.JAYO_COL.IsReadOnly = false;
            }
            else
            {
                this.JAYO_COL.IsReadOnly = true;
            }

        }

        private void INVO_LST_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (INVO_LST_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = INVO_LST_SUB.CurrentCell.Column?.SortMemberPath;
            }

            // Check if Ctrl key is pressed and the pressed key is double quote
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (INVO_LST_SUB.CurrentCell != null)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = INVO_LST_SUB.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = INVO_LST_SUB.Items.IndexOf(currentCell.Item);
                            int columnIndex = INVO_LST_SUB.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = INVO_LST_SUB.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < INVO_LST_SUB.Columns.Count)
                                {
                                    // Get the column information
                                    var column = INVO_LST_SUB.Columns[columnIndex];

                                    // Ensure that the column has a valid SortMemberPath
                                    if (!string.IsNullOrEmpty(column.SortMemberPath))
                                    {
                                        // Use reflection to get and set the property values
                                        var propertyInfo = valueAbove.GetType().GetProperty(column.SortMemberPath);

                                        // Ensure that the property exists and is not null
                                        if (propertyInfo != null)
                                        {
                                            // Get the value from the above cell
                                            object valueAboveCellValue = propertyInfo.GetValue(valueAbove);

                                            // Cast currentCell.Item to the actual data type
                                            var currentItem = currentCell.Item;

                                            // Use reflection to set the value on the current item
                                            if (currentItem.GetType().GetProperty(column.SortMemberPath) is PropertyInfo currentCellProperty)
                                            {
                                                // Set the value on the current cell's item
                                                currentCellProperty.SetValue(currentItem, valueAboveCellValue);

                                                INVO_LST_SUB.BeginEdit();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        e.Handled = true;
                    }
                }
                catch { }
            }


            if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME.Contains("MABL", StringComparison.OrdinalIgnoreCase))
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
                if (CURRENT_COLUMN_NAME.Contains("MABL", StringComparison.OrdinalIgnoreCase))
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
        private void INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_SUB.Dispatcher.Invoke(() =>
            {
                INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                INVO_LST_SUB.RowEditEnding -= INVO_LST_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVO_LST_SUB.CancelEdit();
                    //INVO_LST_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                }
                else
                {
                    INVO_LST_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                    //INVO_LST_SUB.CommitEdit((DataGridEditingUnit)_RC_, true);
                }
                INVO_LST_SUB.RowEditEnding += INVO_LST_SUB_RowEditEnding;
                INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;
            });
        }
        private void INVO_LST_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
            {
                if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }
        private void INVO_LST_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.EditingElement == null || e.Column == null)
            {
                return;
            }

            #region REFILL_CURRENTS
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
                ENTERED_VALUE_ROW = TexboVal?.Text?.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as INVO_LST_FACTOR22;
            if (CURRENT_ROW_ITEMS == null)
            {
                return;
            }
            #endregion

            double min = 0;

            //انبار
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    return;
                }
                else
                {
                    if (CURRENT_ROW_ITEMS?.CODE != null)
                    {
                        var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = N'{CURRENT_ROW_ITEMS.CODE}' AND ANBAR = {ENTERED_VALUE_ROW}").ToList();
                        if (Rst1.Count == 0)
                        {
                            universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                            CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ROW_ITEMS.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            CURRENT_ROW_ITEMS.VAHED_K = null; //Reset VAHED_K
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                    }
                }
            }
            //نام کالا
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    INVO_LST_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                    return;
                }
                if (CURRENT_ROW_ITEMS?.ANBAR is null)
                {
                    return;
                }
                else
                {
                    //اگر عدد وارد کرده برم سرغ کد کالا
                    if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                    {
                        var str = $"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ROW_ITEMS.ANBAR})";

                        var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ROW_ITEMS.ANBAR})").FirstOrDefault();

                        if (!ReferenceEquals(FoundKala, null))
                        {
                            CURRENT_ROW_ITEMS.CODE = FoundKala.CODE;
                            CURRENT_ROW_ITEMS.NAME_CODE = FoundKala.NAME;

                            CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ROW_ITEMS.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            CURRENT_ROW_ITEMS.VAHED_K = null; //Reset VAHED_K
                            universControl.PopNotifyShow("چنین کد کالایی وجود ندارد لطفا اصلاح کنید", Pop1, Pop1Text1, Pop_Border1);
                            return;
                        }
                    }
                    else
                    {
                        //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                        if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                        {
                            SERCHK sERCHK = new SERCHK(I_AM_PISHFACTOR, CURRENT_ROW_ITEMS.ANBAR.ToString());
                            sERCHK.ShowDialog();

                            if (FROM_SEARCH_KAL.CODE is null)
                            {
                                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INVO_LST_PISH2_INDEX, INVO_LST_SUB.Columns[TheCol]);
                                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                                if (!(THECELL is null))
                                    THECELL.Focus();

                                INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                return;
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.CODE = FROM_SEARCH_KAL.CODE;
                                CURRENT_ROW_ITEMS.NAME_CODE = FROM_SEARCH_KAL.NAME_CODE;

                                CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);
                            }
                            //Cleaning
                            FROM_SEARCH_KAL.CODE = null;
                            FROM_SEARCH_KAL.NAME_CODE = null;
                        }
                    }

                    //MODAT_PPID.SelectedValue != null && 
                    if (CURRENT_ROW_ITEMS.CODE != null && CURRENT_ROW_ITEMS.NAME_CODE != null)
                    {
                        //VAHED_LOADITEM();
                        #region AfterUpdate_SelectionChangeKALA
                        var Rst = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE,ANBAR,MOGODI_A,MOGODI,MABL_M FROM STUF_STK WHERE CODE = '{CURRENT_ROW_ITEMS.CODE}' AND ANBAR = {CURRENT_ROW_ITEMS.ANBAR}").ToList();
                        if (Rst.Count == 0)
                        {
                            MOGU.Text = "0";
                        }
                        else
                        {
                            MOGU.Text = Rst.Select(x => x.MOGODI + x.MOGODI_A).FirstOrDefault().ToString();
                        }
                        if (Baseknow.GHAYM.ToString() == "7" && MODAT_PPID.SelectedValue.ToStringNullSafe() != "0" && PEID.SelectedValue != null && PEPID.SelectedValue != null)
                        {
                            var _PEID_ = Convert.ToInt32(PEID.SelectedValue);
                            var _PEPID_ = Convert.ToInt32(PEPID.SelectedValue);

                            string gheymatkala = Convert.ToString(CL_HESABDARI.GETGHeymatKala(Convert.ToInt32(NUMBER.Text), 20, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), CURRENT_ROW_ITEMS.CODE.ToString(), _PEID_, _PEPID_));
                            if (!string.IsNullOrEmpty(gheymatkala))
                            {
                                CURRENT_ROW_ITEMS.MABL = Convert.ToDouble(gheymatkala);
                            }
                            string takhfifkala = Convert.ToString(CL_HESABDARI.GETTaghfifKala1(Convert.ToInt32(NUMBER.Text), 20, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), CURRENT_ROW_ITEMS.CODE.ToString(), _PEID_, _PEPID_));
                            if (!string.IsNullOrEmpty(takhfifkala))
                            {
                                CURRENT_ROW_ITEMS.N_KOL = Convert.ToDouble(takhfifkala);
                            }
                            string takhfifkala2 = Convert.ToString(CL_HESABDARI.GETTaghfifKala2(Convert.ToInt32(NUMBER.Text), 20, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), CURRENT_ROW_ITEMS.CODE.ToString(), _PEID_, _PEPID_));
                            if (!string.IsNullOrEmpty(takhfifkala2))
                            {
                                CURRENT_ROW_ITEMS.TKHN = Convert.ToDouble(takhfifkala2);
                            }
                            var Rst_1 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT CODE,NAME,N_FANI,TOZIH,VAHED,B_SEF,N_SEF,MIN_M,MAX_M,RADAH,KINDK,MABL_F,DEPART,IDD,CMBAA,VAZN,OKF,MENUIT,MEGHTA,MEGHJAY,PGID FROM STUF_DEF WHERE CODE = '{CURRENT_ROW_ITEMS.CODE}'").FirstOrDefault();
                            if (!ReferenceEquals(Rst_1, null))
                            {
                                if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                                {
                                    CURRENT_ROW_ITEMS.MANDAH = Rst_1.TOZIH;
                                }
                            }
                        }
                        else
                        {
                            var Rst_2 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT CODE,NAME,N_FANI,TOZIH,VAHED,B_SEF,N_SEF,MIN_M,MAX_M,RADAH,KINDK,MABL_F,DEPART,IDD,CMBAA,VAZN,OKF,MENUIT,MEGHTA,MEGHJAY,PGID FROM STUF_DEF WHERE CODE = '{CURRENT_ROW_ITEMS.CODE}'").FirstOrDefault();
                            if (!ReferenceEquals(Rst_2, null))
                            {
                                //CURRENT_ROW_ITEMS.VAHED_K = Rst_2.VAHED;
                                if (Baseknow.GHAYM.ToString() == "2")
                                {
                                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                    var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);
                                    CURRENT_ROW_ITEMS.MABL = Rst_2.MABL_F;
                                }
                                else if (Baseknow.GHAYM.ToString() == "5")
                                {
                                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                    var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);

                                    CURRENT_ROW_ITEMS.MABL = Rst_2.B_SEF;
                                }
                                else if (Baseknow.GHAYM.ToString() == "6")
                                {
                                    var Rst_22 = dbms.DoGetDataSQL<CUSTOM1_TAKHPERS>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ROW_ITEMS.CODE + "')").FirstOrDefault();
                                    if (!ReferenceEquals(Rst_22, null))
                                    {
                                        if (CURRENT_ROW_ITEMS.MABL != Rst_22.PRICE_M && Rst_22.PRICE_M != 0)
                                        {
                                            var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL").DisplayIndex;
                                            var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                            var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);

                                            CURRENT_ROW_ITEMS.MABL = Rst_22.PRICE_M;
                                        }

                                        if (CURRENT_ROW_ITEMS.MABL_K != Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk)))
                                        {
                                            var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                            var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                            var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);

                                            CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk));
                                        }
                                    }
                                }
                                if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                                {
                                    CURRENT_ROW_ITEMS.MANDAH = Rst_2.TOZIH;//LAST POINT
                                }
                            }

                            if (Baseknow.GHAYM.ToString() == "1")
                            {
                                var RstOpen = dbms.DoGetDataSQL<Custom5_INVO>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ROW_ITEMS.CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                                if (!ReferenceEquals(RstOpen, null) && RstOpen != null)
                                {
                                    CURRENT_ROW_ITEMS.MABL = RstOpen.MABL;

                                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                    var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);
                                }
                            }
                            if (Baseknow.GHAYM.ToString() == "4")
                            {
                                var Rsto = dbms.DoGetDataSQL<Custom5_INVO>("SELECT     TOP 100 PERCENT MAX(dbo.INVO_LST.NUMBER) AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.HEAD_LST.CUST_NO = '" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = '" + CURRENT_ROW_ITEMS.CODE + "')GROUP BY dbo.INVO_LST.MABL ORDER BY MAX(dbo.INVO_LST.NUMBER) DESC").Take(1).FirstOrDefault();
                                if (!ReferenceEquals(Rsto, null))
                                {
                                    CURRENT_ROW_ITEMS.MABL = Convert.ToDouble(Rsto.MABL);

                                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                    var TheDGCell = CL_LMethods.GetDataGridCell(DGCInf);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(CURRENT_ROW_ITEMS.CODE.ToStringNullSafe()))
                        {
                            #region MEGH_AfterUpdate
                            min = Convert.ToDouble(CL_HESABDARI.Getmin(Convert.ToInt32(CURRENT_ROW_ITEMS.ANBAR), CURRENT_ROW_ITEMS.CODE.ToString()));
                            CURRENT_ROW_ITEMS.MEGHk = (Convert.ToDouble(CURRENT_ROW_ITEMS.MEGH) * CL_HESABDARI.GETNESBAT(CURRENT_ROW_ITEMS.CODE.ToString(), Convert.ToInt32(CURRENT_ROW_ITEMS.VAHED_K)));

                            var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                            var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                            var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                            if (CURRENT_ROW_ITEMS.MABL == 0)
                            {
                                if (TheDGCell_MABL_K != null)
                                {
                                    TheDGCell_MABL_K.IsTabStop = true;
                                }
                            }
                            else
                            {
                                if (TheDGCell_MABL_K != null)
                                {
                                    TheDGCell_MABL_K.IsTabStop = false;
                                }
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL));

                                var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf1 = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol1]);
                                var TheDGCell1 = CL_LMethods.GetDataGridCell(DGCInf1);
                            }
                            var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = N'{CURRENT_ROW_ITEMS.CODE}' AND ANBAR = {CURRENT_ROW_ITEMS.ANBAR}").ToList();
                            if (Rst1.Count == 0)
                            {
                                universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                                CURRENT_ROW_ITEMS.CODE = null;
                            }

                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);

                            var TheCol2 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "N_MOIN").DisplayIndex;
                            var DGCInf2 = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol2]);
                            var TheDGCell2 = CL_LMethods.GetDataGridCell(DGCInf2);

                            if ((bool)TICMBAA.IsChecked)
                            {

                                var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>($"select CMBAA ,code from STUF_DEF where code = '{CURRENT_ROW_ITEMS.CODE}'").FirstOrDefault();
                                if (!ReferenceEquals(rst, null))
                                {
                                    if ((bool)rst.CMBAA)
                                    {

                                        if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100))
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                                        }
                                    }
                                    else
                                    {
                                        if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                        {
                                            Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                            if (msgwin.DialogResult == true)
                                            {
                                                CURRENT_ROW_ITEMS.IMBAA = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.IMBAA = 0;
                            }
                            #endregion
                        }

                        if (CURRENT_ROW_ITEMS.MABL_K != Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk)))
                        {
                            CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk));
                        }
                        double nmoin = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                        if (CURRENT_ROW_ITEMS.N_MOIN != nmoin)
                        {
                            CURRENT_ROW_ITEMS.N_MOIN = Convert.ToDouble(Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                        }

                        #endregion
                    }
                    else
                    {
                        //universControl.PopNotifyShow("لطفا نحوه پرداخت رو انتخاب کنید", Pop1, Pop1Text1, Pop_Border1);
                        //MessageBox.Show();
                        INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                        INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        MODAT_PPID.Focus();
                        INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;

                    }
                }
            }
            //واحد
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    return;
                }
                if (CURRENT_ROW_ITEMS?.ANBAR is null || CURRENT_ROW_ITEMS?.CODE is null)
                {
                    INVO_LST_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                    return;
                }
                else
                {
                    #region AfterUpdate
                    if (CURRENT_ROW_ITEMS.CODE != null && !string.IsNullOrEmpty((e.EditingElement as ComboBox).SelectedValue.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.MEGHk = Convert.ToDouble(CURRENT_ROW_ITEMS.MEGH) * CL_HESABDARI.GETNESBAT(CURRENT_ROW_ITEMS.CODE, Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue));

                        var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGHk").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                        var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);

                        if (CURRENT_ROW_ITEMS.MABL == 0)
                        {
                            TheDGCell_MABL_K.IsTabStop = true;
                        }
                        else
                        {
                            TheDGCell_MABL_K.IsTabStop = true;
                            CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk));
                        }
                        CURRENT_ROW_ITEMS.N_MOIN = Math.Round(System.Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(System.Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);

                        if ((bool)TICMBAA.IsChecked)
                        {
                            var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>("select CMBAA ,code from STUF_DEF where code = '" + CURRENT_ROW_ITEMS.CODE + "'").FirstOrDefault();
                            if (!ReferenceEquals(rst, null))
                            {
                                if ((bool)rst.CMBAA)
                                {
                                    if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100))
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                                    }
                                }
                                else
                                {
                                    if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                    {
                                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                        if (msgwin.DialogResult == true)
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = 0;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                    }
                    #endregion
                }
            }
            //مقدار
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ROW_ITEMS.MEGH = 0;
                    return;
                }
                if (CURRENT_ROW_ITEMS?.ANBAR is null || CURRENT_ROW_ITEMS?.CODE is null || CURRENT_ROW_ITEMS?.VAHED_K is null)
                {
                    return;
                }
                else
                {
                    #region MEGH_LOST_FOCUS
                    //LostFocusMegh
                    var tmegh = ((TextBox)e.EditingElement).Text;
                    if (string.IsNullOrEmpty(tmegh.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.MEGH = 0;
                        tmegh = "0";
                    }
                    if (CURRENT_ROW_ITEMS.CODE != null)
                    {
                        #region MEGH_AfterUpdate
                        min = Convert.ToDouble(CL_HESABDARI.Getmin(Convert.ToInt32(CURRENT_ROW_ITEMS.ANBAR), CURRENT_ROW_ITEMS.CODE.ToString()));
                        var testmegh = Convert.ToDouble(((TextBox)e.EditingElement).Text);
                        var testnesbat1 = CL_HESABDARI.GETNESBAT(CURRENT_ROW_ITEMS.CODE.ToString(), Convert.ToInt32(CURRENT_ROW_ITEMS.VAHED_K));
                        double testdbl = Convert.ToDouble(((TextBox)e.EditingElement).Text) * CL_HESABDARI.GETNESBAT(CURRENT_ROW_ITEMS.CODE.ToString(), Convert.ToInt32(CURRENT_ROW_ITEMS.VAHED_K));
                        CURRENT_ROW_ITEMS.MEGHk = Convert.ToDouble(((TextBox)e.EditingElement).Text) * CL_HESABDARI.GETNESBAT(CURRENT_ROW_ITEMS.CODE.ToString(), Convert.ToInt32(CURRENT_ROW_ITEMS.VAHED_K));

                        var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                        var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                        if (CURRENT_ROW_ITEMS.MABL == 0)
                        {
                            TheDGCell_MABL_K.IsTabStop = true;
                        }
                        else
                        {
                            TheDGCell_MABL_K.IsTabStop = false;
                            CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL));

                        }
                        var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = N'{CURRENT_ROW_ITEMS.CODE}' AND ANBAR = {CURRENT_ROW_ITEMS.ANBAR}").ToList();
                        if (Rst1.Count == 0)
                        {
                            universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                            CURRENT_ROW_ITEMS.CODE = null;
                        }
                        CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                        if ((bool)TICMBAA.IsChecked)
                        {

                            var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>($"select CMBAA ,code from STUF_DEF where code = '{CURRENT_ROW_ITEMS.CODE}'").FirstOrDefault();
                            if (!ReferenceEquals(rst, null))
                            {
                                if ((bool)rst.CMBAA)
                                {

                                    if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100))
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                                    }
                                }
                                else
                                {
                                    if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                    {
                                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                        if (msgwin.DialogResult == true)
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = 0;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            //مقدارکل
            if (e.Column.SortMemberPath == "MEGHk")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ROW_ITEMS.MEGHk = 0;
                    return;
                }
                if (CURRENT_ROW_ITEMS?.ANBAR is null || CURRENT_ROW_ITEMS?.CODE is null || CURRENT_ROW_ITEMS?.VAHED_K is null || CURRENT_ROW_ITEMS?.MEGH is null)
                {
                    return;
                }
                else
                {
                    //اگر خواستدن با مبلغ کل مقدار رو بزنه کامنت های این بخش رو برگردونید
                    #region Meghk_LostFocus_
                    long Temp = 0;
                    double MAND = 0;
                    string namey = "SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ROW_ITEMS.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ROW_ITEMS.VAHED_K + ")))";
                    var rst = dbms.DoGetDataSQL<VAHEDKTOM>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ROW_ITEMS.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ROW_ITEMS.VAHED_K + ")))").FirstOrDefault();
                    if (ReferenceEquals(rst, null))
                    {
                        Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد."); msgwin.ShowDialog();
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.MEGH = CURRENT_ROW_ITEMS.MEGHk / rst.NESBAT;

                        if (CURRENT_ROW_ITEMS.MABL == 0)
                        {
                        }
                        else
                        {

                            if (!ReferenceEquals(CURRENT_ROW_ITEMS.MABL, null))
                            {
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                            }

                        }
                    }
                    #endregion
                }
            }
            //مبلغ
            if (e.Column.SortMemberPath == "MABL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ROW_ITEMS.MABL = 0;
                    return;
                }
                if (
                    CURRENT_ROW_ITEMS?.ANBAR is null ||
                    CURRENT_ROW_ITEMS.CODE is null ||
                    CURRENT_ROW_ITEMS.VAHED_K is null ||
                    CURRENT_ROW_ITEMS.MEGH is null ||
                    CURRENT_ROW_ITEMS.MEGHk is null
                    )

                {
                    return;
                }
                else
                {
                    var tmab = CURRENT_ROW_ITEMS.MABL;
                    if (string.IsNullOrEmpty(tmab.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.MABL = 0;
                        tmab = 0;
                    }
                    //MABL_in_MEGHK();May Should Active
                    #region MABL_AfterUpdate_LOST_FOCUS
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                    var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                    if (tmab == 0)
                    {
                        TheDGCell_MABL_K.IsTabStop = true;
                        CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk) * Convert.ToDouble(tmab));
                    }
                    else
                    {
                        TheDGCell_MABL_K.IsTabStop = false;
                        CURRENT_ROW_ITEMS.MABL_K = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk) * Convert.ToDouble(tmab));
                    }
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(System.Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);

                    var rst = dbms.DoGetDataSQL<CUSTOM_MAX_Number_Mabl>("SELECT Max(INVO_LST.MABL) AS MMABL, Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.CODE FROM INVO_LST WHERE (((INVO_LST.TAG) = 1) AND (INVO_LST.CODE = '" + CURRENT_ROW_ITEMS.CODE + "')) GROUP BY INVO_LST.CODE").FirstOrDefault();
                    if (!ReferenceEquals(rst, null))
                    {
                        if (rst.MMABL > Convert.ToDouble(tmab))
                        {
                            universControl.PopNotifyShow("قیمت فروش کمتر از خرید می باشد ", Pop1, Pop1Text1, Pop_Border1, "#FFDC9E18");
                        }
                    }
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var rstopn = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>("select CMBAA ,code from STUF_DEF where code = '" + CURRENT_ROW_ITEMS.CODE + "'").FirstOrDefault();
                        if (!ReferenceEquals(rstopn, null))
                        {
                            if ((bool)rstopn.CMBAA)
                            {
                                CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                    if (msgwin.DialogResult == true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                        #endregion
                    }
                }
            }
            //مبلغ کل
            if (e.Column.SortMemberPath == "MABL_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ROW_ITEMS.MABL_K = 0;
                    return;
                }
                if (
                   CURRENT_ROW_ITEMS?.ANBAR is null ||
                   CURRENT_ROW_ITEMS.CODE is null ||
                   CURRENT_ROW_ITEMS.VAHED_K is null ||
                   CURRENT_ROW_ITEMS.MEGH is null ||
                   CURRENT_ROW_ITEMS.MEGHk is null ||
                   CURRENT_ROW_ITEMS.MABL is null
                   )
                {
                    return;
                }
                else
                {
                    var mablk = CURRENT_ROW_ITEMS.MABL_K;
                    if (string.IsNullOrEmpty(mablk.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.MABL_K = 0;
                        mablk = 0;
                    }

                    #region AfterUpdate_MABL_K_LostFocus_
                    if (CURRENT_ROW_ITEMS.MEGHk == 0)
                    {
                        CURRENT_ROW_ITEMS.MABL_K = 0;
                    }
                    else
                    {
                        var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                        var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);

                        CURRENT_ROW_ITEMS.MABL = Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk);
                        TheDGCell_MABL_K.IsTabStop = false;
                    }
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if ((bool)rst.CMBAA)
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100))
                                {
                                    CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                                }
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                    if (msgwin.DialogResult == true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.IMBAA = 0;
                    }
                    #endregion
                    #region OnLostFocus

                    if (CURRENT_ROW_ITEMS.MABL == 0 && CURRENT_ROW_ITEMS.CODE != null)
                    {
                        if (CURRENT_ROW_ITEMS.MEGHk == 0)
                        {
                            CURRENT_ROW_ITEMS.MABL_K = 0;
                        }
                        else
                        {
                            if (CURRENT_ROW_ITEMS.MABL != Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk)))
                            {
                                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[CURRENT_ROW_INVO_LST_PISH2_INDEX], INVO_LST_SUB.Columns[TheCol]);
                                var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);

                                CURRENT_ROW_ITEMS.MABL = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / Convert.ToDouble(CURRENT_ROW_ITEMS.MEGHk));
                                TheDGCell_MABL_K.IsTabStop = false;
                            }
                        }
                    }
                    if (CURRENT_ROW_ITEMS.N_MOIN != Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100))
                    {
                        CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K)) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    }
                    #endregion
                }
            }
            //تخفیف
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ROW_ITEMS.N_KOL = 0;
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    return;
                }
                if (
                     CURRENT_ROW_ITEMS?.ANBAR is null ||
                     CURRENT_ROW_ITEMS.CODE is null ||
                     CURRENT_ROW_ITEMS.VAHED_K is null ||
                     CURRENT_ROW_ITEMS.MEGH is null ||
                     CURRENT_ROW_ITEMS.MEGHk is null ||
                     CURRENT_ROW_ITEMS.MABL is null ||
                     CURRENT_ROW_ITEMS.MABL_K is null
                    )
                {
                    return;
                }
                else
                {
                    var nkol = CURRENT_ROW_ITEMS.N_KOL;
                    if (string.IsNullOrEmpty(nkol.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.N_KOL = 0;
                        nkol = 0;
                    }
                    #region AfterUpdate_NKOL_LOSF_FOCUS_
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var rstOpen = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").FirstOrDefault();
                        if (!ReferenceEquals(rstOpen, null))
                        {
                            if ((bool)rstOpen.CMBAA)
                            {
                                CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                    if (msgwin.DialogResult == true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.IMBAA = 0;
                    }
                    #endregion
                }
            }
            //%ت.ن
            if (e.Column.SortMemberPath == "TKHN")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ROW_ITEMS.TKHN = 0;
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    return;
                }
                if (!double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ROW_ITEMS.TKHN = 0;
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    return;
                }
                if (Convert.ToDouble(ENTERED_VALUE_ROW) > 100 || Convert.ToDouble(ENTERED_VALUE_ROW) < 0)
                {
                    CURRENT_ROW_ITEMS.TKHN = null;
                    return;
                }
                if (
                    CURRENT_ROW_ITEMS?.ANBAR is null ||
                    CURRENT_ROW_ITEMS.CODE is null ||
                    CURRENT_ROW_ITEMS.VAHED_K is null ||
                    CURRENT_ROW_ITEMS.MEGH is null ||
                    CURRENT_ROW_ITEMS.MEGHk is null ||
                    CURRENT_ROW_ITEMS.MABL is null ||
                    CURRENT_ROW_ITEMS.MABL_K is null
                    )
                {
                    return;
                }
                else if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    var tkhn = CURRENT_ROW_ITEMS.TKHN;
                    if (string.IsNullOrEmpty(tkhn.ToStringNullSafe()))
                    {
                        CURRENT_ROW_ITEMS.TKHN = 0;
                    }
                    CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if ((bool)rst.CMBAA)
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100))
                                {
                                    CURRENT_ROW_ITEMS.IMBAA = Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Convert.ToDouble(CURRENT_ROW_ITEMS.N_MOIN)) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100);
                                }
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟"); msgwin.ShowDialog();
                                    if (msgwin.DialogResult == true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.IMBAA = 0;
                    }
                }
            }
            //مبلغ تخفیف
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ROW_ITEMS.N_MOIN = 0;
                    return;
                }
                if (
                    CURRENT_ROW_ITEMS?.ANBAR is null ||
                    CURRENT_ROW_ITEMS.CODE is null ||
                    CURRENT_ROW_ITEMS.VAHED_K is null ||
                    CURRENT_ROW_ITEMS.MEGH is null ||
                    CURRENT_ROW_ITEMS.MEGHk is null ||
                    CURRENT_ROW_ITEMS.MABL is null ||
                    CURRENT_ROW_ITEMS.MABL_K is null
                    )
                {
                    return;
                }
                else
                {
                    #region N_MOIN_AfterUpdate
                    if (CURRENT_ROW_ITEMS.MABL_K > 0)
                    {
                        CURRENT_ROW_ITEMS.N_KOL = CURRENT_ROW_ITEMS.N_KOL * 100 / CURRENT_ROW_ITEMS.MABL_K;
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.N_MOIN = 0;
                    }
                    if (Convert.ToBoolean(TICMBAA.IsChecked))
                    {
                        var rst = dbms.DoGetDataSQL<CUSTOM_CMAA_CODE>($"SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '{CURRENT_ROW_ITEMS.CODE}'").FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if ((bool)rst.CMBAA)
                            {
                                CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * Convert.ToDouble(CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE)) / 100));
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();
                                    if (msgwin.DialogResult == true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.IMBAA = 0;
                    }
                    #endregion
                }
            }

        }
        private void INVO_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (!HeaderIsValid())
            {
                return;
            }

            var ROW = e.Row.Item as INVO_LST_FACTOR22;
            if (e.Row.Item == null || ROW is null)
            {
                return;
            }

            if (ConstructorRowDetector.IsPristine(ROW)) { INVO_LST_SUB_CANCEL_EDIT(); return; }


            if (!BodyIsValid(ROW))
            {
                INVO_LST_SUB_CANCEL_EDIT();
                #region NEWWAY
                //var DG = INVO_LST_SUB;
                //e.Cancel = true;
                //DG.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    DG.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                //    DG.RowEditEnding -= INVO_LST_SUB_RowEditEnding;

                //    DG.SelectedItem = ROW;
                //    DG.ScrollIntoView(ROW);
                //    DG.CurrentCell = new DataGridCellInfo(ROW, DG.Columns[2]);
                //    DG.BeginEdit();

                //    DG.RowEditEnding += INVO_LST_SUB_RowEditEnding;
                //    DG.CellEditEnding += INVO_LST_SUB_CellEditEnding;

                //}), System.Windows.Threading.DispatcherPriority.Background);
                #endregion
                return;
            }

            IVM.StartTransaction(); //--------------------------------------------------------------------------------------------------------

            long? _id_ = null;
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            string Qre = "";
            if (ROW?.id == null) //INSERT
            {
                Qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, SANAD_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                                                  OUTPUT INSERTED.id
                                                  VALUES({NUMBER.Text}, {TAG},
                                                  {ROW.ANBAR},
                                                  N'{ROW.CODE}',
                                                  {ROW.MEGH},
                                                  {ROW.MEGHk},
                                                  {ROW.MEGH_MAR},
                                                  N'{ROW.MANDAH}',
                                                  {ROW.MABL},
                                                  {ROW.MABL_K},
                                                  {Convert.ToByte(ROW.FROM_A)},
                                                  N'{ROW.N_RASID}',
                                                  {ROW.MEGH_R},
                                                  {ROW.SANAD_NO},
                                                  {ROW.ANBARF},
                                                  {ROW.VAHED_K},
                                                  {ROW.N_KOL},
                                                  {ROW.N_MOIN},
                                                  {ROW.N_TAF},
                                                  {ROW.AVRAGE},
                                                  {ROW.AVRAGE2},
                                                  {ROW.IMBAA},
                                                  {ROW.TOTALARZ},
                                                  N'{ROW.VISITOR}',
                                                  {ROW.TKHN},
                                                  {(ROW.JAY is null ? "NULL" : ROW.JAY)},
                                                  {(ROW.JAYO is null ? "NULL" : ROW.JAYO)})";
            }
            else //UPDATE
            {
                Qre = $@"UPDATE dbo.INVO_LST SET 
                                                 ANBAR = {ROW.ANBAR} ,
                                                 CODE = {ROW.CODE} ,
                                                 VAHED_K = {ROW.VAHED_K} ,
                                                 MEGH = {ROW.MEGH} ,
                                                 MEGHk = {ROW.MEGHk} ,
                                                 MABL = {ROW.MABL} ,
                                                 MABL_K = {ROW.MABL_K} ,
                                                 N_KOL = {ROW.N_KOL} ,
                                                 TKHN = {ROW.TKHN} ,
                                                 N_MOIN = {ROW.N_MOIN} ,
                                                 IMBAA = {ROW.IMBAA} ,
                                                 MANDAH = N'{ROW.MANDAH}' ,
                                                 JAY = {(ROW.JAY is null ? "NULL" : ROW.JAY)},
                                                 JAYO = {(ROW.JAYO is null ? "NULL" : ROW.JAYO)}
                                                 WHERE id = {ROW.id} AND NUMBER = {NUMBER.Text} AND TAG = {TAG}";
            }

            //اگر رزرو یا رزور قطعی داره موجودی رو کنترل کن و نذار ذخیره کنه
            var _tamir = Convert.ToDouble(((FrameworkElement)TAMIR.SelectedValue).Tag);
            if (_tamir == 1 || _tamir == 4)
            {
                //بررسی موجودی در صورت داشتن موجودی اعمال تغییرات
                var (errorMsgs, _, _, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { ROW }, Qre, null, false);
                ErrosMessages.AddRange(errorMsgs);

                if (queryOutputs.Any())
                {
                    ROW.id = queryOutputs.FirstOrDefault();
                }

                if (ErrosMessages.Any())
                {
                    IVM.ShowErrorMessages(ErrosMessages);
                    IVM.RollbackTransaction();
                    INVO_LST_SUB_CANCEL_EDIT();
                    return;
                }
                else
                {
                    IVM.CommitTransaction();
                }
            }
            else
            {
                if (ROW?.id == null) //INSERT
                {
                    _id_ = dbms.DoGetDataSQL<long?>(Qre).FirstOrDefault();
                    if (_id_ != null)
                    {
                        ROW.id = _id_;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL(Qre);
                }
            }

            JAYEZAH(); //جایزه

            //بررسی قیمیت ها توسط مستر کاکرت
            GoGheymateUpdator();

            INVO_LST_SUB_ReGetData(); //Reload from database

            DisplayMandah();
        }

        bool isSavedSuccess = false;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            isSavedSuccess = false;

            if (!BTN_SAVE.IsEnabled) { return; }

            if (!HeaderIsValid())
            {
                return;
            }

            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0") // Insert | Is New PreInvoice Header
            {
                double num = 0;
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        ////Fake Query for Lock Table
                        //db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                        ////Fake Query for Lock Table
                        //var rst_11 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=20))", null, transaction).FirstOrDefault();

                        var rst_11 = db.Query<double?>("SELECT MAX(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WITH (UPDLOCK, HOLDLOCK) WHERE HEAD_LST.TAG = 20", transaction: transaction, commandTimeout: 60).FirstOrDefault();

                        if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                        {
                            num = 1; //Baseknow.STHFR
                        }
                        else
                        {
                            num = Convert.ToInt64(rst_11 + 1);
                        }
                        var rst_f = db.Query<HEAD_LST>("select NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID from HEAD_LST where NUMBER = " + num, null, transaction).FirstOrDefault();

                        string QRE_HEADINSUP =
                        $@"INSERT INTO dbo.HEAD_LST(NUMBER, TAG, ANBAR, NUMBER1, 					   DATE_N, TAH, 	   MAS, VAS, 					 CUST_NO, 			 MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, 		MABL_HAZ, MOIN_HAZ, 		TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, 					DEPATMAN, 						SHIFT, 				   CUST_KIND, 			 USER_NAME, 		   SHARAYET, 		MBAA , HMBAA, 										   TAMIR, 							  TICMBAA, TKHF, 							 OKF, SADER, ARZD, ARZKIND, 				   CDDATE, CDTIME, OKDATE, OKTIME, 							   JAY, 															   MODAT_PPID, 															 PEPID, 														PEID,							  SGN1, 							SGN2, 							  SGN3, 										sgn1usid, 										  sgn2usid, 									  sgn3usid)
	                    				       VALUES({num}, 20 ,  0   ,    0.0 , {DATE_N.Text.ToRawTarikh()},N'' , {MAS.Text},0.0 , N'{CUST_NO.SelectedValue}' , N'{MOLAH.Text}' ,    0.0 ,     0.0 ,     N'' ,     0.0 ,     N'' , {MABL_HAZ.Text},     N'' , {TAKHFIF.Text} ,     N'' ,   0   ,   0.0 , {DEPATMAN.SelectedValue} , {CL_Generaly.SHIFT_OF_USER}, {CUST_KIND.SelectedValue}, N'{USER_NAME.Text}' , N'{SHARAYET.Text}' , {MBAA.Text} ,  N'' , {((FrameworkElement)TAMIR.SelectedValue).Tag} , {Convert.ToByte(TICMBAA.IsChecked)}, NULL, {Convert.ToByte(OKF.IsChecked)},  0   , 0.0 ,    0   , {Tarikh.FullCurrentDate} ,   0   ,   0   ,   0   , {Convert.ToByte(JAY.IsChecked)}, {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)} , {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)} ,{(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)} , {Convert.ToByte(SGN1.IsChecked)}, {Convert.ToByte(SGN2.IsChecked)}, {Convert.ToByte(SGN3.IsChecked)}, {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                                  )";


                        db.Execute(QRE_HEADINSUP, null, transaction);

                        transaction.Commit();
                        NUMBER.Text = num.ToString();
                        db?.Close();
                    }
                    //Here Save
                    RefreshAfterInsert();
                }
            }
            else //Update
            {
                dbms.DoExecuteSQL(@$"UPDATE dbo.HEAD_LST SET DATE_N={DATE_N.Text.ToRawTarikh()}, 
                                     CUST_KIND={CUST_KIND.SelectedValue},
                                     TICMBAA={Convert.ToByte(TICMBAA.IsChecked)},
                                     OKF={Convert.ToByte(OKF.IsChecked)},
                                     JAY={Convert.ToByte(JAY.IsChecked)},
                                     CUST_NO=N'{CUST_NO.SelectedValue}', 
                                     DEPATMAN={DEPATMAN.SelectedValue}, 
                                     MOLAH=N'{MOLAH.Text}',
                                     USER_NAME=N'{USER_NAME.Text}',
                                     MAS={MAS.Text},
                                     TAKHFIF={TAKHFIF.Text},
                                     MBAA={MBAA.Text},
                                     MABL_HAZ={MABL_HAZ.Text},
                                     SHARAYET=N'{SHARAYET.Text}',
                                     TAMIR={((FrameworkElement)TAMIR.SelectedValue).Tag},
                                     MODAT_PPID = {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)},
                                     PEPID = {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)},
                                     PEID = {(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)},
                                     SGN1 = {Convert.ToByte(SGN1.IsChecked)},
                                     SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                                     SGN3 = {Convert.ToByte(SGN3.IsChecked)}, 
                                     sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                                     sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                                     sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                                     WHERE NUMBER={NUMBER.Text} AND TAG={TAG}");
            }

            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
            }
            long MBK, CMABL = default;
            double nesba;
            var takh = default(double);
            var min = default(double);
            var NOTPR = default(bool);
            bool AllisWell = true;

            if (sender != null)
            {
                INVO_LST_SUB.IsReadOnly = false;
            }

            GoGheymateUpdator();

            DisplayMandah();
            DisplaySumPrices();
            ChangeIsHappend = false;

            //if (INVO_LST_SUB.IsReadOnly)
            //{
            //    INVO_LST_SUB.IsReadOnly = false;
            //}

            if (e != null)
            {
                universControl.PopNotifyShow("اطلاعات سربرگ با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }

            if (INVO_LST_PISH2_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            isSavedSuccess = true;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            string UserMachiny = "";
            if (sender is null && e is null)
            {
                UserMachiny = "_Systemy_";
            }
            //if (!string.IsNullOrEmpty(NUMBER.Text) && OKF.IsChecked == false)
            if (!string.IsNullOrEmpty(NUMBER.Text))
            {
                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = 20)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = 20)", dt, 1);

                byte TAMIRVALUE = 250;
                if (TAMIR.SelectedValue != null)
                {
                    TAMIRVALUE = Convert.ToByte(((FrameworkElement)TAMIR.SelectedValue).Tag);
                }

                if (TAMIRVALUE != 2 && string.IsNullOrEmpty(UserMachiny)) //if (this.TAMIR != 2)
                {
                    if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                    {
                        if (!(sender is null))
                        {
                            Msgwin msgwin = new Msgwin(false, "اول امضا را بردارید ..."); msgwin.ShowDialog(); return;
                        }
                        this.CUST_NO.IsEnabled = false;
                        INVO_LST_SUB.IsReadOnly = true;
                        this.DATE_N.IsEnabled = false;
                        this.MOLAH.IsEnabled = false;
                        AllowEdits = true;
                        MODAT_PPID.IsEnabled = false;
                        this.TICMBAA.IsEnabled = false;
                        BTN_DELETE.IsEnabled = false;
                    }
                    else
                    {
                        this.CUST_NO.IsEnabled = true;
                        INVO_LST_SUB.IsReadOnly = false;
                        this.MOLAH.IsEnabled = true;
                        this.DATE_N.IsEnabled = true;
                        AllowEdits = true;
                        MODAT_PPID.IsEnabled = true;
                        this.TICMBAA.IsEnabled = true;
                        BTN_DELETE.IsEnabled = true;
                    }

                    CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                    AllowEdits = true;

                    CL_HESABDARI.SETSECURITY(this.GetType().Name, "PFACTFR", new WindowInteropHelper(this).Handle, this.GetType().Name);
                }
            }

            CUST_NO.Focus();
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord || INVO_LST_SUB.IsReadOnly) { return; }

            var editableCollectionView = INVO_LST_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
            {
                try { editableCollectionView.CancelEdit(); } catch { }
            }

            _ = AuditLogger.LogActionAsync(
                  actionType: "DELETE",
                  tableName: "پیش فاکتور",
                  recordId: NUMBER.Text,
                  oldValue: "TAG = 20",
                  newValue: null,
                  additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            if (INVO_LST_PISH2_DATA.Count > 0)
            {
                if (INVO_LST_SUB.SelectedItems != null && INVO_LST_SUB.SelectedItems.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        string UserMachiny = "_Systemy_";
                        string dt = DateTime.Now.ToOADate().ToString().Replace("/", ".");
                        dbms.DoExecuteSQL("INSERT INTO dbo.TR_HEAD_LST   (NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, UP_TIME, UP_DATE,OKF,UP_USER_NAME,PC_NAME,IPADD,SADER,ARZD,ARZKIND,CDDATE,CDTIME,OKDATE,OKTIME,JAY,MODAT_PPID,PEPID,PEID )" +
                            " SELECT     NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, " + dt + "   AS Expr1," + Tarikh.FullCurrentDate + " AS Expr2,OKF,'" + CL_HESABDARI.UCurrentUser() + UserMachiny + "','" + CL_HESABDARI.CurrentMachineName() + "' , '" + CL_HESABDARI.GETIPADD() + "',SADER,ARZD,ARZKIND,CDDATE,CDTIME,OKDATE,OKTIME,JAY,MODAT_PPID,PEPID,PEID  FROM dbo.HEAD_LST WHERE (NUMBER = " + NUMBER.Text + " ) And (TAG = 20)");
                        dbms.DoExecuteSQL("INSERT INTO dbo.TR_INVO_LST   (UP_TIME, UP_DATE, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA,JAY,JAYO,TKHN) SELECT    " + dt + "   AS Expr1," + Tarikh.FullCurrentDate + " AS Expr2, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K , FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA,JAY,JAYO,TKHN FROM dbo.INVO_LST WHERE (NUMBER = " + NUMBER.Text + ") And (TAG = 20)");

                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < INVO_LST_SUB.SelectedItems.Count; i++)
                        {
                            var item = INVO_LST_SUB.SelectedItems[i];

                            if (CL_LMethods.IsNewPlaceHolder(INVO_LST_SUB, item)) // Check if the item is a new placeholder Row
                            {
                                continue; // Skip deletion for new placeholder items
                            }

                            var _id_ = item.GetType().GetProperty("id").GetValue(item);

                            if (item.GetType().GetProperty("id").GetValue(item) is null)
                            {
                            }
                            else
                            {
                                try
                                {
                                    //بررسی موجودی در صورت امکان حذف
                                    var items = new List<object> { item };
                                    var (errorMessages, _, _, _) =
                                        IVM.CheckInventoryAndExecuteQuery<int?>(items, $@"DELETE FROM dbo.INVO_LST WHERE id = {_id_}");

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
                        }

                        GoGheymateUpdator();
                        INVO_LST_SUB_ReGetData();
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                {
                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG}");
                            RefreshAfterDelete();
                        }
                        catch (SqlException ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این  پیش فاکتور دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                                return;
                            }
                            else
                            {
                                new Msgwin(false, "به دلیل بروز خطا در پایگاه داده این پیش فاکتور حذف نشد").ShowDialog();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            new Msgwin(false, "خطا در انجام علملیات حذف پیش فاکتور").ShowDialog();
                            return;
                        }
                        INVO_LST_SUB_ReGetData();
                    }
                }
            }
        }

        private void DisplayMandah()
        {
            if (Baseknow.MAND && !IsNull(CUST_NO.SelectedValue))
            {
                if (!CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue.ToString()))
                {
                    if (CUST_NO.SelectedValue != null)
                    {
                        MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                    }
                }
            }
        }

        public void OnOpenHEADLSTPISHFOROOSH2()
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 10, 1) == "5")
            {
                //if (Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == 1)
                if (Convert.ToInt32(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == 1)
                {
                    //this.CUST_NO.AutoExpand = false;
                    this.CUST_NO2.IsTabStop = false;
                }
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 29, 1) == "5" || Strings.Mid(Baseknow.OPTIONSS, 11, 2) == "21" && Baseknow.UGRP == "3")
            {
                this.CUST_NO2.IsTabStop = false;
                this.MOLAH.IsTabStop = false;
            }
            else
            {
                this.CUST_NO2.IsTabStop = true;
                this.MOLAH.IsTabStop = true;
            }

            if (!CL_HESABDARI.LETSGO("CUSTEN"))
            {
                this.CUST_KIND.IsReadOnly = true;
            }
            else
            {
                this.CUST_KIND.IsReadOnly = false;
            }

            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {
                JAY.Visibility = Visibility.Visible;
            }
            else
            {
                this.JAY.Visibility = Visibility.Hidden;
            }
            if (CL_HESABDARI.LETSGO("okpish"))
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 65, 1) == "5")
                {
                    this.okpish.Visibility = Visibility.Visible;
                }
                else
                {
                    this.okpish.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                this.okpish.Visibility = Visibility.Hidden;
            }
            if (Convert.ToBoolean(Baseknow.SIGN))
            {
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
                SGN3.Visibility = Visibility.Visible;
            }

            if (Baseknow.GHAYM == 7)
            {
                this.MODAT_PPID.Visibility = Visibility.Visible;
                this.PEPID.Visibility = Visibility.Visible;
                this.EGHEY_LABEL.Visibility = Visibility.Visible;
                this.PEID.Visibility = Visibility.Visible;
                this.ETAKHF_LABEL.Visibility = Visibility.Visible;

                if (CL_HESABDARI.LETSGO("elamghe")) //بتواند اعلمایه قیمیت را اصلاح کند
                {
                    this.PEPID.IsEnabled = true; //Locked = true;
                    this.PEID.IsEnabled = true; //Locked = true;
                }
                else
                {
                    this.PEPID.IsEnabled = false; //Locked = false;
                    this.PEID.IsEnabled = false; //Locked = false;
                }
            }
        }
        public void Form_Current()
        {
            byte TAMIRVALUE = 250;
            if (TAMIR.SelectedValue != null)
            {
                TAMIRVALUE = Convert.ToByte(((FrameworkElement)TAMIR.SelectedValue).Tag);
            }
            if (NUMBER.Text != "0")
            {
                if (!SGN1.IsChecked ?? false)
                {
                    AllowEdits = true;
                    //Add or Delete
                    BTN_SAVE.IsEnabled = true;
                    BTN_DELETE.IsEnabled = true;
                }
                else
                {
                    AllowEdits = false;
                    //Add or Delete
                    BTN_SAVE.IsEnabled = false;
                    BTN_DELETE.IsEnabled = false;
                }

                if (TAMIRVALUE == 1 || TAMIRVALUE == 2 || TAMIRVALUE == 4)
                {
                    if (!CL_HESABDARI.LETSGO("RESERV") || TAMIRVALUE == 2)
                    {
                        this.TAMIR.IsEnabled = false;
                    }
                    else
                    {
                        this.TAMIR.IsEnabled = true;
                    }
                }
                else
                {
                    if (!NewRecord)
                    {
                        if (!(SGN1.IsChecked ?? false) && !(SGN2.IsChecked ?? false) && !(SGN3.IsChecked ?? false))
                        {
                            this.CUST_KIND.IsEnabled = true;
                            this.DATE_N.IsReadOnly = false;
                            this.MODAT_PPID.IsEnabled = true;
                            this.DEPATMAN.IsEnabled = true;
                            this.CUST_NO.IsEnabled = true;
                            this.CUST_NO2.IsEnabled = true;
                            this.MOLAH.IsReadOnly = false;
                            this.TICMBAA.IsEnabled = true;
                            this.SHARAYET.IsReadOnly = false;
                            this.MABL_HAZ.IsReadOnly = false;

                            AllowEdits = true;
                        }
                        else
                        {
                            this.CUST_KIND.IsEnabled = false;
                            this.DATE_N.IsReadOnly = true;
                            this.MODAT_PPID.IsEnabled = false;
                            this.DEPATMAN.IsEnabled = false;
                            this.CUST_NO.IsEnabled = true;
                            this.CUST_NO2.IsEnabled = false;
                            this.MOLAH.IsReadOnly = true;
                            this.TICMBAA.IsEnabled = false;
                            this.SHARAYET.IsReadOnly = true;
                            this.MABL_HAZ.IsReadOnly = true;

                            AllowEdits = false;
                            AllowDeletions = false;
                        }
                    }

                    if (CL_HESABDARI.LETSGO("RESERV"))
                    {
                        this.TAMIR.IsEnabled = true;
                    }
                    else
                    {
                        this.TAMIR.IsEnabled = false;
                    }
                }

                if (TAMIRVALUE != 0)
                {
                    CUST_KIND.IsEnabled = false; //نوع مشتری
                }
                else
                {
                    if (!CL_HESABDARI.LETSGO("CUSTEN"))
                    {
                        this.CUST_KIND.IsEnabled = false;
                    }
                    else
                    {
                        this.CUST_KIND.IsEnabled = true;
                    }
                }


                if (CL_HESABDARI.LETSGO("PFKEY"))
                {
                    if (Convert.ToBoolean(Baseknow.SIGN))
                    {
                        if (SGN1.IsChecked == true)
                        {
                            this.Command113.IsEnabled = true; //تبدیل به فاکتور
                        }
                        else
                        {
                            this.Command113.IsEnabled = false; //تبدیل به فاکتور
                        }
                    }
                    else
                    {
                        this.Command113.IsEnabled = true; //تبدیل به فاکتور
                    }
                }
                if (CL_HESABDARI.LETSGO("PFHKEY"))
                {
                    if (Convert.ToBoolean(Baseknow.SIGN))
                    {
                        if (((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false)) && TAMIRVALUE != 2)
                        {
                            this.Command116.IsEnabled = true; //تبدیل به حواله
                        }
                        else
                        {
                            this.Command116.IsEnabled = false; //تبدیل به حواله
                        }
                    }
                    else
                    {
                        this.Command116.IsEnabled = true; //تبدیل به حواله
                    }
                }
                // اگرکليد تبديل پيش فاکتور به حواله را دارد و گردش کاري فرمها با امضا هست و کليد تبديل به حواله بلايد بعداز تاييد مالي فعال شود بنابر اين
                if (CL_HESABDARI.LETSGO("PFHKEY"))
                {
                    if (Convert.ToBoolean(Baseknow.SIGN))
                    {
                        if (((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false)) && TAMIRVALUE != 2 && !CL_HESABDARI.LETSGO("KEYTPF"))
                        {
                            this.Command116.IsEnabled = true; //تبدیل به حواله
                        }
                        else if (((SGN1.IsChecked ?? false) && (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false)) && TAMIRVALUE != 2 && CL_HESABDARI.LETSGO("KEYTPF"))
                        {
                            this.Command116.IsEnabled = true; //تبدیل به حواله
                        }
                        else
                        {
                            this.Command116.IsEnabled = false; //تبدیل به حواله
                        }
                    }
                    else
                    {
                        this.Command116.IsEnabled = true; //تبدیل به حواله
                    }
                }

                if ((SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false))
                {
                    INVO_LST_SUB.IsReadOnly = true;
                }

                if (!CL_HESABDARI.LETSGO("DEFA"))
                {
                    this.DEPATMAN.IsEnabled = false;
                }

                if (TAMIRVALUE == 2 || TAMIRVALUE == 3)
                {
                    this.okpish.IsEnabled = false;
                }
                else
                {
                    this.okpish.IsEnabled = true;
                }
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "PFACTFR", new WindowInteropHelper(this).Handle, this.GetType().Name);
                if (!this.IsLoaded)
                {
                    this.Close();
                    return;
                }

                if (this.NUMBER.Text != "0")
                {
                    AllowDeletions = false;
                    AllowEdits = false;
                    ESLAH.IsEnabled = true;
                }
                //this.PERSONEL.Visibility = Visibility.Hidden;
                string _number_ = NUMBER.Text;
                if (double.TryParse(_number_, out double number))
                {
                    if (number > 0)
                    {
                        CL_HESABDARI.LetSigneTick(this.GetType().Name, 20, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                    }
                }
                else
                {
                    this.SGN1.IsEnabled = false;
                    this.SGN2.IsEnabled = false;
                    this.SGN3.IsEnabled = false;
                }
                if ((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false))
                {
                    Command139.IsEnabled = true;
                    Command100.IsEnabled = true;
                }
                else
                {
                    //Command139.IsEnabled = false;
                    //Command100.IsEnabled = false;
                }

                IF_NOT_IS_AZAD_Then_Lock();
            }
        }
        public void ReGetMasterData()
        {
            const string REPLACEMENT_VALUE = "dbo.HEAD_LST.";

            string InvoiceWheres = CL_LMethods.GetRestrictedSqlQuery(20).Replace(REPLACEMENT_VALUE, null);

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                InvoiceWheres = $" WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = 20 ";
            }

            var MasterHead = dbms.DoGetDataSQL<pish_view>($"SELECT * FROM dbo.pish_view {InvoiceWheres} ORDER BY NUMBER").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<pish_view>().FirstOrDefault(x => x.NUMBER.Equals(NUMBER_TO_OPEN) && x.TAG == 20);
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
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1) //[ RecordCount() - 1 ] : just ensure that stand on existing real item
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
            }



            //Update CurrentViewItem of RecordsData From Database
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as pish_view;
                var DBData = dbms.DoGetDataSQL<pish_view>($" SELECT NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.pish_view WHERE NUMBER = {HEADER.NUMBER} AND TAG = {TAG} ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(pish_view).GetProperties();
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
                NewRecord = true;
                RecordsData.View.MoveCurrentToLast();

                ////Form_Current();
            }
            else
            {
                UiDataUpdate();

                Form_Current();
            }
        }
        public void ClearFreshNew()
        {
            AllowEdits = true;

            INVO_LST_PISH2_DATA?.Clear();
            INVO_LST_SUB.IsReadOnly = true;

            NUMBER.Text = null;
            USER_NAME.Text = Baseknow.UUSER;
            DATE_N.Text = Tarikh.FullCurrentDate;
            DEPATMAN.SelectionChanged -= DEPATMAN_SelectionChanged;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh();
            DEPATMAN.SelectionChanged += DEPATMAN_SelectionChanged;

            CUST_KIND.SelectionChanged -= CUST_KIND_SelectionChanged;
            CUST_KIND.SelectedValue = null; CUST_KIND.Items.Refresh();
            CUST_KIND.SelectionChanged += CUST_KIND_SelectionChanged;
            CUST_KIND.IsEnabled = true;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.Text = null;
            PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            JAY.IsChecked = false;
            TICMBAA.IsChecked = false;
            OKF.IsChecked = false;
            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            CUST_NO.SelectionChanged -= CUST_NO_SelectionChanged;
            CUST_NO.SelectedValue = null; CUST_NO.Items.Refresh();
            CUST_NO.SelectionChanged += CUST_NO_SelectionChanged;

            TAMIR.SelectionChanged -= TAMIR_SelectionChanged;
            TAMIR.SelectedIndex = 0; TAMIR.Items.Refresh();
            TAMIR.SelectionChanged += TAMIR_SelectionChanged;

            if (CL_Generaly.IsGHAYM_7)
            {
                MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                MODAT_PPID.SelectedValue = null; MODAT_PPID.Items.Refresh();
                MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
            }

            PEPID.SelectedValue = null; PEPID.Items.Refresh();
            PEID.SelectedValue = null; PEID.Items.Refresh();

            MOLAH.Text = null;
            MAS.Text = "0";
            TAKHFIF.Text = "0";
            MBAA.Text = "0";
            MABL_HAZ.Text = "0";
            SHARAYET.Text = null;

            Text59.Text = "0";
            MOGU.Text = "0";

            //Form_Current();

            CUST_NO.Focus();
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as pish_view;

                NUMBER.Text = HEADER.NUMBER.ToString();
                USER_NAME.Text = HEADER.USER_NAME;
                DATE_N.Text = HEADER.DATE_N.ToString();


                DEPATMAN.SelectionChanged -= DEPATMAN_SelectionChanged;
                DEPATMAN.SelectedValue = HEADER.DEPATMAN; DEPATMAN.Items.Refresh();
                DEPATMAN.SelectionChanged += DEPATMAN_SelectionChanged;

                CUST_KIND.SelectionChanged -= CUST_KIND_SelectionChanged;
                CUST_KIND.SelectedValue = HEADER.CUST_KIND; CUST_KIND.Items.Refresh();
                CUST_KIND.SelectionChanged += CUST_KIND_SelectionChanged;

                JAY.IsChecked = HEADER.JAY;
                TICMBAA.IsChecked = HEADER.TICMBAA;
                OKF.IsChecked = HEADER.OKF;


                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1 ?? false);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2 ?? false);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3 ?? false);

                SGN1usid.Tag = null; SGN2usid.Tag = null; SGN3usid.Tag = null;

                if (HEADER?.sgn1usid != null)
                {
                    SGN1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                }
                if (HEADER?.sgn2usid != null)
                {
                    SGN2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                }
                if (HEADER?.sgn3usid != null)
                {
                    SGN3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);
                }

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                CUST_NO.SelectionChanged -= CUST_NO_SelectionChanged;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + HEADER.CUST_NO + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;

                    if (CUST_NO.ItemsSource == null)
                    {
                        CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                    }

                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = null;
                    CUST_NO.SelectedValue = thevalue;
                    CUST_NO.Items.Refresh();
                }
                CUST_NO.SelectionChanged += CUST_NO_SelectionChanged;

                TAMIR.SelectionChanged -= TAMIR_SelectionChanged;
                // Set the SelectedValue based on the Content
                //TAMIR.SelectedValue = TAMIR.Items.Cast<ComboBoxItem>().FirstOrDefault(item => Convert.ToDouble(item.Tag) == HEADER.TAMIR)?.Tag;
                if (HEADER?.TAMIR != null)
                    TAMIR.SelectedIndex = (int)HEADER.TAMIR; TAMIR.Items.Refresh();

                TAMIR.SelectionChanged += TAMIR_SelectionChanged;


                if (HEADER?.PEPID != null)
                {
                    PEPID.SelectedValue = HEADER.PEPID; PEPID.Items.Refresh();
                }
                if (HEADER?.PEID != null)
                {
                    PEID.SelectedValue = HEADER.PEID; PEID.Items.Refresh();
                }

                MODAT_PPID_Enter(); //بروز رسانی داده های نحوه پرداخت بر اساس داده ها وارد شده

                if (CL_Generaly.IsGHAYM_7)
                {
                    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                    if (HEADER?.MODAT_PPID != null)
                    {
                        MODAT_PPID.SelectedValue = HEADER.MODAT_PPID; MODAT_PPID.Items.Refresh();
                    }
                    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                }


                MOLAH.Text = HEADER.MOLAH;
                MAS.Text = string.IsNullOrEmpty(HEADER.MAS.ToStringNullSafe()) ? "0" : HEADER.MAS.ToStringNullSafe();
                TAKHFIF.Text = string.IsNullOrEmpty(HEADER.TAKHFIF.ToStringNullSafe()) ? "0" : HEADER.TAKHFIF.ToStringNullSafe();
                MBAA.Text = string.IsNullOrEmpty(HEADER.MBAA.ToStringNullSafe()) ? "0" : HEADER.MBAA.ToStringNullSafe();
                MABL_HAZ.Text = string.IsNullOrEmpty(HEADER.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER.MABL_HAZ.ToStringNullSafe();
                SHARAYET.Text = HEADER?.SHARAYET?.ToStringNullSafe();


                INVO_LST_SUB_ReGetData(); //Load DataGrid's data

                MOGU.Text = "0";

                DisplayMandah();

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
                var itemToRemove = RecordsData.View.CurrentItem as pish_view;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<pish_view>;
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
            var itemtoadd = dbms.DoGetDataSQL<pish_view>($"SELECT NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.pish_view WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG} ").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<pish_view>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                ////MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
            }
        }
        public void INVO_LST_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {
                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
																						 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
																					   	 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
																					 	 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
																	FROM            dbo.INVO_LST LEFT OUTER JOIN
																						 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
																						 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
																						 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE        (dbo.INVO_LST.TAG = {TAG}) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();
                INVO_LST_PISH2_DATA?.Clear();
                foreach (var item in QRE_LST)
                {
                    INVO_LST_PISH2_DATA.Add(item);
                }

                DisplaySumPrices();

                if (INVO_LST_SUB_IsFocused && INVO_LST_SUB.SelectedIndex > -1)
                {
                    CL_LMethods.FocusCellReadyToEdit(INVO_LST_SUB, "ANBAR", INVO_LST_SUB.SelectedIndex, false);
                }
            }
        }

        private void CheckBlockCust()//بررسی مسددودی یا بلاک بودن یک حساب
        {
            if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
            {
                CUST_NO.SelectedItem = null;
                //MessageBox.Show(" حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                universControl.PopNotifyShow(" حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }
        private void CheckCreditCust()//بررسی اعتباری ریالی یک حساب
        {
            if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToString())) == false)
                {
                    CUST_NO2.SelectedItem = null;
                    universControl.PopNotifyShow("دقت کنيد اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!", Pop1, Pop1Text1, Pop_Border1, "#FFDC9E18");
                }
            }
        }
        private void IF_NOT_IS_AZAD_Then_Lock()
        {
            //1.
            if (CL_HESABDARI.LETSGO("TFTMLOCK")) //ستون تخفیفات در فاکتور فروش قفل شود
            {
                N_KOL_COL.IsReadOnly = true;
                N_MOIN_COL.IsReadOnly = true;
            }
            else
            {
                N_KOL_COL.IsReadOnly = false;
                N_MOIN_COL.IsReadOnly = false;
            }

            //تخفیف: N_KOL_COL
            //ت.ن % : TKHN_COL
            //مبلغ تخفیف: N_MOIN_COL
            //2.
            if (Baseknow.GHAYM == 7)
            {
                //نحوه پرداخت آزاد انتخاب شده
                if (MODAT_PPID.SelectedIndex == 0)
                {
                    MABL_COL.IsReadOnly = false;
                    MABL_K_COL.IsReadOnly = false;
                    N_KOL_COL.IsReadOnly = false;
                    TKHN_COL.IsReadOnly = false;
                    N_MOIN_COL.IsReadOnly = false;
                }
                else
                {
                    MABL_COL.IsReadOnly = true;
                    MABL_K_COL.IsReadOnly = true;
                    TKHN_COL.IsReadOnly = true;
                    N_KOL_COL.IsReadOnly = true;
                    N_MOIN_COL.IsReadOnly = true;
                }
            }

        }

        private void DisplaySumPrices()
        {
            //جمع پیش فاکتور مبالغ فقط
            Text59.Text = string.IsNullOrEmpty(INVO_LST_PISH2_DATA.Sum(i => i.MABL_K).ToStringNullSafe()) ? "0" : INVO_LST_PISH2_DATA.Sum(i => i.MABL_K).ToStringNullSafe();

            //مالیات
            MBAA.Text = string.IsNullOrEmpty(INVO_LST_PISH2_DATA.Sum(i => i.IMBAA).ToStringNullSafe()) ? "0" : INVO_LST_PISH2_DATA.Sum(i => i.IMBAA).ToStringNullSafe();

        }

        private void CUST_KIND_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_KIND.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox TexBo = (TextBox)CUST_KIND.Template.FindName("PART_EditableTextBox", CUST_KIND);
            if (CUST_KIND.SelectedIndex == -1)
            {
                universControl.PopNotifyShow("لطفا یک نوع مشتری انتخاب نمایید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }
        }
        private void CUST_KIND_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady == true)
            {
                if (CUST_KIND.SelectedItem != null)
                {
                    GoGheymateUpdator();
                    INVO_LST_SUB_ReGetData();
                }
            }
        }

        private void CUST_NO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void CUST_NO_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (HeaderIsValid(false))
            //    INVO_LST_SUB.IsReadOnly = false;
            //else
            //    INVO_LST_SUB.IsReadOnly = true;


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
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("HEAD_LST_PISHFROOSH2", I_AM_PISHFACTOR);
                CMBSearch.ShowDialog();
                if (CUST_NO.SelectedValue is null)
                {
                    return;
                }
            }
            else if (Information.IsNumeric(CUTSNO_TEX.Text))
            {
                try
                {
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " and NUMBER = 1 and tNUMBER = " + CUTSNO_TEX.Text).ToList();
                    if (rst.Count == 1)
                    {
                        var _data_hes = rst.FirstOrDefault()?.n_kol + "-" + rst.FirstOrDefault()?.NUMBER + "-" + rst.FirstOrDefault()?.tNUMBER;
                        var _data_name = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE hes = N'{_data_hes}'").FirstOrDefault();
                        if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == _data_hes))
                        {
                            ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = _data_hes, NAME = _data_name });
                        }
                        CUST_NO.Items.Refresh();
                        CUST_NO.SelectedValue = _data_hes;
                        this.CUST_NO.SelectedValue = _data_hes;
                    }
                    else
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.Text = null;
                        CUST_NO.Items.Refresh();
                        return;
                    }
                }
                catch (Exception) { }
            }
            else
            {
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + CUTSNO_TEX.Text + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;
                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = null;
                    CUST_NO.SelectedValue = thevalue;
                    CUST_NO.Items.Refresh();
                }
                else
                {
                    CUST_NO.SelectedValue = null;
                    CUST_NO.Text = null;
                    CUST_NO.Items.Refresh();
                    return;
                }
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
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

            if (NowIsReady && CUST_NO?.SelectedItem != null && !string.IsNullOrEmpty(CUST_NO?.Text))
            {
                if (Convert.ToDouble(Strings.Mid(Baseknow.OPTIONSS, 19, 1)) == 5)
                {
                    var selectedCustomer = dbms.DoGetDataSQL<Custom2_CUST_HESAB>($"SELECT TOP 1 CUST_COD FROM dbo.CUST_HESAB WHERE hes = N'{CUST_NO.SelectedValue}'").FirstOrDefault();
                    if (selectedCustomer?.CUST_COD != null)
                    {
                        CUST_KIND.SelectedValue = selectedCustomer.CUST_COD; CUST_KIND.Items.Refresh();
                    }
                    else if (selectedCustomer != null)
                    {
                        universControl.PopNotifyShowUp("نوع مشتری در تعریف مشتری مشخص نشده است ضروری است آنرا تعریف کنید ! ", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                    }
                }
            }


        }
        private void CUST_KIND_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void MAS_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(MAS.Text) || MAS.Text == "0" && MODAT_PPID.SelectedIndex == 0)
            {
                universControl.PopNotifyShow("مدت توافق را وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }

        private void MODAT_PPID_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (HeaderIsValid(false))
            //    INVO_LST_SUB.IsReadOnly = false;
            //else
            //    INVO_LST_SUB.IsReadOnly = true;
        }
        private void MODAT_PPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (MODAT_PPID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (CL_Generaly.IsGHAYM_7)
            {
                TextBox TexBo = (TextBox)MODAT_PPID.Template.FindName("PART_EditableTextBox", MODAT_PPID);
                if (MODAT_PPID.SelectedIndex == -1)
                {
                    universControl.PopNotifyShow("لطفا نحوه پرداختی را انتخاب نمایید", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    try
                    {
                        MODAT_PPID.PreviewLostKeyboardFocus -= MODAT_PPID_PreviewLostKeyboardFocus;
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                        {
                            MAS.SetFocusToTextBox();
                        }));
                        MODAT_PPID.PreviewLostKeyboardFocus += MODAT_PPID_PreviewLostKeyboardFocus;
                    }
                    catch { }
                }
            }

        }
        private void MODAT_PPID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady == true && CL_Generaly.IsGHAYM_7)
            {
                if (MODAT_PPID.SelectedValue is null)
                {
                    universControl.PopNotifyShow("نحوه پرداخت نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                var IsSavedFirst = NUMBER.Text != "0" && CL_LMethods.IsNumeric(NUMBER.Text);

                if (MODAT_PPID.SelectedItem != null && MODAT_PPID.SelectedValue != null)
                {
                    if (int.TryParse(MODAT_PPID.SelectedValue.ToString(), out int selectedPpid))
                    {
                        #region BeforeUpdate (Using selectedPpid)
                        if (!CL_HESABDARI.LETSGO("AZADPAY") && selectedPpid == 0)
                        {
                            universControl.PopNotifyShow(" شما اجازه قيمت گذاري آزاد  نداريد ", Pop1, Pop1Text1, Pop_Border1);
                            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                            MODAT_PPID.SelectedIndex = -1; // Be careful, might re-trigger event
                            MODAT_PPID.Items.Refresh();
                            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                            return;
                        }
                        #endregion


                        if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP selectedItem)
                        {
                            // اگر آیتم انتخاب‌شده ممنوعه (مثلاً IsTemporary == true)
                            if (selectedItem.IsTempyDisplay)
                            {
                                MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

                                e.Handled = true;

                                if (e.RemovedItems.Count > 0)
                                {
                                    var previousItem = e.RemovedItems[0] as PRICE_PAYNO_MODATP;
                                    MODAT_PPID.SelectedItem = previousItem;
                                }

                                MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

                                universControl.PopNotifyShowUp($"این گزینه قابل انتخاب نیست : {selectedItem?.PPAME}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                            }
                        }

                        //AfterUpdate
                        GoGheymateUpdator();
                        #region AfterUpdate (Using selectedPpid)
                        int modt;
                        // Use selectedPpid directly
                        modt = System.Convert.ToInt32(CL_HESABDARI.Getmodat(selectedPpid)); // Assuming Getmodat expects int

                        if (modt != Convert.ToInt32(MAS.Text)) // Be careful converting MAS.Text too! Use TryParse here as well if needed.
                        {
                            MAS.Text = modt.ToString();
                        }

                        // Use selectedPpid for the check
                        if (selectedPpid == 0)
                        {
                            this.MAS.IsReadOnly = false;
                            MAS.Focus();
                        }
                        else
                        {
                            this.MAS.IsReadOnly = true;
                        }
                        #endregion

                        // Consider potential issues with calling these within SelectionChanged
                        //MODAT_PPID_Enter(); // Re-populates the ComboBox
                        IF_NOT_IS_AZAD_Then_Lock(); // Locks/unlocks based on selection
                        INVO_LST_SUB_ReGetData(); // Refreshes grid data

                        // Focus logic using selectedPpid
                        if (selectedPpid == 0)
                        {
                            MAS.Focus();
                        }
                    }
                    else
                    {
                        universControl.PopNotifyShow("مقدار انتخاب شده نامعتبر است.", Pop1, Pop1Text1, Pop_Border1); // "Selected value is invalid."
                        return;
                    }
                }

            }
        }
        private void ___MODAT_PPID_Enter()
        {
            if (Baseknow.GHAYM.ToString() != "7")
            {
                return;
            }

            int MSI = -1;
            if (DEPATMAN.SelectedItem == null)
            {
                universControl.PopNotifyShow(" واحد نميتواند خالي باشد ", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                if (MODAT_PPID.SelectedValue != null)
                {
                    MSI = Convert.ToInt32(MODAT_PPID.SelectedValue);
                }

                //PEID
                MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

                var _PEID_ = dbms.DoGetDataSQL<int?>("SELECT PEID FROM dbo.PRICE_ELAMIETF WHERE (PEDATE <= " + DATE_N.Text.ToRawTarikh() + ") And (PEPDEPART = " + DEPATMAN.SelectedValue + ") ORDER BY PEID DESC").FirstOrDefault();
                if (_PEID_ != null) //Has Value
                {
                    MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + _PEID_ + ")  union  SELECT 0, 'آزاد', 0").ToList();
                    MODAT_PPID.DisplayMemberPath = "PPAME";
                    MODAT_PPID.SelectedValuePath = "PPID";
                }
                else
                {
                    if (PEID.SelectedValue != null)
                    {
                        MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + this.PEID.SelectedValue + ")  union  SELECT 0, 'آزاد', 0").ToList();
                    }
                    else
                    {
                        MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
                        //universControl.PopNotifyShow("براي اين تاريخ و اين واحد نحوه پرداخت تعيين نشده", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }

                if (MSI > -1)
                {
                    MODAT_PPID.SelectedValue = MSI; MODAT_PPID.Items.Refresh();
                }

                MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
            }
        }

        private void MODAT_PPID_Enter()
        {
            if (Baseknow.GHAYM.ToString() != "7")
                return;

            // ۱. بدست آوردن مقدار فعلی انتخاب‌شده (MSI)
            int currentSelectedPPID = -1;

            var CurrentRecord = RecordsData.View.CurrentItem as pish_view;

            if (CurrentRecord?.MODAT_PPID != null)
            {
                currentSelectedPPID = Convert.ToInt32(CurrentRecord.MODAT_PPID);
            }

            // ۲. بررسی اینکه آیا DEPARTMENT انتخاب شده یا نه
            if (DEPATMAN.SelectedItem == null)
            {
                universControl.PopNotifyShow("واحد نميتواند خالي باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            string tarikhRaw = DATE_N.Text.ToRawTarikh();
            if (!long.TryParse(tarikhRaw, out long tarikhValue))
            {
                universControl.PopNotifyShow("تاریخ نمی‌تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            // ۴. واکشی لیست فیلترشده از PRICE_PAYNO براساس lastPEID
            List<PRICE_PAYNO_MODATP> filteredList;
            if (PEID.SelectedValue != null)
            {
                filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + this.PEID.SelectedValue + ")  union  SELECT 0, 'آزاد', 0").ToList();
            }
            else
            {
                // ۳. گرفتن PEID آخرین اطلاعیه (براساس تاریخ فاکتور و دپارتمان)
                string tarikh = DATE_N.Text.ToRawTarikh();
                int departId = Convert.ToInt32(DEPATMAN.SelectedValue);
                string sqlGetPEID =
                    "SELECT TOP (1) PEID " +
                    "FROM dbo.PRICE_ELAMIETF " +
                    $"WHERE (PEDATE <= {tarikh}) AND (PEPDEPART = {departId}) " +
                    "ORDER BY PEID DESC";

                int? lastPEID = dbms.DoGetDataSQL<int?>(sqlGetPEID).FirstOrDefault();

                if (lastPEID != null)
                {
                    string sqlFiltered =
                        "SELECT P.PPID, P.PPAME, P.MODAT " +
                        "FROM PRICE_PAYNO P " +
                        "INNER JOIN PRICE_ELAMIETF_DTL D ON P.PPID = D.PPID " +
                        $"WHERE D.PEID = {lastPEID} " +
                        "UNION " +
                        "SELECT 0, 'آزاد', 0";

                    filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlFiltered).ToList();
                }
                else
                {

                    if (CurrentRecord?.PEID != null)
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + CurrentRecord.PEID + ")  union  SELECT 0, 'آزاد', 0").ToList();
                    }
                    else
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
                    }
                }
                return;
            }

            // ۵. اگر مقدار ذخیره‌شده (currentSelectedPPID) جزو filteredList نبود:
            if (currentSelectedPPID > -1 && !filteredList.Any(p => p.PPID == currentSelectedPPID))
            {
                // ۵.۱ واکشی رکورد ذخیره‌شده از جدول اصلی (Full List)
                string sqlGetSaved = $"SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO WHERE PPID = {currentSelectedPPID}";
                PRICE_PAYNO_MODATP savedItem = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlGetSaved).FirstOrDefault();

                // ۵.۲ اگر آن رکورد اصلاً در جدول اصلی هم وجود داشت، به انتهای filteredList اضافه کن
                if (savedItem != null)
                {
                    savedItem.IsTempyDisplay = true;
                    filteredList.Add(savedItem);
                }
                else
                {
                    currentSelectedPPID = -1; // و مقدار انتخابی را ریست می‌کنیم
                }
            }

            // ۶. حالا ItemsSource جدید را تنظیم کن و SelectedValue را ست کن
            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

            MODAT_PPID.ItemsSource = filteredList;
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";

            if (currentSelectedPPID > -1)
            {
                MODAT_PPID.SelectedValue = currentSelectedPPID;
            }
            else
            {
                // اگر هیچ مقدار ذخیره‌ای معتبر نبود، می‌توانیم به‌دلخواه اولین آیتم را ست کنیم
                // یا بگذاریم کاربر خودش انتخاب کند:
                MODAT_PPID.SelectedIndex = -1;
            }

            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
        }

        private void TAMIR_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (HeaderIsValid(false))
            //    INVO_LST_SUB.IsReadOnly = false;
            //else
            //    INVO_LST_SUB.IsReadOnly = true;
        }
        private void TAMIR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TAMIR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox TexBo = (TextBox)TAMIR.Template.FindName("PART_EditableTextBox", TAMIR);
            if (TAMIR.SelectedIndex == -1)
            {
                universControl.PopNotifyShow("لطفا یک وضعیت برای پیش فاکتور انتخاب نمایید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }
        private void TAMIR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady == true)
            {
                if (TAMIR.SelectedItem != null && TAMIR.Text != null)
                {
                    #region BeforeUpdate
                    var min = default(double);
                    if (CL_HESABDARI.LETSGO("RESERVGH") && ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "4") //0;"عادی";1;"رزرو شده";2;"تبدیل به حواله";3;"لغو شد";4;"رزو قطعی"
                    {
                        universControl.PopNotifyShow("شما اجازه رزرو قطعی ندارید برای رزرو قطعی درخواست خود را به مدیر مالی ارسال کنید", Pop1, Pop1Text1, Pop_Border1);
                        TAMIR.SelectedIndex = 0;
                        return;
                    }
                    if (((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "1" || ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "4")
                    {
                        if (Strings.Mid(Baseknow.OPTIONSS, 59, 1) == "5")
                        {
                            var rst2 = dbms.DoGetDataSQL<Custom6_INVO>("SELECT     ANBAR, CODE, SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE NUMBER = " + NUMBER.Text + " and tag = 20 GROUP BY ANBAR, CODE").ToList();
                            foreach (var item in rst2)
                            {
                                var rst3 = dbms.DoGetDataSQL<Custom_STUFDEF0>("select min_m from STUF_DEF where code = '" + item.CODE + "'").ToList();

                                if (rst3.Count() == 1)
                                {
                                    if (ReferenceEquals(rst3.Select(x => x.MIN_M).FirstOrDefault(), null))
                                    {
                                        min = item.MEGHk;
                                    }
                                    else
                                    {
                                        min = CL_HESABDARI.Getmin((item.ANBAR), (item.CODE)) + (item.MEGHk);
                                    }
                                }

                                var RST_mand = (double)dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + item.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + item.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + item.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + item.ANBAR + ")").FirstOrDefault();
                                if (RST_mand > 0)
                                {
                                    if (Math.Round(RST_mand, Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, Convert.ToInt32(Baseknow.DIG)) && item.ANBAR != 0)
                                    {
                                        universControl.PopNotifyShow(" خروج كالای  " + item.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(item.CODE)) + " از انبار موجودی را به مقدار غیر مجاز كاهش میدهد.برگه قابل تبدیل نیست" + " حداقل موجودی لازم  :" + (min - RST_mand), Pop1, Pop1Text1, Pop_Border1);
                                    }
                                };
                            }
                        }
                    }
                    if (((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "1" || ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "4")
                    {
                        INVO_LST_SUB.IsReadOnly = false;
                    }
                    else
                    {
                        INVO_LST_SUB.IsReadOnly = true;
                    }
                    #endregion

                    #region AfterUpdate
                    var RST = dbms.DoGetDataSQL<head_lst_log>("SELECT * FROM HEAD_LST_LOG WHERE TAGG = 20 AND NUMBER =" + NUMBER.Text + " ORDER BY IDD DESC").ToList();
                    string where = " WHERE TAGG = 20 AND NUMBER =" + NUMBER.Text;
                    if (RST.Count > 0)
                    {
                        if (RST.Select(x => x.RESERVED).FirstOrDefault().ToString() != ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString())
                        {

                            dbms.DoExecuteSQL("INSERT INTO dbo.head_lst_log " +
                                           "(" +
                                               "UP_DATE," +
                                               "NUMBER," +
                                               "TAGG," +
                                               "RESERVED," +
                                               "UP_USER_NAME," +
                                               "fieldname," +
                                               "UDATEF" +
                                           ")" +
                                           "VALUES" +
                                           $"(  GETDATE() ," +               // UP_DATE - datetime
                                              $" {NUMBER.Text}, " +            // NUMBER - float
                                              $" {20}, " +           // TAGG - float
                                              $" {Convert.ToDouble(((FrameworkElement)TAMIR.SelectedValue).Tag.ToString())}, " +           // RESERVED - float
                                              $" N'{Baseknow.UUSER}', " +           // UP_USER_NAME - nvarchar(40)
                                              $" N'RESERVED', " +           // fieldname - nvarchar(50)
                                              $" {Tarikh.FullCurrentDate}" +           // UDATEF - bigint
                                              " );");
                        }
                    }
                    else
                    {
                        dbms.DoExecuteSQL("INSERT INTO dbo.head_lst_log " +
                                         "(" +
                                             "UP_DATE," +
                                             "NUMBER," +
                                             "TAGG," +
                                             "RESERVED," +
                                             "UP_USER_NAME," +
                                             "fieldname," +
                                             "UDATEF" +
                                         ")" +
                                         "VALUES" +
                                         $"(  GETDATE()," +               // UP_DATE - datetime
                                            $" {NUMBER.Text}, " +            // NUMBER - float
                                            $" {20}, " +           // TAGG - float
                                            $" {Convert.ToDouble(((FrameworkElement)TAMIR.SelectedValue).Tag.ToString())}, " +           // RESERVED - float
                                            $" N'{Baseknow.UUSER}', " +           // UP_USER_NAME - nvarchar(40)
                                            $" N'RESERVED', " +           // fieldname - nvarchar(50)
                                            $" {Tarikh.FullCurrentDate}" +           // UDATEF - bigint
                                            " );");
                    }
                    if (((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "1" || ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "4")
                    {
                        INVO_LST_SUB.IsReadOnly = false;
                    }
                    else
                    {
                        INVO_LST_SUB.IsReadOnly = true;
                    }
                    #endregion
                }
            }
        }

        private void estelam_Click(object sender, RoutedEventArgs e)
        {
            if (I_AM_PISHFACTOR == null || CUST_NO.SelectedValue == null || !CL_LMethods.IsNumeric(NUMBER.Text) || !CL_LMethods.IsNumeric(DATE_N.Text.ToRawTarikh()))
            {
                return;
            }

            mesagL msgbarrasi = new mesagL(CUST_NO.SelectedValue.ToString(), Convert.ToInt64(NUMBER.Text), Convert.ToInt64(DATE_N.Text.ToRawTarikh()), _openargs: 1, I_AM_PISHFACTOR);
            msgbarrasi.ShowDialog();
        }
        private void okpish_Click(object sender, RoutedEventArgs e)
        {
            if (I_AM_PISHFACTOR == null || CUST_NO.SelectedValue == null || !CL_LMethods.IsNumeric(NUMBER.Text) || !CL_LMethods.IsNumeric(DATE_N.Text.ToRawTarikh()))
            {
                return;
            }

            if (!(bool)SGN1.IsChecked)
            {
                mesagL msgbarrasi = new mesagL(CUST_NO.SelectedValue.ToString(), Convert.ToInt64(NUMBER.Text), Convert.ToInt64(DATE_N.Text.ToRawTarikh()), _openargs: 0, I_AM_PISHFACTOR);
                msgbarrasi.ShowDialog();
                Form_Current();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "اين پيش فاکتور قبلا تاييد شده است ");
                msgwin.ShowDialog();
            }
        }
        private void TICMBAA_Click(object sender, RoutedEventArgs e)
        {
            BTN_SAVE_Click(null, null);

            if (!isSavedSuccess) { return; }

            double SMBAA = 0;
            if (TICMBAA.IsChecked ?? false)
            {
                var rst = dbms.DoGetDataSQL<INVO_LST>("SELECT NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, id, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO FROM INVO_LST WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {TAG}").ToList();
                string where = " WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {TAG}";
                foreach (var item in rst)
                {
                    var rst2 = dbms.DoGetDataSQL<Custom4_INVO>("SELECT CMBAA ,code FROM STUF_DEF where code = '" + item.CODE + "'").FirstOrDefault();
                    if (rst2 != null)
                    {
                        if (rst2.CMBAA ?? false)
                        {
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET IMBAA = {Math.Round((double)((item.MABL_K - item.N_MOIN) * Convert.ToDouble(CL_HESABDARI.GetArzesh(item.CODE)) / 100))} " +
                                $" {where} AND CODE = N'{rst2.CODE}' ");
                            SMBAA = SMBAA + Math.Round((double)((item.MABL_K - item.N_MOIN) * Convert.ToDouble(CL_HESABDARI.GetArzesh(item.CODE)) / 100));
                        }
                        else
                        {
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET IMBAA = 0 {where} ");
                        }
                    }
                }
                if (SMBAA != Convert.ToDouble(MBAA.Text) && SMBAA > 0)
                {
                    MBAA.Text = SMBAA.ToString();
                }

                INVO_LST_SUB_ReGetData();

                if (INVO_LST_PISH2_DATA.Count != 0)
                {
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<INVO_LST>("SELECT NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, id, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO FROM INVO_LST WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {TAG}").ToList();
                string where = " WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {TAG}";
                foreach (var item in rst)
                {
                    dbms.DoExecuteSQL($"UPDATE INVO_LST SET IMBAA = 0 {where} ");
                }
                if (Convert.ToDouble(MBAA.Text) > 0)
                {
                    MBAA.Text = "0";
                }
                if (Convert.ToInt32(TICMBAA.IsChecked) == 0)
                {
                    MBAA.IsReadOnly = false;
                }
                else
                {
                    MBAA.IsReadOnly = true;
                }

                INVO_LST_SUB_ReGetData();
                if (INVO_LST_PISH2_DATA.Count != 0)
                {
                }
            }
        }
        private void JAY_Click(object sender, RoutedEventArgs e)
        {
            BTN_SAVE_Click(null, null);
            if (!isSavedSuccess)
            {
                return;
            }

            JAYEZAH();
        }

        private void JAYEZAH()
        {
            try
            {
                // فرض: NUMBER یک TextBox است یا مشابه آن
                if (!double.TryParse(NUMBER.Text, out double invoiceNumber))
                {
                    new Msgwin(false, "شماره فاکتور نامعتبر است!").ShowDialog();
                    return;
                }

                short invoiceTag = 20; // مقدار ثابت
                bool isRewardSystemActive = JAY.IsChecked ?? false; // CheckBox named JAY
                int performingUserId = (int)Baseknow.USERCOD; // فرض بر اینکه UID اینجاست

                // اجرای stored procedure
                dbms.OpenStoredProcedure("sp_ManageInvoiceRewards", new Dictionary<string, object>
                {
                    { "@InvoiceNumber", invoiceNumber },
                    { "@InvoiceTag", invoiceTag },
                    { "@IsRewardSystemActive", isRewardSystemActive },
                    { "@PerformingUserID", performingUserId }
                });

                // بروزرسانی دیتاگرید جایزه
                INVO_LST_SUB_ReGetData();

                universControl.PopNotifyShow($".وضعیت جایزه بروز شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در پردازش جوایز: {ex.Message}").ShowDialog();
            }
        }

        private void JAYEZAH_ERLIEAR()
        {
            int RDD;
            if (JAY.IsChecked ?? false)
            {
                dbms.DoExecuteSQL("DELETE FROM INVO_LST  WHERE JAY <> 0 AND TAG = 20 AND NUMBER = " + NUMBER.Text);
                var Jrst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 20 AND NUMBER = " + NUMBER.Text).FirstOrDefault();
                var rst = dbms.DoGetDataSQL<JAYMD>("SELECT dbo.INVO_LST.VAHED_K, dbo.invo_edam.idd, dbo.INVO_LST.CODE, dbo.invo_edam.VAHED, dbo.invo_edam.MEGHTA, dbo.invo_edam.MEGHJAY, dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.JAY , dbo.INVO_LST.JAYO, dbo.INVO_LST.id FROM dbo.INVO_LST INNER JOIN dbo.invo_edam ON dbo.INVO_LST.id = dbo.invo_edam.idd WHERE     (dbo.INVO_LST.TAG = 20) AND (dbo.INVO_LST.JAY = 0) AND dbo.INVO_LST.NUMBER = " + NUMBER.Text).ToList();

                foreach (var rstFields in rst) //while (!rst.EOF)
                {
                    if (string.IsNullOrEmpty(rstFields.JAYO.ToString()))
                    {
                        if (rstFields.MEGHTA > 0 && rstFields.MEGHJAY > 0)
                        {
                            if (rstFields.MEGHk / rstFields.MEGHTA >= 1)
                            {
                                var _megh = Convert.ToDouble((double)rstFields.MEGHk / (double)rstFields.MEGHTA) * (double)rstFields.MEGHJAY / CL_HESABDARI.GETVAHEDN(rstFields.CODE, (int)rstFields.VAHED);
                                var _meghk = (double)rstFields.MEGHk;
                                INVO_LST iNVO_LST = new INVO_LST()
                                {
                                    NUMBER = (double)rstFields.NUMBER,
                                    TAG = (double)rstFields.TAG,
                                    ANBAR = (int)rstFields.ANBAR,
                                    JAY = (long)rstFields.JAY,
                                    CODE = rstFields.CODE,
                                    SANAD_NO = 0,
                                    RADIF = (double)rstFields.RADIF + 1,
                                    VAHED_K = (int)rstFields.VAHED_K,

                                    MANDAH = "جایزه"
                                };
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,JAY,CODE,SANAD_NO,RADIF,VAHED_K,MEGH,MEGHk,MANDAH)
                                 VALUES ({rstFields.NUMBER},{rstFields.TAG},{rstFields.ANBAR},{rstFields.JAY},{rstFields.CODE},0,{rstFields.RADIF + 1},{rstFields.VAHED_K},{_megh},{_meghk},N'جایزه')");
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
                var Jrst2 = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 20 AND NUMBER = " + NUMBER.Text + " ORDER BY CODE, MABL_K DESC").ToList();
                RDD = 1;
                string where_condition_invo = " WHERE TAG = 20 AND NUMBER = " + NUMBER.Text + "";
                foreach (var Jrst2Fields in Jrst2)
                {
                    dbms.DoExecuteSQL($"UPDATE INVO_LST SET RADIF = {RDD} {where_condition_invo} AND id = {Jrst2Fields.id}");
                    RDD = RDD + 1;
                }
            }
            else
            {
                dbms.DoExecuteSQL("DELETE FROM INVO_LST  WHERE JAY <> 0 AND TAG = 20 AND NUMBER = " + NUMBER.Text);
            }
        }

        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            //چاپ

            if (NewRecord || INVO_LST_PISH2_DATA.Count < 1) { return; }

            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.INVOICE_PISHFROOSH2.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            double NUMBER_Pas = Convert.ToDouble(NUMBER.Text);

            report["NUMBER_PARAM"] = NUMBER.Text;

            double JAMF, KH, BAA, TF;
            JAMF = 0d;
            TF = 0d;
            BAA = 0d;
            KH = 0d;
            var jst = dbms.DoGetDataSQL<jst_JAMTAF>("SELECT     SUM(MABL_K) AS JAMF, SUM(N_MOIN) AS TF, SUM(IMBAA) AS BAA FROM dbo.INVO_LST WHERE (NUMBER = " + NUMBER_Pas + ") And (TAG = 20)").ToList();
            if (jst.Count > 0 && !ReferenceEquals(jst.FirstOrDefault(), null))
            {
                JAMF = Convert.ToDouble(jst.Select(x => x.JAMF).FirstOrDefault());
                TF = Convert.ToDouble(jst.Select(x => x.TF).FirstOrDefault());
                BAA = Convert.ToDouble(jst.Select(x => x.BAA).FirstOrDefault());
            };

            var jst2 = dbms.DoGetDataSQL<jst_KH>("SELECT     SUM(MABL_HAZ) AS KH FROM dbo.HEAD_LST WHERE (NUMBER = " + NUMBER_Pas + ") And (TAG = 20)").ToList();

            if (jst2.Count > 0 && !ReferenceEquals(jst2.FirstOrDefault(), null))
            {
                KH = Convert.ToDouble(jst2.Select(x => x.KH).FirstOrDefault());
            }
            (report.GetComponentByName("MABK") as StiText).Text = JAMF.ToString();
            (report.GetComponentByName("TF") as StiText).Text = TF.ToString();
            (report.GetComponentByName("KH") as StiText).Text = KH.ToString();

            (report.GetComponentByName("MABAA") as StiText).Text = BAA.ToString();
            (report.GetComponentByName("PAY") as StiText).Text = (JAMF + KH + BAA - TF).ToString();
            var rst = dbms.DoGetDataSQL<rst_weight>("SELECT SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 20) AND (dbo.INVO_LST.NUMBER = " + NUMBER_Pas + ")").FirstOrDefault();
            if (!ReferenceEquals(rst, null))
            {
                if (!ReferenceEquals(rst.Weight, null))
                {
                    if (CL_LMethods.IsNumeric(rst.Weight?.ToString()) && rst.Weight > 0)
                    {
                        (report.GetComponentByName("vazn") as StiText).Enabled = true;
                        (report.GetComponentByName("vazn") as StiText).Text = "وزن : " + Convert.ToString(Math.Round(Convert.ToDouble(rst.Weight)));
                    }
                    else
                    {
                        (report.GetComponentByName("vazn") as StiText).Enabled = false;
                    }
                }
            }

            double addad = JAMF + KH + BAA - TF;

            report.Dictionary.Variables.Add("Variable1", Convert.ToInt64(addad));

            (report.GetComponentByName("WIDTH_D") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("SHARAYET") as StiText).Text = SHARAYET.Text;
            (report.GetComponentByName("DATE_N_FL") as StiText).Text = "تاریخ : " + DATE_N.Text;

            ShowEmzaha(report);

            new WINRPT(report, "پیش فاکتور").Show();

            ProcLoader.Stop(Prc);
        }
        private void Command139_Click(object sender, RoutedEventArgs e)
        {
            //چاپ 1
            if (NewRecord && INVO_LST_PISH2_DATA.Count < 1) { return; }

            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.INVOICE_PISH_2_MBA.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["NUMBER_PARAM"] = NUMBER.Text;
            (report.GetComponentByName("SHARAYET") as StiText).Text = SHARAYET.Text;

            ShowEmzaha(report);

            new WINRPT(report, "پیش فاکتور").Show();
            ProcLoader.Stop(Prc);

        }
        private void Command116_Click(object sender, RoutedEventArgs e)
        {
            if ((OKF.IsChecked ?? false) || !Command116.IsEnabled)
            {
                return;
            }

            if (NewRecord || INVO_LST_PISH2_DATA.Count == 0)
            {
                new Msgwin(false, "پیش فاکتور با سطر های خالی را نمیتوان تبدیل کرد !").ShowDialog();
                return;
            }
            else
            {
                BTN_SAVE_Click(null, null);
            }

            Process Prc = ProcLoader.Start();

            long PURSANT;
            double DARSAD;
            var num = default(long);
            long MBK, CMABL = default;
            double nesba;
            var takh = default(double);
            string rptname;
            var min = default(double);
            var NOTPR = default(bool);
            double JAMFACT;
        ST:;

            PURSANT = 0L;
            DARSAD = 0d;

            try
            {
                var rst = dbms.DoGetDataSQL<int?>("select pishpross from sazman").FirstOrDefault();
                if (rst == 1)
                {
                    Msgwin msgwin = new Msgwin(false, "کاربر ديگردي در حال صدور پيش فاکتور است لطفا با کمي صبر دوباره تلاش کنيد"); msgwin.ShowDialog();
                    dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                }
                else
                {
                n2:
                    dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 1");
                    var RST2 = dbms.DoGetDataSQL<bool>("select OKF from HEAD_lst where NUMBER  = " + NUMBER.Text + " and tag = 20").FirstOrDefault();
                    if (RST2)
                    {
                        if (Strings.Mid(Baseknow.OPTIONSS, 45, 1) == "5")
                        {
                            Baseknow.Text44 = false;
                            Msgwin msgwin = new Msgwin(false, "کاربر ديگردي در حال صدور پيش فاکتور است لطفا با کمي صبر دوباره تلاش کنيد"); msgwin.ShowDialog();
                            if (Baseknow.Text44)
                            {
                                dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                                ProcLoader.Stop(Prc);
                                return;
                            }
                        }
                        else
                        {
                            dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                            Msgwin msgwin = new Msgwin(false, "اين پيش فاكتور قبلا به فاكتور تبديل شده است و اجازه تبديل مجدد نداريد"); msgwin.ShowDialog();
                            ProcLoader.Stop(Prc);
                            return;
                        }
                    };
                    if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
                    {
                        dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                        Msgwin msgwin = new Msgwin(false, "حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد"); msgwin.ShowDialog();
                        ProcLoader.Stop(Prc);
                        return;
                    }
                    else if (CUST_KIND.SelectedIndex < 0)
                    {
                        dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                        Msgwin msgwin = new Msgwin(false, "نوع مشتري نميتواند خالي باشد"); msgwin.ShowDialog();
                        ProcLoader.Stop(Prc);
                        return;
                    }
                    var appendedSharayet = $"{SHARAYET.Text ?? string.Empty} ش.پ {NUMBER.Text}";
                    if (appendedSharayet.Length > 7999)
                    {
                        new Msgwin(false, "شرایط پیش فاکتور پس از افزودن شماره پیش‌فاکتور بیش از ۸۰۰۰ کاراکتر می‌شود. لطفاً متن شرایط را کوتاه‌تر کنید.").ShowDialog();
                        return;
                    }

                    JAYEZAH();

                    var RST2_2 = dbms.DoGetDataSQL<INVO_LST>("select * from invo_lst where NUMBER  = " + NUMBER.Text + " and tag = 20").ToList();
                    string where_conditioninvonumber = " where NUMBER  = " + NUMBER.Text + " and tag = 20";
                    //while (!RST2.EOF)
                    foreach (var RST2_2Fields in RST2_2)
                    {
                        nesba = CL_HESABDARI.GETNESBAT(RST2_2Fields.CODE, (int)RST2_2Fields.VAHED_K);
                        MBK = (long)Math.Round(RST2_2Fields.MEGH * nesba * RST2_2Fields.MABL);
                        double inv_nmoin = Math.Round((double)(RST2_2Fields.N_KOL * MBK / 100)) + Math.Round((double)((MBK - Math.Round((double)(RST2_2Fields.N_KOL * MBK / 100))) * RST2_2Fields.TKHN / 100));
                        takh = (double)(takh + inv_nmoin);
                        dbms.DoExecuteSQL($"UPDATE INVO_LST SET MEGHk = {Convert.ToDouble(RST2_2Fields.MEGH * nesba)}, MABL_K = {Math.Round(RST2_2Fields.MEGH * nesba * RST2_2Fields.MABL)},N_MOIN = {inv_nmoin} {where_conditioninvonumber} AND id = {RST2_2Fields.id} ");

                        bool HichGHEYM = Baseknow.GHAYM.ToString() == "3"; //پیش فرض قیمت هیچکدام
                        //در زمان تبديل پيش فاكتور به فاكتور مبلغ فروش كنترل گردد كه اگر تغيير كرده اخطار دهد بجز مبالغ صفر
                        if (Strings.Mid(Baseknow.OPTIONSS, 51, 1) == "5" && !CL_Generaly.IsGHAYM_7 && !HichGHEYM)
                        {
                            var rst4 = dbms.DoGetDataSQL<PRT1>("select MABL_F , B_SEF from STUF_DEF where code = '" + RST2_2Fields.CODE + "'").ToList();
                            if (rst4.Count == 1)
                            {
                                if (Baseknow.GHAYM.ToString() == "2")
                                {
                                    CMABL = Convert.ToInt64(rst4.Select(x => x.MABL_F).FirstOrDefault());
                                }
                                else if (Baseknow.GHAYM.ToString() == "5")
                                {
                                    CMABL = Convert.ToInt64(rst4.Select(x => x.B_SEF).FirstOrDefault());
                                }
                                else
                                {
                                    // If Forms![BASEKNOW]![GHAYM] = 6 Then
                                    // Set RST = New ADODB.Recordset
                                    // RST.Open "SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " & Me.CUST_KIND & ") AND (TAKH_COD = N'" & Me.CODE & "')", CurrentProject.Connection, adOpenKeyset, adLockOptimistic
                                    // If RST.RecordCount > 0 Then
                                    // CMABL = RST.Fields("PRICE_M")
                                    // End If
                                    // End If
                                }
                            }
                            if (CMABL != RST2_2Fields.MABL && RST2_2Fields.MABL != 0)
                            {
                                Msgwin msgwin = new Msgwin(false, " قيمت كالاي " + RST2_2Fields.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(RST2_2Fields.CODE)) + " با قيمت سيستم منطبق نيست"); msgwin.ShowDialog();
                            };
                            var rst44 = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + RST2_2Fields.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                            if (rst44.Count > 0)
                            {
                                if (rst44.Select(x => x.MABL).FirstOrDefault() > RST2_2Fields.MABL)
                                {
                                    Msgwin msgwin = new Msgwin(false, "قيمت فروش از قيمت خريد كمتر مي باشد. " + "کد کالا : " + RST2_2Fields.CODE + " نام کالا : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(RST2_2Fields.CODE)));
                                    msgwin.ShowDialog();
                                }
                            }
                        };
                    }
                    TAKHFIF.Text = takh.ToString();
                    if (Strings.Mid(Baseknow.OPTIONSS, 59, 1) == "5")
                    {
                        var RST2_5 = dbms.DoGetDataSQL<ThePart1>("SELECT     ANBAR, CODE, SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE NUMBER = " + NUMBER.Text + " and tag = 20 GROUP BY ANBAR, CODE").ToList();
                        foreach (var RST2_5Fields in RST2_5)
                        {
                            var rst3 = dbms.DoGetDataSQL<double?>("SELECT MIN_M FROM STUF_DEF WHERE CODE = '" + RST2_5Fields.CODE + "'").ToList();
                            if (rst3.Count == 1)
                            {
                                if (string.IsNullOrEmpty(rst3.FirstOrDefault().ToString()))
                                {
                                    if (TAMIR.SelectedIndex == 1 || TAMIR.SelectedIndex == 4)
                                    {
                                        min = CL_HESABDARI.Getmin((int)RST2_5Fields.ANBAR, RST2_5Fields.CODE);
                                    }
                                    else
                                    {
                                        min = (double)RST2_5Fields.MEGHk;
                                    }
                                }
                                else if (TAMIR.SelectedIndex == 1 || TAMIR.SelectedIndex == 4)
                                {
                                    min = CL_HESABDARI.Getmin((int)RST2_5Fields.ANBAR, RST2_5Fields.CODE);
                                }
                                else
                                {
                                    min = (double)(CL_HESABDARI.Getmin((int)RST2_5Fields.ANBAR, RST2_5Fields.CODE) + RST2_5Fields.MEGHk);
                                }
                            }
                            var rst_1 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + RST2_5Fields.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + RST2_5Fields.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + RST2_5Fields.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + RST2_5Fields.ANBAR + ")").FirstOrDefault();
                            if (!ReferenceEquals(rst_1, null))
                            {
                                if (Math.Round((double)rst_1, Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, Convert.ToInt32(Baseknow.DIG)) && RST2_5Fields.ANBAR != 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, " خروج كالاي  " + RST2_5Fields.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(RST2_5Fields.CODE)) + " از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.برگه قابل تبديل نيست" + " حداقل موجودي لازم  :" + (min - rst_1)); msgwin.ShowDialog();
                                    NOTPR = true;
                                }
                            };
                        }
                    }
                    if (NOTPR)
                    {
                        Msgwin msgwin = new Msgwin(false, " به دليل كسري كالاهاي مذكور پيش فاكتور قابل تبديل نمي باشد كالاهاي مذكور را حذف يا به حد مجاز كاهش دهيد و مجددا سعي كنيد"); msgwin.ShowDialog();
                        dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                        ProcLoader.Stop(Prc);
                        return;
                    }
                    if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
                    {
                        if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToString())) == false)
                        {
                            Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!"); msgwin.ShowDialog();
                            ProcLoader.Stop(Prc);
                            return;
                        }
                    }
                    if (NUMBER.Text != "0")
                    {
                        if (string.IsNullOrEmpty(OKF.IsChecked.ToString()))
                        {
                            OKF.IsChecked = false;
                        }
                        Msgwin msgwin = new Msgwin(true, "آيا مطمئن هستيد ! پيش فاكتور به حواله تبديل شود؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            long NUM1;

                            using (IDbConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                            {
                                db.Open();
                                using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                                {
                                    var _MasMaghsad_ = db.Query<int?>($"SELECT SHAHRID FROM dbo.CUST_HESAB WHERE hes = N'{CUST_NO.SelectedValue}'", null, transaction).FirstOrDefault();

                                    //Fake Query for Lock Table
                                    db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                                    //Fake Query for Lock Table

                                    var rst_11 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=2))", null, transaction).FirstOrDefault();
                                    if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                                    {
                                        num = Baseknow.STHFR;
                                    }
                                    else
                                    {
                                        num = Convert.ToInt64(rst_11 + 1);
                                    }
                                    CDTIME = CL_HESABDARI.GTFS().ToString();
                                    var rst_f = db.Query<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + num, null, transaction).FirstOrDefault();
                                    var _NUMBER = num;
                                    var _TAG = 2;
                                    var _OKF = false;
                                    var _CUST_NO = CUST_NO.SelectedValue?.ToString();
                                    var _DATE_N = Convert.ToInt64(Tarikh.FullCurrentDate);
                                    var _USER_NAME = USER_NAME.Text;
                                    var _DEPATMAN = (int?)DEPATMAN.SelectedValue;
                                    var _CUST_KIND = (int?)CUST_KIND.SelectedValue;
                                    var _SHIFT = CL_Generaly.SHIFT_OF_USER;
                                    var _MOLAH = MOLAH.Text;
                                    var _TICMBAA = (bool?)TICMBAA.IsChecked;
                                    var _MABL_HAZ = string.IsNullOrEmpty(MABL_HAZ.Text) ? (double?)null : Convert.ToDouble(MABL_HAZ.Text.RemoveQut());
                                    var _TAKHFIF = string.IsNullOrEmpty(TAKHFIF.Text) ? (double?)null : Convert.ToDouble(TAKHFIF.Text.RemoveQut());
                                    var _MBAA = string.IsNullOrEmpty(MBAA.Text) ? (double?)null : Convert.ToDouble(MBAA.Text.RemoveQut());
                                    var _HMBAA = ((Convert.ToDouble(MBAA?.Text?.RemoveQut()) > 0) ? Baseknow.HESMBAA : (string)null)?.ToString();
                                    var _VAS = 1;
                                    var _OKDATE = Convert.ToInt64(Tarikh.FullCurrentDate);
                                    var _OKTIME = Convert.ToInt32(CL_HESABDARI.GTFS());
                                    var _SHARAYET = $"{(SHARAYET.Text ?? string.Empty)} ش.پ {NUMBER.Text}";
                                    var _JAY = (bool?)JAY.IsChecked;
                                    var _TAMIR = 0;
                                    var _MAS_NUMBER = string.IsNullOrEmpty(MAS.Text) ? 0d : Convert.ToDouble(MAS.Text);
                                    var _MAS = string.IsNullOrEmpty(_MasMaghsad_.ToStringNullSafe()) ? _MAS_NUMBER : Convert.ToInt32(_MasMaghsad_);
                                    var _MODAT_PPID = (int?)MODAT_PPID.SelectedValue;
                                    var _PEID = (int?)PEID.SelectedValue;
                                    var _PEPID = (int?)PEPID.SelectedValue;

                                    db.Execute($@"INSERT INTO dbo.HEAD_LST(NUMBER,   TAG,                   OKF,                                        CUST_NO,   DATE_N,      USER_NAME,                                              DEPATMAN,                                               CUST_KIND,   SHIFT,      MOLAH,                              TICMBAA,                                              MABL_HAZ,                                             TAKHFIF,                                          MBAA,                                      HMBAA,   VAS,   OKDATE,   OKTIME,      SHARAYET,                              JAY,   TAMIR,                                                 MODAT_PPID,                                          PEID,                                           PEPID ,  MAS)
                                                            VALUES ({_NUMBER},{_TAG},{Convert.ToByte(_OKF)},{(_CUST_NO != null ? $"'{_CUST_NO}'" : "NULL")},{_DATE_N},N'{_USER_NAME}',{(_DEPATMAN.HasValue ? _DEPATMAN.ToString() : "NULL")},{(_CUST_KIND.HasValue ? _CUST_KIND.ToString() : "NULL")},{_SHIFT},N'{_MOLAH}',{(Convert.ToByte(_TICMBAA ?? false))},{(_MABL_HAZ.HasValue ? _MABL_HAZ.ToString() : "NULL")},{(_TAKHFIF.HasValue ? _TAKHFIF.ToString() : "NULL")},{(_MBAA.HasValue ? _MBAA.ToString() : "NULL")},{(_HMBAA != null ? $"'{_HMBAA}'" : "NULL")},{_VAS},{_OKDATE},{_OKTIME},N'{_SHARAYET}',{(Convert.ToByte(_JAY ?? false))},{_TAMIR}, {(_MODAT_PPID.HasValue ? _MODAT_PPID.ToString() : "NULL")},{(_PEID.HasValue ? _PEID.ToString() : "NULL")},{(_PEPID.HasValue ? _PEPID.ToString() : "NULL")} , {_MAS})", null, transaction);


                                    //______//
                                    OKF.IsChecked = true;
                                    TAMIR.SelectedIndex = 2;
                                    db.Execute("UPDATE  dbo.HEAD_LST SET   TAMIR = 2,OKF = 1 WHERE TAG = 20 AND NUMBER = " + NUMBER.Text, null, transaction);
                                    db.Execute("INSERT INTO dbo.INVO_LST (NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ,TKHN) SELECT     " + num + " aS NUMBER, 2 AS tag, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO,  CUST_NO , ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ,TKHN FROM   dbo.INVO_LST  WHERE     (NUMBER = " + NUMBER.Text + " ) AND (TAG = 20) and (jay = 0)", null, transaction);
                                    db.Execute("INSERT INTO dbo.OTHER_DTL (NUMBER, TAG, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN) SELECT     " + num + " AS Expr1, 2 AS Expr2, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN FROM dbo.OTHER_DTL WHERE     (TAG = 20) AND (NUMBER = " + NUMBER.Text + ")", null, transaction);
                                    db.Execute("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, CAM_KHALY, CAM_POOR, MEGHk, TOZIH, RADIF, VAZNH) SELECT     " + num + " AS Expr1, 2 AS Expr2, CODE, CAM_KHALY, CAM_POOR, MEGHk, TOZIH, RADIF, VAZNH FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = 20) AND (NUMBER = " + NUMBER.Text + ")", null, transaction);
                                    // جايزه
                                    //var Jrst = new ADODB.Recordset();
                                    var MEGHTAA = default(long);
                                    var MEGHJAYY = default(long);
                                    var VAHEDD = default(long);
                                    if ((bool)JAY.IsChecked)
                                    {
                                        //Fake Query for Lock Table
                                        db.Execute("UPDATE TOP(1) dbo.INVO_LST SET MANDAH = MANDAH", null, transaction);
                                        //Fake Query for Lock Table
                                        var Jrst = db.Query<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + num, null, transaction).ToList();
                                        foreach (var JrstFields in Jrst)
                                        {
                                            var rst_two = db.Query<STUF_DEF>("select * from STUF_DEF where CODE = '" + JrstFields.CODE + "'", null, transaction).FirstOrDefault();
                                            if (ReferenceEquals(rst_two, null))
                                            {
                                            }
                                            else
                                            {
                                                MEGHJAYY = (long)rst_two.MEGHJAY;
                                                MEGHTAA = (long)rst_two.MEGHTA;
                                                VAHEDD = rst_two.VAHED;
                                            }
                                            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5" && string.IsNullOrEmpty(JrstFields.JAYO.ToString()) && JrstFields.JAY == 0)
                                            {
                                                var rst_three = db.Query<invo_edam>("select * from invo_edam where idd = " + JrstFields.id, null, transaction).FirstOrDefault();
                                                string where_invo_edam_idd = $" where idd = " + JrstFields.id;
                                                if (!ReferenceEquals(rst_three, null))
                                                {
                                                    if (rst_three.MEGHTA != MEGHTAA || rst_three.MEGHJAY != MEGHJAYY && MEGHTAA + MEGHJAYY > 0L)
                                                    {
                                                        Msgwin msgwin1 = new Msgwin(true, "مقادير جايزه نسبت به قبل تغيير كرده است آيا مقادير جديد را جايگزين كنم؟"); msgwin1.ShowDialog();
                                                        if (msgwin1.DialogResult == true)
                                                        {
                                                            db.Execute($"UPDATE invo_edam SET MEGHTA = {MEGHTAA}, MEGHJAY = {MEGHJAYY}, VAHED = {VAHEDD} {where_invo_edam_idd}", null, transaction);
                                                        }
                                                    }
                                                }
                                                else if (MEGHTAA + MEGHJAYY > 0L)
                                                {
                                                    db.Execute($@"INSERT INTO dbo.invo_edam (idd,    MEGHTA,    MEGHJAY,    VAHED,    NUMBER,    TAGH)
                                                                              VALUES
                                                                              (   {JrstFields.id},
                                                                                  {MEGHTAA},
                                                                                  {MEGHJAYY},
                                                                                  {VAHEDD},
                                                                                  {JrstFields.NUMBER},
                                                                                  2)", null, transaction);
                                                }
                                            }
                                        }
                                        var rst_four = db.Query<tabdilhav_1>("SELECT dbo.INVO_LST.VAHED_K, dbo.invo_edam.idd, dbo.INVO_LST.CODE, dbo.invo_edam.VAHED, dbo.invo_edam.MEGHTA, dbo.invo_edam.MEGHJAY, dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.JAY , dbo.INVO_LST.JAYO, dbo.INVO_LST.id FROM dbo.INVO_LST INNER JOIN dbo.invo_edam ON dbo.INVO_LST.id = dbo.invo_edam.idd WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.JAY = 0) AND dbo.INVO_LST.NUMBER = " + num, null, transaction).ToList();
                                        foreach (var rst_fourFields in rst_four)
                                        {
                                            if (ReferenceEquals(rst_fourFields.JAYO, null))
                                            {
                                                if (rst_fourFields.MEGHTA > 0 && rst_fourFields.MEGHJAY > 0)
                                                {
                                                    if (rst_fourFields.MEGHk / rst_fourFields.MEGHTA >= 1)
                                                    {
                                                        db.Execute($@"INSERT INTO dbo.INVO_LST (NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, SANAD_NO, VAHED_K, JAY)
                                                                      SELECT
                                                                          {rst_fourFields.NUMBER},
                                                                          {rst_fourFields.TAG},
                                                                          {rst_fourFields.ANBAR},
                                                                          (SELECT ISNULL(MAX(RADIF), 0) + 1 FROM dbo.INVO_LST WHERE NUMBER = {rst_fourFields.NUMBER} AND TAG = {rst_fourFields.TAG}),
                                                                          N'{rst_fourFields.CODE}',
                                                                          {Math.Truncate(Convert.ToDouble(rst_fourFields.MEGHk / rst_fourFields.MEGHTA * rst_fourFields.MEGHJAY / CL_HESABDARI.GETVAHEDN(rst_fourFields.CODE, Convert.ToInt32(rst_fourFields.VAHED))))},
                                                                          {Math.Truncate((double)(Convert.ToDouble(rst_fourFields.MEGHk / rst_fourFields.MEGHTA) * rst_fourFields.MEGHJAY))},
                                                                          0,
                                                                          {rst_fourFields.VAHED},
                                                                          {rst_fourFields.id}
                                                                    ", null, transaction);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    var rst_five = db.Query<tabdilhav_2>("SELECT      CUST_NO, DARSAD, PURSANT, TOZIH,PORID FROM dbo.VISITOR_DTL WHERE (NUMBER = " + NUMBER.Text + ") And (TAG = 20)", null, transaction).FirstOrDefault();
                                    if (!ReferenceEquals(rst_five, null))
                                    {
                                        if (string.IsNullOrEmpty(rst_five.CUST_NO.ToString()))
                                        {
                                            Msgwin msgwin2 = new Msgwin(false, "مشخصات ويزيتور صحيح نيست واشكال دارد پورسانت ويزيتور در فاكتور را بررسي و اصلاح كنيد"); msgwin2.ShowDialog();
                                        }
                                    }
                                    db.Execute("INSERT INTO dbo.VISITOR_DTL (NUMBER, TAG, CUST_NO, DARSAD, PURSANT, TOZIH,PORID) SELECT     " + num + ", 2, CUST_NO, DARSAD, PURSANT, TOZIH,PORID FROM dbo.VISITOR_DTL WHERE (NUMBER = " + NUMBER.Text + ") And (TAG = 20)", null, transaction);
                                    Msgwin msgwin3 = new Msgwin(false, "پيش فاكتور تبديل به حواله  شماره :" + num + "  گرديد"); msgwin3.ShowDialog();
                                    TAMIR.SelectedIndex = 2;
                                    db.Execute("UPDATE  dbo.HEAD_LST SET   TAMIR = 2,OKF = 1 WHERE TAG = 20 AND NUMBER = " + NUMBER.Text, null, transaction);
                                    INVO_LST_SUB.IsReadOnly = true;
                                    this.TAMIR.IsEnabled = false;
                                    transaction.Commit();
                                    db?.Close();
                                }
                            }
                        };
                        dbms.DoExecuteSQL($@"INSERT INTO	dbo.head_lst_log (UP_DATE,NUMBER,TAGG,RESERVED,UP_USER_NAME,fieldname,UDATEF)
                                                       VALUES
                                                       (   GETDATE(),
                                                           {NUMBER.Text},
                                                           {20},
                                                           {num},
                                                           N'{CL_HESABDARI.UCurrentUser()}',
                                                           N'TABDILHAVLA',
                                                           {Tarikh.FullCurrentDate}
                                                           )");
                    }
                }
                dbms.DoExecuteSQL("UPDATE    dbo.sazman SET   pishpross = 0");
                if (Convert.ToBoolean(Baseknow.SIGN))
                {
                    string SHARH;
                    double td;
                    td = Convert.ToDouble(DateTime.Now.ToOADate());

                    SHARH = "حواله شماره: " + num + " مورخ " + Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + " از پيش فاکتور : " + NUMBER.Text;
                    dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "','" + SHARH + "','" + CUST_NO.SelectedValue + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",2," + num + ",2," + td + "," + Baseknow.USERCOD + " )");
                }

                OKF.IsChecked = true;
            }
            catch (Exception)
            {
                ProcLoader.Stop(Prc);
                new Msgwin(false, "خطا در انجام عملیات").Show(); return;
            }

            BTN_SAVE_Click(null, null);

            ProcLoader.Stop(Prc);

            if (num > 0)
            {
                new HEAD_LST_HAVL(Convert.ToDouble(num)).ShowDialog();
            }

            this.Close();
        }

        private void Command113_Click(object sender, RoutedEventArgs e)
        {
            if ((OKF.IsChecked ?? false) || !Command113.IsEnabled)
            {
                return;
            }

            if (SHARAYET.Text.Length > 7999)
            {
                new Msgwin(false, "شرایط پیش فاکتور بیش از 8000 کاراکتر است , که این مجاز نیست , آنرا اصلاح کنید").ShowDialog();
                return;
            }

            if (NewRecord || INVO_LST_PISH2_DATA.Count == 0)
            {
                new Msgwin(false, "پیش فاکتور با سطر های خالی را نمیتوان تبدیل کرد !").ShowDialog();
                return;
            }
            else
            {
                BTN_SAVE_Click(null, null);
            }

            byte _TAMIR_ = 250;
            if (TAMIR.SelectedValue != null)
            {
                _TAMIR_ = Convert.ToByte(((FrameworkElement)TAMIR.SelectedValue).Tag);
            }

            long PURSANT = 0;
            double DARSAD = 0;
            var num = default(long);
            long MBK = 0;
            long CMABL = 0;
            double nesba;
            double takh = 0;
            string rptname;
            var min = default(double);
            var NOTPR = default(bool);
            double JAMFACT = 0;

            CL_LMethods.DoWriteMyLog($"Username : {Baseknow.UUSER} , Number : {NUMBER.Text} , DateTime : {DateTime.Now} , Goal : تبدیل به حواله  , Form : پیش فاکتور", default);

            var RST20 = dbms.DoGetDataSQL<bool?>("SELECT OKF FROM HEAD_LST where NUMBER  = " + this.NUMBER.Text + " AND TAG = 20").FirstOrDefault();
            if (RST20 ?? false)
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 45, 1) == "5")
                {
                    Msgwin msgwin = new Msgwin(true, "اين پيش فاكتور قبلا تبديل به فاكتور شده است آيا مايليد مجددا فاكتور صادر شود؟");
                    msgwin.ShowDialog();

                    if (msgwin.DialogResult == false)
                    {
                        return;
                    }
                }
                else
                {
                    new Msgwin(false, "اين پيش فاكتور قبلا به فاكتور تبديل شده است و اجازه تبديل مجدد نداريد").ShowDialog();
                    return;
                }
            }

            if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToStringNullSafe()))
            {
                new Msgwin(false, "حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد").ShowDialog();
                return;
            }
            else if (IsNull(this.CUST_KIND.SelectedValue) || CUST_KIND.SelectedValue.ToStringNullSafe() == "0")
            {
                new Msgwin(false, "نوع مشتري نميتواند خالي باشد").ShowDialog();
                return;
            }

            JAYEZAH();
            var RST202 = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE NUMBER  = " + NUMBER.Text + " AND TAG = 20").ToList();

            foreach (var Fields in RST202) //while (!RST202.EOF)
            {
                nesba = CL_HESABDARI.GETNESBAT(Fields.CODE, Convert.ToInt32(Fields.VAHED_K));
                Fields.MEGHk = Fields.MEGH * nesba;
                Fields.MABL_K = Math.Round(Fields.MEGH * nesba * Fields.MABL);
                MBK = (long)Math.Round(Fields.MEGH * nesba * Fields.MABL);
                Fields.N_MOIN = Math.Round((double)(Fields.N_KOL * MBK / 100)) + Math.Round((double)((MBK - Math.Round((double)(Fields.N_KOL * MBK / 100))) * Fields.TKHN / 100));
                takh = (double)(takh + Fields.N_MOIN);

                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET MEGHk = {Fields.MEGHk}, MABL_K = {Fields.MABL_K}, N_MOIN = {Fields.N_MOIN} WHERE id = {Fields.id}"); //RST202.update();
                //Set rst4 = New ADODB.Recordset

                bool HichGHEYM = Baseknow.GHAYM.ToString() == "3"; //پیش فرض قیمت هیچکدام
                //در زمان تبديل پيش فاكتور به فاكتور مبلغ فروش كنترل گردد كه اگر تغيير كرده اخطار دهد بجز مبالغ صفر
                if (Strings.Mid(Baseknow.OPTIONSS, 51, 1) == "5" && !CL_Generaly.IsGHAYM_7 && !HichGHEYM)
                {
                    //Set rst4 = New ADODB.Recordset
                    var rst4 = dbms.DoGetDataSQL<PISHQ1>("SELECT MABL_F , B_SEF FROM STUF_DEF WHERE CODE = '" + Fields.CODE + "'").ToList();
                    if (rst4.Count == 1)
                    {
                        if (Baseknow.GHAYM == 2)
                        {
                            CMABL = (long)rst4.FirstOrDefault().MABL_F;
                        }
                        else if (Baseknow.GHAYM == 5)
                        {
                            CMABL = (long)rst4.FirstOrDefault().B_SEF;
                        }
                    }
                    if (CMABL != Fields.MABL && Fields.MABL != 0)
                    {
                        new Msgwin(false, " قيمت كالاي " + Fields.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(Fields.CODE)) + " با قيمت سيستم منطبق نيست").ShowDialog();
                    }
                    //Set rst4 = New ADODB.Recordset
                    var rst44 = dbms.DoGetDataSQL<PISHQ2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + Fields.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").FirstOrDefault();
                    if (rst44 != null)
                    {
                        if (rst44.MABL > Fields.MABL)
                        {
                            new Msgwin(false, "قيمت فروش از قيمت خريد كمتر مي باشد. " + "کد کالا : " + Fields.CODE + " نام کالا : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(Fields.CODE))).ShowDialog();
                        }
                    }
                }
                //RST202.MoveNext();
            }
            TAKHFIF.Text = takh.ToStringNullSafe();

            //Set RST2 = New ADODB.Recordset
            if (Strings.Mid(Baseknow.OPTIONSS, 59, 1) == "5")
            {
                var RST2 = dbms.DoGetDataSQL<PISHQ3>("SELECT ANBAR, CODE, SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE NUMBER = " + this.NUMBER.Text + " and tag = 20 GROUP BY ANBAR, CODE").ToList();
                foreach (var Fields in RST2) //while (!RST2.EOF)
                {
                    var rst3 = dbms.DoGetDataSQL<double?>("SELECT MIN_M FROM STUF_DEF WHERE CODE = '" + Fields.CODE + "'").FirstOrDefault();
                    if (rst3 != null)
                    {
                        if (IsNull(rst3)) //MIN_M
                        {
                            if (_TAMIR_ == 1 || _TAMIR_ == 4)
                            {
                                min = CL_HESABDARI.Getmin((int)Fields.ANBAR, Fields.CODE);
                            }
                            else
                            {
                                min = (double)Fields.MEGHk;
                            }
                        }
                        else if (_TAMIR_ == 1 || _TAMIR_ == 4)
                        {
                            min = CL_HESABDARI.Getmin((int)Fields.ANBAR, Fields.CODE);
                        }
                        else
                        {
                            min = (double)(CL_HESABDARI.Getmin((int)Fields.ANBAR, Fields.CODE) + Fields.MEGHk);
                        }
                    }
                    var rst = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + Fields.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + Fields.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + Fields.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + Fields.ANBAR + ")").FirstOrDefault();
                    if (rst != null)
                    {
                        if (Math.Round((double)rst, (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && Fields.ANBAR != 0)
                        {
                            new Msgwin(false, " خروج كالاي  " + Fields.CODE + " : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(Fields.CODE)) + " از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.برگه قابل تبديل نيست" + " حداقل موجودي لازم  :" + (min - rst)).ShowDialog();
                            NOTPR = true;
                        }
                    }
                    //RST2.MoveNext();
                }
            }
            if (NOTPR)
            {
                new Msgwin(false, " به دليل كسري كالاهاي مذكور پيش فاكتور قابل تبديل نمي باشد كالاهاي مذكور را حذف يا به حد مجاز كاهش دهيد و مجددا سعي كنيد").ShowDialog();
                return;
            }

            if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToStringNullSafe())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(CUST_NO.SelectedValue.ToStringNullSafe())))
                {
                    new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!").ShowDialog();
                    return;
                }
            }

            //Let's Convert Profoma Invoice to Invoice
            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                if (IsNull(OKF.IsChecked))
                {
                    OKF.IsChecked = false;
                }
                if (Convert.ToDouble(TAKHFIF.Text) != takh)
                {
                    TAKHFIF.Text = takh.ToStringNullSafe();
                }
                Msgwin msgwin = new Msgwin(true, "آيا مطمئن هستيد ! پيش فاكتور به فاكتور تبديل شود؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    long NUM1;

                    num = CL_HESABDARI.CreateHEAD_LST(2, CL_HESABDARI.FARSIDATE(), CUST_NO.SelectedValue.ToString());
                    NUM1 = CL_HESABDARI.CreateHEAD_LST(13, CL_HESABDARI.FARSIDATE(), CUST_NO.SelectedValue.ToString(), num);

                    CDDATE = CL_HESABDARI.FARSIDATE().ToString();
                    //Set rst = New ADODB.Recordset
                    {
                        var rst = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE TAG = 2 AND NUMBER = " + num).FirstOrDefault();
                        if (rst != null)
                        {
                            rst.NUMBER = num;
                            rst.TAG = 2;
                            rst.OKF = true;
                            rst.CUST_NO = Convert.ToString(CUST_NO.SelectedValue);
                            rst.DATE_N = CL_HESABDARI.FARSIDATE();
                            rst.USER_NAME = USER_NAME.Text;
                            rst.DEPATMAN = Convert.ToInt32(DEPATMAN.SelectedValue);
                            rst.CUST_KIND = Convert.ToInt32(CUST_KIND.SelectedValue);
                            rst.SHIFT = CL_Generaly.SHIFT_OF_USER;
                            rst.MOLAH = MOLAH.Text;
                            rst.TICMBAA = TICMBAA.IsChecked;
                            rst.MABL_HAZ = Convert.ToDouble(MABL_HAZ.Text);
                            rst.TAKHFIF = Convert.ToDouble(TAKHFIF.Text);
                            rst.MBAA = Convert.ToDouble(MBAA.Text); //مالیات
                            rst.HMBAA = (string)Interaction.IIf(Convert.ToDouble(MBAA.Text) > 0, Baseknow.HESMBAA, "NULL");
                            rst.VAS = 1;
                            rst.OKDATE = CL_HESABDARI.FARSIDATE();

                            rst.SHARAYET = string.IsNullOrEmpty(SHARAYET.Text) ? "" : SHARAYET.Text + " ش.پ " + NUMBER.Text;
                            rst.JAY = JAY.IsChecked;
                            rst.MODAT_PPID = ((int?)(MODAT_PPID.SelectedValue is null ? null : MODAT_PPID.SelectedValue));
                            rst.PEID = ((int?)(PEID.SelectedValue is null ? null : PEID.SelectedValue));
                            rst.PEPID = ((int?)(PEPID.SelectedValue is null ? null : PEPID.SelectedValue));
                            rst.MAS = Convert.ToDouble(MAS.Text);


                            var updateQuery = $@"UPDATE HEAD_LST SET 
                                OKF = 1,
                                CUST_NO = '{CUST_NO.SelectedValue}',
                                DATE_N = {CL_HESABDARI.FARSIDATE()},
                                USER_NAME = '{USER_NAME.Text}',
                                DEPATMAN = {Convert.ToInt32(DEPATMAN.SelectedValue)},
                                CUST_KIND = {Convert.ToInt32(CUST_KIND.SelectedValue)},
                                SHIFT = {CL_Generaly.SHIFT_OF_USER},
                                MOLAH = '{MOLAH.Text}',
                                TICMBAA = {((bool)TICMBAA.IsChecked ? "1" : "0")},
                                MABL_HAZ = {Convert.ToDouble(MABL_HAZ.Text)},
                                TAKHFIF = {Convert.ToDouble(TAKHFIF.Text)},
                                MBAA = {Convert.ToDouble(MBAA.Text)},
                                HMBAA = {(Convert.ToDouble(MBAA.Text) > 0 ? $"'{Baseknow.HESMBAA}'" : "NULL")},
                                VAS = 1,
                                OKDATE = {CL_HESABDARI.FARSIDATE()},
                                SHARAYET = '{(string.IsNullOrEmpty(SHARAYET.Text) ? "" : SHARAYET.Text + " ش.پ " + NUMBER.Text)}',
                                JAY = {((bool)JAY.IsChecked ? "1" : "0")},
                                MODAT_PPID = {(MODAT_PPID.SelectedValue == null ? "NULL" : MODAT_PPID.SelectedValue)},
                                PEID = {(PEID.SelectedValue == null ? "NULL" : PEID.SelectedValue)},
                                PEPID = {(PEPID.SelectedValue == null ? "NULL" : PEPID.SelectedValue)},
                                MAS = {Convert.ToDouble(MAS.Text)}  
                               WHERE TAG = 2 AND NUMBER = {num}";

                            dbms.DoExecuteSQL(updateQuery); //rst.update();
                        }
                        else
                        {
                            new Msgwin(false, "ايجاد فاکتور با خطا مواجه شده است از سالم بودن و بدون نويز بودن شبکه مطمءن شويد").ShowDialog();
                            return;
                        }
                    }
                    //Set rst = New ADODB.Recordset
                    {
                        var rst = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE TAG = 13 AND NUMBER = " + num).FirstOrDefault();
                        if (rst != null)
                        {
                            rst.TAG = 13;
                            rst.OKF = false;
                            rst.CUST_NO = Convert.ToString(CUST_NO.SelectedValue);
                            rst.DATE_N = CL_HESABDARI.FARSIDATE();
                            rst.USER_NAME = USER_NAME.Text;
                            rst.DEPATMAN = Convert.ToInt32(DEPATMAN.SelectedValue);
                            rst.CUST_KIND = Convert.ToInt32(CUST_KIND.SelectedValue);
                            rst.SHIFT = CL_Generaly.SHIFT_OF_USER;
                            rst.MOLAH = MOLAH.Text + "ش.پ " + NUMBER.Text;
                            rst.TICMBAA = TICMBAA.IsChecked;
                            rst.MABL_HAZ = Convert.ToDouble(MABL_HAZ.Text);
                            rst.TAKHFIF = Convert.ToDouble(TAKHFIF.Text);
                            rst.MBAA = Convert.ToDouble(MBAA.Text);
                            rst.HMBAA = (string)Interaction.IIf(Convert.ToDouble(MBAA.Text) > 0, Baseknow.HESMBAA, "NULL");
                            rst.VAS = 1;
                            rst.OKDATE = CL_HESABDARI.FARSIDATE();
                            rst.SHARAYET = SHARAYET.Text;
                            rst.JAY = JAY.IsChecked;
                            rst.MODAT_PPID = ((int?)(MODAT_PPID.SelectedValue is null ? null : MODAT_PPID.SelectedValue));
                            rst.PEID = ((int?)(PEID.SelectedValue is null ? null : PEID.SelectedValue));
                            rst.PEPID = ((int?)(PEPID.SelectedValue is null ? null : PEPID.SelectedValue));
                            rst.MAS = Convert.ToDouble(MAS.Text);



                            var updateQuery = $@"UPDATE HEAD_LST SET 
                                                 TAG = 13,
                                                 OKF = 0,
                                                 CUST_NO = '{CUST_NO.SelectedValue}',
                                                 DATE_N = {CL_HESABDARI.FARSIDATE()},
                                                 USER_NAME = '{USER_NAME.Text}',
                                                 DEPATMAN = {Convert.ToInt32(DEPATMAN.SelectedValue)},
                                                 CUST_KIND = {Convert.ToInt32(CUST_KIND.SelectedValue)},
                                                 SHIFT = {CL_Generaly.SHIFT_OF_USER},
                                                 MOLAH = '{MOLAH.Text}ش.پ {NUMBER.Text}',
                                                 TICMBAA = {((bool)TICMBAA.IsChecked ? "1" : "0")},
                                                 MABL_HAZ = {Convert.ToDouble(MABL_HAZ.Text)},
                                                 TAKHFIF = {Convert.ToDouble(TAKHFIF.Text)},
                                                 MBAA = {Convert.ToDouble(MBAA.Text)},
                                                 HMBAA = {(Convert.ToDouble(MBAA.Text) > 0 ? $"'{Baseknow.HESMBAA}'" : "NULL")},
                                                 VAS = 1,
                                                 OKDATE = {CL_HESABDARI.FARSIDATE()},
                                                 SHARAYET = '{SHARAYET.Text}',
                                                 JAY = {((bool)JAY.IsChecked ? "1" : "0")},
                                                 MODAT_PPID = {(MODAT_PPID.SelectedValue == null ? "NULL" : MODAT_PPID.SelectedValue)},
                                                 PEID = {(PEID.SelectedValue == null ? "NULL" : PEID.SelectedValue)},
                                                 PEPID = {(PEPID.SelectedValue == null ? "NULL" : PEPID.SelectedValue)},
                                                 MAS = {Convert.ToDouble(MAS.Text)}
                                             WHERE TAG = 13 AND NUMBER = {num}";

                            dbms.DoExecuteSQL(updateQuery); //rst.update();
                        }
                        else
                        {
                            new Msgwin(false, "ايجاد فاکتور با خطا مواجه شده است از سالم بودن و بدون نويز بودن شبکه مطمءن شويد ").ShowDialog();
                            return;
                        }
                    }
                    // ***************************************************************
                    // CurrentProject.Connection.Execute ("COMMIT TRANSACTION"): DoCmd.Close acForm, "bun"
                    // ***************************************************************
                    this.OKF.IsChecked = true;
                    TAMIR.SelectedValue = 2; TAMIR.Items.Refresh();

                    dbms.DoExecuteSQL("UPDATE  dbo.HEAD_LST SET   TAMIR = 2,OKF = 1 WHERE TAG = 20 AND NUMBER = " + NUMBER.Text);

                    dbms.DoExecuteSQL("INSERT INTO dbo.INVO_LST (NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ,TKHN) SELECT     " + num + " aS NUMBER, 2 AS tag, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO,  CUST_NO , ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ,TKHN FROM   dbo.INVO_LST  WHERE     (NUMBER = " + this.NUMBER.Text + " ) AND (TAG = 20) and (jay = 0)");

                    dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL (NUMBER, TAG, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN) SELECT     " + num + " AS Expr1, 2 AS Expr2, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN FROM dbo.OTHER_DTL WHERE     (TAG = 20) AND (NUMBER = " + this.NUMBER.Text + ")");

                    dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, CAM_KHALY, CAM_POOR, MEGHk, TOZIH, RADIF, VAZNH) SELECT     " + num + " AS Expr1, 2 AS Expr2, CODE, CAM_KHALY, CAM_POOR, MEGHk, TOZIH, RADIF, VAZNH FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = 20) AND (NUMBER = " + this.NUMBER.Text + ")");
                    // جايزه
                    //var Jrst = new ADODB.Recordset();
                    var MEGHTAA = default(long);
                    var MEGHJAYY = default(long);
                    var VAHEDD = default(long);
                    if ((bool)JAY.IsChecked)
                    {
                        var Jrst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + num).ToList();
                        foreach (var Fields in Jrst) //while (!Jrst.EOF)
                        {
                            //Set rst = New ADODB.Recordset
                            var rst1 = dbms.DoGetDataSQL<STUF_DEF>("SELECT * FROM STUF_DEF WHERE CODE = '" + Fields.CODE + "'").FirstOrDefault();
                            if (rst1 == null)
                            {
                            }
                            else
                            {
                                MEGHJAYY = (long)rst1.MEGHJAY;
                                MEGHTAA = (long)rst1.MEGHTA;
                                VAHEDD = rst1.VAHED;
                            }
                            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5" && IsNull(Fields.JAYO) && Fields.JAY == 0)
                            {
                                //Set rst = New ADODB.Recordset
                                var rst = dbms.DoGetDataSQL<INVO_EDAM_CSHARP>("SELECT * FROM INVO_EDAM WHERE idd = " + Fields.id).FirstOrDefault();
                                if (rst != null)
                                {
                                    if (rst.MEGHTA != MEGHTAA || rst.MEGHJAY != MEGHJAYY & MEGHTAA + MEGHJAYY > 0L)
                                    {
                                        Msgwin msgwin1 = new Msgwin(true, "مقادير جايزه نسبت به قبل تغيير كرده است آيا مقادير جديد را جايگزين كنم؟");
                                        msgwin1.ShowDialog();
                                        if (msgwin1.DialogResult == true)
                                        {
                                            rst.MEGHTA = MEGHTAA;
                                            rst.MEGHJAY = MEGHJAYY;
                                            rst.VAHED = (int?)VAHEDD;

                                            var updateQuery = $@"UPDATE INVO_EDAM SET 
                                                                 MEGHTA = {MEGHTAA},
                                                                 MEGHJAY = {MEGHJAYY},
                                                                 VAHED = {((int?)VAHEDD == null ? "NULL" : VAHEDD)}
                                                                WHERE idd = {Fields.id}";
                                            dbms.DoExecuteSQL(updateQuery); //rst.update();
                                        }
                                    }
                                }
                                else if (MEGHTAA + MEGHJAYY > 0L)
                                {
                                    //rst.AddNew();
                                    //rst.Fields("idd") = Fields.id;
                                    //rst.Fields("MEGHTA") = MEGHTAA;
                                    //rst.Fields("MEGHJAY") = MEGHJAYY;
                                    //rst.Fields("VAHED") = VAHEDD;
                                    //rst.Fields("NUMBER") = Fields.NUMBER;
                                    //rst.Fields("TAGH") = 2;

                                    var insertQuery = $@"INSERT INTO invo_edam 
                                                             (idd, MEGHTA, MEGHJAY, VAHED, NUMBER, TAGH)
                                                         VALUES 
                                                             ({Fields.id}, 
                                                              {MEGHTAA}, 
                                                              {MEGHJAYY}, 
                                                              {VAHEDD}, 
                                                              {Fields.NUMBER}, 
                                                              2)";

                                    dbms.DoExecuteSQL(insertQuery); //rst.update();
                                }
                            }
                            //Jrst.MoveNext();
                        }
                        //Set rst = New ADODB.Recordset
                        {
                            var query = $@"SELECT 
                                          L.VAHED_K, E.idd, L.CODE, E.VAHED, E.MEGHTA, E.MEGHJAY, 
                                          L.NUMBER, L.TAG, L.ANBAR, L.RADIF, L.MEGH, L.MEGHk, 
                                          L.JAY, L.JAYO, L.id 
                                        FROM dbo.INVO_LST AS L
                                        INNER JOIN dbo.invo_edam AS E ON L.id = E.idd 
                                        WHERE (L.TAG = 2) AND (L.JAY = 0) AND (L.NUMBER = @num)";
                            var parameters = new { num = num };
                            var rst = dbms.DoGetDataSQL<JAYMD>(query, parameters).ToList();

                            foreach (var fields in rst)
                            {
                                if (fields.JAYO == null)
                                {
                                    if (fields.MEGHTA > 0 && fields.MEGHJAY > 0)
                                    {
                                        if (fields.MEGHk / fields.MEGHTA >= 1)
                                        {
                                            var insertQuery = $@"
                                             INSERT INTO dbo.INVO_LST (NUMBER, TAG, ANBAR, JAY, CODE, SANAD_NO, RADIF, VAHED_K, MEGH, MEGHk)
                                             SELECT
                                                 {fields.NUMBER},
                                                 {fields.TAG},
                                                 {fields.ANBAR},
                                                 {fields.id},
                                                 N'{fields.CODE}',
                                                 0,
                                                 (SELECT ISNULL(MAX(RADIF), 0) + 1 FROM dbo.INVO_LST WHERE NUMBER = {fields.NUMBER} AND TAG = {fields.TAG}),
                                                 {fields.VAHED},
                                                 {Math.Truncate((double)((double)(fields.MEGHk / fields.MEGHTA) * fields.MEGHJAY / CL_HESABDARI.GETVAHEDN(fields.CODE, (int)fields.VAHED)))},
                                                 {Math.Truncate((double)((double)(fields.MEGHk / fields.MEGHTA) * fields.MEGHJAY))}";

                                            dbms.DoExecuteSQL(insertQuery);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Set rst = New ADODB.Recordset
                    {
                        var rst = dbms.DoGetDataSQL<VISITOR_DTL>("SELECT CUST_NO, DARSAD, PURSANT, TOZIH,PORID FROM dbo.VISITOR_DTL WHERE (NUMBER = " + NUMBER.Text + ") And (TAG = 20)").FirstOrDefault();
                        if (rst != null)
                        {
                            if (IsNull(rst.CUST_NO) || rst.CUST_NO == "")
                            {
                                new Msgwin(false, "مشخصات ويزيتور صحيح نيست واشكال دارد پورسانت ويزيتور در فاكتور را بررسي و اصلاح كنيد").ShowDialog();
                            }
                        }
                    }

                    OKF.IsChecked = true;

                    dbms.DoExecuteSQL("INSERT INTO dbo.VISITOR_DTL (NUMBER, TAG, CUST_NO, DARSAD, PURSANT, TOZIH,PORID) SELECT     " + num + ", 2, CUST_NO, DARSAD, PURSANT, TOZIH,PORID FROM dbo.VISITOR_DTL WHERE (NUMBER = " + NUMBER.Text + ") And (TAG = 20)");
                    AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(Convert.ToInt64(num), Convert.ToInt64(num), false);
                    new Msgwin(false, "پيش فاكتور تبديل به فاكتور شماره :" + num + "  گرديد").ShowDialog();

                    var _ftrnums_ = dbms.DoGetDataSQL<dynamic>($"SELECT NUMBER1,NUMBER FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = {num}").FirstOrDefault();
                    string _FTRNUMS_ = _ftrnums_.NUMBER1 + "," + _ftrnums_.NUMBER;

                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this, _FTRNUMS_, default, default, default, true);


                    //DoCmd.OpenForm("HEAD_LST_FROOSH22", default, default, default, default, default, "NUMBER = " + num);
                }
                //Set rst = New ADODB.Recordset
                //rst.Open("HEAD_LST_LOG");
                //rst.AddNew();
                //rst.Fields("UP_DATE") = DateTime.Now;
                //rst.Fields("NUMBER") = this.NUMBER.Text;
                //rst.Fields("TAGG") = 20;
                //rst.Fields("RESERVED") = num;
                //rst.Fields("UP_USER_NAME") = CL_HESABDARI.UCurrentUser();
                //rst.Fields("FIELDNAME") = "TABDILFACTOR";
                //rst.Fields("UDATEF") = CL_HESABDARI.FARSIDATE();

                dbms.DoExecuteSQL(@"INSERT INTO HEAD_LST_LOG (UP_DATE, NUMBER, TAGG, RESERVED, UP_USER_NAME, FIELDNAME, UDATEF)
                            VALUES (@UP_DATE, @NUMBER, 20,@RESERVED, @UP_USER_NAME, 'TABDILFACTOR', @UDATEF)",
                  new
                  {
                      UP_DATE = DateTime.Now,
                      NUMBER = NUMBER.Text,
                      RESERVED = num,
                      UP_USER_NAME = CL_HESABDARI.UCurrentUser(),
                      UDATEF = CL_HESABDARI.FARSIDATE()
                  }); //rst.update();


                this.Close();
            }
        }


        private void Command118_Click(object sender, RoutedEventArgs e)
        {

        }
        private void MANDAH_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisplayMandah();
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN1.IsChecked ?? false);
                SGN1.IsChecked = !SGN_WAS;
                return;
            }

            double mid;
            string SHARH;
            double td;
            mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
            if (mid > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + NUMBER.Text + ",20 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
            }
            else
            {
                td = Tarikh.GET_OADATE_DAO();
                SHARH = "'پيش فاکتور  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt32(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20, GETDATE() ," + Baseknow.USERCOD + " )");
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20 )");
            }

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked)
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN1usid={SGN1usid.Tag ?? "NULL"}, SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }
            else
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN1usid={SGN1usid.Tag ?? "NULL"}, SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }

            Form_Current();
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = mid; // If Not Me.OKF Then Me.OKF = True

        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN2.IsChecked ?? false);
                SGN2.IsChecked = !SGN_WAS;
                return;
            }

            double mid;
            string SHARH;
            double td;
            mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
            if (mid > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد2 ", " :امضا برداشته شد2:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + NUMBER.Text + ",20 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
            }
            else
            {
                td = Tarikh.GET_OADATE_DAO();
                SHARH = "'پيش فاکتور  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt32(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20, GETDATE() ," + Baseknow.USERCOD + " )");
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد2 ", " :امضا برداشته شد2 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20 )");
            }

            SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN2.IsChecked)
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN2usid={SGN2usid.Tag ?? "NULL"},  SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }
            else
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN2usid={SGN2usid.Tag ?? "NULL"},  SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }

            Form_Current();
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = mid; // If Not Me.OKF Then Me.OKF = True
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN3.IsChecked ?? false);
                SGN3.IsChecked = !SGN_WAS;
                return;
            }

            double mid;
            string SHARH;
            double td;
            mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
            if (mid > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد3 ", " :امضا برداشته شد3:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + NUMBER.Text + ",20 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
            }
            else
            {
                td = Tarikh.GET_OADATE_DAO();
                SHARH = "'پيش فاکتور  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt32(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20, GETDATE() ," + Baseknow.USERCOD + " )");
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 20);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد3 ", " :امضا برداشته شد3 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",20," + this.NUMBER.Text + ",20 )");
            }

            SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN3.IsChecked)
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN3usid={SGN3usid.Tag ?? "NULL"},  SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }
            else
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN3usid={SGN3usid.Tag ?? "NULL"},  SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} WHERE TAG = 20 AND NUMBER = {NUMBER.Text}");
            }

            Form_Current();
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = mid; // If Not Me.OKF Then Me.OKF = True
        }
        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //After Update
            if (PERSONEL.SelectedItem != null && !NewRecord && NUMBER.Text != "0")
            {
                //dbms.DoExecuteSQL($"UPDATE TASKS SET PERSONEL = {PERSONEL.SelectedValue} WHERE IDNUM = {Meidnum}");

                Meidnum = CL_HESABDARI.PERSONELUpdate(20, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'پيش فاکتور  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            else
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }
        }

        private void CDOKDATE_AND_TIME()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text == "0")
            {

                var OKFState = dbms.DoGetDataSQL<bool?>($"SELECT OKF FROM HEAD_LST WHERE TAG = 20 AND NUMBER = {NUMBER.Text}").FirstOrDefault();
                if (!ReferenceEquals(OKFState, null))
                {
                    if (Convert.ToByte(OKFState) == 1)
                    {
                        dbms.DoExecuteSQL($"UPDATE HEAD_LST SET OKTIME = {DateTime.Now.ToString("HHmmss")}, OKDATE = {Tarikh.FullCurrentDate} , OKF = 1 WHERE NUMBER = {NUMBER.Text} AND TAG = 20");
                    }
                }
            }
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

        private void cccc_Click(object sender, RoutedEventArgs e)
        {
            return;
            if (NewRecord || INVO_LST_PISH2_DATA.Count < 1) { return; }

            #region OpenReport

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.PishFactor.INVOICE_PISH_2_MBA.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["PISHFactorMBA"]).CommandTimeout = 900;

            #region GroupFooter3_Format
            var SUMMABL = (report.GetComponentByName("Text67") as StiText).Text;
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(SUMMABL));
            //this.HR.CAPTION = ALPHANUM(this.Text279) + " " + "ريال";

            if ((report.GetComponentByName("DEPART") as StiText).Text == "" || IsNull((report.GetComponentByName("DEPART") as StiText).Text))
            {
                (report.GetComponentByName("DEPART") as StiText).Enabled = false;
                (report.GetComponentByName("DEPNAME") as StiText).Enabled = false;
            }
            #endregion

            ShowEmzaha(report);


            //report.Compile();
            report.Render(false);
            report.Show();
            #endregion
        }

        public void ShowEmzaha(StiReport report)
        {
            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked)
            {
                (report.GetComponentByName("FIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
                (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN2.IsChecked)
            {
                (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER;
                (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN3.IsChecked)
            {
                (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER;
                (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER;
            }
        }

        private void Command114_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || INVO_LST_PISH2_DATA.Count < 1) { return; }

            Process Prc = ProcLoader.Start();


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.PishFactor.order_list_p.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["NUMBER_PARAM"] = NUMBER.Text;

            (report.GetComponentByName("WIDTH_D") as StiText).Text = Baseknow.WIDTH_D;

            //report.Render();
            //report.Show();

            new WINRPT(report, "پیش فاکتور").Show();
            ProcLoader.Stop(Prc);
        }

        private void custprint_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || INVO_LST_PISH2_DATA.Count < 1) { return; }

            Process Prc = ProcLoader.Start();


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.PishFactor.p1.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["NUMBER_PARAM"] = NUMBER.Text;

            (report.GetComponentByName("WIDTH_D") as StiText).Text = Baseknow.WIDTH_D;
            (report.GetComponentByName("USER_TEXT") as StiText).Text = Baseknow.UUSER;
            (report.GetComponentByName("TFADDRESS") as StiText).Text = Baseknow.TFADDRESS;
            (report.GetComponentByName("TFTEL") as StiText).Text = Baseknow.TFTEL;

            //Report_Open
            if (Baseknow.MAND)
            {
                var RST = dbms.DoGetDataSQL<double?>("SELECT SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE HES = '" + CUST_NO.SelectedValue + "'").FirstOrDefault();
                if (RST is null)
                {
                    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                }
                else
                {
                    var PRICE = ((RST > 0) ? RST : (RST * -1));

                    string _fprice_ = "0";
                    if (PRICE > 0)
                        _fprice_ = Strings.Format(PRICE, "بدهكار");
                    else
                        _fprice_ = Strings.Format((PRICE * -1), "بستانكار");

                    (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                }
            }

            //report.Render();
            //report.Show();

            new WINRPT(report, "پیش فاکتور").Show();
            ProcLoader.Stop(Prc);
        }

        private void BTN_FACTORS_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 20);
            //new FACTORS_LST(20).ShowDialog();
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void DEPATMAN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GoGheymateUpdator();
        }

        private void CUST_NO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemQuotes && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (RecordsData != null && RecordsData?.View is ListCollectionView collectionView && collectionView.CurrentPosition > 0)
                {
                    if (CUST_NO.SelectedValue != null)
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا از اعمال نام مشتری قبلی برای این پیش فاکتور مطمئن هستید ؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult == false)
                        {
                            return;
                        }
                    }

                    // Find the index of the previous item
                    int previousIndex = collectionView.CurrentPosition;

                    // Retrieve the previous item based on the previous index
                    if (previousIndex >= 0 && collectionView.GetItemAt(previousIndex) is pish_view previousRecord)
                    {
                        if (previousRecord != null && previousRecord?.CUST_NO != null)
                        {
                            var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + previousRecord.CUST_NO + "'").FirstOrDefault();
                            if (data is not null && !string.IsNullOrEmpty(data.hes))
                            {
                                string thevalue = data.hes;
                                if (CUST_NO.ItemsSource == null)
                                {
                                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                                }
                                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                                {
                                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                                }
                                CUST_NO.SelectedValue = null;
                                CUST_NO.SelectedValue = thevalue;
                                CUST_NO.Items.Refresh();
                            }
                        }
                    }
                }

                e.Handled = true;
            }

        }

        private void DEPATMAN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DEPATMAN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox DEPATMAN_TEX = (TextBox)DEPATMAN.Template.FindName("PART_EditableTextBox", DEPATMAN);

            string _SelectedItem_ = "";
            if (DEPATMAN.SelectedItem != null)
            {
                _SelectedItem_ = ((Prg_Proccessy.SQLMODELS.CTABLES.Custom_DEPART)DEPATMAN.SelectedItem).DEPNAME;
            }

            if (DEPATMAN_TEX.Text != _SelectedItem_)
            {
                e.Handled = true;
                new WIN_SearchDEPART(DEPATMAN_TEX.Text.Trim(), I_AM_PISHFACTOR).ShowDialog();
            }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }
        }

        private void PEPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEPID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها

                GoGheymateUpdator();
            }
        }
        private void PEID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها

                GoGheymateUpdator();
            }
        }

        private void PEPID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GoGheymateUpdator()
        {
            var IsSavedBefore = CL_LMethods.IsNumeric(NUMBER.Text) && NUMBER.Text != "0";
            var IsTabdilShodeh = ((FrameworkElement)TAMIR.SelectedValue).Tag.ToString() == "4";

            if (NowIsReady && CL_Generaly.IsGHAYM_7 && IsSavedBefore && !IsTabdilShodeh)
            {
                if (SGN1.IsChecked is false && SGN2.IsChecked is false && SGN3.IsChecked is false)
                {
                    if (CUST_KIND.SelectedValue != null && DEPATMAN.SelectedValue != null && MODAT_PPID.SelectedValue != null)
                    {
                        //if (PEID.SelectedValue is null)
                        //{
                        //    CL_HESABDARI.UpdateGHeymat(Convert.ToInt32(NUMBER.Text), TAG, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked));
                        //}
                        //else if (PEID.SelectedValue != null && PEPID.SelectedValue != null)
                        //{
                        //    CL_HESABDARI.UpdateGHeymatFF(Convert.ToInt32(NUMBER.Text), TAG, Convert.ToInt32(PEPID.SelectedValue), Convert.ToInt32(PEID.SelectedValue), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), Convert.ToInt32(CUST_KIND.SelectedValue));
                        //}

                        if (Convert.ToInt32(MODAT_PPID.SelectedValue) == 0) //اگر آزاده بیا بیرون
                        {
                            return;
                        }

                        int retVal = ExecutePricingUpdate(
                            Convert.ToInt32(NUMBER.Text),
                            TAG,
                            PEPID.SelectedValue is null ? 0 : Convert.ToInt32(PEPID.SelectedValue),
                            PEID.SelectedValue is null ? 0 : Convert.ToInt32(PEID.SelectedValue),
                            Convert.ToInt32(MODAT_PPID.SelectedValue),
                            TICMBAA.IsChecked == true,
                            Convert.ToInt32(CUST_KIND.SelectedValue),
                            Convert.ToInt32(DATE_N.Text.ToRawTarikh()),
                            Convert.ToInt32(DEPATMAN.SelectedValue));

                        string? strSpecificError = default;
                        if (retVal != 0)
                        {
                            switch (retVal)
                            {
                                case -1:
                                    strSpecificError = "خطا: اعلامیه قیمت فعال یافت نشد.";
                                    break;
                                case -2:
                                    strSpecificError = "خطا: قیمت برای یک یا چند کالا در اعلامیه قیمت مشخص، تعریف نشده است.";
                                    break;
                                case -99:
                                    strSpecificError = "خطا: یک خطای عمومی در پایگاه داده رخ داد.";
                                    break;
                                default:
                                    strSpecificError = "خطا: عملیات ناموفق بود. کد خطای ناشناخته: " + retVal;
                                    break;
                            }
                            new Msgwin(false, strSpecificError).ShowDialog();
                        }

                        INVO_LST_SUB_ReGetData();

                        if (!string.IsNullOrEmpty(strSpecificError)) //Error Happened
                        {
                            return;
                        }

                        ChangeIsHappend = true;
                        universControl.PopNotifyShowUp("قیمت بروز شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);



                        IF_NOT_IS_AZAD_Then_Lock();
                    }
                }
            }
        }
        private int ExecutePricingUpdate(int numb, int tgg, int pepid, int peid, int modat_ppid, bool ticmbaa, int cust_kind, int dtt, int depatman)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@numb", numb);
            param.Add("@tgg", tgg);
            param.Add("@PEPID_In", pepid);
            param.Add("@PEID_In", peid);
            param.Add("@MODAT_PPID_In", modat_ppid);
            param.Add("@TICMBAA_In", ticmbaa);
            param.Add("@CUST_KIND_In", cust_kind);
            param.Add("@DTT_In", dtt);
            param.Add("@DEPATMAN_In", depatman);
            param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            dbms.OpenStoredProcedure("sp_UpdateInvoicePricingAndDiscount", param);

            return param.Get<int>("@ReturnValue");
        }

        private void TICMBAA_Checked(object sender, RoutedEventArgs e)
        {

        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_SUB.Items.Count > 0)
            {
                if (INVO_LST_SUB.SelectedItem is not null)
                {
                    var Row = INVO_LST_SUB.SelectedItem as INVO_LST_FACTOR22;
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
