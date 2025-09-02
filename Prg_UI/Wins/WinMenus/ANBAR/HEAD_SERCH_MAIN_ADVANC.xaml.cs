using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.CUC;
using Prg_UI.Functions;
using Prg_UI.Functions.SqlTools;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Controls.Win.Editors;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class HEAD_SERCH_MAIN_ADVANC : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HEAD_SERCH_MAIN_ADVANC()
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

        private string SHART = "";
        private string SQLT = "";
        private string SQLSTA = "";
        private string SQLSTAFIN = "";
        private string sqlsum = "";
        private string grbCOL = "";
        private string grb = "";
        private string SQLUPDATE = "";

        private object itemm;


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
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SEARCHMO", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            ResetDefaultUi();

            CheckAllCheckBoxes(this);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.M)
            {
                e.Handled = true;
                BTN_OPENREPORT_Click(default, default);
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

            //استان
            ALL_OSTAN = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();
            foreach (var item in ALL_OSTAN) { item.OSNAME = item.OSNAME?.FixPersianChars(); }
            OSTANID.ItemsSource = ALL_OSTAN; //Combobox Ui

            //کد شهر
            ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();
            SHAHRID.ItemsSource = ALL_SHAHR;

            //مسیر ویزیت
            ROUTE_NAME.ItemsSource = dbms.DoGetDataSQL<CMB1>($@"SELECT Visit_route.ROUTE_NAME, Visit_route.ROUTE_NAME+N' - '+CUST_HESAB.NAME+N' - '+CUST_HESAB.hes AS Expr1
                                                                   FROM Visit_route
                                                                        INNER JOIN CUST_HESAB ON Visit_route.HES=CUST_HESAB.hes
                                                                   WHERE(Visit_route.RACTIVE=1)").ToList();
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
            DEPATMAN.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();

            //شیفت
            SHIFT_ID.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT_ID, SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();

            //نوع مشتری
            CUST_COD.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();

            //محل مصرف
            N_RASID.ItemsSource = dbms.DoGetDataSQL<CMB2>("SELECT dbo.HEAD_MANF.FNUMB, ISNULL(dbo.HEAD_MANF.NAMES, dbo.STUF_DEF.NAME) AS NAM FROM dbo.STUF_DEF RIGHT OUTER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE = dbo.HEAD_MANF.CODE;").ToList();

            //ماه
            MM.ItemsSource = dbms.DoGetDataSQL<CMB3>("SELECT MON_ID, MON FROM MON").ToList();
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

        public static void CheckAllCheckBoxes(DependencyObject parent)
        {
            if (parent == null)
                return;

            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is CheckBox cb)
                {
                    cb.IsChecked = true;
                }

                // recursive
                CheckAllCheckBoxes(child);
            }
        }

        private void ResetDefaultUi()
        {
            NUMBERB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGHB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGHkB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABLB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABL_KB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            KHFRB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            GHFRB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_KOLB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_MOINB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            IMBAAB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_TAFB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            TOTALARZB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            TAMIRB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            VAHCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            GRPCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            FNUMCOB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            NUMBER1B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MEGH_MARB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            ANBARCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_SB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            SHIFT_IDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            DEPATMANB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            CUST_CODB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MASB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_RASIDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MMB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MIN_MB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MAX_MB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            N_SEFB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            B_SEFB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABL_FB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            AVRAGEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            MABRIALB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            VAZNB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            TKHNB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col1B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col2B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col3B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col4B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col5B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            //col6B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            col7B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            col8B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            col9B.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            OSTANIDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            SHAHRIDB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            TAGCODEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
            ROUTE_NAMEB.SelectedItem = NUMERIC_OP_DATA.FirstOrDefault(o => o.OpValue == "=");
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
            NUMBERC.IsChecked = false;
            NUMBERBS.SelectedItem = null;
            TAGCODE.SelectedItem = null;
            TAGCODEB.SelectedItem = null;
            TAGCODEC.IsChecked = false;
            TAGCODEBS.SelectedItem = null;
            DATE_N.Text = null;
            DATE_NB.SelectedItem = null;
            DATE_NC.IsChecked = false;
            DATE_NBS.SelectedItem = null;
            DATE_NT.Text = null;
            CODE.Text = null;
            CODEB.SelectedItem = null;
            CODEC.IsChecked = false;
            CODEBS.SelectedItem = null;
            KALA.Text = null;
            KALAB.SelectedItem = null;
            KALAC.IsChecked = false;
            KALABS.SelectedItem = null;
            MEGH.Text = null;
            MEGHB.SelectedItem = null;
            MEGHC.IsChecked = false;
            MEGHBS.SelectedItem = null;
            MEGHk.Text = null;
            MEGHkB.SelectedItem = null;
            MEGHkC.IsChecked = false;
            MEGHkBS.SelectedItem = null;
            CUSTNAME.Text = null;
            CUSTNAMEB.SelectedItem = null;
            CUSTNAMEC.IsChecked = false;
            CUSTNAMEBS.SelectedItem = null;
            hes.Text = null;
            hesB.SelectedItem = null;
            hesC.IsChecked = false;
            hesBS.SelectedItem = null;
            MABL.Text = null;
            MABLB.SelectedItem = null;
            MABLC.IsChecked = false;
            MABLBS.SelectedItem = null;
            MABL_K.Text = null;
            MABL_KB.SelectedItem = null;
            MABL_KC.IsChecked = false;
            MABL_KBS.SelectedItem = null;
            N_KOL.Text = null;
            N_KOLB.SelectedItem = null;
            N_KOLC.IsChecked = false;
            N_KOLBS.SelectedItem = null;
            N_MOIN.Text = null;
            N_MOINB.SelectedItem = null;
            N_MOINC.IsChecked = false;
            N_MOINBS.SelectedItem = null;
            IMBAA.Text = null;
            IMBAAB.SelectedItem = null;
            IMBAAC.IsChecked = false;
            IMBAABS.SelectedItem = null;
            TKHN.Text = null;
            TKHNB.SelectedItem = null;
            TKHNC.IsChecked = false;
            TKHNBS.SelectedItem = null;
            KHFR.Text = null;
            KHFRB.SelectedItem = null;
            KHFRC.IsChecked = false;
            KHFRBS.SelectedItem = null;
            GHFR.Text = null;
            GHFRB.SelectedItem = null;
            GHFRC.IsChecked = false;
            GHFRBS.SelectedItem = null;
            N_TAF.Text = null;
            N_TAFB.SelectedItem = null;
            N_TAFC.IsChecked = false;
            N_TAFBS.SelectedItem = null;
            TOTALARZ.Text = null;
            TOTALARZB.SelectedItem = null;
            TOTALARZC.IsChecked = false;
            TOTALARZBS.SelectedItem = null;
            AVRAGE.Text = null;
            AVRAGEB.SelectedItem = null;
            AVRAGEC.IsChecked = false;
            AVRAGEBS.SelectedItem = null;
            MABRIAL.Text = null;
            MABRIALB.SelectedItem = null;
            MABRIALC.IsChecked = false;
            MABRIALBS.SelectedItem = null;
            VAHCODE.SelectedItem = null;
            VAHCODEB.SelectedItem = null;
            VAHCODEC.IsChecked = false;
            VAHCODEBS.SelectedItem = null;
            GRPCODE.SelectedItem = null;
            GRPCODEB.SelectedItem = null;
            GRPCODEC.IsChecked = false;
            GRPCODEBS.SelectedItem = null;
            OSTANID.SelectedItem = null;
            OSTANIDB.SelectedItem = null;
            OSTANIDC.IsChecked = false;
            OSTANIDBS.SelectedItem = null;
            SHAHRID.SelectedItem = null;
            SHAHRIDB.SelectedItem = null;
            SHAHRIDC.IsChecked = false;
            SHAHRIDBS.SelectedItem = null;
            ROUTE_NAME.SelectedItem = null;
            ROUTE_NAMEB.SelectedItem = null;
            ROUTE_NAMEC.IsChecked = false;
            ROUTE_NAMEBS.SelectedItem = null;
            MOLAH.Text = null;
            MOLAHB.SelectedItem = null;
            MOLAHC.IsChecked = false;
            MOLAHBS.SelectedItem = null;
            SHARAYET.Text = null;
            SHARAYETB.SelectedItem = null;
            SHARAYETC.IsChecked = false;
            SHARAYETBS.SelectedItem = null;
            FNUMCO.Text = null;
            FNUMCOB.SelectedItem = null;
            FNUMCOC.IsChecked = false;
            FNUMCOBS.SelectedItem = null;
            NUMBER1.Text = null;
            NUMBER1B.SelectedItem = null;
            NUMBER1C.IsChecked = false;
            NUMBER1BS.SelectedItem = null;
            MEGH_MAR.Text = null;
            MEGH_MARB.SelectedItem = null;
            MEGH_MARC.IsChecked = false;
            MEGH_MARBS.SelectedItem = null;
            MANDAH.Text = null;
            MANDAHB.SelectedItem = null;
            MANDAHC.IsChecked = false;
            MANDAHBS.SelectedItem = null;
            ANBARCODE.SelectedItem = null;
            ANBARCODEB.SelectedItem = null;
            ANBARCODEC.IsChecked = false;
            ANBARCODEBS.SelectedItem = null;
            N_S.Text = null;
            N_SB.SelectedItem = null;
            N_SC.IsChecked = false;
            N_SBS.SelectedItem = null;
            USER_NAME.SelectedItem = null;
            USER_NAMEB.SelectedItem = null;
            USER_NAMEC.IsChecked = false;
            USER_NAMEBS.SelectedItem = null;
            DEPATMAN.SelectedItem = null;
            DEPATMANB.SelectedItem = null;
            DEPATMANC.IsChecked = false;
            DEPATMANBS.SelectedItem = null;
            SHIFT_ID.SelectedItem = null;
            SHIFT_IDB.SelectedItem = null;
            SHIFT_IDC.IsChecked = false;
            SHIFT_IDBS.SelectedItem = null;
            CUST_COD.SelectedItem = null;
            CUST_CODB.SelectedItem = null;
            CUST_CODC.IsChecked = false;
            CUST_CODBS.SelectedItem = null;
            MAS.Text = null;
            MASB.SelectedItem = null;
            MASC.IsChecked = false;
            MASBS.SelectedItem = null;
            N_RASID.SelectedItem = null;
            N_RASIDB.SelectedItem = null;
            N_RASIDC.IsChecked = false;
            N_RASIDBS.SelectedItem = null;
            N_FANI.Text = null;
            N_FANIB.SelectedItem = null;
            N_FANIC.IsChecked = false;
            N_FANIBS.SelectedItem = null;
            MM.SelectedItem = null;
            MMB.SelectedItem = null;
            MMC.IsChecked = false;
            MMBS.SelectedItem = null;
            TAMIR.Text = null;
            TAMIRB.SelectedItem = null;
            TAMIRC.IsChecked = false;
            TAMIRBS.SelectedItem = null;
            MIN_M.Text = null;
            MIN_MB.SelectedItem = null;
            MIN_MC.IsChecked = false;
            MIN_MBS.SelectedItem = null;
            MAX_M.Text = null;
            MAX_MB.SelectedItem = null;
            MAX_MC.IsChecked = false;
            MAX_MBS.SelectedItem = null;
            N_SEF.Text = null;
            N_SEFB.SelectedItem = null;
            N_SEFC.IsChecked = false;
            N_SEFBS.SelectedItem = null;
            B_SEF.Text = null;
            B_SEFB.SelectedItem = null;
            B_SEFC.IsChecked = false;
            B_SEFBS.SelectedItem = null;
            MABL_F.Text = null;
            MABL_FB.SelectedItem = null;
            MABL_FC.IsChecked = false;
            MABL_FBS.SelectedItem = null;
            VAZN.Text = null;
            VAZNB.SelectedItem = null;
            VAZNC.IsChecked = false;
            VAZNBS.SelectedItem = null;
            //col1.SelectedItem = null;
            //col1B.SelectedItem = null;
            //col1C.IsChecked = false;
            //col1BS.SelectedItem = null;
            //col2.SelectedItem = null;
            //col2B.SelectedItem = null;
            //col2C.IsChecked = false;
            //col2BS.SelectedItem = null;
            //col3.SelectedItem = null;
            //col3B.SelectedItem = null;
            //col3C.IsChecked = false;
            //col3BS.SelectedItem = null;
            //col4.SelectedItem = null;
            //col4B.SelectedItem = null;
            //col4C.IsChecked = false;
            //col4BS.SelectedItem = null;
            //col5.SelectedItem = null;
            //col5B.SelectedItem = null;
            //col5C.IsChecked = false;
            //col5BS.SelectedItem = null;
            //col6.SelectedItem = null;
            //col6B.SelectedItem = null;
            //col6C.IsChecked = false;
            //col6BS.SelectedItem = null;
            col7.SelectedItem = null;
            col7B.SelectedItem = null;
            col7C.IsChecked = false;
            col7BS.SelectedItem = null;
            col8.SelectedItem = null;
            col8B.SelectedItem = null;
            col8C.IsChecked = false;
            col8BS.SelectedItem = null;
            col9.SelectedItem = null;
            col9B.SelectedItem = null;
            col9C.IsChecked = false;
            col9BS.SelectedItem = null;

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

        private static string TryCastNumeric(string field) => $"TRY_CONVERT(decimal(38,6), {field})";
        private string FormatAgg(string field, ComboBox aggCombo)
        {
            var agg = aggCombo?.SelectedValue?.ToString();
            bool isNumeric = CheckField(field);

            // بدون تجمیع: خود ستون
            if (string.IsNullOrEmpty(agg))
                return $" {field} ";

            // COUNT روی 1 (برای همه‌ی انواع امن است)
            if (agg.Equals("COUNT", StringComparison.OrdinalIgnoreCase))
                return $" COUNT(1) AS {field}";

            // SUM/AVG/MIN/MAX: اگر عددیِ-متنی بود، داخل تجمیع تبدیل می‌کنیم
            if (isNumeric)
                return $" {agg}({TryCastNumeric(field)}) AS {field}";

            // بقیه‌ی موارد متنی
            return $" {agg}({field}) AS {field}";
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
        private void CreateSum()
        {
            sqlsum = "SELECT ";
            int i = 8, k = 0, wc = 0;

            while (i <= SQLT.Length)
            {
                if (i == SQLT.Length || SQLT[i - 1] == ',')
                {
                    string fieldName = "";
                    if (i - k > 0 && k > 0)
                    {
                        if (sqlsum == "SELECT ")
                            fieldName = SQLT.Substring(i - k, k).Trim();
                        else if (i - k + 2 > 0 && k - 2 > 0)
                            fieldName = SQLT.Substring(i - k + 2, k - 2).Trim();
                    }

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        if (sqlsum != "SELECT ") sqlsum += " ,";
                        if (CheckField(fieldName))
                            sqlsum += $"SUM({TryCastNumeric(fieldName)}) AS {fieldName}";
                        else
                            sqlsum += $" ' ' AS {fieldName} ";
                        wc++;
                    }
                    k = 0;
                }
                else if (i > 3 && i + 2 < SQLT.Length)
                {
                    string asCheck = SQLT.Substring(i - 2, 3);
                    if (asCheck == " AS")
                    {
                        k = 0; i += 2;
                        while (i <= SQLT.Length && (i == SQLT.Length || SQLT[i - 1] != ',')) { i++; k++; }

                        string aliasName = "";
                        if (i - k > 0 && k > 0) aliasName = SQLT.Substring(i - k, k).Trim();

                        if (!string.IsNullOrEmpty(aliasName))
                        {
                            if (sqlsum != "SELECT ") sqlsum += " ,";
                            if (CheckField(aliasName))
                                sqlsum += $"SUM({TryCastNumeric(aliasName)}) AS {aliasName}";
                            else
                                sqlsum += $" ' ' AS {aliasName} ";
                        }
                        k = 0;
                    }
                }
                i++; k++;
            }

            // تهِ SELECT
            if (i >= SQLT.Length)
            {
                if (wc == 0)
                {
                    string lastField = "";
                    if (i - k + 1 > 0 && k > 0)
                        lastField = SQLT.Substring(i - k + 1, k).Trim();

                    if (CheckField(lastField))
                        sqlsum += $" SUM({TryCastNumeric(lastField)}) AS {lastField} FROM ({SQLSTA}) DERIVEDTBL";
                    else
                        sqlsum += (string.IsNullOrEmpty(lastField))
                            ? $" FROM ({SQLSTA}) DERIVEDTBL"
                            : $" ' ' AS {lastField} FROM ({SQLSTA}) DERIVEDTBL";
                }
                else
                {
                    sqlsum += $" FROM ({SQLSTA}) DERIVEDTBL";
                }
            }
            else
            {
                string remainingField = "";
                if (i - k + 2 > 0 && k > 0)
                    remainingField = SQLT.Substring(i - k + 2, k).Trim();

                if (CheckField(remainingField))
                    sqlsum += $" ,SUM({TryCastNumeric(remainingField)}) AS {remainingField} FROM ({SQLSTA}) DERIVEDTBL";
                else
                    sqlsum += $" , ' ' AS {remainingField} FROM ({SQLSTA}) DERIVEDTBL";
            }
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
            SHART = "";

            // NUMBER field
            var numberTextBox = FindName("NUMBER") as NumericTextBox;
            var numberOp = FindName("NUMBERB") as ComboBox;
            if (!string.IsNullOrEmpty(numberTextBox?.Text))
            {
                SHART += $"(NUMBER {numberOp?.SelectedValue ?? "="} {numberTextBox.Text})";
            }

            // OSTANID field
            var ostanidCombo = FindName("OSTANID") as ComboBox;
            var ostanidOp = FindName("OSTANIDB") as ComboBox;
            if (ostanidCombo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(OSTANID {ostanidOp?.SelectedValue ?? "="} {ostanidCombo.SelectedValue})";
            }

            // SHAHRID field
            var shahridCombo = FindName("SHAHRID") as ComboBox;
            var shahridOp = FindName("SHAHRIDB") as ComboBox;
            if (shahridCombo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(SHAHRID {shahridOp?.SelectedValue ?? "="} {shahridCombo.SelectedValue})";
            }

            // ROUTE_NAME field
            var routeNameTextBox = FindName("ROUTE_NAME") as TextBox;
            var routeNameOp = FindName("ROUTE_NAMEB") as ComboBox;
            if (!string.IsNullOrEmpty(routeNameTextBox?.Text))
            {
                ChShart();
                string operatorValue = routeNameOp?.SelectedValue?.ToString() ?? "=";
                switch (operatorValue)
                {
                    case "=":
                        SHART += $"(ROUTE_NAME = '{routeNameTextBox.Text}')";
                        break;
                    case "<>":
                        SHART += $"(ROUTE_NAME <> '{routeNameTextBox.Text}')";
                        break;
                    case SHAMEL: //شامل
                        SHART += $"(ROUTE_NAME like '%{routeNameTextBox.Text}%')";
                        break;
                    case BEDUNE: //بدون
                        SHART += $"(ROUTE_NAME not like '%{routeNameTextBox.Text}%')";
                        break;
                }
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
                    if (string.IsNullOrEmpty(dateToTextBox?.Text?.Replace("/", "").Trim()))
                    {
                        return;
                    }
                    ChShart();
                    SHART += $"(DATE_N BETWEEN {dateTextBox.Text} AND {dateToTextBox.Text})";
                }
                else
                {
                    ChShart();
                    SHART += $"(DATE_N {dateOp?.SelectedValue ?? "="} {dateTextBox.Text})";
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

            // Continue with all other fields...
            // [Rest of CreateShart implementation remains the same as before]

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
            var hesTextBox = FindName("HES") as TextBox;
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
            var mablTextBox = FindName("mabl") as NumericTextBox;
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

            //// col1 field
            //var col1Combo = FindName("col1") as ComboBox;
            //var col1Op = FindName("col1B") as ComboBox;
            //if (col1Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col1 {col1Op?.SelectedValue ?? "="} {col1Combo.SelectedValue})";
            //}

            //// col2 field
            //var col2Combo = FindName("col2") as ComboBox;
            //var col2Op = FindName("col2B") as ComboBox;
            //if (col2Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col2 {col2Op?.SelectedValue ?? "="} {col2Combo.SelectedValue})";
            //}

            //// col3 field
            //var col3Combo = FindName("col3") as ComboBox;
            //var col3Op = FindName("col3B") as ComboBox;
            //if (col3Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col3 {col3Op?.SelectedValue ?? "="} {col3Combo.SelectedValue})";
            //}

            //// col4 field
            //var col4Combo = FindName("col4") as ComboBox;
            //var col4Op = FindName("col4B") as ComboBox;
            //if (col4Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col4 {col4Op?.SelectedValue ?? "="} {col4Combo.SelectedValue})";
            //}

            //// col5 field
            //var col5Combo = FindName("col5") as ComboBox;
            //var col5Op = FindName("col5B") as ComboBox;
            //if (col5Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col5 {col5Op?.SelectedValue ?? "="} {col5Combo.SelectedValue})";
            //}

            //// col6 field
            //var col6Combo = FindName("col6") as ComboBox;
            //var col6Op = FindName("col6B") as ComboBox;
            //if (col6Combo?.SelectedValue != null)
            //{
            //    ChShart();
            //    SHART += $"(col6 {col6Op?.SelectedValue ?? "="} {col6Combo.SelectedValue})";
            //}

            // col7 field
            var col7Combo = FindName("col7") as ComboBox;
            var col7Op = FindName("col7B") as ComboBox;
            if (col7Combo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(col7 {col7Op?.SelectedValue ?? "="} {col7Combo.SelectedValue})";
            }

            // col8 field
            var col8Combo = FindName("col8") as ComboBox;
            var col8Op = FindName("col8B") as ComboBox;
            if (col8Combo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(col8 {col8Op?.SelectedValue ?? "="} {col8Combo.SelectedValue})";
            }

            // col9 field
            var col9Combo = FindName("col9") as ComboBox;
            var col9Op = FindName("col9B") as ComboBox;
            if (col9Combo?.SelectedValue != null)
            {
                ChShart();
                SHART += $"(col9 {col9Op?.SelectedValue ?? "="} {col9Combo.SelectedValue})";
            }

            if (!string.IsNullOrEmpty(SHART))
            {
                SHART = $"({SHART})";
            }
        }
        private void CreateField()
        {
            SQLT = "SELECT ";
            grbCOL = "";

            void Add(string field, CheckBox cb, ComboBox agg)
            {
                if (cb?.IsChecked == true)
                {
                    ChField();
                    SQLT += FormatAgg(field, agg);
                }
            }

            // ساده‌ها
            Add("NUMBER", NUMBERC, NUMBERBS);
            Add("TAGCODE", TAGCODEC, TAGCODEBS);
            Add("DATE_N", DATE_NC, DATE_NBS);
            Add("CODE", CODEC, CODEBS);
            Add("KALA", KALAC, KALABS);

            // عددی‌ها (NVARCHAR در DB → تبدیل داخل Aggregation)
            Add("MEGH", MEGHC, MEGHBS);
            Add("MEGHk", MEGHkC, MEGHkBS);
            Add("MABL", MABLC, MABLBS);
            Add("MABL_K", MABL_KC, MABL_KBS);
            Add("KHFR", KHFRC, KHFRBS);
            Add("GHFR", GHFRC, GHFRBS);
            Add("N_KOL", N_KOLC, N_KOLBS);
            Add("N_MOIN", N_MOINC, N_MOINBS);
            Add("IMBAA", IMBAAC, IMBAABS);

            Add("N_TAF", N_TAFC, N_TAFBS);
            Add("TOTALARZ", TOTALARZC, TOTALARZBS);
            Add("TAMIR", TAMIRC, TAMIRBS);
            Add("FNUMCO", FNUMCOC, FNUMCOBS);
            Add("NUMBER1", NUMBER1C, NUMBER1BS);
            Add("MEGH_MAR", MEGH_MARC, MEGH_MARBS);
            Add("ANBARCODE", ANBARCODEC, ANBARCODEBS);
            Add("N_S", N_SC, N_SBS);
            Add("SHIFT_ID", SHIFT_IDC, SHIFT_IDBS);
            Add("DEPATMAN", DEPATMANC, DEPATMANBS);
            Add("CUST_COD", CUST_CODC, CUST_CODBS);
            Add("MAS", MASC, MASBS);
            Add("N_RASID", N_RASIDC, N_RASIDBS);
            Add("MM", MMC, MMBS);
            Add("MIN_M", MIN_MC, MIN_MBS);
            Add("MAX_M", MAX_MC, MAX_MBS);
            Add("N_SEF", N_SEFC, N_SEFBS);
            Add("B_SEF", B_SEFC, B_SEFBS);
            Add("MABL_F", MABL_FC, MABL_FBS);
            Add("AVRAGE", AVRAGEC, AVRAGEBS);
            Add("MABRIAL", MABRIALC, MABRIALBS);
            Add("VAZN", VAZNC, VAZNBS);
            Add("TKHN", TKHNC, TKHNBS);

            // متنی‌ها
            Add("CUSTNAME", CUSTNAMEC, CUSTNAMEBS);
            Add("HES", hesC, hesBS);
            Add("MOLAH", MOLAHC, MOLAHBS);
            Add("SHARAYET", SHARAYETC, SHARAYETBS);
            Add("N_FANI", N_FANIC, N_FANIBS);
            Add("USER_NAME", USER_NAMEC, USER_NAMEBS);
            Add("ROUTE_NAME", ROUTE_NAMEC, ROUTE_NAMEBS);

            // ستون‌های سفارشی col1..col9 (مثل قبل + نام نمایشی colN*)
            void AddCol(string col, CheckBox cb, ComboBox agg, string coln)
            {
                if (cb?.IsChecked == true)
                {
                    ChField(); SQLT += FormatAgg(col, agg);
                    ChField(); SQLT += $" {coln} ";
                    grbCOL += "," + coln;
                }
            }
            //AddCol("COL1", col1C, col1BS, "COLN1");
            //AddCol("COL2", col2C, col2BS, "COLN2");
            //AddCol("COL3", col3C, col3BS, "COLN3");
            //AddCol("COL4", col4C, col4BS, "COLN4");
            //AddCol("COL5", col5C, col5BS, "COLN5");
            //AddCol("COL6", col6C, col6BS, "COLN6");
            AddCol("COL7", col7C, col7BS, "COLN7");
            AddCol("COL8", col8C, col8BS, "COLN8");
            AddCol("COL9", col9C, col9BS, "COLN9");

            // استان/شهر مثل قبل + اضافه‌کردن نام
            if (OSTANIDC?.IsChecked == true)
            {
                ChField(); SQLT += FormatAgg("OSTANID", OSTANIDBS);
                ChField(); SQLT += " OSNAME "; grbCOL += ",OSNAME";
            }
            if (SHAHRIDC?.IsChecked == true)
            {
                ChField(); SQLT += FormatAgg("SHAHRID", SHAHRIDBS);
                ChField(); SQLT += " CITYNAME "; grbCOL += ",CITYNAME";
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

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            //ذخیره گزارش
            GoFinalProccess(true);

            ChangeIsHappend = false;
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

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
        private void GoFinalProccess(bool _isSave_ = false)
        {
            KALAS_MAIN_ADVANCE KMA = default;
            try
            {
                CreateField();
                CreateShart();

                if (string.IsNullOrEmpty(SQLT) || SQLT == "SELECT ")
                {
                    universControl.PopNotifyShowUp("هیچ فیلدی انتخاب (تیک) نشده!", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                    return;
                }

                SQLSTA = SQLT + " FROM KALAS " + (string.IsNullOrEmpty(SHART) ? "" : " WHERE " + SHART) + (grb != "Group By " ? " " + grb : "");

                if (RSUM.IsChecked ?? false)
                {
                    ////فعلا جمع رو از طریق خود SfDataGrid انجام میدیم
                    //CreateSum();
                    //SQLSTAFIN = SQLSTA + "  UNION ALL  " + sqlsum;
                }
                else
                {
                    SQLSTAFIN = SQLSTA;
                }

                if (SQLSTA != "SELECT  FROM KALAS ")
                {
                    SQLSTAFIN = SQLSTA + SQLSTAFIN + " OPTION (FORCE ORDER, LOOP JOIN, HASH JOIN, ORDER GROUP)";

                    if (_isSave_)
                    {
                        SAVE_REPORT SVR = new SAVE_REPORT();
                        SVR.OpenArgs = SQLSTAFIN;
                        SVR.ShowDialog();
                    }

                    var SelectedColumns = SqlColumnParser.ExtractColumnNames(SQLT);

                    KMA = new KALAS_MAIN_ADVANCE();
                    KMA.SqlQueryPassed = SQLSTAFIN;
                    KMA.ColumnSelectedPassed = SelectedColumns;

                    KMA.isSummed = RSUM.IsChecked ?? false; //جمع زیر گزارش

                    KMA.Show();
                }
                else
                {
                    universControl.PopNotifyShowUp("چيزي براي ذخيره وجود ندارد!", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
            }
            catch (Exception)
            {
                try { if (KMA != null) { KMA?.Close(); } } catch { }
                new Msgwin(false, "خطا در انجام عملیات").Show();
                return;
            }

        }

        private void ANDOR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            //ANDOR_AfterUpdate
            if (!string.IsNullOrEmpty(SHART) && ANDOR.SelectedValue != null)
            {
                if (ANDOR.SelectedValue == "و")
                {
                    SHART = SHART + " AND ";
                }
                else
                {
                    SHART = SHART + " OR ";
                }

                ResetDefaultUi();
                CreateShart();
            }
        }

        private void OSTANID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (OSTANID.SelectedValue is not null)
            {
                SHAHRID.IsEnabled = true;
                //شهرستان
                SHAHRID.ItemsSource = dbms.DoGetDataSQL<TCOD_CITY>($"SELECT CITYCODE, CITYNAME FROM TCOD_CITY WHERE OSCODE = {OSTANID.SelectedValue} ORDER BY CITYNAME").ToList();
            }
            else
            {
                SHAHRID.IsEnabled = false;
            }
        }

        private void BTN_OPENREPORT_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}