using Interfaces;
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
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Wins.WinMenus.Checkha
{
    public partial class WIN_PAY_GETD : Window
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

        public WIN_PAY_GETD(long? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (long)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();

        public ObservableCollection<TOTA_HES> TOTA_HES_DATA { get; set; } = new ObservableCollection<TOTA_HES>();

        //public ObservableCollection<DETA_HES> DETA_HES_DATA { get; set; } = new ObservableCollection<DETA_HES>();
        //public ObservableCollection<TDETA_HES> TDETA_HES_DATA { get; set; } = new ObservableCollection<TDETA_HES>();
        //public ObservableCollection<TDETA_HES2> TDETA_HES2_DATA { get; set; } = new ObservableCollection<TDETA_HES2>();
        //public ObservableCollection<TDETA_HES3> TDETA_HES3_DATA { get; set; } = new ObservableCollection<TDETA_HES3>();
        //public ObservableCollection<TDETA_HES4> TDETA_HES4_DATA { get; set; } = new ObservableCollection<TDETA_HES4>();

        #region LOCALMODEL
        public class SQLKMT
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }
        public class PAYGETDQRE1
        {
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE { get; set; }
        }
        #endregion

        public long? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool PAY_GETD_SUB_IsFocused { get; private set; }

        private bool newrecord;
        public bool NewRecord
        {
            get
            {
                if (HEADER_ID == null || HEADER_ID == 0)
                {
                    newrecord = true;
                }
                else
                {
                    newrecord = false;
                }
                return newrecord;
            }
            set { newrecord = value; }
        }

        public long? HEADER_ID { get; set; } = null;
        public bool ChangeIsHappend { get; private set; }

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

                //TextBox.IsReadOnly = !ican;

                //ComboBox.IsEnabled = ican;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "CHEKD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

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

        private void FILL_ALL_COMBOBOXES()
        {
            //بانکها
            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT CODE, NAMES FROM dbo.TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();

            //نام شعبه
            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT SHOBEH FROM dbo.PAY_GETD GROUP BY PAY_GETD.SHOBEH ORDER BY PAY_GETD.SHOBEH").ToList();

            //نام پرداخت کننده
            NAME_TAH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT NAME_TAH FROM dbo.PAY_GETD GROUP BY PAY_GETD.NAME_TAH ORDER BY PAY_GETD.NAME_TAH").ToList();

            //حساب های کل
            var _KOLROW_ = dbms.DoGetDataSQL<TOTA_HES>("SELECT NUMBER, NAME FROM dbo.TOTA_HES ORDER BY TOTA_HES.NAME").ToList();
            N_KOL.ItemsSource = _KOLROW_;
            N_KOL_SEC.ItemsSource = _KOLROW_;

            //x:Name="N_KOL" DisplayMemberPath="N_KOL" SelectedValuePath="N_KOL"

            N_KOL2.ItemsSource = _KOLROW_;
            N_KOL2_SEC.ItemsSource = _KOLROW_;

            N_KOL3.ItemsSource = _KOLROW_;
            N_KOL3_SEC.ItemsSource = _KOLROW_;
        }

        public void Form_Current()
        {
        }
        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM dbo.PAY_GETD ORDER BY N_SERI ASC ").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<PAY_GETD>().FirstOrDefault(x => x.ID.Equals(NUMBER_TO_OPEN));
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

            if (ChangeIsHappend && !ConfirmExitWithoutSaving())
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
                var HEADER = RecordsData.View.CurrentItem as PAY_GETD;
                var DBData = dbms.DoGetDataSQL<PAY_GETD>($" SELECT * FROM dbo.PAY_GETD WHERE ID = {HEADER.ID} ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(PAY_GETD).GetProperties();
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
            HEADER_ID = null;
            //Clearing Elemts Values
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as PAY_GETD;

                HEADER_ID = HEADER.ID;

                N_SERI.Text = HEADER.N_SERI.ToStringNullSafe(); //شماره سریال
                BANK.SelectedValue = HEADER.BANK; //بانک
                SHOBEH.SelectedValue = HEADER.SHOBEH; //نام شعبه
                DATE.Text = HEADER.DATE.ToStringNullSafe(); //تاریخ دریافت
                DATE_S.Text = HEADER.DATE_S.ToStringNullSafe(); //تاریخ سررسید
                MABL.Text = HEADER.MABL.ToStringNullSafe(); //مبلغ
                NAME_TAH.SelectedValue = HEADER.NAME_TAH; //نام پرداخت کننده
                N_HESAB.Text = HEADER.N_HESAB; //جاری چک
                RADIF.Text = HEADER.RADIF.ToStringNullSafe(); //ردیف دفتر
                VAZ.Text = HEADER.VAZ.ToStringNullSafe(); //صندوق
                SAYADI.Text = HEADER.SAYADI; //شماره صیادی


                N_TAF.SelectedValue = HEADER.N_TAF; //تفضیلی
                N_MOIN.SelectedValue = HEADER.N_MOIN; //حساب معین
                N_KOL.SelectedValue = HEADER.N_KOL; //حساب کل => واگذاری به حساب

                N_TAF2.SelectedValue = HEADER.N_TAF2; //تفضیلی
                N_MOIN2.SelectedValue = HEADER.N_MOIN2; //حساب معین
                N_KOL2.SelectedValue = HEADER.N_KOL2; //حساب کل => برگشت به حساب

                N_TAF3.SelectedValue = HEADER.N_TAF3; //تفضیلی
                N_MOIN3.SelectedValue = HEADER.N_MOIN3; //حساب معین
                N_KOL3.SelectedValue = HEADER.N_KOL3; //حساب کل => وصول به حساب

                N_S.Content = HEADER.N_S; //شماره سند
                NUMBER.Content = HEADER.NUMBER; //شماره فاکتور
                TAG.Content = HEADER.TAG; //نوع فاکتور

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
                var itemToRemove = RecordsData.View.CurrentItem as PAY_GETD;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<PAY_GETD>;
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
            var itemtoadd = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM dbo.PAY_GETD WHERE ID = {HEADER_ID} ").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<PAY_GETD>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                ////MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
            }
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

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            //Here Save
            RefreshAfterInsert();

            ChangeIsHappend = false;
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                try
                {
                    var IsDeletedSomething = true;

                    var rst0 = dbms.DoGetDataSQL<PAYGETDQRE1>("SELECT PAY_GETD.N_SERI, PAY_GETD.BANK, PGET_LST.DATE FROM PAY_GETD INNER JOIN PGET_LST ON (PAY_GETD.BANK = PGET_LST.BANK) AND (PAY_GETD.N_SERI = PGET_LST.N_SERI) WHERE (((PAY_GETD.N_SERI)=" + N_SERI.Text + ") AND ((PAY_GETD.BANK)=" + BANK.SelectedValue + "))").FirstOrDefault();
                    if (rst0 != null)
                    {
                        new Msgwin(false, " اين چك در خزانه داري مورخ  " + Strings.Format(rst0.DATE, "####/##/##") + " داراي گردش مي باشدو نمي توانيد آن را حذف كنيد...!").ShowDialog();
                    }
                    else
                    {
                        var rst = dbms.DoGetDataSQL<double?>("SELECT dbo.DEED_DTL.N_S FROM dbo.PAY_GETD INNER JOIN  dbo.DEED_DTL ON dbo.PAY_GETD.N_SERI = dbo.DEED_DTL.N_SERI AND dbo.PAY_GETD.BANK = dbo.DEED_DTL.BANK WHERE (dbo.PAY_GETD.N_SERI = " + N_SERI.Text + ") And (dbo.PAY_GETD.BANK = " + BANK.SelectedValue + ") GROUP BY dbo.DEED_DTL.N_S").FirstOrDefault();
                        if (rst != null)
                        {
                            new Msgwin(false, "  اين چك در سند شماره " + rst + " داراي گردش مي باشدو نمي توانيد آن را حذف كنيد...!").ShowDialog();
                        }
                        else
                        {
                            this.N_KOL.SelectedValue = 911; N_KOL.Items.Refresh();
                            this.N_MOIN.SelectedValue = 1; N_MOIN.Items.Refresh();
                            this.N_TAF.SelectedValue = 1; N_TAF.Items.Refresh();
                            this.N_KOL2.SelectedValue = null;
                            this.N_MOIN2.SelectedValue = null;
                            this.N_TAF2.SelectedValue = null;
                            this.N_KOL3.SelectedValue = null;
                            this.N_MOIN3.SelectedValue = null;
                            this.N_TAF3.SelectedValue = null;

                            DoCmdSavePAY_GETD();

                            new Msgwin(false, "چك بصورت مجازي از دفتر چك حذف گرديد").ShowDialog();
                        }
                    }

                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        new Msgwin(false, "این چک دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف چک").ShowDialog();
                }
            }
        }

        private void N_KOL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //After_Update
            //واگذار به حساب
            if (N_KOL.SelectedValue != null) //کل
            {
                FILL_SOURCE_MOIN_TAF(N_KOL.SelectedValue.ToStringNullSafe(), N_MOIN, N_TAF);
            }
            else
            {
                N_MOIN.SelectedValue = null;
                N_TAF.SelectedValue = null;
            }

        }
        private void N_MOIN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //واگذار به حساب
            //معین
            if (N_KOL.SelectedValue != null && N_MOIN.SelectedValue != null) //کل
            {
                FILL_SOURCE_TAF(N_KOL.SelectedValue.ToStringNullSafe(), N_MOIN.SelectedValue.ToStringNullSafe(), N_TAF);
            }
            else
            {
                N_TAF.SelectedValue = null;
            }
        }
        private void N_TAF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //واگذار به حساب
            //تفضیلی
            if (N_MOIN.SelectedValue != null)
            {
            }
        }

        #region ITEMSOURCE_CHANGER
        private void FILL_SOURCE_MOIN_TAF(string kolValue, ComboBox _MOIN_COMBO_, ComboBox _TAF_COMBO_)
        {
            //معین
            //object lastMoinSelection = _MOIN_COMBO_.SelectedValue;
            if (_TAF_COMBO_?.ItemsSource != null)
            {
                _TAF_COMBO_.ItemsSource = null;
            }

            _MOIN_COMBO_.ItemsSource = dbms.DoGetDataSQL<SQLKMT>($"SELECT NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={kolValue}");

            if (_MOIN_COMBO_.SelectedValue != null)
            {
                FILL_SOURCE_TAF(kolValue, _MOIN_COMBO_.SelectedValue.ToString(), _TAF_COMBO_);
            }
            else
            {
                _MOIN_COMBO_.SelectedValue = null;

                if (_TAF_COMBO_ != null)
                {
                    _TAF_COMBO_.SelectedValue = null;
                }
            }
        }
        private void FILL_SOURCE_TAF(string kolValue, string moinValue, ComboBox _TAF_COMBO_)
        {
            //تفضیلی
            if (_TAF_COMBO_ != null)
            {
                object lastTafSelection = _TAF_COMBO_.SelectedValue;
                _TAF_COMBO_.ItemsSource = dbms.DoGetDataSQL<SQLKMT>($"SELECT TNUMBER, NAME FROM dbo.TDETA_HES WHERE N_KOL = {kolValue} AND NUMBER = {moinValue}");
                _TAF_COMBO_.SelectedValue = lastTafSelection;
                _TAF_COMBO_.Items.Refresh();
            }
        }
        #endregion

        private void N_KOL2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //برگشت به حساب
            if (N_KOL2.SelectedValue != null) //کل
            {
                FILL_SOURCE_MOIN_TAF(N_KOL2.SelectedValue.ToStringNullSafe(), N_MOIN2, default);
            }
            else
            {
                N_MOIN2.SelectedValue = null;
                N_TAF2.SelectedValue = null;
            }
        }

        private void N_MOIN2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //برگشت به حساب
            //معین
            if (N_KOL2.SelectedValue != null && N_MOIN2.SelectedValue != null) //کل
            {
                FILL_SOURCE_TAF(N_KOL2.SelectedValue.ToString(), N_MOIN2.SelectedValue.ToString(), N_TAF2);
            }
            else
            {
                N_TAF2.SelectedValue = null;
            }
        }

        private void N_TAF2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //برگشت به حساب
        }

        private void N_KOL3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //وصول به حساب
            if (N_KOL3.SelectedValue != null) //کل
            {
                FILL_SOURCE_MOIN_TAF(N_KOL3.SelectedValue.ToStringNullSafe(), N_MOIN3, default);
            }
            else
            {
                N_MOIN3.SelectedValue = null;
                N_TAF3.SelectedValue = null;
            }
        }

        private void N_MOIN3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //وصول به حساب
            //معین
            if (N_KOL3.SelectedValue != null && N_MOIN3.SelectedValue != null) //کل
            {
                FILL_SOURCE_TAF(N_KOL3.SelectedValue.ToString(), N_MOIN3.SelectedValue.ToString(), N_TAF3);
            }
            else
            {
                N_TAF3.SelectedValue = null;
            }
        }

        private void N_TAF3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //وصول به حساب
        }
        private void BTN_CLEARATION_Click(object sender, RoutedEventArgs e)
        {
            if (BANK.SelectedValue == null || string.IsNullOrEmpty(N_SERI.Text))
            {
                return;
            }

            var rst = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.ADA + "' OR HES = '" + Baseknow.ADV + "' ) AND (BES > 0) AND (BANK = " + BANK.SelectedValue + ") AND (N_SERI = " + N_SERI.Text + ")").FirstOrDefault();
            if (rst != null)
            {
                new Msgwin(false, "اين چك در سند شماره " + rst + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را حذف نماييد").ShowDialog();
            }
            else
            {
                if (N_KOL.SelectedValue.ToStringNullSafe() != Baseknow.BANKHA.ToStringNullSafe())
                {
                    N_KOL.SelectedValue = null;
                    N_MOIN.SelectedValue = null;
                    N_TAF.SelectedValue = null;
                }
                N_KOL2.SelectedValue = null;
                N_MOIN2.SelectedValue = null;
                N_TAF2.SelectedValue = null;
                N_KOL3.SelectedValue = null;
                N_MOIN3.SelectedValue = null;
                N_TAF3.SelectedValue = null;
                N_S.Content = null;
            }

            DoCmdSavePAY_GETD();

        }

        private bool DoCmdSavePAY_GETD()
        {
            try
            {
                dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET 
                                        N_S = {(string.IsNullOrEmpty(N_S?.Content.ToStringNullSafe()) ? "NULL" : N_S.Content.ToString())},
                                        N_KOL = {(string.IsNullOrEmpty(N_KOL.SelectedValue.ToStringNullSafe()) ? "NULL" : N_KOL.SelectedValue.ToString())},
                                        N_MOIN = {(string.IsNullOrEmpty(N_MOIN.SelectedValue.ToStringNullSafe()) ? "NULL" : N_MOIN.SelectedValue.ToString())},
                                        N_TAF = {(string.IsNullOrEmpty(N_TAF.SelectedValue.ToStringNullSafe()) ? "NULL" : N_TAF.SelectedValue.ToString())},
                                        N_KOL2 = {(string.IsNullOrEmpty(N_KOL2.SelectedValue.ToStringNullSafe()) ? "NULL" : N_KOL2.SelectedValue.ToString())},
                                        N_MOIN2 = {(string.IsNullOrEmpty(N_MOIN2.SelectedValue.ToStringNullSafe()) ? "NULL" : N_MOIN2.SelectedValue.ToString())},
                                        N_TAF2 = {(string.IsNullOrEmpty(N_TAF2.SelectedValue.ToStringNullSafe()) ? "NULL" : N_TAF2.SelectedValue.ToString())},
                                        N_KOL3 = {(string.IsNullOrEmpty(N_KOL3.SelectedValue.ToStringNullSafe()) ? "NULL" : N_KOL3.SelectedValue.ToString())},
                                        N_MOIN3 = {(string.IsNullOrEmpty(N_MOIN3.SelectedValue.ToStringNullSafe()) ? "NULL" : N_MOIN3.SelectedValue.ToString())},
                                        N_TAF3 = {(string.IsNullOrEmpty(N_TAF3.SelectedValue.ToStringNullSafe()) ? "NULL" : N_TAF3.SelectedValue.ToString())}
                                     WHERE ID = {HEADER_ID}");

                universControl.PopNotifyShow("وضعیت چک ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "خطا چک تکراری در ذخیره وضعیت چک").ShowDialog();
                    return false;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره وضعیت این چک دریافتی!").ShowDialog();
                return false;
            }

            return true;
        }
    }
}
