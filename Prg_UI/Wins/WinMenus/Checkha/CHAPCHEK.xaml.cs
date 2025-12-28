using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Syncfusion.Data.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Stimulsoft.Report.Components;
using System.Drawing;
using Wins.WinOther;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Wins.WinMenus.Checkha
{
    public partial class CHAPCHEK : Window
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

        public CHAPCHEK()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

        public double? NUMBER_TO_OPEN { get; set; }
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
            #region SecuritCheck
            try
            {
                //
                string Formname = "CHAPCHEK";
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

            FILL_ALL_COMBOBOXES();

            #region Form_Open
            //if (IsLoaded("pget_hed"))
            //{
            //    if (Forms["PGET_HED"]["PGET_LST_SUB"]["NO_AM"] == 2 && Forms["PGET_HED"]["PGET_LST_SUB"]["NAHVA"] == 2)
            //    {
            //        rst.Open "SELECT * FROM PAY_GETP WHERE N_SERI = '" + Forms["PGET_HED"]["PGET_LST_SUB"]["N_SERI"] + "' and  BANK = " + Forms["PGET_HED"]["PGET_LST_SUB"]["BANK"] + " and  MABL = " + Forms["PGET_HED"]["PGET_LST_SUB"]["mabl"], CurrentProject.Connection, adOpenKeyset, adLockOptimistic;
            //        if (rst.RecordCount > 0)
            //        {
            //            this.mabl = Forms["PGET_HED"]["PGET_LST_SUB"]["mabl"];
            //            this.DATE_S = rst.Fields["DATE_S"];
            //            this.VAJH = rst.Fields["NAME_TAH"];
            //            this.MOIN = Forms["PGET_HED"]["PGET_LST_SUB"]["BANK"];
            //        }
            //    }
            //}
            #endregion

            date_s.Text = Tarikh.FullCurrentDate;
            date_s.Focus();
            date_s.SelectAll();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_GO.IsFocused)
                {
                    return;
                }

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
            //از حساب
            MOIN.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>($"SELECT CODE, NAMES FROM TCOD_BANKS").ToList();
            MOIN.SelectedIndex = 0;

            //به حساب
            kindp.ItemsSource = dbms.DoGetDataSQL<KIND_PAPER>($"SELECT CO_KIND, KIND FROM KIND_PAPER").ToList();
            kindp.SelectedValue = 1; kindp.Items.Refresh();
        }

        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_GO.IsEnabled) { return; }

            #region Validate

            if (string.IsNullOrEmpty(mabl.Text) || mabl.Text == "0")
            {
                universControl.PopNotifyShow("مبلغ نمیتواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            string date_n_val = date_s.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (string.IsNullOrEmpty(VAJH.Text))
            {
                universControl.PopNotifyShow("در وجه نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (MOIN.SelectedValue == null)
            {
                universControl.PopNotifyShow("از حساب نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (kindp.SelectedValue == null)
            {
                universControl.PopNotifyShow("دسته چک نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            #endregion

            #region Report

            bool isFitChoosed = Convert.ToBoolean(RD_FTICHECK.IsChecked);

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Checkha.{(isFitChoosed ? "CHAPCHEK_FIT" : "CHAPCHEK")}.mrt");
            report.Load(pathreport);

            // Date
            var _DATE_ = report.GetComponentByName("DATE") as StiText;
            _DATE_.Text = date_s.Text;

            //Mabl
            var _MABL_ = report.GetComponentByName("MABL") as StiText;
            _MABL_.Text = "/-" + Convert.ToInt64(mabl.Text).ToString("N0");

            // Alpha
            var _TEXTMABL_ = report.GetComponentByName("TEXTMABL") as StiText;
            _TEXTMABL_.Text = CL_HESABDARI.ALPHANUM(Convert.ToDouble(mabl.Text)) + " ريال******************************************************************************"; // Set the amount text here

            // Vajh
            var _VAJH_ = report.GetComponentByName("VAJH") as StiText;
            _VAJH_.Text = VAJH.Text + "*******************************************************************";

            long COBANK = Convert.ToInt64(MOIN.SelectedValue);
            byte COKIND = Convert.ToByte(kindp.SelectedValue);
            var record = dbms.DoGetDataSQL<FORMAT>($"SELECT * FROM FORMAT WHERE BANK = {COBANK} AND KIND = {COKIND}").FirstOrDefault();
            if (record != null && Convert.ToBoolean(RD_FORMATDB.IsChecked))
            {
                //var _DATEN_ = report.GetComponentByName("DATEN") as StiText;
                //=ALPHANUM(Right([Forms]![CHAPCHEK]![DATE_S]؛2)) & " / " & ALPHANUM(Mid([Forms]![CHAPCHEK]![DATE_S]؛5؛2)) & " / " & ALPHANUM(Left([Forms]![CHAPCHEK]![DATE_S]؛4))
                ApplyTextComponentProperties(report, "DATE", record.DATE_TOP, record.DATE_LEFT, record.DATE_HEIGHT, record.DATE_FONT, (float)record.DATE_SIZE, (bool)record.DATE_B, (bool)record.DATE_I, (bool)record.DATE_U, (int)record.DATE_COLOR, (bool)record.DATE_P);
                ApplyTextComponentProperties(report, "MABL", record.NUM_TOP, record.NUM_LEFT, record.NUM_HEIGHT, record.NUM_FONT, (float)record.NUM_SIZE, (bool)record.NUM_B, (bool)record.NUM_I, (bool)record.NUM_U, (int)record.NUM_COLOR, (bool)record.NUM_P);
                ApplyTextComponentProperties(report, "TEXTMABL", record.ALPHA_TOP, record.ALPHA_LEFT, record.ALPHA_HEIGHT, record.ALPHA_FONT, (float)record.ALPHA_SIZE, (bool)record.ALPHA_B, (bool)record.ALPHA_I, (bool)record.ALPHA_U, (int)record.ALPHA_COLOR, (bool)record.ALPHA_P);
                ApplyTextComponentProperties(report, "VAJH", record.VAJH_TOP, record.VAJH_LEFT, record.VAJH_HEIGHT, record.VAJH_FONT, (float)record.VAJH_SIZE, (bool)record.VAJH_B, (bool)record.VAJH_I, (bool)record.VAJH_U, (int)record.VAJH_COLOR, (bool)record.VAJH_P);
            }
            else if (isFitChoosed)
            {

            }
            else
            {
                SetDefaultPositions(report);
            }

            //report.Render();
            //report.Show();
            //pathreport?.Dispose();

            new Rpts.WINRPT(report, "چاپ سریع چک").Show();
            #endregion

        }
        private void SetDefaultPositions(StiReport report)
        {
            //Paper: A4 => Margins : Left = 4 , Others = 0
            report.Pages[0].Margins = new Stimulsoft.Report.Components.StiMargins(4, 0, 0, 0); // Top, Left, Right, Bottom

            //DATE: Left = 11.3 , Top = 1.3 , Width = 1.8 , Height = 0.61
            {
                var DATE_LEFT = 11.3;
                var DATE_TOP = 1.3;
                var DATE_SIZE = 1.8;
                var DATE_HEIGHT = 0.61;
                ApplyTextComponentProperties(report, "DATE", DATE_TOP, DATE_LEFT, DATE_HEIGHT, "IRANYekanFN", 10, false, false, false, default, true);
            }

            ////مبلغ به حروف
            //TEXTMABL: Left = 1.4 , Top = 2.95 , Width = 11 , Height = 0.61
            {
                var TEXTMABL_TOP = 2.95;
                var TEXTMABL_LEFT = 1.4;
                var TEXTMABL_HEIGHT = 0.61;
                var TEXTMABL_SIZE = 11;
                ApplyTextComponentProperties(report, "TEXTMABL", TEXTMABL_TOP, TEXTMABL_LEFT, TEXTMABL_HEIGHT, "IRANYekanFN", 10, false, false, false, default, true);
            }

            //VAJH: Left = 8.7 , Top = 4.1 , Width = 6.88 , Height = 0.59
            {
                var VAJH_TOP = 4.1;
                var VAJH_LEFT = 8.7;
                var VAJH_HEIGHT = 0.59;
                var VAJH_SIZE = 6.88;
                ApplyTextComponentProperties(report, "VAJH", VAJH_TOP, VAJH_LEFT, VAJH_HEIGHT, "IRANYekanFN", 10, false, false, false, default, true);
            }

            //MABL: Left = 1 , Top = 5.9 , Width = 6.5 , Height = 0.61
            {
                var MABL_TOP = 5.9;
                var MABL_LEFT = 1;
                var MABL_HEIGHT = 0.61;
                var MABL_SIZE = 6.5;
                ApplyTextComponentProperties(report, "MABL", MABL_TOP, MABL_LEFT, MABL_HEIGHT, "IRANYekanFN", 10, false, false, false, default, true);
            }

        }
        private void ApplyTextComponentProperties(StiReport report, string componentName, double? top, double? left, double? height, string fontName, float fontSize, bool bold, bool italic, bool underline, int color, bool visible)
        {
            var component = report.GetComponentByName(componentName) as StiText;
            if (component != null)
            {
                var _TOP_ = (double)(top ?? 0);
                var _LEF_ = (double)(left ?? 0);
                component.Top = (_TOP_); // (_TOP_ / 1000);
                component.Left = (_LEF_);
                if (height.HasValue)
                    component.Height = height.Value;

                component.Font = new Font(fontName, fontSize, GetFontStyle(bold, italic, underline));

                if (color != null && color != 0)
                {
                    //component.TextBrush = new StiSolidBrush(Color.FromArgb(color));
                }

                component.Enabled = visible;
            }
        }
        private System.Drawing.FontStyle GetFontStyle(bool bold, bool italic, bool underline)
        {
            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            if (bold)
                style |= System.Drawing.FontStyle.Bold;
            if (italic)
                style |= System.Drawing.FontStyle.Italic;
            if (underline)
                style |= System.Drawing.FontStyle.Underline;

            return style;
        }

        private void BTN_HELP_Click(object sender, RoutedEventArgs e)
        {
            new WIN_GUIDECHECK().ShowDialog();
        }
    }
}
