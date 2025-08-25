using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using MimeDetective.Storage;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.CompoundFile.XlsIO.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;

namespace Wins.WinMenus.Taarif
{
    public partial class TCODE_MENUITEM_WIN : Window, INavigator
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

        public TCODE_MENUITEM_WIN(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool NewRecord { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        long? MASTER_ID = null;

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
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MENUIT", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            NAMES.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
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
            ANBAR.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT CODE, NAMES FROM dbo.TCOD_ANBAR").ToList();
        }

        private async void LoadMatchingImageAsync(string _CODE_)
        {
            try
            {
                string _PATH_ = Baseknow.BACKPATH + @"\grp\";

                // First, check if the shared folder path exists to avoid unnecessary delay
                var pathExists = await Task.Run(() => Directory.Exists(_PATH_));
                if (!pathExists)
                {
                    return;
                }

                // If the shared folder exists, proceed to search for the image file
                string imagePath = await Task.Run(() => CL_LMethods.FindImageFile(_PATH_, _CODE_));
                if (!string.IsNullOrEmpty(imagePath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 200; // Adjust based on your UI needs
                    image.UriSource = new Uri(imagePath);
                    image.EndInit();
                    image.Freeze(); // Improve performance for UI binding

                    // Assuming 'ProductImage' is an Image control in XAML
                    Dispatcher.Invoke(() => PIC.Source = image);
                }
            }
            catch { }
        }
        public void Form_Current()
        {
            PIC.Source = null;
            if (!this.NewRecord)
            {
                if (!string.IsNullOrEmpty(Baseknow.BACKPATH))
                {
                    LoadMatchingImageAsync(CODE.Text);
                }
            }
        }
        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<TCODE_MENUITEM>($"SELECT ID,CODE, NAMES, pic, ANBAR, tic, CRT, UID FROM dbo.TCODE_MENUITEM").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<TCODE_MENUITEM>().FirstOrDefault(x => x.ID.Equals(NUMBER_TO_OPEN.ToString()));
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
                var HEADER = RecordsData.View.CurrentItem as TCODE_MENUITEM;
                var DBData = dbms.DoGetDataSQL<TCODE_MENUITEM>($" SELECT ID,CODE, NAMES, pic, ANBAR, tic, CRT, UID FROM dbo.TCODE_MENUITEM WHERE ID = {HEADER.ID} ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(TCODE_MENUITEM).GetProperties();
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
            CODE.Text = null;
            NAMES.Text = null;
            ANBAR.SelectedIndex = -1; ANBAR.Items.Refresh();
            PIC.Source = null;
            MASTER_ID = null;

            NAMES.Focus();
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as TCODE_MENUITEM;

                CODE.Text = HEADER.CODE.ToString();
                NAMES.Text = HEADER.NAMES.ToString();
                ANBAR.SelectedValue = HEADER.ANBAR; ANBAR.Items.Refresh();

                MASTER_ID = HEADER.ID;

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
                var itemToRemove = RecordsData.View.CurrentItem as TCODE_MENUITEM;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<TCODE_MENUITEM>;
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
            var itemtoadd = dbms.DoGetDataSQL<TCODE_MENUITEM>($"SELECT ID,CODE, NAMES, pic, ANBAR, tic, CRT, UID FROM dbo.TCODE_MENUITEM WHERE CODE = N'{CODE.Text}' ").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<TCODE_MENUITEM>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
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

            if (string.IsNullOrEmpty(NAMES.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام نمی تواند خالی باشد" });
            }
            if (ANBAR.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار نمی تواند خالی باشد" });
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

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (!HeaderIsValid())
            {
                return;
            }

            try
            {
                if (MASTER_ID is null) //INSERT
                {
                    //Form_BeforeUpdate
                    if (string.IsNullOrEmpty(CODE.Text) || CODE.Text == "0")
                    {
                        var RST = dbms.DoGetDataSQL<double?>("SELECT Max(TCODE_MENUITEM.CODE) AS MaxOfCODE FROM dbo.TCODE_MENUITEM").FirstOrDefault();
                        if (RST.HasValue)
                        {
                            CODE.Text = Convert.ToString(Convert.ToDouble(RST) + 1);
                        }
                        else
                        {
                            CODE.Text = "1";
                        }
                    }

                    var headeridd = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.TCODE_MENUITEM(CODE, NAMES, ANBAR)
                                                               OUTPUT INSERTED.ID
                                                               VALUES({CODE.Text},
                                                               N'{NAMES.Text}',
                                                               {ANBAR.SelectedValue})").FirstOrDefault();
                    if (headeridd != null)
                    {
                        MASTER_ID = headeridd;
                    }
                    RefreshAfterInsert();
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TCODE_MENUITEM
                                         SET CODE = {CODE.Text}, NAMES = N'{NAMES.Text}', ANBAR = {ANBAR.SelectedValue} WHERE ID = {MASTER_ID}");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601)
                {
                    new Msgwin(false, "نام تکراری است !, نمیتوان ذخیره کرد !");
                }
                else if (ex.Number == 2627)
                {
                    new Msgwin(false, "این کد تکراری است آنرا تغییر دهید");
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            ChangeIsHappend = false;

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

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
                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "تعریف گروه بندی برای ویزیتور",
                        recordId: MASTER_ID.ToStringNullSafe(),
                        oldValue: null,
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                try
                {
                    //Delete From Header if there is no sub data
                    dbms.DoExecuteSQL($"DELETE FROM dbo.TCODE_MENUITEM WHERE ID = {MASTER_ID}");
                    RefreshAfterDelete();

                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        new Msgwin(false, "حذف به دلیل داشتن گردش مقدور نیست !").ShowDialog(); return;
                    }
                    else
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog();
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                }
            }
        }
        public void ReGetData()
        {
            throw new NotImplementedException();
        }
    }

}
