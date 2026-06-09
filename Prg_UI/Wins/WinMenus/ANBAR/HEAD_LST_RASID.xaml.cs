using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using MaterialDesignThemes.Wpf;
using Functions;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using Rpts;
using Wins.WinOther;
using static Interfaces.INavigator;
using System.ComponentModel;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using System.Windows.Data;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class HEAD_LST_RASID : Window, ISearchableWindow
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
        public HEAD_LST_RASID(double? _NumberToOpen_ = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();
            this.DataContext = this;

            if (_NumberToOpen_ != null)
            {
                NUMBER.Text = Convert.ToString(_NumberToOpen_);
                IsOpenedFromAutomation = _isAutomasion_;
            }

        }
        public bool IsOpenedFromAutomation { get; } = false;
        private NavigationManager<HEAD_LST> _navigationManager;
        public ObservableCollection<INVO_LST_FACTOR22> INVO_DATA_RASID_KHARID { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        #region LOCALMODEL
        class CMB_TAH
        {
            public string TAH { get; set; }
        }
        class CMB_MOLAH
        {
            public string MOLAH { get; set; }
        }
        #endregion

        #region IS_TAB_STOPS
        public bool MABL_K_COLUMN_TabStop { get; set; }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        InventoryManager IVM = new InventoryManager();

        UniversControl universControl = new UniversControl();
        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;

        public bool NewRecord { get; set; } = false;

        private bool chek;
        private bool _ican;

        string BEFOREDATEN = "";
        public double Meidnum { get; set; }
        public int ANBARDefaultValue { get; private set; }

        /// <summary>
        /// TAG = 1
        /// </summary>
        public string HTAG { get; set; } = "1";
        private bool NowIsReady { get; set; }

        public INVO_LST_FACTOR22 WAS_ROW_ITEM { get; set; } = new INVO_LST_FACTOR22();
        public bool ChangeIsHappend { get; private set; } = false;

        public int CURRENT_COLUMN_INDEX { get; set; }
        public int CURRENT_ROW_INDEX { get; set; }
        public string NameOfCurrentColumn { get; set; }
        public object ENTERED_VALUE_ROW { get; set; }
        public DataGridCell CURRENT_CELL_ROW { get; set; }
        public INVO_LST_FACTOR22? CURRENT_ITMES_ROW { get; set; }
        public Visual I_AM_RASID_KHAREED { get; set; }

        List<COMBOPERSONEL> rst_personel = null;
        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                DEPATMAN.IsEnabled = _ican;
                AllowAdditionEdits(_ican);
            }
        }

        public bool IsOnRequestNumber { get; set; } = false;

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is HEAD_LST item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER.Equals(Convert.ToDouble(item.NUMBER)));
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
                new SearchableProperty { DisplayName = "شماره رسید", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "تحویل گیرنده", PropertyPath = "TAH", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تحویل دهنده", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }

        #endregion

        public bool IsDataGrid_SUB_IsFocused { get; private set; }

        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_RASID_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)

        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            I_AM_RASID_KHAREED = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "RASID", new WindowInteropHelper(this).Handle);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_RASID_SUB, "RASID");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = Baseknow.UUSER;

            FILL_ALL_COMBOBOXES();


            string WhereCondition = $" WHERE (dbo.HEAD_LST.TAG = {HTAG}) ";
            _restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(Convert.ToByte(HTAG), WhereCondition);
            WhereCondition = _restrictionInfo.WhereClause;

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} ";
            }

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
            x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {HTAG}", //On Change for One Record
            Convert.ToDouble(NUMBER.Text)
            );

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            IsOnRequestNumber = Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5";
            if (IsOnRequestNumber) //فقط بر اسا شماره درخواست باشه
            {
                NUMBER1.Visibility = Visibility.Visible;
                LBL_DARKHAST.Visibility = Visibility.Visible;
            }
            else
            {
                NUMBER1.Visibility = Visibility.Hidden;
                LBL_DARKHAST.Visibility = Visibility.Hidden;
            }

            Form_Open();
            Form_Current();

            CL_LMethods.SetTabIndexes(
                DATE_N,
                NUMBER1,
                TAH,
                MOLAH,
                CUST_NO,
                BUTTON_SAVE_RASID,
                INVO_LST_RASID_SUB
                );

            GetDefaultFocus();
        }
        public class CMB_NUMBER1
        {
            public int NUMBER { get; set; }
        }

        private void GetDefaultFocus()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }

        private CL_LMethods.RestrictionInfo _restrictionInfo;
        private string GetAccessDeniedMessage()
        {
            if (_restrictionInfo?.RestrictionMessages?.Any() == true)
            {
                // ایجاد لیست محدودیت‌ها
                var restrictions = string.Join("، ", _restrictionInfo.RestrictionMessages);

                return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                    $"به آخرین شماره مجاز هدایت خواهید شد. (محدودیت: {restrictions})";
            }

            return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                $"شما به آخرین شماره مجاز هدایت خواهید شد.";
        }
        private void ShowMissingOrRestrictedMessage()
        {
            if (_navigationManager?.NUMBER_TO_OPEN == null)
            {
                return;
            }

            double requestedNumber = Convert.ToDouble(_navigationManager.NUMBER_TO_OPEN);
            bool recordExists = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 NUMBER FROM HEAD_LST WHERE NUMBER = {requestedNumber} AND TAG = {HTAG}").FirstOrDefault() != null;
            string message = recordExists ? GetAccessDeniedMessage() : "چنین شماره ای وجود ندارد";
            new Msgwin(false, message).ShowDialog();
            _navigationManager.ClearNumberToOpen();
        }
        private void RefreshAfterUpdate()
        {
            NewRecord = false;
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }
        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    ShowMissingOrRestrictedMessage();
                    return;
                }
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    //new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    ShowMissingOrRestrictedMessage();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر

                FNUMCO.Text = string.IsNullOrEmpty(HEADER_FAC?.FNUMCO.ToStringNullSafe()) ? "0" : HEADER_FAC?.FNUMCO.ToStringNullSafe(); //شماره داخلی

                NUMBER.Text = HEADER_FAC.NUMBER.ToString();

                //شماره درخواست
                NUMBER1.Text = HEADER_FAC.NUMBER1.ToString();
                if (double.TryParse(NUMBER1.Text, out double parsedValue))
                {
                    NUMBER1_TAG = parsedValue;
                }

                SADER.SelectedValue = HEADER_FAC.SADER; SADER.Items.Refresh();
                TAH.Text = HEADER_FAC.TAH.ToStringNullSafe();
                MOLAH.Text = HEADER_FAC.MOLAH.ToStringNullSafe();
                OKF.IsChecked = HEADER_FAC.OKF;

                string thevalue = HEADER_FAC.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();

                if (CUST_NO.ItemsSource == null)
                {
                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                }

                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                {
                    if (data?.NAME != null && !string.IsNullOrEmpty(thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data?.NAME });
                    }
                }

                CUST_NO.SelectedValue = HEADER_FAC.CUST_NO; //مشتری
                CUST_NO.Items.Refresh();

                SGN1usid.Tag = null; SGN2usid.Tag = null;

                SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1 ?? false);
                SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2 ?? false);

                if (rst_personel != null)
                {
                    SGN1usid.SelectedValue = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn1usid)?.IDD; SGN1usid.Items.Refresh();
                    SGN2usid.SelectedValue = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.IDD; SGN2usid.Items.Refresh();
                }

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                DEPATMAN.SelectedValue = HEADER_FAC?.DEPATMAN; DEPATMAN.Items.Refresh();

                if (HEADER_FAC?.OKF == null || HEADER_FAC?.OKF == false)
                {
                    MakeOKFReady();
                }

                ReGetdata(); //پرکردن دیتاگرید ها
                Summer(); //جمع تحویلی

                if (Convert.ToBoolean(SGN1.IsChecked))
                {
                    Command113.IsEnabled = true;
                    Command106.IsEnabled = true;
                }

                Form_Current();
            }
        }

        private void MakeOKFReady()
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                OKF.IsChecked = true;
            }
            else
            {
                OKF.IsChecked = false;
            }
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = null;
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = null;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                DATE_N.Text = null;
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (IsOnRequestNumber)
            {
                if (_navigationManager.IsNewRecord || INVO_DATA_RASID_KHARID.Any())
                {
                    if (NUMBER1.Text == "0" || NUMBER1.SelectedValue == null)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "شماره درخواست نمیتواند خالی باشد" });
                    }
                }
            }

            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری نمیتواند خالی باشد." });
            }


            if (IsNull(this.CUST_NO.SelectedValue) || this.CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " مشتري مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
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
        private bool BodyIsValid(INVO_LST_FACTOR22 TheRow)
        {
            var errors = (from object i in INVO_LST_RASID_SUB.ItemsSource
                          let c = INVO_LST_RASID_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // Validate ANBAR
            if (!int.TryParse(TheRow.ANBAR?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار صحیح انتخاب نشده" });
            }
            // Validate CODE
            if (string.IsNullOrEmpty(TheRow.CODE) || TheRow.CODE.Length > 15)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.NAME_CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کالا صحیح وارد نشده" });
            }
            // Validate MEGH
            if (!double.TryParse(TheRow.MEGH?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            // Validate MANDAH
            if (TheRow.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }
            else
            {
                //بررسی صحیح بودن واحد کالا نسبت به خود کالا
                var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + TheRow.CODE + "' AND ((VAHEDS.VAHED)= " + TheRow.VAHED_K + ")))").ToList();
                if (RSTV1.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });

                    TheRow.VAHED_K = null;
                }
                //واحد کالا بررسی مقدار کل باتوجه به نسبت
                else
                {
                    var NesbatMegh = RSTV1.FirstOrDefault().NESBAT * TheRow.MEGH;
                    if (NesbatMegh != TheRow.MEGHk)
                    {

                        TheRow.MEGHk = NesbatMegh;
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                    }
                }
            }

            //انبار خالی نباشد
            if (TheRow?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {TheRow.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (IsNull(TheRow.CODE))
            { }
            else
            {
                var RST_STUF_STK = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).ToList();
                if (RST_STUF_STK.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {TheRow.CODE} به انبار {TheRow.ANBAR} فوق تعلق ندارد." });
                }
            }



            //مقدار كالا نمي تواند صفر باشد بر اسا تنظیمات بیشتر
            if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
            {
                if (TheRow.MEGH == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار كالا نمي تواند صفر باشد." });
                }
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_RASID_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            //ManageColumnsTabindex(sender, e, "NUMBER", NUMBER_COLUMN_TabStop);

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (INVO_LST_RASID_SUB.IsKeyboardFocusWithin)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            bool isLastRow = DG.SelectedIndex >= 0 && DG.SelectedIndex == DG.Items.Count - 2;  //Last Row that is new Empty
                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    if (DG.SelectedIndex < DG.Items.Count - 1) // Ensure there is a valid next row
                                    {
                                        // Add focus to new row if needed
                                        DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                        if (DG.SelectedItem != null)
                                        {
                                            DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[INVO_LST_SUB_DEF_INDEX_COL]);
                                        }
                                    }
                                    //Dispatcher.BeginInvoke(new Action(() =>
                                    //{
                                    //    DG.BeginEdit();
                                    //}), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    else
                    {
                        if (BUTTON_SAVE_RASID.IsFocused)
                        {
                            BUTTON_SAVE_RASID_Click(null, null);
                            return;
                        }
                    }
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            catch { /*ignore*/ }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G) //Just another method
            {
                e.Handled = true; //Mark the event as handled to prevent further processing

                if (!_navigationManager.IsNewRecord && !string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از باز کردن پنجره سایر اطلاعات مطمئن هستید؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        OTHER_DTL win = new OTHER_DTL(1, CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle));
                        win.NUMBER = Convert.ToInt64(NUMBER.Text);
                        win.Show();
                    }
                }
            }

            if (!INVO_LST_RASID_SUB.IsKeyboardFocusWithin && !INVO_LST_RASID_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
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
                if (focused != null && (IsInside<TextBoxBase>(focused) || IsInside<ComboBox>(focused) || IsInside<CheckBox>(focused)))
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
        private void AllowAdditionEdits(bool ican)
        {
            if (ican is true)
                UnLockedForm();
            else
                LockedForm();
        }
        private void INVO_LST_RASID_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel || (Keyboard.IsKeyDown(Key.Escape) && Keyboard.Modifiers == ModifierKeys.Control))
            {
                return;
            }

            if (/*NowIsReady &&*/ INVO_LST_RASID_SUB != null)
            {
                if (INVO_LST_RASID_SUB.Items.Count > 0)
                {
                    NameOfCurrentColumn = e.Column.SortMemberPath;

                    DataGridColumn col1 = e.Column;
                    DataGridRow row1 = e.Row;
                    int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
                    int col_index = col1.DisplayIndex;
                    var DGCellinfo = new DataGridCellInfo(INVO_LST_RASID_SUB.Items[row_index], INVO_LST_RASID_SUB.Columns[col_index]);
                    var CurrentDGCell = CL_LMethods.GetDataGridCell(DGCellinfo);

                    CURRENT_ROW_INDEX = row_index;
                    CURRENT_COLUMN_INDEX = e.Column.DisplayIndex;


                    //CELL
                    var rowContainer = INVO_LST_RASID_SUB.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
                    DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);

                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    if (cell == null)
                    {
                        INVO_LST_RASID_SUB.ScrollIntoView(rowContainer, INVO_LST_RASID_SUB.Columns[CURRENT_COLUMN_INDEX]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    }
                    CURRENT_CELL_ROW = cell;
                    //CELL

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
                        ENTERED_VALUE_ROW = Comboval.SelectedValue;
                    else if (e.EditingElement is TextBox)
                        ENTERED_VALUE_ROW = TexboVal.Text.Trim();

                    CURRENT_ITMES_ROW = e.Row.Item as INVO_LST_FACTOR22;




                    // (INVO_LST_RASID_SUB.Items[row_index] as INVO_LST_RASID_KHARID)

                    //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MABL_K").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));

                    //انبار
                    if (e.Column.SortMemberPath == "ANBAR")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //(e.EditingElement as ComboBox).SelectedValue = WAS_ROW_ITEM.ANBAR;
                            return;
                        }
                        else
                        {
                            if ((e.Row.Item as INVO_LST_FACTOR22)?.ANBAR != null && !(CURRENT_ITMES_ROW?.CODE is null))
                            {
                                var Rst1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM STUF_STK WHERE CODE = N'{(e.Row.Item as INVO_LST_FACTOR22).CODE}' AND ANBAR = {(e.EditingElement as ComboBox).SelectedValue}").ToList();
                                if (Rst1.Count == 0)
                                {
                                    universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                                    (e.Row.Item as INVO_LST_FACTOR22).CODE = WAS_ROW_ITEM.CODE;
                                    (e.Row.Item as INVO_LST_FACTOR22).NAME_CODE = WAS_ROW_ITEM.NAME_CODE;

                                    INVO_LST_RASID_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                }
                            }
                        }
                        ANBAR_AfterUpdate();
                        //TODO
                    }
                    //نام کالا
                    if (e.Column.SortMemberPath == "NAME_CODE")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //Cleaning
                            CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            return;
                        }

                        if (CURRENT_ITMES_ROW.ANBAR == null)
                        {
                            universControl.PopNotifyShow("لطفا ابتدا انبار را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1);
                            INVO_LST_RASID_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            return;
                        }

                        #region +Enter
                        //OpenSearchKala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.NAME_CODE.ToString(), null);
                        if (CURRENT_ITMES_ROW.NAME_CODE == "+" || CURRENT_ITMES_ROW.NAME_CODE == "++" && !IsNull(CURRENT_ITMES_ROW.ANBAR))
                        {
                            SERCHK CMBSearch = new SERCHK(I_AM_RASID_KHAREED, CURRENT_ITMES_ROW.ANBAR.ToString());//Search Plusy Form Specialy for Customers
                            CMBSearch.ShowDialog();

                            if (FROM_SAERCH_KAL.CODE is null)
                            {
                                //CURRENT_ITMES_ROW.CODEKALA = null;
                                //CURRENT_ITMES_ROW.NAME = null;

                                //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                CURRENT_ITMES_ROW.CODE = null;
                                CURRENT_ITMES_ROW.NAME_CODE = null;
                                var TheCol = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                var DGCInf = new DataGridCellInfo(INVO_LST_RASID_SUB.Items[row_index], INVO_LST_RASID_SUB.Columns[TheCol]);
                                var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                                TheDGCell_MABL_K.Focus();

                                CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                return;
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.CODE = FROM_SAERCH_KAL.CODE;
                                CURRENT_ITMES_ROW.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;

                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);

                                //CURRENT_ITMES_ROW.VAHED_K = null;
                                //Cleaning
                                FROM_SAERCH_KAL.CODE = null;
                                FROM_SAERCH_KAL.NAME_CODE = null;
                            }
                        }
                        #endregion

                        #region BEFORE_UPDATE_SEARCH_FOR_VALUE_ENTERED
                        //اگر عدد وارد کرده برم سرغ کد کالا
                        else if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //اگر کد کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                            var str = $"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ITMES_ROW.ANBAR})";
                            var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>(str).FirstOrDefault();
                            if (!ReferenceEquals(FoundKala, null))
                            {
                                (e.Row.Item as INVO_LST_FACTOR22).CODE = FoundKala.CODE;
                                CURRENT_ITMES_ROW.NAME_CODE = FoundKala.NAME;

                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.CODE = null;
                                CURRENT_ITMES_ROW.NAME_CODE = null;
                                universControl.PopNotifyShow("چنین کد کالایی وجود ندارد لطفا اصلاح کنید", Pop1, Pop1Text1, Pop_Border1);

                                CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                return;
                            }
                            CODE_AfterUpdate();
                        }
                        else
                        {
                            //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                            if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE)
                            {


                                //الکی نره روی گات فوکوس دیتاگرید
                                INVO_LST_RASID_SUB.GotFocus -= INVO_LST_RASID_SUB_GotFocus;

                                //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                                //OpenSearchKala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.ANBAR.ToString(), null);
                                if (ENTERED_VALUE_ROW.ToString() == "+")
                                {
                                    SERCHK sERCHK = new SERCHK(I_AM_RASID_KHAREED, CURRENT_ITMES_ROW.ANBAR.ToString());
                                    sERCHK.ShowDialog();
                                }
                                else
                                {
                                    CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.ANBAR.ToString(), I_AM_RASID_KHAREED);
                                }

                                INVO_LST_RASID_SUB.GotFocus += INVO_LST_RASID_SUB_GotFocus;

                                if (FROM_SAERCH_KAL.CODE is null)
                                {
                                    CURRENT_ITMES_ROW.CODE = null;
                                    CURRENT_ITMES_ROW.NAME_CODE = null;

                                    //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                    var TheCol = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                    var DGCInf = new DataGridCellInfo(INVO_LST_RASID_SUB.Items[row_index], INVO_LST_RASID_SUB.Columns[TheCol]);
                                    var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                                    TheDGCell_MABL_K.Focus();

                                    CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K
                                    return;
                                }
                                else
                                {
                                    CURRENT_ITMES_ROW.CODE = FROM_SAERCH_KAL.CODE;
                                    CURRENT_ITMES_ROW.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;

                                    CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);

                                    //Cleaning
                                    FROM_SAERCH_KAL.CODE = null;
                                    FROM_SAERCH_KAL.NAME_CODE = null;
                                }
                                CODE_AfterUpdate();
                            }
                        }
                        #endregion

                    }
                    //واحد کالا
                    if (e.Column.SortMemberPath == "VAHED_K")
                    {
                        if ((CURRENT_ITMES_ROW?.VAHED_K is null) ||
                            (CURRENT_ITMES_ROW.VAHED_K < 1) ||
                            ((CURRENT_ITMES_ROW.CODE is null))
                            || (CURRENT_ITMES_ROW.NAME_CODE is null))
                        {
                            INVO_LST_RASID_SUB_CANCEL_EDIT();
                            CURRENT_ITMES_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                            return;
                        }

                        #region VAHED_K_NotInList
                        if (!(CURRENT_ITMES_ROW.VAHED_K is null) && !(CURRENT_ITMES_ROW.VAHED_K < 0))
                        {
                            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").FirstOrDefault();
                            if (RST is null)
                            {
                                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                                msgwin.ShowDialog();
                                CURRENT_ITMES_ROW.VAHED_K = null;
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * RST.NESBAT;
                            }
                        }
                        #endregion

                        VAHED_K_AfterUpdate();
                    }
                    //مقدار MEGH
                    if (e.Column.SortMemberPath == "MEGH")
                    {
                        if (CURRENT_ITMES_ROW?.ANBAR is null || CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.VAHED_K is null)
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MEGH").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));
                            CURRENT_ITMES_ROW.MEGH = 0;
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null)
                        {
                            return;
                        }
                        else
                        {
                            var tmegh = ((TextBox)e.EditingElement).Text;
                            if (string.IsNullOrEmpty(tmegh.ToStringNullSafe()))
                            {
                                CURRENT_ITMES_ROW.MEGH = 0;
                                tmegh = "0";
                            }
                            if ((e.Row.Item as INVO_LST_FACTOR22).CODE != null)
                            {
                                //TODO
                                #region MEGH_BeforeUpdate
                                double MEGHCH;
                                MEGH_BeforeUpdate_Sub();
                                #endregion

                                double min;
                                long Temp;
                                double MAND;
                                // RST.Open "SELECT CODE , MIN_M FROM STUF_DEF WHERE CODE = '" && Me.CODE && "'"
                                // If RST.RecordCount > 0 Then
                                // If IsNull(RST.Fields("MIN_M")) Then
                                min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);
                                CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                                CURRENT_ITMES_ROW.MEGH_R = CURRENT_ITMES_ROW.MEGHk;
                                var rst = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                if (rst.Count == 0)
                                {
                                    new Msgwin(false, "كالا به انبار فوق تعلق ندارد.").ShowDialog();
                                }
                                else if ((bool)Baseknow.RMOG && !IsNull((bool)Baseknow.RMOG))
                                {
                                    var rst1 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                                    if (rst1.Count > 0)
                                    {
                                        MAND = (double)rst1.FirstOrDefault();
                                        // If Math.Round(rst.Fields("MAND") - (val(Me.MEGHk.TAG) - Me.MEGHk - Me.MEGH_MAR), 2) < min And Forms![BASEKNOW]![MOJU] And (val(Me.MEGHk.TAG) > Me.MEGHk) Then
                                        if (Math.Round((double)(rst1.FirstOrDefault().Value - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*this.MEGHk.TAG*/) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU && Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*this.MEGHk.TAG*/)) > CURRENT_ITMES_ROW.MEGHk)
                                        {
                                            new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                                            CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                            CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                                            CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                                            CURRENT_ITMES_ROW.MABL = WAS_ROW_ITEM.MABL;
                                            chek = true;
                                            var rst2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                            if (rst2.Count > 0)
                                            {
                                                //rst2.FirstOrDefault().MOGODI = MAND;
                                                //rst2.FirstOrDefault().MOGODI_A = 0;
                                                // rst2.update();
                                                dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                                            }
                                        }
                                        else
                                        {
                                            //rst.Close();
                                            var rst3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                            if (rst3.Count > 0)
                                            {
                                                //rst3.FirstOrDefault().MOGODI = MAND - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*this.MEGHk.TAG*/) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR);
                                                //rst3.FirstOrDefault().MOGODI_A = 0;
                                                //rst3.update();
                                                dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                                            }
                                        }
                                    }
                                }
                                else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE/*this.CODE.TAG*/)
                                {
                                    // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (val(Me.MEGHk.TAG) - Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                                    if (Math.Round((double)(rst.FirstOrDefault().MOGODI + rst.FirstOrDefault().MOGODI_A - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*this.MEGHk.TAG*/) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                                    {
                                        new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                                        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH/*this.MEGH.TAG*/;
                                        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk /*this.MEGHk.TAG*/;
                                        CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R/*this.MEGH_R.TAG*/;
                                        chek = true;
                                    }
                                }
                                if (CURRENT_ITMES_ROW.MABL == 0)
                                {
                                    MABL_K_COLUMN_TabStop = true;
                                }
                                else
                                {
                                    MABL_K_COLUMN_TabStop = false;
                                    CURRENT_ITMES_ROW.MABL_K = Math.Round((double)(CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk));
                                }
                                #region MEGH_R_AfterUpdate()
                                MEGH_R_AfterUpdate();
                                #endregion
                                // If Me.N_MOIN <> Math.Round(Me.N_KOL * Me.MABL_K / 100) + Math.Round((Me.MABL_K - Math.Round(Me.N_KOL * Me.MABL_K / 100)) * Me.TKHN / 100) Then
                                // Me.N_MOIN = Math.Round(Me.N_KOL * Me.MABL_K / 100) + Math.Round((Me.MABL_K - Math.Round(Me.N_KOL * Me.MABL_K / 100)) * Me.TKHN / 100)
                                // End If
                            }
                        }
                        if (CURRENT_ITMES_ROW.MEGH == 0)
                        {
                            var TheCol = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                            var DGCInf = new DataGridCellInfo(INVO_LST_RASID_SUB.Items[CURRENT_ROW_INDEX], INVO_LST_RASID_SUB.Columns[TheCol]);
                            var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                            TheDGCell_MABL_K.Focus();
                        }
                        else
                        {
                            var TheCol = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MANDAH").DisplayIndex;
                            var DGCInf = new DataGridCellInfo(INVO_LST_RASID_SUB.Items[CURRENT_ROW_INDEX], INVO_LST_RASID_SUB.Columns[TheCol]);
                            var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf);
                            TheDGCell_MABL_K.Focus();
                        }
                    }

                    //WAS_ROW_ITEM = (e.Row.Item as INVO_LST_FACTOR22);
                    ChangeIsHappend = true;
                }
            }
        }
        private void INVO_LST_RASID_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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
                var filteredUnits = dbms.DoGetDataSQL<Custom_VAHEDK>(@$"SELECT DISTINCT VAHED, NAMES
                                                                FROM (
                                                                    SELECT dbo.TCOD_VAHEDS.CODE AS VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.TCOD_VAHEDS
                                                                    INNER JOIN dbo.STUF_DEF ON dbo.TCOD_VAHEDS.CODE = dbo.STUF_DEF.VAHED
                                                                    WHERE dbo.STUF_DEF.CODE = N'{CurrentRow.CODE}'
                                                                    UNION ALL
                                                                    SELECT dbo.MODULE_D.VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.MODULE_D
                                                                    INNER JOIN dbo.TCOD_VAHEDS ON dbo.MODULE_D.VAHED = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE dbo.MODULE_D.CODE = N'{CurrentRow.CODE}'
                                                                ) AS Combined").ToList();
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
        private void INVO_LST_RASID_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var REND_ROW = e.Row.Item as INVO_LST_FACTOR22;
            if (ConstructorRowDetector.IsPristine(REND_ROW)) { INVO_LST_RASID_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(REND_ROW))
            {
                INVO_LST_RASID_SUB_CANCEL_EDIT();
                return;
            }

            if (!EnsureHeaderIsReadyForDetailRow())
            {
                INVO_LST_RASID_SUB_CANCEL_EDIT();
                return;
            }

            IVM.StartTransaction(); //--------------------------------------------------------------------------------------------------------
            bool CurrentRowisNew = true;
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (!(REND_ROW is null))
            {
                if (REND_ROW.ANBAR != 0 && REND_ROW.VAHED_K != null && REND_ROW.NAME_CODE != null && REND_ROW.MEGH != 0 && REND_ROW.MEGHk != 0)
                {
                    string QRE_UPD_INVO;
                    if ((REND_ROW.id is null) || (REND_ROW.id is 0))
                    {
                        QRE_UPD_INVO = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE,MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                                                OUTPUT INSERTED.id
                                                VALUES({NUMBER.Text},1,{REND_ROW.ANBAR},NULL,N'{REND_ROW.CODE}',{REND_ROW.MEGH},{REND_ROW.MEGHk},{REND_ROW.MEGH_MAR},N'{REND_ROW.MANDAH}',{REND_ROW.MABL},{REND_ROW.MABL_K},{Convert.ToByte(REND_ROW.FROM_A)},N'{REND_ROW.N_RASID}',
                                                {REND_ROW.MEGH_R},{REND_ROW.SANAD_NO},NULL,{REND_ROW.ANBARF},{REND_ROW.VAHED_K},{REND_ROW.N_KOL},{REND_ROW.N_MOIN},{REND_ROW.N_TAF},{REND_ROW.AVRAGE},{REND_ROW.AVRAGE2},{REND_ROW.IMBAA},{REND_ROW.TOTALARZ},N'{REND_ROW.VISITOR}',{REND_ROW.TKHN},
                                                {REND_ROW.JAY ?? 0},{(REND_ROW.JAYO ?? 0)})";
                    }
                    else
                    {
                        CurrentRowisNew = false;
                        QRE_UPD_INVO = $@"UPDATE dbo.INVO_LST
                                               SET ANBAR={REND_ROW.ANBAR},
                                                   CODE=N'{REND_ROW.CODE}',	
                                                   MEGH={REND_ROW.MEGH},
                                                   MEGHk={REND_ROW.MEGHk},
                                                   MEGH_MAR={REND_ROW.MEGH_MAR},
                                                   MANDAH=N'{REND_ROW.MANDAH}',
                                                   MABL={REND_ROW.MABL},
                                                   MABL_K={REND_ROW.MABL_K},
                                                   FROM_A={Convert.ToByte(REND_ROW.FROM_A)},
                                                   N_RASID=N'{REND_ROW.N_RASID}',
                                                   MEGH_R={REND_ROW.MEGH_R},
                                                   SANAD_NO={REND_ROW.SANAD_NO},
                                                   ANBARF={REND_ROW.ANBARF},
                                                   VAHED_K={REND_ROW.VAHED_K},
                                                   N_KOL={REND_ROW.N_KOL},
                                                   N_MOIN={REND_ROW.N_MOIN},
                                                   N_TAF={REND_ROW.N_TAF},
                                                   AVRAGE={REND_ROW.AVRAGE},
                                                   AVRAGE2={REND_ROW.AVRAGE2},
                                                   IMBAA={REND_ROW.IMBAA},
                                                   TOTALARZ={REND_ROW.TOTALARZ},
                                                   VISITOR=N'{REND_ROW.VISITOR}',
                                                   TKHN={REND_ROW.TKHN},
                                                   JAY={REND_ROW.JAY ?? 0},
                                                   JAYO={REND_ROW.JAYO ?? 0}	
                                                WHERE id = {REND_ROW.id} AND TAG = 1";
                    }

                    //بررسی موجودی در صورت داشتن موجودی اعمال تغییرات
                    var items = new List<object> { REND_ROW };
                    var (errorMessages, _, _, queryOutputs) =
                        IVM.CheckInventoryAndExecuteQuery<int?>(items, QRE_UPD_INVO, null, false); //dbms.DoExecuteSQL(QRE_UPD_INVO);

                    if (queryOutputs.Any())
                    {
                        REND_ROW.id = queryOutputs.FirstOrDefault();
                    }
                    ErrosMessages.AddRange(errorMessages);
                }
                else
                {
                    universControl.PopNotifyShow(".مقادیر سطرها را بصورت صحیح وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                }
            }

            if (ErrosMessages.Any())
            {
                IsSaveSuccess = false;

                if (CurrentRowisNew)
                {
                    REND_ROW.id = null; //Bring Back to null (New State because of Rollback Transaction)
                }
                IVM.RollbackTransaction();
                INVO_LST_RASID_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(ErrosMessages);
                return;
            }
            else
            {
                IVM.CommitTransaction();
            }
            //--------------------------------------------------------------------------------------------------------

            Summer();
            PARAMS_COLUMN.Visibility = Visibility.Visible;

        }
        private void INVO_LST_RASID_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null) && INVO_LST_RASID_SUB.SelectedItem != null)
            {
                if (INVO_LST_RASID_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_RASID_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }
        private bool EnsureHeaderIsReadyForDetailRow()
        {
            if (!_navigationManager.IsNewRecord)
            {
                return true;
            }

            if (!HeaderIsValid() || (string.IsNullOrWhiteSpace(NUMBER?.Text) || NUMBER?.Text == "0"))
            {
                universControl.PopNotifyShowUp("ابتدا اطلاعات سربرگ را تکمیل و ذخیره کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }

            return true;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = INVO_LST_RASID_SUB;
                UIElement uie = e.OriginalSource as UIElement;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (BUTTON_SAVE_RASID.IsFocused)
                    {
                        BUTTON_SAVE_RASID_Click(null, null);
                        return;
                    }
                    else
                    {
                        e.Handled = true;

                        try
                        {
                            if (INVO_LST_RASID_SUB.IsKeyboardFocusWithin)
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
                if (!INVO_LST_RASID_SUB.IsKeyboardFocusWithin && !INVO_LST_RASID_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
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

            if (!_navigationManager.IsNewRecord && Convert.ToInt32(string.IsNullOrEmpty(NUMBER.Text) ? 0 : NUMBER.Text) > 0)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G)
                {
                    OTHER_DTL win = new OTHER_DTL(1, CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle));
                    win.NUMBER = Convert.ToInt64(NUMBER.Text);
                    win.Show();
                }
            }
        }
        void LockedForm()
        {
            NUMBER.IsReadOnly = true;
            DATE_N.IsReadOnly = true;
            USER_NAME.IsReadOnly = true;
            FNUMCO.IsReadOnly = true;
            INVO_LST_RASID_SUB.IsReadOnly = true;

            NUMBER1.IsEnabled = false;
            SADER.IsEnabled = false;
            TAH.IsEnabled = false;
            MOLAH.IsEnabled = false;
            CUST_NO.IsEnabled = false;
            CUST_NO2.IsEnabled = false;
            OKF.IsEnabled = false;
            BUTTON_SAVE_RASID.IsEnabled = false;
            DELETE_RASID.IsEnabled = false;
        }
        void UnLockedForm()
        {
            if (Baseknow.UPDDATE ?? false)
            {
                DATE_N.IsReadOnly = false;
            }

            USER_NAME.IsReadOnly = false;
            FNUMCO.IsReadOnly = false;
            NUMBER1.IsEnabled = true;
            SADER.IsEnabled = true;
            TAH.IsEnabled = true;
            MOLAH.IsEnabled = true;
            CUST_NO.IsEnabled = true;
            CUST_NO2.IsEnabled = true;
            OKF.IsEnabled = true;
            BUTTON_SAVE_RASID.IsEnabled = true;

            if (!_navigationManager.IsNewRecord)
            {
                DELETE_RASID.IsEnabled = true;
            }

            INVO_LST_RASID_SUB.IsReadOnly = false;
        }
        void Summer()
        {
            Text59.Text = dbms.DoGetDataSQL<double?>("SELECT SUM(MEGH_R) FROM dbo.INVO_LST WHERE TAG=1 AND NUMBER=" + NUMBER.Text).First().ToString();
        }
        void MOGUDI_UPDATE()
        {
            if (CURRENT_ITMES_ROW != null)
            {
                if (CURRENT_ITMES_ROW.CODE != null && CURRENT_ITMES_ROW.ANBAR != null)
                {
                    var query = dbms.DoGetDataSQL<STUF_STK_CSHARP>("select * from STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                    if (query.Count == 0)
                        MOGU.Text = null;
                    else
                        MOGU.Text = (query.FirstOrDefault().MOGODI + query.FirstOrDefault().MOGODI_A).ToString();
                }
            }
        }
        void SpaceRemvo(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        void AccepterOnlyNumber(TextBox TXBX, TextCompositionEventArgs e, bool CanEnterDecimals = false)
        {
            if (CanEnterDecimals)
            {
                if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text))
                && (Convert.ToChar(e.TextComposition.Text) != '.'))
                {
                    e.Handled = true;
                }
                // only allow one decimal point
                if ((Convert.ToChar(e.TextComposition.Text) == '.') && (TXBX.Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text)))
                {
                    e.Handled = true;
                }
            }
        }
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
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
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            ANBAR_COLUMN.ItemsSource = ARST;
            //ANBAR_COL.EditingElementStyle.
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نام مشتریان
            //try
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
            //}
            //catch (Exception)
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT hes, NAME  FROM CUST_HESAB OPTION (ORDER GROUP, FAST 1)").ToList();
            //}
            //CUST_NO.DisplayMemberPath = "NAME";
            //CUST_NO.SelectedValuePath = "hes";
            //CUST_NO.SelectedItem = null;

            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //تحویل دهنده
            TAH.ItemsSource = dbms.DoGetDataSQL<CMB_TAH>("SELECT  HEAD_LST.TAH FROM HEAD_LST GROUP BY HEAD_LST.TAH ORDER BY HEAD_LST.TAH").ToList();
            TAH.SelectedValuePath = "TAH";
            TAH.DisplayMemberPath = "TAH";

            //توسط
            MOLAH.ItemsSource = dbms.DoGetDataSQL<CMB_MOLAH>("SELECT  MOLAH FROM HEAD_LST GROUP BY MOLAH ORDER BY MOLAH").ToList();
            MOLAH.SelectedValuePath = "MOLAH";
            MOLAH.DisplayMemberPath = "MOLAH";

            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();


            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
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

            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";
            //PERSONEL.SelectedValue = Baseknow.USERCOD;
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            ///انبار
            SGN1usid.SelectionChanged -= SGN1usid_SelectionChanged;
            SGN1usid.ItemsSource = rst_personel;
            SGN1usid.DisplayMemberPath = "SAL_NAME";
            SGN1usid.SelectedValuePath = "IDD";
            //SGN1usid.SelectedValue = Baseknow.USERCOD;
            SGN1usid.SelectionChanged += SGN1usid_SelectionChanged;

            ///کنترل کیفیت
            SGN2usid.SelectionChanged -= SGN2usid_SelectionChanged;
            SGN2usid.ItemsSource = rst_personel;
            SGN2usid.DisplayMemberPath = "SAL_NAME";
            SGN2usid.SelectedValuePath = "IDD";
            //SGN2usid.SelectedValue = Baseknow.USERCOD;
            SGN2usid.SelectionChanged += SGN2usid_SelectionChanged;

            SADER.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "داخلی" },
                new COMBOYMODEL { ID = 1, NAME = "خارجی" }
            };
            SADER.SelectedValue = 0; SADER.Items.Refresh();

            //واحد ها
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            DEPATMAN.ItemsSource = RST;

            DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectedIndex = 0;
            DEPATMAN.SelectedItem = 0;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;


            IsOnRequestNumber = Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5";
            if (IsOnRequestNumber) // فقط وقتی OPTIONSS[17] == "5"
            {
                NUMBER1.ItemsSource = dbms.DoGetDataSQL<CMB_NUMBER1>(
                    "SELECT NUMBER FROM HEAD_LST WHERE TAG = 23 GROUP BY NUMBER ORDER BY NUMBER"
                ).ToList();
                NUMBER1.DisplayMemberPath = "NUMBER";
                NUMBER1.SelectedValuePath = "NUMBER";
            }
        }
        private void ReGetdata()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {
                List<INVO_LST_FACTOR22> QRE_LST = null;

                QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
                                                                                                              dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
                                                                                                              dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
                                                                                                              dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
                                                                                     FROM            dbo.INVO_LST LEFT OUTER JOIN
                                                                                                              dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
                                                                                                              dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
                                                                                                              dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                                                     WHERE        (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                INVO_DATA_RASID_KHARID?.Clear();
                foreach (var item in QRE_LST)
                    INVO_DATA_RASID_KHARID.Add(item);


                try
                {
                    //Focus on Cell in Row
                    if (INVO_LST_RASID_SUB.Items.Count > 0)
                    {
                        if (CURRENT_ROW_INDEX < 0 || CURRENT_ROW_INDEX >= INVO_LST_RASID_SUB.Items.Count)
                        {
                            CURRENT_ROW_INDEX = 0;
                        }

                        INVO_LST_RASID_SUB.Focus();
                        DataGridRow row = INVO_LST_RASID_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX) as DataGridRow;
                        if (row is null)
                        {
                            object item = INVO_LST_RASID_SUB.Items[CURRENT_ROW_INDEX];
                            INVO_LST_RASID_SUB.ScrollIntoView(INVO_LST_RASID_SUB.Items[CURRENT_ROW_INDEX]);
                            row = (DataGridRow)INVO_LST_RASID_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
                            INVO_LST_RASID_SUB.SelectedItem = item;

                            //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                            var col_index = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                            DataGridCell cell = CL_LMethods.GetCell(INVO_LST_RASID_SUB, row, Convert.ToInt32(col_index));
                            if (cell != null)
                                cell.Focus();
                        }
                        else
                        {
                            object item = INVO_LST_RASID_SUB.Items[CURRENT_ROW_INDEX];
                            INVO_LST_RASID_SUB.SelectedItem = item;
                            INVO_LST_RASID_SUB.ScrollIntoView(item);
                            //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                            var col_index = INVO_LST_RASID_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                            DataGridCell cell = CL_LMethods.GetCell(INVO_LST_RASID_SUB, row, Convert.ToInt32(col_index));
                            if (cell != null)
                                cell.Focus();
                        }
                    }
                }
                catch (Exception)
                {

                }


            }
        }
        private void Form_AfterUpdate1()
        {
            long num = 0;

            if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
            {
                if (TryGetLongFromText(this.NUMBER1.Text, out var requestNumber) && TryGetLongFromText(this.NUMBER.Text, out var receiptNumber))
                {
                    var rst = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM dbo.INVO_LST WHERE TAG = 23 AND NUMBER = @RequestNumber", new { RequestNumber = requestNumber }).ToList();
                    var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM dbo.INVO_LST WHERE TAG = 1 AND NUMBER = @ReceiptNumber", new { ReceiptNumber = receiptNumber }).ToList();
                    if (RST2.Count == 0)
                    {
                        //while (!rst.EOF)
                        for (int i = 0; i < rst.Count; i++)
                        {
                            //RST2.AddNew();
                            dbms.DoExecuteSQL(@"INSERT INTO dbo.INVO_LST
                                                            (
                                                                NUMBER,
                                                                TAG,
                                                                ANBAR,
                                                                CODE,
                                                                RADAH,
                                                                VAHED_K
                                                            )
                                                            VALUES
                                                            (   @ReceiptNumber,
                                                                1,
                                                                @Anbar,
                                                                @Code,
                                                                @Radah,
                                                                @VahedK
                                                                )",
                                new
                                {
                                    ReceiptNumber = receiptNumber,
                                    Anbar = rst[i].ANBAR,
                                    Code = rst[i].CODE,
                                    Radah = rst[i].id,
                                    VahedK = rst[i].VAHED_K
                                });

                            //RST2.update();
                            //rst.MoveNext();
                        }
                        //this.INVO_LST_RASID_SUB.Requery();
                    }
                }
            }
            ;
            if (!IsNull(this.NUMBER.Text) && !IsNull(this.CUST_NO.SelectedValue))
            {
                var rst = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("select * from head_lst where tag = 12 and NUMBER =  " + this.NUMBER.Text).ToList();
                string _where = " where tag = 12 and NUMBER =  " + this.NUMBER.Text;
                if (rst.Count > 0)
                {
                    if (rst.FirstOrDefault().CUST_NO != this.CUST_NO.SelectedValue.ToString())
                    {
                        dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET NUMBER = {rst.FirstOrDefault().NUMBER} , CUST_NO = {CUST_NO.SelectedValue} {_where} ");
                        //rst.update();
                        CL_HESABDARI.GENSANADKHAREED(num, num);
                    }
                }
            }
            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 1, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
            }
        }
        private void Form_AfterUpdate()
        {
            long num = 0;

            // بخش کدهای مربوط به شماره درخواست
            if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
            {
                if (TryGetLongFromText(this.NUMBER1.Text, out var requestNumber) && TryGetLongFromText(this.NUMBER.Text, out var receiptNumber))
                {
                    var rst = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM dbo.INVO_LST WHERE TAG = 23 AND NUMBER = @RequestNumber", new { RequestNumber = requestNumber }).ToList();
                    var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM dbo.INVO_LST WHERE TAG = 1 AND NUMBER = @ReceiptNumber", new { ReceiptNumber = receiptNumber }).ToList();
                    if (RST2.Count == 0)
                    {
                        for (int i = 0; i < rst.Count; i++)
                        {
                            dbms.DoExecuteSQL(@"INSERT INTO dbo.INVO_LST
                                                            (
                                                                NUMBER,
                                                                TAG,
                                                                ANBAR,
                                                                CODE,
                                                                RADAH,
                                                                VAHED_K
                                                            )
                                                            VALUES
                                                            (   @ReceiptNumber,
                                                                1,
                                                                @Anbar,
                                                                @Code,
                                                                @Radah,
                                                                @VahedK
                                                                )",
                                new
                                {
                                    ReceiptNumber = receiptNumber,
                                    Anbar = rst[i].ANBAR,
                                    Code = rst[i].CODE,
                                    Radah = rst[i].id,
                                    VahedK = rst[i].VAHED_K
                                });
                        }
                    }
                }
            }
            ;

            // --- اصلاحیه رفع مغایرت کد مشتری بین رسید (Tag 1) و فاکتور خرید (Tag 12) ---
            if (!IsNull(this.NUMBER.Text) && !IsNull(this.CUST_NO.SelectedValue))
            {
                // خواندن اطلاعات فاکتور خرید متناظر (Tag 12)
                var rst = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("SELECT * FROM dbo.HEAD_LST WHERE TAG = 12 AND NUMBER = @NUM", new { NUM = NUMBER.Text }).ToList();

                // اگر فاکتور خریدی با این شماره وجود دارد
                if (rst.Count > 0)
                {
                    var currentCustNo = CUST_NO.SelectedValue.ToString()?.Trim();
                    var factorCustNo = rst.FirstOrDefault()?.CUST_NO?.Trim();

                    // اگر کد مشتری در فاکتور با کد مشتری جدید (در رسید) متفاوت است
                    if (factorCustNo != currentCustNo)
                    {
                        var updateParams = new { CUST = currentCustNo, NUM = NUMBER.Text };

                        // 1. اصلاح سربرگ فاکتور خرید (Tag 12) -> رفع خطای مغایرت سربرگ در برنامه قدیم
                        dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET CUST_NO = @CUST WHERE TAG = 12 AND NUMBER = @NUM", updateParams);

                        // 2. اصلاح ریز کالاهای فاکتور خرید (Tag 12) -> رفع خطای مغایرت ریز در برنامه قدیم
                        dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET NUMBER = {rst.FirstOrDefault().NUMBER} WHERE TAG = 12 AND NUMBER = @NUM", updateParams);

                        // اصلاح سند حسابداری (اگر تولید شده باشد)
                        CL_HESABDARI.GENSANADKHAREED(num, num);
                    }
                }
            }
            // --- پایان اصلاحیه ---

            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 1, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
            }
        }
        private void Form_Open()
        {
            if (!CL_HESABDARI.LETSGO("ESLAHR"))
            {
                ESLAH.Visibility = Visibility.Hidden;
            }
            else
            {
                ESLAH.Visibility = Visibility.Visible;
            }
            if ((bool)Baseknow.SIGN)
            {
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
            }

            if (Strings.Mid(Baseknow.OPTIONSS, 68, 1) == "5")
            {
                PARAMS_COLUMN.Visibility = Visibility.Visible;
            }
            else
            {
                PARAMS_COLUMN.Visibility = Visibility.Hidden;
            }
        }

        private void INVO_LST_RASID_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_RASID_SUB.Dispatcher.Invoke(() =>
            {
                INVO_LST_RASID_SUB.CellEditEnding -= INVO_LST_RASID_SUB_CellEditEnding;
                INVO_LST_RASID_SUB.RowEditEnding -= INVO_LST_RASID_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVO_LST_RASID_SUB.CancelEdit();
                }
                else
                {
                    INVO_LST_RASID_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_RASID_SUB.RowEditEnding += INVO_LST_RASID_SUB_RowEditEnding;
                INVO_LST_RASID_SUB.CellEditEnding += INVO_LST_RASID_SUB_CellEditEnding;
            });
        }

        public double? NUMBER1_TAG { get; private set; } = null;
        private void NUMBER1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NUMBER1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            bool isOK = true;
            string title = "شماره درخواست";

            // Handle empty value
            if (string.IsNullOrWhiteSpace(NUMBER1.Text?.Trim()))
            {
                isOK = false;
            }

            if (_navigationManager.IsNewRecord)
            {
                string selected = NUMBER1.Text?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(selected))
                {
                    // Check if this NUMBER1 is already used in another record
                    var existingRecordNumber = dbms.DoGetDataSQL<double?>("SELECT NUMBER FROM HEAD_LST WHERE TAG = @TAG AND NUMBER1 = @NUMBER1", new { TAG = 1, NUMBER1 = selected }).FirstOrDefault();

                    if (existingRecordNumber != null)
                    {
                        new Msgwin(false, $"این {title} قبلاً برای رسید انبار شماره {existingRecordNumber} استفاده شده است. ").ShowDialog();
                        isOK = false;

                        // Check if the conflicting warehouse receipt has detail rows
                        var detailCount = dbms.DoGetDataSQL<int>("SELECT COUNT(NUMBER) AS fnum FROM INVO_LST WHERE TAG = @TAG AND NUMBER = @NUMBER", new { TAG = 1, NUMBER = existingRecordNumber }).FirstOrDefault();

                        if (detailCount > 0)
                        {
                            universControl.PopNotifyShow("این برگه دارای اطلاعات می‌باشد. ابتدا اطلاعات سطرهای زیر را حذف کنید سپس شماره درخواست جدید را وارد نمایید.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        }

                        var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER == existingRecordNumber);
                        if (itemfound == null)
                        {
                            new Msgwin(false, $"رسید انبار شماره {existingRecordNumber} در لیست فعلی شما یافت نشد، ممکن است به آن دسترسی نداشته باشید.").ShowDialog();
                        }

                        // Revert to original value
                        NUMBER1.SelectedValue = NUMBER1_TAG;
                        NUMBER1.Text = NUMBER1_TAG?.ToString();
                        NUMBER1.Items.Refresh();
                        e.Handled = true;
                        return;
                    }
                }
            }
            else // Edit existing record
            {
                if (!double.TryParse(NUMBER1.Text, out double enteredNumber1) || enteredNumber1 != NUMBER1_TAG)
                {
                    // Check if current record has detail rows before allowing NUMBER1 change
                    var detailCount = dbms.DoGetDataSQL<int>("SELECT COUNT(NUMBER) AS fnum FROM INVO_LST WHERE TAG = @TAG AND NUMBER = @NUMBER", new { TAG = 1, NUMBER = NUMBER.Text }).FirstOrDefault();  // NUMBER is the current warehouse receipt number

                    if (detailCount > 0)
                    {
                        new Msgwin(false, "این برگه دارای اطلاعات می‌باشد. ابتدا اطلاعات سطرهای زیر را حذف کنید سپس شماره درخواست جدید را وارد نمایید.").ShowDialog();

                        NUMBER1.SelectedValue = NUMBER1_TAG;
                        NUMBER1.Text = NUMBER1_TAG?.ToString();
                        NUMBER1.Items.Refresh();
                        isOK = false;
                        e.Handled = true;
                        return;
                    }
                }
            }

            // Confirmation for changing NUMBER1 on existing record
            if (NUMBER1_TAG > 0)
            {
                if (double.TryParse(NUMBER1.Text, out double newNumber1) && newNumber1 != NUMBER1_TAG && newNumber1 != 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از تغییر شماره درخواست مطمئن هستید؟");
                    msgwin.ShowDialog();

                    if (msgwin.DialogResult == false) // User chose NO
                    {
                        NUMBER1.SelectedValue = NUMBER1_TAG;
                        NUMBER1.Text = NUMBER1_TAG?.ToString();
                        NUMBER1.Items.Refresh();
                        e.Handled = true;
                        return;
                    }
                }
            }

            // Final success path
            if (!string.IsNullOrWhiteSpace(NUMBER1.Text))
            {
                NUMBER1_TAG = Convert.ToDouble(NUMBER1.Text);
            }

            if (isOK)
            {
                // InsertRowsByRequestNumber1();   // Uncomment when needed
            }
        }
        private void InsertRowsByRequestNumber1(bool isNumberSelectedNow = true)
        {
            if (isNumberSelectedNow)
                return;

            if (string.IsNullOrWhiteSpace(NUMBER1.Text) || NUMBER1.Text == "0")
                return;

            if (!double.TryParse(NUMBER1.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double requestNumber))
                return;

            const int requestTag = 23;
            const int receiptTag = 1;
            string title = "شماره درخواست";

            if (CL_HESABDARI.Signed(12, Convert.ToInt64(NUMBER.Text)))
            {
                new Msgwin(false, "براي اين رسید فاکتور صادر شده و به امضاء رسيده است .اگر ميخواهيد آنرا اصلاح کنيد بايد امضاء دوم و سوم فاکتور  برداشته شود . به کارتابل ارسال کنيد و براي مدير مالي جهت برداشتن امضا ارسال کنيد").ShowDialog();
                return;
            }

            // === New Record Handling ===
            if (_navigationManager.IsNewRecord)
            {
                var selected = NUMBER1.Text.Trim();

                var existingRecordNumber = dbms.DoGetDataSQL<double?>(
                    "SELECT NUMBER FROM HEAD_LST WHERE TAG = @TAG AND NUMBER1 = @NUMBER1",
                    new { TAG = 1, NUMBER1 = selected }
                ).FirstOrDefault();

                if (existingRecordNumber != null)
                {
                    new Msgwin(false, $"این {title} قبلاً برای رسید انبار شماره {existingRecordNumber} استفاده شده است. در حال بارگذاری آن برگه...").ShowDialog();

                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER == existingRecordNumber);
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx >= 0)
                        {
                            _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                        }
                    }
                    else
                    {
                        new Msgwin(false, $"رسید انبار شماره {existingRecordNumber} در لیست فعلی شما یافت نشد، ممکن است به آن دسترسی نداشته باشید.").ShowDialog();
                    }

                    NUMBER1.SelectedValue = NUMBER1_TAG;
                    NUMBER1.Text = NUMBER1_TAG?.ToString();
                    NUMBER1.Items.Refresh();
                    return;
                }
            }
            else // Edit existing record
            {
                if (Convert.ToDouble(NUMBER1.Text) != NUMBER1_TAG)
                {
                    var detailCount = dbms.DoGetDataSQL<int>(
                        "SELECT COUNT(NUMBER) AS fnum FROM INVO_LST WHERE TAG = @TAG AND NUMBER = @NUMBER",
                        new { TAG = 1, NUMBER = NUMBER.Text }
                    ).FirstOrDefault();

                    if (detailCount > 0)
                    {
                        new Msgwin(false, "این برگه دارای اطلاعات می‌باشد. ابتدا اطلاعات سطرهای زیر را حذف کنید سپس شماره درخواست جدید را وارد نمایید.").ShowDialog();
                        NUMBER1.SelectedValue = NUMBER1_TAG;
                        NUMBER1.Text = NUMBER1_TAG?.ToString();
                        NUMBER1.Items.Refresh();
                        return;
                    }
                }
            }

            // === Validate Request Exists ===
            var header = dbms.DoGetDataSQL<HEAD_LST>(@"SELECT TOP 1 * FROM HEAD_LST WHERE NUMBER=@Number AND TAG=@Tag", new { Number = requestNumber, Tag = requestTag }
            ).FirstOrDefault();

            if (header == null)
            {
                new Msgwin(false, "چنین شماره درخواستی وجود ندارد").Show();
                return;
            }

            // === Get Request Lines ===
            var requestLines = dbms.DoGetDataSQL<INVO_LST_FACTOR22>(@"SELECT * FROM INVO_LST WHERE NUMBER=@Number AND TAG=@Tag ORDER BY RADIF", new { Number = requestNumber, Tag = requestTag }).ToList();

            if (!requestLines.Any())
                return;

            double receiptNumber = Convert.ToDouble(NUMBER.Text);

            // === Get existing items in current receipt (CODE + ANBAR) ===
            var existingItems = dbms.DoGetDataSQL<(string Code, int? Anbar)>(
                @"SELECT CODE, ANBAR 
          FROM INVO_LST 
          WHERE NUMBER = @Number AND TAG = @Tag 
            AND CODE IS NOT NULL",
                new { Number = receiptNumber, Tag = receiptTag }
            ).ToHashSet();

            // === Filter only new items that do not exist yet ===
            var linesToAdd = requestLines
                .Where(line => !string.IsNullOrWhiteSpace(line.CODE) &&
                               !existingItems.Contains((line.CODE, line.ANBAR)))
                .ToList();

            if (!linesToAdd.Any())
            {
                // All items already exist
                return;
            }

            // === Get next RADIF ===
            int nextRadif = dbms.DoGetDataSQL<int>(
                @"SELECT ISNULL(MAX(RADIF), 0) 
          FROM INVO_LST 
          WHERE NUMBER=@Number AND TAG=@Tag",
                new { Number = receiptNumber, Tag = receiptTag }
            ).FirstOrDefault();

            // === Insert missing lines ===
            foreach (var line in linesToAdd)
            {
                nextRadif++;

                string mandahText = $"{line.MEGHk}|{line.MANDAH}".Trim();
                if (mandahText.Length > 50)
                    mandahText = mandahText.Substring(0, 50);

                dbms.DoExecuteSQL(
                    @"INSERT INTO INVO_LST
            (NUMBER, TAG, ANBAR, RADIF, CODE,
             MEGH, MEGHk, MEGH_MAR, MANDAH,
             MABL, MABL_K, FROM_A, N_RASID,
             MEGH_R, VAHED_K, N_KOL, N_MOIN,
             N_TAF, AVRAGE, CRT)
            VALUES
            (@NUMBER, @TAG, @ANBAR, @RADIF, @CODE,
             @MEGH, @MEGHk, @MEGH_MAR, @MANDAH,
             @MABL, @MABL_K, @FROM_A, @N_RASID,
             @MEGH_R, @VAHED_K, @N_KOL, @N_MOIN,
             @N_TAF, @AVRAGE, GETDATE())",
                    new
                    {
                        NUMBER = receiptNumber,
                        TAG = receiptTag,
                        ANBAR = line.ANBAR,
                        RADIF = nextRadif,
                        CODE = line.CODE,
                        MEGH = 0,
                        MEGHk = 0,
                        MEGH_MAR = 0,
                        MANDAH = mandahText,
                        MABL = 0,
                        MABL_K = 0,
                        FROM_A = line.FROM_A,
                        N_RASID = line.N_RASID,
                        MEGH_R = line.MEGH_R,
                        VAHED_K = line.VAHED_K,
                        N_KOL = 0,
                        N_MOIN = 0,
                        N_TAF = 0,
                        AVRAGE = 0
                    });
            }

            ReGetdata();
        }



        private void NUMBER1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                Msgwin msgwin = new Msgwin(true, "ذخیره را انجام نداده اید آیا مایل به انجام آن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    e.Cancel = true;
                    //BUTTON_SAVE_RASID_Click(null, null);
                }
            }

        }
        private bool TryGetLongFromText(string text, out long value)
        {
            return long.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private bool IsNull(object hTAF2)
        {
            if (hTAF2 is null)
            {
                return true;
            }
            if (!(hTAF2 is null))
            {
                return false;
            }
            return true;
        }
        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //ComboSearch CMBSearch = new ComboSearch(this.GetType().Name, I_AM_RASID_KHAREED);//Search Plusy Form Specialy for Customers
            #region CUST_NO_Exit

            {
                if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
                TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
                if (CUTSNO_TEX is null)
                {
                    return;
                }
                if (CUST_NO.SelectedValue is not null)
                {
                    if ((CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                    {
                        return;
                    }
                }

                if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("HEAD_LST_RASID", I_AM_RASID_KHAREED);//Search Plusy Form Specialy for Customers
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
                            CUST_NO.SelectedValue = null;
                            this.CUST_NO2.SelectedValue = _data_hes;
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
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO2.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            #endregion
        }
        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            #region SGN1_Click
            double MID;
            string SHARH;
            string td = "";
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 1);
            if (MID > 0d)
            {
                //dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + (Convert.ToBoolean(SGN1.IsChecked) ? "امضا شد1 " : ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",2," + this.NUMBER.Text + ",2 )");
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), "امضا شد1 ", ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var SAL = DATE_N.Text.ToRawTarikh().Substring(0, 4);
                var MAH = DATE_N.Text.ToRawTarikh().Substring(4, 2);
                var ROOZ = DATE_N.Text.ToRawTarikh().Substring(6, 2);
                var DTEN = $"{SAL}/{MAH}/{ROOZ}";
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));

                SHARH = "'رسيد انبار شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                string query = "insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1, GETDATE() ," + Baseknow.USERCOD + " )";
                dbms.DoExecuteSQL(query);
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 1);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), "امضا شد1 ", ":امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1 )");
            }
            PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if ((bool)!OKF.IsChecked)
                OKF.IsChecked = true;
            SGN1usid.SelectedValue = Baseknow.USERCOD;

            dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(SGN1.IsChecked == true, 1, 0) + $" WHERE TAG = 1 AND NUMBER = " + NUMBER.Text);
            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked)
            {
                LockedForm();
                Form_Current();
            }

            //if ((bool)SGN1.IsChecked && (bool)SGN2.IsChecked)
            //{
            //    Command106.IsEnabled = false;
            //    Command106.IsEnabled = false;
            //}

            #endregion

        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            #region SGN2_Click
            //if ((bool)SGN2.IsChecked)
            //{
            //    Command106.IsEnabled = true;
            //}
            //else
            //{
            //    Command106.IsEnabled = false;
            //}
            double MID;
            string SHARH;
            string td = "";
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 1);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + Interaction.IIf((bool)SGN2.IsChecked, "امضا شد2 ", "امضا برداشته شد2 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var SAL = DATE_N.Text.ToRawTarikh().Substring(0, 4);
                var MAH = DATE_N.Text.ToRawTarikh().Substring(4, 2);
                var ROOZ = DATE_N.Text.ToRawTarikh().Substring(6, 2);
                var DTEN = $"{SAL}/{MAH}/{ROOZ}";

                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'رسيد انبار شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1, GETDATE() ," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 1);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + Interaction.IIf((bool)SGN2.IsChecked, "امضا شد2 ", "امضا برداشته شد2 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",1," + this.NUMBER.Text + ",1 )");

            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if ((bool)!this.OKF.IsChecked)
                this.OKF.IsChecked = true;
            this.SGN2usid.SelectedValue = Baseknow.USERCOD;

            dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(SGN2.IsChecked == true, 1, 0) + $" WHERE TAG = 1 AND NUMBER = " + NUMBER.Text);

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked)
            {
                LockedForm();
                Form_Current();
            }
            //else
            //{
            //    UnLockedForm();
            //}
            #endregion
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord || !ESLAH.IsEnabled)
            {
                e.Handled = true;
                return;
            }

            if ((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false))
            {
                new Msgwin(false, "لطفا ابتدا امضا را بردارید").ShowDialog();
            }
            else
            {
                DateTime dt;
                if (!IsNull(NUMBER.Text))
                {
                    //UnLockedForm();
                    var RST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("select * from head_lst where tag = 12 and NUMBER =  " + NUMBER.Text).FirstOrDefault();
                    if (RST is null || Strings.Left(System.Convert.ToString(CL_HESABDARI.UCurrentUser()), 10) == (char)1605 + System.Convert.ToString((char)1583) + System.Convert.ToString((char)1740) + System.Convert.ToString((char)1585) + System.Convert.ToString((char)1587) + System.Convert.ToString((char)1740) + System.Convert.ToString((char)1587) + System.Convert.ToString((char)1578) + System.Convert.ToString((char)1605))
                    {
                        dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);

                        CL_LMethods.AllowDeletions(GetType().Name, true, new WindowInteropHelper(this).Handle);

                        AllowEdits = true; //CL_LMethods.AllowEdits(mygrid, true);
                        INVO_LST_RASID_SUB.IsReadOnly = false;

                        BUTTON_SAVE_RASID.IsEnabled = true;
                        DELETE_RASID.IsEnabled = true;

                        if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
                        {
                            //INVO_LST_RASID_SUB.AllowAdditions = false;
                            INVO_LST_RASID_SUB.CanUserAddRows = false;
                            //INVO_LST_RASID_SUB.CanUserDeleteRows = false;
                        }
                        else
                        {
                            INVO_LST_RASID_SUB.CanUserAddRows = true;
                            //INVO_LST_RASID_SUB.CanUserDeleteRows = true;
                        }
                        this.INVO_LST_RASID_SUB.IsReadOnly = false;
                        // If UCurrentUser() <> "َAdminister" And UCurrentUser() <> Me.USER_NAME And Left(UCurrentUser(), 10) <> ChrW(1605) && ChrW(1583) && ChrW(1740) && ChrW(1585) && ChrW(1587) && ChrW(1740) && ChrW(1587) && ChrW(1578) && ChrW(1605) Then
                        // Me.USER_NAME = UCurrentUser()
                        // DoCmd.RunCommand acCmdSaveRecord
                        // End If
                        //this["INVO_LST_RASID_SUB"].Form.Refresh();
                    }
                    else if (CL_HESABDARI.Signed(12, Convert.ToInt64(NUMBER.Text)))
                    {
                        new Msgwin(false, "براي اين رسید فاکتور صادر شده و به امضاء رسيده است .اگر ميخواهيد آنرا اصلاح کنيد بايد امضاء دوم و سوم فاکتور  برداشته شود . به کارتابل ارسال کنيد و براي مدير مالي جهت برداشتن امضا ارسال کنيد").ShowDialog();
                        return;
                    }
                    else
                    {
                        dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);

                        CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                        AllowEdits = true; // CL_LMethods.AllowEdits(mygrid, true);
                        this.INVO_LST_RASID_SUB.IsReadOnly = false;

                        BUTTON_SAVE_RASID.IsEnabled = true;
                        DELETE_RASID.IsEnabled = true;

                        if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
                        {
                            INVO_LST_RASID_SUB.CanUserAddRows = false;
                            //INVO_LST_RASID_SUB.CanUserDeleteRows = false;
                        }
                        else
                        {
                            INVO_LST_RASID_SUB.CanUserAddRows = true;
                            //INVO_LST_RASID_SUB.CanUserDeleteRows = true;
                        }
                        INVO_LST_RASID_SUB.IsReadOnly = false;
                        // If UCurrentUser() <> "َAdminister" And UCurrentUser() <> Me.USER_NAME And Left(UCurrentUser(), 10) <> ChrW(1605) && ChrW(1583) && ChrW(1740) && ChrW(1585) && ChrW(1587) && ChrW(1740) && ChrW(1587) && ChrW(1578) && ChrW(1605) Then
                        // Me.USER_NAME = UCurrentUser()
                        // DoCmd.RunCommand acCmdSaveRecord
                        // End If
                        // Me.AllowEdits = True
                        // Me.AllowDeletions = False
                        // Me![INVO_LST_RASID_SUB].Form.AllowAdditions = False
                        // Me![INVO_LST_RASID_SUB].Form.Refresh
                        new Msgwin(false, " براي اين رسيد فاكتور صادر شده است و تغيرات داده شده در فاکتور نيز اعمال ميشود دقت کنيد که بعداز اصلاح رسيد , فاکتور را هم کنترل کنيد ....!").ShowDialog();
                        // Me![INVO_LST_RASID_SUB].Form.AllowEdits = False
                    }
                }
                //CL_HESABDARI.SETSECURITY(this.GetType().Name, "RASID", 3, new WindowInteropHelper(this).Handle);
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "RASID", new WindowInteropHelper(this).Handle);
            }
        }
        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (!CL_LMethods.IsNumeric(NUMBER.Text) || NUMBER.Text == "0")
            {
                return;
            }

            if (!IsSaveSuccess)
            {
                universControl.PopNotifyShowUp("ابتدا ذخیره را انجام دهید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return;
            }

            //این بخش هنوز تکمیل نیست باید بعدا تکمیل بشود 
            //#Check Matter
            if ((bool)Baseknow.LOCKFAP)
            {
                OKF.IsChecked = true;
            }

            //DoCmd.OpenReport("HAVALAH_ANVAR_VROUD", acPreview, "", "NUMBER =" + this.NUMBER.Text + " AND TAG =" + this.HTAG);
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HAVALAH_ANVAR_VROUD.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["NUM_PARAM"] = NUMBER.Text;


            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM         dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").FirstOrDefault();
            if (!(rst is null))
                (report.GetComponentByName("vazn") as StiText).Text = "وزن كل به كيلو : " + Strings.Format(Math.Round((double)rst), "#,##");

            DateTime dt = DateTime.Now;
            (report.GetComponentByName("zaman") as StiText).Text = $"{Tarikh.SlashyFullDate} - {Tarikh.GetMiladiDateTimeForSQL(true)}";


            //report.Compile();
            //report.Render();
            //report.Show();
            //pathreport?.Dispose();
            new WINRPT(report, "چاپ رسید انبار").Show();

            LockedForm();
            if ((bool)OKF.IsChecked)
            {
                CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);
                // CL_LMethods.AllowEdits(mygrid, false);
                INVO_LST_RASID_SUB.IsReadOnly = true;
                ESLAH.IsEnabled = true;
            }
        }
        private void Command113_Click(object sender, RoutedEventArgs e)
        {
            if (!CL_LMethods.IsNumeric(NUMBER.Text) || NUMBER.Text == "0")
            {
                return;
            }

            if (!IsSaveSuccess)
            {
                universControl.PopNotifyShowUp("ابتدا ذخیره را انجام دهید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return;
            }

            string rptname;
            if (Strings.Mid(Baseknow.OPTIONSS, 10, 1) == "5")
            {
                rptname = "IK" + Conversion.Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2));
            }
            else
            {
                rptname = "IK1";
            }
            if (rptname == "IK1")
            {
                // DoCmd.OpenReport rptname, acViewPreview, "", "NUMBER =" && Me.NUMBER && " AND TAG = 2"
                //foreach (Printer prt in VISUAL_BASIC_C.Application.Printers)
                //{
                //    if (Left(prt.DeviceName, 17) == Conversions.ToDouble("BTP-2002NP(U) 1 ("))
                //    {
                //        VISUAL_BASIC_C.Application.Printer = prt;
                //    }
                //}
                //DoCmd.OpenReport(rptname, acPreview, "", "NUMBER =" + this.NUMBER.Text, default, 2);
            }
            else
            {
                // Application.Printer = Nothing
                //DoCmd.OpenReport(rptname, acPreview, "", "TAG = 1 and NUMBER =" + this.NUMBER.Text, default, 2);
            }
            if ((bool)Baseknow.LOCKFAP)
            {
                this.OKF.IsChecked = true;
            }
            //DoCmd.RunCommand(acCmdSaveRecord);
            if ((bool)this.OKF.IsChecked)
            {
                //this.AllowDeletions = false;
                CL_LMethods.AllowDeletions(GetType().Name, false, new WindowInteropHelper(this).Handle);
                this.AllowEdits = false;
                this.INVO_LST_RASID_SUB.IsReadOnly = true;

                //this.INVO_LST_RASID_SUB.CanUserDeleteRows = false; //this.INVO_LST_RASID_SUB.Form.AllowDeletions = false;
                this.ESLAH.IsEnabled = true;
            }

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.{rptname}.mrt");
            if (pathreport != null)
            {
                report.Load(pathreport);
                ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

                report["NUMBER_Param"] = NUMBER.Text;
                report["TAG_Param"] = 1;

                DateTime dt = DateTime.Now;
                (report.GetComponentByName("zaman") as StiText).Text = $"{Tarikh.SlashyFullDate} - {Tarikh.GetMiladiDateTimeForSQL(true)}";
                (report.GetComponentByName("zaman1") as StiText).Text = $"{Tarikh.SlashyFullDate} - {Tarikh.GetMiladiDateTimeForSQL(true)}";

                (report.GetComponentByName("User") as StiText).Text = Baseknow.UUSER;
                (report.GetComponentByName("Users") as StiText).Text = Baseknow.UUSER;

                (report.GetComponentByName("TFADDRESS") as StiText).Text = Baseknow.TFADDRESS;
                (report.GetComponentByName("TFADDRES") as StiText).Text = Baseknow.TFADDRESS;

                (report.GetComponentByName("TFTEL") as StiText).Text = Baseknow.TFTEL;
                (report.GetComponentByName("TFTE") as StiText).Text = Baseknow.TFTEL;

                (report.GetComponentByName("MCODEM") as StiText).Text = Baseknow.MCODEM;
                (report.GetComponentByName("MCODE") as StiText).Text = Baseknow.MCODEM;

                (report.GetComponentByName("ECODE") as StiText).Text = Baseknow.ECODE;
                (report.GetComponentByName("ECOD") as StiText).Text = Baseknow.ECODE;

                //report.Compile();
                //report.Render();
                //report.Show();

                new WINRPT(report, "قبض باسکول").Show();
            }

        }
        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null; PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (PERSONEL.SelectedIndex > -1 && !(PERSONEL.SelectedValue is null) && Convert.ToInt32(NUMBER.Text) > 0 && CUST_NO.SelectedIndex > -1)
            {
                Meidnum = CL_HESABDARI.PERSONELUpdate(Convert.ToInt32(HTAG), Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue),
                    "'رسید شماره: " + NUMBER.Text + " مورخ " +
                    Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##")
                    + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            else
            {
                //Not in List
                if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده نادیده بگیر
                string personel_tex = ((TextBox)PERSONEL.Template.FindName("PART_EditableTextBox", PERSONEL)).Text;
                //if (Information.IsNumeric(NewData))
                if (int.TryParse(personel_tex, out _))
                {
                    var RST = dbms.DoGetDataSQL<int?>("select idd from sala_dtl where idd = " + personel_tex).FirstOrDefault();
                    if (!(RST is null))
                    {
                        this.PERSONEL.SelectedValue = RST;
                    }
                }
                else
                {
                    //DoCmd.OpenForm("SelectUser", acFormDS, default, "sal_name like N'%" + CODESAL(NewData) + "%' or sal_name like N'%" + CODESAL(Fixp(NewData)) + "%' or sal_name like N'%" + CODESAL(Fixpi(NewData)) + "%'", default, acDialog, 3);
                    SelectUser selectUser = new SelectUser("sal_name like N'%" + CL_HESABDARI.CODESAL(personel_tex) + "%' or sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(personel_tex)) + "%' or sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixpi(personel_tex)) + "%'", new WindowInteropHelper(this).Handle);
                    selectUser.ShowDialog();
                }
            }
        }

        private void Form_Current()
        {
            bool ghat;
            this.PERSONEL.Visibility = Visibility.Visible;
            // میاید چک میکند اگر فرم دارای رکورد در دیتاگرید نباشد دکمه های گزارش را قفل میکند
            if (INVO_DATA_RASID_KHARID.Count > 0)
            {
                this.Command106.IsEnabled = true;
                this.Command113.IsEnabled = true;
            }
            else
            {
                this.Command106.IsEnabled = false;
                this.Command113.IsEnabled = false;
            }

            if (INVO_DATA_RASID_KHARID.Count > 0)
            {
                this.INVO_LST_RASID_SUB.IsReadOnly = false;

                AllowEdits = true;
                CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
                {
                    INVO_LST_RASID_SUB.CanUserAddRows = false;
                }
                else
                {
                    INVO_LST_RASID_SUB.CanUserAddRows = true;
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("SELECT * FROM HEAD_LST WHERE TAG = 12 AND NUMBER =  " + this.NUMBER.Text).ToList();
                if (rst.Count == 0 /*|| Strings.Left((string)CL_HESABDARI.UCurrentUser(), 10) == Convert.ToDouble(Convert.ToString(Strings.ChrW(1605)) + Strings.ChrW(1583) + Strings.ChrW(1740) + Strings.ChrW(1585) + Strings.ChrW(1587) + Strings.ChrW(1740) + Strings.ChrW(1587) + Strings.ChrW(1578) + Strings.ChrW(1605))*/)
                {
                    AllowEdits = true;
                    CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                    if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
                    {
                        INVO_LST_RASID_SUB.CanUserAddRows = false;
                    }
                    else
                    {
                        INVO_LST_RASID_SUB.CanUserAddRows = true;
                    }
                    INVO_LST_RASID_SUB.IsReadOnly = false;
                }
                else
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);
                    AllowEdits = false;
                }
            }
            if ((bool)Baseknow.SIGN)
            {
                if ((bool)this.SGN1.IsChecked)
                {
                    this.Command106.IsEnabled = true;
                    this.Command113.IsEnabled = true;
                }
                else
                {
                    this.Command106.IsEnabled = false;
                    this.Command113.IsEnabled = false;
                }
            }
            if ((bool)OKF?.IsChecked && !NewRecord)
            {
                AllowEdits = false;
                CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);
                this.INVO_LST_RASID_SUB.IsReadOnly = true;
                this.ESLAH.IsEnabled = true;
            }
            //else
            //{
            //    CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
            //    AllowEdits = true;
            //    this.INVO_LST_RASID_SUB.IsReadOnly = false;
            //    this.ESLAH.IsEnabled = false;
            //}
            if (Convert.ToDouble(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 1, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
            }

            if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
            {
                ANBAR_COLUMN.IsReadOnly = true;
                NAME_CODE_COLUMN.IsReadOnly = true;
                VAHED_K_COLUMN.IsReadOnly = true;
            }
            else
            {
                ANBAR_COLUMN.IsReadOnly = false;
                NAME_CODE_COLUMN.IsReadOnly = false;
                VAHED_K_COLUMN.IsReadOnly = false;
            }

            INVO_LST_RASID_SUB.IsReadOnly = true;
        }

        private void INVO_LST_RASID_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(INVO_LST_RASID_SUB.Items.Count < 1) && !(INVO_LST_RASID_SUB.SelectedItem is null))
                {
                    if (INVO_LST_RASID_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        //WAS_ROW_ITEM = (INVO_LST_FACTOR22)INVO_LST_RASID_SUB.SelectedItem;

                        if (!(INVO_LST_RASID_SUB.CurrentCell.Column is null))
                            CURRENT_COLUMN_INDEX = INVO_LST_RASID_SUB.CurrentCell.Column.DisplayIndex;

                        CURRENT_ROW_INDEX = INVO_LST_RASID_SUB.SelectedIndex;

                        var CurrentRow = INVO_LST_RASID_SUB.SelectedItem as INVO_LST_FACTOR22;
                        if (CurrentRow?.CODE != null && CurrentRow?.ANBAR != null)
                        {
                            MOGU.Text = CL_HESABDARI.GetStkKala(CurrentRow.CODE, (double)CurrentRow.ANBAR).ToStringNullSafe();
                        }
                    }
                }
            }
        }
        private void INVO_LST_RASID_SUB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_RASID_SUB.SelectedItem != null && INVO_LST_RASID_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (INVO_LST_RASID_SUB.Items.Count > 0)
                {
                    //WAS_ROW_ITEM = (INVO_LST_FACTOR22)INVO_LST_RASID_SUB.SelectedItem;
                    if (!(INVO_LST_RASID_SUB.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = INVO_LST_RASID_SUB.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = INVO_LST_RASID_SUB.SelectedIndex;
                }
            }
        }
        private void ANBAR_AfterUpdate()
        {
            // CURRENT_ITMES_ROW.CODE.Requery();
            if (!(IsNull(CURRENT_ITMES_ROW.CODE) || CURRENT_ITMES_ROW.CODE == ""))
            {
                MEGH_AfterUpdate();
                if (chek)
                {
                    // this.Undo();
                }
            }
        }
        private void MEGH_AfterUpdate()
        {
            if (CURRENT_ITMES_ROW?.VAHED_K == null || CURRENT_ITMES_ROW?.ANBAR == null)
            {
                return;
            }

            // var rst = new ADODB.Recordset();
            double min;
            long Temp;
            double MAND;
            // RST.Open "SELECT CODE , MIN_M FROM STUF_DEF WHERE CODE = '" && Me.CODE && "'"
            // If RST.RecordCount > 0 Then
            // If IsNull(RST.Fields("MIN_M")) Then
            min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);
            CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
            CURRENT_ITMES_ROW.MEGH_R = CURRENT_ITMES_ROW.MEGHk;
            var rst = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
            if (rst.Count == 0)
            {
                new Msgwin(false, "كالا به انبار فوق تعلق ندارد.").ShowDialog();
            }
            else if ((bool)Baseknow.RMOG && !IsNull((bool)Baseknow.RMOG))
            {
                var rs = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                if (rs.Count > 0)
                {
                    MAND = (double)rs.FirstOrDefault();
                    // If Math.Round(rst.Fields("MAND") - (val(Me.MEGHk.TAG) - Me.MEGHk - Me.MEGH_MAR), 2) < min And Forms![BASEKNOW]![MOJU] And (val(Me.MEGHk.TAG) > Me.MEGHk) Then
                    if (Math.Round((double)(MAND - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU && Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk)) > CURRENT_ITMES_ROW.MEGHk)
                    {
                        new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                        CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                        CURRENT_ITMES_ROW.MABL = WAS_ROW_ITEM.MABL;
                        chek = true;
                        // rs.Close();
                        var rst1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                        if (rst1.Count > 0)
                        {
                            //rst.Fields("MOGODI") = MAND;
                            //rst.Fields("MOGODI_A") = 0;
                            //rst.update();
                            dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                        }
                    }
                    else
                    {
                        // rs.Close();
                        var rst2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                        if (rst2.Count > 0)
                        {
                            //rst.Fields("MOGODI") = MAND - (Conversion.Val(this.MEGHk.TAG) - this.MEGHk - this.MEGH_MAR);
                            //rst.Fields("MOGODI_A") = 0;
                            //rst.update();
                            dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                        }
                    }
                }
            }
            else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
            {
                // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (val(Me.MEGHk.TAG) - Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                if (Math.Round((double)(rst.FirstOrDefault().MOGODI + rst.FirstOrDefault().MOGODI_A - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                {
                    new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                    CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                    CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                    CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                    chek = true;
                }
            }
            if (CURRENT_ITMES_ROW.MABL == 0)
            {
                MABL_K_COLUMN_TabStop = true;
            }
            else
            {
                MABL_K_COLUMN_TabStop = false;
                CURRENT_ITMES_ROW.MABL_K = Math.Round((double)(CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk));
            }
            // If Me.N_MOIN <> Math.Round(Me.N_KOL * Me.MABL_K / 100) + Math.Round((Me.MABL_K - Math.Round(Me.N_KOL * Me.MABL_K / 100)) * Me.TKHN / 100) Then
            // Me.N_MOIN = Math.Round(Me.N_KOL * Me.MABL_K / 100) + Math.Round((Me.MABL_K - Math.Round(Me.N_KOL * Me.MABL_K / 100)) * Me.TKHN / 100)
            // End If
            //rst.Close();
        }

        private void CODE_AfterUpdate()
        {
            if (CURRENT_ITMES_ROW != null)
            {
                //var rst = new ADODB.Recordset();
                var min = default(double);
                double MAND;
                // this.VAHED_K.Requery();

                //موجودی
                //TotallMOGU();

                //  rst.Close();
                //var rst1 = dbms.DoGetDataSQL<QRE_KH_02>("select VAHED , MIN_M from STUF_DEF where CODE = '" + CURRENT_ITMES_ROW.CODE + "'").ToList();
                //if (rst1.Count > 0)
                //{
                //    CURRENT_ITMES_ROW.VAHED_K = rst1.FirstOrDefault().VAHED;
                //    // If IsNull(RST.Fields("MIN_M")) Then
                //    min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);
                //    // Else
                //    // min = Getmin(Me.ANBAR, Me.CODE)
                //    // End If
                //}
                //rst.Close();
                var IsNewRow = CURRENT_ITMES_ROW.id is null or 0;
                if (!IsNewRow)
                {
                    //var rst2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("select * from STUF_STK where CODE = '" + WAS_ROW_ITEM.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                    var rst2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("select * from STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                    if (rst2.Count == 0)
                    {
                        new Msgwin(false, "كالا به انبار فوق تعلق ندارد.").ShowDialog();
                    }
                    else if ((bool)Baseknow.RMOG && !IsNull((bool)Baseknow.RMOG))
                    {
                        var rst3 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + WAS_ROW_ITEM.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                        if (rst3.Count > 0)
                        {
                            MAND = (double)rst3.FirstOrDefault();
                            if (Math.Round((double)(MAND - CURRENT_ITMES_ROW.MEGHk), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                            {
                                // If Math.Round(rst.Fields("MAND") - Me.MEGHk, Forms![BASEKNOW]![DIG]) < Math.Round(min, Forms![BASEKNOW]![DIG]) And Me.ANBAR <> 0 And Forms![BASEKNOW]![MOJU] Then
                                new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();

                                CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                chek = true;
                                // rst.Close();
                                var rst4 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                if (rst4.Count > 0)
                                {
                                    //rst.Fields("MOGODI") = MAND;
                                    //rst.Fields("MOGODI_A") = 0;
                                    //rst.update();
                                    dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                                }
                            }
                            else
                            {
                                var rst4 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                                if (rst4.Count > 0)
                                {
                                    MAND = (double)rst4.FirstOrDefault();
                                    // rst4.Close();
                                    var rst5 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                    if (rst5.Count > 0)
                                    {
                                        //rst.Fields("MOGODI") = MAND + this.MEGHk;
                                        //rst.Fields("MOGODI_A") = 0;
                                        //rst.update();
                                        dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI={MAND + CURRENT_ITMES_ROW.MEGHk} , MOGODI_A=0 WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);

                                    }
                                }
                            }
                        }
                    }
                    else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                    {
                        // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - (val(Me.MEGHk.TAG) - Me.MEGH_MAR)) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                        if (Math.Round((double)(rst2.FirstOrDefault().MOGODI + rst2.FirstOrDefault().MOGODI_A - CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                        {
                            new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();

                            CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                            chek = true;
                        }
                    }
                    else if (Math.Round((double)(rst2.FirstOrDefault().MOGODI + rst2.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                    {
                        // If (rst.Fields("MOGODI") + rst.Fields("MOGODI_A")) - (Me.MEGHk - Me.MEGH_MAR) < min And Forms![BASEKNOW]![MOJU] And Me.ANBAR > 0 Then
                        new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();

                        CURRENT_ITMES_ROW = CURRENT_ITMES_ROW;
                        chek = true;
                    }
                }

            }
        }
        private void Form_AfterUpdate_Sub()
        {
            if (USER_NAME != CL_HESABDARI.UCurrentUser())
            {
                USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();
            }
            if (/*this.RecordsetClone.RecordCount > 0*/ INVO_DATA_RASID_KHARID.Count > 0)
            {
                Command106.IsEnabled = true;
                Command113.IsEnabled = true;
            }
            else
            {
                Command106.IsEnabled = false;
                Command113.IsEnabled = false;
            }
        }

        private void MEGH_BeforeUpdate_Sub()
        {
            double MEGHCH;
            if (!_navigationManager.IsNewRecord && Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
            {
                if (CURRENT_ITMES_ROW == null)
                {
                    // Try to recover from DataGrid current item
                    if (INVO_LST_RASID_SUB.SelectedItem is INVO_LST_FACTOR22 selectedRow)
                    {
                        CURRENT_ITMES_ROW = selectedRow;
                    }
                    else
                    {
                        return; // Cannot proceed safely
                    }
                }

                if (CURRENT_ITMES_ROW?.RADAH != null)
                {
                    var rst = dbms.DoGetDataSQL<QRE_KH_0>("SELECT     NUMBER,ID, TAG, ANBAR, CODE, VAHED_K, MEGH, MEGHk FROM dbo.INVO_LST WHERE (TAG = 23) And   (NUMBER = " + NUMBER1.Text.ToStringNullSafe() + ") AND  (ID = " + CURRENT_ITMES_ROW.RADAH + ")").ToList();
                    if (rst.Count == 0)
                    {
                        new Msgwin(false, "مغايرت در درخواست خريد و رسيد!!!.").ShowDialog();
                    }
                    else
                    {
                        var RST2 = dbms.DoGetDataSQL<double?>("SELECT SUM(dbo.INVO_LST.MEGH) AS MEGHS, SUM(dbo.INVO_LST.MEGHk) AS MEGHkS FROM  dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE  (dbo.INVO_LST.TAG = 1) And   (dbo.HEAD_LST.NUMBER1 = " + NUMBER1.Text + ") AND (dbo.HEAD_LST.NUMBER <> " + NUMBER.Text + " ) AND  (dbo.INVO_LST.RADAH = " + CURRENT_ITMES_ROW.RADAH + ") ").ToList();
                        if (RST2.Count == 0 || IsNull(RST2.FirstOrDefault()))
                        {
                            MEGHCH = (double)rst.FirstOrDefault().MEGH;
                        }
                        else
                        {
                            MEGHCH = ((double)(rst.FirstOrDefault().MEGH - RST2.FirstOrDefault()));
                        }
                        if (CURRENT_ITMES_ROW.MEGH > MEGHCH)
                        {
                            new Msgwin(false, "مقدار وارده از مقدار درخواستي بيشتر است!  " + '\n' + " مقدار رسيد شده قبلي : " + RST2.FirstOrDefault() + "       مقدار درخواستي :  " + rst.FirstOrDefault().MEGH + " مانده :  " + MEGHCH).ShowDialog();
                        }
                    }
                }

            }
        }
        private void MEGH_R_AfterUpdate()
        {
            // var rst = new ADODB.Recordset();
            if (CURRENT_ITMES_ROW.MABL == 0)
            {
                CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH_R;
                CURRENT_ITMES_ROW.MEGH = CURRENT_ITMES_ROW.MEGH_R / CL_HESABDARI.GETNESBAT(CURRENT_ITMES_ROW.CODE, (int)CURRENT_ITMES_ROW.VAHED_K);
                MEGH_AfterUpdate();
            }
            //rst.Close();
            Command106.IsEnabled = true;
        }
        private void PARAMS_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                //{{DataGrid.NewItemPlaceholder}}
                var BTN_DATA = (sender as Button).Tag;
                if (BTN_DATA.ToStringNullSafe() is "{DataGrid.NewItemPlaceholder}") { return; }

                var SATR_RASID_KARID = (INVO_LST_FACTOR22)BTN_DATA;
                if (!IsNull(SATR_RASID_KARID.id))
                {
                    var Id = dbms.DoGetDataSQL<Int64?>("Select id From dbo.IVO_EXTENDED WHERE id=" + SATR_RASID_KARID.id).FirstOrDefault();
                    if (Id == null)
                        dbms.DoExecuteSQL("INSERT INTO dbo.IVO_EXTENDED (id,FLD1,FLD2,FLD3,FLD4,FLD5,FLD6,FLD7,FLD8,FLD9,FLD10,CRT,UID) VALUES(" + SATR_RASID_KARID.id + ",0,0,0,0,0,0,0,0,0,0,GETDATE()," + Baseknow.USERCOD + ")");
                    new ZF_IVO_EXTENDED((int)SATR_RASID_KARID.id, I_AM_RASID_KHAREED).ShowDialog();
                }
                else
                {
                    universControl.PopNotifyShowUp("ابتدا دکمه ذخیره را بزنید سپس قسمت پارامتر ها رو باز کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    return;
                }
            }
        }
        private void VAHED_K_AfterUpdate()
        {
            MEGH_AfterUpdate();
            // Me.MEGHk = Me.MEGH * GETNESBAT(Me.CODE, Me.VAHED_K)
            // MEGH_R = Me.MEGHk
        }

        bool IsSaveSuccess = true;
        private void BUTTON_SAVE_RASID_Click(object sender, RoutedEventArgs e)
        {
            IsSaveSuccess = true;

            if (!HeaderIsValid())
            {
                IsSaveSuccess = false;
                return;
            }

            if (!DoCmdSaveHeader()) //Header Save
            {
                IsSaveSuccess = false;
                return;
            }

            if (IsOnRequestNumber)
            {
                InsertRowsByRequestNumber1(false);
            }

            ChangeIsHappend = false;
            Form_AfterUpdate();
            Form_AfterUpdate_Sub();

            universControl.PopNotifyShow(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            INVO_LST_RASID_SUB.IsReadOnly = false;
            if (INVO_DATA_RASID_KHARID.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            IsSaveSuccess = true;
        }

        private bool DoCmdSaveHeader()
        {
            try
            {
                string tah = ((TextBox)TAH.Template.FindName("PART_EditableTextBox", TAH)).Text;
                string molah = ((TextBox)MOLAH.Template.FindName("PART_EditableTextBox", MOLAH)).Text;

                if (_navigationManager.IsNewRecord) //Frist New Fresh Insert Add   
                {
                    double num = 0;
                    using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                        {
                            //Fake Query for Lock Table
                            db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                            //Fake Query for Lock Table

                            var rst_11 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=1))", null, transaction).FirstOrDefault();
                            if (rst_11 == null || rst_11 == 0 || ReferenceEquals(rst_11, null))
                            {
                                if (Baseknow.STHFR > 0)
                                {
                                    num = Baseknow.STHFR;
                                }
                                else
                                {
                                    num = 1;
                                }
                                NUMBER.Text = num.ToString();
                                NUMBER.UpdateLayout();
                            }
                            else
                            {
                                num = Convert.ToInt64(rst_11 + 1);
                                NUMBER.Text = num.ToString();
                                NUMBER.UpdateLayout();
                            }

                            int? fnumcoValue = null;
                            if (int.TryParse(FNUMCO.Text, out int fnumcoParsed))
                            {
                                fnumcoValue = fnumcoParsed;
                            }
                            int? sgn1usidValue = SGN1usid.SelectedValue as int?;
                            int? sgn2usidValue = SGN2usid.SelectedValue as int?;
                            int? depatmanValue = DEPATMAN.SelectedValue as int?;
                            int? saderValue = SADER.SelectedValue as int?;
                            const string QRE_HEADINSUP_PARAMETRIC = @"
                            INSERT INTO dbo.HEAD_LST
                            (
                                NUMBER,NUMBER1, TAG, DATE_N, TAH, VAS, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, 
                                MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, 
                                ANBARF, FNUMCO, SHIFT, USER_NAME, SGN1, SGN2, SGN4, MBAA, HMBAA, 
                                TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, 
                                OKTIME, JAY, PEPID, PEID, sgn1usid, sgn2usid, DEPATMAN
                            )
                            VALUES
                            (
                                @NUMBER,@NUMBER1, @TAG, @DATE_N, @TAH, @VAS, @CUST_NO, @MOLAH, @M_NAGHD, @MABL_VAR, 
                                @MOIN_VAR, @MABL_HAV, @MOIN_HAV, @MABL_HAZ, @MOIN_HAZ, @TAKHFIFA, @MOIN_KHF, 
                                @ANBARF, @FNUMCO, @SHIFT, @USER_NAME, @SGN1, @SGN2, @SGN4, @MBAA, @HMBAA, 
                                @TICMBAA, @TKHF, @OKF, @SADER, @ARZD, @ARZKIND, @CDDATE, @CDTIME, @OKDATE, 
                                @OKTIME, @JAY, @PEPID, @PEID, @sgn1usid, @sgn2usid, @DEPATMAN
                            );";
                            var parameters = new
                            {
                                NUMBER = num,
                                NUMBER1 = NUMBER1.SelectedValue?.ToString(),
                                TAG = 1, // ثابت
                                DATE_N = DATE_N.Text.ToRawTarikh(),
                                TAH = tah,
                                VAS = 0, // ثابت
                                CUST_NO = CUST_NO.SelectedValue,
                                MOLAH = molah,
                                M_NAGHD = 0, // ثابت
                                MABL_VAR = 0, // ثابت
                                MOIN_VAR = "", // ثابت
                                MABL_HAV = 0, // ثابت
                                MOIN_HAV = "", // ثابت
                                MABL_HAZ = 0, // ثابت
                                MOIN_HAZ = "", // ثابت
                                TAKHFIFA = 0, // ثابت - نام پارامتر تغییر کرد تا با ستون مچ شود (TAKHFIFA)
                                MOIN_KHF = "", // ثابت
                                ANBARF = 0, // ثابت
                                FNUMCO = fnumcoValue, // مقدار عددی و ایمن شده
                                SHIFT = CL_Generaly.SHIFT_OF_USER,
                                USER_NAME = USER_NAME.Text,
                                SGN1 = Convert.ToByte(SGN1.IsChecked),
                                SGN2 = Convert.ToByte(SGN2.IsChecked),
                                SGN4 = (byte?)null, // ثابت
                                MBAA = 0, // ثابت
                                HMBAA = "", // ثابت
                                TICMBAA = (decimal?)null, // ثابت
                                TKHF = (decimal?)null, // ثابت
                                OKF = Convert.ToByte(OKF.IsChecked),
                                SADER = saderValue, // مقدار ایمن شده
                                ARZD = 0, // ثابت
                                ARZKIND = 0, // ثابت
                                CDDATE = Baseknow.dt,
                                CDTIME = Tarikh.GET_OADATE_DAO(),
                                OKDATE = 0, // ثابت
                                OKTIME = 0, // ثابت
                                JAY = (string)null, // ثابت
                                PEPID = (string)null, // ثابت
                                PEID = (string)null, // ثابت
                                sgn1usid = sgn1usidValue, // مقدار ایمن شده
                                sgn2usid = sgn2usidValue, // مقدار ایمن شده
                                DEPATMAN = depatmanValue  // فیلد جدید و ایمن شده
                            };

                            db.Execute(QRE_HEADINSUP_PARAMETRIC, parameters, transaction);
                            transaction.Commit();
                            db?.Close();

                            _navigationManager.IsNewRecord = false;
                            RefreshAfterUpdate();
                        }
                    }
                }
                else //Update Edit
                {
                    var qre = @"
UPDATE dbo.HEAD_LST
SET 
    DATE_N     = @DATE_N,
    DEPATMAN   = @DEPATMAN,
    SHIFT      = @SHIFT,
    FNUMCO     = @FNUMCO,
    SADER      = @SADER,
    TAH        = @TAH,
    MOLAH      = @MOLAH,
    CUST_NO    = @CUST_NO,
    OKF        = @OKF,
    SGN1usid   = @SGN1usid,
    SGN2usid   = @SGN2usid,
    NUMBER1    = @NUMBER1
WHERE TAG = 1 
  AND NUMBER = @NUMBER";

                    dbms.DoExecuteSQL(qre, new
                    {
                        DATE_N = DATE_N.Text.ToRawTarikh(),
                        DEPATMAN = DEPATMAN.SelectedValue,
                        SHIFT = CL_Generaly.SHIFT_OF_USER,
                        FNUMCO = string.IsNullOrWhiteSpace(FNUMCO.Text) ? null : FNUMCO.Text,
                        SADER = SADER.SelectedValue,
                        TAH = tah,
                        MOLAH = molah,
                        CUST_NO = CUST_NO.SelectedValue,
                        OKF = Convert.ToInt32(OKF.IsChecked ?? true),
                        SGN1usid = SGN1usid.SelectedValue,
                        SGN2usid = SGN2usid.SelectedValue,
                        NUMBER1 = string.IsNullOrWhiteSpace(NUMBER1.Text) ? null : NUMBER1.Text,
                        NUMBER = NUMBER.Text
                    });

                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره سربرگ").Show();
                return false;
            }

            return true;
        }

        private void GetFocusOnDefaultCell()
        {
            var DG = INVO_LST_RASID_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }
        private void INVO_LST_RASID_SUB_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void INVO_LST_RASID_SUB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MOGUDI_UPDATE();
        }
        private void INVO_LST_RASID_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            // Check if Ctrl key is pressed and the pressed key is double quote
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (INVO_LST_RASID_SUB?.CurrentCell != null && INVO_LST_RASID_SUB.IsEnabled && !INVO_LST_RASID_SUB.IsReadOnly)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = INVO_LST_RASID_SUB.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = INVO_LST_RASID_SUB.Items.IndexOf(currentCell.Item);
                            int columnIndex = INVO_LST_RASID_SUB.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = INVO_LST_RASID_SUB.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < INVO_LST_RASID_SUB.Columns.Count)
                                {
                                    // Get the column information
                                    var column = INVO_LST_RASID_SUB.Columns[columnIndex];

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

                                                INVO_LST_RASID_SUB.Items.Refresh();

                                                INVO_LST_RASID_SUB.BeginEdit();
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

            if (NowIsReady && e.Key == Key.Delete && DELETE_RASID.IsEnabled)
            {
                e.Handled = true;

                DELETE_RASID_Click(null, null);
            }
        }
        private void SGN1usid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void SGN2usid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void DELETE_RASID_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_RASID.Visibility == Visibility.Visible;
            if (!DELETE_RASID.IsEnabled || !IsVisible) { return; }

            if (INVO_LST_RASID_SUB.IsReadOnly || !INVO_LST_RASID_SUB.IsEnabled)
            {
                return;
            }

            var editableCollectionView = INVO_LST_RASID_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
            {
                try { editableCollectionView.CancelEdit(); } catch { }
            }

            if (INVO_LST_RASID_SUB.Items.Count > 0 && INVO_LST_RASID_SUB.SelectedItem != null)
            {
                if (INVO_LST_RASID_SUB.SelectedItems is null) return;

                List<MsgModel> ErrosMessages = new List<MsgModel>();
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();

                if (msgwin.DialogResult != true) return;

                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + ") AND (TAG = 1)", dt, 1);

                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "رسید انبار",
                        recordId: NUMBER.Text,
                        oldValue: "TAG = 1",
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                foreach (var selecteditem in INVO_LST_RASID_SUB.SelectedItems.OfType<INVO_LST_FACTOR22>().ToList())
                {
                    if (CL_LMethods.IsNewPlaceHolder(INVO_LST_RASID_SUB, selecteditem)) continue;

                    var _id_ = selecteditem.GetType().GetProperty("id").GetValue(selecteditem);
                    if (_id_ == null)
                    {
                        INVO_DATA_RASID_KHARID.Remove(selecteditem as INVO_LST_FACTOR22);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            //بررسی موجودی در صورت امکان حذف
                            var items = new List<object> { selecteditem };
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

                ReGetdata();
                Summer();
            }
            else
            {
                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER = {NUMBER.Text} AND TAG = {HTAG}");

                        _navigationManager?.DeleteCurrentRecord(); //Refresh Record Source
                    }
                    catch (SqlException ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }

                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این رسید دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
        private void NUMBER1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void NUMBER1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(FNUMCO, e);
        }
        private void FNUMCO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void FNUMCO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FNUMCO.Text)) { FNUMCO.Text = "0"; }
        }
        private void FNUMCO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(FNUMCO, e);
        }
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = BEFOREDATEN;
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = BEFOREDATEN;
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            else
            {
                DATE_N.Text = BEFOREDATEN;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }
        private void BTN_INVOCES_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 1);
        }
        private void ClearFreshAll()
        {
            NUMBER1.SelectedValue = null;
            NUMBER1.Text = "0";
            NUMBER.Text = "0";

            NUMBER1.Tag = null;
            NUMBER1_TAG = null;

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh();

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();

            SADER.SelectedValue = 0; SADER.Items.Refresh();
            TAH.SelectedValue = null; TAH.Items.Refresh(); TAH.Text = null;
            MOLAH.SelectedValue = null; MOLAH.Items.Refresh(); MOLAH.Text = null;

            FNUMCO.Text = "0"; //شماره داخلی
            Text59.Text = "0";

            MakeOKFReady();

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false; SGN1usid.SelectedValue = null; SGN1usid.Items.Refresh();
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false; SGN2usid.SelectedValue = null; SGN2usid.Items.Refresh();

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOGU.Text = null; //موجودی

            INVO_DATA_RASID_KHARID?.Clear();

            Form_Current();

            AllowEdits = true;

            INVO_LST_RASID_SUB.IsReadOnly = true;

            GetDefaultFocus();
        }
        private void BTN_NEW_FACTOR_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeIsHappend)
            {
                ClearFreshAll();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "ذخیره را انجام نداده ای آیا از ادامه مطمئن هستید؟");
                if (msgwin.DialogResult != true)
                {
                    return;
                }
            }
        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_RASID_SUB.Items.Count > 0)
            {
                if (INVO_LST_RASID_SUB.SelectedItem is not null)
                {
                    var Row = INVO_LST_RASID_SUB.SelectedItem as INVO_LST_FACTOR22;
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
