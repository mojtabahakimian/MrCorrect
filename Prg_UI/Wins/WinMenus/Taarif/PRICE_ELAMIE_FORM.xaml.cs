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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Syncfusion.Data.Extensions;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.ComponentModel;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Wins.WinOther;
using static Interfaces.INavigator;
using Prg_Proccessy.Generaly;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using Microsoft.VisualBasic;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    public partial class PRICE_ELAMIE_FORM : Window, ISearchableWindow
    {
        public PRICE_ELAMIE_FORM()
        {
            InitializeComponent();

            this.DataContext = this;
        }

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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        private NavigationManager<PRICE_ELAMIE> _navigationManager;
        public ObservableCollection<PRICE_ELAMIE_DTL> PRICE_ELAMIE_DTL_DATA { get; set; } = new ObservableCollection<PRICE_ELAMIE_DTL>();

        #region LOCAL_MODEL
        public class PGID_MODEL
        {
            public int? PGID { get; set; }
            public string? PGNAME { get; set; }
        }
        #endregion

        public bool NowIsReady { get; private set; }
        public bool DG_SUB_IsFocused { get; private set; }

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                return _newrecord;
            }
            set { _newrecord = value; }
        }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int DG_SUB_DEF_INDEX_COL
        {
            get
            {
                if (DG_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = DG_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "PGID")?.DisplayIndex;
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
        private List<COMBOPERSONEL> rst_personel;

        public byte TAG { get; } = 30;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                PEPNAME.IsReadOnly = !ican; //نام عنوان
                PEPDATE.IsReadOnly = !ican; //تاریخ از اعمال
                DG_SUB.IsReadOnly = !ican;

                PEPDEPART.IsEnabled = ican; //واحد دپارتمان
                BTN_SAVE.IsEnabled = ican; //ذخیره
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = DG_SUB;
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (DG_SUB.IsKeyboardFocusWithin)
                    {
                        try
                        {
                            if (DG.CurrentColumn != null)
                            {
                                int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                                bool isLastColumn = currentColumnIndex == DG.Columns.Count - 2;
                                bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                                if (isLastColumn)
                                {
                                    // If it's the last column, move focus to the first cell of next row
                                    if (isLastRow)
                                    {
                                        // Add focus to new row if needed
                                        DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DG_SUB_DEF_INDEX_COL]);

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
                        catch { /*ignore*/ }

                    }
                    else if (BTN_SAVE.IsFocused)
                    {
                        BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        return;
                    }

                    CL_LMethods.SendKey_US(Key.Tab);
                }
                else
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.S || e.SystemKey == Key.S))
                    {
                        e.Handled = true;
                        BTN_SAVE_Click(null, null);
                    }
                }

                if (!DG_SUB.IsKeyboardFocusWithin && !DG_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VISIT_ROUTE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            USERNAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            _navigationManager = new NavigationManager<PRICE_ELAMIE>(
                dbms,
                x => x?.PEPID?.ToString(),
                $"SELECT * FROM PRICE_ELAMIE ORDER BY CRT",
                x => $"SELECT * FROM PRICE_ELAMIE WHERE PEPID = {x?.PEPID} ",
                default);

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            if (!NewRecord)
            {
                AllowEdits = false;
            }

            if (Baseknow.SIGN ?? false)
            {
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
                SGN3.Visibility = Visibility.Visible;
            }
            else
            {
                SGN1.Visibility = Visibility.Hidden;
                SGN2.Visibility = Visibility.Hidden;
                SGN3.Visibility = Visibility.Hidden;
            }

            CL_LMethods.SetTabIndexes(
             PEPNAME,
             PEPDATE,
             PEPDEPART,
             BTN_SAVE,
             DG_SUB
             );

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(PRICE_ELAMIE record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<PRICE_ELAMIE>($"SELECT * FROM PRICE_ELAMIE WHERE PEPID = {PEPID.Text}").FirstOrDefault();
                record = itemtoadd;
                NewRecord = false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(PRICE_ELAMIE HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, PRICE_ELAMIE_DTL_DATA);
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین آیتمی ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new
                Command106.IsEnabled = true;

                PEPID.Text = HEADER_FAC.PEPID.ToString();
                PEPNAME.Text = HEADER_FAC?.PEPNAME; //نام عنوان
                PEPDATE.Text = HEADER_FAC?.PEPDATE?.ToString(); //تاریخ از اعمال
                PEPDEPART.SelectedValue = HEADER_FAC?.PEPDEPART; //دپارتمان(واحد)


                SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1 ?? false);
                SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2 ?? false);
                SGN3.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN3 ?? false);

                SGN1usid.Tag = null; SGN2usid.Tag = null; SGN3usid.Tag = null;

                //if (HEADER_FAC?.sgn1usid != null)
                //{
                //    SGN1usid.Tag = Convert.ToInt32(HEADER_FAC.sgn1usid);
                //}
                //if (HEADER_FAC?.sgn2usid != null)
                //{
                //    SGN2usid.Tag = Convert.ToInt32(HEADER_FAC.sgn2usid);
                //}
                //if (HEADER_FAC?.sgn3usid != null)
                //{
                //    SGN3usid.Tag = Convert.ToInt32(HEADER_FAC.sgn3usid);
                //}

                //SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn1usid)?.SAL_NAME;
                //SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.SAL_NAME;
                //SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                ESLAH.IsEnabled = true;

                Form_Current();

                DG_SUB_ReGetData();
            }
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;

        /*
         * PEPNAME.IsReadOnly = !ican; //نام عنوان
           PEPDATE.IsReadOnly = !ican; //تاریخ از اعمال
           PEPDEPART.IsReadOnly = !ican; //دپارتمان(واحد)
           
           PEPDEPART.IsEnabled = ican; //واحد دپارتمان
           BTN_SAVE.IsEnabled = ican; //ذخیره
           Command106.IsEnabled = ican; //چاپ
           BTN_DELETE.IsEnabled = ican; //حذف
         */

        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is PRICE_ELAMIE item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.PEPID.Equals(item.PEPID));
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
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "PEPID", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "نام", PropertyPath = "PEPNAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تاریخ اعمال از", PropertyPath = "PEPDATE", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USERNAME", PropertyType = typeof(string) },
            };
        }
        #endregion

        private void Form_Current()
        {
            if (NewRecord || string.IsNullOrEmpty(PEPID.Text) || PEPID.Text == "0")
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                if (!(SGN1.IsChecked ?? false))
                {
                    DG_SUB.IsReadOnly = false;
                    AllowDeletions = true;

                    if (!string.IsNullOrEmpty(PEPID.Text))
                    {
                        AllowDeletions = false;
                        AllowEdits = false;
                        DG_SUB.IsReadOnly = true;
                    }
                    else
                    {
                        AllowDeletions = true;
                        AllowEdits = true;
                        DG_SUB.IsReadOnly = false;
                    }
                }
                else
                {
                    DG_SUB.IsReadOnly = true;
                    AllowDeletions = false;
                    AllowEdits = false;
                }
            }
            //AllowDeletions = false;
            //AllowEdits = false;
        }
        private void RefreshAfterUpdate()
        {
            NewRecord = false;
            var CURRENT_HEADER = dbms.DoGetDataSQL<PRICE_ELAMIE>($"SELECT * FROM PRICE_ELAMIE WHERE PEPID = {PEPID.Text}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }
        private void FILL_ALL_COMBOBOXES()
        {
            PEPDEPART.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            PEPDEPART.DisplayMemberPath = "DEPNAME";
            PEPDEPART.SelectedValuePath = "DEPATMAN";
            PEPDEPART.SelectionChanged -= PEPDEPART_SelectionChanged;
            PEPDEPART.SelectedValue = CL_Generaly.VAHED_OF_USER; PEPDEPART.Items.Refresh();
            PEPDEPART.SelectionChanged += PEPDEPART_SelectionChanged;

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

            //گروه قیمتی
            PGID_COLUMN.ItemsSource = dbms.DoGetDataSQL<PGID_MODEL>("SELECT PGID, PGNAME FROM PRICE_GRP").ToList();
        }
        private void MakeDefaultFocuseReady()
        {
            PEPNAME.Focus();
            PEPNAME.SelectAll();
        }
        private void DataGridActivation()
        {
            if (NewRecord)
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                DG_SUB.IsReadOnly = false;
            }
        }
        private void ClearFreshAll()
        {
            NewRecord = true;

            PEPID.Text = null;
            PEPNAME.Text = null;
            USERNAME.Text = Baseknow.UUSER;
            PEPDATE.Text = Tarikh.FullCurrentDate;
            PEPDEPART.SelectionChanged -= PEPDEPART_SelectionChanged;
            PEPDEPART.SelectedValue = CL_Generaly.VAHED_OF_USER; PEPDEPART.Items.Refresh();
            PEPDEPART.SelectionChanged += PEPDEPART_SelectionChanged;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.Text = null;
            PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            ESLAH.IsEnabled = false;
            Command106.IsEnabled = false;

            PRICE_ELAMIE_DTL_DATA?.Clear(); //دیتاگرید فاکتور فروش
            AllowEdits = true;

            DG_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "PGID", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "HENTER", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //CL_HESABDARI.SETSECURITYSUB(DG_SUB, "HENTER");

            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(PEPNAME.Text) || string.IsNullOrWhiteSpace(PEPNAME.Text)) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام نمیتواند خالی باشد" });
            }

            var _PEPDATE_ = PEPDATE.Text.ToRawTarikh();
            if (string.IsNullOrEmpty(_PEPDATE_))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ اعمال نمیتواند خالی باشد" });
            }
            else if (!Tarikh.IsValidedDate(_PEPDATE_))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ اعمال صحیح نمی باشد" });
            }

            if (PEPDEPART.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد (دپارتمان) نمیتواند خالی باشد" });
            }

            var dt = PEPDATE.Text.ToRawTarikh();
            var dep = PEPDEPART.SelectedValue;
            var cnt = dbms.DoGetDataSQL<int>(
                "SELECT COUNT(*) FROM dbo.HEAD_LST INNER JOIN dbo.PRICE_ELAMIE ON dbo.HEAD_LST.PEPID = dbo.PRICE_ELAMIE.PEPID " +
                "WHERE (DEPATMAN = @dep) AND (DATE_N > @dt) AND (TAG = 2 OR TAG = 20)", new { dep, dt }).FirstOrDefault();

            if (cnt > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "در این محدوده قبلا فاکتور صادر شده است، بنابراین نمی‌توانید اعلامیه جدید صادر کنید!" });
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

        private bool BodyIsValid(PRICE_ELAMIE_DTL TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (TheRow?.PGID == null || TheRow?.PGID <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروهی قیمتی نیمتواند خالی باشد" });
            }

            if (TheRow?.PRICE1 == null || TheRow?.PRICE1 < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
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

        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }
        public Visual I_AM_VISIT_ROUTE { get; private set; }
        public PRICE_ELAMIE_DTL? CURRENT_ROW_ITEMS { get; private set; }
        public PRICE_ELAMIE_DTL? WAS_ROW_ITEM { get; private set; } = new PRICE_ELAMIE_DTL();
        public double Meidnum { get; private set; }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            try
            {
                if (!DoCmdHeaderSave())
                {
                    return;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"این عنوان قبلا تعریف شده نمیتوان عنوان تکراری تعریف کرد").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }

            this.DG_SUB.IsReadOnly = false;
            ESLAH.IsEnabled = true;

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (PRICE_ELAMIE_DTL_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if ((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false))
            {
                if (!(sender is null))
                {
                    Msgwin msgwin = new Msgwin(false, "اول امضا را بردارید ..."); msgwin.ShowDialog(); return;
                }
            }

            if (!NewRecord && !string.IsNullOrEmpty(PEPID.Text))
            {
                SecurityAllCheck();

                var dt = DateTime.Now;
                CL_HESABDARI.TR("PRICE_ELAMIE", "(PEPID = " + PEPID.Text + $")", dt, 1); //12
                CL_HESABDARI.TR("PRICE_ELAMIE_DTL", "(PEPID = " + PEPID.Text + $")", dt, 1); //12

                AllowDeletions = true;
                AllowEdits = true;
                DG_SUB.IsReadOnly = false; // UnLocked
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (NewRecord || DG_SUB.IsEnabled == false || !BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (PRICE_ELAMIE_DTL_DATA.Count > 0)
            {
                if (DG_SUB.IsReadOnly) { return; }

                try
                {
                    var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(DG_SUB.ItemsSource);
                    if (view.IsAddingNew && view.CanCancelEdit)
                    {
                        //view.CancelNew();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else if (view.IsEditingItem && view.CanCancelEdit)
                    {
                        //view.CancelEdit();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else
                    {
                        //Cancel Any Editting to avoid conflict during remove
                        DG_SUB.CommitEdit(DataGridEditingUnit.Cell, true);
                        DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                    }
                }
                catch { }
            }


            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                bool IsDeletedSomething = false;

                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف اعلامیه قیمت",
                    recordId: PEPNAME.Text,
                    oldValue: "",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                if (PRICE_ELAMIE_DTL_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    //CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.PEPID.Text + $") AND (TAG = {FTAG})", dt, 1); //1
                    #endregion

                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                    {
                        var item = DG_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                        {
                            PRICE_ELAMIE_DTL_DATA.Remove((PRICE_ELAMIE_DTL)item);
                            continue; // Skip deletion for new placeholder items
                        }

                        var _PEPID_ = item.GetType().GetProperty("PEPID").GetValue(item);
                        var _PGID_ = item.GetType().GetProperty("PGID").GetValue(item);

                        if (_PEPID_ != null && _PGID_ != null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"DELETE FROM dbo.PRICE_ELAMIE_DTL WHERE PEPID = {_PEPID_} AND PGID = {_PGID_}");

                                IsDeletedSomething = true;
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
                    else if (IsDeletedSomething)
                    {
                        DG_SUB_ReGetData();
                    }
                }
                else
                {
                    if (!NewRecord)
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.PRICE_ELAMIE WHERE PEPID = {PEPID.Text} ");

                            //ClearFreshAll();
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
                                new Msgwin(false, "این مسیر ویزیت دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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

        private bool DoCmdHeaderSave(bool DisplayMsg = true)
        {
            int _PEPID_ = Convert.ToInt32(string.IsNullOrEmpty(PEPID.Text) ? "0" : PEPID.Text);
            if (_navigationManager.IsNewRecord)
            {
                _PEPID_ = (int)CL_HESABDARI.GetLIDD("PRICE_ELAMIE", "PEPID");
            }
            string PEPNAME_TEX = PEPNAME.Text.Trim();

            var masterRecord = new PRICE_ELAMIE
            {
                PEPNAME = PEPNAME_TEX,
                PEPID = _PEPID_,
                PEPDATE = Convert.ToInt32(PEPDATE.Text.ToRawTarikh()),
                PEPDEPART = (int)PEPDEPART.SelectedValue,
                TR_DATE = DateTime.Now,
                SGN1 = SGN1.IsChecked ?? false,
                SGN2 = SGN2.IsChecked ?? false,
                SGN3 = SGN3.IsChecked ?? false,
                USERNAME = USERNAME.Text,
            };

            var RowExisting = dbms.DoGetDataSQL<string?>($"SELECT 1 FROM PRICE_ELAMIE WHERE PEPNAME = @PEPNAME AND PEPDATE = @PEPDATE",
                new { PEPNAME = PEPNAME.Text, PEPDATE = Convert.ToInt32(PEPDATE.Text.ToRawTarikh()) }).FirstOrDefault();

            if (NewRecord && RowExisting != null)
            {
                Msgwin msgwin0 = new Msgwin(true, $"این اعلامیه به نام '{PEPNAME_TEX}' با تاریخ اعمال {PEPDATE.Text.ToRawTarikh()} از قبل وجود دارد , آیا از اضافه کردن تکراری آن مطمئن هستید ؟");
                _ = msgwin0.ShowDialog();
                return false;
            }

            if (_navigationManager.IsNewRecord) //Insert
            {
                string insertSql = @"
                                    INSERT INTO PRICE_ELAMIE 
                                        (PEPID, PEPNAME, PEPDATE, PEPDEPART, TR_DATE, SGN1, SGN2, SGN3, USERNAME)
                                    VALUES 
                                        (@PEPID, @PEPNAME, @PEPDATE, @PEPDEPART, @TR_DATE, @SGN1, @SGN2, @SGN3, @USERNAME)";
                _ = dbms.DoExecuteSQL(insertSql, masterRecord);
                PEPID.Text = _PEPID_.ToString();
                RefreshAfterUpdate();
            }
            else
            {
                string updateSql = @"
                                    UPDATE PRICE_ELAMIE
                                    SET
                                        PEPNAME = @PEPNAME,
                                        PEPDATE = @PEPDATE,
                                        PEPDEPART = @PEPDEPART,
                                        SGN1 = @SGN1,
                                        SGN2 = @SGN2,
                                        SGN3 = @SGN3
                                    WHERE
                                        PEPID = @PEPID";
                _ = dbms.DoExecuteSQL(updateSql, masterRecord);
            }

            return true;
        }

        public void DG_SUB_ReGetData()
        {
            if (!NewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<PRICE_ELAMIE_DTL>(@$"SELECT * FROM dbo.PRICE_ELAMIE_DTL WHERE PEPID = @PEPID ORDER BY CRT", new { PEPID = PEPID.Text }).ToList();

                PRICE_ELAMIE_DTL_DATA?.Clear();
                foreach (var item in QRE_LST)
                    PRICE_ELAMIE_DTL_DATA?.Add(item);
            }
        }
        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || PRICE_ELAMIE_DTL_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.DASHBOARD.ELAMIEY_DEF.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["NUMBER_PARAM"] = PEPID.Text;
            (report.GetComponentByName("USERNAME") as StiText).Text = Baseknow.UUSER;
            new WINRPT(report, "اعلامیه قیمت").Show();
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.Invoke(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_SUB.CancelEdit();
                    //DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                }
                else
                {
                    DG_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                    //DG_SUB.CommitEdit((DataGridEditingUnit)_RC_, true);
                }
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }


        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is PRICE_ELAMIE_DTL rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = DG_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as PRICE_ELAMIE_DTL;
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.EditingElement == null || e.Column == null)
            {
                return;
            }

            #region REFILL_CURRENTS
            ComboBox Comboval = null; TextBox TexboVal = null; CheckBox? CheckVal = null;
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

            CURRENT_ROW_ITEMS = e.Row.Item as PRICE_ELAMIE_DTL;
            if (CURRENT_ROW_ITEMS == null)
            {
                return;
            }

            if (!(e.EditingElement is null))
            {
                CheckVal = e.EditingElement as CheckBox;
            }

            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                ENTERED_VALUE_ROW = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();

            ComboBox HES_COMBO = null;
            if (e.EditingElement is ContentPresenter contentPresenter)
            {
                HES_COMBO = contentPresenter.ContentTemplate.FindName("EditCombo", contentPresenter) as ComboBox;

                if (HES_COMBO == null)
                {
                    HES_COMBO = DataGridHelper.FindVisualChild<ComboBox>(contentPresenter);
                }
                if (HES_COMBO != null)
                {
                    ENTERED_VALUE_ROW = HES_COMBO.Text;
                }
            }
            #endregion

            //نام مشتری
            //if (e.Column.SortMemberPath == "NAME_HES" || e.Column.Header.ToString() == "نام مشتری")
            //{
            //    var HSC = HES_COMBO?.SelectedItem as CUST_HESAB_COMBINED;
            //    if (HES_COMBO?.SelectedValue is null || HSC?.NAME != ENTERED_VALUE_ROW) //if is different then
            //    {
            //        var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HES_COMBO, dbms);
            //        if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            //        {
            //            CURRENT_ROW_ITEMS.COUST_NO = WAS_ROW_ITEM.COUST_NO;
            //            CURRENT_ROW_ITEMS.NAME_HES = WAS_ROW_ITEM.NAME_HES;
            //            universControl.PopNotifyShowUp($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
            //        }
            //        else
            //        {
            //            CURRENT_ROW_ITEMS.COUST_NO = _SelectedHesab_.hes;
            //            CURRENT_ROW_ITEMS.NAME_HES = _SelectedHesab_.NAME;


            //            //COUST_NO_BeforeUpdate
            //            // تعیین سطح حساب برای اعمال فیلتر مناسب
            //            UpdatePathForOthers(CURRENT_ROW_ITEMS);
            //        }
            //    }
            //    else
            //    {
            //        CURRENT_ROW_ITEMS.NAME_HES = HSC.NAME;
            //    }
            //}
        }
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (!HeaderIsValid()) { return; }

            var ROW = e.Row.Item as PRICE_ELAMIE_DTL;
            if (e.Row.Item == null || ROW is null) { return; }

            if (ConstructorRowDetector.IsPristine(ROW)) { DG_SUB_CANCEL_EDIT(); return; } //اگر سطر «دست‌نخورده» است، بدون خطا عمل کن

            if (!BodyIsValid(ROW))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            ROW.PEPID = Convert.ToInt32(PEPID.Text); //PeP
            ROW.USERNAME = Baseknow.UUSER; //PeP

            int? idd = null;
            try
            {
                if (ROW?.PERID is null || ROW?.PERID == 0) //INSERT
                {
                    // بررسی وجود رکورد با کلید جدید
                    var duplicatePGID = dbms.DoGetDataSQL<PRICE_ELAMIE_DTL>(
                        "SELECT TOP 1 * FROM dbo.PRICE_ELAMIE_DTL WHERE PEPID = @PEPID AND PGID = @PGID",
                        new { PEPID = ROW.PEPID, PGID = ROW.PGID }).FirstOrDefault();

                    if (duplicatePGID != null)
                    {
                        DG_SUB_CANCEL_EDIT();
                        universControl.PopNotifyShow("این گروه قیمتی قبلاً ثبت شده (تکراری) است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    //Getting New PERID for New Row
                    var NewPerID = (int)CL_HESABDARI.GetLIDD("PRICE_ELAMIE_DTL", "PERID");
                    var DetailRecord = new PRICE_ELAMIE_DTL
                    {
                        PEPID = ROW.PEPID,
                        PERID = NewPerID, // PeR
                        PGID = ROW.PGID,
                        PRICE1 = ROW.PRICE1,
                        USERNAME = ROW.USERNAME,
                        TR_DATE = DateTime.Now,
                        UID = Baseknow.USERCOD
                    };

                    dbms.DoExecuteSQL(@"
                             INSERT INTO PRICE_ELAMIE_DTL
                                (PERID, PEPID, PRICE1, TR_DATE, USERNAME, PGID, UID)
                            VALUES
                                (@PERID, @PEPID, @PRICE1, @TR_DATE, @USERNAME, @PGID, @UID)", DetailRecord);

                    idd = NewPerID;
                }
                else //UPDATE
                {
                    bool duplicateExistsInMemory = PRICE_ELAMIE_DTL_DATA.Count(x => x.PGID == ROW.PGID) > 1; // اگر بیش از یکی بود یعنی تکراری

                    if (duplicateExistsInMemory)
                    {
                        DG_SUB_CANCEL_EDIT();
                        universControl.PopNotifyShow("این گروه قیمتی قبلاً اضافه شده است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    var DetailRecord = new PRICE_ELAMIE_DTL
                    {
                        PEPID = ROW.PEPID,
                        PERID = ROW.PERID, // PeR

                        PGID = ROW.PGID,
                        PRICE1 = ROW.PRICE1,
                        USERNAME = ROW.USERNAME,
                    };

                    string updateSql = @"
                                    UPDATE PRICE_ELAMIE_DTL
                                    SET
                                        PEPID = @PEPID,
                                        PGID = @PGID
                                        PRICE1 = @PRICE1,
                                        USERNAME = @USERNAME,
                                    WHERE
                                        PERID = @PERID";

                    dbms.DoExecuteSQL(updateSql, DetailRecord);
                }

            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "آیتم تکراری وارد شده آنرا اصلاح کنید").ShowDialog();
                }

                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
            }
            if (idd != null) //So Much Important
            {
                ROW.PERID = (int)idd;
            }

        }

        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MouseRightButtonUp="DG_SUB_MouseRightButtonUp"
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
        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && DG_SUB.SelectedItem != null)
            {
                if (!(e is null) && DG_SUB.SelectedItem is not null)
                {
                    if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((PRICE_ELAMIE_DTL)DG_SUB.SelectedItem).Clone() as PRICE_ELAMIE_DTL;
                    }
                }
            }
        }
        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (DG_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = DG_SUB.CurrentCell.Column?.SortMemberPath;
            }
            else
            {
                return;
            }

            string ColumnTarget = "";
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME.Contains("PRICE1", StringComparison.OrdinalIgnoreCase))
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
                if (CURRENT_COLUMN_NAME.Contains("PRICE1", StringComparison.OrdinalIgnoreCase))
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

            if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            {
                try
                {
                    // 1) اگر داخل یک TextBox در حالت ویرایش هستیم، کاری نکنیم
                    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
                    {
                        // اجازه بدهید Delete عادی متن کارش رو بکنه
                        return;
                    }
                    //else
                    //{
                    //    // اگر داخل حالت ویرایش سلول هستیم، از رفتار پیش‌فرض Delete (حذف کاراکتر) استفاده کن
                    //    var cell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                    //    if (cell != null && cell.IsEditing)
                    //        return;
                    //}
                }
                catch { }

                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
        }

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PEPID.Text) || PEPID.Text == "0")
            {
                var wasChecked = SGN1.IsChecked ?? false;
                SGN1.IsChecked = !wasChecked;
                return;
            }

            //double mid;
            //string sharh;
            //double td;
            //var TAG = 30;

            //mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(PEPID.Text), TAG);
            //if (mid > 0d)
            //{
            //    dbms.DoExecuteSQL(
            //        $"INSERT INTO events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG) " +
            //        $"VALUES ({mid},'{CL_HESABDARI.UCurrentUser()}'," +
            //        $"'{CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD))}" +
            //        $"{(SGN1.IsChecked == true ? " :امضا شد1 " : " :امضا برداشته شد1:")}'," +
            //        $"{Tarikh.FullCurrentDate}," +
            //        $"{(DateTime.Now.Hour * 100 + DateTime.Now.Minute)},{TAG},{PEPID.Text},{TAG})");
            //    dbms.DoExecuteSQL(
            //        $"UPDATE TASKS SET PERSONEL = {CL_HESABDARI.GETUSERTASK(mid)}, STATUS = 1 WHERE IDNUM = {mid}");
            //}
            //else
            //{
            //    td = Tarikh.GET_OADATE_DAO();
            //    sharh = $"'اعلامیه قیمت شماره: {PEPID.Text} مورخ " +
            //            $"{Strings.Format(Convert.ToInt32(PEPDATE.Text.ToRawTarikh()), "####/##/##")}', '0'";
            //    dbms.DoExecuteSQL(
            //        $"INSERT INTO TASKS(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO) " +
            //        $"VALUES ({Baseknow.USERCOD},'{CL_HESABDARI.UCurrentUser()}',{sharh}," +
            //        $"{Tarikh.FullCurrentDate},{(DateTime.Now.Hour * 100 + DateTime.Now.Minute)}," +
            //        $"{TAG},{PEPID.Text},{TAG},GETDATE(),{Baseknow.USERCOD})");
            //    mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(PEPID.Text), TAG);
            //    dbms.DoExecuteSQL(
            //        $"INSERT INTO EVENTS(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG) " +
            //        $"VALUES ({mid},'{CL_HESABDARI.UCurrentUser()}'," +
            //        $"'{CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD))}" +
            //        $"{(SGN1.IsChecked == true ? " : امضا شد1 " : " :امضا برداشته شد1 ")}'," +
            //        $"{Tarikh.FullCurrentDate},{(DateTime.Now.Hour * 100 + DateTime.Now.Minute)}," +
            //        $"{TAG},{PEPID.Text},{TAG})");
            //}
            //Meidnum = mid;

            ////SGN1usid.Tag = Baseknow.USERCOD;
            ////SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD)?.SAL_NAME;

            dbms.DoExecuteSQL($"UPDATE dbo.PRICE_ELAMIE SET SGN1={Convert.ToByte(SGN1.IsChecked ?? false)} WHERE PEPID={PEPID.Text}");

            Form_Current();
            PERSONEL.Visibility = Visibility.Visible;
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PEPID.Text) || PEPID.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN2.IsChecked ?? false);
                SGN2.IsChecked = !SGN_WAS;
                return;
            }

            dbms.DoExecuteSQL($"UPDATE dbo.PRICE_ELAMIE SET SGN2={Convert.ToByte(SGN2.IsChecked ?? false)} WHERE PEPID={PEPID.Text}");
            Form_Current();
            PERSONEL.Visibility = Visibility.Visible;
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PEPID.Text) || PEPID.Text == "0")
            {
                var SGN_WAS = Convert.ToBoolean(SGN3.IsChecked ?? false);
                SGN3.IsChecked = !SGN_WAS;
                return;
            }

            dbms.DoExecuteSQL($"UPDATE dbo.PRICE_ELAMIE SET SGN3={Convert.ToByte(SGN3.IsChecked ?? false)} WHERE PEPID={PEPID.Text}");
            Form_Current();
            PERSONEL.Visibility = Visibility.Visible;
        }
        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //After Update
            if (PERSONEL.SelectedItem != null && !NewRecord && PEPID.Text != "0")
            {
                Meidnum = CL_HESABDARI.PERSONELUpdate(TAG, Convert.ToDouble(PEPID.Text),
                    Convert.ToInt32(PERSONEL.SelectedValue), "'اعلامیه قیمت  شماره: " + PEPID.Text
                    + " مورخ " + Strings.Format(Convert.ToInt64(PEPDATE.Text.ToRawTarikh()), "####/##/##") +
                    "  به نام: " + PEPNAME.Text + "'");

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
        private void PEPDEPART_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
