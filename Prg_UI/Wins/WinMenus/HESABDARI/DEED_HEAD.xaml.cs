using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.Checkha;
using Wins.WinOther;
using static Functions.DataGridClipboardManager;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;
using static Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;
using Msgwin = Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    public partial class DEED_HEAD : Window, ISearchableWindow
    {
        public double N_S_NUMBER { get; set; } = -1;
        public DEED_HEAD(double? _n_s_ = null, bool _isAutomasion_ = false)
        {
            if (_n_s_ != null && _n_s_ > 0)
            {
                N_S_NUMBER = (double)_n_s_;
                IsOpenedFromAutomation = _isAutomasion_;
            }
            InitializeComponent();
            this.Owner = PublicVRB.WINBASE;//#OWNER
            this.DataContext = this;
        }
        public bool IsOpenedFromAutomation { get; } = false;
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBalanceSanadOk())
            {
                return;
            }
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

        public ObservableCollection<DEED_DTL> SANAD_DATA { get; set; } = new ObservableCollection<DEED_DTL>();
        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public DEED_DTL? WAS_ROW_ITEM { get; private set; }

        public DEED_DTL? CURRENT_ITMES_ROW { get; private set; }

        public Visual I_AM_SANAD { get; set; }

        public Search_Model FROM_SEARCH { get; set; } = new Search_Model();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        // هنگام ورود سریع ردیف‌ها، هر بار کامیت سلول حساب/نام‌حساب یک رفت‌وبرگشت شبکه‌ای همزمان روی UI Thread
        // ایجاد می‌کرد که روی شبکه (برخلاف لوکال) باعث هنگ کوتاه دقیقا هنگام رسیدن فوکوس به ستون شرح می‌شد.
        // کش کردن نتیجه CUST_HESAB بر اساس کد حساب، رفت‌وبرگشت‌های تکراری برای حساب‌های تکراری را حذف می‌کند.
        private static readonly ConcurrentDictionary<string, CUST_HESAB?> _custHesabCache = new();

        private CUST_HESAB? GetCustHesabCached(string hes)
        {
            if (string.IsNullOrEmpty(hes)) return null;

            if (_custHesabCache.TryGetValue(hes, out var cached))
                return cached;

            var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + hes + "'").FirstOrDefault();
            _custHesabCache[hes] = data;
            return data;
        }

        public object ENTERED_VALUE_ROW { get; private set; }

        public int CURRENT_COLUMN_INDEX { get; private set; }

        public int CURRENT_ROW_INDEX { get; private set; }

        public int GHATEI { get; private set; }

        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();


        List<COMBOPERSONEL> rst_personel = null;

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                //if (string.IsNullOrEmpty(N_S.Text) || Convert.ToInt32(N_S.Text) == 0)
                //{
                //    _newrecord = true;
                //}
                //else
                //{
                //    _newrecord = false;
                //}
                return _newrecord;

            }
            set { _newrecord = value; }
        }
        public bool ChangeIsHappend { get; set; } = false;
        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        bool PERSONEL_First_Open = true;

        #region LOCALMODEL
        public class DEED_QR1
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public byte NUM { get; set; }
            public string? USER_NAME { get; set; }
            public int? IDD { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }

        public class DEED_QR2
        {
            public double? SumOfBED { get; set; }
            public double? SumOfBES { get; set; }
            public double? Expr1 { get; set; }
        }

        public class DEED_QR3
        {
            public bool? SND_TAHI { get; set; }
            public bool? SND_MALI { get; set; }
            public bool? SND_MODIR { get; set; }
        }

        public class DEED_QR4
        {
            public double? MaxOfN_S { get; set; }
        }

        public class DEED_QR5
        {
            public double? MaxOfN_S { get; set; }
        }
        #endregion

        private bool _ican;

        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                AllowAdditionEdits(_ican);
            }
        }

        private void AllowAdditionEdits(bool ican)
        {
            if (ican is true)
            {
                N_S.IsReadOnly = false;
                BASE.IsReadOnly = false;
                ESLAH.IsEnabled = true;
                //Command22.IsEnabled = true;
                //Command3.IsEnabled = true;
                Sanad_Status.IsReadOnly = false;
                NO_S.IsReadOnly = false;
                USER_NAME.IsReadOnly = false;
                DATE_S.IsReadOnly = false;
                SHARH_S.IsReadOnly = false;
                Child14.IsReadOnly = false;
                //SGN1.IsEnabled = true;
                //SGN2.IsEnabled = true;
                //SGN3.IsEnabled = true;
                PERSONEL.IsReadOnly = false;

                DATE_S.IsEnabled = true;
                SHARH_S.IsEnabled = true;
                SAVE_BTN.IsEnabled = true;
            }
            else
            {
                N_S.IsReadOnly = true;
                BASE.IsReadOnly = true;
                //ESLAH.IsEnabled = false;
                //Command22.IsEnabled = false;
                //Command3.IsEnabled = false;
                Sanad_Status.IsReadOnly = true;
                NO_S.IsReadOnly = true;
                USER_NAME.IsReadOnly = true;
                DATE_S.IsReadOnly = true;
                SHARH_S.IsReadOnly = true;
                Child14.IsReadOnly = true;
                sgn1usid.IsReadOnly = true;
                //SGN1.IsEnabled = false;
                //SGN2.IsEnabled = false;
                //SGN3.IsEnabled = false;
                sgn2usid.IsReadOnly = true;
                sgn3usid.IsReadOnly = true;
                PERSONEL.IsReadOnly = true;

                DATE_S.IsEnabled = false;
                SHARH_S.IsEnabled = false;
                SAVE_BTN.IsEnabled = false;
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

        public bool hn { get; private set; }
        public double Meidnum { get; private set; }
        public bool PLUS { get; private set; }
        public bool IsDataGrid_SUB_IsFocused { get; private set; }
        public bool IsPastingRows { get; private set; } = false;

        //--------------------------------------------------------------------------

        /// <summary>
        /// Background
        /// </summary>
        public Brush DEFAULT_BG_OKF { get; set; }
        /// <summary>
        /// BorderThickness
        /// </summary>
        public Thickness DEFAULT_BT_OKF { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_SANAD = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SANAD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            CL_HESABDARI.SETSECURITYSUB(Child14, "SANAD");

            FILL_ALL_COMBOBOXES();


            DATE_S.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            NO_S.Text = "0";
            DATE_S.IsEnabled = true;
            DEFAULT_BG_OKF = OKF.Background;
            DEFAULT_BT_OKF = OKF.BorderThickness;


            ReGetMasterData();

            #region Form_OnOpen
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                //this.OKF.DefaultValue = true;
            }
            else
            {
                //this.OKF.DefaultValue = false;
            }
            if (CL_HESABDARI.LETSGOUPDATE(this.GetType().Name, "SANAD", 3))
            {
                this.ESLAH.Visibility = Visibility.Visible;
            }
            else
            {
                this.ESLAH.Visibility = Visibility.Visible;
            }
            #endregion


            #region DataGrid_On_Open
            CL_HESABDARI.LETSGOUPDATE(this.GetType().Name, "Child14", 3);
            #endregion


            #region DataGrid_On_Load
            //ERROR
            //if (Strings.Mid(Baseknow.OPTIONSS, 43, 1) == "5")
            //{
            //    this.MHAZ_NO.ColumnHidden = false;
            //    this.MHAZ_NO.ColumnWidth = 3000;
            //}
            //else
            //{
            //    this.MHAZ_NO.ColumnWidth = 0;
            //}
            //if (Strings.Mid(Baseknow.OPTIONSS, 14, 1) == "5")
            //{
            //    this.ARZD.ColumnHidden = false;
            //    this.ARZD.ColumnWidth = 1000;
            //}
            //else
            //{
            //    this.ARZD.ColumnWidth = 0;
            //}

            //PLUS = false;
            #endregion

            Form_Current();

            if (N_S.Text is not null && N_S.Text != "")
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
            {
                Command22.IsEnabled = true;
                Command3.IsEnabled = true;
            }
            else
            {
                Command22.IsEnabled = false;
                Command3.IsEnabled = false;
            }


        }

        private void Form_Current()
        {
            if (string.IsNullOrEmpty(N_S.Text))
            {
                Child14.IsReadOnly = true;
                DATE_S.Focus();
            }
            else
            {
                Child14.IsReadOnly = false;
            }
            if (!IsNull(N_S.Text) && N_S.Text != "")
            {
                if (Printed(Convert.ToDouble(N_S.Text), 0))
                {
                    OKF.BorderThickness = new Thickness(1);
                    OKF.Background = new SolidColorBrush(Color.FromRgb(102, 35, 91)); // 6723891
                }
                else
                {
                    OKF.BorderThickness = DEFAULT_BT_OKF;
                    OKF.Background = DEFAULT_BG_OKF;
                }
            }
            else
            {
                OKF.BorderThickness = DEFAULT_BT_OKF;
                OKF.Background = DEFAULT_BG_OKF;
            }

            if (GHATEI != 0)
            {
                Child14.IsReadOnly = true;
                AllowDeletions = false;
                AllowEdits = false;
            }
            else
            {
                Child14.IsReadOnly = false;
                AllowDeletions = true;
                AllowEdits = true;
            }

            if (NO_S.Text != "0" && !string.IsNullOrEmpty(NO_S.Text) && GHATEI != 0)
            {
                AllowEdits = false;
                Child14.IsReadOnly = true;
            }
            else
            {
                AllowEdits = true;
                Child14.IsReadOnly = false;
                AllowDeletions = true;
                AllowEdits = true;
            }

            IsBalanceSanadOk();

            if (Convert.ToBoolean(Baseknow.SIGN))
            {
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
                SGN3.Visibility = Visibility.Visible;

                var rst = dbms.DoGetDataSQL<DEED_QR3>("SELECT SND_TAHI,SND_MALI,SND_MODIR FROM dbo.SIGN WHERE     USERCO = " + Baseknow.USERCOD).ToList();
                if (rst.Count > 0)
                {
                    if (Convert.ToBoolean(rst.FirstOrDefault().SND_TAHI))
                    {
                        SGN1.IsEnabled = true;
                    }
                    else
                    {
                        SGN1.IsEnabled = false;
                    }
                    if (Convert.ToBoolean(rst.FirstOrDefault().SND_MALI))
                    {
                        SGN2.IsEnabled = true;
                    }
                    else
                    {
                        SGN2.IsEnabled = false;
                    }
                    if (Convert.ToBoolean(rst.FirstOrDefault().SND_MODIR))
                    {
                        SGN3.IsEnabled = true;
                    }
                    else
                    {
                        SGN3.IsEnabled = false;
                    }
                }

                DATE_S.IsReadOnly = false;
                SHARH_S.IsReadOnly = false;

                if (Convert.ToBoolean(SGN3.IsChecked))
                {
                    SGN1.IsEnabled = false;
                    SGN2.IsEnabled = false;
                    AllowDeletions = false;
                    Child14.IsReadOnly = true;

                    DATE_S.IsReadOnly = false;
                    SHARH_S.IsReadOnly = false;
                }
                else if (Convert.ToBoolean(SGN2.IsChecked))
                {
                    SGN1.IsEnabled = false;
                    AllowDeletions = false;
                    Child14.IsReadOnly = true;

                    DATE_S.IsReadOnly = false;
                    SHARH_S.IsReadOnly = false;
                }
            }

            if (Convert.ToBoolean(OKF.IsChecked) && !NewRecord)
            {
                AllowDeletions = false;
                AllowEdits = false;
                Child14.IsReadOnly = true;
            }
            else
            {
                AllowDeletions = true;
                AllowEdits = true;
                Child14.IsReadOnly = false;
            }
            if (NO_S.Text == "0")
            {
                if (Convert.ToBoolean(CL_HESABDARI.LETSGOUPDATE(GetType().Name, "SANAD", 3)))
                {
                    ESLAH.IsEnabled = true;
                }
                else
                {
                    ESLAH.IsEnabled = false;
                }
            }
            else if (SANAD_DATA.Count == 0)
            {
                if (Convert.ToBoolean(CL_HESABDARI.LETSGOUPDATE(GetType().Name, "SANAD", 3)))
                {
                    ESLAH.IsEnabled = true;
                }
                else
                {
                    ESLAH.IsEnabled = false;
                }
            }
            else if (Convert.ToBoolean(CL_HESABDARI.LETSGOUPDATE(GetType().Name, "SANAD", 3)))
            {
                ESLAH.IsEnabled = true;
            }
            else
            {
                ESLAH.IsEnabled = false;
            }

            PERSONEL.Visibility = Visibility.Visible;

            if (CL_LMethods.IsNumeric(N_S.Text) && N_S.Text != "0") // N_S > 0
            {
                if (Convert.ToInt32(N_S.Text) > 0)
                {
                    CL_HESABDARI.LetSigneTick(GetType().Name, 0, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                }
            }
            else
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }
        }

        private bool IsBalanceSanadOk()
        {
            if (!IsNull(this.N_S.Text) && this.N_S.Text != "" && SANAD_DATA.Any())
            {
                var rst = dbms.DoGetDataSQL<DEED_QR2>("SELECT Sum(DEED_DTL.BED) AS SumOfBED, Sum(DEED_DTL.BES) AS SumOfBES, SUM(ROUND(BED - BES, 0)) AS Expr1 FROM DEED_DTL WHERE (((DEED_DTL.N_S)=" + this.N_S.Text + "))").ToList();
                if (rst.FirstOrDefault().Expr1 != 0 & rst.FirstOrDefault().SumOfBED != rst.FirstOrDefault().SumOfBES)
                {
                    Msgwin msgwin = new Msgwin(false, $"سند تراز نمي باشد.جمع بدهكار و بستانكار سند بايد مساوي  باشد.مبلغ اختلاف : {rst.FirstOrDefault().Expr1}");
                    msgwin.ShowDialog();
                    return false;
                }
            }
            return true;
        }

        string WhereLimitcondition = "";
        private void ReGetMasterData()
        {
            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereLimitcondition = $" WHERE N_S = {N_S_NUMBER} ";
            }

            var MasterHead = dbms.DoGetDataSQL<DEED_HED>($"SELECT N_S, DATE_S, SHARH_S, NO_S, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, BAYEG FROM dbo.DEED_HED {WhereLimitcondition} ORDER BY N_S").ToList();
            RecordsData.Source = MasterHead;

            if (N_S_NUMBER > 0) //Opened by number (N_S)
            {
                // Filter the items to find the one with N_S equal to N_S_NUMBER
                var item = RecordsData.View.Cast<DEED_HED>().FirstOrDefault(x => x.N_S == N_S_NUMBER);
                if (item != null)
                {
                    // Set the CurrentItem to the found item
                    RecordsData.View.MoveCurrentTo(item);

                    MoveReGetData(Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                }
            }
            else
            {
                MoveReGetData(Jahat.LastItem);
            }

        }
        private void MoveReGetData(Jahat jahat, int? custom_postiion = null)
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

            if ((ChangeIsHappend || NewRecord) && !ConfirmExitWithoutSaving())
            {
                return;
            }

            switch (jahat)
            {
                case Jahat.FirstItem: //اولین
                    NewRecord = false;
                    RecordsData.View.MoveCurrentToFirst();
                    break;
                case Jahat.BackItem: //قبلی
                    if (RecordsData.View.CurrentPosition > 0) //Possible To Back
                    {
                        if (NewRecord)
                        {
                            jahat = Jahat.LastItem;
                            RecordsData.View.MoveCurrentToLast();
                        }
                        else
                        {
                            RecordsData.View.MoveCurrentToPrevious();
                        }
                        NewRecord = false;
                    }
                    break;

                case Jahat.NextItem: //بعدی
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1)
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToNext();
                    }
                    break;

                case Jahat.LastItem: //آخرین
                    RecordsData.View.MoveCurrentToLast();
                    break;

                case Jahat.CustomPosition:
                    if (custom_postiion > -1)
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToPosition((int)custom_postiion);
                    }
                    break;

                case Jahat.NewItem: //جدید خالی
                    NewRecord = true;
                    RecordsData.View.MoveCurrentToLast();
                    ClearFreshNew();
                    Form_Current();
                    break;
            }

            //Update CurrentViewItem
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as DEED_HED;
                var DBData = dbms.DoGetDataSQL<DEED_HED>($"SELECT TOP 1 N_S, DATE_S, SHARH_S, NO_S, ANBAR, N_FACTOR, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, CRT, UID, BAYEG FROM dbo.DEED_HED WHERE N_S = {HEADER.N_S}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(DEED_HED).GetProperties();
                    foreach (var property in properties)
                    {
                        if (property.CanWrite)
                        {
                            var value = property.GetValue(DBData);
                            property.SetValue(HEADER, value);
                        }
                    }
                    GHATEI = Convert.ToByte(HEADER.GHATEI);

                    RecordsData.View.Refresh();
                }
            }

            DisplayCounts();

            UiDataUpdate(jahat);

            Form_Current();

            if (jahat == Jahat.NewItem)
            {
                ClearFreshNew();
            }

            ChangeIsHappend = false; // Reset it
        }
        private void UiDataUpdate(Jahat jahat)
        {
            if (RecordsData.View?.CurrentItem is not null && jahat != Jahat.NewItem) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as DEED_HED;

                N_S.Text = HEADER.N_S.ToStringNullSafe();
                DATE_S.Text = HEADER.DATE_S.ToStringNullSafe();
                BASE.Text = HEADER.@base.ToStringNullSafe();
                BAYEG.Text = HEADER.BAYEG.ToStringNullSafe();
                SHARH_S.Text = HEADER.SHARH_S.ToStringNullSafe();

                OKF.IsChecked = Convert.ToBoolean(HEADER.OKF);
                NO_S.Text = HEADER.NO_S.ToStringNullSafe();
                USER_NAME.Text = HEADER.USER_NAME.ToStringNullSafe();

                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3);

                sgn1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                sgn2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                sgn3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);

                sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn1usid)?.SAL_NAME;
                sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                ReGetData(); //Load DataGrid's data

            }
        }
        private bool ConfirmExitWithoutSaving()
        {
            Msgwin msgwin = new Msgwin(true, "آیتم جدید را ذخیره نکرده اید , آیا از خروج از این آیتم اطمینان دارید ؟");
            msgwin.ShowDialog();
            return msgwin.DialogResult == true;
        }
        public void RefreshAfterInsert()
        {
            var itemtoadd = dbms.DoGetDataSQL<DEED_HED>($"SELECT N_S, DATE_S, SHARH_S, NO_S, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, BAYEG FROM dbo.DEED_HED WHERE N_S={N_S.Text}").FirstOrDefault();

            var underlyingCollection = RecordsData.Source as List<DEED_HED>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                //MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
            }
        }

        public bool Printed(double num, int tg, int no = 0)
        {
            bool PrintedRet = default;
            var rst = dbms.DoGetDataSQL<DEED_QR1>("SELECT * FROM CHAPNUM WHERE NUMBER = " + num + " AND  tag = " + tg).ToList();
            if (rst.Count > 0)
            {
                PrintedRet = true;
            }
            else
            {
                PrintedRet = false;
            }

            return PrintedRet;
        }

        private void DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE.Visibility == Visibility.Visible;
            if (!DELETE.IsEnabled || !IsVisible) { return; }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (Child14.IsEnabled == false || Child14.IsReadOnly)
            {
                universControl.PopNotifyShow("ابتدا دکمه اصلاح را بزنید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            var editableCollectionView = Child14.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem)
            {
                editableCollectionView.CommitEdit();
            }

            _ = AuditLogger.LogActionAsync(
               actionType: "DELETE",
               tableName: "سطر صدور و ویرایش اسناد",
               recordId: N_S.Text,
               oldValue: null,
               newValue: null,
               additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            //if (Child14.Items.Count > 0 && Child14.SelectedItems != null && Child14.SelectedItems.Count > 0)
            if (SANAD_DATA.Count > 0)
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    CL_HESABDARI.TR("DEED_HED", $"(N_S = {N_S.Text})", DateTime.Now, 1);
                    CL_HESABDARI.TR("DEED_DTL", $"(N_S = {N_S.Text})", DateTime.Now, 1);



                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < Child14.SelectedItems.Count; i++)
                    {
                        var item = Child14.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(Child14, item)) { continue; }

                        var _id_ = item.GetType().GetProperty("id").GetValue(item);
                        var _N_S_ = item.GetType().GetProperty("N_S").GetValue(item);

                        if (_id_ == null)
                        {
                            if (item != null)
                            {
                                var deedItem = item as DEED_DTL;
                                if (deedItem != null)
                                {
                                    SANAD_DATA.Remove(deedItem);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                dbms.DoExecuteSQL($"DELETE FROM DEED_DTL WHERE N_S = {_N_S_} AND id = {_id_}");
                            }
                            catch (SqlException ex) when (ex.Number == 547)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این سطر سند , حساب : {(item as DEED_DTL).NAME_HES} و بدهکار : {(item as DEED_DTL).BED} و بستانکار {(item as DEED_DTL).BED} دارای گردش است و نمیتوان آنرا حذف کرد !" });
                            }
                            catch (SqlException)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این سطر سند , حساب : {(item as DEED_DTL).NAME_HES} و بدهکار : {(item as DEED_DTL).BED} و بستانکار {(item as DEED_DTL).BED} به دلیل بروز خطا در پایگاه داده حذف انجام نشد  !" });
                            }
                            catch
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این سطر سند , حساب : {(item as DEED_DTL).NAME_HES} و بدهکار : {(item as DEED_DTL).BED} و بستانکار {(item as DEED_DTL).BED} خطا در انجام علمیات حذف !" });
                            }
                        }
                    }

                    if (ErrosMessages.Any())
                    {
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();
                    }

                    ReGetData();

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.DEED_HED WHERE N_S = {N_S.Text}");
                    }
                    catch (SqlException ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }

                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این سند دارای اطلاعات وابسته است و نمی توان آنرا حذف کرد").ShowDialog();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }
                        new Msgwin(false, "خطا در انجام عملیات حذف این سند").ShowDialog();
                        return;
                    }
                    ReGetData();
                    RefreshAfterDelete();
                    ClearFreshNew();
                }
            }
        }

        private void SAVE_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBalanceSanadOk())
            {
                return;
            }

            if (DATE_VALIDATE(true) == false)
            {
                return;
            }


            #region After_Update
            //Was in DATE_S_LostFocus , but not now
            if (IsNull(this.N_S.Text) || this.N_S.Text == "" || this.N_S.Text == "0")
            {
                var rst = dbms.DoGetDataSQL<DEED_QR5>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED HAVING ((Not (Max(DEED_HED.N_S)) Is Null))").ToList();
                if (rst.Count > 0)
                {
                    this.N_S.Text = Convert.ToString(rst.FirstOrDefault().MaxOfN_S + 1);
                }
                else
                {
                    this.N_S.Text = "1";
                }
            }
            #endregion

            bool IsInsertHappend = false;

            var SanadHeadRow = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 N_S FROM dbo.DEED_HED WHERE N_S = {N_S.Text}").ToList();
            if (N_S.Text is not null && N_S.Text != "0" && NO_S.Text is not null && !string.IsNullOrEmpty(DATE_S.Text.ToRawTarikh()))
            {
                if (SanadHeadRow.Count == 0)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_HED (      N_S ,  DATE_S                  ,  SHARH_S ,       NO_S , GHATEI ,                        USER_NAME ,                             SGN1 ,                              SGN2 ,                             SGN3  , OKF ,UID , CRT)
				                                    VALUES({N_S.Text} , {DATE_S.Text.ToRawTarikh()} , N'{SHARH_S.Text.FixPersianChars()}' , {(NO_S.Text == "" ? "NULL" : NO_S.Text)} , 0.0     ,N'{CL_HESABDARI.UCurrentUser()}' , {Convert.ToByte(SGN1.IsChecked)} , {Convert.ToByte(SGN2.IsChecked)}  , {Convert.ToByte(SGN3.IsChecked)}  ,{Convert.ToByte(OKF.IsChecked)} , {Baseknow.USERCOD} , GETDATE())");

                    IsInsertHappend = true;

                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED 
                    SET DATE_S = {DATE_S.Text.ToRawTarikh()}, 
                        NO_S = {(NO_S.Text == "" ? "NULL" : NO_S.Text)}, 
                        SGN1 = {Convert.ToByte(SGN1.IsChecked)}, 
                        SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                        SGN3 = {Convert.ToByte(SGN3.IsChecked)}, 
                        SHARH_S =  N'{SHARH_S.Text.FixPersianChars()}' , 
                        OKF = {Convert.ToByte(OKF.IsChecked)}
                    WHERE N_S = {N_S.Text}");
                }
            }

            //After_Update 
            if (IsNull(this.N_S.Text))
            {
                this.Child14.IsReadOnly = true;
                this.N_S.Focus();
            }
            else
            {
                this.Child14.IsReadOnly = false;
            }

            #region Form_After_Update
            if (Convert.ToInt32(this.N_S.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 0, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            if (string.IsNullOrEmpty(BAYEG.Text) || BAYEG.Text == "0")
            {
                //BAYEG.Text = CL_HESABDARI.UpdateOrGenerateBAYEG(100000000, Convert.ToInt32(N_S.Text)).ToString();
                BAYEG.Text = CL_HESABDARI.UpdateOrGenerateBAYEG(100000000, Convert.ToInt32(N_S.Text)).ToString();
            }

            BASE.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 BASE FROM DEED_HED WHERE N_S = {N_S.Text}").FirstOrDefault();

            //این خط به دلیل وجود مشکلات همزمانی غیر فعال شد و به جای این از متد بالا  استفاده میشود UpdateOrGenerateBAYEG
            //BAYEG.Text = dbms.DoGetDataSQL<int?>("UPDATE dbo.DEED_HED SET BAYEG = 100000000 + BASE OUTPUT INSERTED.BAYEG WHERE BAYEG IS NULL").FirstOrDefault()?.ToString();
            //if (string.IsNullOrEmpty(BAYEG.Text))
            //{
            //    BAYEG.Text = Convert.ToString(100000000 + Convert.ToInt32(string.IsNullOrEmpty(BASE.Text) ? "1" : BASE.Text));
            //    //this.BAYEG.Requery;
            //}
            //dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET BAYEG = {BAYEG.Text} WHERE N_S = {N_S.Text}");
            #endregion

            if (IsInsertHappend)
            {
                RefreshAfterInsert();
            }

            var col_index = Child14.Columns.FirstOrDefault(c => c.SortMemberPath == "HES").DisplayIndex;
            Child14.SelectedIndex = Child14.Items.Count - 1;
            Child14.CurrentCell = new DataGridCellInfo(Child14.SelectedItem, Child14.Columns[col_index]);
            //

            if (SanadHeadRow.Count == 0 && SANAD_DATA.Count == 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Child14.BeginEdit();

                }), DispatcherPriority.Background);
            }

            universControl.PopNotifyShow(".ذخیره سربرگ انجام شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            ChangeIsHappend = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (!IsBalanceSanadOk())
            //{
            //    return;
            //}

            #region Form_Unload
            if ((N_S.Text is not null && N_S.Text != "") && (NO_S.Text is not null && NO_S.Text != ""))
            {
                if (Convert.ToInt32(this.NO_S.Text) == 0)
                {
                    var rst = dbms.DoGetDataSQL<DEED_QR2>("SELECT Sum(DEED_DTL.BED) AS SumOfBED, Sum(DEED_DTL.BES) AS SumOfBES, SUM(ROUND(BED - BES, 0)) AS Expr1 FROM DEED_DTL WHERE (((DEED_DTL.N_S)=" + this.N_S.Text + "))").ToList();
                    if (rst.FirstOrDefault().Expr1 != 0 && rst.FirstOrDefault().SumOfBED != rst.FirstOrDefault().SumOfBES)
                    {
                        //ERROR
                        //DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "سند تراز نمي باشد.جمع بدهكار و بستانكار سند بايد مساوي  باشد.مبلغ اختلاف :  " + rst.FirstOrDefault().SumOfBED != rst.FirstOrDefault().SumOfBES));
                        Msgwin msgwin = new Msgwin(false, $"سند تراز نمي باشد.جمع بدهكار و بستانكار سند بايد مساوي  باشد.مبلغ اختلاف : {rst.FirstOrDefault().SumOfBED - rst.FirstOrDefault().SumOfBES}");
                    }
                }
            }
            #endregion

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

        private void Child14_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Child14_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            Child14.Dispatcher.InvokeAsync(() =>
            {
                Child14.CellEditEnding -= Child14_CellEditEnding;
                Child14.RowEditEnding -= Child14_RowEditEnding;

                IsSaveSuccess = false;

                if (_RC_ is null)
                {
                    Child14.CancelEdit();
                }
                else
                {
                    Child14.CancelEdit((DataGridEditingUnit)_RC_);
                }
                Child14.RowEditEnding += Child14_RowEditEnding;
                Child14.CellEditEnding += Child14_CellEditEnding;
            });
        }

        private void ClearFreshNew()
        {
            ChangeIsHappend = false;

            AllowEdits = true;

            SANAD_DATA?.Clear();

            Child14.IsReadOnly = true;
            N_S.Text = null;
            DATE_S.Text = null;
            BASE.Text = null;
            BAYEG.Text = null;
            SHARH_S.Text = null;
            Text8.Text = null;
            Text10.Text = null;
            bedt.Text = null;
            best.Text = null;
            SSBED.Text = null;
            SSBES.Text = null;
            sgn1usid.Text = null; sgn1usid.Tag = null; SGN1.IsChecked = false;
            sgn2usid.Text = null; sgn2usid.Tag = null; SGN2.IsChecked = false;
            sgn3usid.Text = null; sgn3usid.Tag = null; SGN3.IsChecked = false;

            USER_NAME.Text = Baseknow.UUSER;
            NO_S.Text = "0";


            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            GHATEI = 0;

            DATE_S.Focus(); DATE_S.SelectAll();
        }
        public void RefreshAfterDelete()
        {
            var LastCurrentPosition = RecordsData.View.CurrentPosition;

            if (RecordsData.View.CurrentItem != null)
            {
                var itemToRemove = RecordsData.View.CurrentItem as DEED_HED;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<DEED_HED>;
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
        private void ReGetData()
        {
            SSBED.Text = SANAD_DATA.Sum(i => i.BED).ToString();
            SSBES.Text = SANAD_DATA.Sum(i => i.BES).ToString();

            SANAD_DATA?.Clear();
            if (N_S.Text is not null && N_S.Text != "")
            {
                if (Convert.ToInt32(N_S.Text) > 0)
                {
                    //var Sanaddata = dbms.DoGetDataSQL<DEED_DTL>($"SELECT N_S, RADIF, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, NUMBER, TAG, HES, id, ARZD, MHAZ_NO, HES_T2, HES_T3, HES_T4, CRT, UID FROM DEED_DTL WHERE N_S = {N_S.Text}").ToList();
                    var Sanaddata = dbms.DoGetDataSQL<DEED_DTL>($@"
SELECT dd.N_S, dd.RADIF, dd.HES_K, dd.HES_M, dd.HES_T, dd.SHARH, dd.BED, dd.BES, dd.N_SERI, dd.BANK, dd.NUMBER, dd.TAG, dd.HES, dd.id, dd.ARZD, dd.MHAZ_NO, dd.HES_T2, dd.HES_T3, dd.HES_T4, dd.CRT, dd.UID,
       ch.NAME AS NAME_HES
FROM dbo.DEED_DTL dd WITH (INDEX(N_SI))
LEFT JOIN dbo.CUST_HESAB ch ON ch.hes = dd.HES
WHERE dd.N_S = {N_S.Text}").ToList(); if (Sanaddata.Count > 0)
                    {
                        for (int i = 0; i < Sanaddata.Count; i++)
                        {
                            SANAD_DATA.Add(Sanaddata[i]);
                        }

                        //foreach (var item in Sanaddata)
                        //{
                        //    SANAD_DATA.Add(item);
                        //}


                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

        }

        private void CommitInProgressEdits()
        {
            try
            {
                var editableCollectionView = Child14.Items as IEditableCollectionView;
                if (editableCollectionView == null)
                {
                    return;
                }

                if (editableCollectionView.IsEditingItem)
                {
                    editableCollectionView.CommitEdit();
                }

                if (editableCollectionView.IsAddingNew)
                {
                    Child14.CancelEdit(DataGridEditingUnit.Row);
                    editableCollectionView.CancelNew();
                }
            }
            catch (Exception)
            {
                try
                {
                    Child14.CancelEdit(DataGridEditingUnit.Cell);
                    Child14.CancelEdit(DataGridEditingUnit.Row);

                    var editableCollectionView = Child14.Items as IEditableCollectionView;
                    if (editableCollectionView != null)
                    {
                        if (editableCollectionView.IsAddingNew)
                        {
                            editableCollectionView.CancelNew();
                        }

                        if (editableCollectionView.IsEditingItem)
                        {
                            editableCollectionView.CancelEdit();
                        }
                    }
                }
                catch { }

                // Ignore any error during commit as we want to proceed to ReadOnly mode anyway
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
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

            //کمبوباکس ارجاع
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";
        }

        private DataGridCellInfo? _editingCellInfo;
        private void Child14_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }

            #region NEED
            ComboBox Comboval = null;
            TextBox TexboVal = null;
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
            else if (TexboVal != null)
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();
            else
                ENTERED_VALUE_ROW = null;

            CURRENT_ITMES_ROW = e.Row.Item as DEED_DTL;

            if (e.Column != null)
            {
                _editingCellInfo = new DataGridCellInfo(e.Row.Item, e.Column);
            }
            #endregion

            if (e.Column.SortMemberPath == "HES")
            {
                if (CURRENT_ITMES_ROW.HES == "-" || CURRENT_ITMES_ROW.HES == "+")
                {
                    ComboSearch CMBSearch = new ComboSearch("DEED_HEAD", I_AM_SANAD);
                    CMBSearch.ShowDialog();

                    if (FROM_SEARCH.HES is not null)
                    {
                        CURRENT_ITMES_ROW.HES = FROM_SEARCH.HES;
                        CURRENT_ITMES_ROW.NAME_HES = FROM_SEARCH.NAME;

                        #region HES_After_Update
                        double? KOL = null, MOIN = null, taf = null;
                        double? TAF2 = null;
                        double? taf3 = null;
                        double? taf4 = null;

                        if (!IsNull(CURRENT_ITMES_ROW.HES))
                        {
                            CURRENT_ITMES_ROW.HES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(CURRENT_ITMES_ROW.HES));
                            CURRENT_ITMES_ROW.HES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(CURRENT_ITMES_ROW.HES));
                            CURRENT_ITMES_ROW.HES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(CURRENT_ITMES_ROW.HES));
                            CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.HES, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                            CURRENT_ITMES_ROW.HES_T2 = (int?)TAF2;
                            CURRENT_ITMES_ROW.HES_T3 = (int?)taf3;
                            CURRENT_ITMES_ROW.HES_T4 = (int?)taf4;
                            // Me.HES_T2 = IIf(GETTAF2(Me.HES) = -1, Null, GETTAF2(Me.HES))
                        }
                        #endregion
                    }
                    else
                    {
                        CURRENT_ITMES_ROW.HES = null;
                        CURRENT_ITMES_ROW.NAME_HES = null;

                        if (ENTERED_VALUE_ROW == "" || ENTERED_VALUE_ROW is null)
                        {
                            universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                            Child14_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                    }
                    FROM_SEARCH.HES = null;
                    FROM_SEARCH.NAME = null;


                }
                else
                {
                    if (WAS_ROW_ITEM?.HES != ENTERED_VALUE_ROW.ToStringNullSafe())
                    {

                        //CL_HESAB_SEARCH.Go_Search_Hesab(ENTERED_VALUE_ROW.ToString(), "DEED_HEAD", I_AM_SANAD);
                        var data = GetCustHesabCached(ENTERED_VALUE_ROW.ToStringNullSafe());
                        if (data is not null && !string.IsNullOrEmpty(data.hes))
                        {
                            CURRENT_ITMES_ROW.HES = data.hes;
                            CURRENT_ITMES_ROW.NAME_HES = data.NAME;


                            if (!IsNull(CURRENT_ITMES_ROW.HES))
                            {
                                double? KOL = null, MOIN = null, taf = null;
                                double? TAF2 = null;
                                double? taf3 = null;
                                double? taf4 = null;

                                CURRENT_ITMES_ROW.HES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(CURRENT_ITMES_ROW.HES));
                                CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.HES, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                                CURRENT_ITMES_ROW.HES_T2 = (int?)TAF2;
                                CURRENT_ITMES_ROW.HES_T3 = (int?)taf3;
                                CURRENT_ITMES_ROW.HES_T4 = (int?)taf4;
                                // Me.HES_T2 = IIf(GETTAF2(Me.HES) = -1, Null, GETTAF2(Me.HES))
                            }
                        }
                        else
                        {
                            CURRENT_ITMES_ROW.HES = null;
                            CURRENT_ITMES_ROW.NAME_HES = null;

                            if (!e.Row.IsNewItem && string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                            {
                                Child14_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                            }
                            return;
                        }

                    }

                }
            }

            if (e.Column.SortMemberPath == "NAME_HES")
            {
                //CURRENT_ITMES_ROW.NAME_HES = ENTERED_VALUE_ROW.ToStringNullSafe();
                if (CURRENT_ITMES_ROW.HES is null && CURRENT_ITMES_ROW.NAME_HES is null)
                {
                    #region Not_In_List

                    if (CURRENT_ITMES_ROW.NAME_HES == "-" || CURRENT_ITMES_ROW.NAME_HES == "+")
                    {
                        ComboSearch CMBSearch = new ComboSearch("DEED_HEAD", I_AM_SANAD);
                        CMBSearch.ShowDialog();

                        if (FROM_SEARCH.HES is not null)
                        {
                            CURRENT_ITMES_ROW.HES = FROM_SEARCH.HES;
                            CURRENT_ITMES_ROW.NAME_HES = FROM_SEARCH.NAME;

                            #region HES_After_Update
                            double? KOL = null, MOIN = null, taf = null;
                            double? TAF2 = null;
                            double? taf3 = null;
                            double? taf4 = null;

                            if (!IsNull(CURRENT_ITMES_ROW.HES))
                            {
                                CURRENT_ITMES_ROW.HES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(CURRENT_ITMES_ROW.HES));
                                CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.HES, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                                CURRENT_ITMES_ROW.HES_T2 = (int?)TAF2;
                                CURRENT_ITMES_ROW.HES_T3 = (int?)taf3;
                                CURRENT_ITMES_ROW.HES_T4 = (int?)taf4;
                                // Me.HES_T2 = IIf(GETTAF2(Me.HES) = -1, Null, GETTAF2(Me.HES))
                            }
                            #endregion
                        }
                        else
                        {
                            CURRENT_ITMES_ROW.HES = null;
                            CURRENT_ITMES_ROW.NAME_HES = null;
                            if (ENTERED_VALUE_ROW != "" && ENTERED_VALUE_ROW is not null)
                            {
                                universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                            }
                        }
                        FROM_SEARCH.HES = null;
                        FROM_SEARCH.NAME = null;


                    }
                    else if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                    {
                        CL_HESAB_SEARCH.Go_Search_Hesab(ENTERED_VALUE_ROW.ToString(), "DEED_HEAD", I_AM_SANAD);

                        if (FROM_SEARCH.HES is not null)
                        {
                            CURRENT_ITMES_ROW.HES = FROM_SEARCH.HES;
                            CURRENT_ITMES_ROW.NAME_HES = FROM_SEARCH.NAME;

                            #region HES_After_Update
                            double? KOL = null, MOIN = null, taf = null;
                            double? TAF2 = null;
                            double? taf3 = null;
                            double? taf4 = null;

                            if (!IsNull(CURRENT_ITMES_ROW.HES))
                            {
                                CURRENT_ITMES_ROW.HES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(CURRENT_ITMES_ROW.HES));
                                CURRENT_ITMES_ROW.HES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(CURRENT_ITMES_ROW.HES));
                                CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.HES, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                                CURRENT_ITMES_ROW.HES_T2 = (int?)TAF2;
                                CURRENT_ITMES_ROW.HES_T3 = (int?)taf3;
                                CURRENT_ITMES_ROW.HES_T4 = (int?)taf4;
                                // Me.HES_T2 = IIf(GETTAF2(Me.HES) = -1, Null, GETTAF2(Me.HES))
                            }
                            #endregion
                        }
                        else
                        {
                            CURRENT_ITMES_ROW.HES = null;
                            CURRENT_ITMES_ROW.NAME_HES = null;
                            if (ENTERED_VALUE_ROW != "" && ENTERED_VALUE_ROW is not null)
                            {
                                universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                            }

                        }
                        FROM_SEARCH.HES = null;
                        FROM_SEARCH.NAME = null;
                    }
                    if (CURRENT_ITMES_ROW.NAME_HES == "=")
                    {
                        //DoCmd.OpenForm("tota_hes_sheet", acFormDS, default, default, acFormReadOnly, default, "1");
                        //PLUS = true;
                    }
                    #endregion
                }
                if (CURRENT_ITMES_ROW.HES is not null && CURRENT_ITMES_ROW.HES != "")
                {
                    CURRENT_ITMES_ROW.NAME_HES = GetCustHesabCached(CURRENT_ITMES_ROW.HES)?.NAME;
                }
            }

            if (e.Column.SortMemberPath == "BED")
            {
                #region On_Exit

                if ((CURRENT_ITMES_ROW.HES == Baseknow.ADA || CURRENT_ITMES_ROW.HES == Baseknow.ADV) && CURRENT_ITMES_ROW.BED > 0)
                {
                    string _serverfilter = "";
                    if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                    {
                        CURRENT_ITMES_ROW.N_SERI = 0;
                        CURRENT_ITMES_ROW.BANK = 0;
                    }


                    SGETCHEK sGETCHEK = new SGETCHEK(I_AM_SANAD, CURRENT_ITMES_ROW.BED.ToString(), CURRENT_ROW_INDEX);
                    sGETCHEK.ShowDialog();


                    if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                    {
                        CURRENT_ITMES_ROW.N_SERI = null;
                        CURRENT_ITMES_ROW.BANK = null;
                    }
                }

                #endregion

                if (CURRENT_ITMES_ROW.BES is not null && CURRENT_ITMES_ROW.BED is not null)
                {
                    if (CURRENT_ITMES_ROW.BES > 0 && CURRENT_ITMES_ROW.BED > 0)
                    {
                        //Child14_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        //CURRENT_ITMES_ROW.BED = WAS_ROW_ITEM?.BED;
                        universControl.PopNotifyShow("بدهكار و بستانكار سند صحيح نمي باشد", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }

            if (e.Column.SortMemberPath == "BES")
            {
                #region On_Exit

                if ((CURRENT_ITMES_ROW.HES == Baseknow.APA || CURRENT_ITMES_ROW.HES == Baseknow.APV) && CURRENT_ITMES_ROW.BES > 0)
                {
                    string _serverfilter = "";
                    if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                    {
                        CURRENT_ITMES_ROW.N_SERI = 0;
                        CURRENT_ITMES_ROW.BANK = 0;
                        _serverfilter = "";
                    }
                    else
                    {

                        _serverfilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.BES;
                    }

                    //DoCmd.OpenForm("SPAYCHEK", acNormal, default, "N_SERI = " + this.N_SERI + " AND BANK = " + this.BANK, default, acDialog);
                    SPAYCHEK sPAYCHEK = new SPAYCHEK(_serverfilter, I_AM_SANAD, CURRENT_ITMES_ROW.BES.ToString(), CURRENT_ROW_INDEX);
                    sPAYCHEK.ShowDialog();

                    if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                    {
                        CURRENT_ITMES_ROW.N_SERI = null;
                        CURRENT_ITMES_ROW.BANK = null;
                    }
                }
                #endregion

                if (CURRENT_ITMES_ROW.BES is not null && CURRENT_ITMES_ROW.BED is not null)
                {
                    if (CURRENT_ITMES_ROW.BES > 0 && CURRENT_ITMES_ROW.BED > 0 || CURRENT_ITMES_ROW.BES == 0 && CURRENT_ITMES_ROW.BED == 0)
                    {
                        //CURRENT_ITMES_ROW.BES = WAS_ROW_ITEM?.BES;
                        //Child14_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        //RestoreFocusCell(e);
                        universControl.PopNotifyShow("بدهكار و بستانكار سند صحيح نمي باشد", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }

        }
        private void RestoreFocusCell(DataGridCellEditEndingEventArgs e)
        {
            try
            {
                e.Cancel = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Child14.CurrentCell = _editingCellInfo.Value;
                    Child14.BeginEdit();
                    if (e.EditingElement is TextBox tb)
                    {
                        tb.SelectAll();
                    }
                }), DispatcherPriority.Background);
            }
            catch { }
        }

        bool IsSaveSuccess = true;
        private void Child14_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }

            var ROW = e.Row.Item as DEED_DTL;
            if (ConstructorRowDetector.IsPristine(ROW)) { Child14_CANCEL_EDIT(); return; }
            if (ROW is null) { return; }


            IsSaveSuccess = false;

            #region Validation

            //var TheCurrentRow = e.Row.Item as DEED_DTL;
            //if (TheCurrentRow?.HES_K == null)
            //{
            //    universControl.PopNotifyShow("کد حساب کل به صورت مجزا خالی است , برای رفع این مشکل مجددا کد حساب را وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
            //    Child14_CANCEL_EDIT();
            //    return;
            //}
            //else if (TheCurrentRow?.HES_M == null)
            //{
            //    universControl.PopNotifyShow("کد حساب معین به صورت مجزا خالی است , برای رفع این مشکل مجددا کد حساب را وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
            //    Child14_CANCEL_EDIT();
            //    return;
            //}
            //else if (TheCurrentRow?.HES_T == null)
            //{
            //    universControl.PopNotifyShow("کد حساب تفضیلی به صورت مجزا خالی است , برای رفع این مشکل مجددا کد حساب را وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
            //    Child14_CANCEL_EDIT();
            //    return;

            //}
            if (ROW.HES_K == null)
            {
                universControl.PopNotifyShow("حساب کل نمی‌تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                Child14_CANCEL_EDIT();
                return;
            }
            if (ROW?.SHARH?.Length > 250)
            {
                universControl.PopNotifyShow("طول شرح حداکثر میتواند 250 کاراکتر باشد.", Pop1, Pop1Text1, Pop_Border1);

                Child14_CANCEL_EDIT();
                return;
            }

            if (ROW?.NAME_HES is null || ROW.HES is null)
            {
                universControl.PopNotifyShow("حساب به درستی انتخاب نشده.", Pop1, Pop1Text1, Pop_Border1);

                Child14_CANCEL_EDIT();
                return;
            }

            if (ROW.BED == 0 && ROW.BES == 0)
            {
                universControl.PopNotifyShow("بدهکار یا بستانکار نمیتواند خالی باشد !.", Pop1, Pop1Text1, Pop_Border1);

                Child14_CANCEL_EDIT();
                return;
            }
            #endregion

            #region DataGrid_Before_Update
            //Check Where Should Use
            if (!hn)
            {
                if (CURRENT_ITMES_ROW.BES is not null && CURRENT_ITMES_ROW.BED is not null)
                {
                    if (CURRENT_ITMES_ROW.BES > 0 && CURRENT_ITMES_ROW.BED > 0 || CURRENT_ITMES_ROW.BES <= 0 && CURRENT_ITMES_ROW.BED <= 0)
                    {
                        Child14_CANCEL_EDIT();
                        universControl.PopNotifyShow("بدهكار و بستانكار سند صحيح نمي باشد!", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            #endregion

            #region DataGrid_After_Update
            if (USER_NAME.Text != CL_HESABDARI.UCurrentUser().ToString())
            {
                USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();
            }
            #endregion

            #region HES_Before_Update
            if (CL_HESABDARI.ISTAF(CURRENT_ITMES_ROW.HES))
            {
                Child14_CANCEL_EDIT();
                Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                msgwin.ShowDialog();
                return;
            }
            #endregion

            DG_ON_CURRENT();

            try
            {
                CmdSaveRecord(ROW);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // 2627 & 2601 : duplicate key
                {
                    universControl.PopNotifyShowUp("این سطر تکراری است و نمی‌توان آن را ثبت کرد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
                else if (ex.Number == 547)   // 547 : foreign key constraint violation
                {
                    universControl.PopNotifyShowUp("ابتدا سربرگ مربوطه را ذخیره کنید، سپس جزئیات را ثبت نمایید.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
                else
                {
                    universControl.PopNotifyShowUp("خطا در انجام عملیات ثبت سطر ! اطلاعات ذخیره نشده است.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
            }
            catch (Exception)
            {
                throw;
            }

            IsSaveSuccess = true;

            if (!IsPastingRows)
            {
                ReGetData();

                var col_index = Child14.Columns.FirstOrDefault(c => c.SortMemberPath == "HES").DisplayIndex;
                Child14.SelectedIndex = Child14.Items.Count - 1;
                Child14.CurrentCell = new DataGridCellInfo(Child14.SelectedItem, Child14.Columns[col_index]);
                //

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Child14.BeginEdit();

                }), DispatcherPriority.Background);
            }
        }

        private void DATE_S_LostFocus(object sender, RoutedEventArgs e)
        {
            DATE_VALIDATE(false);
        }

        private bool DATE_VALIDATE(bool DisplayMsg = true)
        {
            string date_n_val = DATE_S.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_S.Text = null;
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    }
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_S.Text = null;
                        if (DisplayMsg)
                        {
                            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        }
                        return false;
                    }
                }
            }
            else
            {
                DATE_S.Text = null;
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                return false;
            }

            return true;
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (GHATEI == 0)
            {
                if (string.IsNullOrEmpty(N_S.Text))
                {
                    return;
                }

                if (!IsNull(this.N_S.Text) && Convert.ToInt32(N_S.Text) > 0)
                {
                    if (!string.IsNullOrWhiteSpace(NO_S.Text) && NO_S.Text != "0")
                    {
                        Msgwin msgwin = new Msgwin(false, "سند اتوماتیک است و قابل اصلاح نیست");
                        msgwin.ShowDialog();
                        return;
                    }

                    CL_HESABDARI.TR("DEED_HED", $"(N_S = {N_S.Text})", DateTime.Now, 1);
                    CL_HESABDARI.TR("DEED_DTL", $"(N_S = {N_S.Text})", DateTime.Now, 1);

                    CL_HESABDARI.LetSigneTick(GetType().Name, 0, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);

                    if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
                    {
                        Msgwin msgwin = new Msgwin(false, " اول امضاء را برداريد ...");
                        msgwin.ShowDialog();
                        return;
                    }
                    else
                    {
                        this.AllowEdits = true;
                        Child14.IsReadOnly = false;
                        //Child14.CanUserAddRows = true;
                        //Child14.CanUserDeleteRows = true;

                        DATE_S.IsEnabled = true;
                        NO_S.IsEnabled = true;
                        N_S.IsEnabled = true;
                        SHARH_S.IsEnabled = true;
                        BASE.IsEnabled = true;
                        SAVE_BTN.IsEnabled = true;
                        DELETE.IsEnabled = true;
                        ESLAH.IsEnabled = true;


                        //Command22.IsEnabled = true;
                        //Command3.IsEnabled = true;
                        //PERSONEL.IsEnabled = true;
                        //SGN1.IsEnabled = true;
                        //SGN2.IsEnabled = true;
                        //SGN3.IsEnabled = true;
                        //sgn1usid.IsEnabled = true;
                        //sgn2usid.IsEnabled = true;
                        //sgn3usid.IsEnabled = true;
                    }
                }
            }


        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            CommitInProgressEdits();

            #region Click
            double MID;
            string SHARH;
            //double td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToInt64(this.BASE.Text), 0);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'سند حسابداري شماره: " + this.N_S.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_S.Text.ToRawTarikh()), "####/##/##") + "  به شرح: " + SHARH_S.Text + "','" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0, GETDATE() ," + Baseknow.USERCOD + " )");

                MID = CL_HESABDARI.Gettaskid(Convert.ToInt64(this.BASE.Text), 0);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            sgn1usid.Tag = Baseknow.USERCOD;
            sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD)?.SAL_NAME;

            if (!(bool)OKF.IsChecked || SGN1.IsChecked == true)
            {
                this.OKF.IsChecked = true;
                //this.PERSONEL.Top = this.SGN1.Top;
                DATE_S.IsEnabled = false;
                NO_S.IsEnabled = false;
                N_S.IsEnabled = false;
                SHARH_S.IsEnabled = false;
                BASE.IsEnabled = false;
                SAVE_BTN.IsEnabled = false;
                DELETE.IsEnabled = false;
                ESLAH.IsEnabled = true;
                Command22.IsEnabled = true;
                Command3.IsEnabled = true;
                //PERSONEL.IsEnabled = false;
                //SGN1.IsEnabled = false;
                //SGN2.IsEnabled = false;
                //SGN3.IsEnabled = false;
                //sgn1usid.IsEnabled = false;
                //sgn2usid.IsEnabled = false;
                //sgn3usid.IsEnabled = false;

                Child14.IsReadOnly = true;

            }


            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command22.IsEnabled = true;
                this.Command3.IsEnabled = true;
            }
            else
            {
                this.Command22.IsEnabled = false;
                this.Command3.IsEnabled = false;
            }
            #endregion

            dbms.DoExecuteSQL($"UPDATE DEED_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE N_S = {N_S.Text}");
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            CommitInProgressEdits();

            #region Click
            double MID;
            string SHARH;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.BASE.Text), 0);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.N_S.Text + ",0 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'سند حسابداري شماره: " + this.N_S.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_S.Text.ToRawTarikh()), "####/##/##") + "  به شرح: " + SHARH_S.Text + "','" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";


                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0,"
                    + " GETDATE() " +
                    "," + Baseknow.USERCOD + " )");

                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.BASE.Text), 0);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            sgn2usid.Tag = Baseknow.USERCOD;
            sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD)?.SAL_NAME;

            if (!(bool)this.OKF.IsChecked || SGN2.IsChecked == true)
            {
                this.OKF.IsChecked = true;
                //this.PERSONEL.Top = this.SGN1.Top;
                DATE_S.IsEnabled = false;
                NO_S.IsEnabled = false;
                N_S.IsEnabled = false;
                SHARH_S.IsEnabled = false;
                BASE.IsEnabled = false;
                SAVE_BTN.IsEnabled = false;
                DELETE.IsEnabled = false;
                ESLAH.IsEnabled = true;
                Command22.IsEnabled = true;
                Command3.IsEnabled = true;
                //PERSONEL.IsEnabled = false;
                //SGN1.IsEnabled = false;
                //SGN2.IsEnabled = false;
                //SGN3.IsEnabled = false;
                //sgn1usid.IsEnabled = false;
                //sgn2usid.IsEnabled = false;
                //sgn3usid.IsEnabled = false;

                Child14.IsReadOnly = true;

            }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command22.IsEnabled = true;
                this.Command3.IsEnabled = true;
            }
            else
            {
                this.Command22.IsEnabled = false;
                this.Command3.IsEnabled = false;
            }
            #endregion
            dbms.DoExecuteSQL($"UPDATE DEED_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE N_S = {N_S.Text}");
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            CommitInProgressEdits();

            #region Click
            double MID;
            string SHARH;
            MID = CL_HESABDARI.Gettaskid(Convert.ToInt32(this.BASE.Text), 0);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.N_S.Text + ",0 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'سند حسابداري شماره: " + this.N_S.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_S.Text.ToRawTarikh()), "####/##/##") + "  به شرح: " + SHARH_S.Text + "','" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0, GETDATE() ," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.BASE.Text), 0);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",0," + this.BASE.Text + ",0 )");
            }

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            sgn3usid.Tag = Baseknow.USERCOD;
            sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD)?.SAL_NAME;

            if (!(bool)OKF.IsChecked || SGN3.IsChecked == true)
            {
                this.OKF.IsChecked = true;
                //this.PERSONEL.Top = this.SGN1.Top;
                DATE_S.IsEnabled = false;
                NO_S.IsEnabled = false;
                N_S.IsEnabled = false;
                SHARH_S.IsEnabled = false;
                BASE.IsEnabled = false;
                SAVE_BTN.IsEnabled = false;
                DELETE.IsEnabled = false;
                ESLAH.IsEnabled = true;
                Command22.IsEnabled = true;
                Command3.IsEnabled = true;
                //PERSONEL.IsEnabled = false;
                //SGN1.IsEnabled = false;
                //SGN2.IsEnabled = false;
                //SGN3.IsEnabled = false;
                //sgn1usid.IsEnabled = false;
                //sgn2usid.IsEnabled = false;
                //sgn3usid.IsEnabled = false;

                Child14.IsReadOnly = true;
            }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command22.IsEnabled = true;
                this.Command3.IsEnabled = true;
            }
            else
            {
                this.Command22.IsEnabled = false;
                this.Command3.IsEnabled = false;
            }
            #endregion
            dbms.DoExecuteSQL($"UPDATE DEED_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE N_S = {N_S.Text}");
        }


        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }
            if (string.IsNullOrEmpty(N_S.Text) || N_S.Text == "0")
            {
                new Msgwin(false, "ابتدا سند را ذخیره و سپس ارجاع دهید").ShowDialog();
                return;
            }

            #region Selection_Changed
            if (N_S.Text is null || N_S.Text == "0" || N_S.Text == "" || DATE_S.Text.ToRawTarikh() is null || DATE_S.Text.ToRawTarikh() == "" || PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow("شماره سند و تاریخ نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            CL_HESABDARI.PERSONELUpdate(0, Convert.ToDouble(this.BASE.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), "'سند حسابداري شماره: " + this.N_S.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_S.Text.ToRawTarikh()), "####/##/##") + "  به شرح: " + SHARH_S.Text + "','" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'");
            Msgwin msgwin = new Msgwin(false, "ارجاع داده شد.");
            msgwin.ShowDialog();
            #endregion
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is DEED_HED item)
            {
                if (item != null)
                {
                    var itemfound = RecordsData.View.Cast<DEED_HED>().FirstOrDefault(x => x.N_S == item.N_S);
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
                new SearchableProperty { DisplayName = "شماره سند", PropertyPath = "N_S", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_S", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "شرح سند", PropertyPath = "SHARH_S", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "شماره بایگانی", PropertyPath = "BAYEG", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "نوع سند", PropertyPath = "NO_S", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "شماره مبنا", PropertyPath = "BASE", PropertyType = typeof(double) },
                // Add other searchable properties
            };
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dg = Child14;
            try
            {
                UIElement uie = e.OriginalSource as UIElement;
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (Child14.CurrentColumn is not null && Child14.CurrentColumn.SortMemberPath == "SHARH")
                    {
                        if (uie is System.Windows.Controls.TextBox tb && tb.Text.EndsWith("+"))
                        {
                            e.Handled = true;
                            var sharhListWin = new SHARH_LIST();
                            if (sharhListWin.ShowDialog() == true && !string.IsNullOrEmpty(sharhListWin.SelectedSharh))
                            {
                                tb.Text = tb.Text.Substring(0, tb.Text.Length - 1) + sharhListWin.SelectedSharh;
                                tb.SelectionStart = tb.Text.Length;
                            }
                            return; // Stop further Enter processing
                        }
                        else if (Child14.SelectedItem is Prg_Proccessy.SQLMODELS.DEED_DTL currentRow && currentRow.SHARH != null && currentRow.SHARH.EndsWith("+"))
                        {
                            e.Handled = true;
                            var sharhListWin = new SHARH_LIST();
                            if (sharhListWin.ShowDialog() == true && !string.IsNullOrEmpty(sharhListWin.SelectedSharh))
                            {
                                currentRow.SHARH = currentRow.SHARH.Substring(0, currentRow.SHARH.Length - 1) + sharhListWin.SelectedSharh;
                                Child14.Items.Refresh(); // Only refresh if not in edit mode
                            }
                            return; // Stop further Enter processing
                        }
                    }
                    if (uie is DataGridCell || (uie as FrameworkElement)?.Parent is DataGridCell)
                    {
                        if (Child14.CurrentColumn is not null)
                        {
                            if (Child14.SelectedIndex == Child14.Items.Count - 2 && Child14.CurrentColumn.SortMemberPath == "MHAZ_NO")
                            {
                                CL_LMethods.SendKey_US(Key.Tab);

                                var col_index = Child14.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_HES").DisplayIndex;
                                Child14.SelectedIndex = Child14.Items.Count - 1;
                                Child14.CurrentCell = new DataGridCellInfo(Child14.SelectedItem, Child14.Columns[col_index]);
                                //

                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    Child14.BeginEdit();

                                }), DispatcherPriority.Background);
                            }
                        }
                    }
                    if (SAVE_BTN.IsFocused)
                    {
                        //Enter Key Continue
                    }
                    else
                    {
                        e.Handled = true;
                        CL_LMethods.SendKey_US(Key.Tab);
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
            }
            catch { }

            if (!Child14.IsKeyboardFocusWithin && !Child14.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }
            else
            {
                if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    DataGridExtension.HandleKeyPress(sender, e, Child14);
                }
            }




            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (IsDataGrid_SUB_IsFocused)
                {
                    //DELETE_Click(null, null);
                }
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

        private void ValidateDataGridRow(DataGridRowEditEndingEventArgs args, PasteValidationResult validationResult)
        {
            // Default to true
            validationResult.IsRowValid = true;

            if (args.Row.Item is DEED_DTL item)
            {
                item.id = null; //Reset id to be sure the new data will insert not update the same row existing before
                item.RADIF = null;
                item.UID = null;
                item.CRT = null;
                CURRENT_ITMES_ROW = item;

                //برای اینکه اگر فقط حساب ها وارد شده بود , بیاد بقیه مشتقات اون رو هم خودش بگیره
                var _HESNAME_ = GetCustHesabCached(CURRENT_ITMES_ROW.HES)?.NAME;
                if (!string.IsNullOrEmpty(_HESNAME_))
                {
                    CURRENT_ITMES_ROW.NAME_HES = _HESNAME_;

                    double? KOL = null, MOIN = null, taf = null;
                    double? TAF2 = null;
                    double? taf3 = null;
                    double? taf4 = null;
                    CURRENT_ITMES_ROW.HES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(CURRENT_ITMES_ROW.HES));
                    CURRENT_ITMES_ROW.HES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(CURRENT_ITMES_ROW.HES));
                    CURRENT_ITMES_ROW.HES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(CURRENT_ITMES_ROW.HES));
                    CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.HES, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                    CURRENT_ITMES_ROW.HES_T2 = (int?)TAF2;
                    CURRENT_ITMES_ROW.HES_T3 = (int?)taf3;
                    CURRENT_ITMES_ROW.HES_T4 = (int?)taf4;

                    Child14_RowEditEnding(Child14, args);
                    validationResult.IsRowValid = IsSaveSuccess;
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                }
            }
            else
            {
                // If the item is not of type CUSTOM_MODEL, invalidate the row
                args.Cancel = true;
                validationResult.IsRowValid = false;
            }
        }
        private void AddItemToDataSource(DEED_DTL item)
        {
            // Ensure thread safety if MY_ALL_DATA is accessed from multiple threads
            Application.Current.Dispatcher.Invoke(() =>
            {
                SANAD_DATA.Add(item);
            });
        }

        private bool IsClipboardValidForPasting()
        {
            if (!Clipboard.ContainsText()) return false;

            var data = Clipboard.GetText();

            try
            {
                var serializer = new DataContractSerializer(typeof(List<DEED_DTL>));
                using (var stream = new MemoryStream(Convert.FromBase64String(data)))
                {
                    var items = serializer.ReadObject(stream) as List<DEED_DTL>;

                    // Return true if the list is not null or empty, otherwise false
                    return items?.Any() == true;
                }
            }
            catch (Exception)
            {
                // General fallback for other unexpected errors
                return false;
            }
        }


        private void DG_ON_CURRENT()
        {
            // اگر دیتایی نباشد، فیلدها را صفر کن
            if (Child14 == null || Child14.Items == null || Child14.Items.Count == 0)
            {
                bedt.Text = best.Text = SSBED.Text = SSBES.Text = "0";
                return;
            }

            // لیست «نمایش‌داده‌شده» تا مرتب‌سازی/فیلتر را رعایت کنیم
            var items = Child14.Items.OfType<DEED_DTL>().ToList();
            if (items.Count == 0)
            {
                bedt.Text = best.Text = SSBED.Text = SSBES.Text = "0";
                return;
            }

            long totalBed = 0, totalBes = 0;
            long uptoBed = 0, uptoBes = 0;

            // ایندکس سطر انتخابی (با احتساب NewItemPlaceholder)
            int selected = Child14.SelectedIndex;
            if (selected < 0) selected = items.Count - 1; // اگر هیچ‌سطر انتخاب نیست، تا آخر جمع بزن

            for (int i = 0; i < items.Count; i++)
            {
                var r = items[i];
                long bed = (long)(r?.BED ?? 0);
                long bes = (long)(r?.BES ?? 0);

                totalBed += bed;
                totalBes += bes;

                // جمع تا «واردِ» سطر جاری (inclusive)
                if (i <= selected)
                {
                    uptoBed += bed;
                    uptoBes += bes;
                }
            }

            SSBED.Text = totalBed.ToString();   // جمع سند (بدهکار/بستانکار کل)
            SSBES.Text = totalBes.ToString();

            bedt.Text = uptoBed.ToString();   // جمع سند (بدهکار/بستانکار کل)
            best.Text = uptoBes.ToString();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }

        private void Child14_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (Child14.SelectedItem == null)
            //{
            //    e.Handled = true;
            //    return;
            //}

            if (!(Child14.CurrentCell.Column is null))
                CURRENT_COLUMN_INDEX = Child14.CurrentCell.Column.DisplayIndex;

            CURRENT_ROW_INDEX = Child14.SelectedIndex;

            DG_ON_CURRENT();
        }

        public bool CmdSaveRecord(DEED_DTL ROW)
        {
            //Saving...

            long? ID = null;
            //INSERT
            if (ROW.id is null || ROW.id <= 0)
            {
                if (ROW.N_SERI == 0 || ROW.BANK == 0)
                {
                    ROW.N_SERI = null;
                    ROW.BANK = null;
                }
                ROW.N_S = Convert.ToDouble(N_S.Text);
                ROW.ARZD = 1;

                //ID = dbms.DoGetDataSQL<long?>($@"INSERT INTO DEED_DTL (            N_S ,                                              N_SERI ,                                                BANK ,                                                RADIF ,                                               HES_K ,                                               HES_M ,                                               HES_T ,             BED ,             BES ,                HES ,             ARZD ,                                                HES_T2 ,                                                HES_T3,                                                   SHARH ,                                                HES_T4) 
                //                         OUTPUT INSERTED.id VALUES ({ROW.N_S} ,{(ROW.N_SERI is null ? "NULL" : ROW.N_SERI)},{(ROW.BANK is null ? "NULL" : ROW.BANK)},{(ROW.RADIF is null ? "NULL" : ROW.RADIF)} , {(ROW.HES_K == null ? "NULL" : ROW.HES_K)} , {(ROW.HES_M == null ? "NULL" : ROW.HES_M)} , {(ROW.HES_T == null ? "NULL" : ROW.HES_T)} , {ROW.BED} , {ROW.BES} , N'{ROW.HES}' , {ROW.ARZD} , {(ROW.HES_T2 == null ? "NULL" : ROW.HES_T2)} , {(ROW.HES_T3 == null ? "NULL" : ROW.HES_T3)},N'{(ROW.SHARH is null ? "" : ROW.SHARH)}', {(ROW.HES_T4 == null ? "NULL" : ROW.HES_T4)})").FirstOrDefault();

                ID = dbms.DoGetDataSQL<long?>($@"INSERT INTO DEED_DTL (N_S, N_SERI, BANK, RADIF, HES_K, HES_M, HES_T, BED, BES, HES, ARZD, HES_T2, HES_T3, SHARH, HES_T4) 
                                          OUTPUT INSERTED.id VALUES (@N_S, @N_SERI, @BANK, @RADIF, @HES_K, @HES_M, @HES_T, @BED, @BES, @HES, @ARZD, @HES_T2, @HES_T3, @SHARH, @HES_T4)",
                                       new
                                       {
                                           ROW.N_S,
                                           ROW.N_SERI,
                                           ROW.BANK,
                                           ROW.RADIF,
                                           ROW.HES_K,
                                           ROW.HES_M,
                                           ROW.HES_T,
                                           BED = ROW.BED ?? 0,
                                           BES = ROW.BES ?? 0,
                                           ROW.HES,
                                           ROW.ARZD,
                                           ROW.HES_T2,
                                           ROW.HES_T3,
                                           SHARH = ROW.SHARH ?? "",
                                           ROW.HES_T4
                                       }).FirstOrDefault();

                if (ID != null)
                {
                    ROW.id = ID;
                }
            }
            else //UPDATE
            {
                ROW.N_S = Convert.ToDouble(N_S.Text);
                ROW.ARZD = 1;

                //dbms.DoExecuteSQL($@"UPDATE DEED_DTL SET 
                //                            N_S = {ROW.N_S} , 
                //                                        N_SERI = {(ROW.N_SERI is null ? "NULL" : ROW.N_SERI)},
                //                                        BANK = {(ROW.BANK is null ? "NULL" : ROW.BANK)},
                //                            RADIF = {(ROW.RADIF is null ? "NULL" : ROW.RADIF)} , 
                //                            HES_K = {(ROW.HES_K == null ? "NULL" : ROW.HES_K)} , 
                //                            HES_M = {(ROW.HES_M == null ? "NULL" : ROW.HES_M)} , 
                //                            HES_T = {(ROW.HES_T == null ? "NULL" : ROW.HES_T)} , 
                //                            BED = {ROW.BED} , 
                //                            BES = {ROW.BES} , 
                //                            HES = N'{ROW.HES}' , 
                //                            ARZD = {ROW.ARZD} , 
                //                            HES_T2 = {(ROW.HES_T2 == null ? "NULL" : ROW.HES_T2)} , 
                //                            HES_T3 = {(ROW.HES_T3 == null ? "NULL" : ROW.HES_T3)} , 
                //                            HES_T4 = {(ROW.HES_T4 == null ? "NULL" : ROW.HES_T4)},
                //                                        SHARH = N'{(ROW.SHARH is null ? "NULL" : ROW.SHARH)}' WHERE id = {ROW.id}");

                dbms.DoExecuteSQL($@"UPDATE DEED_DTL SET 
                                                        N_S = @N_S, 
                                                        N_SERI = @N_SERI,
                                                        BANK = @BANK,
				                                        RADIF = @RADIF, 
				                                        HES_K = @HES_K, 
				                                        HES_M = @HES_M, 
				                                        HES_T = @HES_T, 
				                                        BED = @BED, 
				                                        BES = @BES, 
				                                        HES = @HES, 
				                                        ARZD = @ARZD, 
				                                        HES_T2 = @HES_T2, 
				                                        HES_T3 = @HES_T3, 
				                                        HES_T4 = @HES_T4,
                                                        SHARH = @SHARH
                                                  WHERE id = @id",
                                               new
                                               {
                                                   ROW.N_S,
                                                   ROW.N_SERI,
                                                   ROW.BANK,
                                                   ROW.RADIF,
                                                   ROW.HES_K,
                                                   ROW.HES_M,
                                                   ROW.HES_T,
                                                   BED = ROW.BED ?? 0,
                                                   BES = ROW.BES ?? 0,
                                                   ROW.HES,
                                                   ROW.ARZD,
                                                   ROW.HES_T2,
                                                   ROW.HES_T3,
                                                   SHARH = ROW.SHARH ?? "",
                                                   ROW.HES_T4,
                                                   ROW.id
                                               });
            }

            return true;
        }


        private bool SANAD_Row_Deleter(DEED_DTL item)
        {
            if (item == null)
            {
                return false;
            }

            bool isDeleteSomething = false;

            IEditableCollectionView itemsView = Child14.Items as IEditableCollectionView;

            //if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
            if (!itemsView.IsAddingNew)
            {
                if (item.id is null)
                {
                    SANAD_DATA.Remove(item as DEED_DTL);
                }
                else
                {
                    try
                    {
                        // YOUR_CODE_HERE
                        var _id = item.id;
                        var _n_s = item.N_S;
                        dbms.DoExecuteSQL($"DELETE FROM DEED_DTL WHERE N_S = {_n_s} AND id = {_id}");
                        // YOUR_CODE_HERE
                        isDeleteSomething = true;
                        ReGetData();
                        if (SANAD_DATA.Count == 0)
                        {
                            Child14.CanUserAddRows = false;
                            Child14.CanUserAddRows = true;
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        new Msgwin(false, $"این سطر سند , حساب : {item.NAME_HES} و بدهکار : {item.BED} و بستانکار {item.BED} دارای گردش است و نمیتوان آنرا حذف کرد !").ShowDialog();
                    }
                    catch (SqlException)
                    {
                        new Msgwin(false, $"این سطر سند , حساب : {item.NAME_HES} و بدهکار : {item.BED} و بستانکار {item.BED} به دلیل بروز خطا در پایگاه داده حذف انجام نشد  !").ShowDialog();
                    }
                    catch
                    {
                        new Msgwin(false, $"این سطر سند , حساب : {item.NAME_HES} و بدهکار : {item.BED} و بستانکار {item.BED} خطا در انجام علمیات حذف !").ShowDialog();
                    }

                }
            }
            else
            {
                Msgwin msgwin11 = new Msgwin(false, "چیزی برای حذف وجود ندارند");
                msgwin11.ShowDialog();
                return false;
            }

            return isDeleteSomething;
        }

        private void Command3_Click(object sender, RoutedEventArgs e)
        {
            string stDocName;
            if (!IsNull(this.N_S.Text))
            {
                //DoCmd.RunCommand(acCmdSaveRecord);
                var rst = dbms.DoGetDataSQL<DEED_QR2>("SELECT Sum(DEED_DTL.BED) AS SumOfBED, Sum(DEED_DTL.BES) AS SumOfBES, SUM(ROUND(BED - BES, 0)) AS Expr1 FROM DEED_DTL WHERE (((DEED_DTL.N_S)=" + this.N_S.Text + "))").ToList();
                if (rst.FirstOrDefault().Expr1 != 0)
                {
                    Msgwin msgwin = new Msgwin(false, $@"سند تراز نمي باشد.جمع بدهكار و بستانكار سند بايد مساوي  باشد.مبلغ اختلاف : {rst.FirstOrDefault().Expr1} ");
                    msgwin.ShowDialog();
                }
                else
                {

                    dbms.DoExecuteSQL("DELETE FROM dbo.DEAD_DTL_PRINT");
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + $"' AS Expr1  FROM dbo.DEAD_WITH_GRP WHERE    (N_S = {N_S.Text})");
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + $"' AS Expr1  FROM dbo.DEAD_WITH_GRP1 WHERE   (N_S = {N_S.Text})");

                    #region R_SANAD_PRINT_B
                    //Report Loading
                    Process Prc = ProcLoader.Start();



                    var report = new StiReport();

                    var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.SANAD_Reports.R_SANAD_PRINT_B.mrt");

                    report.Load(pathreport);

                    report.Dictionary.Databases.Clear();


                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

                    //Parameters
                    var Saman_Name = dbms.DoGetDataSQL<string>("SELECT NAME FROM SAZMAN").FirstOrDefault();

                    //report Show
                    report.Render(false);


                    report.ShowWithWpf();
                    ProcLoader.Stop(Prc);

                    OKF.IsChecked = true;
                    #endregion

                    //if ((bool)OKF.IsChecked && this.NO_S.Text == "0")
                    //{
                    //    this.AllowDeletions = false;
                    //    this.AllowEdits = false;
                    //    this.Child14.IsReadOnly = true;

                    //    DATE_S.IsEnabled = false;
                    //    NO_S.IsEnabled = false;
                    //    N_S.IsEnabled = false;
                    //    SHARH_S.IsEnabled = false;
                    //    BASE.IsEnabled = false;
                    //    SAVE_BTN.IsEnabled = false;
                    //    DELETE.IsEnabled = false;
                    //    ESLAH.IsEnabled = true;
                    //    Command22.IsEnabled = false;
                    //    Command3.IsEnabled = false;
                    //    //PERSONEL.IsEnabled = false;
                    //    SGN1.IsEnabled = false;
                    //    SGN2.IsEnabled = false;
                    //    SGN3.IsEnabled = false;
                    //    sgn1usid.IsEnabled = false;
                    //    sgn2usid.IsEnabled = false;
                    //    sgn3usid.IsEnabled = false;

                    //    if (CL_HESABDARI.LETSGOUPDATE(this.GetType().Name, "SANAD", 3))
                    //    {
                    //        this.ESLAH.IsEnabled = true;
                    //    }
                    //    else
                    //    {
                    //        this.ESLAH.IsEnabled = false;
                    //    }
                    //}
                }
            }
            if (!IsNull(this.N_S.Text))
            {
                if (Printed(Convert.ToDouble(this.N_S.Text), 0))
                {
                    //this.prnl.BorderStyle = 1;
                    //this.prnl.BackColor = 6723891;
                }
                else
                {
                    //this.prnl.BorderStyle = 0;
                    //this.prnl.BackColor = -2147483633;
                }
            }
            else
            {
                //this.prnl.BorderStyle = 0;
                //this.prnl.BackColor = -2147483633;
            }
        }

        private void Command22_Click(object sender, RoutedEventArgs e)
        {
            string stDocName;
            if (!IsNull(this.N_S.Text))
            {
                //DoCmd.RunCommand(acCmdSaveRecord);
                var rst = dbms.DoGetDataSQL<DEED_QR2>("SELECT Sum(DEED_DTL.BED) AS SumOfBED, Sum(DEED_DTL.BES) AS SumOfBES, SUM(ROUND(BED - BES, 0)) AS Expr1 FROM DEED_DTL WHERE (((DEED_DTL.N_S)=" + this.N_S.Text + "))").ToList();
                if (rst.FirstOrDefault().Expr1 != 0)
                {
                    Msgwin msgwin = new Msgwin(false, $@"سند تراز نمي باشد.جمع بدهكار و بستانكار سند بايد مساوي  باشد.مبلغ اختلاف : {rst.FirstOrDefault().Expr1} ");
                    msgwin.ShowDialog();
                }
                else
                {

                    dbms.DoExecuteSQL("DELETE FROM dbo.DEAD_DTL_PRINT");
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + $"' AS Expr1  FROM dbo.DEAD_WITH_GRP WHERE    (N_S = {N_S.Text})");
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + $"' AS Expr1  FROM dbo.DEAD_WITH_GRP1 WHERE   (N_S = {N_S.Text})");

                    #region R_SANAD_PRINT_B
                    //Report Loading
                    Process Prc = ProcLoader.Start();


                    var report = new StiReport();

                    var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.SANAD_Reports.R_SANAD_PRINT.mrt");

                    report.Load(pathreport);

                    report.Dictionary.Databases.Clear();


                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

                    //Parameters
                    var Saman_Name = dbms.DoGetDataSQL<string>("SELECT NAME FROM SAZMAN").FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(SHARH_S.Text))
                    {
                        (report.GetComponentByName("SHARH_HEAD") as StiText).Text = "شرح سند: " + SHARH_S.Text.Trim();
                    }

                    //report Show
                    report.Render(false);


                    ProcLoader.Stop(Prc);

                    report.ShowWithWpf();

                    OKF.IsChecked = true;
                    #endregion

                    //if ((bool)OKF.IsChecked && this.NO_S.Text == "0")
                    //{
                    //    this.AllowDeletions = false;
                    //    this.AllowEdits = false;
                    //    this.Child14.IsReadOnly = true;

                    //    DATE_S.IsEnabled = false;
                    //    NO_S.IsEnabled = false;
                    //    N_S.IsEnabled = false;
                    //    SHARH_S.IsEnabled = false;
                    //    BASE.IsEnabled = false;
                    //    SAVE_BTN.IsEnabled = false;
                    //    DELETE.IsEnabled = false;
                    //    ESLAH.IsEnabled = true;
                    //    Command22.IsEnabled = false;
                    //    Command3.IsEnabled = false;
                    //    //PERSONEL.IsEnabled = false;
                    //    SGN1.IsEnabled = false;
                    //    SGN2.IsEnabled = false;
                    //    SGN3.IsEnabled = false;
                    //    sgn1usid.IsEnabled = false;
                    //    sgn2usid.IsEnabled = false;
                    //    sgn3usid.IsEnabled = false;

                    //    if (CL_HESABDARI.LETSGOUPDATE(this.GetType().Name, "SANAD", 3))
                    //    {
                    //        this.ESLAH.IsEnabled = true;
                    //    }
                    //    else
                    //    {
                    //        this.ESLAH.IsEnabled = false;
                    //    }
                    //}
                }
            }
            if (!IsNull(this.N_S.Text))
            {
                if (Printed(Convert.ToDouble(this.N_S.Text), 0))
                {
                    //this.prnl.BorderStyle = 1;
                    //this.prnl.BackColor = 6723891;
                }
                else
                {
                    //this.prnl.BorderStyle = 0;
                    //this.prnl.BackColor = -2147483633;

                }
            }
            else
            {
                //this.prnl.BorderStyle = 0;
                //this.prnl.BackColor = -2147483633;
            }
        }

        private void USER_NAME_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dbms.DoExecuteSQL("DELETE FROM dbo.DEAD_DTL_PRINT");
        }


        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                Msgwin msgwin = new Msgwin(true, "ذخیره را انجام نداده اید آیا از خروج از این سند مطمئن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }
            }

            MoveReGetData(Jahat.NewItem);
            DATE_S.Focus();
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.LastItem);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.NextItem);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.BackItem);
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.FirstItem);
        }
        private void SERVERRELOAD_Btn_Click(object sender, RoutedEventArgs e)
        {
            ReGetMasterData();
        }

        private void SANDLISTS_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD_LIST, this, WhereLimitcondition);
        }

        private void Child14_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && Child14.SelectedItem != null && Child14.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (Child14.Items.Count > 0)
                {
                    if (!(Child14.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = Child14.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = Child14.SelectedIndex;

                    DG_ON_CURRENT();
                }
            }
        }
        private void Child14_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Child14.IsReadOnly == false)
            {

            }
        }
        private void Child14_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)Child14.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)Child14.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && Child14.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<DEED_DTL>(Child14);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !Child14.IsReadOnly && Child14.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<DEED_DTL>(Child14, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
                //if (IsClipboardValidForPasting())
                //{
                //}
            }


            if (Child14.CurrentColumn is not null)
            {
                #region BED


                if (e.Key == Key.Add)
                {
                    if (Child14.CurrentColumn.SortMemberPath is "BED")
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
                    if (Child14.CurrentColumn.SortMemberPath is "BED")
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
                #endregion

                #region BES
                if (e.Key == Key.Add)
                {
                    if (Child14.CurrentColumn.SortMemberPath is "BES")
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
                    if (Child14.CurrentColumn.SortMemberPath is "BES")
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
                #endregion
            }
            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                DELETE_Click(null, null);
            }

            // Check if Ctrl key is pressed and the pressed key is double quote
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (Child14.CurrentCell != null)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = Child14.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = Child14.Items.IndexOf(currentCell.Item);
                            int columnIndex = Child14.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = Child14.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < Child14.Columns.Count)
                                {
                                    // Get the column information
                                    var column = Child14.Columns[columnIndex];

                                    // Ensure that the column has a valid SortMemberPath
                                    if (!string.IsNullOrEmpty(column.SortMemberPath))
                                    {
                                        // Use reflection to get and set the property values
                                        var propertyInfo = valueAbove.GetType().GetProperty(column.SortMemberPath);

                                        if (valueAbove == null || currentCell.Item == null)
                                            return;  // ردیف معتبری در کار نیست

                                        string propName = column.SortMemberPath;

                                        // ---------- پراپرتی مبدأ ----------
                                        PropertyInfo srcProp = valueAbove.GetType()
                                                                         .GetProperty(propName,
                                                                                      BindingFlags.Public | BindingFlags.Instance);

                                        // ---------- پراپرتی مقصد ----------
                                        PropertyInfo dstProp = currentCell.Item.GetType()
                                                                               .GetProperty(propName,
                                                                                            BindingFlags.Public | BindingFlags.Instance);

                                        // اگر هر کدام پیدا نشد یا مقصد قابل‌نوشتن نبود، ادامه نده
                                        if (srcProp == null || dstProp == null || !dstProp.CanWrite)
                                            return;

                                        // هر دو پراپرتی باید یکسان یا سازگار باشند
                                        if (!dstProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                                            return;

                                        // کپی مقدار
                                        object val = srcProp.GetValue(valueAbove);
                                        dstProp.SetValue(currentCell.Item, val);

                                    }
                                }
                            }
                        }
                        e.Handled = true;
                    }
                }
                catch (Exception) { }

            }
        }
        private void Child14_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
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
        private void Child14_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (Child14?.SelectedItem is not null)
                {
                    if (Child14.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((DEED_DTL)Child14.SelectedItem).Clone() as DEED_DTL;

                        CURRENT_ITMES_ROW = Child14.SelectedItem as DEED_DTL;
                    }
                }
            }
        }
        private void Child14_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {

        }
        private void Child14_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid? dg = sender as DataGrid;
            if (dg == null) return;

            if (dg.CurrentItem == null || dg.CurrentItem == CollectionView.NewItemPlaceholder)
            {
                e.Handled = true; // Cancel opening the menu. Avoids the crash.
                return;
            }
            if (dg?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            else if (dg?.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            base.OnContextMenuOpening(e);
        }


        private void F8_CUSTOMER_Click(object sender, RoutedEventArgs e)
        {
            if (Child14.IsEnabled == true)
            {
                var CurrentData = Child14.SelectedItem as DEED_DTL;

                if (CurrentData != null)
                {
                    if (CurrentData.HES is not null)
                    {
                        new F_MENU_KOL_MOIN_TAFZIL(CurrentData.HES.ToString());
                    }
                }
            }
        }

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            if (SANAD_DATA.Count == 0)
            {
                return;
            }

            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(Child14, "DGExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }

        private void COPY_CLICK(object sender, RoutedEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)Child14.Items).IsEditingItem;
            if (!isEditing)
            {
                e.Handled = true;
                DataGridClipboardManager.CopySelectedItems<DEED_DTL>(Child14);
            }
            else
            {
                var editingElement = CL_LMethods.FindChild<TextBox>(Child14);
                if (editingElement != null)
                {
                    if (!string.IsNullOrEmpty(editingElement.SelectedText))
                    {
                        Clipboard.SetText(editingElement.SelectedText);
                    }
                }
            }
        }

        private void PASTE_CLICK(object sender, RoutedEventArgs e)
        {
            if (Child14.SelectedItem != null || Child14.SelectedItems.Count > 0)
            {
                var isEditing = ((IEditableCollectionView)Child14.Items).IsEditingItem;
                if (!isEditing && !Child14.IsReadOnly && Child14.IsEnabled)
                {
                    e.Handled = true;

                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<DEED_DTL>(Child14, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;

                    Child14.CommitEdit();
                }
                else
                {
                    //System.Windows.Forms.SendKeys.SendWait("^v");

                    if (ApplicationCommands.Paste.CanExecute(null, Keyboard.FocusedElement as IInputElement))
                    {
                        ApplicationCommands.Paste.Execute(null, Keyboard.FocusedElement as IInputElement);
                    }
                }
            }
            else
            {
                universControl.PopNotifyShowUp("عمل انتقال کپی را باید با راست کلیک روی یک سطر خالی انجام بدید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
            }
        }

        private void Child14_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null) return;

            try
            {
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
                    var isEditing = ((IEditableCollectionView)Child14.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }
    }
}
