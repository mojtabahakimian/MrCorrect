using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class WIN_LIST_KALA : Window
    {
        public WIN_LIST_KALA(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
            }
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<LIST_KALA_MODEL> LIST_KALA_DATA { get; set; } = new ObservableCollection<LIST_KALA_MODEL>();
        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        #region LOCALMODEL
        public class QRE_CTRL5
        {
            public double? TAG { get; set; }
            public double? Expr1 { get; set; }
            public double? NUMBER { get; set; }
            public double? Expr2 { get; set; }
            public string? CUST_NO { get; set; }
            public string? Expr3 { get; set; }
        }
        #endregion

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
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            FILL_ALL_COMBOBOXES();

            DT1.Text = "11111111";
            DT2.Text = "99999999";
            CheckDataConsistency();
            ReGetData();
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
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        private void CheckDataConsistency()
        {
            var sql = @"SELECT HEAD_LST_1.TAG, dbo.HEAD_LST.TAG AS Expr1, dbo.HEAD_LST.NUMBER,
                                   HEAD_LST_1.NUMBER AS Expr2, HEAD_LST_1.CUST_NO, dbo.HEAD_LST.CUST_NO AS Expr3
                            FROM dbo.HEAD_LST
                            INNER JOIN dbo.HEAD_LST HEAD_LST_1 ON dbo.HEAD_LST.NUMBER = HEAD_LST_1.NUMBER
                                AND dbo.HEAD_LST.CUST_NO <> HEAD_LST_1.CUST_NO
                            WHERE (HEAD_LST_1.TAG = 12) AND (dbo.HEAD_LST.TAG = 1)";
            var result = dbms.DoGetDataSQL<QRE_CTRL5>(sql).ToList();
            if (result.Count > 0)
            {
                new Msgwin(false, "فاکتور خرید با رسیدها در نام مشتری مغایرت دارد\nلطفا ابتدا مغایرت را اصلاح کنید").ShowDialog();
                // In the original VBA, it opens "mesag" and "R_F_MO" forms
                // You may need to adjust this based on your application structure
                // DoCmd.OpenForm "mesag", , , , , acDialog, "..."
                // DoCmd.OpenForm "R_F_MO", acFormDS
            }
        }
        private void ReGetData()
        {
            Process Prc = ProcLoader.Start();

            LIST_KALA_DATA.Clear();
            var dt1 = DT1.Text.ToRawTarikh();
            var dt2 = DT2.Text.ToRawTarikh();
            if (string.IsNullOrWhiteSpace(dt1) || string.IsNullOrWhiteSpace(dt2))
            {
                return;
            }
            // SQL query based on the VBA code
            // Note: The product codes in the VBA are hardcoded ('378', '375')
            // In the original Access form RecordSource, different codes were used ('2952', '3357', '3359', '3358')
            // Using the codes from dt1_AfterUpdate/dt2_AfterUpdate methods
            var sql = @"SELECT TOP 100 PERCENT
                               dbo.CUST_HESAB.NAME,
                               dbo.STUF_DEF.NAME AS kala,
                               dbo.INVO_LST.TAG,
                               dbo.TAGCOD.BARGAH,
                               dbo.INVO_LST.CODE,
                               dbo.HEAD_LST.CUST_NO,
                               SUM(dbo.INVO_LST.MEGH_MAR) AS mar,
                               SUM(dbo.INVO_LST.MEGHk) AS mrgh
                            FROM dbo.INVO_LST
                            INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER
                                AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG
                            INNER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                            INNER JOIN dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE
                            INNER JOIN dbo.TAGCOD ON dbo.INVO_LST.TAG = dbo.TAGCOD.CODE
                            WHERE (dbo.HEAD_LST.DATE_N >= @DT1)
                              AND (dbo.HEAD_LST.DATE_N <= @DT2)
                              AND (INVO_LST.CODE=N'2952' OR INVO_LST.CODE=N'3357' OR INVO_LST.CODE=N'3359' OR INVO_LST.CODE=N'3358')
                            GROUP BY dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_NO, dbo.CUST_HESAB.NAME,
                                     dbo.INVO_LST.TAG, dbo.STUF_DEF.NAME, dbo.TAGCOD.BARGAH
                            ORDER BY dbo.CUST_HESAB.NAME, dbo.STUF_DEF.NAME, dbo.INVO_LST.TAG";

            var rows = dbms.DoGetDataSQL<LIST_KALA_MODEL>(sql, new { DT1 = dt1, DT2 = dt2 }).ToList();
            foreach (var item in rows)
            {
                LIST_KALA_DATA.Add(item);
            }

            RecordCountText.Text = rows.Count.ToString();
            TotalMarText.Text = (LIST_KALA_DATA.Sum(r => r.mar) ?? 0).ToString("#,0");
            TotalMrghText.Text = (LIST_KALA_DATA.Sum(r => r.mrgh) ?? 0).ToString("#,0");

            TextHasChanged = false;

            ProcLoader.Stop(Prc);
        }

        bool TextHasChanged = false;
        private void DT1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextHasChanged = true;
        }
        private void DT2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextHasChanged = true;
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

        private void DT1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextHasChanged && !string.IsNullOrWhiteSpace(DT1.Text.ToRawTarikh()) && !string.IsNullOrWhiteSpace(DT2.Text.ToRawTarikh()))
            {
                ReGetData();
            }
        }

        private void DT2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TextHasChanged && !string.IsNullOrWhiteSpace(DT1.Text.ToRawTarikh()) && !string.IsNullOrWhiteSpace(DT2.Text.ToRawTarikh()))
            {
                ReGetData();
            }
        }
    }

}
