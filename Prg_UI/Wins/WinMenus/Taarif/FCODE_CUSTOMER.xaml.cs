using Dapper;
using Functions;
using ImageMagick;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Wins.WinOther;
using static Prg_UI.HelperWins.Msgwin;

namespace Wins.WinMenus.Taarif
{
    public partial class FCODE_CUSTOMER : Window, ISearchableWindow, INotifyPropertyChanged, ICloneable
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

        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        public FCODE_CUSTOMER(string? openargs = null)
        {
            InitializeComponent();

            OpenArgs = openargs;

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        ////public double NUMBER { get; set; }
        ////public double N_KOL { get; set; }
        public string OpenArgs { get; set; }
        public bool NowIsReady { get; private set; }

        private bool _newrecord = false;
        public bool NewRecord
        {
            set { _newrecord = value; }
            get
            {
                //if (!string.IsNullOrEmpty(TNUMBER.Text) && Convert.ToDouble(TNUMBER.Text) > 0)
                //{
                //var Custy = dbms.DoGetDataSQL<string>($"SELECT TOP 1 RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar))  AS HES FROM TDETA_HES WHERE     (((TDETA_HES.N_KOL) = {KOL}) AND ((TDETA_HES.NUMBER) = {MOIN})) AND ((TDETA_HES.TNUMBER) = {TNUMBER.Text.Trim()})").ToList();
                //if (Custy.Count > 0)
                //{
                //    _newrecord = false;
                //}
                //}
                return _newrecord;
            }
        }

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
                //کدمشتری
                ////CustomerCode.IsReadOnly = false;
                //نام مشتری
                NAMES.IsReadOnly = false;
                //آدرس
                ADDRESS.IsReadOnly = false;
                //تلفن
                TEL.IsReadOnly = false;
                //کد اقتصادی
                ECODE.IsReadOnly = false;
                //کدپستی
                PCODE.IsReadOnly = false;
                //کد ملی
                MCODEM.IsReadOnly = false;
                //سایر
                CODE_E.IsReadOnly = false;
                //موبایل جهت ارسال پیامک
                MOBILE.IsReadOnly = false;
                //طول جغرافیایی
                Latitude.IsReadOnly = false;
                //عرض جغرافیایی
                Longitude.IsReadOnly = false;

                //استان
                OSTANID.IsEnabled = true;
                //شهر
                SHAHRID.IsEnabled = true;
                //نوع مشتری
                CUST_COD.IsEnabled = true;
                //مسیر ویزیت
                ROUTE_NAME.IsEnabled = true;
                //شخصیت
                tob.IsEnabled = true;
                //ذخیره
                Command25.IsEnabled = true;
                //اطلاعات بیشتر
                Command41.IsEnabled = true;
                //حذف مشتری
                Customer_Delete_Btn.IsEnabled = true;
            }
            else
            {
                //کدمشتری
                ////CustomerCode.IsReadOnly = true;
                //نام مشتری
                NAMES.IsReadOnly = true;
                //آدرس
                ADDRESS.IsReadOnly = true;
                //تلفن
                TEL.IsReadOnly = true;
                //کد اقتصادی
                ECODE.IsReadOnly = true;
                //کدپستی
                PCODE.IsReadOnly = true;
                //کد ملی
                MCODEM.IsReadOnly = true;
                //سایر
                CODE_E.IsReadOnly = true;
                //موبایل جهت ارسال پیامک
                MOBILE.IsReadOnly = true;
                //طول جغرافیایی
                Latitude.IsReadOnly = true;
                //عرض جغرافیایی
                Longitude.IsReadOnly = true;

                //استان
                OSTANID.IsEnabled = false;
                //شهر
                SHAHRID.IsEnabled = false;
                //نوع مشتری
                CUST_COD.IsEnabled = false;
                //مسیر ویزیت
                ROUTE_NAME.IsEnabled = false;
                //شخصیت
                tob.IsEnabled = false;
                //ذخیره
                Command25.IsEnabled = false;
                //اطلاعات بیشتر
                Command41.IsEnabled = false;
                //حذف مشتری
                Customer_Delete_Btn.IsEnabled = false;
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

        #region LOCAL_MODEL
        public class ExistingCustomerInfo
        {
            public string? Hes { get; set; }
            public string? Name { get; set; }
            public string DuplicateType { get; set; }
        }
        public class SQL_FC1
        {
            public int? USERCO { get; set; }
            public string? HES { get; set; }
        }
        public class FCODE_CUSTOMER_MODEL
        {
            public int? IDD { get; set; }
            public int? CUST_COD { get; set; }
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public int? TNUMBER2 { get; set; }
            public int? TNUMBER3 { get; set; }
            public int? TNUMBER4 { get; set; }
            public string? NAME { get; set; }
            public string? TOZIH { get; set; }
            public double? BED_BES { get; set; }
            public string? ADDRESS { get; set; }
            public string? TEL { get; set; }
            public string? CODE_E { get; set; }
            public string? ECODE { get; set; }
            public string? PCODE { get; set; }
            public string? IYALAT { get; set; }
            public string? CITY { get; set; }
            public string? MCODEM { get; set; }
            public string? MOBILE { get; set; }
            public string? ROUTE_NAME { get; set; }
            public string? HES { get; set; }
            public double? Longitude { get; set; }
            public double? Latitude { get; set; }
            public int? OSTANID { get; set; }
            public int? SHAHRID { get; set; }
            public int? TOB { get; set; }
        }
        public class SHAKHSIAT
        {
            public string NAME { get; set; }
            public int CODE { get; set; }
        }
        public class VISITOUR_SQL1
        {
            public string? ROUTE_NAME { get; set; }
            public string? Expr1 { get; set; }
        }

        public class VISITOUR_SQL2
        {
            public string? ROUTE_NAME { get; set; }
            public string? COUST_NO { get; set; }
            public bool? RACTIVE { get; set; }
            public int? IDR { get; set; }
            public string? CLASS { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        #endregion

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public Visual I_AM_FCODE_CUSTOMER { get; set; }
        public double? KOL { get; private set; }
        public double MOIN { get; private set; }

        public double? N_KOL { get; private set; }
        public double? NUMBER { get; private set; }
        public int TNUMBER { get; private set; }
        public double TNUMBER2 { get; set; }
        public double TNUMBER3 { get; set; }
        public int TNUMBER4 { get; private set; }

        private double taf;
        private double TAF2;
        private double TAF3;
        private double TAF4;
        private int levH;
        private string HESH;

        public bool ChangeIsHappend { get; private set; } = false;

        string TableNameLevel = "TDETA_HES";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_FCODE_CUSTOMER = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MOSHTARI", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            NAMES.Focus();
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is FCODE_CUSTOMER_MODEL item)
            {
                if (item != null)
                {
                    var itemfound = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().FirstOrDefault(x => x.TNUMBER.Equals(Convert.ToInt32(item.TNUMBER)));
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
                new SearchableProperty { DisplayName = "نام", PropertyPath = "NAME", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "کد", PropertyPath = "TNUMBER", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F7)
            {
                e.Handled = true;
                var searchWindow = new EnhancedSearchWindow(this);
                searchWindow.Owner = this;
                searchWindow.ShowDialog();
            }

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Command25.IsFocused || ESLAH.IsFocused || Command41.IsFocused || Customer_Delete_Btn.IsFocused)
                {
                }
                else
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                //var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "بله , برگرد", NO_CAPTION = "نه , مهم نیست" };
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Form_Current()
        {
            if (this.NewRecord /* || string.IsNullOrEmpty(TNUMBER.Text)*/)
            {
                //this.TNUMBER.TAG = "";
                AllowEdits = true;
                AllowDeletions = true;
            }
            else
            {
                //this.TNUMBER.TAG = this.TNUMBER;
                AllowEdits = false;
                AllowDeletions = false;
            }
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "MOSHTARI", new WindowInteropHelper(this).Handle, this.GetType().Name);
        }
        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //CultureInfo currentCulture = InputLanguageManager.Current.CurrentInputLanguage;
            //if (currentCulture.Name == "fa-IR")
            //{
            //    // The current keyboard input language is Persian
            //}
            //else
            //{
            //    // The current keyboard input language is not Persian
            //}
            //TextBox textBox = e.Source as TextBox;
            //if (textBox != null)
            //{
            //    // Cancel the text input
            //    e.Handled = true;
            //    // Get the current caret position
            //    int caretIndex = textBox.CaretIndex;
            //    // Insert the fixed text at the caret position
            //    string fixedText = CL_HESABDARI.Fixp(e.Text);
            //    textBox.Text = textBox.Text.Insert(caretIndex, fixedText);
            //    // Move the caret to the end of the newly inserted text
            //    textBox.CaretIndex = caretIndex + fixedText.Length;
            //}
        }

        public void ReGetMasterData()
        {
            string SH = null;
            KOL = Baseknow.BEDEHKAR;
            MOIN = 1;
            double? CKOL = null;
            double? CMOIN = null;
            double? CTAF = null;
            double? CTAF2 = null;
            double? CTAF3 = null;
            double? CTAF4 = null;
            CKOL = Baseknow.BEDEHKAR;
            CMOIN = 1;

            var RST2 = dbms.DoGetDataSQL<SQL_FC1>("SELECT USERCO,HES FROM BLOCK_HES WHERE HES LIKE '#%' AND  USERCO = " + Baseknow.USERCOD).ToList();

            if (RST2.Count > 0)
            {
                var HES = RST2.FirstOrDefault()?.HES;

                if (!string.IsNullOrEmpty(HES))
                {
                    CL_HESABDARI.GETTAF3(Strings.Right(HES, HES.Length - 1), ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    if (CTAF == null)
                    {
                        levH = 1;
                        KOL = Convert.ToDouble(CKOL);
                        MOIN = Convert.ToDouble(CMOIN);
                    }
                    else
                    {
                        if (CTAF2 == null)
                        {
                            levH = 2;
                            KOL = Convert.ToDouble(CKOL);
                            MOIN = Convert.ToDouble(CMOIN);
                            taf = Convert.ToDouble(CTAF);
                        }
                        else
                        {
                            if (CTAF3 == null)
                            {
                                levH = 3;
                                KOL = Convert.ToDouble(CKOL);
                                MOIN = Convert.ToDouble(CMOIN);
                                taf = Convert.ToDouble(CTAF);
                                TAF2 = Convert.ToDouble(CTAF2);
                            }
                            else
                            {
                                levH = 4;
                                KOL = Convert.ToDouble(CKOL);
                                MOIN = Convert.ToDouble(CMOIN);
                                taf = Convert.ToDouble(CTAF);
                                TAF2 = Convert.ToDouble(CTAF2);
                                TAF3 = Convert.ToDouble(CTAF3);
                            }
                        }
                    }
                }
            }
            if (levH == 0)
            {
                levH = 1;
            }
            switch (levH)
            {
                case 1:
                    RecordsData.Source = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES.N_KOL,TDETA_HES.CUST_COD, TDETA_HES.NUMBER, TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TOZIH, TDETA_HES.BED_BES,  TDETA_HES.ADDRESS , TDETA_HES.TEL, TDETA_HES.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES WHERE     (((TDETA_HES.N_KOL) = " + CKOL.ToString() + ") AND ((TDETA_HES.NUMBER) = " + CMOIN.ToString() + "))").ToList();
                    NUMBER = CMOIN;
                    N_KOL = CKOL;
                    TableNameLevel = "TDETA_HES";
                    LBL_HEADER.Content = "تعریف مشتری در تفضیلی سطح 1";
                    RecordsData.Source = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().OrderBy(item => item.TNUMBER).ToList();
                    if (!string.IsNullOrEmpty(OpenArgs))
                    {
                        var item = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().FirstOrDefault(x => x.TNUMBER.Equals(Convert.ToInt32(OpenArgs)));
                        if (item != null)
                        {
                            RecordsData.View.MoveCurrentTo(item);
                            MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                        }
                        else
                        {
                            new Msgwin(false, "چنین کد مشتری وجود ندارد").ShowDialog();
                            Close();
                            return;
                        }
                    }
                    break;
                case 2:
                    RecordsData.Source = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES2.N_KOL,TDETA_HES2.CUST_COD, TDETA_HES2.NUMBER, TDETA_HES2.TNUMBER, TDETA_HES2.TNUMBER2, TDETA_HES2.NAME, TDETA_HES2.TOZIH, TDETA_HES2.BED_BES,  TDETA_HES2.ADDRESS , TDETA_HES2.TEL, TDETA_HES2.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES2 WHERE     (((TDETA_HES2.N_KOL) = " + CKOL.ToString() + ") AND ((TDETA_HES2.NUMBER) = " + CMOIN.ToString() + ") AND ((TDETA_HES2.TNUMBER) = " + CTAF.ToString() + ") )").ToList();
                    N_KOL = CKOL;
                    NUMBER = CMOIN;
                    TNUMBER = (int)CTAF;
                    TableNameLevel = "TDETA_HES2";
                    LBL_HEADER.Content = "تعریف مشتری در تفضیلی سطح 2";
                    RecordsData.Source = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().OrderBy(item => item.TNUMBER2).ToList();
                    if (!string.IsNullOrEmpty(OpenArgs))
                    {
                        var item = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().FirstOrDefault(x => x.TNUMBER2.Equals(Convert.ToInt32(OpenArgs)));
                        if (item != null)
                        {
                            RecordsData.View.MoveCurrentTo(item);
                            MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                        }
                        else
                        {
                            new Msgwin(false, "چنین کد مشتری وجود ندارد").ShowDialog();
                            Close();
                            return;
                        }
                    }
                    break;
                case 3:
                    RecordsData.Source = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES3.N_KOL,TDETA_HES3.CUST_COD, TDETA_HES3.NUMBER, TDETA_HES3.TNUMBER, TDETA_HES3.TNUMBER2, TDETA_HES3.TNUMBER3, TDETA_HES3.NAME, TDETA_HES3.TOZIH, TDETA_HES3.BED_BES,  TDETA_HES3.ADDRESS , TDETA_HES3.TEL, TDETA_HES3.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER3 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES3 WHERE     (((TDETA_HES3.N_KOL) = " + CKOL.ToString() + ") AND ((TDETA_HES3.NUMBER) = " + CMOIN.ToString() + ") AND ((TDETA_HES3.TNUMBER) = " + CTAF.ToString() + ") AND ((TDETA_HES3.TNUMBER2) = " + CTAF2.ToString() + ") )").ToList();
                    N_KOL = CKOL;
                    NUMBER = CMOIN;
                    TNUMBER = (int)CTAF;
                    TNUMBER2 = (double)CTAF2;
                    TableNameLevel = "TDETA_HES3";
                    LBL_HEADER.Content = "تعریف مشتری در تفضیلی سطح 3";
                    RecordsData.Source = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().OrderBy(item => item.TNUMBER3).ToList();
                    if (!string.IsNullOrEmpty(OpenArgs))
                    {
                        var item = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().FirstOrDefault(x => x.TNUMBER3.Equals(Convert.ToInt32(OpenArgs)));
                        if (item != null)
                        {
                            RecordsData.View.MoveCurrentTo(item);
                            MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                        }
                        else
                        {
                            new Msgwin(false, "چنین کد مشتری وجود ندارد").ShowDialog();
                            Close();
                            return;
                        }
                    }
                    break;
                case 4:
                    RecordsData.Source = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES4.N_KOL,TDETA_HES4.CUST_COD, TDETA_HES4.NUMBER, TDETA_HES4.TNUMBER, TDETA_HES4.TNUMBER2, TDETA_HES4.TNUMBER3, TDETA_HES4.TNUMBER4, TDETA_HES4.NAME, TDETA_HES4.TOZIH, TDETA_HES4.BED_BES,  TDETA_HES4.ADDRESS , TDETA_HES4.TEL, TDETA_HES4.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER3 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER4 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES4 WHERE     (((TDETA_HES4.N_KOL) = " + CKOL.ToString() + ") AND ((TDETA_HES4.NUMBER) = " + CMOIN.ToString() + ") AND ((TDETA_HES4.TNUMBER) = " + CTAF.ToString() + ") AND ((TDETA_HES4.TNUMBER2) = " + CTAF2.ToString() + ") AND ((TDETA_HES4.TNUMBER3) = " + CTAF3.ToString() + ") )").ToList();
                    N_KOL = CKOL;
                    NUMBER = CMOIN;
                    TNUMBER = (int)CTAF;
                    TNUMBER2 = (double)CTAF2;
                    TNUMBER3 = (double)CTAF3;
                    TableNameLevel = "TDETA_HES4";
                    LBL_HEADER.Content = "تعریف مشتری در تفضیلی سطح 4";
                    RecordsData.Source = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().OrderBy(item => item.TNUMBER4).ToList();
                    if (!string.IsNullOrEmpty(OpenArgs))
                    {
                        var item = RecordsData.View.Cast<FCODE_CUSTOMER_MODEL>().FirstOrDefault(x => x.TNUMBER4.Equals(Convert.ToInt32(OpenArgs)));
                        if (item != null)
                        {
                            RecordsData.View.MoveCurrentTo(item);
                            MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                        }
                        else
                        {
                            new Msgwin(false, "چنین کد مشتری وجود ندارد").ShowDialog();
                            Close();
                            return;
                        }
                    }
                    break;
            }

            if (string.IsNullOrEmpty(OpenArgs))
            {
                MoveReGetData(INavigator.Jahat.LastItem);
            }

        }
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
        public void MoveReGetData(INavigator.Jahat jahat, int? custom_postiion = null)
        {
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
                    Form_Current();
                    break;
            }

            //Update CurrentViewItem
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;
                //var DBData = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>($"SELECT TOP 1 * FROM dbo.CUST_HESAB WHERE hes = N'{HEADER.HES}'").FirstOrDefault();
                var DBData = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>(@$"SELECT * FROM {TableNameLevel} WHERE IDD = {HEADER.IDD}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(FCODE_CUSTOMER_MODEL).GetProperties();
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
            //کدمشتری
            CustomerCode.Text = null;
            //نام مشتری
            NAMES.Text = null;
            //آدرس
            ADDRESS.Text = null;
            //تلفن
            TEL.Text = null;
            //کد اقتصادی
            ECODE.Text = null;
            //کدپستی
            PCODE.Text = null;
            //کد ملی
            MCODEM.Text = null;
            //سایر
            CODE_E.Text = null;
            //موبایل جهت ارسال پیامک
            MOBILE.Text = null;
            //طول جغرافیایی
            Latitude.Text = null;
            //عرض جغرافیایی
            Longitude.Text = null;

            //استان
            OSTANID.SelectedValue = null;
            //شهر
            SHAHRID.SelectedValue = null;
            //نوع مشتری
            CUST_COD.SelectedIndex = 0;
            //مسیر ویزیت
            ROUTE_NAME.SelectedValue = null;
            //شخصیت
            tob.SelectedIndex = 0;

        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var ROW = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;

                int TafziliNumber = 0;

                switch (levH)
                {
                    case 1:
                        TafziliNumber = (int)ROW.TNUMBER;
                        break;
                    case 2:
                        TafziliNumber = (int)ROW.TNUMBER2;
                        break;
                    case 3:
                        TafziliNumber = (int)ROW.TNUMBER3;
                        break;
                    case 4:
                        TafziliNumber = (int)ROW.TNUMBER4;
                        break;
                }

                //کدمشتری
                CustomerCode.Text = TafziliNumber.ToStringNullSafe();

                //نام مشتری
                NAMES.Text = ROW.NAME;
                //آدرس
                ADDRESS.Text = ROW.ADDRESS;
                //تلفن
                TEL.Text = ROW.TEL;
                //کد اقتصادی
                ECODE.Text = ROW.ECODE;
                //کدپستی
                PCODE.Text = ROW.PCODE;
                //کد ملی
                MCODEM.Text = ROW.MCODEM;
                //سایر
                CODE_E.Text = ROW.CODE_E;
                //موبایل جهت ارسال پیامک
                MOBILE.Text = ROW.MOBILE;
                //طول جغرافیایی
                Latitude.Text = ROW.Latitude.ToStringNullSafe();
                //عرض جغرافیایی
                Longitude.Text = ROW.Longitude.ToStringNullSafe();
                //استان
                OSTANID.SelectedValue = ROW.OSTANID; OSTANID.Items.Refresh();
                //شهر
                SHAHRID.SelectedValue = ROW.SHAHRID; SHAHRID.Items.Refresh();
                //نوع مشتری
                CUST_COD.SelectedValue = ROW.CUST_COD; CUST_COD.Items.Refresh();

                //مسیر ویزیت
                ROUTE_NAME.SelectedValue = ROW.ROUTE_NAME; ROUTE_NAME.Items.Refresh();

                //شخصیت
                tob.SelectedValue = ROW.TOB; tob.Items.Refresh();
                //توضیح
                TOZIH.Text = ROW.TOZIH;

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
                var itemToRemove = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<FCODE_CUSTOMER_MODEL>;
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
            FCODE_CUSTOMER_MODEL itemtoadd = null;
            switch (levH)
            {
                case 1:
                    itemtoadd = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES.N_KOL,TDETA_HES.CUST_COD, TDETA_HES.NUMBER, TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TOZIH, TDETA_HES.BED_BES,  TDETA_HES.ADDRESS , TDETA_HES.TEL, TDETA_HES.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES WHERE     (((TDETA_HES.N_KOL) = " + N_KOL + ") AND ((TDETA_HES.NUMBER) = " + NUMBER + $") ) AND TNUMBER = {TNUMBER}").FirstOrDefault();
                    break;
                case 2:
                    itemtoadd = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES2.N_KOL,TDETA_HES2.CUST_COD, TDETA_HES2.NUMBER, TDETA_HES2.TNUMBER, TDETA_HES2.TNUMBER2, TDETA_HES2.NAME, TDETA_HES2.TOZIH, TDETA_HES2.BED_BES,  TDETA_HES2.ADDRESS , TDETA_HES2.TEL, TDETA_HES2.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES2 WHERE     (((TDETA_HES2.N_KOL) = " + N_KOL + ") AND ((TDETA_HES2.NUMBER) = " + NUMBER + ") AND ((TDETA_HES2.tNUMBER) = " + TNUMBER + $") ) AND TNUMBER2 = {TNUMBER2}").FirstOrDefault();
                    break;
                case 3:
                    itemtoadd = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES3.N_KOL,TDETA_HES3.CUST_COD, TDETA_HES3.NUMBER, TDETA_HES3.TNUMBER, TDETA_HES3.TNUMBER2, TDETA_HES3.TNUMBER3, TDETA_HES3.NAME, TDETA_HES3.TOZIH, TDETA_HES3.BED_BES,  TDETA_HES3.ADDRESS , TDETA_HES3.TEL, TDETA_HES3.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER3 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES3 WHERE     (((TDETA_HES3.N_KOL) = " + N_KOL + ") AND ((TDETA_HES3.NUMBER) = " + NUMBER + ") AND ((TDETA_HES3.tNUMBER) = " + TNUMBER + ") AND ((TDETA_HES3.NUMBER2) = " + TNUMBER2 + $") ) AND TNUMBER3 = {TNUMBER3}").FirstOrDefault();
                    break;
                case 4:
                    itemtoadd = dbms.DoGetDataSQL<FCODE_CUSTOMER_MODEL>("SELECT  IDD,TDETA_HES4.N_KOL,TDETA_HES4.CUST_COD, TDETA_HES4.NUMBER, TDETA_HES4.TNUMBER, TDETA_HES4.TNUMBER2, TDETA_HES4.TNUMBER3, TDETA_HES4.TNUMBER4, TDETA_HES4.NAME, TDETA_HES4.TOZIH, TDETA_HES4.BED_BES,  TDETA_HES4.ADDRESS , TDETA_HES4.TEL, TDETA_HES4.CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM,MOBILE,ROUTE_NAME, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER2 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER3 AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER4 AS nvarchar))  AS HES, Longitude , Latitude, OSTANID,   SHAHRID ,   TOB FROM TDETA_HES4 WHERE     (((TDETA_HES4.N_KOL) = " + N_KOL + ") AND ((TDETA_HES4.NUMBER) = " + NUMBER + ") AND ((TDETA_HES4.tNUMBER) = " + TNUMBER + ") AND ((TDETA_HES4.NUMBER2) = " + TNUMBER2 + ") AND ((TDETA_HES4.NUMBER3) = " + TNUMBER3 + $") ) AND TNUMBER4 = {TNUMBER4}").FirstOrDefault();
                    break;
            }

            var underlyingCollection = RecordsData.Source as List<FCODE_CUSTOMER_MODEL>;
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                DisplayCounts();
                NewRecord = false;
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //استان
            OSTANID.ItemsSource = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();
            //شهرستان
            SHAHRID.ItemsSource = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();
            //نوع مشتری
            CUST_COD.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUSTKIND.CUST_COD, CUSTKIND.CUSTKNAME FROM CUSTKIND ORDER BY CUSTKIND.CUSTKNAME").ToList();
            CUST_COD.SelectedIndex = 0;

            //#cHECK
            //مسیر ویزیت
            ROUTE_NAME.ItemsSource = dbms.DoGetDataSQL<VISITOUR_SQL1>(@"SELECT Visit_route.ROUTE_NAME, Visit_route.ROUTE_NAME+N' - '+CUST_HESAB.NAME+N' - '+CUST_HESAB.hes AS Expr1
                                                                 FROM Visit_route
                                                                      INNER JOIN CUST_HESAB ON Visit_route.HES=CUST_HESAB.hes
                                                                 WHERE(Visit_route.RACTIVE=1)
                                                                 OPTION (MERGE JOIN)").ToList();

            //شخصیت مودیان
            List<SHAKHSIAT> theitems = new List<SHAKHSIAT>
            {
                new SHAKHSIAT { NAME = "حقیقی", CODE = 1 },
                new SHAKHSIAT { NAME = "حقوقی", CODE = 2 },
                new SHAKHSIAT { NAME = "مشارکت مدنی", CODE = 3 },
                new SHAKHSIAT { NAME = "اتباع غیر ایرانی", CODE = 4 }
            };
            tob.ItemsSource = theitems;
            tob.SelectedIndex = 0;
        }
        private void OSTANID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (OSTANID.SelectedValue is not null)
            {
                //شهرستان
                SHAHRID.ItemsSource = dbms.DoGetDataSQL<TCOD_CITY>($"SELECT CITYCODE, CITYNAME FROM TCOD_CITY WHERE OSCODE = {OSTANID.SelectedValue} ORDER BY CITYNAME").ToList();
            }
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            switch (levH)
            {
                case 1:
                    if (TNUMBER > 0)
                    {
                        CL_HESABDARI.TR("TDETA_HES", "(N_KOL = " + this.N_KOL + " )  AND (NUMBER = " + this.NUMBER + ") AND (TNUMBER = " + this.TNUMBER + ")", DateTime.Now, 1);
                    }
                    break;
                case 2:
                    if (TNUMBER2 > 0)
                    {
                        CL_HESABDARI.TR("TDETA_HES2", "(N_KOL = " + this.N_KOL + " )  AND (NUMBER = " + this.NUMBER + ") AND (TNUMBER = " + this.TNUMBER + ") AND (TNUMBER2 = " + this.TNUMBER2 + ")", DateTime.Now, 1);
                    }
                    break;
                case 3:
                    if (TNUMBER3 > 0)
                    {
                        CL_HESABDARI.TR("TDETA_HES3", "(N_KOL = " + this.N_KOL + " )  AND (NUMBER = " + this.NUMBER + ") AND (TNUMBER = " + this.TNUMBER + ") AND (TNUMBER2 = " + this.TNUMBER2 + ") AND (TNUMBER3 = " + this.TNUMBER3 + ")", DateTime.Now, 1);
                    }
                    break;
                case 4:
                    if (TNUMBER4 > 0)
                    {
                        CL_HESABDARI.TR("TDETA_HES4", "(N_KOL = " + this.N_KOL + " )  AND (NUMBER = " + this.NUMBER + ") AND (TNUMBER = " + this.TNUMBER + ") AND (TNUMBER2 = " + this.TNUMBER2 + ") AND (TNUMBER3 = " + this.TNUMBER3 + ") AND (TNUMBER4 = " + this.TNUMBER4 + ")", DateTime.Now, 1);
                    }
                    break;
            }
            AllowEdits = true;
            AllowDeletions = true;
        }
        private void Command41_Click(object sender, RoutedEventArgs e)
        {
            if (CL_HESABDARI.LETSGO("moreinfo"))
            {
                //var CUST_NO_HES = KOL + "-" + MOIN + "-" + TNUMBER.Text;
                var CUST_NO_HES = HESH;
                new FCODE_CUSTOMER_MORE(CUST_NO_HES).ShowDialog();
            }
            else
            {
                new Msgwin(false, "دسترسی ندارید").ShowDialog();
            }
        }
        private void Command25_Click(object sender, RoutedEventArgs e) //------------------ذخیره
        {
            if (!BodyIsValidCompute()) { return; }

            GetMaxNumReadyHesabs();

            var HEADER = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;

            int? NEWIDD = null;
            switch (levH)
            {
                case 1:
                    //TDETA_HES
                    try
                    {
                        if (NewRecord)
                        {
                            NEWIDD = dbms.DoGetDataSQL<int?>($@" INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, TOZIH, ADDRESS, TEL, CODE_E, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                OUTPUT INSERTED.IDD
                                VALUES({KOL},
                                {MOIN}   ,
                                {TNUMBER}  ,
                                N'{NAMES.Text.Trim()}' ,
                                N'{TOZIH.Text.Trim()}' ,
                                N'{ADDRESS.Text.Trim()}' ,
                                N'{TEL.Text.Trim()}' ,
                                N'{CODE_E.Text}' ,
                                N'{ECODE.Text}' ,
                                N'{PCODE.Text}' ,
                                N'{MCODEM.Text}' ,
                                {CUST_COD.SelectedValue}   ,
                                N'{MOBILE.Text}' ,
                                N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}' ,
                                {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())} ,
                                {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())} ,
                                {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)}   ,
                                {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)}   ,
                                {Baseknow.USERCOD}   ,
                                N'{Baseknow.UUSER}' ,
                                {tob.SelectedValue})").FirstOrDefault();

                            if (NEWIDD != null)
                            {
                                HEADER.IDD = (int)NEWIDD;
                                CustomerCode.Text = TNUMBER.ToString();
                            }

                            RefreshAfterInsert();
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                     SET  NAME = N'{NAMES.Text.Trim()}', TOZIH = N'{TOZIH.Text.Trim()}',
                                     ADDRESS = N'{ADDRESS.Text.Trim()}', TEL = N'{TEL.Text.Trim()}',
                                     CODE_E = N'{CODE_E.Text}', ECODE = N'{ECODE.Text}', PCODE = N'{PCODE.Text}',
                                     MCODEM = N'{MCODEM.Text}', CUST_COD = {CUST_COD.SelectedValue}, MOBILE = N'{MOBILE.Text}',
                                     ROUTE_NAME = N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}',
                                     Longitude = {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())}, 
                                     Latitude = {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())},
                                     OSTANID = {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)},
                                     SHAHRID = {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)} , 
                                     tob = {tob.SelectedValue}
                                     WHERE IDD = {HEADER.IDD}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            new Msgwin(false, "مشتری تکراری است!").ShowDialog();
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات").ShowDialog(); return;
                    }
                    break;
                case 2:
                    //TDETA_HES2
                    try
                    {
                        if (NewRecord)
                        {
                            NEWIDD = dbms.DoGetDataSQL<int?>($@" INSERT INTO dbo.TDETA_HES2(N_KOL, NUMBER, TNUMBER,TNUMBER2, NAME, TOZIH, ADDRESS, TEL, CODE_E, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                OUTPUT INSERTED.IDD
                                VALUES({KOL},
                                {MOIN}   ,
                                {TNUMBER}  ,
                                {TNUMBER2}  ,
                                N'{NAMES.Text.Trim()}' ,
                                N'{TOZIH.Text.Trim()}' ,
                                N'{ADDRESS.Text.Trim()}' ,
                                N'{TEL.Text.Trim()}' ,
                                N'{CODE_E.Text}' ,
                                N'{ECODE.Text}' ,
                                N'{PCODE.Text}' ,
                                N'{MCODEM.Text}' ,
                                {CUST_COD.SelectedValue}   ,
                                N'{MOBILE.Text}' ,
                                N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}' ,
                                {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())} ,
                                {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())} ,
                                {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)}   ,
                                {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)}   ,
                                {Baseknow.USERCOD}   ,
                                N'{Baseknow.UUSER}' ,
                                {tob.SelectedValue})").FirstOrDefault();

                            if (NEWIDD != null)
                            {
                                HEADER.IDD = (int)NEWIDD;
                                CustomerCode.Text = TNUMBER2.ToString();
                            }

                            RefreshAfterInsert();
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES2
                                     SET  NAME = N'{NAMES.Text.Trim()}', TOZIH = N'{TOZIH.Text.Trim()}',
                                     ADDRESS = N'{ADDRESS.Text.Trim()}', TEL = N'{TEL.Text.Trim()}',
                                     CODE_E = N'{CODE_E.Text}', ECODE = N'{ECODE.Text}', PCODE = N'{PCODE.Text}',
                                     MCODEM = N'{MCODEM.Text}', CUST_COD = {CUST_COD.SelectedValue}, MOBILE = N'{MOBILE.Text}',
                                     ROUTE_NAME = N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}',
                                     Longitude = {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())}, 
                                     Latitude = {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())},
                                     OSTANID = {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)},
                                     SHAHRID = {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)} , 
                                     tob = {tob.SelectedValue}
                                     WHERE IDD = {HEADER.IDD}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            new Msgwin(false, "مشتری تکراری است!").ShowDialog();
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات").ShowDialog(); return;
                    }
                    break;
                case 3:
                    //TDETA_HES3
                    try
                    {
                        if (NewRecord)
                        {
                            NEWIDD = dbms.DoGetDataSQL<int?>($@" INSERT INTO dbo.TDETA_HES3(N_KOL, NUMBER, TNUMBER,TNUMBER2,TNUMBER3, NAME, TOZIH, ADDRESS, TEL, CODE_E, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                OUTPUT INSERTED.IDD
                                VALUES({KOL},
                                {MOIN}   ,
                                {TNUMBER}  ,
                                {TNUMBER2}  ,
                                {TNUMBER3}  ,
                                N'{NAMES.Text.Trim()}' ,
                                N'{TOZIH.Text.Trim()}' ,
                                N'{ADDRESS.Text.Trim()}' ,
                                N'{TEL.Text.Trim()}' ,
                                N'{CODE_E.Text}' ,
                                N'{ECODE.Text}' ,
                                N'{PCODE.Text}' ,
                                N'{MCODEM.Text}' ,
                                {CUST_COD.SelectedValue}   ,
                                N'{MOBILE.Text}' ,
                                N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}' ,
                                {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())} ,
                                {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())} ,
                                {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)}   ,
                                {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)}   ,
                                {Baseknow.USERCOD}   ,
                                N'{Baseknow.UUSER}' ,
                                {tob.SelectedValue})").FirstOrDefault();

                            if (NEWIDD != null)
                            {
                                HEADER.IDD = (int)NEWIDD;
                                CustomerCode.Text = TNUMBER3.ToString();
                            }

                            RefreshAfterInsert();
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES3
                                     SET  NAME = N'{NAMES.Text.Trim()}', TOZIH = N'{TOZIH.Text.Trim()}',
                                     ADDRESS = N'{ADDRESS.Text.Trim()}', TEL = N'{TEL.Text.Trim()}',
                                     CODE_E = N'{CODE_E.Text}', ECODE = N'{ECODE.Text}', PCODE = N'{PCODE.Text}',
                                     MCODEM = N'{MCODEM.Text}', CUST_COD = {CUST_COD.SelectedValue}, MOBILE = N'{MOBILE.Text}',
                                     ROUTE_NAME = N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}',
                                     Longitude = {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())}, 
                                     Latitude = {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())},
                                     OSTANID = {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)},
                                     SHAHRID = {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)} , 
                                     tob = {tob.SelectedValue}
                                     WHERE IDD = {HEADER.IDD}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            new Msgwin(false, "مشتری تکراری است!").ShowDialog();
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات").ShowDialog(); return;
                    }
                    break;
                case 4:
                    //TDETA_HES4
                    try
                    {
                        if (NewRecord)
                        {
                            NEWIDD = dbms.DoGetDataSQL<int?>($@" INSERT INTO dbo.TDETA_HES4(N_KOL, NUMBER, TNUMBER,TNUMBER2,TNUMBER3, TNUMBER4, NAME, TOZIH, ADDRESS, TEL, CODE_E, ECODE, PCODE, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID, USERCO, USER_NAME, tob)
                                OUTPUT INSERTED.IDD
                                VALUES({KOL},
                                {MOIN}   ,
                                {TNUMBER}  ,
                                {TNUMBER2}  ,
                                {TNUMBER3}  ,
                                {TNUMBER4}  ,
                                N'{NAMES.Text.Trim()}' ,
                                N'{TOZIH.Text.Trim()}' ,
                                N'{ADDRESS.Text.Trim()}' ,
                                N'{TEL.Text.Trim()}' ,
                                N'{CODE_E.Text}' ,
                                N'{ECODE.Text}' ,
                                N'{PCODE.Text}' ,
                                N'{MCODEM.Text}' ,
                                {CUST_COD.SelectedValue}   ,
                                N'{MOBILE.Text}' ,
                                N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}' ,
                                {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())} ,
                                {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())} ,
                                {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)}   ,
                                {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)}   ,
                                {Baseknow.USERCOD}   ,
                                N'{Baseknow.UUSER}' ,
                                {tob.SelectedValue})").FirstOrDefault();

                            if (NEWIDD != null)
                            {
                                HEADER.IDD = (int)NEWIDD;
                                CustomerCode.Text = TNUMBER4.ToString();
                            }

                            RefreshAfterInsert();
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES4
                                     SET  NAME = N'{NAMES.Text.Trim()}', TOZIH = N'{TOZIH.Text.Trim()}',
                                     ADDRESS = N'{ADDRESS.Text.Trim()}', TEL = N'{TEL.Text.Trim()}',
                                     CODE_E = N'{CODE_E.Text}', ECODE = N'{ECODE.Text}', PCODE = N'{PCODE.Text}',
                                     MCODEM = N'{MCODEM.Text}', CUST_COD = {CUST_COD.SelectedValue}, MOBILE = N'{MOBILE.Text}',
                                     ROUTE_NAME = N'{(ROUTE_NAME.SelectedValue is null ? "NULL" : ROUTE_NAME.SelectedValue)}',
                                     Longitude = {(string.IsNullOrEmpty(Longitude.Text.Trim()) ? "NULL" : Longitude.Text.Trim())}, 
                                     Latitude = {(string.IsNullOrEmpty(Latitude.Text.Trim()) ? "NULL" : Latitude.Text.Trim())},
                                     OSTANID = {(OSTANID.SelectedValue is null ? "NULL" : OSTANID.SelectedValue)},
                                     SHAHRID = {(SHAHRID.SelectedValue is null ? "NULL" : SHAHRID.SelectedValue)} , 
                                     tob = {tob.SelectedValue}
                                     WHERE IDD = {HEADER.IDD}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            new Msgwin(false, "مشتری تکراری است!").ShowDialog();
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات").ShowDialog(); return;
                    }
                    break;
            }

            ROUTE_NAME_BeforeUpdate();

            CustomerCode.IsReadOnly = true;

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            if (OpenArgs is not null)
            {
            }

            NewRecord = false;
        }

        private void ROUTE_NAME_BeforeUpdate()
        {
            GetMaxNumReadyHesabs();

            if (ROUTE_NAME.SelectedValue != null)
            {
                var RST2 = dbms.DoGetDataSQL<VISITOUR_SQL2>("SELECT  DBO.VISIT_ROUTE_DTL.* FROM VISIT_ROUTE_DTL WHERE COUST_NO = '" + HESH + "'").ToList();
                var rst = dbms.DoGetDataSQL<VISITOUR_SQL2>("SELECT  DBO.VISIT_ROUTE_DTL.* FROM VISIT_ROUTE_DTL WHERE COUST_NO = '" + HESH + "' and ROUTE_NAME = '" + ROUTE_NAME.SelectedValue + "'").ToList();
                if (RST2.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "اين مشتري زير مجموعه مسير ويزيتوري :" + RST2.FirstOrDefault().ROUTE_NAME + " قبلا ثبت شده است آيا مايليد در آن مسير غير فعال شود و به اين مسير اضافه شود؟ ");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        dbms.DoExecuteSQL("UPDATE    dbo.Visit_route_dtl Set RACTIVE = 0 WHERE     (COUST_NO = N'" + HESH + "') and ROUTE_NAME <> '" + ROUTE_NAME.SelectedValue + "'");
                        if (rst.Count > 0)
                        {
                            var _RACTIVE_ = true;
                            dbms.DoExecuteSQL($@"UPDATE dbo.Visit_route_dtl
                                            SET RACTIVE = {Convert.ToByte(_RACTIVE_)}
                                            WHERE IDR = {rst.FirstOrDefault().IDR}");
                        }
                        else
                        {
                            var __COUST_NO = HESH;
                            var __RACTIVE = true;
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                             VALUES(N'{ROUTE_NAME.SelectedValue}',
                                             N'{__COUST_NO}' ,
                                             {Convert.ToByte(__RACTIVE)})");
                        }
                    }
                }
                else
                {
                    var _COUST_NO = HESH;
                    var _RACTIVE = true;
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.Visit_route_dtl(ROUTE_NAME, COUST_NO, RACTIVE)
                                             VALUES(N'{ROUTE_NAME.SelectedValue}',
                                             N'{_COUST_NO}' ,
                                             {Convert.ToByte(_RACTIVE)})");
                }
            }
        }
        private void GetMaxNumReadyHesabs()
        {
            switch (levH)
            {
                case 1:
                    if (this.TNUMBER == 0)
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT Max(TDETA_HES.TNUMBER) AS MaxOfTNUMBER FROM TDETA_HES WHERE (((TDETA_HES.N_KOL)= " + KOL + ") AND ((TDETA_HES.NUMBER)=" + MOIN + "))").FirstOrDefault();
                        if (rst != null)
                        {
                            this.TNUMBER = Convert.ToInt32(rst + 1);
                        }
                        else
                        {
                            if (rst == null)
                            {
                                new Msgwin(false, "حساب بدهكاران داراي معين يك نمي باشد لطفا آنرا تعريف كنيد.!!!" + "حساب بدهكاران : " + KOL).ShowDialog(); ;
                            }
                            else
                            {
                                this.TNUMBER = 1;
                            }
                        }
                    }
                    HESH = KOL + "-" + MOIN + "-" + this.TNUMBER;
                    break;
                case 2:
                    if (this.TNUMBER2 == 0)
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT Max(TDETA_HES2.TNUMBER2) AS MaxOfTNUMBER FROM TDETA_HES2 WHERE (((TDETA_HES2.N_KOL)= " + KOL + ") AND ((TDETA_HES2.NUMBER)=" + MOIN + ") AND ((TDETA_HES2.tNUMBER)=" + taf + "))").FirstOrDefault();
                        if (rst != null)
                        {
                            this.TNUMBER2 = (int)(rst + 1);
                        }
                        else
                        {
                            this.TNUMBER2 = 1;
                        }
                    }
                    HESH = KOL + "-" + MOIN + "-" + this.TNUMBER + "-" + this.TNUMBER2;
                    break;
                case 3:
                    if (this.TNUMBER3 == 0)
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT Max(TDETA_HES3.TNUMBER2) AS MaxOfTNUMBER FROM TDETA_HES3 WHERE (((TDETA_HES3.N_KOL)= " + KOL + ") AND ((TDETA_HES3.NUMBER)=" + MOIN + ") AND ((TDETA_HES3.tNUMBER)=" + taf + ") AND ((TDETA_HES3.tNUMBER2)=" + TAF2 + "))").FirstOrDefault();
                        if (rst != null)
                        {
                            this.TNUMBER3 = (int)(rst + 1);
                        }
                        else
                        {
                            this.TNUMBER3 = 1;
                        }
                    }
                    HESH = KOL + "-" + MOIN + "-" + this.TNUMBER + "-" + this.TNUMBER2 + "-" + this.TNUMBER3;
                    break;
                case 4:
                    if (this.TNUMBER4 == 0)
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT Max(TDETA_HES4.TNUMBER2) AS MaxOfTNUMBER FROM TDETA_HES4 WHERE (((TDETA_HES4.N_KOL)= " + KOL + ") AND ((TDETA_HES4.NUMBER)=" + MOIN + ") AND ((TDETA_HES4.TNUMBER)=" + taf + ") AND ((TDETA_HES4.TNUMBER2)=" + TAF2 + ") AND ((TDETA_HES4.TNUMBER3)=" + TAF3 + "))").FirstOrDefault();
                        if (rst != null)
                        {
                            this.TNUMBER4 = (int)(rst + 1);
                        }
                        else
                        {
                            this.TNUMBER4 = 1;
                        }
                    }
                    HESH = KOL + "-" + MOIN + "-" + this.TNUMBER + "-" + this.TNUMBER2 + "-" + this.TNUMBER3 + "-" + this.TNUMBER4;
                    break;
            }
        }
        private void MCODEM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
        }


        private bool BodyIsValidCompute()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(NAMES.Text.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری وارد نشده!" });
            }
            if (tob.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فیلد شخصیت خالی است" });
            }
            if (CUST_COD.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فیلد نوع مشتری خالی است" });
            }

            if (NAMES.Text.Trim().Length > 99)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول نام مشتری غیر مجاز است" });
            }
            if (TOZIH.Text.Trim().Length > 250)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول توضیح غیر مجاز است" });
            }
            if (ADDRESS.Text.Trim().Length > 99)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول آدرس غیر مجاز است" });
            }
            if (TEL.Text.Trim().Length > 49)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول تلفن غیر مجاز است" });
            }
            if (CODE_E.Text.Trim().Length > 19)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول فیلد سایر غیر مجاز است" });
            }
            if (ECODE.Text.Trim().Length > 19)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول کد اقتصادی غیر مجاز است" });
            }
            if (PCODE.Text.Trim().Length > 10)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول کد پستی غیر مجاز است" });
            }
            if (MOBILE.Text.Trim().Length > 54)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول موبایل غیر مجاز است" });
            }

            var ROW = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;
            string ThePhone = MOBILE.Text;
            if (string.IsNullOrEmpty(ThePhone)) { ThePhone = TEL.Text; }
            string? ExsitingCustomer = null;

            if (!string.IsNullOrEmpty(ROW?.NAME))
            {
                //اگر همین مشتری رو دوباره اصلاح کرده مهم نیست
                ExsitingCustomer = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE NAME = N'{ROW?.NAME}' ").FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(ThePhone) && !string.IsNullOrWhiteSpace(ThePhone) && string.IsNullOrEmpty(ExsitingCustomer))
            {
                string checkDuplicatesSql = @"
                                    SELECT 
                                         CASE 
                                             WHEN TEL = @MobileParam OR MOBILE = @MobileParam THEN 'Mobile'
                                             WHEN NAME = @CustomerName THEN 'Name'
                                         END AS DuplicateType,
                                         HES, NAME
                                     FROM dbo.CUST_HESAB
                                     WHERE (TEL = @MobileParam OR MOBILE = @MobileParam OR NAME = @CustomerName)";

                var duplicates = dbms.DoGetDataSQL<ExistingCustomerInfo>(
                    checkDuplicatesSql,
                    new { MobileParam = ThePhone, CustomerName = NAMES.Text?.FixPersianChars() }
                );

                bool mobileExists = duplicates.Any(d => d.DuplicateType == "Mobile");
                bool nameExists = duplicates.Any(d => d.DuplicateType == "Name");

                if (mobileExists || nameExists)
                {
                    string details = "";
                    if (mobileExists)
                    {
                        var dupe = duplicates.First(d => d.DuplicateType == "Mobile");
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"شماره موبایل '{MOBILE.Text}' قبلاً برای مشتری '{dupe.Name}' با کد حساب '{dupe.Hes}' ثبت شده است. " });
                    }
                    if (nameExists)
                    {
                        var dupe = duplicates.First(d => d.DuplicateType == "Name");
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"نام '{NAMES.Text}' قبلاً برای مشتری با کد حساب '{dupe.Hes}' ثبت شده است." });

                    }
                }
            }


            //if (NewRecord)
            //{
            //    if (string.IsNullOrEmpty(TNUMBER.Text) || TNUMBER.Text == "0")
            //    {
            //        double _tnumber = (double)dbms.DoGetDataSQL<double?>(@$"SELECT TOP 1 TNUMBER FROM TDETA_HES WHERE(((TDETA_HES.N_KOL)={KOL})AND((TDETA_HES.NUMBER)={MOIN})) ORDER BY TNUMBER DESC").FirstOrDefault();

            //        TNUMBER.Text = Convert.ToString(_tnumber + 1);
            //    }
            //    else
            //    {
            //        var _tnumber = dbms.DoGetDataSQL<double?>($@"SELECT TOP 1 TNUMBER FROM TDETA_HES
            //                                                 WHERE(((TDETA_HES.N_KOL)={KOL})AND((TDETA_HES.NUMBER)={MOIN}))AND((TDETA_HES.TNUMBER)={TNUMBER.Text})").ToList();
            //        if (_tnumber.Count > 0)
            //        {
            //            ErrosMessages.Add(new MsgModel { MessageText_U = "این کد وارد شده از قبل وجود دارد , آنرا تغییر دهید یا فیلد را خالی بگذارید" });
            //        }
            //    }
            //}

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
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

        private void Latitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CL_LMethods.IsValidDouble(Latitude.Text))
            {
                Latitude.Text = "";
                e.Handled = true;
            }
        }
        private void Longitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CL_LMethods.IsValidDouble(Longitude.Text))
            {
                Latitude.Text = "";
                e.Handled = true;
            }
        }

        private void Customer_Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = Customer_Delete_Btn.Visibility == Visibility.Visible;
            if (!Customer_Delete_Btn.IsEnabled || !IsVisible) { return; }

            var HEADER = RecordsData.View.CurrentItem as FCODE_CUSTOMER_MODEL;

            try
            {
                if (NewRecord || HEADER?.IDD == 0) { return; }

                Msgwin msgwin = new Msgwin(true, "آیا از حذف این مشتری اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    ESLAH_Click(null, null);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "تعریف مشتری",
                            recordId: $"{HESH}",
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    dbms.DoExecuteSQL($@"DELETE FROM {TableNameLevel} WHERE IDD = {HEADER.IDD}");

                    RefreshAfterDelete();

                    universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    new Msgwin(false, "حساب این مشتری دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
            }
        }
        private void ROUTE_NAME_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ROUTE_NAME.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (ROUTE_NAME.SelectedIndex > -1)
            {
                ROUTE_NAME_BeforeUpdate();
            }
        }

        private void ALLCUSTOMERS_Click(object sender, RoutedEventArgs e)
        {
            // Guard clause: bail out if CurrentItem isn't the model we expect
            if (!(RecordsData.View.CurrentItem is FCODE_CUSTOMER_MODEL selectedCustomer))
                return;

            // Collect the parts of the parent code
            var codeParts = new List<string>
            {
                N_KOL.ToStringNullSafe(),
                NUMBER.ToStringNullSafe()
            };

            if (TNUMBER > 0) codeParts.Add(TNUMBER.ToStringNullSafe());
            if (TNUMBER2 > 0) codeParts.Add(TNUMBER2.ToStringNullSafe());
            if (TNUMBER3 > 0) codeParts.Add(TNUMBER3.ToStringNullSafe());

            string HESPARENT = string.Join("-", codeParts);

            new FCODE_CUSTOMER_LST(levH, TableNameLevel, HESPARENT, selectedCustomer).Show();
        }


    }
}
