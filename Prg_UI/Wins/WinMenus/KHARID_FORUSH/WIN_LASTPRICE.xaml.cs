using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    /// <summary>
    /// Interaction logic for WIN_LASTPRICE.xaml
    /// </summary>
    public partial class WIN_LASTPRICE : Window
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private bool _isInternalSync;
        private readonly string? _initialCode;
        private readonly string? _initialCustNo;

        public WIN_LASTPRICE(string? initialCode = null, string? initialCustNo = null)
        {
            _initialCode = initialCode;
            _initialCustNo = initialCustNo;
            InitializeComponent();
            this.DataContext = this;

        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(GetType().Name);

            FILL_ALL_COMBOBOXES();

            if (!string.IsNullOrWhiteSpace(_initialCode))
            {
                CODE.SelectedValue = _initialCode;
                CODE.SelectedValue = _initialCode;
            }

            if (!string.IsNullOrWhiteSpace(_initialCustNo))
            {
                HMOIN1.SelectedValue = _initialCustNo;
                HHMOIN1.SelectedValue = _initialCustNo;
            }

            DT.Text = Tarikh.SlashyFullDate;
            ReGetData();
        }


        private void ReGetData()
        {
            try
            {
                var code = GetCodeValue();
                var custNo = GetCustomerValue();

                if (string.IsNullOrWhiteSpace(code))
                {
                    ResetValues();
                    return;
                }

                LoadMainPrices(code, custNo);
                LoadHistory(code, custNo);
            }
            catch (Exception ex)
            {
                new Msgwin(false, ex.Message).ShowDialog();
            }
        }

        private void LoadMainPrices(string code, string custNo)
        {
            var lastSaleAny = dbms.DoGetDataSQL<PriceAndDateRow>(@"
                    SELECT TOP 1
                        I.MABL,
                        H.DATE_N AS MaxOfDATE_N
                    FROM dbo.HEAD_LST AS H
                    INNER JOIN dbo.INVO_LST AS I
                        ON H.TAG = I.TAG
                       AND H.NUMBER = I.NUMBER
                    WHERE I.TAG = @TagSale
                      AND I.CODE = @CODE
                    ORDER BY H.DATE_N DESC",
                    new { TagSale = 2, CODE = code }).FirstOrDefault();

            var lastBuyAny = dbms.DoGetDataSQL<PriceAndDateRow>(@"
                    SELECT TOP 1
                        I.MABL,
                        H.DATE_N AS MaxOfDATE_N
                    FROM dbo.HEAD_LST AS H
                    INNER JOIN dbo.INVO_LST AS I
                        ON H.TAG = I.TAG
                       AND H.NUMBER = I.NUMBER
                    WHERE I.TAG = @TagBuy
                      AND I.CODE = @CODE
                    ORDER BY H.DATE_N DESC",
                    new { TagBuy = 1, CODE = code }).FirstOrDefault();

            var firstPrice = dbms.DoGetDataSQL<FirstPriceRow>(@"
                    SELECT TOP 1 FI_A
                    FROM dbo.STUF_FSK
                    WHERE CODE = @CODE",
                    new { CODE = code }).FirstOrDefault();

            var lastSaleToCustomer = dbms.DoGetDataSQL<PriceAndDateRow>(@"
                    SELECT TOP 1
                        I.MABL,
                        H.DATE_N AS MaxOfDATE_N
                    FROM dbo.HEAD_LST AS H
                    INNER JOIN dbo.INVO_LST AS I
                        ON H.TAG = I.TAG
                       AND H.NUMBER = I.NUMBER
                    WHERE I.TAG = @TagSale
                      AND I.CODE = @CODE
                      AND H.CUST_NO = @CUST_NO
                    ORDER BY H.DATE_N DESC",
                    new { TagSale = 2, CODE = code, CUST_NO = custNo }).FirstOrDefault();

            LFP.Text = (lastSaleAny?.MABL ?? 0m).ToString("N0");
            LKP.Text = (lastBuyAny?.MABL ?? 0m).ToString("N0");
            LP.Text = (firstPrice?.FI_A ?? 0m).ToString("N0");
            LFPM.Text = (lastSaleToCustomer?.MABL ?? 0m).ToString("N2");

            if (lastSaleToCustomer?.MaxOfDATE_N != null && lastSaleToCustomer.MaxOfDATE_N > 0)
            {
                DT.Text = lastSaleToCustomer.MaxOfDATE_N.Value.ToString();
            }
        }

        private void LoadHistory(string code, string custNo)
        {
            DGR_SaleHistory.ItemsSource = dbms.DoGetDataSQL<HistoryRow>(@"
                    SELECT TOP 200
                        H.NUMBER,
                        H.DATE_N,
                        H.CUST_NO,
                        ISNULL(C.NAME, N'') AS CUST_NAME,
                        I.MEGHk,
                        I.MABL_K,
                        I.MABL
                    FROM dbo.HEAD_LST AS H
                    INNER JOIN dbo.INVO_LST AS I
                        ON H.TAG = I.TAG
                       AND H.NUMBER = I.NUMBER
                    LEFT JOIN dbo.CUST_HESAB AS C
                        ON C.hes = H.CUST_NO
                    WHERE I.TAG = @TagSale
                      AND I.CODE = @CODE
                      AND (@CUST_NO = N'' OR H.CUST_NO = @CUST_NO)
                    ORDER BY H.DATE_N DESC, H.NUMBER DESC",
                    new { TagSale = 2, CODE = code, CUST_NO = custNo }).ToList();

            DGR_BuyHistory.ItemsSource = dbms.DoGetDataSQL<HistoryRow>(@"
                    SELECT TOP 200
                        H.NUMBER,
                        H.DATE_N,
                        H.CUST_NO,
                        ISNULL(C.NAME, N'') AS CUST_NAME,
                        I.MEGHk,
                        I.MABL,
                        I.MABL_K
                    FROM dbo.HEAD_LST AS H
                    INNER JOIN dbo.INVO_LST AS I
                        ON H.TAG = I.TAG
                       AND H.NUMBER = I.NUMBER
                    LEFT JOIN dbo.CUST_HESAB AS C
                        ON C.hes = H.CUST_NO
                    WHERE I.TAG = @TagBuy
                      AND I.CODE = @CODE
                    ORDER BY H.DATE_N DESC, H.NUMBER DESC",
                    new { TagBuy = 1, CODE = code }).ToList();
        }

        private void ResetValues()
        {
            LFP.Text = "0";
            LKP.Text = "0";
            LP.Text = "0";
            LFPM.Text = "0";
            DGR_SaleHistory.ItemsSource = new List<HistoryRow>();
            DGR_BuyHistory.ItemsSource = new List<HistoryRow>();
        }

        private string GetCodeValue()
        {
            return (CODE.SelectedValue?.ToString() ?? CODE.SelectedValue?.ToString() ?? string.Empty).Trim();
        }
        private string GetCustomerValue()
        {
            return (HMOIN1.SelectedValue?.ToString() ?? HHMOIN1.SelectedValue?.ToString() ?? string.Empty).Trim();
        }

        private void CODE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalSync) return;
            _isInternalSync = true;
            //CODE.SelectedValue = CODE.SelectedValue;
            _isInternalSync = false;
            ReGetData();
        }

        private void Combo13_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalSync) return;
            _isInternalSync = true;
            //CODE.SelectedValue = CODE.SelectedValue;
            _isInternalSync = false;
            ReGetData();
        }

        private void HMOIN1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalSync) return;
            _isInternalSync = true;
            HHMOIN1.SelectedValue = HMOIN1.SelectedValue;
            _isInternalSync = false;
            ReGetData();
        }

        private void HHMOIN1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalSync) return;
            _isInternalSync = true;
            HMOIN1.SelectedValue = HHMOIN1.SelectedValue;
            _isInternalSync = false;
            ReGetData();
        }

        private void Combo13_LostFocus(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void DT_LostFocus(object sender, RoutedEventArgs e)
        {
            string rawDate = DT.Text.ToRawTarikh();
            if (string.IsNullOrWhiteSpace(rawDate))
            {
                DT.Text = Tarikh.SlashyFullDate;
                return;
            }

            if (!long.TryParse(rawDate, out long dVal))
            {
                new Msgwin(false, "فرمت تاریخ صحیح نیست.").ShowDialog();
                DT.Text = Tarikh.SlashyFullDate;
                return;
            }

            bool hasError = CL_HESABDARI.CHEKDATEM(dVal, Convert.ToBoolean(Baseknow.CTL_DT));
            if (hasError)
            {
                DT.Text = Tarikh.SlashyFullDate;
            }
            else
            {
                DT.Text = dVal.ToString();
            }
        }

        private void Command12_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private sealed class PriceAndDateRow
        {
            public decimal? MABL { get; set; }
            public long? MaxOfDATE_N { get; set; }
        }
        private sealed class FirstPriceRow
        {
            public decimal? FI_A { get; set; }
        }
        private sealed class HistoryRow
        {
            public long NUMBER { get; set; }
            public long DATE_N { get; set; }
            public string CUST_NO { get; set; } = "";
            public string CUST_NAME { get; set; } = "";
            public double MEGHk { get; set; }
            public decimal MABL { get; set; }
            public decimal MABL_K { get; set; }
            public string DATE_N_FM => DATE_N.ToString();
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
            //نام کالا
            var items = dbms.DoGetDataSQL<STUF_DEF>(@"
                    SELECT CODE, NAME
                    FROM dbo.STUF_DEF
                    ORDER BY NAME").ToList();
            CODE.ItemsSource = items;
            Combo13.ItemsSource = CODE.ItemsSource;

            //مشتری ها
            var RST_HES = new List<Custom_CUST_HESAB>();
            foreach (var item in RST_HES)
            {
                if (!string.IsNullOrEmpty(item?.NAME))
                {
                    item.NAME = item.NAME.FixPersianChars();
                }
            }
            HHMOIN1.ItemsSource = RST_HES;
            HHMOIN1.DisplayMemberPath = "NAME";
            HHMOIN1.SelectedValuePath = "hes";

            HMOIN1.ItemsSource = HHMOIN1.ItemsSource;
            HMOIN1.DisplayMemberPath = "hes";
            HMOIN1.SelectedValuePath = "hes";
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

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            //if (!BTN_SAVE.IsEnabled) { return; }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            //if (!ESLAH.IsEnabled) { return; }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CODE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CODE.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CODE.Template.FindName("PART_EditableTextBox", CODE);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (CODE.SelectedValue is not null)
            {
                if ((CODE.SelectedItem as STUF_DEF)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, CUTSNO_TEX.Text);
            if (RST_KALA != null)
            {
                CODE.SelectedValue = RST_KALA.CODE;
            }
            else
            {
                new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                return;
            }
        }

        private void HMOIN1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HMOIN1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)HMOIN1.Template.FindName("PART_EditableTextBox", HMOIN1);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (HMOIN1.SelectedValue is not null)
            {
                if ((HMOIN1.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HMOIN1, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"حساب شخص نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                if (HMOIN1.SelectedValue is not null)
                {
                    if (CL_HESABDARI.ISTAF(HMOIN1.SelectedValue.ToString()))
                    {
                        Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                        msgwin.ShowDialog();
                        HMOIN1.SelectedValue = null;
                    }
                    if (CL_HESABDARI.BLOCKEDCUST(HMOIN1.SelectedValue.ToString()))
                    {
                        HMOIN1.SelectedItem = null;
                        universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }
    }
}
