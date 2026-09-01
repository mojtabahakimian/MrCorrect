using MaterialDesignThemes.Wpf;
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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;
using TextBox = System.Windows.Controls.TextBox;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for GETCHEK.xaml
    /// </summary>
    public partial class GETCHEK : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        private bool mabup;

        public string CUST_NO { get; set; }
        public string N_KOL { get; set; }
        public string N_MOIN { get; set; }
        public string BEFOREDATEN { get; private set; }
        public string N_TAF { get; set; }
        public string ANBAR { get; set; } = "1";
        public bool can { get; private set; }
        public bool CANCEL { get; private set; }
        public bool IsReadOnlyMode { get; set; } = false;
        public class QueryT1
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }

        }

        public class QueryT2
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }

        }

        public Visual THE_WIN { get; set; }
        public string MABL_CHEK_ARG { get; set; }
        public string DATE_CHEK_ARG { get; set; }
        public int INDEX_DG { get; set; }
        public bool NowIsReady { get; private set; }

        private long? CurrentRecordID = null;


        // کلیدهای اصلی رکورد در زمان Load - برای استفاده در Save
        private double? _original_N_SERI = null;
        private int? _original_BANK = null;
        private long? _original_DATE_S = null;
        private double? _original_MABL = null;
        public GETCHEK(Visual the_win, string _mabl_chek_arg = null, int _current_index = -1, bool isreadonly = false, double? _originMabl_ = null)
        {
            IsReadOnlyMode = isreadonly;
            THE_WIN = the_win;
            MABL_CHEK_ARG = _mabl_chek_arg;
            INDEX_DG = _current_index;
            _original_MABL = _originMabl_;
            InitializeComponent();
        }

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

        private bool IsNull(object item)
        {
            if (item == null) return true;
            if (item is string s) return string.IsNullOrEmpty(s);
            return false;
        }


        public class N_HESAB_MODEL
        {
            public string N_HESAB { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            Fill_ComboBoxes();

            // On Open here ...
            if (!string.IsNullOrWhiteSpace(this.N_KOL))
            {
                this.HES.SelectedValue = this.N_KOL + "-" + this.N_MOIN + "-" + this.N_TAF;
            }

            MABL.Text = MABL_CHEK_ARG;
            mabup = false;
            SANDUGH.SelectedValue = 1;
            var KhazanehRow = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST);
            DATE.Text = (THE_WIN as PGET_HED).DATE.Text.ToRawTarikh();

            if (KhazanehRow?.N_SERI is not null && KhazanehRow.BANK is not null)
            {
                var CheckExistData = dbms.DoGetDataSQL<PAY_GETD>($"SELECT TOP 1 * FROM PAY_GETD WHERE N_SERI = {KhazanehRow.N_SERI} AND BANK = {KhazanehRow.BANK} ORDER BY RADIF").ToList();
                if (CheckExistData.Count > 0)
                {
                    CurrentRecordID = CheckExistData.FirstOrDefault()?.ID; // Capture ID

                    RADIF.Text = CheckExistData.FirstOrDefault()?.RADIF.ToString();
                    N_SERI.Text = CheckExistData.FirstOrDefault()?.N_SERI.ToString();
                    BANK.SelectedValue = CheckExistData.FirstOrDefault()?.BANK.ToString();
                    SHOBEH.SelectedValue = CheckExistData.FirstOrDefault()?.SHOBEH?.ToString();
                    LIST_NO.SelectedValue = CheckExistData.FirstOrDefault()?.LIST_NO?.ToString();
                    DATE_S.Text = CheckExistData.FirstOrDefault()?.DATE_S.ToString();
                    DATE.Text = CheckExistData.FirstOrDefault()?.DATE.ToString();
                    MABL.Text = CheckExistData.FirstOrDefault()?.MABL.ToString();

                    if (!string.IsNullOrWhiteSpace(N_KOL))
                    {
                        HES.SelectedValue = CheckExistData.FirstOrDefault()?.HES1?.ToString();
                    }

                    N_KOL = CheckExistData.FirstOrDefault()?.N_KOL.ToString();
                    N_MOIN = CheckExistData.FirstOrDefault()?.N_MOIN.ToString();
                    N_TAF = CheckExistData.FirstOrDefault()?.N_TAF.ToString();

                    if (!string.IsNullOrWhiteSpace(this.N_KOL))
                    {
                        this.HES.SelectedValue = this.N_KOL + "-" + this.N_MOIN + "-" + this.N_TAF;
                    }

                    NAME_TAH.SelectedValue = CheckExistData.FirstOrDefault()?.NAME_TAH?.ToString();
                    N_HESAB.Text = CheckExistData.FirstOrDefault()?.N_HESAB?.ToString();
                    SANDUGH.SelectedValue = CheckExistData.FirstOrDefault()?.SANDUGH?.ToString();
                    SAYADI.Text = CheckExistData.FirstOrDefault()?.SAYADI?.ToString();

                    var loadedCheck = CheckExistData.FirstOrDefault();
                    bool hasCheckMovement = loadedCheck?.VAZ == 4 ||
                                            loadedCheck?.N_KOL2 != null ||
                                            loadedCheck?.N_KOL3 != null;

                    if (hasCheckMovement)
                    {
                        HES.IsEnabled = false;
                        HES.ToolTip = "حساب واگذاری این چک دارای گردش است و فقط از طریق عملیات خزانه‌داری قابل تغییر می‌باشد.";
                    }

                    // ✅ ذخیره کلید اولیه برای استفاده در Save
                    _original_N_SERI = CheckExistData.FirstOrDefault()?.N_SERI;
                    _original_BANK = CheckExistData.FirstOrDefault()?.BANK;
                    _original_DATE_S = CheckExistData.FirstOrDefault()?.DATE_S;
                    _original_MABL = KhazanehRow.MABL;
                }
                else
                {
                    N_SERI.Text = KhazanehRow.N_SERI?.ToString() ?? "";
                    BANK.SelectedValue = KhazanehRow.BANK?.ToString();
                    _original_N_SERI = KhazanehRow.N_SERI;
                    _original_BANK = KhazanehRow.BANK;
                    _original_DATE_S = null;
                }
            }

            N_HESAB.ItemSource = dbms.DoGetDataSQL<N_HESAB_MODEL>("SELECT DISTINCT N_HESAB FROM dbo.PAY_GETD").ToList();

            if (IsReadOnlyMode)
            {
                N_SERI.IsEnabled = false;
                BANK.IsEnabled = false;
                SHOBEH.IsEnabled = false;
                LIST_NO.IsEnabled = false;
                DATE_S.IsEnabled = false;
                DATE.IsEnabled = false;
                MABL.IsEnabled = false;
                NAME_TAH.IsEnabled = false;
                N_HESAB.IsEnabled = false;
                SANDUGH.IsEnabled = false;
                SAYADI.IsEnabled = false;
                HES.IsEnabled = false;

                _SaveExit.IsEnabled = false;
                _SaveExit.Visibility = Visibility.Collapsed;

                this.Title += " (فقط خواندنی)";
            }
        }

        private void Fill_ComboBoxes()
        {
            SANDUGH.ItemsSource = dbms.DoGetDataSQL<QueryT1>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = " + CL_HESABDARI.GETMOIN(Baseknow.ADA) + ")").ToList();
            SANDUGH.SelectedValuePath = "TNUMBER";
            SANDUGH.DisplayMemberPath = "NAME";

            HES.ItemsSource = dbms.DoGetDataSQL<QueryT2>(@"SELECT 
                                                            RTRIM(CAST(TOTA_HES.NUMBER AS nvarchar)) + '-' + 
                                                            RTRIM(CAST(DETA_HES.NUMBER AS nvarchar)) + '-' + 
                                                            RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes, 
                                                            TDETA_HES.NAME
                                                        FROM TOTA_HES
                                                        INNER JOIN DETA_HES 
                                                            INNER JOIN TDETA_HES 
                                                                ON DETA_HES.NUMBER = TDETA_HES.NUMBER 
                                                               AND DETA_HES.N_KOL = TDETA_HES.N_KOL 
                                                            ON TOTA_HES.NUMBER = DETA_HES.N_KOL
                                                        ").ToList();
            HES.SelectedValuePath = "hes";
            HES.DisplayMemberPath = "hes";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();
            BANK.SelectedValuePath = "CODE";
            BANK.DisplayMemberPath = "NAMES";

            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.SHOBEH FROM PAY_GETD GROUP BY PAY_GETD.SHOBEH ORDER BY PAY_GETD.SHOBEH").ToList();
            SHOBEH.SelectedValuePath = "SHOBEH ";
            SHOBEH.DisplayMemberPath = "SHOBEH ";

            LIST_NO.ItemsSource = dbms.DoGetDataSQL<LIST_NO_CSHARP>("SELECT LIST_NO FROM PAY_GETD GROUP BY LIST_NO").ToList();
            LIST_NO.SelectedValuePath = "LIST_NO";
            LIST_NO.DisplayMemberPath = "LIST_NO";

            //NAME_TAH
            string NAME_TAH_DISPLAY = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).NAME_FHES;
            var NAME_TAH_TS1 = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.NAME_TAH FROM PAY_GETD GROUP BY PAY_GETD.NAME_TAH ORDER BY PAY_GETD.NAME_TAH").ToList();
            if (!(NAME_TAH_TS1).Any(item => item?.NAME_TAH == NAME_TAH_DISPLAY))
            {
                (NAME_TAH_TS1).Add(new PAY_GETD { NAME_TAH = NAME_TAH_DISPLAY });
            }
            NAME_TAH.ItemsSource = NAME_TAH_TS1;
            NAME_TAH.SelectedValuePath = "NAME_TAH";
            NAME_TAH.DisplayMemberPath = "NAME_TAH";
            if (!string.IsNullOrEmpty(NAME_TAH_DISPLAY))
            {
                NAME_TAH.SelectedValue = NAME_TAH_DISPLAY;
            }
        }

        private void BANK_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (BANK.IsEditable && e.OriginalSource is not TextBox) return;
            if (!NowIsReady) return;

            if (BANK.Template?.FindName("PART_EditableTextBox", BANK) is not TextBox textBox)
                return;

            string inputCode = textBox?.Text?.Trim();
            if (string.IsNullOrEmpty(inputCode)) return;

            if (BANK.SelectedValue == null && !int.TryParse(inputCode, out _))
                return;

            if (BANK.SelectedValue == null && BANK.ItemsSource is List<TCOD_BANKS> bankList)
            {
                if (int.TryParse(inputCode, out int bankCode))
                {
                    var matchingBank = bankList.FirstOrDefault(b => b?.CODE == bankCode);
                    if (matchingBank != null)
                        BANK.SelectedValue = matchingBank.CODE;
                }
            }

            // Avoid null reference
            if (!string.IsNullOrWhiteSpace(N_SERI?.Text) && BANK.SelectedValue is not null)
            {
                if (SANDUGH?.SelectedValue is not null && !string.IsNullOrEmpty(DATE_S?.Text?.ToRawTarikh()))
                    return;

                string excludeQuery = CurrentRecordID != null && CurrentRecordID > 0
                    ? $" AND ID <> {CurrentRecordID}"
                    : (_original_N_SERI != null && _original_BANK != null ? $" AND NOT (N_SERI = '{_original_N_SERI}' AND BANK = {_original_BANK})" : "");

                var rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE N_SERI = N'{N_SERI.Text}' AND BANK = {BANK.SelectedValue}{excludeQuery}")?.ToList();

                if (rst?.Count > 0)
                {
                    var first = rst.First();
                    new Msgwin(false, "چکی با همین سریال و بانک قبلاً ثبت شده است...").ShowDialog();

                    N_SERI.Text = first.N_SERI?.ToString();
                    BANK.SelectedValue = first.BANK.ToString();
                    DATE_S.Text = first.DATE_S.ToString();
                    SHOBEH.SelectedValue = first.SHOBEH?.ToString();
                    LIST_NO.SelectedValue = first.LIST_NO?.ToString();
                    DATE.Text = first.DATE.ToString();
                    NAME_TAH.SelectedValue = first.NAME_TAH?.ToString();
                    N_HESAB.Text = first.N_HESAB?.ToString();
                    MABL.Text = first.MABL?.ToString();
                    MABL.IsReadOnly = false;

                    if (first.N_KOL?.ToString() == "911" && first.N_MOIN?.ToString() == "1" && first.N_TAF?.ToString() == "1") //911-1-1
                    {
                        //NOT
                    }
                    else
                    {
                        N_KOL = first.N_KOL?.ToString();
                        N_MOIN = first.N_MOIN?.ToString();
                        N_TAF = first.N_TAF?.ToString();
                        KIND.SelectedValue = first.KIND?.ToString();
                        if (!string.IsNullOrWhiteSpace(N_KOL))
                        {
                            HES.SelectedValue = first.HES1?.ToString();
                        }
                    }

                    SANDUGH.SelectedValue = first.SANDUGH?.ToString();
                    SAYADI.Text = first.SAYADI?.ToString();
                }
            }
        }

        private void HES_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (HES.IsEditable && e.OriginalSource is not TextBox) return;

            string selected = HES?.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selected))
            {
                if (CL_HESABDARI.GETKOL(selected) != Baseknow.BANKHA)
                {
                    new Msgwin(false, "چک در این بخش فقط به بانک قابل واگذاری می‌باشد").ShowDialog();
                    CANCEL = true;
                }

                N_KOL = CL_HESABDARI.GETKOL(selected).ToString();
                N_MOIN = CL_HESABDARI.GETMOIN(selected).ToString();
                N_TAF = CL_HESABDARI.GETTAF(selected).ToString();
            }
            else
            {
                N_KOL = N_MOIN = N_TAF = null;
            }
        }

        private void SHOBEH_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (SHOBEH.IsEditable && e.OriginalSource is not TextBox) return;

            if (SHOBEH.Template?.FindName("PART_EditableTextBox", SHOBEH) is TextBox shobeText)
            {
                string val = shobeText?.Text?.Trim();
                if (!string.IsNullOrEmpty(val) &&
                    SHOBEH.ItemsSource is List<PAY_GETD> list &&
                    !list.Any(item => item?.SHOBEH == val))
                {
                    list.Add(new PAY_GETD { SHOBEH = val });
                    SHOBEH.SelectedValue = val;
                }
            }
        }
        private void LIST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (LIST_NO.IsEditable && e.OriginalSource is not TextBox) return;

            if (LIST_NO.Template?.FindName("PART_EditableTextBox", LIST_NO) is TextBox listNoText)
            {
                string val = listNoText?.Text?.Trim();
                if (!string.IsNullOrEmpty(val) && LIST_NO.ItemsSource is List<LIST_NO_CSHARP> list &&
                    !list.Any(item => item?.LIST_NO.ToString() == val)
                    && CL_LMethods.IsNumeric(val))
                {
                    list.Add(new LIST_NO_CSHARP { LIST_NO = Convert.ToInt32(val) });
                    LIST_NO.SelectedValue = val;
                }
            }
        }

        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            TextBox listNoText = LIST_NO.Template?.FindName("PART_EditableTextBox", LIST_NO) as TextBox;
            if (string.IsNullOrEmpty(listNoText?.Text) || !int.TryParse(listNoText?.Text, out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد شعبه صحیح نیست" });
            }
            TextBox nameTahText = NAME_TAH.Template?.FindName("PART_EditableTextBox", NAME_TAH) as TextBox;
            if (string.IsNullOrEmpty(nameTahText?.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام پرداخت کننده نمی تواند خالی باشد" });
            }
            if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue) || IsNull(this.DATE_S.Text.ToRawTarikh()) || this.DATE_S.Text.ToRawTarikh() == "")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سريال ، نام بانك و تاريخ سررسيد  نمي تواند خالي باشد!" });
            }
            if (NAME_TAH.Text?.Length > 190)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام پرداخت کنند باید مختصر و کوتاه باشد" });
            }

            if (!DATE_IS_VALID(DATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح نیست" });
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

            // Sync N_SERI and BANK to parent row BEFORE validation and save
            if (THE_WIN is PGET_HED pgetHed && pgetHed.CURRENT_ITMES_ROW != null)
            {
                if (double.TryParse(N_SERI.Text, out double serialVal))
                {
                    pgetHed.CURRENT_ITMES_ROW.N_SERI = serialVal;
                }
                if (BANK.SelectedValue != null && int.TryParse(BANK.SelectedValue.ToString(), out int bankVal))
                {
                    pgetHed.CURRENT_ITMES_ROW.BANK = bankVal;
                }
            }

            //Validations:
            (THE_WIN as PGET_HED).CmdSaveRecord((THE_WIN as PGET_HED).CURRENT_ITMES_ROW);

            //Click
            try
            {
                {
                    DateTime dt;
                    dt = DateTime.Now;
                    CL_HESABDARI.TR("PAY_GETD", "N_SERI = " + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue.ToString() + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh(), dt, 1);
                    can = false;
                    if (mabup)
                    {
                        var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * from PAY_GETD where N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh()).ToList();
                        if (rst.Count > 0)
                        {
                            rst.FirstOrDefault().MABL = Convert.ToDouble(this.MABL.Text);
                            string _where = " where N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();
                            dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET MABL = {rst.FirstOrDefault().MABL} {_where} ");
                        }
                    }
                    if (Convert.ToDouble(this.MABL.Text) != ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).MABL)
                    {
                        this.MABL.Text = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).MABL.ToString();
                    }
                    if (this.CUST_NO != ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES || IsNull(this.CUST_NO))
                    {
                        this.CUST_NO = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES;
                    }
                    if (((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES == Baseknow.ADA)
                    {
                        if (Convert.ToInt32(this.KIND.SelectedValue) != 1 || IsNull(this.KIND.SelectedValue))
                        {
                            this.KIND.SelectedValue = 1;
                        }
                    }
                    else if (Convert.ToInt32(this.KIND.SelectedValue) != 0 || IsNull(this.KIND.SelectedValue))
                    {
                        this.KIND.SelectedValue = 0;
                    }
                    if (this.NAME_TAH.SelectedValue == "")
                    {
                        this.NAME_TAH.SelectedValue = " ";
                    }
                }
            }
            catch { }

            //BeforeUpdate
            long dfn;
            long rdn;
            if (can)
            {
                CANCEL = true;
            }
            else
            {
                if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue))
                {
                    CANCEL = true;
                    return;
                }
                else
                {
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).N_SERI = Convert.ToDouble(N_SERI.Text);
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).BANK = Convert.ToInt32(BANK.SelectedValue);
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).SHARH = Strings.Left(" چك" + N_SERI.Text + "بانك" + CL_HESABDARI.GETBANK(Convert.ToDouble(BANK.SelectedValue)) + " " + SHOBEH.SelectedValue + " مورخ " + Strings.Format(Convert.ToInt32(DATE_S.Text.ToRawTarikh()), "####/##/##") + "-" + NAME_TAH.Text, 255);
                    CANCEL = false;
                }
                if (this.CUST_NO != ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES || IsNull(this.CUST_NO))
                {
                    this.CUST_NO = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES;
                }
                var rst = dbms.DoGetDataSQL<PAY_GETD_LOG>("SELECT * FROM dbo.PAY_GETD_LOG").ToList();
                dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI,             BANK,             DATE_S,                      DATE_V,                    DATETIM, VAZ,    SANDUGH,                 USER_NAME)
                                                          VALUES ({N_SERI.Text},{BANK.SelectedValue},{DATE_S.Text.ToRawTarikh()}, {CL_HESABDARI.FARSIDATE()},  GETDATE(),  1, {SANDUGH.SelectedValue}, N'{CL_HESABDARI.UCurrentUser()}')");

                if (((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES == Baseknow.ADA)
                {
                    if (Convert.ToInt32(KIND.SelectedValue) != 1 || IsNull(this.KIND.SelectedValue))
                    {
                        this.KIND.SelectedValue = 1;
                    }
                }
                else if (Convert.ToInt32(KIND.SelectedValue) != 0 || IsNull(this.KIND.SelectedValue))
                {
                    this.KIND.SelectedValue = 0;
                }
                var rst2 = dbms.DoGetDataSQL<int?>("SELECT TOP 100 PERCENT FIRSTNUM, BOOKNUM FROM dbo.DAFT_ASN ORDER BY BOOKNUM DESC").ToList();
                if (rst2.Count > 0)
                {
                    rdn = Convert.ToInt32(rst2.FirstOrDefault(0));
                    dfn = Convert.ToInt32(rst2.FirstOrDefault(1));
                }
                else
                {
                    Msgwin msgwin = new Msgwin(false, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع: 1 شماره دفتر: 1");
                    msgwin.Show();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DAFT_ASN(FIRSTNUM, BOOKNUM)
                                                               VALUES(1        ,1)");

                    rdn = 1L;
                    dfn = 1L;
                }

                bool isNewRadif = false;

                if (string.IsNullOrWhiteSpace(RADIF.Text) || RADIF.Text == "0")
                {
                    isNewRadif = true;
                    var rst3 = dbms.DoGetDataSQL<double?>("SELECT Max(PAY_GETD.RADIF) AS MaxOfRADIF  FROM PAY_GETD WHERE ANBAR = " + dfn).ToList();
                    if (rst3.Count == 0 || IsNull(rst3?.FirstOrDefault()))
                    {
                        this.RADIF.Text = rdn.ToString();
                        this.ANBAR = dfn.ToString();
                    }
                    else
                    {
                        this.RADIF.Text = Convert.ToString(rst3.FirstOrDefault(0) + 1);
                        this.ANBAR = dfn.ToString();
                    }
                }

                PAY_GETD existingRecord = null;
                if (CurrentRecordID != null && CurrentRecordID > 0)
                {
                    existingRecord = dbms.DoGetDataSQL<PAY_GETD>($"SELECT TOP 1 * FROM PAY_GETD WHERE ID = {CurrentRecordID}").FirstOrDefault();
                }

                var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;
                var _SHOBEH_ = SHOBEH.SelectedValue.ToStringNullSafe().Length > 20 ? SHOBEH.SelectedValue.ToStringNullSafe().Substring(0, 19) : SHOBEH.SelectedValue.ToStringNullSafe();

                var _SAYADI_ = SAYADI.Text.Length > 16 ? SAYADI.Text.Substring(0, 16) : SAYADI.Text;

                string selected = HES?.SelectedValue?.ToString()?.Trim(); // حساب مقصد/واگذاری؛ مستقل از CUST_NO
                bool hasCheckMovement = existingRecord?.VAZ == 4 ||
                                        existingRecord?.N_KOL2 != null ||
                                        existingRecord?.N_KOL3 != null;
                string destinationHes;

                if (hasCheckMovement)
                {
                    // پس از ورود چک به گردش، حساب‌های واگذاری فقط باید توسط فرم‌های
                    // FORCHEK / BAKCHEK / وصول تغییر کنند؛ ویرایش مشخصات چک نباید آن‌ها را بازنویسی کند.
                    N_KOL = existingRecord?.N_KOL?.ToString();
                    N_MOIN = existingRecord?.N_MOIN?.ToString();
                    N_TAF = existingRecord?.N_TAF?.ToString();
                    destinationHes = existingRecord?.HES1;
                }
                else if (string.IsNullOrWhiteSpace(selected) || selected == "911-1-1")
                {
                    N_KOL = N_MOIN = N_TAF = null;
                    destinationHes = null;

                    if (selected == "911-1-1" && HES?.SelectedValue != null)
                    {
                        HES.SelectedValue = null;
                    }
                }
                else
                {
                    if (CL_HESABDARI.GETKOL(selected) != Baseknow.BANKHA)
                    {
                        new Msgwin(false, "چک در این بخش فقط به بانک قابل واگذاری می‌باشد").ShowDialog();
                        CANCEL = true;
                        return;
                    }

                    N_KOL = CL_HESABDARI.GETKOL(selected).ToString();
                    N_MOIN = CL_HESABDARI.GETMOIN(selected).ToString();
                    N_TAF = CL_HESABDARI.GETTAF(selected).ToString();
                    destinationHes = selected;
                }

                try
                {
                    // آماده‌سازی پارامترها
                    var parameters = new
                    {
                        N_SERI = N_SERI.Text,
                        BANK = BANK.SelectedValue,
                        DATE_S = DATE_S.Text.ToRawTarikh(),
                        DATE = DATE.Text.ToRawTarikh(),
                        SHOBEH = _SHOBEH_,
                        MABL = MABL.Text,
                        NAME_TAH = _NAME_TAH_,
                        ANBAR = ANBAR,
                        RADIF = RADIF.Text,
                        CUST_NO = CUST_NO,
                        VAZ = existingRecord != null ? (existingRecord.VAZ ?? 1) : 1,
                        LIST_NO = LIST_NO.SelectedValue,
                        KIND = KIND.SelectedValue,
                        SANDUGH = SANDUGH.SelectedValue,
                        SAYADI = _SAYADI_,
                        N_HESAB = string.IsNullOrEmpty(N_HESAB.Text) ? (object)DBNull.Value : N_HESAB.Text,
                        N_KOL = string.IsNullOrEmpty(N_KOL) ? (object)DBNull.Value : N_KOL,
                        N_MOIN = string.IsNullOrEmpty(N_MOIN) ? (object)DBNull.Value : N_MOIN,
                        N_TAF = string.IsNullOrEmpty(N_TAF) ? (object)DBNull.Value : N_TAF,
                        HES1 = string.IsNullOrWhiteSpace(destinationHes) ? (object)DBNull.Value : destinationHes,
                        ID = CurrentRecordID
                    };

                    if (existingRecord != null)
                    {
                        var updateSql = @"UPDATE dbo.PAY_GETD 
                                SET N_SERI = @N_SERI, 
                                    BANK = @BANK, 
                                    DATE_S = @DATE_S, 
                                    DATE = @DATE, 
                                    SHOBEH = @SHOBEH, 
                                    MABL = @MABL, 
                                    NAME_TAH = @NAME_TAH, 
                                    ANBAR = @ANBAR, 
                                    RADIF = @RADIF, 
                                    CUST_NO = @CUST_NO, 
                                    VAZ = @VAZ, 
                                    LIST_NO = @LIST_NO, 
                                    KIND = @KIND, 
                                    SANDUGH = @SANDUGH, 
                                    SAYADI = @SAYADI, 
                                    N_HESAB = @N_HESAB, 
                                    N_KOL = @N_KOL, 
                                    N_MOIN = @N_MOIN, 
                                    N_TAF = @N_TAF,
                                    HES1 = @HES1
                                WHERE ID = @ID";
                        dbms.DoExecuteSQL(updateSql, parameters);
                    }
                    else
                    {
                        var insertSql = @"INSERT INTO dbo.PAY_GETD(
                                     N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, 
                                     ANBAR, RADIF, CUST_NO, VAZ, LIST_NO, KIND, SANDUGH, 
                                     N_HESAB, SAYADI, N_KOL, N_MOIN, N_TAF, HES1)
                                 VALUES(
                                     @N_SERI, @BANK, @DATE_S, @DATE, @SHOBEH, @MABL, @NAME_TAH, 
                                     @ANBAR, @RADIF, @CUST_NO, @VAZ, @LIST_NO, @KIND, @SANDUGH, 
                                     @N_HESAB, @SAYADI, @N_KOL, @N_MOIN, @N_TAF, @HES1)";

                        dbms.DoExecuteSQL(insertSql, parameters);
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات تکراری است").ShowDialog(); return;
                }

                if (isNewRadif)
                {
                    Msgwin msgwin1 = new Msgwin(false, $"شماره دفتر :{this.RADIF.Text}");
                    msgwin1.Show();
                }


                (THE_WIN as PGET_HED).CmdSaveRecord(((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST));
                (THE_WIN as Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED).SANAD();

                this.Close();

            }
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            can = true;
            if (THE_WIN is PGET_HED PGWIN)
            {
                PGWIN.IsExitChkButtonPressed = true;
                this.Close();
            }
        }

        private void DATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            string date_n_val = DATE.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE.Text = null;
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE.Text = null;
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            else
            {
                DATE.Text = null;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }

        public bool DATE_IS_VALID(string DATE, bool DisplayMsg = false, bool ForceSync = true)
        {
            bool Date_Is_Valid = true;

            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    }
                    Date_Is_Valid = false;
                }
                else
                {
                    if (ForceSync && !Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        if (DisplayMsg)
                        {
                            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        }
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            N_SERI.SetFocusToTextBox();
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
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", (THE_WIN as HESABDARI.PGET_HED).Pop1, (THE_WIN as HESABDARI.PGET_HED).Pop1Text1, (THE_WIN as HESABDARI.PGET_HED).Pop_Border1);
                    return;
                }
            }
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

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }
    }
}
