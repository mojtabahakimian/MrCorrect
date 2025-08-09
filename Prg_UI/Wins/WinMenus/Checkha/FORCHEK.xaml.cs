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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for FORCHEK.xaml
    /// </summary>
    public partial class FORCHEK : Window
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
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private bool can;
        public string BEFOREDATEN { get; private set; }
        UniversControl universControl = new UniversControl();
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
        public Visual I_AM_FORCHECK { get; set; }
        public string ServerFilter { get; set; }
        public int INDEX_DG { get; set; }
        public FORCHEK(Visual thewin, string _filter, int _current_index = -1)
        {
            THE_WIN = thewin;
            ServerFilter = _filter;
            InitializeComponent();
            INDEX_DG = _current_index;
        }
        public class Query1T
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }
        public class Query2T
        {
            public int? BANK { get; set; }
            public string? NAMES { get; set; }
        }
        public string SE_N_SERI { get; set; }
        public string SE_DATE_S { get; set; }
        public string SE_SANDUGH { get; set; }
        public string SE_SHOBEH { get; set; }
        public string SE_DATE { get; set; }
        public string SE_NAME_TAH { get; set; }
        public string SE_N_HESAB { get; set; }
        public string SE_MABL { get; set; }
        public string SE_BANK { get; set; }

        private void Fill_ComboBoxes()
        {
            BANK.ItemsSource = dbms.DoGetDataSQL<Query2T>("SELECT PAY_GETD.BANK, TCOD_BANKS.NAMES FROM PAY_GETD INNER JOIN TCOD_BANKS ON PAY_GETD.BANK = TCOD_BANKS.CODE").ToList();
            BANK.SelectedValuePath = "BANK";
            BANK.DisplayMemberPath = "NAMES";

            SANDUGH.ItemsSource = dbms.DoGetDataSQL<Query1T>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = 113) AND (NUMBER = 1)");
            SANDUGH.SelectedValuePath = "TNUMBER";
            SANDUGH.DisplayMemberPath = "NAME";

            N_SERI.ItemsSource = dbms.DoGetDataSQL<PAY_GETD>("SELECT N_SERI, N_S, N_KOL, N_KOL2 FROM PAY_GETD WHERE (N_S IS NULL) AND (N_KOL IS NULL) AND (N_KOL2 IS NULL)").ToList();
            N_SERI.SelectedValuePath = "N_SERI";
            N_SERI.DisplayMemberPath = "N_SERI";

            if (N_SERI.ItemsSource == null)
            {
                N_SERI.ItemsSource = new List<PAY_GETD>();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_FORCHECK = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle); // The ID of this window to Pass to other forms

            CL_LMethods.SetTabIndexes(
                N_SERI,
                BANK,
                _SaveExit
                );

            Fill_ComboBoxes();

            //ON_Open
            List<PAY_GETD> rst = null;
            if (!string.IsNullOrEmpty(ServerFilter))
            {
                rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {ServerFilter} ").ToList();
            }
            if (rst?.Count == 0 || rst?.Count == null)
            {
                this.N_SERI.IsReadOnly = false;
            }
            else
            {
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.N_SERI.SelectedValue = rst.FirstOrDefault().N_SERI;
                N_SERI.Items.Refresh();

                if (N_SERI.SelectedValue == null)
                {
                    var Nseri = rst.FirstOrDefault().N_SERI;

                    if (!((List<PAY_GETD>)N_SERI.ItemsSource).Any(item => item?.N_SERI == Nseri))
                    {
                        ((List<PAY_GETD>)N_SERI.ItemsSource).Add(new PAY_GETD { N_SERI = Nseri });
                    }
                    N_SERI.SelectedValue = Nseri; //مشتری
                    N_SERI.Items.Refresh();
                }

                this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                this.SANDUGH.SelectedValue = rst.FirstOrDefault().SANDUGH;
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                this.N_SERI.IsReadOnly = true;
            }

            N_SERI.Focus();
        }

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }
        private void N_SERI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (N_SERI.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            //After_Update

            if (N_SERI.SelectedValue == null)
            {
                e.Handled = true;
                new Msgwin(false, "شماره سریال نمیتواند خالی باشد").Show();
                return;
            }

            if (N_SERI.SelectedValue?.ToString() != N_SERI.Text.Trim())
            {
                FOR_CHK_SERCH fOR_CHK_SERCH = new FOR_CHK_SERCH("1", "N_SERI = " + N_SERI.SelectedValue.ToStringNullSafe(), I_AM_FORCHECK);
                fOR_CHK_SERCH.ShowDialog();
            }

        }

        private void BANK_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (BANK.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (DATE_S.Text.ToRawTarikh() is null && (MABL.Text == "0" || MABL == null))
            {
                return;
            }
            if (BANK.SelectedValue == null)
            {
                new Msgwin(false, "بانک نمیتواند خالی باشد").Show();
                return;
            }
            if (N_SERI.SelectedValue == null)
            {
                return;
            }

            //NotInList
            var NewData = ((TextBox)BANK.Template.FindName("PART_EditableTextBox", BANK)).Text; //متن کمبوباکس رو به طور واقعی میگیریم
            if (int.TryParse(NewData.ToString(), out _)) //if is Number آیا عدد هست متن وارد شده
            {
                var _itm = BANK.ItemsSource as List<Query2T>; // برای راحتی و سادگی کد آیتم های کموبباکس رو بریز داخل یه لیست
                var _SelectVal = _itm?.FirstOrDefault(item => item.BANK.Equals(NewData)).BANK; // با لینک چک کن که آیا کدی با این کد وارد شده وجود دارد ؟

                BANK.SelectedValue = _SelectVal; //بذارش توی کمبوباکس
            }

            //After_Update
            var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue).ToList();
            if (rst.Count == 0)
            {
            }
            else
            {
                this.N_SERI.SelectedValue = rst.FirstOrDefault().N_SERI;
                this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.SANDUGH.SelectedValue = rst.FirstOrDefault().SANDUGH;
            }
        }

        private void DATE_S_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh()).ToList();
            if (rst.Count == 0)
            {
            }
            else
            {
                this.N_SERI.SelectedValue = rst.FirstOrDefault().N_SERI;
                this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.SANDUGH.SelectedValue = rst.FirstOrDefault().SANDUGH;
            }
        }

        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            can = false;
            if (IsNull(this.N_SERI.SelectedValue) || IsNull(this.BANK.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "اطلاعات وارد نشده است و قابل ذخيره شدن نيست");
                msgwin.Show();
                return;
            }

            (THE_WIN as PGET_HED).CmdSaveRecord((THE_WIN as PGET_HED).CURRENT_ITMES_ROW);
            SE_N_SERI = N_SERI.SelectedValue.ToString();
            SE_DATE_S = DATE_S.Text.ToRawTarikh();
            SE_SANDUGH = SANDUGH.SelectedValue.ToString();
            SE_SHOBEH = SHOBEH.Text;
            SE_DATE = DATE.Text;
            SE_NAME_TAH = NAME_TAH.Text;
            SE_N_HESAB = N_HESAB.Text;
            SE_MABL = MABL.Text;
            SE_BANK = BANK.SelectedValue.ToString();

            var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;

            dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD
                SET N_SERI = {SE_N_SERI} , DATE_S = {SE_DATE_S} , SANDUGH = {SE_SANDUGH} , SHOBEH = N'{SE_SHOBEH}' , DATE = {SE_DATE} , NAME_TAH = N'{SE_NAME_TAH}' , N_HESAB = N'{SE_N_HESAB}' , MABL = {SE_MABL} , BANK = {SE_BANK}
                WHERE N_SERI = {SE_N_SERI} AND DATE_S = {SE_DATE_S} AND SANDUGH = {SE_SANDUGH} AND SHOBEH = N'{SE_SHOBEH}' AND DATE = {SE_DATE} AND NAME_TAH = N'{_NAME_TAH_}' AND N_HESAB = N'{SE_N_HESAB}' AND MABL = {SE_MABL} AND BANK = {SE_BANK}");

            if (can)
            {
            }
            else
            {
                //ON_Close
                var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh()).ToList();
                if (rst.Count > 0)
                {
                    rst.FirstOrDefault().N_KOL = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_K;
                    rst.FirstOrDefault().N_MOIN = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_M;
                    rst.FirstOrDefault().N_TAF = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_T;
                    rst.FirstOrDefault().HES1 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES;
                    rst.FirstOrDefault().VAZ = 4;
                    rst.FirstOrDefault().SANDUGH = Convert.ToInt32(this.SANDUGH.SelectedValue);

                    string _WHERE_ = " WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();
                    dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET N_MOIN = {rst.FirstOrDefault().N_MOIN}, VAZ = {rst.FirstOrDefault().VAZ}, SANDUGH = {rst.FirstOrDefault().SANDUGH} , N_KOL = {rst.FirstOrDefault().N_KOL} , N_TAF = {rst.FirstOrDefault().N_TAF} , HES1 = N'{rst.FirstOrDefault().HES1}' {_WHERE_}");
                }
                if (rst.FirstOrDefault().KIND == 0)
                {
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES = Baseknow.ADV;
                }
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).MABL = Convert.ToDouble(this.MABL.Text);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).N_SERI = Convert.ToDouble(this.N_SERI.SelectedValue);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).BANK = Convert.ToInt32(this.BANK.SelectedValue);

                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).SHARH = Strings.Right("چك " + N_SERI.SelectedValue + "بانك " + CL_HESABDARI.GETBANK(Convert.ToDouble(BANK.SelectedValue)) + " " + SHOBEH.Text + " مورخ " + Strings.Format(Convert.ToDouble(DATE_S.Text.ToRawTarikh()), "####/##/##"), 255);
                CL_HESABDARI.GETDLOG(4, this.N_SERI.Text.ToString(), Convert.ToInt32(this.BANK.SelectedValue), Convert.ToInt64(DATE_S.Text.ToRawTarikh()), Convert.ToInt32(this.SANDUGH.SelectedValue));
            }

            //Finally Save and Sand as Ensure Oprate
            _ = (THE_WIN as PGET_HED).CmdSaveRecord((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST);
            (THE_WIN as PGET_HED).SANAD();

            this.Close();
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            #region Click
            can = true;
            (THE_WIN as PGET_HED).FORCHEK_EXIT_BTN = true;
            this.Close();
            #endregion
        }

        private void DATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_SaveExit.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
    }
}
