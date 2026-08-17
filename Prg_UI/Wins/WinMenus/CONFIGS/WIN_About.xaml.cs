using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_SendInvoice.CNNMANAGER;
using ScriptSqly.Migrations;
using Prg_UI.UiTools;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinSetting;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_About : Window
    {
        public WIN_About()
        {
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        private SAZMAN _sazmanData; // برای نگهداری اطلاعات خوانده شده

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

                bool isReadOnly = !ican;
                BTN_SAVE.IsEnabled = ican;

                UNIVERSITY_CO.IsReadOnly = isReadOnly;
                NAME.IsReadOnly = isReadOnly;
                IYALAT.IsReadOnly = isReadOnly;
                CITY.IsReadOnly = isReadOnly;
                ECODE.IsReadOnly = isReadOnly;
                PCODE.IsReadOnly = isReadOnly;
                MCODEM.IsReadOnly = isReadOnly;
                MANAGER.IsReadOnly = isReadOnly;
                MOAVEN.IsReadOnly = isReadOnly;
                ZIHESAB.IsReadOnly = isReadOnly;
                AMINAMVAL.IsReadOnly = isReadOnly;

                SERVERNAM.IsReadOnly = isReadOnly;
                Whether.IsReadOnly = isReadOnly;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //
                string Formname = "ABOUT";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            VersionLabel.Text = CL_VERSION.MrCorrectFullVersion;

            FILL_ALL_COMBOBOXES();

            LoadSazmanData();

            AllowEdits = false;

        }
        private void LoadSazmanData()
        {
            var result = dbms.DoGetDataSQL<SAZMAN>("SELECT TOP 1 * FROM SAZMAN");
            _sazmanData = result.FirstOrDefault();
            if (_sazmanData != null)
            {
                UNIVERSITY_CO.Text = _sazmanData.UNIVERSITY_CO.ToString();
                NAME.Text = _sazmanData.NAME;
                IYALAT.Text = _sazmanData.IYALAT;
                CITY.Text = _sazmanData.CITY;
                ECODE.Text = _sazmanData.ECODE;
                PCODE.Text = _sazmanData.PCODE;
                MCODEM.Text = _sazmanData.MCODEM;
                MANAGER.Text = _sazmanData.MANAGER;
                MOAVEN.Text = _sazmanData.MOAVEN;
                ZIHESAB.Text = _sazmanData.ZIHESAB;
                AMINAMVAL.Text = _sazmanData.AMINAMVAL;
                SERVERNAM.Text = _sazmanData.SERVERNAM;
                Whether.Text = _sazmanData.Whether;
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        private void BTN_ESLAH_Click(object sender, RoutedEventArgs e)
        {
            AllowEdits = true;
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!AllowEdits || _sazmanData == null) return;

            string sql = @"UPDATE SAZMAN SET 
                UNIVERSITY_CO = @UNIVERSITY_CO, NAME = @NAME, IYALAT = @IYALAT, CITY = @CITY,
                ECODE = @ECODE, PCODE = @PCODE, MCODEM = @MCODEM, MANAGER = @MANAGER,SERVERNAM=@SERVERNAM,Whether=@Whether,
                MOAVEN = @MOAVEN, ZIHESAB = @ZIHESAB, AMINAMVAL = @AMINAMVAL";
            var parameters = new
            {
                UNIVERSITY_CO = UNIVERSITY_CO.Text,
                NAME = NAME.Text,
                IYALAT = IYALAT.Text,
                CITY = CITY.Text,
                ECODE = ECODE.Text,
                PCODE = PCODE.Text,
                MCODEM = MCODEM.Text,
                MANAGER = MANAGER.Text,
                MOAVEN = MOAVEN.Text,
                ZIHESAB = ZIHESAB.Text,
                AMINAMVAL = AMINAMVAL.Text,
                SERVERNAM = SERVERNAM.Text,
                Whether = Whether.Text,
            };
            try
            {
                dbms.DoExecuteSQL(sql, parameters);
                new Msgwin(false, "تغییرات با موفقیت ذخیره شد").ShowDialog();
                AllowEdits = false;
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره اطلاعات");
                return;
            }

            ChangeIsHappend = false;
        }

        private void Command41_Click(object sender, RoutedEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از اجرای اسکریپت اطمینان دارید؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            ScriptSqly.Migrations.ScriptSqly.LetsGo(CL_CCNNMANAGER.CONNECTION_STR, true);
            new Msgwin(false, "اسکریپت‌ها اجرا شدند.").Show();
        }

        private void Command39_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Command43_Click(object sender, RoutedEventArgs e)
        {
            //اصلاح کد پیج
            dbms.OpenStoredProcedure("CODEPAGE");
            dbms.OpenStoredProcedure("CODEPAGE2");
            new Msgwin(false, "پروسیجرهای اصلاح کدپیج اجرا شدند.").Show();
        }

        private void Command38_Click(object sender, RoutedEventArgs e)
        {
            CL_LOCKWATCH Lockwatch = new CL_LOCKWATCH();
            if (Lockwatch.GoCheck() == false)
            {
                //
            }
        }

        private void Command42_Click(object sender, RoutedEventArgs e)
        {
            //تبدیل 1400
        }

        private void SERVERNAM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SERVERNAM.Text.Trim()))
            {
                SERVERNAM.Text = _sazmanData.SERVERNAM;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از اجرای اسکریپت حقوق اطمینان دارید؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            ScriptSqly.Migrations.ScriptSqly.LetsGo(CL_CCNNMANAGER.CONNECTION_STR, false, 2); //حقوق و دستمزد
            new Msgwin(false, "اسکریپت حقوق اجرا شدند.").Show();
        }
    }
}
