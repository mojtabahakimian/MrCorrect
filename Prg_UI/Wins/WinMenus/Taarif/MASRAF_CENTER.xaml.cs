using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Wins.WinMenus.Taarif
{
    public partial class MASRAF_CENTER : Window, INavigator
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
        public MASRAF_CENTER()
        {
            InitializeComponent();
        }
        public List<HKOLM> HKM_SOURCE { get; set; } = new List<HKOLM>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();
        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();

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
        private bool can;
        public bool AllowEdits
        {
            get { return can; }
            set
            {
                can = value;
                if (can is true) // Is Enable and ReadOnly = False
                {

                }
                else
                {

                }
            }
        }
        public long? MasterID { get; set; }
        public bool NewRecord { get; private set; }
        public double? NUMBER_TO_OPEN { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool INavigator.NewRecord { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MARM", new WindowInteropHelper(this).Handle);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            NAMES.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            HKM_SOURCE = dbms.DoGetDataSQL<HKOLM>(@"SELECT NUMBER, NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME").ToList(); //کل

            N_KOL.ItemsSource = HKM_SOURCE;
        }
        private void ReLoadMoin() //معین
        {
            if (N_KOL.SelectedValue != null)
            {
                NUMBER.ItemsSource = dbms.DoGetDataSQL<HKOLM>($@"SELECT NAME, NUMBER FROM dbo.DETA_HES WHERE DETA_HES.N_KOL={N_KOL.SelectedValue}").ToList();
                TNUMBER.ItemsSource = null;
            }
        }
        private void ReLoadTaf() //تفضیلی
        {
            if (N_KOL.SelectedValue != null && NUMBER.SelectedValue != null)
            {
                TNUMBER.ItemsSource = dbms.DoGetDataSQL<HTAF>($@"SELECT TNUMBER, NAME, TDETA_HES.TNUMBER FROM dbo.TDETA_HES  WHERE (((TDETA_HES.N_KOL)={N_KOL.SelectedValue}) AND ((TDETA_HES.NUMBER)={NUMBER.SelectedValue})) ORDER BY TDETA_HES.NAME").ToList();
            }
        }
        private void N_KOL_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) //کل
        {
            ReLoadMoin();
        }
        private void NUMBER_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) //معین
        {
            ReLoadTaf();
        }
        private bool BodyIsValid()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(NAMES.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مرکز مصرف نمی تواند خالی باشد" });
            }
            if (N_KOL.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل نمیتواند خالی باشد" });
            }
            if (NUMBER.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین نمیتواند خالی باشد" });
            }
            if (TNUMBER.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی نمیتواند خالی باشد" });
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
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BodyIsValid())
            {
                return;
            }

            long? _id_ = null;
            try
            {
                if (MasterID > 0) //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_MANF
                                         SET FNUMB = {FNUMB.Text},NAMES = N'{NAMES.Text}', N_KOL = {N_KOL.SelectedValue}, NUMBER = {NUMBER.SelectedValue}, TNUMBER = {TNUMBER.SelectedValue}
                                         WHERE ID = {MasterID}");
                }
                else //INSERT
                {
                    //Form_BeforeInsert
                    if (string.IsNullOrEmpty(FNUMB.Text) || FNUMB.Text == "0")
                    {
                        var RST = dbms.DoGetDataSQL<int?>("SELECT Max(HEAD_MANF.FNUMB) AS MaxOfFNUMB FROM HEAD_MANF").FirstOrDefault();
                        if (RST is null || RST == 0)
                        {
                            FNUMB.Text = "1";
                        }
                        else
                        {
                            FNUMB.Text = Convert.ToInt32(RST + 1).ToString();
                        }
                    }

                    _id_ = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.HEAD_MANF(FNUMB, NAMES, N_KOL, NUMBER, TNUMBER)
                                                       OUTPUT INSERTED.ID
                                                       VALUES({FNUMB.Text},
                                                       N'{NAMES.Text}',
                                                       {N_KOL.SelectedValue},
                                                       {NUMBER.SelectedValue},
                                                       {TNUMBER.SelectedValue})
                                                       ").FirstOrDefault();

                    if (_id_ != null)
                    {
                        MasterID = (long)_id_;
                        NewRecord = false;
                        RefreshAfterInsert();
                    }

                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "ذخیره به دلیل خطا انجام نشد!").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e) //-----------------------------------------------------------------------------
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (MasterID != null)
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "مراکز مصرف",
                            recordId: MasterID.ToStringNullSafe(),
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    try
                    {
                        dbms.DoExecuteSQL($@" DELETE FROM dbo.HEAD_MANF WHERE ID = {MasterID} ");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این مرکز صرف دارای گردش است و نمیتوان آنرا پاک کرد!");
                        }
                        else
                        {
                            new Msgwin(false, "خطا حذف انجام نشد!");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                    }
                    RefreshAfterDelete();
                }
            }
        }

        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<HEAD_MANF>($"SELECT ID, FNUMB, CODE, DATE_ACTIV, IMBIBE_MANF, IMBIBE_SAR, GHEYMAT, NAMES, N_KOL, NUMBER, TNUMBER, SA_HOUR, SA_NHOU, TOZIH, CRT, UID FROM dbo.HEAD_MANF WHERE((NOT(HEAD_MANF.N_KOL) IS NULL))OR((NOT(HEAD_MANF.NAMES) IS NULL))  ORDER BY FNUMB").ToList();
            RecordsData.Source = MasterHead;

            //if (NUMBER_TO_OPEN != null)
            //{
            //    var item = RecordsData.View.Cast<_YOURMODEL_>().FirstOrDefault(x => x.CODE.Equals(NUMBER_TO_OPEN.ToString()));
            //    if (item != null)
            //    {
            //        // Set the CurrentItem to the found item
            //        RecordsData.View.MoveCurrentTo(item);

            //        MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
            //    }
            //}
            //else
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
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1)  //[RecordCount() - 1] : just ensure that stand on existing real item
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
                var HEADER = RecordsData.View.CurrentItem as HEAD_MANF;
                var DBData = dbms.DoGetDataSQL<HEAD_MANF>($"SELECT ID,FNUMB, CODE, DATE_ACTIV, IMBIBE_MANF, IMBIBE_SAR, GHEYMAT, NAMES, N_KOL, NUMBER, TNUMBER, SA_HOUR, SA_NHOU, TOZIH, CRT, UID FROM dbo.HEAD_MANF WHERE ID={HEADER.ID}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(HEAD_MANF).GetProperties();
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
            }
            else
            {
                UiDataUpdate();
            }
        }
        public void ClearFreshNew()
        {
            MasterID = null;
            FNUMB.Text = null;
            NAMES.Text = null;

            N_KOL.SelectedIndex = -1;
            NUMBER.SelectedIndex = -1;
            TNUMBER.SelectedIndex = -1;
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as HEAD_MANF;

                MasterID = HEADER.ID;
                FNUMB.Text = HEADER?.FNUMB.ToStringNullSafe();
                NAMES.Text = HEADER.NAMES;
                N_KOL.SelectedValue = HEADER.N_KOL; N_KOL.Items.Refresh();
                NUMBER.SelectedValue = HEADER.NUMBER; NUMBER.Items.Refresh();
                TNUMBER.SelectedValue = HEADER.TNUMBER; TNUMBER.Items.Refresh();
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
                var itemToRemove = RecordsData.View.CurrentItem as HEAD_MANF;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<HEAD_MANF>;
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
            var itemtoadd = dbms.DoGetDataSQL<HEAD_MANF>($"SELECT ID,FNUMB, CODE, DATE_ACTIV, IMBIBE_MANF, IMBIBE_SAR, GHEYMAT, NAMES, N_KOL, NUMBER, TNUMBER, SA_HOUR, SA_NHOU, TOZIH, CRT, UID FROM dbo.HEAD_MANF WHERE ID={MasterID}").FirstOrDefault();

            var underlyingCollection = RecordsData.Source as List<HEAD_MANF>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
            }
        }

        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NewItem);
            NAMES.Focus();
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

        public void Form_Current()
        {
        }
        public void ReGetData()
        {
        }
    }
}
