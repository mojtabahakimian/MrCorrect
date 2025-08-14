using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using MimeTypes;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using static Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;

namespace Wins.WinMenus.Taarif
{
    public partial class AZAE_WIN : Window, INavigator
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
        double? INavigator.NUMBER_TO_OPEN { get => throw new NotImplementedException(); set => throw new NotImplementedException(); } //خنثی

        public AZAE_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<GRADE_CUST_TAB> MasterData { get; set; } = new ObservableCollection<GRADE_CUST_TAB>(); //Master
        public long? DTTIM { get; set; }
        public int? GCTABID_FORM { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public string? NUMBER_TO_OPEN { get; set; } = "115-1-1003";
        public bool NowIsReady { get; private set; }

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                return _newrecord;
            }
            set { _newrecord = value; }
        }
        public long? HEADER_ID { get; set; }
        public void ReGetMasterData()
        {
            // Load master records
            var MasterHead = dbms.DoGetDataSQL<AZAE>("SELECT HES, TOPETEB, TOPASN, ENDIS, TOPMEGH, JAMZARIB, EMTIAZ, gradeid, DTTIM, CRT, UID, ID FROM dbo.AZAE").ToList();

            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                AZAE? item = null;
                if (NUMBER_TO_OPEN.Contains("-"))
                    item = RecordsData.View.Cast<AZAE>().FirstOrDefault(x => x.HES?.Trim() == NUMBER_TO_OPEN.Trim()); //حساب
                else
                    item = RecordsData.View.Cast<AZAE>().FirstOrDefault(x => x.ID.Equals(Convert.ToInt64(NUMBER_TO_OPEN)));

                if (item != null)
                {
                    // Set the CurrentItem to the found item
                    RecordsData.View.MoveCurrentTo(item);
                    MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                }
                else
                {
                    MoveReGetData(INavigator.Jahat.LastItem);
                    new Msgwin(false, "همچین کدی پیدا نشد !").ShowDialog();
                }
            }
            else
            {
                MoveReGetData(INavigator.Jahat.LastItem);
            }
        }
        public void ReGetData()
        {
            var MasterHead = dbms.DoGetDataSQL<GRADE_CUST_TAB>($"SELECT GCTABID, GCNAME, GCZARIB, GCCUST_HES, GCDATE, USERNAME, CRT, UID FROM GRADE_CUST_TAB WHERE GCCUST_HES = N'{HES.SelectedValue}' ");
            MasterData?.Clear();
            foreach (var item in MasterHead) { MasterData.Add(item); }

            // For each master record, load detail records
            foreach (var gradeCustomer in MasterData)
            {
                var detailData = dbms.DoGetDataSQL<GRADE_CUST_GRP>("SELECT GCGRPID, GCGRPNAME, GCGRPZARIB, GCGRPGRADE, GCTABID, GCVALUE, GCVALUESCAL, GCPS, CRT, UID FROM GRADE_CUST_GRP WHERE GCTABID = @GCTABID", new { GCTABID = gradeCustomer.GCTABID });

                gradeCustomer.DETAILROWDATA = new ObservableCollection<GRADE_CUST_GRP>(detailData);
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
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1)
                    {
                        //var nextitem = RecordsData.View.CurrentPosition + 1;
                        //if (nextitem != null)
                        //{
                        //}
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
                var HEADER = RecordsData.View.CurrentItem as AZAE;
                var DBData = dbms.DoGetDataSQL<AZAE>($"SELECT HES, TOPETEB, TOPASN, ENDIS, TOPMEGH, JAMZARIB, EMTIAZ, gradeid, DTTIM, CRT, UID, ID FROM dbo.AZAE WHERE ID = {HEADER.ID}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(AZAE).GetProperties();
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
                //ClearFreshNew();

                //OKF.IsChecked = false; //تیک تایید
                Form_Current();
            }
            else
            {
                UiDataUpdate();
            }
        }
        public void ClearFreshNew()
        {
            HEADER_ID = null;
            HES.SelectedIndex = -1; HES.Items.Refresh();
            TOPETEB.Text = "0"; // سقف اعتبار ریالی
            TOPMEGH.Text = "0"; // سقف اعتبار مقداری
            TOPASN.IsChecked = false;
            ENDIS.IsChecked = false;
            JAMZARIB.Text = "0"; //جمع ضرائب
            gradeid.SelectedIndex = -1; gradeid.Items.Refresh();
            EMTIAZ.Text = "0"; //امتیزا کل

            MasterData?.Clear();

        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as AZAE;
                HEADER_ID = HEADER.ID;
                HES.SelectedValue = HEADER.HES.Trim(); HES.Items.Refresh();
                TOPETEB.Text = HEADER.TOPETEB.ToStringNullSafe(); // سقف اعتبار ریالی
                TOPMEGH.Text = HEADER.TOPMEGH.ToStringNullSafe(); // سقف اعتبار مقداری
                TOPASN.IsChecked = HEADER.TOPASN;
                ENDIS.IsChecked = HEADER.ENDIS;
                JAMZARIB.Text = HEADER.JAMZARIB.ToStringNullSafe(); //جمع ضرائب
                gradeid.SelectedValue = HEADER.gradeid; gradeid.Items.Refresh();
                EMTIAZ.Text = HEADER.EMTIAZ.ToStringNullSafe(); //امتیزا کل

                ReGetData(); //Load DataGrid's data

                if (HEADER_ID != null)
                {
                    Form_Current();
                }
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
                var itemToRemove = RecordsData.View.CurrentItem as AZAE;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<AZAE>;
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

        public bool GRADE_CUST_TAB_SUB_IsInside_Focused { get; private set; } = false;
        public bool GRADE_GRP_SUB_IsInside_Focused { get; private set; } = false;

        UniversControl universControl = new UniversControl();

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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                if (_ican is true) // Is Enable and ReadOnly = False
                {
                    TOPETEB.IsReadOnly = false; //سقف اعتباری ریالی
                    TOPMEGH.IsReadOnly = false; //سقف اعتبار مقداری

                    HES.IsEnabled = true;
                    HES_CODE.IsEnabled = true;
                    TOPASN.IsEnabled = true; //تیک اسناد پاس نشده
                    ENDIS.IsEnabled = true; //اعتبار سنجی کنترل شود
                    JAMZARIB.IsEnabled = true; //جمع ضرائب
                    gradeid.IsEnabled = true; //گرید
                    EMTIAZ.IsEnabled = true; //امتیاز کل
                    ADDGRADE.IsEnabled = true; //اضافه کردن رده بندی
                    Command14.IsEnabled = true; //چاپ
                    SAVE_AVZ.IsEnabled = true; //ذخیره
                    DELETE_AVZ.IsEnabled = true;//حذف

                    if (HEADER_ID != null)
                    {
                        GRADE_CUST_TAB_SUB.IsEnabled = true;
                    }
                }
                else
                {
                    TOPETEB.IsReadOnly = true; //سقف اعتباری ریالی
                    TOPMEGH.IsReadOnly = true; //سقف اعتبار مقداری

                    HES.IsEnabled = false;
                    HES_CODE.IsEnabled = false;
                    TOPASN.IsEnabled = false; //تیک اسناد پاس نشده
                    ENDIS.IsEnabled = false; //اعتبار سنجی کنترل شود
                    JAMZARIB.IsEnabled = false; //جمع ضرائب
                    gradeid.IsEnabled = false; //گرید
                    EMTIAZ.IsEnabled = false; //امتیاز کل
                    ADDGRADE.IsEnabled = false; //اضافه کردن رده بندی
                    Command14.IsEnabled = false; //چاپ
                    SAVE_AVZ.IsEnabled = false; //ذخیره
                    DELETE_AVZ.IsEnabled = false;//حذف

                    GRADE_CUST_TAB_SUB.IsEnabled = false;
                }
            }
        }

        public Search_Model FROM_SEARCH { get; set; }
        public Visual I_AM_AZAE { get; set; }

        private int _GCNAME_index;
        public int GCNAME_INDEX_COL
        {
            get
            {
                if (GRADE_CUST_TAB_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = GRADE_CUST_TAB_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "GCNAME")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _GCNAME_index = 0;
                    }
                    else
                    {
                        _GCNAME_index = (int)defaultcolumnindex;
                    }
                }
                return _GCNAME_index;
            }
        }
        private bool IsElementWithinAnyDataGrid(DependencyObject element)
        {
            while (element != null)
            {
                if (element is DataGrid)
                {
                    return true; // Found a DataGrid in the ancestry of the focused element
                }
                // Move up the visual tree
                element = VisualTreeHelper.GetParent(element);
            }
            return false; // No DataGrid found in the ancestry of the focused element
        }
        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (!GRADE_CUST_TAB_SUB_IsInside_Focused && !GRADE_GRP_SUB_IsInside_Focused) { }

            var focusedElement = Keyboard.FocusedElement as DependencyObject;
            if (!IsElementWithinAnyDataGrid(focusedElement))
            {
                UIElement uie = e.OriginalSource as UIElement;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    CL_LMethods.SendKey_US(Key.Tab);
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_AZAE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            if (!CL_HESABDARI.LETSGO("TARIF"))
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            #region Form_Open
            if (Strings.Mid(Baseknow.tindata, 20, 7) == "CORRECT")
            {
                //this.GRADE_CUST_TAB_SUB.Visible = true;
                GRADE_CUST_TAB_SUB.Visibility = Visibility.Visible;
            }
            else
            {
                GRADE_CUST_TAB_SUB.Visibility = Visibility.Hidden;
            }
            #endregion

            Form_Current();


            #region TMP_TEST_SHOULD_REMOVE
            //HES.SelectedValue = "115-1-1003"; HES.Items.Refresh();
            //GRADE_CUST_TAB_SUB.IsEnabled = true;
            //AllowEdits = true;
            #endregion

            HES.Focus();
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!long.TryParse(TOPETEB.Text, out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار سقف اعتباری ریالی مجاز نیست" });
            }
            if (!long.TryParse(TOPMEGH.Text, out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار سقف اعتبار مقداری مجاز نیست" });
            }
            if (!Single.TryParse(EMTIAZ.Text, out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار امتیاز مجاز نیست" });
            }

            if (string.IsNullOrEmpty(HES.SelectedValue.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مشتری نمیتواند خالی باشد!" });
            }


            if (ErrosMessages.Count > 0)
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
        public void Form_Current()
        {
            if (!string.IsNullOrEmpty(HES.SelectedValue.ToStringNullSafe()))
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
            }
            else
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
            }
        }
        public ObservableCollection<GSCALE> GSCALE_DATA { get; set; } = new ObservableCollection<GSCALE>();
        public List<GSCADTL> GSCADTL_DATA { get; set; } = new List<GSCADTL>(); //Bind
        public bool ChangeIsHappend { get; private set; } = false;

        private void FILL_ALL_COMBOBOXES()
        {
            //مشتری
            HES.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT  hes, NAME FROM dbo.CUST_HESAB").ToList();
            HES_CODE.ItemsSource = HES.ItemsSource;
            HES.SelectedIndex = -1; HES.Items.Refresh();

            //گرید
            gradeid.ItemsSource = dbms.DoGetDataSQL<GRADE_RANGE>("SELECT GID, GNAME FROM GRADE_RANGE").ToList();

            //مقدار
            var data2 = dbms.DoGetDataSQL<GSCADTL>("SELECT GSCADTCOD, GSCANAME, GSCAGRADE, GSCAFROM, GSCATO, GSCACOD, CRT, UID FROM dbo.GSCADTL").ToList(); //GCVALUESCAL 
            foreach (var item in data2)
            {
                GSCADTL_DATA.Add(item);
            }

            //ارزش
            var data = dbms.DoGetDataSQL<GSCALE>("SELECT GSCACOD, GSCANAME FROM dbo.GSCALE").ToList();
            GSCALE_DATA?.Clear();
            foreach (var item in data)
            {
                GSCALE_DATA.Add(item);
            }

        }
        private void Command14_Click(object sender, RoutedEventArgs e)
        {
            if (HES.SelectedValue is null)
            {
                
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.TAARIF.GRADE_REP.mrt");
                report.Load(pathreport);

                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                report["HESPARAM"] = HES.SelectedValue.ToStringNullSafe();
                ((StiSqlSource)report.Dictionary.DataSources["GV"]).CommandTimeout = 300;

                //report.Render(false);
                //report.Show();

                new Rpts.WINRPT(report, "مشخصات مشتریان - اعضاء  و اعتبارات").Show();
            }
        }
        private void HES_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            TextBox CUTSNO_TEX = (TextBox)HES.Template.FindName("PART_EditableTextBox", HES);

            if (HES.SelectedValue is not null)
            {
                if ((HES.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("AZAE", I_AM_AZAE);
                CMBSearch.ShowDialog();
                if (HES.SelectedValue is null)
                {
                    return;
                }
            }
            else if (Information.IsNumeric(CUTSNO_TEX.Text))
            {
                try
                {
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " and NUMBER = 1 and TNUMBER = " + CUTSNO_TEX.Text).ToList();
                    if (rst.Count == 1)
                    {
                        var _data_hes = rst.FirstOrDefault()?.n_kol + "-" + rst.FirstOrDefault()?.NUMBER + "-" + rst.FirstOrDefault()?.tNUMBER;

                        HES.Items.Refresh();
                        HES.SelectedValue = null;
                        HES_CODE.SelectedValue = _data_hes;
                        //CUST_NO_AfterUpdate();
                    }
                    else
                    {
                        HES.SelectedValue = null;
                        HES.Text = null;
                        HES.Items.Refresh();
                        return;
                    }
                }
                catch (Exception) { }
            }

        }
        private void HES_CODE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES_CODE.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (HES_CODE.SelectedValue is null)
            {
                HES.SelectedValue = null; HES.Items.Refresh();
            }
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(HES.SelectedValue.ToStringNullSafe()))
            {
                var dt = DateTime.Now;
                CL_HESABDARI.TR("AZAE", "(HES = '" + HES.SelectedValue + "' )", dt, 1);
                CL_HESABDARI.TR("GRADE_CUST_TAB", "(GCCUST_HES = '" + HES.SelectedValue + "' )", dt, 1);
                if (GRADE_CUST_TAB_SUB.Visibility == Visibility.Visible && GRADE_CUST_TAB_SUB.Items.Count > 0)
                {
                    foreach (var MasterRow in MasterData)
                    {
                        CL_HESABDARI.TR("GRADE_CUST_GRP", "(GCTABID = " + MasterRow.GCTABID + " )", DateTime.Now, 1);
                    }

                }
                this.AllowDeletions = true;
                this.AllowEdits = true;
            }
        }
        //Header Save
        private void SAVE_AVZ_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            long? tmpid = null;
            try
            {
                if (HEADER_ID is null)
                {
                    tmpid = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.AZAE(HES, TOPETEB, TOPASN, ENDIS, TOPMEGH, JAMZARIB, EMTIAZ, gradeid, DTTIM)
                                                        VALUES('{HES.SelectedValue}',
                                                        {TOPETEB.Text},
                                                        {Convert.ToByte(TOPASN.IsChecked)},
                                                        {Convert.ToByte(ENDIS.IsChecked)},
                                                        {TOPMEGH.Text},
                                                        {JAMZARIB.Text},
                                                        {EMTIAZ.Text},
                                                        {gradeid.SelectedValue},
                                                        GETDATE() )
                                                        ").FirstOrDefault();
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.AZAE
                                         SET HES = '{HES.SelectedValue}', TOPETEB = {TOPETEB.Text}, 
                                         TOPASN = {Convert.ToByte(TOPASN.IsChecked)}, ENDIS = {Convert.ToByte(ENDIS.IsChecked)}, 
                                         TOPMEGH = {TOPMEGH.Text}, JAMZARIB = {JAMZARIB.Text}, EMTIAZ = {EMTIAZ.Text}, gradeid = {gradeid.SelectedValue}
                                         WHERE ID = {HEADER_ID} ");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "برای این مشتری قبلا اعتبار ثبت شده  (تکراری)!").ShowDialog();
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

            if (tmpid != null)
            {
                HEADER_ID = (long)tmpid;
            }

            GRADE_CUST_TAB_SUB.IsEnabled = true;

            universControl.PopNotifyShow("اطلاعات ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void DBXPANDER_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            // Get the button that was clicked
            var button = (Button)sender;
            // Find the clicked row
            DataGridRow dataGridRow = CL_LMethods.FindParent<DataGridRow>((DependencyObject)button);
            //var dataGridRow = (DataGridRow)GRADE_CUST_TAB_SUB.ItemContainerGenerator.ContainerFromItem(button.DataContext);
            if (dataGridRow != null)
            {
                // Toggle the visibility of the RowDetails
                if (dataGridRow.DetailsVisibility == Visibility.Collapsed)
                {
                    dataGridRow.DetailsVisibility = Visibility.Visible;
                    var packIcon = (PackIcon)button.Content;
                    packIcon.Kind = PackIconKind.Minus;
                }
                else
                {
                    dataGridRow.DetailsVisibility = Visibility.Collapsed;
                    var packIcon = (PackIcon)button.Content;
                    packIcon.Kind = PackIconKind.Plus;
                }
            }
        }

        //DATA GRID HERE----------------------------------------------------------------------------------------------------------------------------
        private void GRADE_CUST_TAB_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var focusedControl = Keyboard.FocusedElement as FrameworkElement;
            if (focusedControl != null)
            {
                if (focusedControl.DataContext is GRADE_CUST_GRP)
                {
                    GRADE_GRP_SUB_IsInside_Focused = true;

                    GRADE_CUST_TAB_SUB_IsInside_Focused = false;
                }
                else if (focusedControl.DataContext is GRADE_CUST_TAB)
                {
                    GRADE_CUST_TAB_SUB_IsInside_Focused = true;

                    GRADE_GRP_SUB_IsInside_Focused = false;
                }
                else
                {
                    var dataGrid = sender as DataGrid;
                    if (dataGrid != null)
                    {
                        // Using reflection to get the "Name" property of the DataGrid
                        string dataGridName = dataGrid.GetType().GetProperty("Name").GetValue(dataGrid).ToString();
                        // Now, check the name to determine how to handle the event
                        if (dataGridName == "GRADE_GRP_SUB")
                        {
                            GRADE_GRP_SUB_IsInside_Focused = true;
                            GRADE_CUST_TAB_SUB_IsInside_Focused = false;
                        }
                        else if (dataGridName == "GRADE_CUST_TAB_SUB")
                        {
                            GRADE_CUST_TAB_SUB_IsInside_Focused = true;
                            GRADE_GRP_SUB_IsInside_Focused = false;
                        }
                    }
                    else
                    {
                        GRADE_GRP_SUB_IsInside_Focused = false;
                        GRADE_CUST_TAB_SUB_IsInside_Focused = false;
                    }
                }
            }

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None) //EnterTab and ComeDown On NewRow
            {
                if (GRADE_CUST_TAB_SUB_IsInside_Focused)
                {
                    DataGrid DG = GRADE_CUST_TAB_SUB;
                    e.Handled = true;

                    if (DG.CurrentColumn != null)
                    {
                        int DefaultColumnIndex = CL_LMethods.GetLastColumn(DG).DisplayIndex;
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

                                DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[GCNAME_INDEX_COL]);

                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    DG.BeginEdit();
                                }), DispatcherPriority.Background);

                                return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                            }
                        }
                    }
                }
                if (!GRADE_GRP_SUB_IsInside_Focused) //فوکوس توی دیتاگرید داخلی نباشه!
                {
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }


            if (e.Key is Key.Delete && GRADE_CUST_TAB_SUB_IsInside_Focused)
            {
                //var DG = sender as DataGrid;

                if (GRADE_CUST_TAB_SUB.Items.Count > 0 && GRADE_CUST_TAB_SUB.SelectedItem != null)
                {
                    if (!(GRADE_CUST_TAB_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            for (int i = 0; i < GRADE_CUST_TAB_SUB.SelectedItems.Count; i++)
                            {
                                var item = GRADE_CUST_TAB_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("GCTABID").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _id = item.GetType().GetProperty("GCTABID").GetValue(item);
                                        var _GCNAME_ = item.GetType().GetProperty("GCNAME").GetValue(item);

                                        var InnerRows = item.GetType().GetProperty("DETAILROWDATA").GetValue(item) as IEnumerable<GRADE_CUST_GRP>;
                                        if (InnerRows.Count() > 0)
                                        {
                                            foreach (var _row_ in InnerRows)
                                            {
                                                try
                                                {
                                                    dbms.DoExecuteSQL($@"DELETE FROM dbo.GRADE_CUST_GRP WHERE GCGRPID = {_row_.GCGRPID}");
                                                }
                                                catch (SqlException ex)
                                                {
                                                    if (ex.Number == 547)
                                                    {
                                                        e.Handled = true;

                                                        ErrosMessages.Add(new MsgModel { MessageText_U = $" {_GCNAME_} + : دارای گردش است  " });
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


                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.GRADE_CUST_TAB WHERE GCTABID = {_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این نام گروه : {_GCNAME_} گردش دارد و نمیتواند حذف کرد" });
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
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }
                    }
                }
            }

        }
        private void GRADE_CUST_TAB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            GRADE_CUST_TAB_SUB.Dispatcher.InvokeAsync(() =>
            {
                GRADE_CUST_TAB_SUB.CellEditEnding -= GRADE_CUST_TAB_SUB_CellEditEnding;
                GRADE_CUST_TAB_SUB.RowEditEnding -= GRADE_CUST_TAB_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    GRADE_CUST_TAB_SUB.CancelEdit();
                }
                else
                {
                    GRADE_CUST_TAB_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                GRADE_CUST_TAB_SUB.RowEditEnding += GRADE_CUST_TAB_SUB_RowEditEnding;
                GRADE_CUST_TAB_SUB.CellEditEnding += GRADE_CUST_TAB_SUB_CellEditEnding;
            });
        }
        private bool GRADE_CUST_TAB_IsValid(GRADE_CUST_TAB Row, bool _DisplayErrors_ = true)
        {
            var errors = (from object i in GRADE_CUST_TAB_SUB.ItemsSource
                          let c = GRADE_CUST_TAB_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                if (_DisplayErrors_)
                {
                    universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                }
                return false;
            }

            //نام گروه , ضریب طبقه , تاریخ ثبت , نام کاربر
            List<MsgModel> ErrorMessages = new List<MsgModel>();

            if (!HeaderIsValid())
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "اطلاعات سربرگ درست نیست!" });
            }

            if (string.IsNullOrEmpty(Row?.GCNAME))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "نام گروه نمیتواند خالی باشد" });
            }
            else if (Row?.GCNAME.Length > 149)
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "طول نام گروه غیر مجاز است" });
            }

            if (string.IsNullOrEmpty(Row.GCZARIB?.ToStringNullSafe()))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "ضریب طبقه نمیتواند خالی باشد" });
            }
            else if (!double.TryParse(Row.GCZARIB?.ToStringNullSafe(), out _))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "ضریب طبقه غیر مجاز وارد شده" });
            }

            if (ErrorMessages.Any())
            {
                if (_DisplayErrors_)
                {
                    ErrorMessages = ErrorMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrorMessages).ShowDialog();
                }
                GRADE_CUST_TAB_CANCEL_EDIT();
                return false;
            }

            return true;
        }
        //Saving Master DataGrid:
        private void GRADE_CUST_TAB_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as GRADE_CUST_TAB;

            if (e.Row.Item == null)
            {
                return;
            }

            if (!GRADE_CUST_TAB_IsValid(ROW))
            {
                return;
            }

            int? id = null;
            try
            {
                if (ROW.GCTABID is null) //INSERT
                {
                    var MAXID = dbms.DoGetDataSQL<int?>($"SELECT Max(GCTABID) + 1  AS MIDD FROM GRADE_CUST_TAB").FirstOrDefault();
                    if (MAXID is null || MAXID == 0)
                    {
                        MAXID = 1;
                    }

                    id = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.GRADE_CUST_TAB(GCTABID,GCCUST_HES, GCNAME, GCZARIB, GCDATE, USERNAME)
                                                    OUTPUT INSERTED.GCTABID
                                                    VALUES({MAXID},
                                                    N'{HES.SelectedValue}' ,
                                                    N'{ROW.GCNAME}',
                                                    {ROW.GCZARIB},
                                                    {ROW.GCDATE},
                                                    N'{ROW.USERNAME}')").FirstOrDefault();
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL(@$"UPDATE dbo.GRADE_CUST_TAB SET GCCUST_HES = N'{HES.SelectedValue}' , GCNAME = N'{ROW.GCNAME}', GCZARIB = {ROW.GCZARIB} WHERE GCTABID = {ROW.GCTABID} ");
                }

                CL_HESABDARI.UpAZAE(HES.SelectedValue.ToString());
            }
            catch (SqlException ex)
            {
                GRADE_CUST_TAB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این نام گروه تکراری است!").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog();
                }

                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            if (id != null)
            {
                ROW.GCTABID = id;
            }

        }
        private void GRADE_CUST_TAB_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }
        //Details
        private void GRADE_GRP_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            string THECOL = e.Column.Header.ToStringNullSafe();

            int DefVale = 0;
            var _Arzesh_ = ((GRADE_CUST_GRP)e.Row.Item).GCVALUE; //بر اساس داده کمبوباکس ارزش

            ComboBox CMB = CL_LMethods.FindVisualChild<ComboBox>(e.EditingElement);

            if (THECOL == "مقدار")
            {
                if (CMB is ComboBox)
                {
                    DefVale = Convert.ToInt32(CMB.SelectedValue);

                    if (_Arzesh_ is not null)
                    {
                        CMB.ItemsSource = GSCADTL_DATA.Where(i => i.GSCACOD == _Arzesh_).ToList(); //GCVALUESCAL_Enter : GCVALUESCAL.RowSource = "SELECT GSCADTCOD, GSCANAME FROM GSCADTL WHERE GSCACOD = " + this.GCVALUE;

                        if (DefVale != 0)
                        {
                            CMB.SelectedValue = DefVale;
                        }
                    }
                }
            }
            else
            {
                if (CMB is not null)
                {
                    CMB.ItemsSource = GSCADTL_DATA; //GCVALUESCAL_Exit : GCVALUESCAL.RowSource = "SELECT GSCADTCOD, GSCANAME FROM GSCADTL ";

                    // dbms.DoGetDataSQL<GSCADTL>("SELECT GSCADTCOD, GSCANAME FROM GSCADTL").ToList();

                    if (DefVale != 0)
                    {
                        CMB.SelectedValue = DefVale;
                    }
                }
            }
            if (CMB is not null)
            {
                CMB.Items.Refresh();
            }

        }
        private bool GRADE_CUST_GRP_IsValid(GRADE_CUST_GRP Row, bool _DisplayErrors_ = true)
        {
            List<MsgModel> ErrorMessages = new List<MsgModel>();

            if (!HeaderIsValid())
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "اطلاعات سربرگ درست نیست!" });
            }

            // Validate GCGRPNAME
            if (string.IsNullOrEmpty(Row?.GCGRPNAME))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "عنوان سنجش نمیتواند خالی باشد" });
            }
            else if (Row.GCGRPNAME.Length > 149)
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "طول عنوان سنجش بیش از حد مجاز است" });
            }

            if (string.IsNullOrEmpty(Row?.GCGRPZARIB.ToStringNullSafe()))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "مقدار ضریب نمی تواند خالی باشد" });
            }
            else if (!double.TryParse(Row.GCGRPZARIB.ToString(), out _))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "مقدار ضریب غیر مجاز است" });
            }

            if (string.IsNullOrEmpty(Row?.GCGRPGRADE.ToStringNullSafe()))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "مقدار امتیاز نمی تواند خالی باشد" });
            }
            else if (!double.TryParse(Row.GCGRPGRADE.ToString(), out _))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "مقدار امتیاز غیر مجاز است" });
            }

            // Validate GCVALUESCAL
            if (Row.GCVALUESCAL.HasValue && !int.TryParse(Row.GCVALUESCAL.Value.ToString(), out _))
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "فیلد مقدار مجاز نیست" });
            }


            if (ErrorMessages.Any())
            {
                if (_DisplayErrors_)
                {
                    ErrorMessages = ErrorMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrorMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }
        private void GRADE_GRP_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None) //EnterTab and ComeDown On NewRow
            {
                DataGrid DG = sender as DataGrid;
                if (DG != null)
                {
                    e.Handled = true;

                    if (GRADE_GRP_SUB_IsInside_Focused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int DefaultColumnIndex = CL_LMethods.GetLastColumn(DG).DisplayIndex;
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == 4; //مدارک
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    int defaultcolumnindex = (int)(DG.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "GCGRPNAME")?.DisplayIndex);

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[defaultcolumnindex]);

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


            if (e.Key is Key.Delete && GRADE_GRP_SUB_IsInside_Focused)
            {
                if (e.Key is Key.Delete)
                {
                    var DG = sender as DataGrid;

                    if (DG.Items.Count > 0 && DG.SelectedItem != null)
                    {
                        //REOMVE_ATTACHMENT_CELL
                        var currentCell = DG.CurrentCell;
                        if (currentCell.Column.Header.ToString() == "مدارک")
                        {
                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف فایل ضمیمه هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {
                                var currentItem = currentCell.Item as GRADE_CUST_GRP;
                                if (currentItem != null)
                                {
                                    currentItem.GCPS = null;
                                    DG.BeginEdit();
                                    DG.CommitEdit(); // Commit the row edit
                                    DG.CommitEdit(DataGridEditingUnit.Row, true);
                                    DG.Items.Refresh();

                                    // Prevent the default delete action
                                    e.Handled = true;
                                    return; // Let's get out
                                }
                            }

                        }
                        //endregion

                        if (!(DG.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                            if (msgwin.DialogResult == true)
                            {
                                for (int i = 0; i < DG.SelectedItems.Count; i++)
                                {
                                    var item = DG.SelectedItems[i];

                                    if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                    {
                                        if (item.GetType().GetProperty("GCGRPID").GetValue(item) is null)
                                        {
                                        }
                                        else
                                        {
                                            var _id_ = item.GetType().GetProperty("GCGRPID").GetValue(item);
                                            var _GCNAME_ = item.GetType().GetProperty("GCNAME").GetValue(item);

                                            try
                                            {
                                                IsDeletedSomething = true;

                                                dbms.DoExecuteSQL($@"DELETE FROM dbo.GRADE_CUST_GRP WHERE GCGRPID = {_id_}");
                                            }
                                            catch (SqlException ex)
                                            {
                                                if (ex.Number == 547)
                                                {
                                                    e.Handled = true;

                                                    ErrosMessages.Add(new MsgModel { MessageText_U = $" {_GCNAME_} دارای گردش است : " });
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
                                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                                new MsgListwin(false, ErrosMessages).ShowDialog();

                                return;
                            }
                        }
                    }
                }
            }
        }
        //
        private void GRADE_GRP_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var ROW = e.Row.Item as GRADE_CUST_GRP;

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            if (!GRADE_CUST_GRP_IsValid(ROW))
            {
                GRADE_GRP_SUB_CANCEL_EDIT(sender);
                return;
            }

            int? id = null;
            try
            {
                if (ROW.GCGRPID is null) //INSERT
                {
                    var MAXID = dbms.DoGetDataSQL<int?>($"SELECT Max(GCGRPID)+1 AS MIDD FROM GRADE_CUST_GRP").FirstOrDefault();
                    if (MAXID is null || MAXID == 0)
                    {
                        MAXID = 1;
                    }

                    var ParentRow = GRADE_CUST_TAB_SUB.SelectedItem as GRADE_CUST_TAB;
                    var qre = @$"INSERT INTO dbo.GRADE_CUST_GRP(GCGRPID, GCGRPNAME, GCGRPZARIB, GCGRPGRADE, GCTABID, GCVALUESCAL, GCPS)
                                                    OUTPUT INSERTED.GCGRPID
                                                    VALUES({MAXID},
                                                    N'{ROW.GCGRPNAME}' ,
                                                    {ROW.GCGRPZARIB} ,
                                                    {ROW.GCGRPGRADE} ,
                                                    {ParentRow.GCTABID} ,
                                                    {(ROW.GCVALUESCAL is null ? "NULL" : ROW.GCVALUESCAL)} ,
                                                    @img)
                                                    ";
                    if (ROW?.GCPS is null)
                    {
                        id = dbms.DoGetDataSQL<int?>(qre, new { img = (byte[])null }).FirstOrDefault();
                    }
                    else
                    {
                        id = dbms.DoGetDataSQL<int?>(qre, new { img = ROW.GCPS }).FirstOrDefault();
                    }
                }
                else //UPDATE
                {
                    var qre = @$" UPDATE dbo.GRADE_CUST_GRP
                                          SET GCGRPNAME = N'{ROW.GCGRPNAME}', 
                                          GCGRPZARIB = {ROW.GCGRPZARIB}, GCGRPGRADE = {ROW.GCGRPGRADE}, 
                                          GCVALUESCAL = {(ROW.GCVALUESCAL is null ? "NULL" : ROW.GCVALUESCAL)},
                                          GCPS = @img
                                          WHERE GCGRPID = {ROW.GCGRPID} ";
                    if (ROW?.GCPS is null)
                    {
                        //dbms.DoExecuteSQL(qre, new { img = "NULL" });
                        dbms.DoExecuteSQL(qre, new { img = (byte[])null });
                    }
                    else
                    {
                        dbms.DoExecuteSQL(qre, new { img = ROW.GCPS });
                    }
                }

                CL_HESABDARI.UpAZAE(HES.SelectedValue.ToString());
            }
            catch (SqlException ex)
            {
                GRADE_CUST_TAB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این نام گروه تکراری است!").ShowDialog();
                }

                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            if (id != null) // اگر آیدی برای ذخیره جدید گرفته بیا پرش کن واگر نه که آپدیت بوده و رد شد
            {
                ROW.GCGRPID = id;
            }
        }
        private void GRADE_GRP_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            #region GET_VALUE_FOR_DATAGRID_PROPERTIES
            DataGridRow row1 = e.Row;
            var CURRENT_ROW_INDEX = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            var CURRENT_ITMES_ROW = e.Row.Item as GRADE_CUST_GRP;
            string ENTERED_VALUE_ROW = "";
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                ENTERED_VALUE_ROW = ((TextBox)e.EditingElement).Text;
            }
            if (!(e.EditingElement is null) && e.EditingElement is TextBlock)
            {
                ENTERED_VALUE_ROW = ((TextBlock)e.EditingElement).Text;
            }
            if (!(e.EditingElement is null))
            {
                var Comboval = e.EditingElement as ComboBox;
                if (!ReferenceEquals(Comboval, null))
                {
                    ENTERED_VALUE_ROW = (string)Comboval.SelectedValue;
                }
                else
                {
                    var comboBox = CL_LMethods.FindParent<ComboBox>(e.EditingElement);
                    if (comboBox is not null)
                    {
                        ENTERED_VALUE_ROW = (comboBox.SelectedValue).ToStringNullSafe();
                    }
                }
            }
            #endregion

            if (e.Column.Header.ToString() == "ارزش")
            {
            }
        }
        private void GRADE_GRP_SUB_CANCEL_EDIT(object sender)
        {
            var DG = sender as DataGrid;
            DG.Dispatcher.InvokeAsync(() =>
            {
                DG.CellEditEnding -= GRADE_GRP_SUB_CellEditEnding;
                DG.RowEditEnding -= GRADE_GRP_SUB_RowEditEnding;

                DG.CancelEdit();

                DG.RowEditEnding += GRADE_GRP_SUB_RowEditEnding;
                DG.CellEditEnding += GRADE_GRP_SUB_CellEditEnding;
            });
        }

        private void MADAREK_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag != null)) return;

            //IDD
            long _ID_ = Convert.ToInt64((btn.Tag as GRADE_CUST_GRP)?.GCGRPID);

            var CURRENT_CUST_GRP = (btn.Tag as GRADE_CUST_GRP);
            //اگر آیدی دارد یعنی رکرود قبلا ثبت شده
            if (_ID_ > 0)
            {
                ////ببین آیا اصلا ضمیمه ای داره ؟
                //CURRENT_CUST_GRP.GCPS = dbms.DoGetDataSQL<byte[]>($"SELECT TOP(1) GCPS FROM dbo.GRADE_CUST_GRP WHERE GCGRPID = {_ID_}").FirstOrDefault();
                if (!(CURRENT_CUST_GRP.GCPS is null))
                {
                    try
                    {
                        //ساخت مسیر یکتا در تمپ برای دریافت فایل از سرور
                        string THE_PATH_FILE = CL_LMethods.GetTempFilePathWithExtension("");
                        //ببینم فایل مون چه فرمتی داره
                        var THE_MIMTYP = MimeTypeImagy.GetMimeType(CURRENT_CUST_GRP.GCPS, THE_PATH_FILE);

                        string THE_TYPEFILE = "";

                        #region COMENTEDPART
                        ////اگر دسترسی برای نوشتن فایل داریم برو انجام بده
                        //string uniqueFileName = Guid.NewGuid().ToString() + "TMP";
                        //string directoryPath = Path.GetTempPath(); // Get the temporary directory path
                        //string GOTFILE_PATH = Path.Combine(directoryPath, uniqueFileName);

                        //if (!Directory.Exists(GOTFILE_PATH))
                        //{
                        //    Directory.CreateDirectory(GOTFILE_PATH);
                        //}
                        ////آیا دسترسی دارم برای اینکه چیزی بنویسم ؟
                        //if (CL_LMethods.HasWritePermissionOnDir(GOTFILE_PATH))
                        //{
                        //    File.WriteAllBytes(GOTFILE_PATH, CURRENT_CUST_GRP.GCPS);

                        //    var MY_PROCESS1 = Process.Start("explorer.exe", THE_PATH_FILE);
                        //    //BINY_FILE = null;
                        //}
                        //else
                        //{
                        //    Debug.Print("No access permition to folder(directory)");
                        //    Msgwin msgwin = new Msgwin(false, "خطا در دریافت فایل , عدم دسترسی به فضای ذخیره سازی!");
                        //    msgwin.ShowDialog();
                        //    return;
                        //}
                        #endregion

                        if (true) //Just Format
                        {
                            if (THE_MIMTYP is "application/octet-stream")//اگر فرمت ناشناخته بود
                            {
                                THE_TYPEFILE = ExtensionTypeManage.GetExtensionFileType(CURRENT_CUST_GRP.GCPS);
                                var OLE_Removed_Bytes = ExtensionTypeManage.MimeTypeManagement.GetNormalizeBytes(CURRENT_CUST_GRP.GCPS);

                                THE_PATH_FILE = THE_PATH_FILE + THE_TYPEFILE;  ////C:\Users\ADMINI~1\AppData\Local\Temp\csharp.png
                                                                               //نوشتن فایل و باز کردن
                                bool FileWroteSuccesfully = CL_LMethods.WriteTheFile(THE_PATH_FILE, OLE_Removed_Bytes);
                                if (!FileWroteSuccesfully)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                THE_TYPEFILE = MimeTypeMap.GetExtension(THE_MIMTYP);

                                THE_PATH_FILE = THE_PATH_FILE + THE_TYPEFILE;   ////C:\Users\ADMINI~1\AppData\Local\Temp\csharp.png
                                //نوشتن فایل و باز کردن
                                bool FileWroteSuccesfully = CL_LMethods.WriteTheFile(THE_PATH_FILE, CURRENT_CUST_GRP.GCPS);
                                if (!FileWroteSuccesfully)
                                {
                                    return;
                                }
                            }

                            Process? MY_PROCESS1 = null;
                            if (CL_LMethods.IsImageTYPE(THE_TYPEFILE)) // اگر عکس , عکس باز کن
                            {
                                try
                                {
                                    var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                                    MY_PROCESS1 = Process.Start(new ProcessStartInfo()
                                    {
                                        FileName = "rundll32.exe",
                                        Arguments = string.Format(
                                            "\"{0}{1}\", ImageView_Fullscreen {2}",
                                            Environment.Is64BitOperatingSystem ?
                                                path.Replace(" (x86)", "") : path,
                                            @"\Windows Photo Viewer\PhotoViewer.dll", THE_PATH_FILE),
                                        WindowStyle = ProcessWindowStyle.Maximized,
                                        UseShellExecute = false
                                    });
                                }
                                catch (Exception) { MY_PROCESS1 = Process.Start("explorer.exe", THE_PATH_FILE); }
                            }
                            else
                            {
                                MY_PROCESS1 = Process.Start("explorer.exe", THE_PATH_FILE);
                            }
                            if (!(MY_PROCESS1 is null)) //Finnaly After file has Open
                            {
                                MY_PROCESS1?.WaitForExit();
                                if (MY_PROCESS1.HasExited)
                                {
                                    //حالا میخوایم فایلی که بسته توی دیتامون آپدیتش کنیم که اگر تغیری داده اوکی بشه
                                    // چون ممکنه فرمت ورد یا اکسل رو باز کرده بشه !
                                    //IDEVNT
                                    if (File.Exists(THE_PATH_FILE)) //ناگهانی حذف نشده باشه
                                    {
                                        var TheNewUpdatedBianaryFile = CL_LMethods.ConvertFileToByte(THE_PATH_FILE);
                                        if (!(TheNewUpdatedBianaryFile is null))
                                        {
                                            try
                                            {
                                                string QRE_UPDATE = "";
                                                QRE_UPDATE = $@"UPDATE dbo.GRADE_CUST_GRP SET GCPS = @img WHERE GCGRPID = {_ID_}";

                                                if (TheNewUpdatedBianaryFile is null)
                                                {
                                                    dbms.DoExecuteSQL(QRE_UPDATE, new { img = "NULL" });
                                                }
                                                else
                                                {
                                                    dbms.DoExecuteSQL(QRE_UPDATE, new { img = CURRENT_CUST_GRP.GCPS });
                                                }

                                                try
                                                {
                                                    //و در نهایت حذف فایل داخل تمپ
                                                    File.Delete(THE_PATH_FILE);
                                                }
                                                catch { }
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exr)
                    {
                        Msgwin msgwin = new Msgwin(false, "خطا در انجام عملیات !");
                        msgwin.ShowDialog();
                        return;
                    }
                }
                else
                {
                    var dialog = new OpenFileDialog
                    {
                        CheckFileExists = true,
                        Multiselect = false,
                        //Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|Rar Archives (*.rar)|*.rar|Zip Archives (*.zip)|*.zip|PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx|Excel Sheets (*.xlsx)|*.xlsx"
                        Filter = "Allowed Files (*.jpg, *.png, *.rar, *.zip, *.pdf, *.docx, *.xlsx)|*.jpg;*.png;*.rar;*.zip;*.pdf;*.docx;*.xlsx"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        string[] allowedExtensions = { ".jpg", ".png", ".rar", ".zip", ".pdf", ".docx", ".xlsx" };
                        string fileExtension = Path.GetExtension(dialog.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            new Msgwin(false, "فایلی که انتخاب کرده اید مجاز نیست!").Show();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    CURRENT_CUST_GRP.GCPS = CL_LMethods.ConvertFileToByte(dialog.FileName);
                }
            }
            else // صرفا آماده کردن فایل و گذاشتن در حافظه برای ذخیره در صورت پایین اومدن از سطر
            {
                var dialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Multiselect = false,
                    //Filter = "Images (*.jpg,*.png)|*.jpg;*.png|All Files(*.*)|*.*"
                };
                if (dialog.ShowDialog() != true) { return; }

                CURRENT_CUST_GRP.GCPS = CL_LMethods.ConvertFileToByte(dialog.FileName);
            }
        }
        private void DELETE_AVZ_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_AVZ.Visibility == Visibility.Visible;
            if (!DELETE_AVZ.IsEnabled || !IsVisible) { return; }

            if (MasterData.Count > 0)
            {
                new Msgwin(false, "اطلاعات وابسته وجود دارد نمی توان آنرا حذف کرد , ابتدا سطر های پایین را حذف نموده و مجددا امتحان کنید").ShowDialog();
                return;
            }

            if (!string.IsNullOrEmpty(HES.SelectedValue.ToStringNullSafe()) && MasterData != null)
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف کامل این کالا هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("AZAE", "(HES = '" + HES.SelectedValue + "' )", dt, 1);
                    CL_HESABDARI.TR("GRADE_CUST_TAB", "(GCCUST_HES = '" + HES.SelectedValue + "' )", dt, 1);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "مشخصات مشتریان - اعضاء  و اعتبارات",
                            recordId: HEADER_ID.ToStringNullSafe(),
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    try
                    {
                        dbms.DoExecuteSQL($"DELETE FROM dbo.AZAE WHERE ID = {HEADER_ID}");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این اعتبار تعریف شده دارای گردش است و نمیتوان آنرا پاک کرد!"); msgwin.ShowDialog();
                        }
                        else
                        {
                            new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                    }

                    RefreshAfterDelete(); //Refresh the RecordSet to remove deleted item
                }
            }
        }
        private void TBN_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {

            }
        }
        private void ADDGRADE_Click(object sender, RoutedEventArgs e)
        {
            GRADE_FORMAT_WIN GFW = new GRADE_FORMAT_WIN();
            GFW.ShowDialog();
        }

        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NewItem);
            HES.Focus();
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            //NewRecord = false;
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
            //NewRecord = false;
            MoveReGetData(INavigator.Jahat.FirstItem);
        }
        private void SERVERRELOAD_Btn_Click(object sender, RoutedEventArgs e)
        {
            ReGetMasterData();
        }


    }
}
