using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.Checkha.NAZDESANDOGH
{
    /// <summary>
    /// Interaction logic for VOSULDIALOG2.xaml
    /// </summary>
    public partial class VOSULDIALOG2 : Window
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
        public VOSULDIALOG2(Visual _WIN_, string _WinName_, string _OpenArgs_)
        {
            InitializeComponent();

            this.DataContext = this;


            OpenArgs = _OpenArgs_;
            WinName = _WinName_;
            WIN = _WIN_;
        }

        #region LOCALMODEL
        public class VOSUL2_MODEL_QRE2
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }
            public int? Expr1 { get; set; }
        }
        public class VOSUL_MODEL_QRE1
        {
            public long? DATE { get; set; }
            public string? MOLAH { get; set; }
            public double? N_S { get; set; }
            public int? IDH { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public int? RADIF { get; set; }
            public int? N_MOIN { get; set; }
            public int? N_TAF { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public double? MABL { get; set; }
            public int? N_KOL { get; set; }
            public int? KIND { get; set; }
            public string? HES1 { get; set; }
        }
        #endregion

        List<NAZDBANK_D_MODEL> RecordsChecks = new List<NAZDBANK_D_MODEL>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

        public bool NowIsReady { get; private set; }
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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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

        public string OpenArgs { get; }
        public string WinName { get; }
        public Visual WIN { get; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VOSULDIALOG2 = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            if (WinName == "CHEK_VOSUL_LES")
            {
                HHMOIN.IsEnabled = false;
            }
            else
            {
                HHMOIN.IsEnabled = true;
            }

            //#Check
            //if (Screen.ActiveForm.NAME == "CHEK_VOSUL_LES")
            //{
            //    DTS.Text = CL_HESABDARI.ADDDAY(Forms["CHEK_VOSUL_LES"]["CHEK_VLIST"]["DATE_S"], 1);
            //}

            HHMOIN.ItemsSource = dbms.DoGetDataSQL<VOSUL2_MODEL_QRE2>("SELECT CAST(N_KOL AS nvarchar) + N'-' + CAST(NUMBER AS nvarchar) + N'-' + CAST(TNUMBER AS nvarchar) AS hes, NAME, TNUMBER AS Expr1 FROM TDETA_HES WHERE     ( ((TDETA_HES.N_KOL) =" + Baseknow.BANKHA + "))").ToList();

            if (!string.IsNullOrEmpty(OpenArgs))
            {
                HHMOIN.SelectedValue = OpenArgs;
                HHMOIN.Items.Refresh();
            }

            DTS.Text = Tarikh.FullCurrentDate;
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
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (!IS_DATE_VALID(DTS.Text.ToRawTarikh()))
            {
                return;
            }

            if (HHMOIN.SelectedValue == null)
            {
                if (CL_HESABDARI.ISHESAB3(HHMOIN.Text))
                {
                    HHMOIN.SelectedValue = HHMOIN.Text; HHMOIN.Items.Refresh();
                }
                else
                {
                    new Msgwin(false, "اين حساب وجود ندارد").ShowDialog();
                    return;
                }

                new Msgwin(false, "حسابي كه چك بايد به آن وصول شود مشخص نگرديده است...!").ShowDialog();
                return;
            }
            else if (CL_HESABDARI.ISTAF(HHMOIN.SelectedValue.ToStringNullSafe()))
            {
                new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!").ShowDialog();
                return;
            }

            if (WIN is CHEK_VLISTALL)
            {
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.RD_SARRESID_DT_1 = RD_SARRESID_DT.IsChecked ?? false;
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.RD_AFTER_SARRESID_DT_2 = RD_AFTER_SARRESID_DT.IsChecked ?? false;
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.RD_ZEIL_DT_3 = RD_ZEIL_DT.IsChecked ?? false;

                if (string.IsNullOrEmpty(DTC.Text) || DTC.Text == "0") DTC.Text = "1";
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.NUMBERIC_DTC_DAY_2 = Convert.ToInt32(DTC.Text);
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.DTS_DATE = DTS.Text.ToRawTarikh();
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.HHMOIN_VOSUL = HHMOIN.SelectedValue.ToStringNullSafe();
            }

            this.Close();
        }

        public Visual I_AM_VOSULDIALOG2 { get; private set; }
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
        private void HHMOIN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHMOIN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox _THE_TEXTBOX_ = (TextBox)HHMOIN.Template.FindName("PART_EditableTextBox", HHMOIN);
            if (HHMOIN.SelectedValue is not null)
            {
                if ((HHMOIN?.SelectedItem as VOSUL2_MODEL_QRE2)?.NAME == HHMOIN?.Text)
                {
                    return;
                }
            }
            if (string.IsNullOrEmpty(_THE_TEXTBOX_.Text) || string.IsNullOrWhiteSpace(_THE_TEXTBOX_.Text))
            {
                universControl.PopNotifyShow("مقدار وارد شده خالی است", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            CL_LMethods.GetSearchedValueCustomer(HHMOIN, "VOSULDIALOG2", default, dbms, I_AM_VOSULDIALOG2, false);

            if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
            {
                HHMOIN.SelectedValue = HESAB_FROM_SEARCH.FULL_HES;
            }
            else
            {
                HHMOIN.SelectedValue = null;

                universControl.PopNotifyShow("حسابی صحیح انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1);
            }

            HHMOIN.Items.Refresh();
            HESAB_FROM_SEARCH.DoClear();
        }

        private bool IS_DATE_VALID(string _DATE_)
        {
            string date_n_val = _DATE_.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }

            return true;
        }

        private void DTS_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IS_DATE_VALID(DTS.Text.ToRawTarikh()))
            {
                e.Handled = true;
            }
        }
        private void serial_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //RecordsChecks
        }

    }
}
