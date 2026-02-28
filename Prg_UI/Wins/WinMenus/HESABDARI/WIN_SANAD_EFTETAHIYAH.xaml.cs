using Dapper;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for WIN_SANAD_EFTETAHIYAH.xaml
    /// </summary>
    public partial class WIN_SANAD_EFTETAHIYAH : Window
    {
        public WIN_SANAD_EFTETAHIYAH()
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
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        public class COMMODEL1
        {
            public string NAME { get; set; } = "";
        }
        public class COMMODEL2
        {
            public double N_S { get; set; }
        }

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

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "EFTE", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

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
            // نام دیتابیس برای سال قبل
            try
            {
                NDBS.ItemsSource = dbms.DoGetDataSQL<COMMODEL1>($"SELECT NAME as NAME from sys.databases where name not in ('master', 'model', 'tempdb', 'msdb') order by name").ToList();
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در دریافت دیتابیس های موجود").Show();
            }
        }

        private void ReGetData()
        {
            // Set Current DB Name
            ODBS.Text = CL_Generaly.General_DBname;

            int currentYear = (int)Baseknow.YEA;
            int previousYear = currentYear - 1;
            string currentDbName = CL_Generaly.General_DBname;
            string previousDbName = "";

            if (currentDbName.Length >= 4 && int.TryParse(currentDbName.Substring(currentDbName.Length - 4), out _))
            {
                previousDbName = currentDbName.Substring(0, currentDbName.Length - 4) + previousYear;
            }
            else
            {
                previousDbName = dbms.DoGetDataSQL<string?>("SELECT TOP 1 name FROM sys.databases WHERE name < DB_NAME()  AND database_id > 4 ORDER BY name DESC;").FirstOrDefault();
            }

            bool PreviousDbExist = false;
            if (!string.IsNullOrWhiteSpace(previousDbName))
            {
                PreviousDbExist = dbms.DoGetDataSQL<bool>($"SELECT IIF(DB_ID('{previousDbName}') IS NOT NULL, 1, 0) AS [Exists];").FirstOrDefault();

                if (!PreviousDbExist)
                {
                    previousDbName = dbms.DoGetDataSQL<string?>("SELECT TOP 1 name FROM sys.databases WHERE name < DB_NAME()  AND database_id > 4 ORDER BY name DESC;").FirstOrDefault();
                    PreviousDbExist = dbms.DoGetDataSQL<bool>($"SELECT IIF(DB_ID('{previousDbName}') IS NOT NULL, 1, 0) AS [Exists];").FirstOrDefault();
                }
            }

            if (!PreviousDbExist)
            {
                new Msgwin(false, "نام دیتابیس سال قبل یافت نشد !").Show(); return;
            }
            else if (currentDbName.ToLower().Trim() == previousDbName)
            {
                new Msgwin(false, "نام دیتابیس سال قبل با سالی جاری نمیتواند یکسان باشد !").Show(); return;
            }

            //نام دیتابیس در سال قبل
            NDBS.SelectedValue = previousDbName; NDBS.Items.Refresh();

            LoadNumberVouchers(previousDbName);

            // Get Target Doc ID (S2) from Current DB (Max N_S + 1)
            var maxNsCurr = dbms.DoGetDataSQL<int?>("SELECT MAX(N_S) FROM DEED_HED").FirstOrDefault();
            if (maxNsCurr != null)
            {
                S2.Text = (maxNsCurr.Value + 1).ToString();
            }
            else
            {
                S2.Text = "1";
            }

            // Set Default Date
            DT.Text = Tarikh.SlashyFullDate; // e.g., 1403/01/01
            SH.Text = "سند افتتاحیه";
        }

        private void LoadNumberVouchers(string previousDbName)
        {
            //شماره سند اختتامیه سال قبل
            S1.ItemsSource = dbms.DoGetDataSQL<COMMODEL2>($"SELECT N_S as N_S FROM [{previousDbName}].dbo.DEED_HED ORDER BY N_S DESC").ToList();
            S1.SelectedIndex = 0;
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // 1. Basic empty field validations
            if (string.IsNullOrWhiteSpace(NDBS.Text))
                ErrosMessages.Add(new MsgModel { MessageText_U = "لطفا بانک اطلاعاتی سال قبل را انتخاب کنید." });

            if (string.IsNullOrWhiteSpace(DT.Text))
                ErrosMessages.Add(new MsgModel { MessageText_U = "لطفا تاریخ سند را وارد کنید." });

            // 2. Numeric validations for Vouchers
            if (string.IsNullOrWhiteSpace(S1.Text) || !double.TryParse(S1.Text, out double s1Val) || s1Val <= 0)
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سند اختتامیه (سال قبل) نامعتبر است." });

            if (string.IsNullOrWhiteSpace(S2.Text) || !double.TryParse(S2.Text, out double s2Val) || s2Val <= 0)
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سند افتتاحیه (سال جاری) نامعتبر است." });

            // 3. Database existence and relation validation
            if (!string.IsNullOrWhiteSpace(NDBS.Text))
            {
                var exists = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM master.dbo.sysdatabases WHERE name = @Name", new { Name = NDBS.Text }).FirstOrDefault();
                if (exists == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "بانک اطلاعاتی انتخاب شده وجود ندارد." });
                }
                else if (double.TryParse(S1.Text, out double checkS1Val))
                {
                    // Escape brackets to prevent dynamic SQL injection in database name context
                    string safeDbName = NDBS.Text.Replace("]", "]]");

                    // Verify that the requested Source Voucher (S1) actually exists in the previous DB
                    var voucherExists = dbms.DoGetDataSQL<int>($@"
                        SELECT COUNT(1) 
                        FROM [{safeDbName}].dbo.DEED_HED 
                        WHERE N_S = @N_S", new { N_S = checkS1Val }).FirstOrDefault();

                    if (voucherExists == 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سند اختتامیه مورد نظر در بانک اطلاعاتی سال قبل یافت نشد." });
                    }
                }
            }

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

        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            // Execute all verifications logically inside the consolidated Validation method.
            if (!HeaderIsValid()) return;

            string dbName = NDBS.Text.Replace("]", "]]"); // Re-sanitizing for T-SQL structural requirement
            double s1Val = double.Parse(S1.Text);
            double s2Val = double.Parse(S2.Text);
            string cleanDate = DT.Text.ToRawTarikh();

            Msgwin msgConfirm = new Msgwin(true, "آيا درمورد صدور سند افتتاحيه اطمينان داريد؟");
            msgConfirm.ShowDialog();
            if (msgConfirm.DialogResult != true) return;

            try
            {
                // Check Existing Target Voucher
                var existingS2 = dbms.DoGetDataSQL<double?>("SELECT N_S FROM DEED_HED WHERE N_S = @N_S", new { N_S = s2Val }).FirstOrDefault();

                if (existingS2 == null)
                {
                    // Insert Header
                    dbms.DoExecuteSQL("INSERT INTO DEED_HED (N_S, DATE_S, SHARH_S, NO_S, OKF, USER_NAME) VALUES (@N_S, @DATE_S, @SHARH_S, 0, 1, @USER_NAME)", new
                    {
                        N_S = s2Val,
                        DATE_S = cleanDate,
                        SHARH_S = SH.Text,
                        USER_NAME = CL_HESABDARI.UCurrentUser()
                    });
                }
                else
                {
                    Msgwin msgDup = new Msgwin(true, "شماره سند تكراري مي باشد. آيا اطمينان داريد؟ (سند جایگزین خواهد شد)");
                    msgDup.ShowDialog();
                    if (msgDup.DialogResult != true) return;

                    // Update Header
                    dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = @DATE_S, SHARH_S = @SHARH_S, NO_S = 0, OKF = 1, USER_NAME = @USER_NAME WHERE N_S = @N_S", new
                    {
                        N_S = s2Val,
                        DATE_S = cleanDate,
                        SHARH_S = SH.Text,
                        USER_NAME = CL_HESABDARI.UCurrentUser()
                    });
                }

                // Clean Details
                dbms.DoExecuteSQL("DELETE FROM DEED_DTL WHERE N_S = @N_S", new { N_S = s2Val });

                // Transfer Details (Swap BED/BES). Fully parameterized to avoid SQL Injection.
                string insertSql = $@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, BED, BES, SHARH, N_SERI, BANK, ARZD, RADIF)
                    SELECT @New_NS, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES,
                           BES, -- Swap
                           BED, -- Swap
                           CASE WHEN SHARH = N'سند اختتاميه' THEN N'سند افتتاحيه' ELSE SHARH END,
                           N_SERI, BANK, 1,
                           ROW_NUMBER() OVER (ORDER BY id)
                    FROM [{dbName}].dbo.DEED_DTL
                    WHERE N_S = @Old_NS";

                dbms.DoExecuteSQL(insertSql, new { New_NS = s2Val, Old_NS = s1Val });

                new Msgwin(false, "عملیات با موفقیت انجام شد.").ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا: " + ex.Message).ShowDialog();
            }
        }

        private void NDBS_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NDBS.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (NDBS.SelectedValue != null)
            {
                LoadNumberVouchers(NDBS.SelectedValue.ToString());
            }
        }
    }
}