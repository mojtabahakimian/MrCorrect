using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.PMML;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for PAYCHEK.xaml
    /// </summary>
    public partial class PAYCHEK : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private bool can;
        private bool mabup;
        public string BEFOREDATEN { get; private set; }
        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }
        public string ServerFilter;
        public bool CANCEL { get; private set; }
        public int INDEX_DG { get; set; }
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
        public Visual THE_WIN { get; set; }
        public string N_KOL { get; private set; }
        public string N_MOIN { get; private set; }
        public string MABL_CHEK_ARG { get; set; }
        bool DaftarShouldUpdate;
        public string N_TAF { get; private set; }
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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

        public PAYCHEK(string _serverfilter, Visual _thewin, string _mabl_check_arg = null, int _current_index = -1, bool isreadonly = false, double? _originMabl_ = null)
        {
            IsReadOnlyMode = isreadonly;
            THE_WIN = _thewin;
            MABL_CHEK_ARG = _mabl_check_arg;
            ServerFilter = _serverfilter;
            INDEX_DG = _current_index;
            _original_MABL = _originMabl_;
            InitializeComponent();
        }

        public class QueryT2
        {
            public string? hes { get; set; }
            public string? Expr1 { get; set; }

        }

        public class N_HESAB_MODEL
        {
            public string N_HESAB { get; set; }
        }
        public class NAME_TAH_MODEL
        {
            public string NAME_TAH { get; set; }
        }
        public bool IsReadOnlyMode { get; set; } = false;

        // کلیدهای اصلی رکورد در زمان Load - برای استفاده در Save
        private long? CurrentRecordID = null;
        private double? _original_N_SERI = null;
        private int? _original_BANK = null;
        private long? _original_DATE_S = null;
        private double? _original_MABL = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            #region On_Open
            Fill_ComboBoxes();
            MABL.Text = MABL_CHEK_ARG;
            var pgetHed = THE_WIN as PGET_HED;
            PGET_LST KhazanehRow = null;
            if (pgetHed?.PGET_LST_SUB != null && INDEX_DG >= 0 && INDEX_DG < pgetHed.PGET_LST_SUB.Items.Count)
            {
                KhazanehRow = pgetHed.PGET_LST_SUB.Items[INDEX_DG] as PGET_LST;
            }

            if (pgetHed?.DATE != null)
            {
                DATE.Text = pgetHed.DATE.Text.ToRawTarikh();
            }

            if (KhazanehRow?.N_SERI is not null && KhazanehRow.BANK is not null)
            {
                var CheckExistData = dbms.DoGetDataSQL<PAY_GETP>($"SELECT * FROM PAY_GETP WHERE N_SERI = {KhazanehRow.N_SERI} AND BANK = {KhazanehRow.BANK}").ToList();
                if (CheckExistData.Count > 0)
                {
                    DaftarShouldUpdate = true;
                    N_SERI.Text = CheckExistData.FirstOrDefault()?.N_SERI.ToString();
                    BANK.SelectedValue = CheckExistData.FirstOrDefault()?.BANK.ToString();
                    SHOBEH.Text = CheckExistData.FirstOrDefault()?.SHOBEH?.ToString();
                    DATE_S.Text = CheckExistData.FirstOrDefault()?.DATE_S.ToString();
                    DATE.Text = CheckExistData.FirstOrDefault()?.DATE.ToString();
                    MABL.Text = CheckExistData.FirstOrDefault()?.MABL.ToString();
                    NAME_TAH.SelectedValue = CheckExistData.FirstOrDefault()?.NAME_TAH?.ToString();
                    N_HESAB.Text = CheckExistData.FirstOrDefault()?.N_HESAB?.ToString();
                    HES1.SelectedValue = CheckExistData.FirstOrDefault()?.HES1?.ToString();
                    SAYADI.Text = CheckExistData.FirstOrDefault()?.SAYADI;

                    // ✅ ذخیره کلید اولیه برای استفاده در Save
                    CurrentRecordID = CheckExistData.FirstOrDefault()?.ID;
                    _original_N_SERI = CheckExistData.FirstOrDefault()?.N_SERI;
                    _original_BANK = CheckExistData.FirstOrDefault()?.BANK;
                    _original_DATE_S = CheckExistData.FirstOrDefault()?.DATE_S;
                    _original_MABL = KhazanehRow.MABL;
                }
                else
                {
                    DaftarShouldUpdate = false;
                    N_SERI.Text = KhazanehRow.N_SERI?.ToString() ?? "";
                    BANK.SelectedValue = KhazanehRow.BANK?.ToString();

                    _original_N_SERI = KhazanehRow.N_SERI;
                    _original_BANK = KhazanehRow.BANK;
                    _original_DATE_S = null;
                }
            }
            #endregion

            CheckTabIndexes();

            #region TAB_STOP
            //N_SERI.IsTabStop = true;
            //N_SERI.Focusable = true;
            //N_SERI.IsEnabled = true;
            //N_SERI.TabIndex = 1;

            //BANK.IsTabStop = true;
            //BANK.Focusable = true;
            //BANK.IsEnabled = true;
            //BANK.TabIndex = 2;

            //SHOBEH.IsTabStop = true;
            //SHOBEH.Focusable = true;
            //SHOBEH.IsEnabled = true;
            //SHOBEH.TabIndex = 3;

            //DATE_S.IsTabStop = true;
            //DATE_S.Focusable = true;
            //DATE_S.IsEnabled = true;
            //DATE_S.TabIndex = 4;

            //NAME_TAH.IsTabStop = true;
            //NAME_TAH.Focusable = true;
            //NAME_TAH.IsEnabled = true;
            //NAME_TAH.TabIndex = 5;

            //N_HESAB.IsTabStop = true;
            //N_HESAB.Focusable = true;
            //N_HESAB.IsEnabled = true;
            //N_HESAB.TabIndex = 6;

            //HES1.IsTabStop = true;
            //HES1.Focusable = true;
            //HES1.IsEnabled = true;
            //HES1.TabIndex = 7;

            //SAYADI.IsTabStop = true;
            //SAYADI.Focusable = true;
            //SAYADI.IsEnabled = true;
            //SAYADI.TabIndex = 8;
            #endregion

            if (IsReadOnlyMode)
            {
                N_SERI.IsEnabled = false;
                BANK.IsEnabled = false;
                SHOBEH.IsEnabled = false;
                DATE_S.IsEnabled = false;
                DATE.IsEnabled = false;
                MABL.IsEnabled = false;
                NAME_TAH.IsEnabled = false;
                N_HESAB.IsEnabled = false;
                HES1.IsEnabled = false;
                SAYADI.IsEnabled = false;
                KIND.IsEnabled = false;

                _SaveExit.IsEnabled = false;
                _SaveExit.Visibility = Visibility.Collapsed;

                this.Title += " (فقط خواندنی)";
            }
        }

        private void Fill_ComboBoxes()
        {
            var pgetHed = THE_WIN as PGET_HED;
            string NAME_TAH_DISPLAY = null;
            if (pgetHed?.PGET_LST_SUB != null && INDEX_DG >= 0 && INDEX_DG < pgetHed.PGET_LST_SUB.Items.Count)
            {
                NAME_TAH_DISPLAY = (pgetHed.PGET_LST_SUB.Items[INDEX_DG] as PGET_LST)?.NAME_THES;
            }


            HES1.ItemsSource = dbms.DoGetDataSQL<QueryT2>("SELECT hes, hes + N' - ' +  ISNULL(NAME, N'') AS Expr1 FROM CUST_HESAB WHERE (dbo.GETKOL(HES) = " + Baseknow.BANKHA + ")").ToList();
            HES1.SelectedValuePath = "hes";
            HES1.DisplayMemberPath = "Expr1";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES;").ToList();
            BANK.DisplayMemberPath = "NAMES";
            BANK.SelectedValuePath = "CODE";

            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETP>("SELECT PAY_GETP.SHOBEH FROM PAY_GETP GROUP BY PAY_GETP.SHOBEH ORDER BY PAY_GETP.SHOBEH;").ToList();
            SHOBEH.SelectedValuePath = "SHOBEH";
            SHOBEH.DisplayMemberPath = "SHOBEH";

            #region NAME_TAH

            var NAME_TAH_TS1 = dbms.DoGetDataSQL<PAY_GETP>("SELECT PAY_GETP.NAME_TAH FROM PAY_GETP GROUP BY PAY_GETP.NAME_TAH ORDER BY PAY_GETP.NAME_TAH;").ToList();
            if (!(NAME_TAH_TS1).Any(item => item?.NAME_TAH == NAME_TAH_DISPLAY))
            {
                (NAME_TAH_TS1).Add(new PAY_GETP { NAME_TAH = NAME_TAH_DISPLAY });
            }
            NAME_TAH.ItemsSource = NAME_TAH_TS1;
            NAME_TAH.SelectedValuePath = "NAME_TAH";
            NAME_TAH.DisplayMemberPath = "NAME_TAH";
            if (!string.IsNullOrEmpty(NAME_TAH_DISPLAY))
            {
                NAME_TAH.SelectedValue = NAME_TAH_DISPLAY;
            }
            #endregion

            N_HESAB.ItemSource = dbms.DoGetDataSQL<N_HESAB_MODEL>("SELECT DISTINCT N_HESAB FROM dbo.PAY_GETD").ToList();
        }

        private void BANK_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (BANK.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            #region NUM_TO_BANK
            if (!NowIsReady)
            {
                return;
            }

            ComboBox comboBox = BANK;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                string inputCode = textBox.Text;
                if (comboBox.SelectedValue == null && !int.TryParse(inputCode, out int _))
                {
                    return;
                }

                // Cast the ItemsSource back to the correct type, and search for matching bank code
                if (comboBox.SelectedValue is null)
                {
                    var bankList = comboBox.ItemsSource as List<TCOD_BANKS>;

                    if (bankList == null) return; // Ensure that the list is properly cast

                    var matchingBank = bankList.FirstOrDefault(b => b.CODE == Convert.ToInt32(inputCode));

                    if (matchingBank != null)
                    {
                        BANK.SelectedValue = matchingBank.CODE;
                        comboBox.SelectedValue = matchingBank.CODE;
                        //return;
                    }
                    else
                    {
                        comboBox.SelectedValue = null;
                    }
                }
            }
            #endregion

            #region NotInList
            //var NewData = ((TextBox)BANK.Template.FindName("PART_EditableTextBox", BANK)).Text; //متن کمبوباکس رو به طور واقعی میگیریم
            //if (int.TryParse(NewData.ToString(), out _)) //if is Number آیا عدد هست متن وارد شده
            //{
            //    var _itm = BANK.ItemsSource as List<TCOD_BANKS>; // برای راحتی و سادگی کد آیتم های کموبباکس رو بریز داخل یه لیست
            //    // Use LINQ to find the item that matches the search value
            //    var _SelectVal = _itm?.FirstOrDefault(item => item.CODE is not null && item.CODE.Equals(NewData)).CODE; // با لینک چک کن که آیا کدی با این کد وارد شده وجود دارد ؟

            //    BANK.SelectedValue = _SelectVal; //بذارش توی کمبوباکس
            //}
            #endregion

            #region After_Update
            if (!IsNull(this.N_SERI.Text) & !IsNull(this.BANK.SelectedValue))
            {
                string excludeQuery = CurrentRecordID != null && CurrentRecordID > 0
                    ? $" AND ID <> {CurrentRecordID}"
                    : (_original_N_SERI != null && _original_BANK != null ? $" AND NOT (N_SERI = '{_original_N_SERI}' AND BANK = {_original_BANK})" : "");

                var Filter = "N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + excludeQuery;
                var rst = dbms.DoGetDataSQL<PAY_GETP>($"SELECT * FROM PAY_GETP WHERE {Filter} ").ToList();
                if (rst.Count == 0)
                {
                }
                else
                {
                    Msgwin msgwin = new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود");
                    msgwin.ShowDialog();
                    //DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود");
                    this.N_SERI.Text = rst.FirstOrDefault().N_SERI.ToString();
                    this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                    this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                    this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                    this.SHOBEH.SelectedValue = rst.FirstOrDefault().SHOBEH;
                    this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                    this.NAME_TAH.SelectedValue = rst.FirstOrDefault().NAME_TAH;
                    this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                    this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                    //this.MABL.IsTabStop = true;
                    this.MABL.IsReadOnly = false;
                    this.N_KOL = rst.FirstOrDefault().N_KOL.ToString();
                    this.N_MOIN = rst.FirstOrDefault().N_MOIN.ToString();
                    this.N_TAF = rst.FirstOrDefault().N_TAF.ToString();
                    this.KIND.SelectedValue = rst.FirstOrDefault().KIND;
                    this.HES1.SelectedValue = rst.FirstOrDefault().HES1;
                    this.SAYADI.Text = rst.FirstOrDefault().SAYADI;

                    if (THE_WIN is PGET_HED khazanehWin && khazanehWin.CURRENT_ITMES_ROW != null)
                    {
                        khazanehWin.CURRENT_ITMES_ROW.N_SERI = rst.FirstOrDefault().N_SERI;
                        khazanehWin.CURRENT_ITMES_ROW.BANK = rst.FirstOrDefault().BANK;
                        DaftarShouldUpdate = true;
                    }
                }
            }
            //rst.Close();
            #endregion

        }

        public bool DATE_IS_VALID(string DATE, bool DisplayMsg = false, bool ForceSynce = true)
        {
            bool Date_Is_Valid = true;

            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    if (DisplayMsg)
                    {
                    }
                    Date_Is_Valid = false;
                }
                else
                {
                    if (ForceSynce && !Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        if (DisplayMsg)
                        {
                        }
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                if (DisplayMsg)
                {
                }
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }
        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            TextBox NAME_TAH_TEX = (TextBox)NAME_TAH.Template.FindName("PART_EditableTextBox", NAME_TAH);
            if (string.IsNullOrEmpty(NAME_TAH_TEX.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام پرداخت کننده نمی تواند خالی باشد" });
            }
            if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue) || IsNull(this.DATE_S.Text.ToRawTarikh()) || this.DATE_S.Text.ToRawTarikh() == "")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سريال ، نام بانك و تاريخ سررسيد  نمي تواند خالي باشد!" });
            }
            if (NAME_TAH.Text.Length > 190)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام پرداخت کنند باید مختصر و کوتاه باشد" });
            }

            if (!DATE_IS_VALID(DATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ پرداخت صحیح نیست" });
            }
            if (!DATE_IS_VALID(DATE_S.Text.ToRawTarikh(), default, false))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح نیست" });
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return;
            }

            var pgetWin = THE_WIN as PGET_HED;
            if (pgetWin?.CURRENT_ITMES_ROW != null)
            {
                pgetWin.CmdSaveRecord(pgetWin.CURRENT_ITMES_ROW);
            }

            PGET_LST activeLstRow = null;
            if (pgetWin?.PGET_LST_SUB != null && INDEX_DG >= 0 && INDEX_DG < pgetWin.PGET_LST_SUB.Items.Count)
            {
                activeLstRow = pgetWin.PGET_LST_SUB.Items[INDEX_DG] as PGET_LST;
            }

            //Click
            can = false;
            try
            {
                {
                    DateTime dt;
                    dt = DateTime.Now;
                    CL_HESABDARI.TR("PAY_GETP", "N_SERI = " + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh(), dt, 1);
                    can = false;
                    if (mabup)
                    {
                        var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh()).ToList();
                        string _where = " WHERE N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();
                        if (rst.Count > 0)
                        {
                            rst.FirstOrDefault().MABL = Convert.ToDouble(this.MABL.Text);
                            //rst.update();
                            dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP SET MABL = {rst.FirstOrDefault().MABL} {_where} ");
                        }
                        //rst.Close();
                    }
                    if (activeLstRow?.FHES == Baseknow.APA)
                    {
                        if (Convert.ToInt32(this.KIND.SelectedValue) != 1 || IsNull(this.KIND))
                        {
                            this.KIND.SelectedValue = 1;
                        }
                    }
                    else if (Convert.ToInt32(this.KIND.SelectedValue) != 0 || IsNull(this.KIND))
                    {
                        this.KIND.SelectedValue = 0;
                    }
                    if (activeLstRow?.MABL != null)
                    {
                        this.MABL.Text = activeLstRow.MABL.ToString();
                    }
                    if (this.NAME_TAH.SelectedValue == "")
                    {
                        this.NAME_TAH.SelectedValue = " ";
                    }
                }
            }
            catch
            {
            }

            //Before_Update
            if (can)
            {
                CANCEL = true;
            }
            else if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue))
            {
                CANCEL = true;
            }
            else
            {

                if (activeLstRow?.FHES == Baseknow.APA)
                {
                    if (Convert.ToInt32(this.KIND.SelectedValue) != 1 || IsNull(this.KIND))
                    {
                        this.KIND.SelectedValue = 1;
                    }
                }
                else if (Convert.ToInt32(this.KIND.SelectedValue) != 0 || IsNull(this.KIND))
                {
                    this.KIND.SelectedValue = 0;
                }
                if (activeLstRow != null)
                {
                    activeLstRow.N_SERI = Convert.ToDouble(this.N_SERI.Text);
                    activeLstRow.BANK = Convert.ToInt32(this.BANK.SelectedValue);
                    activeLstRow.SHARH = Strings.Left("چك " + N_SERI.Text + "بانك " + CL_HESABDARI.GETBANK(Convert.ToDouble(this.BANK.SelectedValue)) + " " + SHOBEH.SelectedValue + " مورخ " + Strings.Format(Convert.ToInt32(DATE_S.Text.ToRawTarikh()), "####/##/##") + "-" + NAME_TAH.Text, 255);
                }

                if (!string.IsNullOrWhiteSpace(HES1.SelectedValue?.ToString()))
                {
                    if (HES1.SelectedValue.ToString().Trim() != "911-1-1")
                    {
                        this.N_KOL = Convert.ToString(CL_HESABDARI.GETKOL(this.HES1.SelectedValue.ToString()));
                        this.N_MOIN = Convert.ToString(CL_HESABDARI.GETMOIN(this.HES1.SelectedValue.ToString()));
                        this.N_TAF = Convert.ToString(CL_HESABDARI.GETTAF(this.HES1.SelectedValue.ToString()));
                    }
                }
                else
                {
                    this.N_KOL = null;
                    this.N_MOIN = null;
                    this.N_TAF = null;
                }

                var KhazanehRow = activeLstRow;
                var CheckExistData = dbms.DoGetDataSQL<PAY_GETP>($"SELECT TOP 1 * FROM PAY_GETP WHERE N_SERI = {N_SERI.Text} AND BANK = {BANK.SelectedValue} AND DATE_S = {DATE_S.Text.ToRawTarikh()} ORDER BY RADIF").ToList();

                var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;
                var _N_HESAB_ = N_HESAB.Text.Length > 198 ? N_HESAB.Text.Substring(0, 198) : N_HESAB.Text;

                var _SHOBEH_ = SHOBEH.SelectedValue.ToStringNullSafe().Length > 50 ? SHOBEH.SelectedValue.ToStringNullSafe().Substring(0, 50) : SHOBEH.SelectedValue.ToStringNullSafe();


                var _KIND_VAL_ = KIND.SelectedValue != null ? KIND.SelectedValue.ToString() : "0";
                var _HES1_VAL_ = HES1.SelectedValue != null ? HES1.SelectedValue.ToString() : "";
                var _N_KOL_VAL_ = string.IsNullOrWhiteSpace(N_KOL) ? "NULL" : N_KOL;
                var _N_MOIN_VAL_ = string.IsNullOrWhiteSpace(N_MOIN) ? "NULL" : N_MOIN;
                var _N_TAF_VAL_ = string.IsNullOrWhiteSpace(N_TAF) ? "NULL" : N_TAF;

                try
                {
                    if (DaftarShouldUpdate)
                    {
                        if (_original_N_SERI != null)
                        {
                            // حالت ویرایش: UPDATE با کلید اصلی (نه کلید جدید کاربر)
                            dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP SET
                                     N_SERI   = {N_SERI.Text},
                                     BANK     = {BANK.SelectedValue},
                                     DATE_S   = {DATE_S.Text.ToRawTarikh()},
                                     DATE     = {DATE.Text.ToRawTarikh()},
                                     SHOBEH   = N'{_SHOBEH_}',
                                     MABL     = {MABL.Text},
                                     NAME_TAH = N'{_NAME_TAH_}',
                                     N_HESAB  = N'{_N_HESAB_}',
                                     KIND     = {_KIND_VAL_},
                                     HES1     = N'{_HES1_VAL_}',
                                     SAYADI   = N'{(string.IsNullOrEmpty(SAYADI.Text) ? "0" : SAYADI.Text)}',
                                     N_KOL    = {_N_KOL_VAL_},
                                     N_MOIN   = {_N_MOIN_VAL_},
                                     N_TAF    = {_N_TAF_VAL_},
                                     N_S = NULL, N_KOL2 = NULL, N_MOIN2 = NULL, N_TAF2 = NULL,
                                     N_KOL3 = NULL, N_MOIN3 = NULL, N_TAF3 = NULL,
                                     NUMBER = NULL, TAG = NULL, ANBAR = NULL,
                                     RADIF = NULL, CUST_NO = DEFAULT, VAZ = NULL,
                                     HES2 = NULL, HES3 = NULL
                                 WHERE N_SERI = {_original_N_SERI}
                                   AND BANK   = {_original_BANK}
                                   AND DATE_S = {_original_DATE_S}");
                        }
                        else
                        {
                            var _id_ = CheckExistData.FirstOrDefault().ID;
                            dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP SET
                                     N_SERI   = {N_SERI.Text},
                                     BANK     = {BANK.SelectedValue},
                                     DATE_S   = {DATE_S.Text.ToRawTarikh()},
                                     DATE     = {DATE.Text.ToRawTarikh()},
                                     SHOBEH   = N'{_SHOBEH_}',
                                     MABL     = {MABL.Text},
                                     NAME_TAH = N'{_NAME_TAH_}',
                                     N_HESAB  = N'{_N_HESAB_}',
                                     KIND     = {_KIND_VAL_},
                                     HES1     = N'{_HES1_VAL_}',
                                     SAYADI   = N'{(string.IsNullOrEmpty(SAYADI.Text) ? "0" : SAYADI.Text)}',
                                     N_KOL    = {_N_KOL_VAL_},
                                     N_MOIN   = {_N_MOIN_VAL_},
                                     N_TAF    = {_N_TAF_VAL_},
                                     N_S = NULL, N_KOL2 = NULL, N_MOIN2 = NULL, N_TAF2 = NULL,
                                     N_KOL3 = NULL, N_MOIN3 = NULL, N_TAF3 = NULL,
                                     NUMBER = NULL, TAG = NULL, ANBAR = NULL,
                                     RADIF = NULL, CUST_NO = DEFAULT, VAZ = NULL,
                                     HES2 = NULL, HES3 = NULL
                                 WHERE ID = {_id_}");
                        }
          
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETP       (N_SERI,                BANK,                     DATE_S,                     DATE,                   SHOBEH,       MABL,          NAME_TAH,          N_HESAB, N_S,  N_KOL,  N_MOIN,  N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO,                KIND, VAZ,                   HES1, HES2, HES3, SAYADI)
				                                           VALUES({N_SERI.Text},{BANK.SelectedValue},{DATE_S.Text.ToRawTarikh()},{DATE.Text.ToRawTarikh()},N'{_SHOBEH_}',{MABL.Text},N'{_NAME_TAH_}',N'{_N_HESAB_}',NULL,{_N_KOL_VAL_},{_N_MOIN_VAL_},{_N_TAF_VAL_},   NULL,    NULL,   NULL,   NULL,    NULL,   NULL,   NULL,NULL,  NULL,  NULL,       0,{_KIND_VAL_},NULL,N'{_HES1_VAL_}', NULL, NULL, N'{(string.IsNullOrEmpty(SAYADI.Text) ? "0" : SAYADI.Text)}')");
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات تکراری است").ShowDialog(); return;
                }

                if (THE_WIN is PGET_HED pgetHedWindow)
                {
                    if (pgetHedWindow.PGET_LST_SUB != null && INDEX_DG >= 0 && INDEX_DG < pgetHedWindow.PGET_LST_SUB.Items.Count)
                    {
                        if (pgetHedWindow.PGET_LST_SUB.Items[INDEX_DG] is PGET_LST itemToSave)
                        {
                            pgetHedWindow.CmdSaveRecord(itemToSave);
                        }
                    }
                    pgetHedWindow.SANAD();
                }

                this.Close();
            }
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            can = true;
            if (THE_WIN is PGET_HED pgetHedWin)
            {
                pgetHedWin.PAYCHEK_EXIT_BTN = true;
            }
            this.Close();
        }

        private void HES1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (HES1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            #region Before_Update
            if (!IsNull(this.HES1.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.HES1.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    CANCEL = true;
                }
            }
            #endregion

            //After_Update
            if (!IsNull(this.HES1.SelectedValue))
            {
                this.N_KOL = Convert.ToString(CL_HESABDARI.GETKOL(this.HES1.SelectedValue.ToString()));
                this.N_MOIN = Convert.ToString(CL_HESABDARI.GETMOIN(this.HES1.SelectedValue.ToString()));
                this.N_TAF = Convert.ToString(CL_HESABDARI.GETTAF(this.HES1.SelectedValue.ToString()));
            }
            else
            {
                this.N_KOL = null;
                this.N_MOIN = null;
                this.N_TAF = null;
            }
        }

        private void DATE_S_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            string date_n_val = DATE_S.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_S.Text = BEFOREDATEN;
                    if (THE_WIN is PGET_HED pgetHedWin)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", pgetHedWin.Pop1, pgetHedWin.Pop1Text1, pgetHedWin.Pop_Border1);
                    }
                    return;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            N_SERI.SetFocusToTextBox();
        }

        private void SHOBEH_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (SHOBEH == null) { return; }

            var shobetext = SHOBEH.Template?.FindName("PART_EditableTextBox", SHOBEH) as TextBox;

            if (shobetext == null) { return; }

            if (string.IsNullOrEmpty(shobetext.Text))
            {
                var sourceList = SHOBEH.ItemsSource as List<PAY_GETP>;
                if (sourceList != null)
                {
                    bool itemExists = sourceList.Any(item => item?.SHOBEH == shobetext.Text);
                    if (!itemExists)
                    {
                        sourceList.Add(new PAY_GETP { SHOBEH = shobetext.Text });
                        SHOBEH.SelectedValue = shobetext.Text;
                    }
                }
            }
        }

        // Diagnostic method to debug tab order and focus ability
        private void CheckTabIndexes()
        {
            // Create a list to keep track of tab information for each control
            var tabElements = new List<string>();

            foreach (UIElement element in mygrid.Children)
            {
                if (element is Control control)
                {
                    // Check if the control is visible, enabled, and a tab stop
                    if (control.Visibility == Visibility.Visible &&
                        control.IsEnabled &&
                        control.IsTabStop)
                    {
                        // Add tab information for the control to the list
                        tabElements.Add($"Tab Index: {control.TabIndex}, Focusable: {control.Focusable}, Type: {control.GetType().Name}");
                    }
                }
            }

            // Order the list by tab index
            tabElements = tabElements.OrderBy(te => int.Parse(te.Split(',')[0].Split(':')[1].Trim())).ToList();

            // Output the list to the debug console or show it in a message box
            foreach (string tabElement in tabElements)
            {
                Debug.WriteLine(tabElement);
            }

            // Optionally show the list in a message box for debugging purposes (comment out in production)
            // MessageBox.Show(string.Join(Environment.NewLine, tabElements), "Tab Order Information");
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_SaveExit.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (e.Key is Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                try
                {
                    e.Handled = true;
                    this?.Close();
                }
                catch { }
            }
        }

        private void SAYADI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (!string.IsNullOrWhiteSpace(SAYADI.Text))
            {
                if (SAYADI.Text.Length < 16 && SAYADI.Text != "0")
                {
                    Msgwin msgwin = new Msgwin(false, "شماره صیادی نباید کمتر از 16 رقم باشد.");
                    msgwin.ShowDialog();
                }
            }
        }

        private void _SaveExit_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }


        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }
    }
}
