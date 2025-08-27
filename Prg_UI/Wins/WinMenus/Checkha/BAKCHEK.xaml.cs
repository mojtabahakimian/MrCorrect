using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinMenus.HESABDARI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Themes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for BAKCHEK.xaml
    /// </summary>
    public partial class BAKCHEK : Window
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
        public class BACK_QRE_1
        {
            public double? N_SERI { get; set; }
            public double? N_S { get; set; }
            public int? N_KOL2 { get; set; }
            public int? N_KOL3 { get; set; }
        }
        public class BACK_QRE_2
        {
            public string? hes { get; set; }
            public string? Expr1 { get; set; }
        }
        public class BACK_QRE_3
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }
        public class VAZ_MODEL
        {
            public int ID { get; set; }
            public string NAME { get; set; }
        }
        public string SE_N_SERI { get; set; }
        public string SE_DATE_S { get; set; }
        public string SE_SHOBEH { get; set; }
        public string SE_DATE { get; set; }
        public string SE_NAME_TAH { get; set; }
        public string SE_N_HESAB { get; set; }
        public string SE_MABL { get; set; }
        public string SE_KOL { get; set; }
        public string SE_MOIN { get; set; }
        public string SE_TAF { get; set; }
        public string SE_BANK { get; set; }
        public string SE_HES1 { get; set; }
        public string SE_SANDUGH { get; set; }
        public string SE_VAZ { get; set; }
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool can { get; private set; }
        public Visual THE_WIN { get; set; }
        public Visual THE_WIN_2 { get; set; }
        public string ServerFilter { get; set; }
        public BAKCHEK(Visual thewin, string _severfilter, int _current_index = -1)
        {
            THE_WIN = thewin;
            INDEX_DG = _current_index;
            ServerFilter = _severfilter;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            THE_WIN_2 = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
            //ON_Open
            List<PAY_GETD> rst = null;
            if (!string.IsNullOrEmpty(ServerFilter))
            {
                rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {ServerFilter} ").ToList();
            }

            if (rst?.Count == 0 || rst?.Count == null)
            {
                this.N_SERI.IsReadOnly = false;
                this.SANDUGH.SelectedIndex = 0;
                this.SANDUGH.Refreshy();
                this.VAZ.SelectedIndex = 0;
            }
            else
            {
                this.RADIF.Text = rst.FirstOrDefault().RADIF.ToString();
                this.N_SERI.SelectedValue = rst.FirstOrDefault().N_SERI;
                this.DATE_S.Text = rst.FirstOrDefault().DATE_S.ToString();
                this.SHOBEH.Text = rst.FirstOrDefault().SHOBEH;
                this.DATE.Text = rst.FirstOrDefault().DATE.ToString();
                this.NAME_TAH.Text = rst.FirstOrDefault().NAME_TAH;
                this.N_HESAB.Text = rst.FirstOrDefault().N_HESAB;
                this.MABL.Text = rst.FirstOrDefault().MABL.ToString();
                this.KOL.Text = rst.FirstOrDefault().N_KOL.ToString();
                this.MOIN.Text = rst.FirstOrDefault().N_MOIN.ToString();
                this.TAF.Text = rst.FirstOrDefault().N_TAF.ToString();
                this.BANK.SelectedValue = rst.FirstOrDefault().BANK;
                this.HES1.SelectedValue = rst.FirstOrDefault().HES1;
                this.N_SERI.IsReadOnly = true;
                this.SANDUGH.SelectedValue = rst.FirstOrDefault().SANDUGH;
                this.VAZ.SelectedValue = rst.FirstOrDefault().VAZ;


            }
            Fill_ComboBoxes();
            N_SERI.Focus();
        }

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;

            var parentWindow = THE_WIN as PGET_HED;
            if (can || parentWindow == null || INDEX_DG < 0)
            {
                return;
            }
            var currentItem = parentWindow.PGET_LST_SUB.Items[INDEX_DG] as PGET_LST;
            if (currentItem == null)
            {
                return;
            }
            if (!HeaderIsValid(false))
            {
                return;
            }

            #region ON_Close
            double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL, HMOIN, HTAF, HTAF2, HTAF3, HTAF4, KHMAVAV;
            double KHNIM;
            double KHSAKHT;
            double KHSAY;
            double BAZAR;
            var HS = new double[8];
            if (can)
            {
                return;
            }
            else
            {
                var query = "SELECT * FROM PAY_GETD WHERE N_SERI = @N_SERI AND BANK = @BANK AND DATE_S = @DATE_S";
                var parameters = new { N_SERI = this.N_SERI.SelectedValue, BANK = this.BANK.SelectedValue, DATE_S = this.DATE_S.Text.ToRawTarikh() };
                var rst = dbms.DoGetDataSQL<PAY_GETD>(query, parameters).ToList();

                string _where = " WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();


                if (rst.Count > 0)
                {
                    rst.FirstOrDefault().N_KOL2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_K;
                    rst.FirstOrDefault().N_MOIN2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_M;
                    rst.FirstOrDefault().N_TAF2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES_T;
                    rst.FirstOrDefault().HES2 = ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).THES;
                    rst.FirstOrDefault().VAZ = Convert.ToDouble(this.VAZ.SelectedValue);
                    rst.FirstOrDefault().SANDUGH = Convert.ToInt32(this.SANDUGH.SelectedValue);
                    dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET N_KOL2 = {rst.FirstOrDefault().N_KOL2} , N_MOIN2 = {rst.FirstOrDefault().N_MOIN2} , N_TAF2 = {rst.FirstOrDefault().N_TAF2} , HES2 = N'{rst.FirstOrDefault().HES2}' ,VAZ = {rst.FirstOrDefault().VAZ} , SANDUGH = {rst.FirstOrDefault().SANDUGH} {_where}");
                }
                if (rst?.FirstOrDefault()?.KIND == 0)
                {
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADV));
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES = Baseknow.ADV;
                }

                double? _KOL_ = null;
                if (!string.IsNullOrEmpty(KOL.Text) && CL_LMethods.IsNumeric(KOL.Text))
                {
                    _KOL_ = Convert.ToDouble(KOL.Text);
                }
                if (_KOL_ != null && _KOL_ != Baseknow.BANKHA)
                {
                    Msgwin msgwin = new Msgwin(false, "اين چك قبلا واگذار گرديده است.بنابراين از حساب اين شخص كسر شده و صاحب چك بدهكار مي گردد.");
                    msgwin.ShowDialog();

                    CL_HESABDARI.GETTAF3(this.HES1.SelectedValue.ToString(), ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_K = (Convert.ToInt32(CKOL) == 0) ? null : (int)CKOL;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_M = (Convert.ToInt32(CMOIN) == 0) ? null : (int)CMOIN;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T = (Convert.ToInt32(CTAF) == 0) ? null : (int)CTAF;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T2 = (Convert.ToInt32(CTAF2) == 0) ? null : (int)CTAF2;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T3 = (Convert.ToInt32(CTAF3) == 0) ? null : (int)CTAF3;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T4 = (Convert.ToInt32(CTAF4) == 0) ? null : (int)CTAF4;
                    ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES = this.HES1.SelectedValue.ToString();
                }
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).MABL = Convert.ToDouble(this.MABL.Text);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).N_SERI = Convert.ToDouble(this.N_SERI.SelectedValue);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).BANK = Convert.ToInt32(this.BANK.SelectedValue);
                ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).SHARH = Strings.Right("چك برگشتي " + N_SERI.SelectedValue + "بانك " + CL_HESABDARI.GETBANK(Convert.ToInt32(BANK.SelectedValue)) + " " + SHOBEH.Text + " مورخ " + Strings.Format(Convert.ToDouble(DATE_S.Text.ToRawTarikh()), "####/##/##"), 255);
                var rst2 = dbms.DoGetDataSQL<PAY_GETD_LOG>("SELECT N_SERI, BANK, DATE_S, DATE_V ,DATETIM, VAZ, SANDUGH, USER_NAME FROM dbo.PAY_GETD_LOG WHERE     (N_SERI = " + this.N_SERI.SelectedValue + " ) AND (BANK = " + this.BANK.SelectedValue + ") AND (DATE_S = " + this.DATE_S.Text.ToRawTarikh() + ") AND (VAZ = 5)").ToList();
                if (rst2.Count == 0)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                        VALUES({this.N_SERI.SelectedValue}, 
                                        {this.BANK.SelectedValue}   , 
                                        {this.DATE_S.Text.ToRawTarikh()}   , 
                                        {CL_HESABDARI.FARSIDATE()}   , 
                                        GETDATE(), 
                                        5 , 
                                        {this.SANDUGH.SelectedValue}   , 
                                        N'{CL_HESABDARI.UCurrentUser().ToString()}')");

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                        VALUES({this.N_SERI.SelectedValue}, 
                                        {this.BANK.SelectedValue}   , 
                                        {this.DATE_S.Text.ToRawTarikh()}   , 
                                        {CL_HESABDARI.FARSIDATE()}   , 
                                        GETDATE(), 
                                        {(VAZ.SelectedValue is null ? "NULL" : VAZ.SelectedValue)} , 
                                        {this.SANDUGH.SelectedValue}   , 
                                        N'{CL_HESABDARI.UCurrentUser().ToString()}')");
                }
            }
            #endregion
        }

        private void Fill_ComboBoxes()
        {
            N_SERI.ItemsSource = dbms.DoGetDataSQL<BACK_QRE_1>("SELECT PAY_GETD.N_SERI, PAY_GETD.N_S, PAY_GETD.N_KOL2, PAY_GETD.N_KOL3 FROM PAY_GETD WHERE (((PAY_GETD.N_S) IS NULL OR (PAY_GETD.N_S) = 0) AND ((PAY_GETD.N_KOL2) IS NULL) AND ((PAY_GETD.N_KOL3) IS NULL));").ToList();
            N_SERI.SelectedValuePath = "N_SERI";
            N_SERI.DisplayMemberPath = "N_SERI";

            BANK.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS INNER JOIN PAY_GETD ON TCOD_BANKS.CODE = PAY_GETD.BANK ORDER BY TCOD_BANKS.NAMES").ToList();
            BANK.SelectedValuePath = "CODE";
            BANK.DisplayMemberPath = "NAMES";

            HES1.ItemsSource = dbms.DoGetDataSQL<BACK_QRE_2>("SELECT hes, hes + N' - ' + ISNULL(NAME, N'') AS Expr1 FROM CUST_HESAB").ToList();
            HES1.SelectedValuePath = "hes";
            HES1.DisplayMemberPath = "Expr1";

            SANDUGH.ItemsSource = dbms.DoGetDataSQL<BACK_QRE_3>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = " + CL_HESABDARI.GETMOIN(Baseknow.ADA) + ")").ToList();
            SANDUGH.SelectedValuePath = "TNUMBER";
            SANDUGH.DisplayMemberPath = "NAME";
            SANDUGH.SelectedIndex = 0;

            List<VAZ_MODEL> comboBoxItems = new List<VAZ_MODEL>
            {
                new VAZ_MODEL { ID = 1, NAME = "نزد صندوق" },
                new VAZ_MODEL { ID = 2, NAME = "نزد بانك" },
                new VAZ_MODEL { ID = 3, NAME = "وصول شده" },
                new VAZ_MODEL { ID = 4, NAME = "واگذار شده" },
                new VAZ_MODEL { ID = 5, NAME = "برگشت شده" },
                new VAZ_MODEL { ID = 6, NAME = "مسترد شده" }
            };
            VAZ.ItemsSource = comboBoxItems.ToList();
            VAZ.SelectedValuePath = "ID";
            VAZ.DisplayMemberPath = "NAME";
            VAZ.SelectedIndex = 0;
        }

        private void N_SERI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (N_SERI.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            #region After_Update
            if (N_SERI.SelectedValue is not null)
            {
                BACK_CHK_SERCH bACK_CHK_SERCH = new BACK_CHK_SERCH($"N_SERI = {N_SERI.SelectedValue} ", THE_WIN_2);
                bACK_CHK_SERCH.ShowDialog();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "چکی با این شماره سریال وجود ندارد ، لطفا از صحت شماره سریال چک اطمینان حاصل فرمایید");
                msgwin.ShowDialog();
            }
            #endregion
        }

        private void _Exit_Click(object sender, RoutedEventArgs e)
        {
            can = true;
            this.Close();
        }

        private void _SaveExit_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            (THE_WIN as PGET_HED).CmdSaveRecord((THE_WIN as PGET_HED).CURRENT_ITMES_ROW);
            if (!IsNull(N_SERI.SelectedValue))
            {
                SE_N_SERI = N_SERI.SelectedValue.ToStringNullSafe();
                SE_DATE_S = DATE_S.Text.ToRawTarikh();
                SE_SHOBEH = SHOBEH.Text;
                SE_DATE = DATE.Text.ToRawTarikh();
                SE_NAME_TAH = NAME_TAH.Text;
                SE_N_HESAB = N_HESAB.Text;
                SE_MABL = MABL.Text;
                SE_KOL = KOL.Text;
                SE_MOIN = MOIN.Text;
                SE_TAF = TAF.Text;
                SE_BANK = BANK.SelectedValue.ToStringNullSafe();
                SE_HES1 = HES1.SelectedValue?.ToString();
                SE_SANDUGH = SANDUGH.SelectedValue.ToStringNullSafe();
                SE_VAZ = VAZ.SelectedValue.ToStringNullSafe();
                
            }
            else
            {
                return;
            }

            //Click
            DateTime dt;
            dt = DateTime.Now;
            CL_HESABDARI.TR("PAY_GETD", "N_SERI = " + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh(), dt, 1);
            can = false;
            if (!IsNull(this.N_SERI.SelectedValue) && !IsNull(this.BANK.SelectedValue))
            {
                var _NAME_TAH_ = NAME_TAH.Text.Length > 198 ? NAME_TAH.Text.Substring(0, 198) : NAME_TAH.Text;

                dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD
                 SET N_SERI = {SE_N_SERI} , DATE_S = {SE_DATE_S} , SHOBEH = N'{SE_SHOBEH}' , DATE = {SE_DATE} , NAME_TAH = N'{_NAME_TAH_}' , N_HESAB = N'{SE_N_HESAB}' , MABL = {SE_MABL} , N_KOL = {(string.IsNullOrEmpty(SE_KOL) ? "NULL" : SE_KOL)} , N_MOIN = {(string.IsNullOrEmpty(SE_MOIN) ? "NULL" : SE_MOIN)} , N_TAF = {(string.IsNullOrEmpty(SE_TAF) ? "NULL" : SE_TAF)} , BANK = {SE_BANK} , HES1 = N'{(string.IsNullOrEmpty(SE_HES1) ? "NULL" : SE_HES1)}' , SANDUGH = {SE_SANDUGH} , VAZ = {SE_VAZ}
                 WHERE N_SERI = {SE_N_SERI} AND BANK = {SE_BANK} AND DATE_S = {SE_DATE_S}
                 ");
            }

            (THE_WIN as Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED).SANAD();

            this.Close();
        }

        private bool HeaderIsValid(bool _DisplayMsg_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (IsNull(this.N_SERI.Text) || IsNull(this.BANK.SelectedValue) || IsNull(this.DATE_S.Text.ToRawTarikh()) || this.DATE_S.Text.ToRawTarikh() == "")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سريال ، نام بانك و تاريخ سررسيد  نمي تواند خالي باشد!" });
            }
            if (ErrosMessages.Any())
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
            if (!IsVisible || !IsLoaded || isClosing) { return; }

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !(_SaveExit.IsFocused))
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
    }
}
