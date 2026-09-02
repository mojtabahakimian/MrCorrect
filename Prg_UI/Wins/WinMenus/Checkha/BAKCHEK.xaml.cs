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
        public long? CurrentPayGetdId { get; set; }
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
        public bool IsReadOnlyMode { get; set; } = false;

        public BAKCHEK(Visual thewin, string _severfilter, int _current_index = -1, bool isreadonly = false)
        {
            IsReadOnlyMode = isreadonly;
            THE_WIN = thewin;
            INDEX_DG = _current_index;
            ServerFilter = _severfilter;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            THE_WIN_2 = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            //ON_Open
            List<PAY_GETD> rst = null;
            if (!string.IsNullOrEmpty(ServerFilter))
            {
                rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {ServerFilter} ").ToList();

                // اگر با فیلتر کامل پیدا نشد، یک بار بدون شرط مبلغ جستجو می‌کند تا از عدم وجود چک مطمئن شود
                if ((rst == null || rst.Count == 0) && ServerFilter.Contains("AND MABL ="))
                {
                    string fallbackFilter = ServerFilter.Substring(0, ServerFilter.IndexOf("AND MABL =")).Trim();
                    rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {fallbackFilter} ").ToList();
                }
            }

            // اگر فرم برای ویرایش چک باز شده اما چک در دیتابیس وجود ندارد (حذف شده است)
            if (!string.IsNullOrEmpty(ServerFilter) && (rst == null || rst.Count == 0))
            {
                new Msgwin(false, "این چک در دیتابیس یافت نشد یا ممکن است حذف شده باشد.").ShowDialog();
                can = true;
                //this.Close();
                //return;
            }

            if (rst == null || rst.Count == 0)
            {
                this.N_SERI.IsReadOnly = false;
                this.SANDUGH.SelectedIndex = 0;
                this.SANDUGH.Refreshy();
                this.VAZ.SelectedIndex = 0;
            }
            else
            {
                var row = rst.FirstOrDefault();
                CurrentPayGetdId = row?.ID;
                this.RADIF.Text = row.RADIF?.ToString() ?? "";

                // اگر شماره سریال در ItemsSource کمبوباکس موجود نباشد، آن را اضافه می‌کنیم تا SelectedValue پاک نشود
                var nSeriList = N_SERI.ItemsSource as List<BACK_QRE_1> ?? new List<BACK_QRE_1>();
                if (row.N_SERI.HasValue && !nSeriList.Any(x => x.N_SERI == row.N_SERI))
                {
                    nSeriList.Insert(0, new BACK_QRE_1 { N_SERI = row.N_SERI, N_S = row.N_S, N_KOL2 = row.N_KOL2, N_KOL3 = row.N_KOL3 });
                    N_SERI.ItemsSource = null;
                    N_SERI.ItemsSource = nSeriList;
                }

                this.N_SERI.SelectedValue = row.N_SERI;
                this.N_SERI.Text = row.N_SERI?.ToString() ?? "";
                this.DATE_S.Text = row.DATE_S.ToString();
                this.SHOBEH.Text = row.SHOBEH ?? "";
                this.DATE.Text = row.DATE.ToString();
                this.NAME_TAH.Text = row.NAME_TAH ?? "";
                this.N_HESAB.Text = row.N_HESAB ?? "";
                this.MABL.Text = row.MABL?.ToString() ?? "";
                this.KOL.Text = row.N_KOL?.ToString() ?? "";
                this.MOIN.Text = row.N_MOIN?.ToString() ?? "";
                this.TAF.Text = row.N_TAF?.ToString() ?? "";
                this.BANK.SelectedValue = row.BANK;
                this.HES1.SelectedValue = row.HES1;
                this.N_SERI.IsReadOnly = true;
                if (row.SANDUGH.HasValue)
                {
                    this.SANDUGH.SelectedValue = row.SANDUGH.Value;
                }
                if (row.VAZ.HasValue)
                {
                    this.VAZ.SelectedValue = Convert.ToInt32(row.VAZ.Value);
                }
            }

            if (IsReadOnlyMode)
            {
                RADIF.IsEnabled = false;
                N_SERI.IsEnabled = false;
                BANK.IsEnabled = false;
                SANDUGH.IsEnabled = false;
                SHOBEH.IsEnabled = false;
                DATE_S.IsEnabled = false;
                DATE.IsEnabled = false;
                NAME_TAH.IsEnabled = false;
                N_HESAB.IsEnabled = false;
                MABL.IsEnabled = false;
                HES1.IsEnabled = false;
                VAZ.IsEnabled = false;

                _SaveExit.IsEnabled = false;
                _SaveExit.Visibility = Visibility.Collapsed;

                this.Title += " (فقط خواندنی)";
            }
            else
            {
                N_SERI.Focus();
            }

            CL_LMethods.SetTabIndexes(
                N_SERI,
                _SaveExit
                );
        }

        bool isClosing = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsReadOnlyMode) return;

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
                var query = CurrentPayGetdId.HasValue && CurrentPayGetdId > 0
                    ? "SELECT * FROM PAY_GETD WHERE ID = @ID"
                    : "SELECT * FROM PAY_GETD WHERE N_SERI = @N_SERI AND BANK = @BANK AND DATE_S = @DATE_S";
                var parameters = CurrentPayGetdId.HasValue && CurrentPayGetdId > 0
                    ? (object)new { ID = CurrentPayGetdId.Value }
                    : new { N_SERI = this.N_SERI.SelectedValue, BANK = this.BANK.SelectedValue, DATE_S = this.DATE_S.Text.ToRawTarikh() };
                var rst = dbms.DoGetDataSQL<PAY_GETD>(query, parameters).ToList();

                string _where = CurrentPayGetdId.HasValue && CurrentPayGetdId > 0
                    ? " WHERE ID = " + CurrentPayGetdId.Value
                    : " WHERE N_SERI=" + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh();


                // حفظ حساب واگذاری قبلی (HES2) قبل از به روزرسانی PAY_GETD
                string previousHes2 = rst?.FirstOrDefault()?.HES2;

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

                    // اگر چک قبلاً واگذار شده باشد، برای "از حساب" (FHES) از حساب واگذاری قبلی (previousHes2) استفاده می‌کنیم.
                    // اگر خالی بود، از this.HES1 استفاده می‌کنیم.
                    string targetHesForFrom = !string.IsNullOrEmpty(previousHes2)
                        ? previousHes2
                        : (this.HES1.SelectedValue != null ? this.HES1.SelectedValue.ToString() : "");

                    if (!string.IsNullOrEmpty(targetHesForFrom))
                    {
                        CL_HESABDARI.GETTAF3(targetHesForFrom, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_K = (Convert.ToInt32(CKOL) == 0) ? null : (int)CKOL;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_M = (Convert.ToInt32(CMOIN) == 0) ? null : (int)CMOIN;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T = (Convert.ToInt32(CTAF) == 0) ? null : (int)CTAF;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T2 = (Convert.ToInt32(CTAF2) == 0) ? null : (int)CTAF2;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T3 = (Convert.ToInt32(CTAF3) == 0) ? null : (int)CTAF3;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES_T4 = (Convert.ToInt32(CTAF4) == 0) ? null : (int)CTAF4;
                        ((THE_WIN as PGET_HED).PGET_LST_SUB.Items[INDEX_DG] as PGET_LST).FHES = targetHesForFrom;
                    }
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
        }

        private void N_SERI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsVisible || !IsLoaded || isClosing || IsReadOnlyMode || N_SERI.IsReadOnly) { return; }

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

            DateTime dt = DateTime.Now;
            CL_HESABDARI.TR("PAY_GETD", "N_SERI = " + this.N_SERI.SelectedValue + " AND BANK = " + this.BANK.SelectedValue + " AND DATE_S = " + this.DATE_S.Text.ToRawTarikh(), dt, 1);
            can = false;

            var pgetHed = THE_WIN as PGET_HED;

            this.Close();

            if (pgetHed != null)
            {
                pgetHed.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (INDEX_DG >= 0 && INDEX_DG < pgetHed.PGET_LST_SUB.Items.Count)
                    {
                        var parentItem = pgetHed.PGET_LST_SUB.Items[INDEX_DG] as PGET_LST;
                        if (parentItem != null)
                        {
                            _ = pgetHed.CmdSaveRecord(parentItem);
                        }
                    }
                    pgetHed.SANAD();
                    pgetHed.MoveToNextRowFromLastCell();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
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
