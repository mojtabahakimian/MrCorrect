using AUTO_BAZ.HelperWins;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_UI.Wins.WinMenus.Checkha.BAKCHEK;
using TextBox = System.Windows.Controls.TextBox;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for CREATE_CHEKPDP.xaml
    /// </summary>
    public partial class CREATE_CHEKDP : Window
    {
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
        public int INDEX_DG { get; set; }
        public string BEFOREDATEN { get; private set; }
        public Visual THE_WIN { get; set; }
        public Visual I_AM_CREATE_CHEKDP { get; set; }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        private int RRDF;

        public CREATE_CHEKDP(Visual _thewin, int _current_index = -1)
        {
            INDEX_DG = _current_index;
            THE_WIN = _thewin;
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

        public class Query1
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }

        public class Query2
        {
            public string? hes { get; set; }
            public string? nam { get; set; }
        }

        private void Fill_ComboBoxes()
        {
            CUST_NO.ItemsSource = dbms.DoGetDataSQL<Query2>("SELECT hes, NAME + N' ' + hes AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            CUST_NO.SelectedValuePath = "hes";
            CUST_NO.DisplayMemberPath = "nam";

            CUST_NO2.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes FROM CUST_HESAB").ToList();
            CUST_NO2.SelectedValuePath = "hes";
            CUST_NO2.DisplayMemberPath = "hes";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();
            BANK.SelectedValuePath = "CODE";
            BANK.DisplayMemberPath = "NAMES";

            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.SHOBEH FROM PAY_GETD GROUP BY PAY_GETD.SHOBEH ORDER BY PAY_GETD.SHOBEH;").ToList();
            SHOBEH.SelectedValuePath = "SHOBEH";
            SHOBEH.DisplayMemberPath = "SHOBEH";

            LIST_NO.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT LIST_NO FROM PAY_GETD GROUP BY LIST_NO").ToList();
            LIST_NO.SelectedValuePath = "LIST_NO";
            LIST_NO.DisplayMemberPath = "LIST_NO";

            VAZ.ItemsSource = dbms.DoGetDataSQL<Query1>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = 113) AND (NUMBER = 1)").ToList();
            VAZ.SelectedValuePath = "TNUMBER";
            VAZ.DisplayMemberPath = "NAME";
            VAZ.SelectedIndex = 0;

            SANDUGH.ItemsSource = dbms.DoGetDataSQL<Query1>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = " + CL_HESABDARI.GETMOIN(Baseknow.ADA) + ")").ToList();
            SANDUGH.SelectedValuePath = "TNUMBER";
            SANDUGH.DisplayMemberPath = "NAME";
            SANDUGH.SelectedIndex = 0;

            List<VAZ_MODEL> comboBoxItems = new List<VAZ_MODEL>
            {
                new VAZ_MODEL { ID = 0, NAME = "تجاری" },
                new VAZ_MODEL { ID = 1, NAME = "غیر تجاری" },
             };

            KIND.ItemsSource = comboBoxItems;
            KIND.SelectedValuePath = "ID";
            KIND.DisplayMemberPath = "NAME";
            KIND.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_CREATE_CHEKDP = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
            #region Form_Load
            VAZ.ItemsSource = dbms.DoGetDataSQL<Query1>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = 1)").ToList();
            #endregion
            Fill_ComboBoxes();
        }

    

        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();

        private void CUST_NO2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CUST_NO2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (CUST_NO2.Text is null || CUST_NO2.SelectedValue == null)
            {
                return;
            }
            #region AfterUpdate
            this.CUST_NO.ItemsSource = this.CUST_NO2.ItemsSource;
            this.CUST_NO.SelectedValuePath = "hes";
            this.CUST_NO.DisplayMemberPath = "nam";
            this.NAME_TAH.Text = CL_HESABDARI.GETHESNAME(this.CUST_NO2.SelectedValue.ToString());
            #endregion
            //ERROR
            //#region NotINList
            //string SPACELESSV;
            //if ((bool)Baseknow.BARCOD  &&  Baseknow.UGRP != "1")
            //{
            //    if (CUST_NO.Text == "-" | CUST_NO.Text == "+")
            //    {
            //        DoCmd.OpenForm("SERSNDTAF1", default, default, default, default, default, "24");
            //        Response = acDataErrContinue;
            //    }
            //}
            //#endregion

        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void _Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (IsNull(CUST_NO.SelectedValue) || IsNull(N_SERI.Text) || IsNull(DATE_S.Text.ToRawTarikh()) || IsNull(SHOBEH.SelectedValue))
            {
                return;
            }
            //ERROR
            #region Click
            //var rst = new ADODB.Recordset();
            int i;
            //var RST2 = new ADODB.Recordset();
            //var rst3 = new ADODB.Recordset();
            long dt;
            long DTT;
            long MMT;
            long SALT;
            long dfn;
            long rdn;
            if (!IsNull(this.N_SERI.Text) && !IsNull(this.BANK.SelectedValue) && !IsNull(this.DATE_S.Text.ToRawTarikh()))
            {
                var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD").ToList();
                var RST2 = dbms.DoGetDataSQL<DAFT_ASN>("SELECT TOP 100 PERCENT FIRSTNUM, BOOKNUM FROM dbo.DAFT_ASN ORDER BY BOOKNUM DESC").ToList();
                if (RST2.Count > 0)
                {
                    rdn = (long)RST2.FirstOrDefault().FIRSTNUM;
                    dfn = (long)RST2.FirstOrDefault().BOOKNUM;
                }
                else
                {
                    //ERROR
                    Msgwin msgwin = new Msgwin(false, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع :1 شماره دفتر :");
                    msgwin.ShowDialog();
                    //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع :1 شماره دفتر :");
                    //RST2.Close();
                    //RST2.Open("DAFT_ASN", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                    // RST2.AddNew();
                    RST2.FirstOrDefault().FIRSTNUM = 1;
                    RST2.FirstOrDefault().BOOKNUM = 1;
                    // RST2.update();
                    rdn = 1L;
                    dfn = 1L;
                }
                var _id = ((THE_WIN as HESABDARI.PGET_HED).ID.Text);
                var rstradif = dbms.DoGetDataSQL<PGET_LST>("SELECT MAX(RADIF) AS Expr1 FROM dbo.PGET_LST WHERE (ID = " + _id + ")").ToList();
                if (rstradif.Count == 1 && !IsNull(rstradif.FirstOrDefault()))
                {
                    //ERROR ?
                    //RRDF = Convert.ToInt32(rst.FirstOrDefault()) + 1;
                }
                else
                {
                    RRDF = 1;
                }
                var _RST2 = dbms.DoGetDataSQL<PGET_LST>("SELECT * FROM PGET_LST").ToList();
                dt = Convert.ToInt64(this.DATE_S.Text.ToRawTarikh());
                SALT = Convert.ToInt64(CL_HESABDARI.UYear(this.DATE_S.Text.ToRawTarikh()));
                DTT = Convert.ToInt64(CL_HESABDARI.UDay(this.DATE_S.Text.ToRawTarikh()));
                var loopTo = Convert.ToInt32(this.NUM.Text) - 1;
                for (i = 0; i <= loopTo; i++)
                {

                    var rst3 = dbms.DoGetDataSQL<PAY_GETD>("SELECT Max(PAY_GETD.RADIF) AS MaxOfRADIF  FROM PAY_GETD WHERE ANBAR = " + dfn).ToList();
                    if (rst3.Count == 0 || IsNull(rst3.FirstOrDefault()))
                    {
                        rst.FirstOrDefault().RADIF = 1;
                        rst.FirstOrDefault().ANBAR = dfn;
                    }
                    else
                    {
                        //rst.FirstOrDefault().RADIF = Convert.ToInt32(rst3.FirstOrDefault()) + 1;
                        rst.FirstOrDefault().ANBAR = dfn;
                    }
                    if (i > 0)
                    {
                        MMT = Convert.ToInt64(CL_HESABDARI.UMonth(dt.ToString())) + Convert.ToInt64(GAP.Text);
                        if (MMT > 12L)
                        {
                            SALT = Convert.ToInt64(CL_HESABDARI.UYear(dt.ToString())) + 1;
                            MMT = MMT - 12L;
                            if (CL_HESABDARI.UDay(this.DATE_S.Text.ToRawTarikh()) == "31")
                            {
                                DTT = 31L;
                            }
                        }
                        if (MMT > 6L && Convert.ToInt64(CL_HESABDARI.UDay(dt.ToString())) > 30)
                        {
                            DTT = 30L;
                        }
                        if (MMT == 12L && Convert.ToInt64(CL_HESABDARI.UDay(dt.ToString())) > 29)
                        {
                            DTT = 29;
                        }
                        dt = SALT * 10000L + MMT * 100L + DTT;
                    }
                    rst.FirstOrDefault().N_SERI = Convert.ToDouble(this.N_SERI.Text) + i;
                    rst.FirstOrDefault().BANK = Convert.ToInt32(this.BANK.SelectedValue);
                    rst.FirstOrDefault().DATE_S = dt;
                    rst.FirstOrDefault().SHOBEH = this.SHOBEH.SelectedValue.ToString();
                    rst.FirstOrDefault().DATE = Convert.ToInt64(((THE_WIN as HESABDARI.PGET_HED).DATE.Text.ToRawTarikh()));
                    rst.FirstOrDefault().NAME_TAH = this.NAME_TAH.Text;
                    rst.FirstOrDefault().N_HESAB = this.N_HESAB.Text;
                    rst.FirstOrDefault().MABL = Convert.ToDouble(this.MABL.Text);
                    rst.FirstOrDefault().LIST_NO = Convert.ToInt32(this.LIST_NO.SelectedValue);
                    rst.FirstOrDefault().CUST_NO = this.CUST_NO.SelectedValue.ToString();
                    rst.FirstOrDefault().VAZ = Convert.ToDouble(this.VAZ.SelectedValue);
                    rst.FirstOrDefault().SANDUGH = Convert.ToInt32(this.SANDUGH.SelectedValue);
                    //INSERT INTO PAY_GETD
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD(N_SERI,                                              BANK,                       DATE_S,                       DATE,                          SHOBEH,                       MABL,                          NAME_TAH,                          N_HESAB,ANBAR,RADIF,                          CUST_NO,                       VAZ,                       LIST_NO,                       SANDUGH)
			                                               VALUES({rst.FirstOrDefault().N_SERI},{rst.FirstOrDefault().BANK},{rst.FirstOrDefault().DATE_S},{rst.FirstOrDefault().DATE},N'{rst.FirstOrDefault().SHOBEH}',{rst.FirstOrDefault().MABL},N'{rst.FirstOrDefault().NAME_TAH}',N'{rst.FirstOrDefault().N_HESAB}',NULL, NULL, N'{rst.FirstOrDefault().CUST_NO}',{rst.FirstOrDefault().VAZ},{rst.FirstOrDefault().LIST_NO},{rst.FirstOrDefault().SANDUGH})");


                    _RST2.FirstOrDefault().ID = Convert.ToInt32(((THE_WIN as HESABDARI.PGET_HED).ID.Text));
                    _RST2.FirstOrDefault().DATE = Convert.ToInt64(((THE_WIN as HESABDARI.PGET_HED).DATE.Text.ToRawTarikh()));
                    _RST2.FirstOrDefault().RADIF = RRDF;
                    _RST2.FirstOrDefault().NO_AM = 1;
                    if (Convert.ToInt32(this.KIND.SelectedValue) == 1)
                    {
                        _RST2.FirstOrDefault().NAHVA = 2;
                    }
                    else
                    {
                        _RST2.FirstOrDefault().NAHVA = 6;
                    }
                    _RST2.FirstOrDefault().FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(this.CUST_NO2.SelectedValue.ToString()));
                    _RST2.FirstOrDefault().FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(this.CUST_NO2.SelectedValue.ToString()));
                    _RST2.FirstOrDefault().FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(this.CUST_NO2.SelectedValue.ToString()));
                    _RST2.FirstOrDefault().FHES = this.CUST_NO2.SelectedValue.ToString();
                    _RST2.FirstOrDefault().THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                    _RST2.FirstOrDefault().THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                    _RST2.FirstOrDefault().THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                    _RST2.FirstOrDefault().THES = Baseknow.ADA;
                    _RST2.FirstOrDefault().SHARH = Strings.Left("چك " + (this.N_SERI.Text + i) + "بانك " + CL_HESABDARI.GETBANK(Convert.ToDouble(this.BANK.SelectedValue)) + " " + this.SHOBEH.SelectedValue + " مورخ " + Strings.Format(dt, "####/##/##") + "-" + NAME_TAH.Text, 255);
                    _RST2.FirstOrDefault().MABL = Convert.ToDouble(this.MABL.Text);
                    _RST2.FirstOrDefault().N_SERI = Convert.ToInt32(this.N_SERI.Text + i);
                    _RST2.FirstOrDefault().BANK = Convert.ToInt32(this.BANK.SelectedValue);
                    //INSERT INTO PGET_LST
                    //ERROR
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PGET_LST(ID,                                                  DATE,                         RADIF,                         NO_AM,                         NAHVA,                         FHES_K,                         FHES_M,                         FHES_T,                         THES_K,                         THES_M,                         THES_T,                            SHARH,                         MABL,                         N_SERI,                         BANK,                            FHES,                            THES, ARZD)
				                                           VALUES({_RST2.FirstOrDefault().ID},{_RST2.FirstOrDefault().DATE},{_RST2.FirstOrDefault().RADIF},{_RST2.FirstOrDefault().NO_AM},{_RST2.FirstOrDefault().NAHVA},{_RST2.FirstOrDefault().FHES_K},{_RST2.FirstOrDefault().FHES_M},{_RST2.FirstOrDefault().FHES_T},{_RST2.FirstOrDefault().THES_K},{_RST2.FirstOrDefault().THES_M},{_RST2.FirstOrDefault().THES_T},N'{_RST2.FirstOrDefault().SHARH}',{_RST2.FirstOrDefault().MABL},{_RST2.FirstOrDefault().N_SERI},{_RST2.FirstOrDefault().BANK},N'{_RST2.FirstOrDefault().FHES}',N'{_RST2.FirstOrDefault().THES}',1)");

                    //rst3.Close();
                    RRDF = RRDF + 1;
                }
            }
            (THE_WIN as HESABDARI.PGET_HED).ReGetData();
            var thewin = (THE_WIN as HESABDARI.PGET_HED);
            universControl.PopNotifyShow("ذخیره با موفقیت انجام شد.", thewin.Pop1, thewin.Pop1Text1, thewin.Pop_Border1, "#FF1AAA2C");
            this.Close();
            //DoCmd.Close(acForm, this.NAME);
            //System.Windows.Forms["PGET_HED"]["PGET_LST_SUB"].Form.Requery();
            //System.Windows.Forms("PGET_HED").LETSANAD = true;
            //Environment.Exit(0);
            #endregion
        }

        private void DATE_S_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
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

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_Confirm.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CUST_NO.Focus();
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is System.Windows.Controls.TextBox)) return; }

            if (CUST_NO != null)
            {
                TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
                if (CUTSNO_TEX != null && CUST_NO.SelectedValue is not null)
                {
                    if ((CUST_NO.SelectedItem as CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("CREATE_CHEKDP", I_AM_CREATE_CHEKDP);//Search Plusy Form Specialy for Customers
                    CMBSearch.ShowDialog();

                    if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                    {
                        CUST_NO.SelectedValue = HESAB_FROM_SEARCH.FULL_HES;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                    HESAB_FROM_SEARCH.DoClear();
                }
                else
                {
                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT    hes FROM dbo.CUST_HESAB WHERE     (hes = N'" + CUTSNO_TEX.Text + "')").FirstOrDefault();
                    if (!string.IsNullOrEmpty(data.hes))
                    {
                        CUST_NO.SelectedValue = data.hes;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                }

                this.CUST_NO2.ItemsSource = this.CUST_NO.ItemsSource;
                this.CUST_NO2.SelectedValuePath = "hes";
                this.CUST_NO2.DisplayMemberPath = "hes";

                this.NAME_TAH.Text = CL_HESABDARI.GETHESNAME(this.CUST_NO.SelectedValue.ToString());
            }
        }
    }
}
