using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Data.Expressions.Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.Checkha.BAKCHEK;
using TextBox = System.Windows.Controls.TextBox;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for CREATE_CHEKPDP.xaml
    /// </summary>
    public partial class CREATE_CHEKPDP : Window
    {
        UniversControl universControl = new UniversControl();
        public int INDEX_DG { get; set; }
        public Visual THE_WIN { get; set; }
        public string BEFOREDATEN { get; private set; }
        public Visual I_AM_CHPDP { get; set; }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public CREATE_CHEKPDP(Visual _thewin, int _current_index = -1)
        {
            INDEX_DG = _current_index;
            THE_WIN = _thewin;
            InitializeComponent();
        }
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
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
        private int RRDF;
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

        public class Query1
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }
            public int? N_KOL { get; set; }
            public double? BANKHA { get; set; }
        }
        public class Query2
        {
            public string? hes { get; set; }
            public string? nam { get; set; }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_CHPDP = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
            #region Form_Load
            Fill_ComboBoxes();
            #endregion
        }

        private void Fill_ComboBoxes()
        {
            HES.ItemsSource = dbms.DoGetDataSQL<Query1>("SELECT CAST(N_KOL AS nvarchar) + N'-' + CAST(NUMBER AS nvarchar) + N'-' + CAST(TNUMBER AS nvarchar) AS hes, NAME, TNUMBER AS Expr1 FROM TDETA_HES WHERE ( ((TDETA_HES.N_KOL) =" + Baseknow.BANKHA + "))").ToList();
            HES.SelectedValuePath = "hes";
            HES.DisplayMemberPath = "NAME";

            CUST_NO.ItemsSource = dbms.DoGetDataSQL<Query2>("SELECT hes, NAME + N' ' + hes AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            CUST_NO.SelectedValuePath = "hes";
            CUST_NO.DisplayMemberPath = "nam";

            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.SelectedValuePath = "hes";
            CUST_NO2.DisplayMemberPath = "hes";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES;").ToList();
            BANK.SelectedValuePath = "CODE";
            BANK.DisplayMemberPath = "NAMES";

            SHOBEH.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.SHOBEH FROM PAY_GETD GROUP BY PAY_GETD.SHOBEH ORDER BY PAY_GETD.SHOBEH;").ToList();
            SHOBEH.SelectedValuePath = "SHOBEH";
            SHOBEH.DisplayMemberPath = "SHOBEH";

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

         private void CUST_NO2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CUST_NO2.IsEditable) { if (!(e.OriginalSource is System.Windows.Controls.TextBox)) return; }
            if (CUST_NO.Text is null || CUST_NO.SelectedValue == null)
            {
                return;
            }
            #region AfterUpdate
            //CUST_NO.ItemsSource = CUST_NO2.ItemsSource;
            //CUST_NO.SelectedValue = "hes";
            //CUST_NO.DisplayMemberPath = "nam";

            if (!IsNull(CUST_NO2.SelectedValue))
            {
                NAME_TAH.Text = CL_HESABDARI.GETHESNAME(CUST_NO2.SelectedValue.ToString());
            }
            #endregion

            //#region NotInList
            //string SPACELESSV;
            //if ((bool)Baseknow.BARCOD  && Baseknow.UGRP != "1")
            //{
            //    if (CUST_NO.SelectedValue == "-" | CUST_NO.SelectedValue == "+")
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
            #region Click
            int i;
            long dt;
            long DTT;
            long MMT;
            long SALT;
            long dfn;
            long rdn;
            if (!IsNull(this.N_SERI.Text) && !IsNull(this.BANK.SelectedValue) && !IsNull(this.DATE_S.Text.ToRawTarikh()) && !IsNull(HES.SelectedValue))
            {
                var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP").ToList();
                var rstradif = dbms.DoGetDataSQL<PGET_LST>("SELECT MAX(RADIF) AS Expr1 FROM dbo.PGET_LST WHERE (ID = " + ((THE_WIN as HESABDARI.PGET_HED).ID.Text.ToRawTarikh()) + ")").ToList();
                if (rstradif.Count == 1 && !IsNull(rstradif.FirstOrDefault()))
                {
                    //RRDF = Convert.ToInt32(rstradif.FirstOrDefault().RADIF) + 1;
                }
                else
                {
                    RRDF = 1;
                }
                var RST2 = dbms.DoGetDataSQL<PGET_LST>("SELECT * FROM PGET_LST").ToList();
                dt = Convert.ToInt64(this.DATE_S.Text.ToRawTarikh());
                SALT = Convert.ToInt64(CL_HESABDARI.UYear(this.DATE_S.Text.ToRawTarikh()));
                DTT = Convert.ToInt64(CL_HESABDARI.UDay(this.DATE_S.Text.ToRawTarikh()));
                var loopTo = Convert.ToInt32(this.NUM.Text) - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    if (i > 0)
                    {
                        MMT = Convert.ToInt64(CL_HESABDARI.UMonth(dt.ToString())) + Convert.ToInt64(this.GAP.Text);
                        if (MMT > 12L)
                        {
                            SALT = Convert.ToInt64(CL_HESABDARI.UYear(dt.ToString())) + 1;
                            MMT = MMT - 12L;
                            if (Convert.ToInt32(CL_HESABDARI.UDay(this.DATE_S.Text.ToRawTarikh())) == 31)
                            {
                                DTT = 31L;
                            }
                        }
                        if (MMT > 6L && Convert.ToInt32(CL_HESABDARI.UDay(dt.ToString())) > 30)
                        {
                            DTT = 30L;
                        }
                        if (MMT == 12L && Convert.ToInt32(CL_HESABDARI.UDay(dt.ToString())) > 29)
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
                    //rst.FirstOrDefault().CUST_NO = Convert.ToDouble(this.CUST_NO.SelectedValue.ToString());
                    rst.FirstOrDefault().N_KOL = Convert.ToInt32(CL_HESABDARI.GETKOL(this.HES.SelectedValue.ToString()));
                    rst.FirstOrDefault().N_MOIN = Convert.ToInt32(CL_HESABDARI.GETMOIN(this.HES.SelectedValue.ToString()));
                    rst.FirstOrDefault().N_TAF = Convert.ToInt32(CL_HESABDARI.GETTAF(this.HES.SelectedValue.ToString()));
                    rst.FirstOrDefault().HES1 = this.HES.SelectedValue.ToString();
                    //INSERT INTO PAY_GETP
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETP(N_SERI,                                              BANK,                       DATE_S,                       DATE,                          SHOBEH,                       MABL,                          NAME_TAH,                          N_HESAB,                       N_KOL,                       N_MOIN,                       N_TAF,                       CUST_NO,                          HES1)
				                                           VALUES({rst.FirstOrDefault().N_SERI},{rst.FirstOrDefault().BANK},{rst.FirstOrDefault().DATE_S},{rst.FirstOrDefault().DATE},N'{rst.FirstOrDefault().SHOBEH}',{rst.FirstOrDefault().MABL},N'{rst.FirstOrDefault().NAME_TAH}',N'{rst.FirstOrDefault().N_HESAB}',{rst.FirstOrDefault().N_KOL},{rst.FirstOrDefault().N_MOIN},{rst.FirstOrDefault().N_TAF},{CUST_NO.SelectedValue.ToString()},N'{rst.FirstOrDefault().HES1}')");


                    RST2.FirstOrDefault().ID = Convert.ToInt32(((THE_WIN as HESABDARI.PGET_HED).ID.Text));
                    RST2.FirstOrDefault().DATE = Convert.ToInt64(((THE_WIN as HESABDARI.PGET_HED).DATE.Text.ToRawTarikh()));
                    RST2.FirstOrDefault().RADIF = RRDF;
                    RST2.FirstOrDefault().NO_AM = 2;
                    if (Convert.ToInt32(this.KIND.SelectedValue) == 1)
                    {
                        RST2.FirstOrDefault().NAHVA = 2;
                    }
                    else
                    {
                        RST2.FirstOrDefault().NAHVA = 6;
                    }
                    RST2.FirstOrDefault().THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(this.CUST_NO2.SelectedValue.ToString()));
                    RST2.FirstOrDefault().THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(this.CUST_NO2.SelectedValue.ToString()));
                    RST2.FirstOrDefault().THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(this.CUST_NO2.SelectedValue.ToString()));
                    RST2.FirstOrDefault().THES_T2 = CL_HESABDARI.GETTAF2(CUST_NO2.SelectedValue.ToString()) == -1 ? null : Convert.ToInt32(CL_HESABDARI.GETTAF2(CUST_NO2.SelectedValue.ToString()));
                    var _THES_T2 = (RST2.FirstOrDefault().THES_T2 is null ? "NULL" : RST2.FirstOrDefault().THES_T2.ToString());
                    RST2.FirstOrDefault().THES = this.CUST_NO2.SelectedValue.ToString();
                    RST2.FirstOrDefault().FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APA));
                    RST2.FirstOrDefault().FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APA));
                    RST2.FirstOrDefault().FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APA));
                    RST2.FirstOrDefault().FHES = Baseknow.APA;
                    RST2.FirstOrDefault().SHARH = Strings.Left("چك" + (this.N_SERI.Text + i) + "بانك" + CL_HESABDARI.GETBANK(Convert.ToDouble(this.BANK.SelectedValue)) + " " + this.SHOBEH.SelectedValue + " مورخ " + Strings.Format(dt, "####/##/##") + "-" + NAME_TAH.Text, 255);
                    RST2.FirstOrDefault().MABL = Convert.ToDouble(this.MABL.Text);
                    RST2.FirstOrDefault().N_SERI = Convert.ToInt32(this.N_SERI.Text) + i;
                    RST2.FirstOrDefault().BANK = Convert.ToInt32(this.BANK.SelectedValue);
                    //INSERT INTO PGET_LST
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PGET_LST(ID,                                                DATE,                        RADIF,                        NO_AM,                        NAHVA,                        FHES_K,                        FHES_M,                        FHES_T,                        THES_K,                        THES_M,                        THES_T,                           SHARH,                        MABL,                        N_SERI,                        BANK,                           FHES,                           THES, ARZD, THES_T2)
				                                           VALUES({RST2.FirstOrDefault().ID},{RST2.FirstOrDefault().DATE},{RST2.FirstOrDefault().RADIF},{RST2.FirstOrDefault().NO_AM},{RST2.FirstOrDefault().NAHVA},{RST2.FirstOrDefault().FHES_K},{RST2.FirstOrDefault().FHES_M},{RST2.FirstOrDefault().FHES_T},{RST2.FirstOrDefault().THES_K},{RST2.FirstOrDefault().THES_M},{RST2.FirstOrDefault().THES_T},N'{RST2.FirstOrDefault().SHARH}',{RST2.FirstOrDefault().MABL},{RST2.FirstOrDefault().N_SERI},{RST2.FirstOrDefault().BANK},N'{RST2.FirstOrDefault().FHES}',N'{RST2.FirstOrDefault().THES}',1,{_THES_T2})");
                    RRDF = RRDF + 1;
                }
            }
            else
            {
                return;
            }
            (THE_WIN as HESABDARI.PGET_HED).ReGetData();
            var thewin = (THE_WIN as HESABDARI.PGET_HED);
            universControl.PopNotifyShow("ذخیره با موفقیت انجام شد.", thewin.Pop1, thewin.Pop1Text1, thewin.Pop_Border1, "#FF1AAA2C");
            this.Close();
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CUST_NO.Focus();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_Confirm.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is System.Windows.Controls.TextBox)) return; }

            if (CUST_NO != null)
            {
                TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
                if (CUTSNO_TEX is null)
                {
                    return;
                }
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
                    ComboSearch CMBSearch = new ComboSearch("CREATE_CHEKPDP", I_AM_CHPDP);//Search Plusy Form Specialy for Customers
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
                    if (data is not null && !string.IsNullOrEmpty(data.hes))
                    {
                        CUST_NO.SelectedValue = data.hes;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                }

                if (CUST_NO.SelectedValue != null)
                {
                    NAME_TAH.Text = CL_HESABDARI.GETHESNAME(CUST_NO.SelectedValue.ToString());
                }
            }

        }
    }
}
