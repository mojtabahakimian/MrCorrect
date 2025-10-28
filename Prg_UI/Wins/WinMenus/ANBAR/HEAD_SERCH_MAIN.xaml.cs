using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.CUC;
using Prg_UI.Functions;
using Prg_UI.Functions.SqlTools;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class HEAD_SERCH_MAIN : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HEAD_SERCH_MAIN()
        {
            InitializeComponent();
            this.DataContext = this;
            InitializeOperatorData();
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        #region LOCALS
        public class CMB1
        {
            public string? ROUTE_NAME { get; set; }
            public string? Expr1 { get; set; }
        }
        public class CMB2
        {
            public string? FNUMB { get; set; }
            public string? NAM { get; set; }
        }
        public class CMB3
        {
            public int? MON_ID { get; set; }
            public string? MON { get; set; }
        }
        public class OperatorItem
        {
            public string OpValue { get; set; }
            public string OpDisplay { get; set; }
        }

        public class AggregateItem
        {
            public string OpValue { get; set; }
            public string OpDisplay { get; set; }
        }
        #endregion

        private const string BEYN = "بین";
        private const string SHAMEL = "شامل";
        private const string BEDUNE = "بدون";

        private string SQLT = " SELECT NUMBER, BARGAH, ANBNAME, NUMBER1, DATE_N, N_S, CUSTNAME, MOLAH, ANBARF, FNUMCO, MEGH, MEGHk, MEGH_MAR, MABL, kala, MABL_K, SANAD_NO, CUST_NO, VAHEDNAME, GRPNAME, code, hes, USER_NAME, SHNAME, CUSTKNAME, DEPNAME, MANDAH, SHIFT_ID, DEPATMAN, CUST_COD, TAGCODE, GRPCODE, ANBARCODE, VAHCODE, id, MAS, N_RASID, N_FANI, SHARAYET, IMBAA, HMBAA, TAMIR, TICMBAA, OKF, TOZIH, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, CMBAA, vazn, N_TAF, TOTALARZ, N_KOL, N_MOIN, MM, KHFR, GHFR, TAG, VAHED, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, AVRAGE, mabrial, ANBARAS, ECODE, PCODE, IYALAT, CITY, TKHN, col1, col2, col3, col4, col5, col6, col7, col8, col9, coln1, coln2, coln3, coln4, coln5, coln6, coln7, coln8, coln9, ADDRESS, TEL, CODE_E, MCODEM, MOBILE, Longitude, Latitude, ROUTE_NAME, OSTANID, SHAHRID, OSNAME, CITYNAME ";

        private string SHART = "";
        private string SQLSTA = "";
        private string SQLSTAFIN = "";
        private string grbCOL = "";
        private string grb = "";

        private ObservableCollection<OperatorItem> _numericOpData;
        private ObservableCollection<OperatorItem> _textOpData;
        private ObservableCollection<OperatorItem> _dateOpData;
        private ObservableCollection<AggregateItem> _aggOpData;
        public ObservableCollection<OperatorItem> NUMERIC_OP_DATA
        {
            get => _numericOpData;
            set { _numericOpData = value; OnPropertyChanged(); }
        }
        public ObservableCollection<OperatorItem> TEXT_OP_DATA
        {
            get => _textOpData;
            set { _textOpData = value; OnPropertyChanged(); }
        }
        public ObservableCollection<OperatorItem> DATE_OP_DATA
        {
            get => _dateOpData;
            set { _dateOpData = value; OnPropertyChanged(); }
        }
        public ObservableCollection<AggregateItem> AGG_OP_DATA
        {
            get => _aggOpData;
            set { _aggOpData = value; OnPropertyChanged(); }
        }
        private void InitializeOperatorData()
        {
            // Initialize numeric operators
            NUMERIC_OP_DATA = new ObservableCollection<OperatorItem>
            {
                new OperatorItem { OpValue = "=", OpDisplay = "=" },
                new OperatorItem { OpValue = "<>", OpDisplay = "<>" },
                new OperatorItem { OpValue = ">", OpDisplay = ">" },
                new OperatorItem { OpValue = ">=", OpDisplay = ">=" },
                new OperatorItem { OpValue = "<", OpDisplay = "<" },
                new OperatorItem { OpValue = "<=", OpDisplay = "<=" },
                new OperatorItem { OpValue = BEYN, OpDisplay = BEYN }
            };

            //text operators
            TEXT_OP_DATA = new ObservableCollection<OperatorItem>
            {
                new OperatorItem { OpValue = "="    },
                new OperatorItem { OpValue = "<>"   },
                new OperatorItem { OpValue =SHAMEL  },
                new OperatorItem { OpValue = BEDUNE }
            };

            //date operators
            DATE_OP_DATA = new ObservableCollection<OperatorItem>
            {
                new OperatorItem { OpValue = "="     },
                new OperatorItem { OpValue = "<>"   },
                new OperatorItem { OpValue = ">"     },
                new OperatorItem { OpValue = ">="   },
                new OperatorItem { OpValue = "<"     },
                new OperatorItem { OpValue = "<="    },
                new OperatorItem { OpValue = BEYN   }
            };

            //aggregate operators
            AGG_OP_DATA = new ObservableCollection<AggregateItem>
            {
                new AggregateItem { OpValue = "SUM", OpDisplay = "جمع" },
                new AggregateItem { OpValue = "AVG", OpDisplay = "میانگین" },
                new AggregateItem { OpValue = "MIN", OpDisplay = "حداقل" },
                new AggregateItem { OpValue = "MAX", OpDisplay = "حداکثر" },
                new AggregateItem { OpValue = "COUNT", OpDisplay = "تعداد (شمارش)" }
            };
        }

        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }

        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SEARCHMO", new WindowInteropHelper(this).Handle, this.GetType().Name); //جستجوگر موجودي کالا
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ResetDefaultUi();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
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
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
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
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع برگه
            TAGCODE.ItemsSource = dbms.DoGetDataSQL<TAGCOD>($"SELECT CODE, BARGAH FROM TAGCOD").ToList();

            //واحد کالا
            VAHCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_VAHEDS>($"SELECT CODE, NAMES FROM TCOD_VAHEDS").ToList();

            //گروه کالا
            GRPCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_STUFGROUP>($"SELECT CODE, NAMES FROM TCOD_STUFGROUP").ToList();

            //انبار
            ANBARCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_ANBAR>($"SELECT CODE, NAMES FROM TCOD_ANBAR").ToList();

            //کاربران
            var RST_PERSONEL = dbms.DoGetDataSQL<SALA_DTL>("SELECT SAL_NAME, IDD FROM dbo.SALA_DTL WHERE (ENABL=0) ORDER BY IDD").ToList();
            foreach (var rows in RST_PERSONEL)
            {
                if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }
            USER_NAME.ItemsSource = RST_PERSONEL;

            //واحد فروش
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            //شیفت
            SHIFT_ID.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT_ID, SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();

            //نوع مشتری
            CUST_COD.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();

            //محل مصرف
            N_RASID.ItemsSource = dbms.DoGetDataSQL<CMB2>("SELECT dbo.HEAD_MANF.FNUMB, ISNULL(dbo.HEAD_MANF.NAMES, dbo.STUF_DEF.NAME) AS NAM FROM dbo.STUF_DEF RIGHT OUTER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE = dbo.HEAD_MANF.CODE;").ToList();

        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

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

        private void ResetDefaultUi()
        {
            NUMBERB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGHB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGHkB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABLB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABL_KB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            VAHCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            GRPCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            FNUMCOB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            NUMBER1B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGH_MARB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            ANBARCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_SB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            SHIFT_IDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            CUST_CODB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MASB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_RASIDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            TAGCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            CODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");

            // Set default operators for text fields
            KALAB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            USER_NAMEB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            CUSTNAMEB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            hesB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            MOLAHB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            SHARAYETB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            N_FANIB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);
            MANDAHB.SelectedItem = TEXT_OP_DATA.FirstOrDefault(o => o.OpValue == SHAMEL);

            // Set default date operator
            DATE_NB.SelectedItem = DATE_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
        }
        private void ClearFreshAll()
        {
            NUMBER.Text = null;
            NUMBERB.SelectedItem = null;
            TAGCODE.SelectedItem = null;
            TAGCODEB.SelectedItem = null;
            DATE_N.Text = null;
            DATE_NB.SelectedItem = null;
            DATE_NT.Text = null;
            CODE.Text = null;
            CODEB.SelectedItem = null;
            KALA.Text = null;
            KALAB.SelectedItem = null;
            MEGH.Text = null;
            MEGHB.SelectedItem = null;
            MEGHk.Text = null;
            MEGHkB.SelectedItem = null;
            CUSTNAME.Text = null;
            CUSTNAMEB.SelectedItem = null;
            hes.Text = null;
            hesB.SelectedItem = null;
            MABL.Text = null;
            MABLB.SelectedItem = null;
            MABL_K.Text = null;
            MABL_KB.SelectedItem = null;
            VAHCODE.SelectedItem = null;
            VAHCODEB.SelectedItem = null;
            GRPCODE.SelectedItem = null;
            GRPCODEB.SelectedItem = null;
            MOLAH.Text = null;
            MOLAHB.SelectedItem = null;
            SHARAYET.Text = null;
            SHARAYETB.SelectedItem = null;
            FNUMCO.Text = null;
            FNUMCOB.SelectedItem = null;
            NUMBER1.Text = null;
            NUMBER1B.SelectedItem = null;
            MEGH_MAR.Text = null;
            MEGH_MARB.SelectedItem = null;
            MANDAH.Text = null;
            MANDAHB.SelectedItem = null;
            ANBARCODE.SelectedItem = null;
            ANBARCODEB.SelectedItem = null;
            N_S.Text = null;
            N_SB.SelectedItem = null;
            USER_NAME.SelectedItem = null;
            USER_NAMEB.SelectedItem = null;
            SHIFT_ID.SelectedItem = null;
            SHIFT_IDB.SelectedItem = null;
            CUST_COD.SelectedItem = null;
            CUST_CODB.SelectedItem = null;
            MAS.Text = null;
            MASB.SelectedItem = null;
            N_RASID.SelectedItem = null;
            N_RASIDB.SelectedItem = null;
            N_FANI.Text = null;
            N_FANIB.SelectedItem = null;
            //ANDOR_AfterUpdate
        }
        private void DATE_NB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (DATE_NB.SelectedItem is OperatorItem DTNB)
            {
                if (DTNB.OpValue == BEYN)
                {
                    DATE_NT_SP.Visibility = Visibility.Visible;
                }
                else
                {
                    DATE_NT_SP.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ChShart()
        {
            if (!string.IsNullOrEmpty(SHART) && !SHART.TrimEnd().EndsWith("AND") && !SHART.TrimEnd().EndsWith("OR"))
            {
                SHART += " AND ";
            }
        }

        private void ChField()
        {
            // اگر هنوز هیچ ستونی اضافه نشده:
            if (string.IsNullOrWhiteSpace(SQLT))
            {
                SQLT = "SELECT ";
                return;
            }

            // اگر فقط خود SELECT (با فاصله یا بدون فاصله) هست، ویرگول نذار
            var core = SQLT.Trim();
            if (core.Equals("SELECT", StringComparison.OrdinalIgnoreCase))
                return;

            // در غیر این صورت اگر آخرش ویرگول نداره، ویرگول اضافه کن
            if (!SQLT.TrimEnd().EndsWith(","))
                SQLT += " , ";
        }
        private bool CheckField(string fieldName)
        {
            var numericFields = new[]
            {
                "MEGH", "MEGHk", "MABL", "MABL_K", "N_KOL", "N_MOIN", "IMBAA",
                "MEGH_MAR", "MAS", "N_RASID", "KHFR", "GHFR", "N_TAF", "TOTALARZ",
                "TAMIR", "MIN_M", "MAX_M", "N_SEF", "B_SEF", "MABL_F", "AVRAGE",
                "MABRIAL", "VAZN", "TKHN"
            };

            return numericFields.Contains(fieldName);
        }

        private void CreateShart()
        {
            //SHART = "";

            // NUMBER field
            var numberTextBox = FindName("NUMBER") as NumericTextBox;
            var numberOp = FindName("NUMBERB") as ComboBox;
            if (!string.IsNullOrEmpty(numberTextBox?.Text))
            {
                SHART += $"(NUMBER {numberOp?.SelectedValue ?? "="} {numberTextBox.Text})";
            }

            // TAGCODE field
            var tagcodeCombo = FindName("TAGCODE") as ComboBox;
            var tagcodeOp = FindName("TAGCODEB") as ComboBox;
            if (tagcodeCombo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(TAGCODE {tagcodeOp?.SelectedValue ?? "="} {tagcodeCombo.SelectedValue})";
            }

            // DATE_N field
            var dateTextBox = FindName("DATE_N") as Xceed.Wpf.Toolkit.MaskedTextBox;
            var dateOp = FindName("DATE_NB") as ComboBox;
            var dateToTextBox = FindName("DATE_NT") as Xceed.Wpf.Toolkit.MaskedTextBox;
            if (!string.IsNullOrEmpty(dateTextBox?.Text.ToRawTarikh().Trim()))
            {
                if (dateOp?.SelectedValue?.ToString() == BEYN)
                {
                    if (string.IsNullOrEmpty(dateToTextBox?.Text?.ToRawTarikh().Trim()))
                    {
                        return;
                    }
                    ChShart();
                    SHART += $"(DATE_N BETWEEN {dateTextBox.Text.ToRawTarikh()} AND {dateToTextBox.Text.ToRawTarikh()})";
                }
                else
                {
                    ChShart();
                    SHART += $"(DATE_N {dateOp?.SelectedValue ?? "="} {dateTextBox.Text.ToRawTarikh()})";
                }
            }

            // CODE field
            var codeTextBox = FindName("CODE") as NumericTextBox;
            var codeOp = FindName("CODEB") as ComboBox;
            if (!string.IsNullOrEmpty(codeTextBox?.Text))
            {
                ChShart();
                SHART += $"(CODE {codeOp?.SelectedValue ?? "="} {codeTextBox.Text})";
            }

            // KALA field
            var kalaTextBox = FindName("KALA") as TextBox;
            var kalaOp = FindName("KALAB") as ComboBox;
            if (!string.IsNullOrEmpty(kalaTextBox?.Text))
            {
                ChShart();
                switch (kalaOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(KALA = '{kalaTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(KALA <> '{kalaTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(KALA like '%{kalaTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(KALA not like '%{kalaTextBox.Text}%')";
                        break;
                }
            }

            // MEGH field
            var meghTextBox = FindName("MEGH") as NumericTextBox;
            var meghOp = FindName("MEGHB") as ComboBox;
            if (!string.IsNullOrEmpty(meghTextBox?.Text))
            {
                ChShart();
                SHART += $"(MEGH {meghOp?.SelectedValue ?? "="} {meghTextBox.Text})";
            }

            // MEGHk field
            var meghkTextBox = FindName("MEGHk") as NumericTextBox;
            var meghkOp = FindName("MEGHkB") as ComboBox;
            if (!string.IsNullOrEmpty(meghkTextBox?.Text))
            {
                ChShart();
                SHART += $"(MEGHk {meghkOp?.SelectedValue ?? "="} {meghkTextBox.Text})";
            }

            // CUSTNAME field
            var custnameTextBox = FindName("CUSTNAME") as TextBox;
            var custnameOp = FindName("CUSTNAMEB") as ComboBox;
            if (!string.IsNullOrEmpty(custnameTextBox?.Text))
            {
                ChShart();
                switch (custnameOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(CUSTNAME = '{custnameTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(CUSTNAME <> '{custnameTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(CUSTNAME like '%{custnameTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(CUSTNAME not like '%{custnameTextBox.Text}%')";
                        break;
                }
            }

            // HES field
            var hesTextBox = FindName("hes") as TextBox;
            var hesOp = FindName("hesB") as ComboBox;
            if (!string.IsNullOrEmpty(hesTextBox?.Text))
            {
                ChShart();
                switch (hesOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(HES = '{hesTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(HES <> '{hesTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(HES like '%{hesTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(HES not like '%{hesTextBox.Text}%')";
                        break;
                }
            }

            // mabl field
            var mablTextBox = FindName("MABL") as NumericTextBox;
            var mablOp = FindName("MABLB") as ComboBox;
            if (!string.IsNullOrEmpty(mablTextBox?.Text))
            {
                ChShart();
                SHART += $"(mabl {mablOp?.SelectedValue ?? "="} {mablTextBox.Text})";
            }

            // MABL_K field
            var mablkTextBox = FindName("MABL_K") as NumericTextBox;
            var mablkOp = FindName("MABL_KB") as ComboBox;
            if (!string.IsNullOrEmpty(mablkTextBox?.Text))
            {
                ChShart();
                SHART += $"(MABL_K {mablkOp?.SelectedValue ?? "="} {mablkTextBox.Text})";
            }

            // KHFR field
            var khfrTextBox = FindName("KHFR") as NumericTextBox;
            var khfrOp = FindName("KHFRB") as ComboBox;
            if (!string.IsNullOrEmpty(khfrTextBox?.Text))
            {
                ChShart();
                SHART += $"(KHFR {khfrOp?.SelectedValue ?? "="} {khfrTextBox.Text})";
            }

            // GHFR field
            var ghfrTextBox = FindName("GHFR") as NumericTextBox;
            var ghfrOp = FindName("GHFRB") as ComboBox;
            if (!string.IsNullOrEmpty(ghfrTextBox?.Text))
            {
                ChShart();
                SHART += $"(GHFR {ghfrOp?.SelectedValue ?? "="} {ghfrTextBox.Text})";
            }

            // N_KOL field
            var nkolTextBox = FindName("N_KOL") as NumericTextBox;
            var nkolOp = FindName("N_KOLB") as ComboBox;
            if (!string.IsNullOrEmpty(nkolTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_KOL {nkolOp?.SelectedValue ?? "="} {nkolTextBox.Text})";
            }

            // N_MOIN field
            var nmoinTextBox = FindName("N_MOIN") as NumericTextBox;
            var nmoinOp = FindName("N_MOINB") as ComboBox;
            if (!string.IsNullOrEmpty(nmoinTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_MOIN {nmoinOp?.SelectedValue ?? "="} {nmoinTextBox.Text})";
            }

            // IMBAA field
            var imbaaTextBox = FindName("IMBAA") as NumericTextBox;
            var imbaaOp = FindName("IMBAAB") as ComboBox;
            if (!string.IsNullOrEmpty(imbaaTextBox?.Text))
            {
                ChShart();
                SHART += $"(IMBAA {imbaaOp?.SelectedValue ?? "="} {imbaaTextBox.Text})";
            }

            // N_TAF field
            var ntafTextBox = FindName("N_TAF") as NumericTextBox;
            var ntafOp = FindName("N_TAFB") as ComboBox;
            if (!string.IsNullOrEmpty(ntafTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_TAF {ntafOp?.SelectedValue ?? "="} {ntafTextBox.Text})";
            }

            // TOTALARZ field
            var totalarzTextBox = FindName("TOTALARZ") as NumericTextBox;
            var totalarzOp = FindName("TOTALARZB") as ComboBox;
            if (!string.IsNullOrEmpty(totalarzTextBox?.Text))
            {
                ChShart();
                SHART += $"(TOTALARZ {totalarzOp?.SelectedValue ?? "="} {totalarzTextBox.Text})";
            }

            // TAMIR field
            var tamirTextBox = FindName("TAMIR") as NumericTextBox;
            var tamirOp = FindName("TAMIRB") as ComboBox;
            if (!string.IsNullOrEmpty(tamirTextBox?.Text))
            {
                ChShart();
                SHART += $"(TAMIR {tamirOp?.SelectedValue ?? "="} {tamirTextBox.Text})";
            }

            // VAHCODE field
            var vahcodeTextBox = FindName("VAHCODE") as NumericTextBox;
            var vahcodeOp = FindName("VAHCODEB") as ComboBox;
            if (!string.IsNullOrEmpty(vahcodeTextBox?.Text))
            {
                ChShart();
                SHART += $"(VAHCODE {vahcodeOp?.SelectedValue ?? "="} {vahcodeTextBox.Text})";
            }

            // GRPCODE field
            var grpcodeTextBox = FindName("GRPCODE") as NumericTextBox;
            var grpcodeOp = FindName("GRPCODEB") as ComboBox;
            if (!string.IsNullOrEmpty(grpcodeTextBox?.Text))
            {
                ChShart();
                SHART += $"(GRPCODE {grpcodeOp?.SelectedValue ?? "="} {grpcodeTextBox.Text})";
            }

            // MOLAH field
            var molahTextBox = FindName("MOLAH") as TextBox;
            var molahOp = FindName("MOLAHB") as ComboBox;
            if (!string.IsNullOrEmpty(molahTextBox?.Text))
            {
                ChShart();
                switch (molahOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(MOLAH = '{molahTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(MOLAH <> '{molahTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(MOLAH like '%{molahTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(MOLAH not like '%{molahTextBox.Text}%')";
                        break;
                }
            }

            // SHARAYET field
            var sharayetTextBox = FindName("SHARAYET") as TextBox;
            var sharayetOp = FindName("SHARAYETB") as ComboBox;
            if (!string.IsNullOrEmpty(sharayetTextBox?.Text))
            {
                ChShart();
                switch (sharayetOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(SHARAYET = '{sharayetTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(SHARAYET <> '{sharayetTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(SHARAYET like '%{sharayetTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(SHARAYET not like '%{sharayetTextBox.Text}%')";
                        break;
                }
            }

            // FNUMCO field
            var fnumcoTextBox = FindName("FNUMCO") as NumericTextBox;
            var fnumcoOp = FindName("FNUMCOB") as ComboBox;
            if (!string.IsNullOrEmpty(fnumcoTextBox?.Text))
            {
                ChShart();
                SHART += $"(FNUMCO {fnumcoOp?.SelectedValue ?? "="} {fnumcoTextBox.Text})";
            }

            // NUMBER1 field
            var number1TextBox = FindName("NUMBER1") as NumericTextBox;
            var number1Op = FindName("NUMBER1B") as ComboBox;
            if (!string.IsNullOrEmpty(number1TextBox?.Text))
            {
                ChShart();
                SHART += $"(NUMBER1 {number1Op?.SelectedValue ?? "="} {number1TextBox.Text})";
            }

            // MEGH_MAR field
            var meghmarTextBox = FindName("MEGH_MAR") as NumericTextBox;
            var meghmarOp = FindName("MEGH_MARB") as ComboBox;
            if (!string.IsNullOrEmpty(meghmarTextBox?.Text))
            {
                ChShart();
                SHART += $"(MEGH_MAR {meghmarOp?.SelectedValue ?? "="} {meghmarTextBox.Text})";
            }

            // MANDAH field
            var mandahTextBox = FindName("MANDAH") as TextBox;
            var mandahOp = FindName("MANDAHB") as ComboBox;
            if (!string.IsNullOrEmpty(mandahTextBox?.Text))
            {
                ChShart();
                switch (mandahOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(MANDAH = '{mandahTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(MANDAH <> '{mandahTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(MANDAH like '%{mandahTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(MANDAH not like '%{mandahTextBox.Text}%')";
                        break;
                }
            }

            // ANBARCODE field
            var anbarcodeTextBox = FindName("ANBARCODE") as NumericTextBox;
            var anbarcodeOp = FindName("ANBARCODEB") as ComboBox;
            if (!string.IsNullOrEmpty(anbarcodeTextBox?.Text))
            {
                ChShart();
                SHART += $"(ANBARCODE {anbarcodeOp?.SelectedValue ?? "="} {anbarcodeTextBox.Text})";
            }

            // N_S field
            var nsTextBox = FindName("N_S") as NumericTextBox;
            var nsOp = FindName("N_SB") as ComboBox;
            if (!string.IsNullOrEmpty(nsTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_S {nsOp?.SelectedValue ?? "="} {nsTextBox.Text})";
            }

            // USER_NAME field
            var usernameTextBox = FindName("USER_NAME") as TextBox;
            if (!string.IsNullOrEmpty(usernameTextBox?.Text))
            {
                ChShart();
                SHART += $"(USER_NAME = '{usernameTextBox.Text}')";
            }

            // SHIFT_ID field
            var shiftidTextBox = FindName("SHIFT_ID") as NumericTextBox;
            var shiftidOp = FindName("SHIFT_IDB") as ComboBox;
            if (!string.IsNullOrEmpty(shiftidTextBox?.Text))
            {
                ChShart();
                SHART += $"(SHIFT_ID {shiftidOp?.SelectedValue ?? "="} {shiftidTextBox.Text})";
            }

            // DEPATMAN field
            var depatmanTextBox = FindName("DEPATMAN") as NumericTextBox;
            var depatmanOp = FindName("DEPATMANB") as ComboBox;
            if (!string.IsNullOrEmpty(depatmanTextBox?.Text))
            {
                ChShart();
                SHART += $"(DEPATMAN {depatmanOp?.SelectedValue ?? "="} {depatmanTextBox.Text})";
            }

            // CUST_COD field
            var custcodTextBox = FindName("CUST_COD") as NumericTextBox;
            var custcodOp = FindName("CUST_CODB") as ComboBox;
            if (!string.IsNullOrEmpty(custcodTextBox?.Text))
            {
                ChShart();
                SHART += $"(CUST_COD {custcodOp?.SelectedValue ?? "="} {custcodTextBox.Text})";
            }

            // MAS field
            var masTextBox = FindName("MAS") as NumericTextBox;
            var masOp = FindName("MASB") as ComboBox;
            if (!string.IsNullOrEmpty(masTextBox?.Text))
            {
                ChShart();
                SHART += $"(MAS {masOp?.SelectedValue ?? "="} {masTextBox.Text})";
            }

            // N_RASID field
            var nrasidTextBox = FindName("N_RASID") as NumericTextBox;
            var nrasidOp = FindName("N_RASIDB") as ComboBox;
            if (!string.IsNullOrEmpty(nrasidTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_RASID {nrasidOp?.SelectedValue ?? "="} {nrasidTextBox.Text})";
            }

            // N_FANI field
            var nfaniTextBox = FindName("N_FANI") as TextBox;
            var nfaniOp = FindName("N_FANIB") as ComboBox;
            if (!string.IsNullOrEmpty(nfaniTextBox?.Text))
            {
                ChShart();
                switch (nfaniOp?.SelectedValue?.ToString())
                {
                    case "=":
                        SHART += $"(N_FANI = '{nfaniTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(N_FANI <> '{nfaniTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(N_FANI like '%{nfaniTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(N_FANI not like '%{nfaniTextBox.Text}%')";
                        break;
                }
            }

            // mm field
            var mmTextBox = FindName("mm") as NumericTextBox;
            var mmOp = FindName("MMB") as ComboBox;
            if (!string.IsNullOrEmpty(mmTextBox?.Text))
            {
                ChShart();
                SHART += $"(MM {mmOp?.SelectedValue ?? "="} {mmTextBox.Text})";
            }

            // MIN_M field
            var minmTextBox = FindName("MIN_M") as NumericTextBox;
            var minmOp = FindName("MIN_MB") as ComboBox;
            if (!string.IsNullOrEmpty(minmTextBox?.Text))
            {
                ChShart();
                SHART += $"(MIN_M {minmOp?.SelectedValue ?? "="} {minmTextBox.Text})";
            }

            // MAX_M field
            var maxmTextBox = FindName("MAX_M") as NumericTextBox;
            var maxmOp = FindName("MAX_MB") as ComboBox;
            if (!string.IsNullOrEmpty(maxmTextBox?.Text))
            {
                ChShart();
                SHART += $"(MAX_M {maxmOp?.SelectedValue ?? "="} {maxmTextBox.Text})";
            }

            // N_SEF field
            var nsefTextBox = FindName("N_SEF") as NumericTextBox;
            var nsefOp = FindName("N_SEFB") as ComboBox;
            if (!string.IsNullOrEmpty(nsefTextBox?.Text))
            {
                ChShart();
                SHART += $"(N_SEF {nsefOp?.SelectedValue ?? "="} {nsefTextBox.Text})";
            }

            // B_SEF field
            var bsefTextBox = FindName("B_SEF") as NumericTextBox;
            var bsefOp = FindName("B_SEFB") as ComboBox;
            if (!string.IsNullOrEmpty(bsefTextBox?.Text))
            {
                ChShart();
                SHART += $"(B_SEF {bsefOp?.SelectedValue ?? "="} {bsefTextBox.Text})";
            }

            // MABL_F field
            var mablfTextBox = FindName("MABL_F") as NumericTextBox;
            var mablfOp = FindName("MABL_FB") as ComboBox;
            if (!string.IsNullOrEmpty(mablfTextBox?.Text))
            {
                ChShart();
                SHART += $"(MABL_F {mablfOp?.SelectedValue ?? "="} {mablfTextBox.Text})";
            }

            // AVRAGE field
            var avrageTextBox = FindName("AVRAGE") as NumericTextBox;
            var avrageOp = FindName("AVRAGEB") as ComboBox;
            if (!string.IsNullOrEmpty(avrageTextBox?.Text))
            {
                ChShart();
                SHART += $"(AVRAGE {avrageOp?.SelectedValue ?? "="} {avrageTextBox.Text})";
            }

            // MABRIAL field
            var mabrialTextBox = FindName("MABRIAL") as NumericTextBox;
            var mabrialOp = FindName("MABRIALB") as ComboBox;
            if (!string.IsNullOrEmpty(mabrialTextBox?.Text))
            {
                ChShart();
                SHART += $"(mabrial {mabrialOp?.SelectedValue ?? "="} {mabrialTextBox.Text})";
            }

            // VAZN field
            var vaznTextBox = FindName("VAZN") as NumericTextBox;
            var vaznOp = FindName("VAZNB") as ComboBox;
            if (!string.IsNullOrEmpty(vaznTextBox?.Text))
            {
                ChShart();
                SHART += $"(VAZN {vaznOp?.SelectedValue ?? "="} {vaznTextBox.Text})";
            }

            // TKHN field
            var tkhnTextBox = FindName("TKHN") as NumericTextBox;
            var tkhnOp = FindName("TKHNB") as ComboBox;
            if (!string.IsNullOrEmpty(tkhnTextBox?.Text))
            {
                ChShart();
                SHART += $"(TKHN {tkhnOp?.SelectedValue ?? "="} {tkhnTextBox.Text})";
            }

            if (!string.IsNullOrEmpty(SHART))
            {
                SHART = $"({SHART})";
            }
        }
        private void DISABLED_CreateField()
        {
            if (string.IsNullOrEmpty(SQLT))
            {
                SQLT = "SELECT ";
                grbCOL = "";
            }

            void Add(string field)
            {
                ChField();
            }

            // ساده‌ها
            //Add("NUMBER");
            //Add("TAGCODE", TAGCODEC, TAGCODEBS);
            //Add("DATE_N", DATE_NC, DATE_NBS);
            //Add("CODE", CODEC, CODEBS);
            //Add("KALA", KALAC, KALABS);

            //// عددی‌ها (NVARCHAR در DB → تبدیل داخل Aggregation)
            //Add("MEGH", MEGHC, MEGHBS);
            //Add("MEGHk", MEGHkC, MEGHkBS);
            //Add("MABL", MABLC, MABLBS);
            //Add("MABL_K", MABL_KC, MABL_KBS);
            //Add("KHFR", KHFRC, KHFRBS);
            //Add("GHFR", GHFRC, GHFRBS);
            //Add("N_KOL", N_KOLC, N_KOLBS);
            //Add("N_MOIN", N_MOINC, N_MOINBS);
            //Add("IMBAA", IMBAAC, IMBAABS);

            //Add("N_TAF", N_TAFC, N_TAFBS);
            //Add("TOTALARZ", TOTALARZC, TOTALARZBS);
            //Add("TAMIR", TAMIRC, TAMIRBS);
            //Add("FNUMCO", FNUMCOC, FNUMCOBS);
            //Add("NUMBER1", NUMBER1C, NUMBER1BS);
            //Add("MEGH_MAR", MEGH_MARC, MEGH_MARBS);
            //Add("ANBARCODE", ANBARCODEC, ANBARCODEBS);
            //Add("N_S", N_SC, N_SBS);
            //Add("SHIFT_ID", SHIFT_IDC, SHIFT_IDBS);
            //Add("DEPATMAN", DEPATMANC, DEPATMANBS);
            //Add("CUST_COD", CUST_CODC, CUST_CODBS);
            //Add("MAS", MASC, MASBS);
            //Add("N_RASID", N_RASIDC, N_RASIDBS);
            //Add("MM", MMC, MMBS);
            //Add("MIN_M", MIN_MC, MIN_MBS);
            //Add("MAX_M", MAX_MC, MAX_MBS);
            //Add("N_SEF", N_SEFC, N_SEFBS);
            //Add("B_SEF", B_SEFC, B_SEFBS);
            //Add("MABL_F", MABL_FC, MABL_FBS);
            //Add("AVRAGE", AVRAGEC, AVRAGEBS);
            //Add("MABRIAL", MABRIALC, MABRIALBS);
            //Add("VAZN", VAZNC, VAZNBS);
            //Add("TKHN", TKHNC, TKHNBS);

            //// متنی‌ها
            //Add("CUSTNAME", CUSTNAMEC, CUSTNAMEBS);
            //Add("HES", hesC, hesBS);
            //Add("MOLAH", MOLAHC, MOLAHBS);
            //Add("SHARAYET", SHARAYETC, SHARAYETBS);
            //Add("N_FANI", N_FANIC, N_FANIBS);
            //Add("USER_NAME", USER_NAMEC, USER_NAMEBS);
            //Add("ROUTE_NAME", ROUTE_NAMEC, ROUTE_NAMEBS);

            // ستون‌های سفارشی col1..col9 (مثل قبل + نام نمایشی colN*)
            void AddCol(string col, CheckBox cb, ComboBox agg, string coln)
            {
                if (cb?.IsChecked == true)
                {
                    ChField(); SQLT += $" {coln} ";
                    grbCOL += "," + coln;
                }
            }

            // Group By مثل قبل (فقط فیلدهایی که بدون تجمیع انتخاب شده‌اند)
            grb = "Group By ";
            var controlsToCheck = new[]
            {
                "NUMBERBS","TAGCODEBS","DATE_NBS","CODEBS","KALABS","MEGHBS","MEGHkBS","CUSTNAMEBS","hesBS",
                "MABLBS","MABL_KBS","KHFRBS","GHFRBS","N_KOLBS","N_MOINBS","IMBAABS","N_TAFBS","TOTALARZBS",
                "TAMIRBS","VAHCODEBS","GRPCODEBS","MOLAHBS","SHARAYETBS","FNUMCOBS","NUMBER1BS","MEGH_MARBS",
                "MANDAHBS","ANBARCODEBS","N_SBS","USER_NAMEBS","SHIFT_IDBS","DEPATMANBS","CUST_CODBS","MASBS",
                "N_RASIDBS","N_FANIBS","MMBS","MIN_MBS","MAX_MBS","N_SEFBS","B_SEFBS","MABL_FBS","AVRAGEBS",
                "MABRIALBS","VAZNBS","TKHNBS","col1BS","col2BS","col3BS","col4BS","col5BS","col6BS","col7BS",
                "col8BS","col9BS","OSTANIDBS","SHAHRIDBS","ROUTE_NAMEBS"
            };

            foreach (var controlName in controlsToCheck)
            {
                var aggCombo = FindName(controlName) as ComboBox;
                var checkBoxName = controlName.Substring(0, controlName.Length - 2) + "C";
                var checkBox = FindName(checkBoxName) as CheckBox;

                if (checkBox?.IsChecked == true && string.IsNullOrEmpty(aggCombo?.SelectedValue?.ToString()))
                {
                    var fieldName = controlName.Substring(0, controlName.Length - 2);
                    grb += (grb == "Group By ") ? fieldName : "," + fieldName;
                }
            }

            grb += grbCOL;
        }


        private void ANDOR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            //CreateField();
            CreateShart();

            //ANDOR_AfterUpdate
            if (!string.IsNullOrEmpty(SHART) && ANDOR.SelectedValue != null)
            {
                if (ANDOR.SelectedValue is ComboBoxItem SelectedVal)
                {
                    if (SelectedVal?.Content == "و")
                    {
                        SHART = SHART + " AND ";
                    }
                    else
                    {
                        SHART = SHART + " OR ";
                    }

                    ClearFreshAll();
                    ResetDefaultUi();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new HEAD_SERCH_MAIN_ADVANC().Show();
        }

        /// <summary>
        /// Go
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            //Command71 اجرای گزارش

            GoFinalProccess();
        }
        private void GoFinalProccess()
        {
            KALAS_MAIN_ADVANCE KMA = default;
            try
            {
                //CreateField();
                CreateShart();

                // Apply user restrictions //ست کردن دسترسی محدود طبق دسترسی فاکتور فروش
                const string REPLACEMENT_VALUE = "dbo.HEAD_LST.";
                byte _TAG_ = 0;
                if (TAGCODE?.SelectedValue != null)
                {
                    byte.TryParse(TAGCODE.SelectedValue.ToString(), out _TAG_);
                }
                var restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(_TAG_, " WHERE "); // Assuming TAGCODE 0 is appropriate for a general search
                if (!string.IsNullOrEmpty(restrictionInfo.WhereClause))
                {
                    if (!string.IsNullOrEmpty(SHART))
                    {
                        SHART += " AND " + restrictionInfo.WhereClause.Replace("WHERE", "");
                    }
                    else
                    {
                        SHART = restrictionInfo.WhereClause.Replace("WHERE", "");
                    }

                    SHART = SHART.Replace(REPLACEMENT_VALUE, null);
                }

                SQLSTA = SQLT + " FROM KALAS " + (string.IsNullOrEmpty(SHART) ? "" : " WHERE " + SHART) + (grb != "Group By " ? " " + grb : "");

                SQLSTAFIN = SQLSTA;

                SQLSTAFIN = SQLSTA + " OPTION (FORCE ORDER, LOOP JOIN, HASH JOIN, ORDER GROUP)";
                var SelectedColumns = SqlColumnParser.ExtractColumnNames(SQLT);

                KMA = new KALAS_MAIN_ADVANCE();
                KMA.SqlQueryPassed = SQLSTAFIN;
                KMA.ColumnSelectedPassed = SelectedColumns;
                KMA.RestrictionMessages = restrictionInfo.RestrictionMessages;
                KMA.isAdvancedF12 = false;
                KMA.isSummed = true; //جمع زیر گزارش

                #region CleanMadeText
                RestoreDefaultNew();
                #endregion

                KMA.Show();
            }
            catch (Exception)
            {
                try { if (KMA != null) { KMA?.Close(); } } catch { }
                new Msgwin(false, "خطا در انجام عملیات").Show();
                return;
            }
        }

        private void RestoreDefaultNew()
        {
            SQLT = " SELECT NUMBER, BARGAH, ANBNAME, NUMBER1, DATE_N, N_S, CUSTNAME, MOLAH, ANBARF, FNUMCO, MEGH, MEGHk, MEGH_MAR, MABL, kala, MABL_K, SANAD_NO, CUST_NO, VAHEDNAME, GRPNAME, code, hes, USER_NAME, SHNAME, CUSTKNAME, DEPNAME, MANDAH, SHIFT_ID, DEPATMAN, CUST_COD, TAGCODE, GRPCODE, ANBARCODE, VAHCODE, id, MAS, N_RASID, N_FANI, SHARAYET, IMBAA, HMBAA, TAMIR, TICMBAA, OKF, TOZIH, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, CMBAA, vazn, N_TAF, TOTALARZ, N_KOL, N_MOIN, MM, KHFR, GHFR, TAG, VAHED, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, AVRAGE, mabrial, ANBARAS, ECODE, PCODE, IYALAT, CITY, TKHN, col1, col2, col3, col4, col5, col6, col7, col8, col9, coln1, coln2, coln3, coln4, coln5, coln6, coln7, coln8, coln9, ADDRESS, TEL, CODE_E, MCODEM, MOBILE, Longitude, Latitude, ROUTE_NAME, OSTANID, SHAHRID, OSNAME, CITYNAME ";

            SHART = "";
            SQLSTA = "";
            SQLSTAFIN = "";
            grbCOL = "";
            grb = "";

            SHART = string.Empty;
            grb = string.Empty;
        }
    }

}
