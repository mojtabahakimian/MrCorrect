using Functions;
using Interfaces;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Wins.WinMenus.Taarif
{
    public partial class TAKHFIF_DEF_FORM : Window, INavigator
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
        public class COMBOITEM
        {
            public int CODE { get; set; }
            public string NAMES { get; set; }
        }
        public class COMBOITEM1
        {
            public int? CUST_COD { get; set; }
            public string? CUSTKNAME { get; set; }
        }
        #endregion
        public TAKHFIF_DEF_FORM()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public ObservableCollection<TAKHFIF_DEF_DTL> TAKHFIF_DEF_DTL_DATA { get; set; } = new ObservableCollection<TAKHFIF_DEF_DTL>();
        public ObservableCollection<CUSTKIND_TF> CUSTKIND_TF_DATA { get; set; } = new ObservableCollection<CUSTKIND_TF>();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();

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
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                TSHARH.IsReadOnly = !ican;

                TKIND.IsEnabled = ican;
                BTN_SAVE.IsEnabled = ican;

                TAKHFIF_DTL_SUB.IsEnabled = ican;
                CUSTKIND_TF_SUB.IsEnabled = ican;
            }
        }

        public bool TAKHFIF_DTL_SUB_IsFocused { get; private set; }
        public bool CUSTKIND_TF_SUB_IsFocused { get; private set; }

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NewRecord { get; set; }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            TSHARH.Focus();
        }
        private void ActivateDataGrids(bool can)
        {
            TAKHFIF_DTL_SUB.IsEnabled = can;
            CUSTKIND_TF_SUB.IsEnabled = can;
        }
        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<TAKHFIF_DEF>($"SELECT TID, TSHARH, TKIND, TCREA, USER_NAME, CRT, UID FROM dbo.TAKHFIF_DEF").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<TAKHFIF_DEF>().FirstOrDefault(x => x.TID.Equals(NUMBER_TO_OPEN));
                if (item != null)
                {
                    // Set the CurrentItem to the found item
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
                var HEADER = RecordsData.View.CurrentItem as TAKHFIF_DEF;
                var DBData = dbms.DoGetDataSQL<TAKHFIF_DEF>($"SELECT TID, TSHARH, TKIND, TCREA, USER_NAME, CRT, UID FROM dbo.TAKHFIF_DEF WHERE TID = {HEADER.TID}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(TAKHFIF_DEF).GetProperties();
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
                NewRecord = true;

                ClearFreshNew();

                Form_Current();

                ActivateDataGrids(true);
            }
            else
            {
                UiDataUpdate();
            }
        }
        public void ClearFreshNew()
        {
            TID.Text = null;
            TSHARH.Text = null;
            TAKHFIF_DEF_DTL_DATA.Clear();
            CUSTKIND_TF_DATA.Clear();
            TKIND.SelectedIndex = -1; TKIND.Items.Refresh();
            USER_NAME.Text = Baseknow.UUSER;
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as TAKHFIF_DEF;
                TID.Text = HEADER.TID.ToString();
                TSHARH.Text = HEADER.TSHARH.ToString();

                TKIND.SelectedValue = HEADER.TKIND; TKIND.Items.Refresh();
                USER_NAME.Text = HEADER.USER_NAME;

                ReGetData(); //Load DataGrid's data

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
                var itemToRemove = RecordsData.View.CurrentItem as TAKHFIF_DEF;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<TAKHFIF_DEF>;
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
            }
            else if (LastCurrentPosition + 1 <= ((System.Windows.Data.ListCollectionView)RecordsData.View).Count - 1)
            {
                MoveReGetData(INavigator.Jahat.CustomPosition, LastCurrentPosition + 1);
            }
            else
            {
                MoveReGetData(INavigator.Jahat.NewItem);
            }
        }
        public void RefreshAfterInsert()
        {
            var itemtoadd = dbms.DoGetDataSQL<TAKHFIF_DEF>($"SELECT TID, TSHARH, TKIND, TCREA, USER_NAME, CRT, UID FROM dbo.TAKHFIF_DEF WHERE TID = {TID.Text}").FirstOrDefault();

            var underlyingCollection = RecordsData.Source as List<TAKHFIF_DEF>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
            }
        }
        public void Form_Current()
        {
            if (!this.NewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.ESLAH.IsEnabled = true;
            }
            if (!this.NewRecord)
            {
                if (!CL_HESABDARI.LETSGO("TKHESL"))
                {
                    this.ESLAH.IsEnabled = false;
                }
                else
                {
                    this.ESLAH.IsEnabled = true;
                }
            }
            else
            {
                AllowEdits = true;
                this.ESLAH.IsEnabled = true;
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            CUSTKIND_COLUMN.ItemsSource = dbms.DoGetDataSQL<COMBOITEM1>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND ORDER BY CUSTKNAME").ToList();

            //شخصیت
            TKIND.ItemsSource = new List<COMBOITEM>
            {
                  new COMBOITEM { CODE = 1, NAMES = "براساس مبلغ پرداخت نقدي" },
                  new COMBOITEM { CODE = 2, NAMES = "براساس مبلغ خالص فروش" }
            };
        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TSHARH.Text))
            {
                new Msgwin(false, "شرح نمیتواند خالی باشد").ShowDialog();
                return;
            }
            if (TKIND.SelectedValue is null)
            {
                new Msgwin(false, "نوع تخفیف نمیتواند خالی باشد").ShowDialog();
                return;
            }

            bool IsInsert = false;
            if (string.IsNullOrEmpty(TID.Text) || TID.Text == "0") //INSERT
            {
                IsInsert = true;
            }
            else
            {
                IsInsert = false;
            }

            try
            {
                if (IsInsert) //INSERT
                {
                    int? TIDMAX = null;
                    var RST = dbms.DoGetDataSQL<int?>("SELECT Max(TAKHFIF_DEF.Tid) AS MaxOfid FROM TAKHFIF_DEF").FirstOrDefault();
                    if (RST is null || RST == 0)
                    {
                        TIDMAX = 1;
                    }
                    else
                    {
                        TIDMAX = Convert.ToInt32(RST) + 1;
                    }

                    var ID = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.TAKHFIF_DEF(TID, TSHARH, TKIND, TCREA, USER_NAME)
                                                        VALUES({TIDMAX},
                                                        N'{TSHARH.Text}',
                                                        {TKIND.SelectedValue} ,
                                                        GETDATE(),
                                                        N'{USER_NAME.Text}') ").FirstOrDefault();

                    if (ID != null)
                    {
                        TID.Text = ID.ToString();

                        RefreshAfterInsert();
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TAKHFIF_DEF SET TSHARH = N'{TSHARH.Text}', TKIND = {TKIND.SelectedValue} WHERE TID = {TID.Text}");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این سربرگ تخفیف تکراری است").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
        }
        public void ReGetData()
        {
            TAKHFIF_DEF_DTL_DATA?.Clear();
            var TAKHFIF_RST = dbms.DoGetDataSQL<TAKHFIF_DEF_DTL>($"SELECT ID,TID, AZMAB, TAMAB, TPERS, TMAB, CRT, UID FROM dbo.TAKHFIF_DEF_DTL WHERE TID = {TID.Text}").ToList();
            foreach (var item in TAKHFIF_RST)
            {
                TAKHFIF_DEF_DTL_DATA.Add(item);
            }

            CUSTKIND_TF_DATA?.Clear();
            var CUSTKIND_RST = dbms.DoGetDataSQL<CUSTKIND_TF>($"SELECT ID,CUST_COD, TID, CREA, ACTIV, CRT, UID FROM dbo.CUSTKIND_TF  WHERE TID = {TID.Text}").ToList();
            foreach (var item in CUSTKIND_RST)
            {
                CUSTKIND_TF_DATA.Add(item);
            }
        }
        private void TAKHFIF_DTL_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (TAKHFIF_DTL_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = TAKHFIF_DTL_SUB.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "AZMAB" || CURRENT_COLUMN_NAME is "TAMAB" || CURRENT_COLUMN_NAME is "TMAB")
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
                if (CURRENT_COLUMN_NAME is "AZMAB" || CURRENT_COLUMN_NAME is "TAMAB" || CURRENT_COLUMN_NAME is "TMAB")
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
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || string.IsNullOrEmpty(TID.Text) || NewRecord) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "تعریف تخفیفات پیشرفته تر",
                        recordId: TID.Text,
                        oldValue: null,
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (!string.IsNullOrEmpty(TID.Text))
                {
                    DateTime dt = DateTime.Now;
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_TAKHFIF_DEF (TID, TSHARH, TKIND, TCREA, USER_NAME, UP_TIME, UP_DATE, UP_USER_NAME, PC_NAME, IPADD)
                                     SELECT TID, TSHARH, TKIND, TCREA, USER_NAME, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}, '{Baseknow.UUSER}', '{CL_HESABDARI.CurrentMachineName()}', '{CL_HESABDARI.GETIPADD()}'
                                     FROM dbo.TAKHFIF_DEF
                                     WHERE TID = {TID.Text}");

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_TAKHFIF_DEF_DTL (TID, AZMAB, TAMAB, TPERS, TMAB, UP_TIME, UP_DATE)
                                     SELECT TID, AZMAB, TAMAB, TPERS, TMAB, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}
                                     FROM dbo.TAKHFIF_DEF_DTL
                                     WHERE TID = {TID.Text}");

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_CUSTKIND_TF (CUST_COD, TID, CREA, ACTIV, UP_TIME, UP_DATE)
                                     SELECT CUST_COD, TID, CREA, ACTIV, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}
                                     FROM dbo.CUSTKIND_TF
                                     WHERE TID = {TID.Text}");
                }

                if (TAKHFIF_DEF_DTL_DATA.Count > 0 || CUSTKIND_TF_DATA.Count > 0) //Any Sub Items ?
                {
                    if (TAKHFIF_DTL_SUB_IsFocused)
                    {
                        #region RIGHT
                        if (!(TAKHFIF_DTL_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            var editableCollectionView = TAKHFIF_DTL_SUB.Items as IEditableCollectionView;
                            if (editableCollectionView != null && editableCollectionView.IsEditingItem) { editableCollectionView.CommitEdit(); }

                            for (int i = 0; i < TAKHFIF_DTL_SUB.SelectedItems.Count; i++)
                            {
                                var item = TAKHFIF_DTL_SUB.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(TAKHFIF_DTL_SUB, item))
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                long? _ID_ = (long?)item.GetType().GetProperty("ID").GetValue(item);

                                if (_ID_ != null)
                                {
                                    try
                                    {
                                        IsDeletedSomething = true;

                                        dbms.DoExecuteSQL($@"DELETE FROM dbo.TAKHFIF_DEF_DTL WHERE ID = {_ID_}");
                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 547)
                                        {
                                            //e.Handled = true;
                                            var _AZMAB_ = item.GetType().GetProperty("AZMAB").GetValue(item);
                                            var _TPERS_ = item.GetType().GetProperty("TPERS").GetValue(item);
                                            ErrosMessages.Add(new MsgModel { MessageText_U = $"این \"از مبلغ\" : {_AZMAB_} با این \"درصد تخفیف\" : {_TPERS_} دارای گردش است و نمی توان حذف کرد!" });
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
                        #endregion
                    }
                    else if (CUSTKIND_TF_SUB_IsFocused)
                    {
                        #region LEFT
                        if (!(CUSTKIND_TF_SUB.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            var editableCollectionView = CUSTKIND_TF_SUB.Items as IEditableCollectionView;
                            if (editableCollectionView != null && editableCollectionView.IsEditingItem) { editableCollectionView.CommitEdit(); }

                            for (int i = 0; i < CUSTKIND_TF_SUB.SelectedItems.Count; i++)
                            {
                                var item = CUSTKIND_TF_SUB.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(CUSTKIND_TF_SUB, item))
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                long? _ID_ = (long?)item.GetType().GetProperty("ID").GetValue(item);

                                if (_ID_ != null)
                                {
                                    try
                                    {
                                        IsDeletedSomething = true;

                                        dbms.DoExecuteSQL($@"DELETE FROM dbo.CUSTKIND_TF WHERE ID = {_ID_}");
                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 547)
                                        {
                                            //e.Handled = true;
                                            var _CUST_COD_ = item.GetType().GetProperty("CUST_COD").GetValue(item);
                                            ErrosMessages.Add(new MsgModel { MessageText_U = $"این نوع مشتری {_CUST_COD_} دارای گردش است و نمیتوان آنرا حذف کرد !" });
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
                        #endregion
                    }
                }
                else
                {
                    dbms.DoExecuteSQL($"DELETE FROM dbo.TAKHFIF_DEF WHERE TID = {TID.Text}");
                    RefreshAfterDelete();                    
                }
            }
        }
        private void TAKHFIF_DTL_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TAKHFIF_DTL_SUB_IsFocused = false;
            }
            else
            {
                TAKHFIF_DTL_SUB_IsFocused = true;
            }
        }
        private void CUSTKIND_TF_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                CUSTKIND_TF_SUB_IsFocused = false;
            }
            else
            {
                CUSTKIND_TF_SUB_IsFocused = true;
            }
        }

        private bool BodyIsValid(TAKHFIF_DEF_DTL _row)
        {
            var ROW = _row;

            var errors = (from object i in TAKHFIF_DTL_SUB.ItemsSource
                          let c = TAKHFIF_DTL_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();


            if (string.IsNullOrEmpty(ROW?.AZMAB.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "از مبلغ نمیتواند خالی باشد" });
            }
            else if (ROW?.AZMAB < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "از مبلغ نمیتواند منفی باشد" });
            }
            else if (!double.TryParse(ROW?.AZMAB.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "از مبلغ وارد شده در محدوده مجاز نیست" });
            }

            if (string.IsNullOrEmpty(ROW?.TAMAB.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تا مبلغ نمیتواند خالی باشد" });
            }
            else if (ROW?.TAMAB < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تا مبلغ نمیتواند منفی باشد" });
            }
            else if (!double.TryParse(ROW?.TAMAB.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تا مبلغ وارد شده در محدوده مجاز نیست" });
            }

            if (string.IsNullOrEmpty(ROW?.TPERS.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف نمیتواند خالی باشد" });
            }
            else if (ROW?.TPERS < 0 || ROW?.TPERS > 100)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف در محدوده مجاز نیست" });
            }

            if (string.IsNullOrEmpty(ROW?.TMAB.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ ثابت نمیتواند خالی باشد" });
            }
            else if (ROW?.TMAB < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ ثابت نمیتواند منفی باشد" });
            }
            else if (!double.TryParse(ROW?.TMAB.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ ثابت وارد شده در محدوده مجاز نیست" });
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
        private bool BodyIsValid(CUSTKIND_TF _row)
        {
            var ROW = _row;

            var errors = (from object i in TAKHFIF_DTL_SUB.ItemsSource
                          let c = TAKHFIF_DTL_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();


            if (string.IsNullOrEmpty(ROW?.CUST_COD.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه مشتری نمیتواند خالی باشد" });
            }
            else if (!double.TryParse(ROW?.CUST_COD.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه مشتری وارد شده در محدوده مجاز نیست" });
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

        private void TAKHFIF_DTL_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }
        }
        private void CUSTKIND_TF_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }
        }
        private void TAKHFIF_DTL_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as TAKHFIF_DEF_DTL;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            long? ID = null;
            try
            {
                if (ROW?.ID == null) //INSERT
                {

                    ID = dbms.DoGetDataSQL<long?>(@$"INSERT INTO dbo.TAKHFIF_DEF_DTL(TID, AZMAB, TAMAB, TPERS, TMAB)
                                                     OUTPUT INSERTED.ID
                                                     VALUES({TID.Text},
                                                     {ROW.AZMAB} ,
                                                     {ROW.TAMAB} ,
                                                     {ROW.TPERS} ,
                                                     {ROW.TMAB})").FirstOrDefault();
                    if (ID != null)
                    {
                        ROW.ID = ID;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TAKHFIF_DEF_DTL SET AZMAB = {ROW.AZMAB}, TAMAB = {ROW.TAMAB}, TPERS = {ROW.TPERS}, TMAB = {ROW.TMAB} WHERE ID = {ROW.ID}");
                }
            }
            catch (SqlException ex)
            {
                TAKHFIF_DTL_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این تخفیف تکراری است!").ShowDialog();
                    return;
                }
            }
            catch (Exception ex)
            {
                TAKHFIF_DTL_SUB_CANCEL_EDIT();
                new Msgwin(false, "خطا در ذخیره تخفیفات").ShowDialog();
                return;
            }
        }
        private void CUSTKIND_TF_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as CUSTKIND_TF;
            if (!BodyIsValid(ROW))
            {
                return;
            }

            long? ID = null;
            try
            {
                if (ROW?.ID == null) //INSERT
                {
                    ID = dbms.DoGetDataSQL<long?>(@$"INSERT INTO dbo.CUSTKIND_TF(CUST_COD, TID, CREA, ACTIV)
                                                     OUTPUT INSERTED.ID
                                                     VALUES({ROW.CUST_COD},
                                                     {TID.Text} ,
                                                     GETDATE(),
                                                     {Convert.ToByte(ROW.ACTIV)}) ").FirstOrDefault();
                    if (ID != null)
                    {
                        ROW.ID = ID;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.CUSTKIND_TF SET CUST_COD = {ROW.CUST_COD}, ACTIV = {Convert.ToByte(ROW.ACTIV)} WHERE ID = {ROW.ID}");
                }
            }
            catch (SqlException ex)
            {
                CUSTKIND_TF_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این گروه مشتری تکراری است!").ShowDialog();
                    return;
                }
            }
            catch (Exception ex)
            {
                CUSTKIND_TF_SUB_CANCEL_EDIT();
                new Msgwin(false, "خطا در ذخیره گروه مشتریان").ShowDialog();
                return;
            }
        }

        private void TAKHFIF_DTL_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TAKHFIF_DTL_SUB.Dispatcher.InvokeAsync(() =>
            {
                TAKHFIF_DTL_SUB.CellEditEnding -= TAKHFIF_DTL_SUB_CellEditEnding;
                TAKHFIF_DTL_SUB.RowEditEnding -= TAKHFIF_DTL_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TAKHFIF_DTL_SUB.CancelEdit();
                }
                else
                {
                    TAKHFIF_DTL_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TAKHFIF_DTL_SUB.RowEditEnding += TAKHFIF_DTL_SUB_RowEditEnding;
                TAKHFIF_DTL_SUB.CellEditEnding += TAKHFIF_DTL_SUB_CellEditEnding;
            });
        }
        private void CUSTKIND_TF_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            CUSTKIND_TF_SUB.Dispatcher.InvokeAsync(() =>
            {
                CUSTKIND_TF_SUB.CellEditEnding -= CUSTKIND_TF_SUB_CellEditEnding;
                CUSTKIND_TF_SUB.RowEditEnding -= CUSTKIND_TF_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    CUSTKIND_TF_SUB.CancelEdit();
                }
                else
                {
                    CUSTKIND_TF_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                CUSTKIND_TF_SUB.RowEditEnding += CUSTKIND_TF_SUB_RowEditEnding;
                CUSTKIND_TF_SUB.CellEditEnding += CUSTKIND_TF_SUB_CellEditEnding;
            });
        }


        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NewItem);
            TSHARH.Focus();
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

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TID.Text))
            {
                DateTime dt = DateTime.Now;
                dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_TAKHFIF_DEF (TID, TSHARH, TKIND, TCREA, USER_NAME, UP_TIME, UP_DATE, UP_USER_NAME, PC_NAME, IPADD)
                                     SELECT TID, TSHARH, TKIND, TCREA, USER_NAME, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}, '{Baseknow.UUSER}', '{CL_HESABDARI.CurrentMachineName()}', '{CL_HESABDARI.GETIPADD()}'
                                     FROM dbo.TAKHFIF_DEF
                                     WHERE TID = {TID.Text}");

                dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_TAKHFIF_DEF_DTL (TID, AZMAB, TAMAB, TPERS, TMAB, UP_TIME, UP_DATE)
                                     SELECT TID, AZMAB, TAMAB, TPERS, TMAB, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}
                                     FROM dbo.TAKHFIF_DEF_DTL
                                     WHERE TID = {TID.Text}");

                dbms.DoExecuteSQL($@"INSERT INTO dbo.TR_CUSTKIND_TF (CUST_COD, TID, CREA, ACTIV, UP_TIME, UP_DATE)
                                     SELECT CUST_COD, TID, CREA, ACTIV, {Tarikh.GET_OADATE_DAO()}, {CL_HESABDARI.FARSIDATE()}
                                     FROM dbo.CUSTKIND_TF
                                     WHERE TID = {TID.Text}");

                AllowDeletions = true;
                AllowEdits = true;
                ActivateDataGrids(true);
            }
        }
     
    }
}
