using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for BAKCHEKP.xaml
    /// </summary>
    public partial class BAKCHEKP : Window
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
        public string ServerFilter { get; set; }
        public bool can { get; private set; }
        public class ComboBoxItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public Visual THE_WIN { get; set; }
        private string N_SERI_ON { get; set; }
        private string BANK_ON { get; set; }
        private string DATE_S_ON { get; set; }
        public int INDEX_DG { get; set; }
        private string MABL_ON { get; set; }

        public class Query1T
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public BAKCHEKP(Visual thewin, string _severfilter, int _current_index = -1)
        {
            THE_WIN = thewin;
            ServerFilter = _severfilter;
            INDEX_DG = _current_index;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //ON_Open
            List<PAY_GETP> rst = null;
            if (!string.IsNullOrEmpty(ServerFilter))
            {
                rst = dbms.DoGetDataSQL<PAY_GETP>($"SELECT * FROM PAY_GETP WHERE {ServerFilter}").ToList();
            }
            if (rst?.Count == 0 || rst?.Count == null)
            {
                Fill_ComboBoxes();
            }
            else
            {
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();

                var thevalue = rst.FirstOrDefault().N_SERI;
                N_SERI.ItemsSource = new List<PAY_GETP>();

                if (!((List<PAY_GETP>)N_SERI.ItemsSource).Any(item => item?.N_SERI == thevalue))
                {
                    ((List<PAY_GETP>)N_SERI.ItemsSource).Add(new PAY_GETP { N_SERI = thevalue });
                }
                N_SERI.SelectedValuePath = "N_SERI";
                N_SERI.DisplayMemberPath = "N_SERI";
                N_SERI.SelectedValue = null;
                N_SERI.SelectedValue = thevalue.ToString();
                N_SERI.Items.Refresh();

                this.DATE_S.SelectedValue = rst.FirstOrDefault().DATE_S.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH.ToString();
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH.ToString();
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB?.ToString();
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.KOL.Text = rst.FirstOrDefault().N_KOL.ToString();
                this.MOIN.Text = rst.FirstOrDefault().N_MOIN.ToString();
                this.TAF.Text = rst.FirstOrDefault().N_TAF.ToString();
                this.VAZ.SelectedValue = rst.FirstOrDefault().VAZ.ToString();

                this.BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS INNER JOIN PAY_GETP ON TCOD_BANKS.CODE = PAY_GETP.BANK WHERE (PAY_GETP.N_SERI = " + this.N_SERI.SelectedValue + ") ORDER BY TCOD_BANKS.NAMES").ToList();
                this.BANK.SelectedValuePath = "CODE";
                this.BANK.DisplayMemberPath = "NAMES";
                this.BANK.SelectedIndex = 0;

                this.DATE_S.ItemsSource = dbms.DoGetDataSQL<PAY_GETP>("SELECT DATE_S , BANK,N_SERI  FROM PAY_GETP WHERE (N_SERI = " + this.N_SERI.SelectedValue + ") AND (BANK = " + this.BANK.SelectedValue + ")").ToList();
                this.DATE_S.SelectedValuePath = "DATE_S";
                this.DATE_S.DisplayMemberPath = "DATE_S";
                this.DATE_S.SelectedIndex = 0;
            }
            N_SERI.Focus();
        }

        private void Fill_ComboBoxes()
        {
            N_SERI.ItemsSource = dbms.DoGetDataSQL<PAY_GETP>("SELECT N_SERI, N_S, N_KOL2, N_KOL3 FROM PAY_GETP WHERE (N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (N_S IS NULL OR N_S = 0)").ToList();
            N_SERI.SelectedValuePath = "N_SERI";
            N_SERI.DisplayMemberPath = "N_SERI";

            List<ComboBoxItem> comboBoxItems = new List<ComboBoxItem>
            {
                new ComboBoxItem { Id = 1, Name = "نزد شخص" },
                new ComboBoxItem { Id = 2, Name = "عودت شده" }
            };

            VAZ.ItemsSource = comboBoxItems.ToList();
            VAZ.SelectedValuePath = "Id";
            VAZ.DisplayMemberPath = "Name";
        }

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;         

            //ON_Close
            if (can)
            {

            }
            else
            {
                var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE N_SERI = " + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.SelectedValue).ToList();
                if (rst.Count == 0)
                {
                }
                else
                {
                    string _where = " WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.SelectedValue;
                    rst.FirstOrDefault().N_KOL2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_K;//Forms["PGET_HED"]["PGET_LST_SUB"].Form["THES_K"];
                    rst.FirstOrDefault().N_MOIN2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_M;//Forms["PGET_HED"]["PGET_LST_SUB"].Form["THES_M"];
                    rst.FirstOrDefault().N_TAF2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_T;///Forms["PGET_HED"]["PGET_LST_SUB"].Form["THES_T"];
                    rst.FirstOrDefault().VAZ = Convert.ToDouble(this.VAZ.SelectedValue);
                    dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL2 = {rst.FirstOrDefault().N_KOL2},
                                                N_MOIN2 = {rst.FirstOrDefault().N_MOIN2},
                                                N_TAF2 =   {rst.FirstOrDefault().N_TAF2} ,
                                                VAZ = {rst.FirstOrDefault().VAZ} {_where} ");
                }
                if (rst.FirstOrDefault().KIND == 0)
                {
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES = Baseknow.APV;
                }
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).MABL = Convert.ToDouble(this.MABL.Text);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).N_SERI = Convert.ToDouble(this.N_SERI.Text);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).BANK = Convert.ToInt32(this.BANK.SelectedValue);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).SHARH = Strings.Right(" برگشت چك پرداختي " + N_SERI.SelectedValue + "بانك" + CL_HESABDARI.GETBANK(Convert.ToDouble(BANK.SelectedValue)) + " " + SHOBEH.Text + " مورخ " + Strings.Format(Convert.ToDouble(DATE_S.Text.ToRawTarikh()), "####/##/##"), 255);
            }
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

        private void N_SERI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (N_SERI.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            //After_Update

            TextBox SERIAL_TEX = (TextBox)N_SERI.Template.FindName("PART_EditableTextBox", N_SERI);
            if (string.IsNullOrEmpty(SERIAL_TEX.Text))
            {
                SERIAL_TEX.Text = N_SERI.SelectedValue.ToStringNullSafe();
            }
            if (string.IsNullOrEmpty(SERIAL_TEX.Text))
            {
                return;
            }

            var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE N_SERI=" + SERIAL_TEX.Text).ToList();
            if (rst.Count == 0)
            {
            }
            else
            {
                this.N_SERI.SelectedValue = rst.FirstOrDefault().N_SERI;
                this.DATE_S.SelectedValue = rst.FirstOrDefault().DATE_S;
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.KOL.Text = rst.FirstOrDefault().N_KOL.ToString();
                this.MOIN.Text = rst.FirstOrDefault().N_MOIN.ToString();
                this.TAF.Text = rst.FirstOrDefault().N_TAF.ToString();
                this.VAZ.SelectedValue = rst.FirstOrDefault().VAZ;
                this.BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS INNER JOIN PAY_GETP ON TCOD_BANKS.CODE = PAY_GETP.BANK WHERE (PAY_GETP.N_SERI = " + this.N_SERI.SelectedValue + ") ORDER BY TCOD_BANKS.NAMES").ToList();
                this.BANK.SelectedValuePath = "CODE";
                this.BANK.DisplayMemberPath = "NAMES";
                this.BANK.SelectedIndex = 0;
                this.DATE_S.ItemsSource = dbms.DoGetDataSQL<PAY_GETP>("SELECT DATE_S, BANK,N_SERI  FROM PAY_GETP WHERE (N_SERI = " + this.N_SERI.SelectedValue + ") AND (BANK = " + this.BANK.SelectedValue + ")").ToList();
                this.DATE_S.SelectedValuePath = "DATE_S";
                this.DATE_S.DisplayMemberPath = "DATE_S";
                this.DATE_S.SelectedIndex = 0;
            }
        }

        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            (THE_WIN as PGET_HED).CmdSaveRecord((THE_WIN as PGET_HED).CURRENT_ITMES_ROW);

            //Click

            N_SERI_ON = N_SERI.Text;
            BANK_ON = BANK.SelectedValue.ToString();
            DATE_S_ON = DATE_S.SelectedValue.ToString();
            MABL_ON = MABL.Text;

            DateTime dt;
            dt = DateTime.Now;
            CL_HESABDARI.TR("PAY_GETD", "N_SERI = " + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue.ToString() + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh(), dt, 1);
            can = false;
            if (!IsNull(this.N_SERI.Text) && !IsNull(this.BANK.SelectedValue))
            {
                var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;

                dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD
                    SET N_SERI = {N_SERI.Text}, BANK = {BANK.SelectedValue}, DATE_S = {DATE_S.SelectedValue}, DATE = {DATE.Text}, SHOBEH = N'{SHOBEH.Text}', MABL = {MABL.Text}, NAME_TAH = N'{_NAME_TAH_}',
                    N_HESAB = N'{N_HESAB.Text}', VAZ = {(VAZ.SelectedValue is null ? "NULL" : VAZ.SelectedValue)}
                    WHERE N_SERI = {N_SERI_ON} AND BANK = {BANK_ON} AND DATE_S = {DATE_S_ON} AND MABL = {MABL_ON}");

            }
            (THE_WIN as Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED).SANAD();
            this.Close();
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            can = true;
            (THE_WIN as PGET_HED).BAKCHEKP_EXIT_BTN = true;
            this.Close();
        }

        private void BANK_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (BANK.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            //After_Update
            var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT  *  FROM PAY_GETP WHERE N_SERI=" + this.N_SERI.Text + " AND BANK = " + this.BANK.SelectedValue).ToList();
            if (rst.Count == 0)
            {
            }
            else
            {
                this.N_SERI.Text = rst.FirstOrDefault().N_SERI.ToString();
                this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                this.DATE_S.SelectedValue = rst.FirstOrDefault().DATE_S;
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.KOL.Text = rst.FirstOrDefault().N_KOL.ToString();
                this.MOIN.Text = rst.FirstOrDefault().N_MOIN.ToString();
                this.TAF.Text = rst.FirstOrDefault().N_TAF.ToString();
                this.VAZ.SelectedValue = rst.FirstOrDefault().VAZ;

                DATE_S.ItemsSource = "SELECT    DATE_S , BANK,N_SERI  FROM PAY_GETP WHERE (N_SERI = " + this.N_SERI.Text + ") AND (BANK = " + this.BANK.SelectedValue + ")";
                DATE_S.SelectedValuePath = "DATE_S";
                DATE_S.DisplayMemberPath = "DATE_S";
            }
        }

        private void DATE_S_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (DATE_S.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_SaveExit.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
    }
}
