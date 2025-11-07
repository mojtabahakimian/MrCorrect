using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Syncfusion.Data.Extensions;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Wins.WinOther;
using Microsoft.VisualBasic;
using System.Globalization;
using Functions;
using System.Reflection;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using System.Windows.Data;
using Interfaces;
using static Prg_UI.HelperWins.Msgwin;
using Rpts;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Wins.WinMenus.SALARY
{
    public partial class PMORAKH : Window, ISearchableWindow , IComboLookupProvider
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

        public PMORAKH(int? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (int)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();

        #region LOCALMODEL
        public class COMBO_PERSONE
        {
            public int? CODE { get; set; }
            public string? PER { get; set; }
        }
        #endregion

        public int? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
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

        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;

                CODE.IsEnabled = ican;
                KINDM.IsEnabled = ican;

                MODATE.IsReadOnly = !ican;
                MOLAH.IsReadOnly = !ican;
                MOSTDATE.IsReadOnly = !ican;
                MOENDATE.IsReadOnly = !ican;
                DA.IsReadOnly = !ican;
                HOU.IsReadOnly = !ican;
                minu.IsReadOnly = !ican;

            }
        }

        private double? PSK1 = null, PSM1 = null, PST1 = null, PST2 = null, PST3 = null, PST4, Meidnum = null;

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                if (NowIsReady && (string.IsNullOrEmpty(IDNUM.Text) || string.IsNullOrWhiteSpace(IDNUM.Text) || IDNUM.Text == "0"))
                {
                    _newrecord = true;
                }
                else
                {
                    _newrecord = false;
                }

                return _newrecord;
            }
            set { _newrecord = value; }
        }

        /// <summary>
        /// 440
        /// </summary>
        const double SALMINUT_440 = 440;

        /// <summary>
        /// 60
        /// </summary>
        const double HMINUT_60 = 60;

        public class SGN_IMODEL
        {
            public string SEMAT_USER { get; set; }
            public string NAME_HESAB_USER { get; set; }
        }
        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "SGN0137TX");
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
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "SGN0237TX");
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
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "SGN0337TX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is PMORAKH_MODEL item)
            {
                if (item != null)
                {
                    var itemfound = RecordsData.View.Cast<PMORAKH_MODEL>().FirstOrDefault(x => x.CODE.Equals(Convert.ToInt32(item.CODE)));
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
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "IDNUM", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "کد پرسنلی", PropertyPath = "CODE", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "تاريخ درخواست", PropertyPath = "MODATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "تاريخ شروع", PropertyPath = "MOSTDATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "تاريخ پایان", PropertyPath = "MOENDATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "توضیحات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
            };
        }
        public IEnumerable<ComboLookupSpec> GetComboLookups()
        {
            yield return new ComboLookupSpec { DisplayName = "نام پرسنل", KeyPropertyPath = "CODE", Combo = CODE };
            // مثال عمومی برای فرم‌های بعدی:
            // yield return new ComboLookupSpec { DisplayName = "نام کالا", KeyPropertyPath = "GOOD_ID", Combo = GOODS_COMBO };
            // yield return new ComboLookupSpec { DisplayName = "نام مشتری", KeyPropertyPath = "CUST_ID", Combo = CUSTOMER_COMBO };
        }
        #endregion
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MORAKH", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            CL_HESABDARI.LetSigneTick(this.GetType().Name, 37, (int)Baseknow.USERCOD, new WindowInteropHelper(this).Handle);

            //Form_Open
            #region FormOpen
            if ((bool)Baseknow.SIGN)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
                this.SGN1usid.Visibility = Visibility.Visible;
                this.SGN1usid.Visibility = Visibility.Visible;
                this.SGN3usid.Visibility = Visibility.Visible;
                this.BTN_CHAP.IsEnabled = false;
            }
            else
            {
                this.SGN1.Visibility = Visibility.Hidden;
                this.SGN2.Visibility = Visibility.Hidden;
                this.SGN3.Visibility = Visibility.Hidden;
                this.SGN1usid.Visibility = Visibility.Hidden;
                this.SGN1usid.Visibility = Visibility.Hidden;
                this.SGN3usid.Visibility = Visibility.Hidden;
                this.BTN_CHAP.IsEnabled = true;
            }
            CL_HESABDARI.GETTAF3(Baseknow.PERSONEL, ref PSK1, ref PSM1, ref PST1, ref PST2, ref PST3, ref PST4);
            #endregion

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            if (NewRecord)
            {
                MODATE.Text = Tarikh.FullCurrentDate;
                MOSTDATE.Text = Tarikh.FullCurrentDate;
                MOENDATE.Text = Tarikh.FullCurrentDate;
            }

            Form_Current();

            CODE.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_SAVE.IsFocused) //Not Focused on this button
                {
                    e.Handled = true;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }

            try
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
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
        private void FILL_ALL_COMBOBOXES()
        {
            //پرسنل
            CODE.ItemsSource = dbms.DoGetDataSQL<COMBO_PERSONE>($"SELECT CODE, PNAME + N' ' + PFAMILY + N' ' + RTRIM(CAST(CODE AS NVARCHAR)) AS PER FROM PERSONEL ORDER BY PNAME + N' ' + PFAMILY + N' ' + RTRIM(CAST(CODE AS NVARCHAR))").ToList();


            //نوع مرخصی
            KINDM.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "استحقاقي" },
                new COMBOYMODEL { ID = 1, NAME = "استراحت پزشكي" },
                new COMBOYMODEL { ID = 2, NAME = "ساعتي" },
                new COMBOYMODEL { ID = 3, NAME = "بدون حقوق" },
                new COMBOYMODEL { ID = 4, NAME = "تشویقی" }
            };
            KINDM.SelectedValue = 0; KINDM.Items.Refresh();

            //کبموباکس ارجاع به
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
        }

        private void ReGetMasterData()
        {
            //if (NUMBER_TO_OPEN == null || NUMBER_TO_OPEN == 0)
            //{
            //    return;
            //}
            var MasterHead = dbms.DoGetDataSQL<PMORAKH_MODEL>($"SELECT CODE, MODATE, MORAKHDAY, MAND, KINDM, MOLAH, MOSTDATE, MOENDATE, OKF, IDNUM, sgn1usid, sgn2usid, sgn3usid, SGN1, SGN2, SGN3, CRT, UID FROM dbo.PMORAKH ORDER BY IDNUM").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<PMORAKH_MODEL>().FirstOrDefault(x => x.IDNUM.Equals(NUMBER_TO_OPEN));
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
            //var RST = dbms.DoGetDataSQL<PMORAKH_MODEL>($"SELECT CODE, MODATE, MORAKHDAY, MAND, KINDM, MOLAH, MOSTDATE, MOENDATE, OKF, IDNUM, sgn1usid, sgn2usid, sgn3usid, SGN1, SGN2, SGN3, CRT, UID FROM dbo.PMORAKH WHERE IDNUM = {NUMBER_TO_OPEN}").FirstOrDefault();
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
                    AllowEdits = true;
                    ClearFreshAll();
                    break;
            }

            //Update CurrentViewItem
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as PMORAKH_MODEL;
                var DBData = dbms.DoGetDataSQL<PMORAKH_MODEL>($" SELECT CODE, MODATE, MORAKHDAY, MAND, KINDM, MOLAH, MOSTDATE, MOENDATE, OKF, IDNUM, sgn1usid, sgn2usid, sgn3usid, SGN1, SGN2, SGN3, CRT, UID FROM dbo.PMORAKH WHERE IDNUM = {HEADER.IDNUM} ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(PMORAKH_MODEL).GetProperties();
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
                ClearFreshAll();

                Form_Current();
            }
            else
            {
                UiDataUpdate();
            }
        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as PMORAKH_MODEL;

                IDNUM.Text = HEADER.IDNUM.ToString();
                CODE.SelectedValue = HEADER.CODE; CODE.Items.Refresh();
                OKF.IsChecked = Convert.ToBoolean(HEADER.OKF);
                MODATE.Text = HEADER.MODATE.ToStringNullSafe();
                KINDM.SelectedValue = HEADER.KINDM; KINDM.Items.Refresh();
                MOLAH.Text = HEADER.MOLAH.ToStringNullSafe();
                MOSTDATE.Text = HEADER.MOSTDATE.ToStringNullSafe();
                MOENDATE.Text = HEADER.MOENDATE.ToStringNullSafe();

                //Hours and Days ...
                MORAKHDAY.Text = HEADER.MORAKHDAY.ToStringNullSafe();

                GetComputeMandDayOff(HEADER.MAND.ToStringNullSafe()); ////MAND.Text = HEADER.MAND.ToStringNullSafe();

                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3);

                SGN1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                SGN2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                SGN3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;


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
                var itemToRemove = RecordsData.View.CurrentItem as PMORAKH_MODEL;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<PMORAKH_MODEL>;
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
            var itemtoadd = dbms.DoGetDataSQL<PMORAKH_MODEL>($"SELECT CODE, MODATE, MORAKHDAY, MAND, KINDM, MOLAH, MOSTDATE, MOENDATE, OKF, IDNUM, sgn1usid, sgn2usid, sgn3usid, SGN1, SGN2, SGN3, CRT, UID FROM dbo.PMORAKH WHERE IDNUM = {IDNUM.Text} ").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<PMORAKH_MODEL>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                ////MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
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

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (CODE.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "پرسنل نمیتواند خالی باشد" });
            }

            if (!IsDateValid(MODATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ درخواست صحیح نیست" });
            }

            if (KINDM.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع مرخصی نمیتواند خالی باشد" });
            }

            if (!IsDateValid(MOSTDATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ شروع صحیح نیست" });
            }

            if (!IsDateValid(MOENDATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ پایان صحیح نیست" });
            }

            if (Convert.ToInt64(minu.Text) <= 0 && Convert.ToInt64(HOU.Text) <= 0 && Convert.ToInt64(DA.Text) <= 0)
            {
                if (NewRecord)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقادیر روز , ساعت , دقیقه حداقل یکی از آنها باید مقدار داشته باشد" });
                }
            }

            //Just in Case:
            if (!ErrosMessages.Any()) //if there is No Error untill here
            {
                GetComputeMandDayOff();
                Update_Day_Hour_Minut();
                CorrectDayOffsForInvalidDateRange();
                UpdateMultipy_MORAKHDAY_Value();
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


        bool IsSavedSuccess = true;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            IsSavedSuccess = true;

            if (!HeaderIsValid())
            {
                return;
            }

            if (!CmdSaveRecord()) //Try To Save data
            {
                return;
            }

            Form_Current();

            GetComputeMandDayOff();

            IsSavedSuccess = true;

            ChangeIsHappend = false;

            NewRecord = false;

            RefreshAfterInsert();

            universControl.PopNotifyShowUp($".ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (NewRecord) { return; }

            if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
            {
                new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                return;
            }

            if (CODE.SelectedValue != null)
            {
                var dt = DateTime.Now;
                CL_HESABDARI.TR("PMORAKH", "(IDNUM = " + IDNUM.Text + " )", dt, 1);

                AllowEdits = true;
                AllowDeletions = true;
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (BTN_DELETE.Visibility != Visibility.Visible || !BTN_DELETE.IsEnabled || NewRecord)
            {
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "ثبت مرخصی",
                        recordId: IDNUM.Text,
                        oldValue: null,
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                try
                {
                    dbms.DoExecuteSQL($@"DELETE FROM dbo.PMORAKH WHERE IDNUM = {IDNUM.Text}");

                    ClearFreshAll();

                    universControl.PopNotifyShowUp($".حذف انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        new Msgwin(false, $"این مرخصی دارای گردش است و نمیتوان آنرا پاک کرد !").ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog();
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog();
                }

                RefreshAfterDelete();
            }
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(IDNUM.Text) || IDNUM.Text == "0")
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null; PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (PERSONEL.SelectedItem != null)
            {
                string SelectedTextCMB = ((COMBOPERSONEL)PERSONEL.SelectedItem).SAL_NAME.ToStringNullSafe();
                Meidnum = CL_HESABDARI.PERSONELUpdate(37, Convert.ToDouble(IDNUM.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'برگه مرخصي شماره: : " + IDNUM.Text + "  مورخ " + Strings.Format(Convert.ToInt64(MODATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue) + "','" + PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue + "'");
                universControl.PopNotifyShow($"به این کاربر {SelectedTextCMB} ارجاع داده شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }
        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                SGN1.IsChecked = !SGN1.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;
            string td;
            SHARH = "'برگه مرخصي شماره: " + IDNUM.Text + " مورخ " + Strings.Format(Convert.ToInt64(MODATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue) + "','" + PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue + "'";

            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
            }

            CL_HESABDARI.PERSONELUpdate(37, Convert.ToDouble(IDNUM.Text), Convert.ToInt32(PERSONEL.SelectedValue), SHARH);
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChap();

            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE dbo.PMORAKH SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(this.SGN1.IsChecked == true, 1, 0) + $" WHERE IDNUM = {IDNUM.Text}");

            AllowEdits = false;
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                SGN2.IsChecked = !SGN2.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;
            string td;
            SHARH = "'برگه مرخصي شماره: " + IDNUM.Text + " مورخ " + Strings.Format(Convert.ToInt64(MODATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue) + "','" + PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue + "'";

            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, " :امضا شد2 ", " :امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, " : امضا شد2 ", " :امضا برداشته شد2 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
            }

            CL_HESABDARI.PERSONELUpdate(37, Convert.ToDouble(IDNUM.Text), Convert.ToInt32(PERSONEL.SelectedValue), SHARH);
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChap();

            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE dbo.PMORAKH SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(SGN2.IsChecked == true, 1, 0) + $" WHERE IDNUM = {IDNUM.Text}");

            AllowEdits = false;
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                SGN3.IsChecked = !SGN3.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;
            string td;
            SHARH = "'برگه مرخصي شماره: " + IDNUM.Text + " مورخ " + Strings.Format(Convert.ToInt64(MODATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue) + "','" + PSK1 + "-" + PSM1 + "-" + CODE.SelectedValue + "'";

            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, " :امضا شد3 ", " :امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDNUM.Text), 37);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, " : امضا شد3 ", " :امضا برداشته شد3 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",37," + this.IDNUM.Text + $",37 )");
            }

            CL_HESABDARI.PERSONELUpdate(37, Convert.ToDouble(IDNUM.Text), Convert.ToInt32(PERSONEL.SelectedValue), SHARH);
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChap();

            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE dbo.PMORAKH SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(SGN3.IsChecked == true, 1, 0) + $" WHERE IDNUM = {IDNUM.Text}");

            AllowEdits = false;
        }
        private void CODE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CODE.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            TextBox PERSONEL_TXB = (TextBox)CODE.Template.FindName("PART_EditableTextBox", CODE);

            if (CODE.SelectedValue is not null)
            {
                GetComputeMandDayOff();

                if ((CODE.SelectedItem as COMBO_PERSONE)?.PER == PERSONEL_TXB.Text)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(PERSONEL_TXB.Text.Trim()))
            {
                if (CL_LMethods.IsNumeric(PERSONEL_TXB.Text.Trim()))
                {
                    var CPDERESULT = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 CODE FROM dbo.PERSONEL WHERE CODE = {PERSONEL_TXB.Text.Trim()}").FirstOrDefault();
                    if (CPDERESULT != null)
                    {
                        CODE.SelectedValue = CPDERESULT; CODE.Items.Refresh();
                    }
                }
                else
                {
                    var Filter = "NAME LIKE N'%" + PERSONEL_TXB.Text + "%'";
                    new SERCHP(Filter, new WindowInteropHelper(this).Handle).ShowDialog();
                }
            }
        }
        private void BTN_MYDAYOFF_Click(object sender, RoutedEventArgs e)
        {
            if (CODE.SelectedValue == null)
            {
                new Msgwin(false, "ابتدا پرنسل را انتخاب کنید سپس مجددا امتحان کنید").ShowDialog();
                return;
            }

            TEXTB_POP.Text = $"با توجه به تعداد روزهای کارکرد شما تا تاریخ امروز یعنی {Tarikh.SlashyFullDate}، میزان مرخصی باقی ‌مانده شما به شرح زیر است:";

            GetComputeMandDayOff(JustSeeMandTemp: true);

            MANDDAYOFF_POP.IsOpen = true;
        }

        private void MODATE_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsDateValid(MODATE.Text))
            {
                e.Handled = true;
            }
            else
            {
                GetComputeMandDayOff();
            }
        }

        private void KINDM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (KINDM.SelectedValue == null)
            {
                universControl.PopNotifyShowUp("نوع مرخصی نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
            }
        }

        private void MOSTDATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsDateValid(MOSTDATE.Text))
            {
                e.Handled = true;
            }
            else
            {
                //تاریخ شروع //MOSTDATE_AfterUpdate
                CorrectDayOffsForInvalidDateRange(false);
            }
        }

        private void MOENDATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsDateValid(MOENDATE.Text))
            {
                e.Handled = true;
            }
            else
            {
                //تاریخ پایان //MOenDATE_AfterUpdate
                CorrectDayOffsForInvalidDateRange(false);
            }
        }

        private void minu_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(minu.Text)) { minu.Text = "0"; }

            UpdateMultipy_MORAKHDAY_Value();

            if (HeaderIsValid())
            {
                CmdSaveRecord();
            }
            else { return; }

            GetComputeMandDayOff();
        }

        private void HOU_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(HOU.Text)) { HOU.Text = "0"; }

            UpdateMultipy_MORAKHDAY_Value();

            if (HeaderIsValid())
            {
                CmdSaveRecord();
            }
            else { return; }

            GetComputeMandDayOff();
        }

        private void DA_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DA.Text)) { DA.Text = "0"; }

            UpdateMultipy_MORAKHDAY_Value();

            if (HeaderIsValid())
            {
                CmdSaveRecord();
            }
            else { return; }

            GetComputeMandDayOff();

            CorrectDayOffsForInvalidDateRange();
        }
        private void BTN_NEWDAYOFF_Click(object sender, RoutedEventArgs e)
        {
            AllowEdits = true;
            ClearFreshAll();
        }
        private void BTN_CHAP_Click(object sender, RoutedEventArgs e)
        {
            if (!NewRecord && HeaderIsValid() && IsSavedSuccess)
            {
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.SALARY.MORAKHASI.mrt");
                report.Load(pathreport);
                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;
                report["NUMBER_PARAM"] = IDNUM.Text;

                //PrinterSettings MyPrinterSettings = new PrinterSettings();
                //MyPrinterSettings.DefaultPageSettings.PaperSize = MyPrinterSettings.PaperSizes[1];
                //StiPrintProvider.PaperSizeForUsing = MyPrinterSettings.PaperSizes[1];

                //report.Pages[0].PaperSize = PaperKind.A5;
                //foreach (StiComponent component in report.Pages[0].Components)
                //{
                //    if (component is StiText)
                //    {
                //        StiText textComponent = (StiText)component;
                //        textComponent.Width = report.Pages[0].Width * 0.8;  // 80% of page width
                //        textComponent.Left = report.Pages[0].Width * 0.1;   // 10% left margin
                //    }
                //}
                //foreach (StiText text in report.GetComponents().OfType<StiText>())
                //{
                //    text.AutoWidth = true;
                //    text.CanGrow = true;
                //    text.CanShrink = true;
                //}
                //foreach (StiPage page in report.Pages)
                //{
                //    page.PaperSize = PaperKind.A5;
                //    page.Orientation = StiPageOrientation.Landscape;
                //    page.Margins = new StiMargins(1, 1, 1, 1);
                //}




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

                (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت


                var selectedItem = KINDM.SelectedItem;
                var propertyInfo = selectedItem.GetType().GetProperty(KINDM.DisplayMemberPath);
                if (propertyInfo != null)
                {
                    var displayValue = propertyInfo.GetValue(selectedItem, null);
                    (report.GetComponentByName("KIND_TEXT") as StiText).Text = displayValue.ToStringNullSafe();
                }

                (report.GetComponentByName("DA") as StiText).Text = DA.Text;
                (report.GetComponentByName("minu") as StiText).Text = minu.Text;
                (report.GetComponentByName("HOU") as StiText).Text = HOU.Text;

                (report.GetComponentByName("MMINU") as StiText).Text = MMINU.Text;
                (report.GetComponentByName("MHOU") as StiText).Text = MHOU.Text;
                (report.GetComponentByName("MDA") as StiText).Text = MDA.Text;


                //report.Render();
                //report.Show();
                //pathreport?.Dispose();

                new WINRPT(report, "مرخصی").Show();
            }

            //DoCmd.OpenReport "morakhasi", acPreview, , "IDNUM = " & Me.IDNUM
        }

        private void ClearFreshAll()
        {
            OKF.IsChecked = false; //تایید فاکتور
            NUMBER_TO_OPEN = null;

            IDNUM.Text = null;
            CODE.SelectedValue = null; CODE.Items.Refresh();
            OKF.IsChecked = false;
            MODATE.Text = null;
            KINDM.SelectedValue = 0; KINDM.Items.Refresh();
            MOLAH.Text = null;

            MODATE.Text = Tarikh.FullCurrentDate;
            MOSTDATE.Text = Tarikh.FullCurrentDate;
            MOENDATE.Text = Tarikh.FullCurrentDate;

            //Hours and Days ...
            MAND.Text = null;
            DA.Text = "0";
            HOU.Text = "0";
            minu.Text = "0";

            SEE_DAY.Text = null;
            SEE_HOUR.Text = null;
            SEE_MINUT.Text = null;

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            CODE.Focus();
        }
        private bool CmdSaveRecord()
        {
            try
            {
                if ((bool)Baseknow.LOCKFAP)
                {
                    if ((bool)!OKF.IsChecked)
                    {
                        OKF.IsChecked = true;
                    }
                }

                if (NewRecord)
                {
                    var _ID_ = dbms.DoGetDataSQL<int?>($@"INSERT INTO dbo.PMORAKH(CODE, MODATE, MORAKHDAY, MAND, KINDM, MOLAH, MOSTDATE, MOENDATE, OKF)
                                                          OUTPUT INSERTED.IDNUM
                                                          VALUES({CODE.SelectedValue},
                                                          {MODATE.Text.ToRawTarikh()} ,
                                                          {MORAKHDAY.Text.ToRawTarikh()} ,
                                                          {MAND.Text} ,
                                                          {KINDM.SelectedValue} ,
                                                          N'{MOLAH.Text}' ,
                                                          {MOSTDATE.Text.ToRawTarikh()} ,
                                                          {MOENDATE.Text.ToRawTarikh()} ,
                                                          {Convert.ToByte(OKF.IsChecked ?? false)}) ").FirstOrDefault();

                    if (_ID_ != null)
                    {
                        IDNUM.Text = _ID_.ToString();
                    }
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.PMORAKH
                                         SET CODE = {CODE.SelectedValue},
                                         MODATE = {MODATE.Text.ToRawTarikh()},
                                         MORAKHDAY = {MORAKHDAY.Text.ToRawTarikh()},
                                         MAND = {MAND.Text},
                                         KINDM = {KINDM.SelectedValue}, 
                                         MOLAH = N'{MOLAH.Text}', 
                                         MOSTDATE = {MOSTDATE.Text.ToRawTarikh()},
                                         MOENDATE = {MOENDATE.Text.ToRawTarikh()}
                                         WHERE IDNUM = {IDNUM.Text}");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این مرخصی دارای اطلاعات وابسته است و نمیتوان آنرا حذف کرد").ShowDialog();
                    return false;
                }
                if (ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات به دلیل تکراری بودن ثبت نمی شود , احتمالا تاریخ درخواست مرخصی تکراری است").ShowDialog();
                    return false;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره اطلاعات!").ShowDialog();
                return false;
            }

            return true;
        }

        private void Form_Current()
        {
            if (Convert.ToBoolean(OKF.IsChecked))
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
            }

            if (!NewRecord && Convert.ToInt64(IDNUM.Text) > 0)
            {
                Update_Day_Hour_Minut();

                CL_HESABDARI.LetSigneTick(this.GetType().Name, 37, (int)Baseknow.USERCOD, new WindowInteropHelper(this).Handle);

                AllowEdits = false;
            }
            else
            {
                AllowEdits = true;
            }

            ActivateChap();
        }

        private void ActivateChap()
        {
            if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
            {
                //چاپ برگه
                BTN_CHAP.IsEnabled = true;
            }
        }

        private void BTN_MORAKH_LST_Click(object sender, RoutedEventArgs e)
        {
            if (CODE.SelectedValue == null)
            {
                new Msgwin(false, "ابتدا پرنسل را انتخاب کنید سپس مجددا امتحان کنید").ShowDialog();
                return;
            }

            new WIN_MORAKH_LST(CODE.SelectedValue.ToString()).ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend && !IsSavedSuccess)
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

        private void MODATE_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MODATE.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
        private void MOSTDATE_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MOSTDATE.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
        private void MOENDATE_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MOENDATE.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private bool IsDateValid(string DATE_N)
        {
            string date_n_val = DATE_N.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }

            return true;
        }
        private void GetComputeMandDayOff(string? MAND_FROM_DB = null, bool JustSeeMandTemp = false)
        {
            if (CODE.SelectedValue == null)
            {
                return;
            }

            //MORAKHDAY_AfterUpdate
            int ho;
            int DD;
            int mi;
            //BTN_SAVE_Click(null, null);

            string _REQUEST_DATE_ = "";
            if (JustSeeMandTemp)
            {
                _REQUEST_DATE_ = Tarikh.FullCurrentDate;
            }
            else
            {
                _REQUEST_DATE_ = MODATE.Text.ToRawTarikh();
            }

            string _MAND_DAYOFF_ = "";
            if (!string.IsNullOrEmpty(MAND_FROM_DB))
            {
                _MAND_DAYOFF_ = MAND_FROM_DB;
            }
            else
            {
                _MAND_DAYOFF_ = Math.Truncate(CL_HESABDARI.MORAKH_MAND(Convert.ToInt64(_REQUEST_DATE_), Convert.ToInt64(CODE.SelectedValue))).ToString();
            }
            if (!JustSeeMandTemp)
            {
                MAND.Text = _MAND_DAYOFF_;
                if (MAND.Text == "110110110")
                {
                    MORAKHDAY.Text = "0";
                    MAND.Text = "0";
                }

                //Fix = Math.Truncate

                MDA.Text = Math.Truncate(Convert.ToDouble(MAND.Text) / SALMINUT_440).ToString();
                MHOU.Text = Math.Truncate((Convert.ToDouble(MAND.Text) / SALMINUT_440 - Convert.ToDouble(MDA.Text)) * SALMINUT_440 / HMINUT_60).ToString();
                MMINU.Text = Math.Truncate(((Convert.ToDouble(MAND.Text) / SALMINUT_440 - Convert.ToDouble(MDA.Text)) * SALMINUT_440 / HMINUT_60 - Convert.ToDouble(MHOU.Text)) * HMINUT_60).ToString();

            }
            else
            {
                SEE_DAY.Text = Math.Truncate(Convert.ToDouble(_MAND_DAYOFF_) / SALMINUT_440).ToString();
                SEE_HOUR.Text = Math.Truncate((Convert.ToDouble(_MAND_DAYOFF_) / SALMINUT_440 - Convert.ToDouble(SEE_DAY.Text)) * SALMINUT_440 / HMINUT_60).ToString();
                SEE_MINUT.Text = Math.Truncate(((Convert.ToDouble(_MAND_DAYOFF_) / SALMINUT_440 - Convert.ToDouble(SEE_DAY.Text)) * SALMINUT_440 / HMINUT_60 - Convert.ToDouble(SEE_HOUR.Text)) * HMINUT_60).ToString();
            }

        }
        private void CorrectDayOffsForInvalidDateRange(bool _DisplayMsg_ = true) //اصلاح برای روزهاي مرخصي با محدوده تاريخي
        {
            if (CODE.SelectedValue == null)
            {
                return;
            }

            int DD = (int)(CL_HESABDARI.DIFF(Convert.ToInt64(MOSTDATE.Text.ToRawTarikh()), Convert.ToInt64(MOENDATE.Text.ToRawTarikh())) + 1);
            if (Convert.ToInt32(DA.Text) != DD && Convert.ToInt32(KINDM.SelectedValue) != 2)
            {
                DA.Text = DD.ToStringNullSafe();
                UpdateMultipy_MORAKHDAY_Value();
                //new Msgwin(false, "تعداد روزهاي مرخصي با محدوده تاريخي داده شده تطبيق ندارد آن را اصلاح كردم!").ShowDialog();
                if (_DisplayMsg_)
                {
                    universControl.PopNotifyShowUp("تعداد روزهاي مرخصي با محدوده تاريخي داده شده تطبيق ندارد آن را اصلاح كردم!", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue);
                }

                GetComputeMandDayOff();
            }
        }
        private void UpdateMultipy_MORAKHDAY_Value() // آپدیت مرخصی به دقیقه
        {
            if (CODE.SelectedValue == null)
            {
                return;
            }

            MORAKHDAY.Text = Math.Truncate(Convert.ToDouble(DA.Text) * SALMINUT_440 + Convert.ToDouble(HOU.Text) * HMINUT_60 + Convert.ToDouble(minu.Text)).ToString();
        }
        private void Update_Day_Hour_Minut()
        {
            if (CODE.SelectedValue == null)
            {
                return;
            }

            DA.Text = Math.Truncate(Convert.ToDouble(MORAKHDAY.Text.ToRawTarikh()) / SALMINUT_440).ToString();
            HOU.Text = Math.Truncate((Convert.ToDouble(MORAKHDAY.Text.ToRawTarikh()) / SALMINUT_440 - Convert.ToDouble(DA.Text)) * SALMINUT_440 / HMINUT_60).ToString();
            minu.Text = Math.Truncate(((Convert.ToDouble(MORAKHDAY.Text.ToRawTarikh()) / SALMINUT_440 - Convert.ToDouble(DA.Text)) * SALMINUT_440 / HMINUT_60 - Convert.ToDouble(HOU.Text)) * HMINUT_60).ToString();
        }

        private void BTN_DAYOFFPOP_Click(object sender, RoutedEventArgs e)
        {
            MANDDAYOFF_POP.IsOpen = false;
        }
    }
}
