using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.HESABDARI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextBox = System.Windows.Controls.TextBox;



namespace Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for SGETCHEK.xaml
    /// </summary>
    public partial class SGETCHEK : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public Visual THE_WIN { get; set; }
        public string MABL_CHEK_ARG { get; set; }
        public string DATE_CHEK_ARG { get; set; }
        public int INDEX_DG { get; set; }
        public bool NowIsReady { get; private set; }

        public string N_KOL { get; set; }
        public string N_MOIN { get; set; }
        public string BEFOREDATEN { get; private set; }
        public string N_TAF { get; set; }
        public string ANBAR { get; set; } = "1";
        public bool can { get; private set; }
        public bool CANCEL { get; private set; }

        private bool mabup;

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

        public class QueryT3
        {
            public string? hes { get; set; }
            public string? nam { get; set; }
            public string? Expr1 { get; set; }
        }

        public SGETCHEK(Visual the_win, string _mabl_chek_arg = null, int _current_index = -1)
        {
            THE_WIN = the_win;
            MABL_CHEK_ARG = _mabl_chek_arg;
            INDEX_DG = _current_index;
            InitializeComponent();
        }

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

        public class N_HESAB_MODEL
        {
            public string N_HESAB { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            // On Open here ...
            #region ON_Open
            //if (!IsNull(this.N_KOL))
            //{
            //    this.HES.SelectedValue = this.N_KOL + "-" + this.N_MOIN + "-" + this.N_TAF;
            //}

            Fill_ComboBoxes();
            MABL.Text = MABL_CHEK_ARG;
            mabup = false;
            SANDUGH.SelectedValue = 1;
            //DoCmd.OpenForm "GETCHEK", acNormal, , "N_SERI = " & Me.N_SERI & " AND BANK = " & Me.BANK & " AND MABL = " & Me.mabl, , acDialog
            var KhazanehRow = ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL);
            DATE.Text = (THE_WIN as DEED_HEAD).DATE_S.Text.ToRawTarikh();

            if (KhazanehRow.N_SERI is not null && KhazanehRow.BANK is not null && KhazanehRow.BED is not null)
            {
                var CheckExistData = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE N_SERI = {KhazanehRow.N_SERI} AND BANK = {KhazanehRow.BANK} AND MABL = {KhazanehRow.BED}").ToList();
                if (CheckExistData.Count > 0)
                {
                    N_SERI.Text = CheckExistData.FirstOrDefault()?.N_SERI.ToString();
                    BANK.SelectedValue = CheckExistData.FirstOrDefault()?.BANK.ToString();
                    SHOBEH.SelectedValue = CheckExistData.FirstOrDefault()?.SHOBEH?.ToString();
                    LIST_NO.SelectedValue = CheckExistData.FirstOrDefault()?.LIST_NO?.ToString();
                    DATE_S.Text = CheckExistData.FirstOrDefault()?.DATE_S.ToString();
                    DATE.Text = CheckExistData.FirstOrDefault()?.DATE.ToString();
                    MABL.Text = CheckExistData.FirstOrDefault()?.MABL.ToString();
                    NAME_TAH.SelectedValue = CheckExistData.FirstOrDefault()?.NAME_TAH?.ToString();
                    N_HESAB.Text = CheckExistData.FirstOrDefault()?.N_HESAB?.ToString();
                    SANDUGH.SelectedValue = CheckExistData.FirstOrDefault()?.SANDUGH?.ToString();
                    CUST_NO.SelectedValue = CheckExistData.FirstOrDefault()?.CUST_NO?.ToString();
                    CUST_NO_2.SelectedValue = CheckExistData.FirstOrDefault()?.CUST_NO?.ToString();
                    SAYADI.Text = CheckExistData.FirstOrDefault()?.SAYADI?.ToString();

                    string _Old_HES = CheckExistData.FirstOrDefault()?.N_KOL.ToString() + "-" + CheckExistData.FirstOrDefault()?.N_MOIN.ToString() + "-" + CheckExistData.FirstOrDefault()?.N_TAF.ToString();
                    HES.SelectedValue = _Old_HES;
                }
            }
            #endregion

            N_HESAB.ItemSource = dbms.DoGetDataSQL<N_HESAB_MODEL>("SELECT DISTINCT N_HESAB FROM dbo.PAY_GETD").ToList();


        }

        private void Fill_ComboBoxes()
        {
            SANDUGH.ItemsSource = dbms.DoGetDataSQL<QueryT1>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = " + CL_HESABDARI.GETMOIN(Baseknow.ADA) + ")").ToList();
            SANDUGH.SelectedValuePath = "TNUMBER";
            SANDUGH.DisplayMemberPath = "NAME";
            //ERROR
            HES.ItemsSource = dbms.DoGetDataSQL<QueryT2>("SELECT RTRIM(CAST(TOTA_HES.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(DETA_HES.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes, TDETA_HES.NAME FROM TOTA_HES INNER JOIN DETA_HES INNER JOIN TDETA_HES ON DETA_HES.NUMBER = TDETA_HES.NUMBER AND DETA_HES.N_KOL = TDETA_HES.N_KOL ON TOTA_HES.NUMBER = DETA_HES.N_KOL WHERE (dbo.DETA_HES.N_KOL  = " + Baseknow.BANKHA + ")").ToList();
            HES.SelectedValuePath = "hes";
            HES.DisplayMemberPath = "NAME";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();
            BANK.SelectedValuePath = "CODE";
            BANK.DisplayMemberPath = "NAMES";

            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.SHOBEH FROM PAY_GETD GROUP BY PAY_GETD.SHOBEH ORDER BY PAY_GETD.SHOBEH").ToList();
            SHOBEH.SelectedValuePath = "SHOBEH ";
            SHOBEH.DisplayMemberPath = "SHOBEH ";

            LIST_NO.ItemsSource = dbms.DoGetDataSQL<LIST_NO_CSHARP>("SELECT LIST_NO FROM PAY_GETD GROUP BY LIST_NO").ToList();
            LIST_NO.SelectedValuePath = "LIST_NO";
            LIST_NO.DisplayMemberPath = "LIST_NO";

            //string test = dbms.DoGetDataSQL<QueryT3>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes, NAME AS nam, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS Expr1 FROM TDETA_HES").ToString();

            CUST_NO.ItemsSource = dbms.DoGetDataSQL<QueryT3>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes, NAME AS nam, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS Expr1 FROM TDETA_HES").ToList();
            CUST_NO.SelectedValuePath = "hes";
            CUST_NO.DisplayMemberPath = "nam";

            CUST_NO_2.ItemsSource = dbms.DoGetDataSQL<QueryT3>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes, NAME AS nam, RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS Expr1 FROM TDETA_HES").ToList();
            CUST_NO_2.SelectedValuePath = "hes";
            CUST_NO_2.DisplayMemberPath = "hes";

            #region NAME_TAH
            string NAME_TAH_DISPLAY = ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).NAME_HES;
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
            #endregion
        }

        private void BANK_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (BANK.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            #region BANK_After_Update
            if (!IsNull(N_SERI.Text) & !IsNull(BANK.SelectedValue))
            {
                var rst = dbms.DoGetDataSQL<PAY_GETD>("select * from PAY_GETD where N_SERI=" + N_SERI.Text + " AND BANK = " + BANK.SelectedValue).ToList();
                if (rst.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود");
                    msgwin.ShowDialog();
                    //DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود");
                    this.N_SERI.Text = rst.FirstOrDefault().N_SERI.ToString();
                    this.BANK.SelectedValue = rst.FirstOrDefault().BANK.ToString();
                    this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                    this.SHOBEH.SelectedValue = rst.FirstOrDefault().SHOBEH.ToString();
                    this.LIST_NO.SelectedValue = rst.FirstOrDefault().LIST_NO.ToString();
                    this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                    this.NAME_TAH.SelectedValue = rst.FirstOrDefault().NAME_TAH.ToString();
                    this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB?.ToString();
                    this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                    this.MABL.IsTabStop = true;
                    this.MABL.IsReadOnly = false;
                    this.N_KOL = rst.FirstOrDefault().N_KOL.ToString();
                    this.N_MOIN = rst.FirstOrDefault().N_MOIN.ToString();
                    this.N_TAF = rst.FirstOrDefault().N_TAF.ToString();
                    this.KIND.SelectedValue = rst.FirstOrDefault().KIND.ToString();
                    this.SANDUGH.SelectedValue = rst.FirstOrDefault().SANDUGH.ToString();

                }
            }

            #region NUM_TO_BANK
            if (!NowIsReady)
            {
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                string inputCode = textBox.Text;
                if (!int.TryParse(inputCode, out int _))
                {
                    return;
                }

                // Cast the ItemsSource back to the correct type, and search for matching bank code
                var bankList = comboBox.ItemsSource as List<TCOD_BANKS>;

                if (bankList == null) return; // Ensure that the list is properly cast

                var matchingBank = bankList.FirstOrDefault(b => b.CODE == Convert.ToInt32(inputCode));

                if (matchingBank != null)
                {
                    BANK.SelectedValue = matchingBank.CODE;
                    comboBox.SelectedValue = matchingBank.CODE;
                    return;
                }
                else
                {
                    comboBox.SelectedValue = null;
                }
            }
            #endregion

            #endregion
        }

        private void MABL_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            return;
            //if (MABL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            #region Before_Update
            if (!string.IsNullOrEmpty(MABL.Text))
            {
                var rst = dbms.DoGetDataSQL<PAY_GETD>("select * from PAY_GETD where  N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue).ToList();
                if (rst.Count > 0)
                {
                    if (!IsNull(rst.FirstOrDefault().N_KOL2) & rst.FirstOrDefault().N_KOL2 != 911 | !IsNull(rst.FirstOrDefault().N_KOL3) | rst.FirstOrDefault().N_KOL != Baseknow.BANKHA & rst.FirstOrDefault().N_KOL != 911)
                    {
                        Msgwin msgwin = new Msgwin(false, "مبلغ چكي كه وصولي يا واگذاري يا برگشتي خورده قابل تغيير  نيست .ابتدا وصولي يا برگشتي يا واگذاري آن را حذف كنيد سپس مبلغ آن را اصلاح كنيد در حال حاضر با خارج شدن از اين بخش دقت كنيد مبلغ خزانه داري را هم اصلاح كنيد");
                        msgwin.ShowDialog();
                        //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "مبلغ چكي كه وصولي يا واگذاري يا برگشتي خورده قابل تغيير  نيست .ابتدا وصولي يا برگشتي يا واگذاري آن را حذف كنيد سپس مبلغ آن را اصلاح كنيد در حال حاضر با خارج شدن از اين بخش دقت كنيد مبلغ خزانه داري را هم اصلاح كنيد");
                        CANCEL = true;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("مبلغ نباید خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
            }
            #endregion

            #region After_Update
            mabup = true;
            #endregion
        }

        private void HES_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (HES.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            #region Before_Update
            if (!IsNull(this.HES))
            {
                if (CL_HESABDARI.GETKOL(this.HES.SelectedValue?.ToString()) != Baseknow.BANKHA)
                {
                    Msgwin msgwin = new Msgwin(false, "چك در اين بخش فقط به بانك قابل واگذاري ميباشد");
                    msgwin.ShowDialog();
                    //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "چك در اين بخش فقط به بانك قابل واگذاري ميباشد");
                    CANCEL = true;
                }
            }
            #endregion

            #region After_Update
            if (!IsNull(this.HES))
            {
                this.N_KOL = Convert.ToString(CL_HESABDARI.GETKOL(this.HES.SelectedValue?.ToString()));
                this.N_MOIN = Convert.ToString(CL_HESABDARI.GETMOIN(this.HES.SelectedValue?.ToString()));
                this.N_TAF = Convert.ToString(CL_HESABDARI.GETTAF(this.HES.SelectedValue?.ToString()));
            }
            else
            {
                this.N_KOL = null;
                this.N_MOIN = null;
                this.N_TAF = null;
            }
            #endregion
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            can = true;
            //(THE_WIN as DEED_HEAD).IsExitChkButtonPressed = true;
            this.Close();
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

        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            //Validations:
            //(THE_WIN as DEED_HEAD).CmdSaveRecord((THE_WIN as DEED_HEAD).CURRENT_ITMES_ROW);
            #region Click
            try
            {
                List<MsgModel> ErrosMessages = new List<MsgModel>();

                TextBox NAME_TAH_TEX = (TextBox)NAME_TAH.Template.FindName("PART_EditableTextBox", NAME_TAH);
                if (string.IsNullOrEmpty(NAME_TAH_TEX.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نام پرداخت کننده نمی تواند خالی باشد" });
                }
                TextBox LIST_NO_TEX = (TextBox)LIST_NO.Template.FindName("PART_EditableTextBox", LIST_NO);
                if (string.IsNullOrEmpty(LIST_NO_TEX.Text) || !int.TryParse(LIST_NO_TEX.Text, out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "کد شعبه صحیح نیست" });
                }
                if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue) || IsNull(this.DATE_S.Text.ToRawTarikh()) || this.DATE_S.Text.ToRawTarikh() == "")
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سريال ، نام بانك و تاريخ سررسيد  نمي تواند خالي باشد!" });
                }
                if (ErrosMessages.Any())
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                    return;
                }

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
                            //rst.update();
                            string _where = " where N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();
                            dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET MABL = {rst.FirstOrDefault().MABL} {_where} ");
                        }
                        //rst.Close();
                    }
                    if (Convert.ToDouble(this.MABL.Text) != ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).BED)
                    {
                        this.MABL.Text = ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).BED.ToString();
                    }
                    //if (this.CUST_NO != ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES || IsNull(this.CUST_NO))
                    //{
                    //    this.CUST_NO = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES;
                    //}
                    if (((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).HES_T.ToString() == Baseknow.ADA)
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
                    if (this.NAME_TAH.SelectedValue == "")
                    {
                        this.NAME_TAH.SelectedValue = " ";
                    }
                    //DoCmd.Close(acForm, this.NAME, acSaveYes);
                }


            }
            catch
            {

                //errdetector(Information.Err());
            }
            #endregion

            #region BeforeUpdate
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
                }
                else
                {
                    ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).N_SERI = Convert.ToDouble(N_SERI.Text);
                    //Forms["PGET_HED"]["PGET_LST_SUB"].Form["N_SERI"] = this.N_SERI;
                    ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).BANK = Convert.ToInt32(BANK.SelectedValue);
                    //Forms["PGET_HED"]["PGET_LST_SUB"].Form["BANK"] = this.BANK;
                    ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).SHARH = Strings.Left("چك" + N_SERI.Text + "بانك" + CL_HESABDARI.GETBANK(Convert.ToDouble(BANK.SelectedValue)) + " " + SHOBEH.SelectedValue + " مورخ " + Strings.Format(Convert.ToInt32(DATE_S.Text.ToRawTarikh()), "####/##/##") + "-" + NAME_TAH.Text, 255);

                    //(THE_WIN as PGET_HED).MOLAH.Text = "Test";
                    //(THE_WIN as PGET_HED).KHAZANEH_DATA[0].SHARH = "KASDKAKSDK";
                    //Forms["PGET_HED"]["PGET_LST_SUB"].Form["SHARH"] = Strings.Left("ß " + N_SERI + "ÈÇäß " + GETBANK(BANK) + " " + SHOBEH + " ãæÑÎ " + Strings.Format(DATE_S, "####/##/##") + "-" + NAME_TAH, 255);
                    CANCEL = false;
                }
                //if (this.CUST_NO != ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES || IsNull(this.CUST_NO))
                //{
                //    this.CUST_NO = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES;
                //}
                var rst = dbms.DoGetDataSQL<PAY_GETD_LOG>("SELECT * FROM dbo.PAY_GETD_LOG").ToList();
                // If RST.RecordCount = 0 Then
                //rst.AddNew();
                //rst.Fields("N_SERI") = this.N_SERI.Text;
                //rst.Fields("BANK") = this.BANK.SelectedValue;
                //rst.Fields("DATE_S") = this.DATE_S;
                //rst.Fields("DATE_V") = CL_HESABDARI.FARSIDATE();
                //rst.Fields("DATETIM") = DateTime.Now;
                //rst.Fields("VAZ") = 1;
                //rst.Fields("SANDUGH") = this.SANDUGH;
                //rst.Fields("USER_NAME") = CL_HESABDARI.UCurrentUser();
                //rst.update();

                dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI,             BANK,             DATE_S,                      DATE_V,                    DATETIM, VAZ,    SANDUGH,                 USER_NAME)
                                                          VALUES ({N_SERI.Text},{BANK.SelectedValue},{DATE_S.Text.ToRawTarikh()}, {CL_HESABDARI.FARSIDATE()},  GETDATE(),  1, {SANDUGH.SelectedValue}, N'{CL_HESABDARI.UCurrentUser()}')");

                if (((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL).HES_T.ToString() == Baseknow.ADA)
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
                if (IsNull(this.RADIF.Text) || this.RADIF.Text == "")
                {
                    var rst2 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FIRSTNUM, BOOKNUM FROM dbo.DAFT_ASN ORDER BY BOOKNUM DESC").ToList();
                    if (rst2.Count > 0)
                    {
                        rdn = Convert.ToInt32(rst2.FirstOrDefault(0));
                        dfn = Convert.ToInt32(rst2.FirstOrDefault(1));
                    }
                    else
                    {
                        Msgwin msgwin = new Msgwin(false, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع: 1 شماره دفتر: 1");
                        msgwin.ShowDialog();
                        //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع: 1 شماره دفتر: 1"
                        //rst2.Close();
                        //rst2.Open("DAFT_ASN");
                        //rst2.AddNew();
                        //rst2.Fields(0) = 1;
                        //rst2.Fields(1) = 1;
                        //rst2.update();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DAFT_ASN(FIRSTNUM, BOOKNUM)
                                                               VALUES(1        ,1)");

                        rdn = 1L;
                        dfn = 1L;
                        //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "شماره دفتر :" + this.RADIF);
                        //rst.Close();
                        //rst2.Close();
                    }
                    var rst3 = dbms.DoGetDataSQL<double?>("SELECT Max(PAY_GETD.RADIF) AS MaxOfRADIF  FROM PAY_GETD WHERE ANBAR = " + dfn).ToList();
                    if (rst3.Count == 0 || IsNull(rst3.FirstOrDefault()))
                    {
                        this.RADIF.Text = rdn.ToString();
                        this.ANBAR = dfn.ToString();
                    }
                    else
                    {
                        this.RADIF.Text = Convert.ToString(rst3.FirstOrDefault(0) + 1);
                        this.ANBAR = dfn.ToString();
                    }

                    N_KOL = CL_HESABDARI.GETKOL(HES.SelectedValue.ToString()).ToString();
                    N_MOIN = CL_HESABDARI.GETMOIN(HES.SelectedValue.ToString()).ToString();
                    N_TAF = CL_HESABDARI.GETTAF(HES.SelectedValue.ToString()).ToString();

                    var KhazanehRow = ((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL);
                    var CheckExistData = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE N_SERI = {KhazanehRow.N_SERI} AND BANK = {KhazanehRow.BANK} AND DATE_S = {DATE_S.Text.ToRawTarikh()}").ToList();

                    var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;

                    if (CheckExistData.Count > 0)
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET N_SERI = {N_SERI.Text}, BANK = {BANK.SelectedValue}, DATE_S = {DATE_S.Text.ToRawTarikh()}, DATE = {DATE.Text.ToRawTarikh()}, SHOBEH = N'{SHOBEH.SelectedValue}', MABL = {MABL.Text}, NAME_TAH = N'{_NAME_TAH_}', ANBAR = {ANBAR}, RADIF = {RADIF.Text}, CUST_NO = N'{CUST_NO.SelectedValue}', VAZ = 1, LIST_NO = {LIST_NO.SelectedValue}, KIND = {KIND.SelectedValue}, SANDUGH = {SANDUGH.SelectedValue} , SAYADI = N'{SAYADI.Text}' , N_KOL = {(N_KOL is null ? "NULL" : N_KOL)} , N_MOIN = {(N_MOIN is null ? "NULL" : N_MOIN)} , N_TAF = {(N_TAF is null ? "NULL" : N_TAF)} 
                                                             WHERE N_SERI = {N_SERI.Text} AND BANK = {BANK.SelectedValue} ");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD(N_SERI,                BANK,                     DATE_S,                     DATE,                   SHOBEH,       MABL,          NAME_TAH,  ANBAR,       RADIF,                   CUST_NO,VAZ,                LIST_NO,                KIND,                SANDUGH,                                          N_HESAB,            SAYADI,                              N_KOL,                               N_MOIN,                              N_TAF)
				                                       VALUES( {N_SERI.Text},{BANK.SelectedValue},{DATE_S.Text.ToRawTarikh()},{DATE.Text.ToRawTarikh()},N'{SHOBEH.SelectedValue}',{MABL.Text},N'{_NAME_TAH_}',{ANBAR},{RADIF.Text},N'{CUST_NO.SelectedValue}',  1,{LIST_NO.SelectedValue},{KIND.SelectedValue},{SANDUGH.SelectedValue},{(N_HESAB.Text is null ? "NULL" : N_HESAB.Text)} , N'{SAYADI.Text}' , {(N_KOL is null ? "NULL" : N_KOL)}, {(N_MOIN is null ? "NULL" : N_MOIN)}, {(N_TAF is null ? "NULL" : N_TAF)})");
                    }

                    Msgwin msgwin1 = new Msgwin(false, $"شماره دفتر :{this.RADIF.Text}");
                    msgwin1.ShowDialog();

                    //(THE_WIN as DEED_HEAD).CmdSaveRecord(((THE_WIN as DEED_HEAD).Child14.Items[INDEX_DG] as DEED_DTL));
                    (THE_WIN as Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED).SANAD();

                    this.Close();
                }
            }
            #endregion

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
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", (THE_WIN as DEED_HEAD).Pop1, (THE_WIN as DEED_HEAD).Pop1Text1, (THE_WIN as DEED_HEAD).Pop_Border1);
                    return;
                }
            }
        }

        private void SHOBEH_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (SHOBEH.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            var shobetext = (TextBox)SHOBEH.Template.FindName("PART_EditableTextBox", SHOBEH);
            if (!string.IsNullOrEmpty(shobetext.Text))
            {
                if (!((List<PAY_GETD>)SHOBEH.ItemsSource).Any(item => item?.SHOBEH == shobetext.Text))
                {
                    ((List<PAY_GETD>)SHOBEH.ItemsSource).Add(new PAY_GETD { SHOBEH = shobetext.Text });
                    SHOBEH.SelectedValue = shobetext.Text;
                }
            }
        }

        private void LIST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (LIST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            var listnotext = (TextBox)LIST_NO.Template.FindName("PART_EditableTextBox", LIST_NO);
            if (!string.IsNullOrEmpty(listnotext.Text))
            {
                var test = LIST_NO.ToStringNullSafe();
                if (!((List<LIST_NO_CSHARP>)LIST_NO.ItemsSource).Any(item => item?.LIST_NO.ToString() == listnotext.Text))
                {
                    if (!CL_LMethods.IsNumeric(listnotext.Text))
                    {
                        return;
                    }
                    ((List<LIST_NO_CSHARP>)LIST_NO.ItemsSource).Add(new LIST_NO_CSHARP { LIST_NO = Convert.ToInt32(listnotext.Text) });
                    LIST_NO.SelectedValue = listnotext.Text;
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
        }

        private void SAYADI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (SAYADI.Text.Length < 16 && SAYADI.Text != "0")
            {
                Msgwin msgwin = new Msgwin(false, "شماره صیادی نباید کمتر از 16 رقم باشد.");
                msgwin.ShowDialog();
            }
        }

        private void CUST_NO_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            CUST_NO_2.SelectedValue = CUST_NO.SelectedValue;
        }

        private void CUST_NO_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (CUST_NO_2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            CUST_NO.SelectedValue = CUST_NO_2.SelectedValue;
        }

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }
    }
}
