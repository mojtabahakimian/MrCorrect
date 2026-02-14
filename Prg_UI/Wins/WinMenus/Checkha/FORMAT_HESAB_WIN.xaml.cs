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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for FORMAT_HESAB_WIN.xaml
    /// </summary>
    public partial class FORMAT_HESAB_WIN : Window
    {
        public class FORMAT_HESAB
        {
            public int? BANK { get; set; }
            public short? KIND { get; set; }
            public float? DATE_TOP { get; set; }
            public float? DATE_LEFT { get; set; }
            public string? DATE_FONT { get; set; }
            public short? DATE_SIZE { get; set; }
            public float? DATE_WITH { get; set; }
            public bool? DATE_P { get; set; }
            public float? SHO_TOP { get; set; }
            public float? SHO_LEFT { get; set; }
            public string? SHO_FONT { get; set; }
            public short? SHO_SIZE { get; set; }
            public float? SHO_WITH { get; set; }
            public bool? SHO_P { get; set; }
            public float? SHES_TOP { get; set; }
            public float? SHES_LEFT { get; set; }
            public string? SHES_FONT { get; set; }
            public short? SHES_SIZE { get; set; }
            public float? SHES_WITH { get; set; }
            public bool? SHES_P { get; set; }
            public float? NAME_TOP { get; set; }
            public float? NAME_LEFT { get; set; }
            public string? NAME_FONT { get; set; }
            public short? NAME_SIZE { get; set; }
            public float? NAME_WITH { get; set; }
            public bool? NAME_P { get; set; }
            public float? CDATE_TOP { get; set; }
            public float? CDATE_LEFT { get; set; }
            public string? CDATE_FONT { get; set; }
            public short? CDATE_SIZE { get; set; }
            public float? CDATE_WITH { get; set; }
            public bool? CDATE_P { get; set; }
            public float? SER_TOP { get; set; }
            public float? SER_LEFT { get; set; }
            public string? SER_FONT { get; set; }
            public short? SER_SIZE { get; set; }
            public float? SER_WITH { get; set; }
            public bool? SER_P { get; set; }
            public float? CHES_TOP { get; set; }
            public float? CHES_LEFT { get; set; }
            public string? CHES_FONT { get; set; }
            public short? CHES_SIZE { get; set; }
            public float? CHES_WITH { get; set; }
            public bool? CHES_P { get; set; }
            public float? BSHO_TOP { get; set; }
            public float? BSHO_LEFT { get; set; }
            public string? BSHO_FONT { get; set; }
            public short? BSHO_SIZE { get; set; }
            public float? BSHO_WITH { get; set; }
            public bool? BSHO_P { get; set; }
            public float? MAB_TOP { get; set; }
            public float? MAB_LEFT { get; set; }
            public string? MAB_FONT { get; set; }
            public short? MAB_SIZE { get; set; }
            public float? MAB_WITH { get; set; }
            public bool? MAB_P { get; set; }
            public float? AMAB_TOP { get; set; }
            public float? AMAB_LEFT { get; set; }
            public string? AMAB_FONT { get; set; }
            public short? AMAB_SIZE { get; set; }
            public float? AMAB_WITH { get; set; }
            public bool? AMAB_P { get; set; }
            public float? CITY_TOP { get; set; }
            public float? CITY_LEFT { get; set; }
            public string? CITY_FONT { get; set; }
            public short? CITY_SIZE { get; set; }
            public float? CITY_WITH { get; set; }
            public bool? CITY_P { get; set; }
        }
        public FORMAT_HESAB_WIN()
        {
            InitializeComponent();

            this.DataContext = this;

            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("fa-IR");
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


        public ObservableCollection<FormatHesabRow> FormatRows { get; } = new ObservableCollection<FormatHesabRow>();
        public List<TCOD_BANKS> Banks { get; private set; } = new List<TCOD_BANKS>();
        public List<FontItem> Fonts { get; private set; } = new List<FontItem>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //string formName = "FORMAT_HESAB";
                //var helper = new WindowInteropHelper(this);
                //helper.EnsureHandle();
                //CL_HESABDARI.SETSECURITY(GetType().Name, formName, helper.Handle, GetType().Name);
                //if (!IsLoaded)
                //{
                //    Close();
                //    return;
                //}
            }
            catch
            {
                try { Close(); } catch { }
                return;
            }
            #endregion

            CL_HESABDARI.AMALIYAT_USER(GetType().Name);
            LoadCombos();
            BuildDefaultRows();

            if (Banks.Count > 0)
            {
                BankCombo.SelectedIndex = 0;
            }
        }

        private void LoadCombos()
        {
            Banks = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT CODE, NAMES FROM TCOD_BANKS").ToList();
            Fonts = dbms.DoGetDataSQL<FontItem>("SELECT font.قلم AS NAME FROM font ORDER BY font.قلم").ToList();

            OnPropertyChanged(nameof(Banks));
            OnPropertyChanged(nameof(Fonts));
        }

        private void BuildDefaultRows()
        {
            FormatRows.Clear();
            AddRow(FormatHesabField.Date, "تاریخ روز");
            AddRow(FormatHesabField.Sho, "شعبه بانک");
            AddRow(FormatHesabField.Shes, "شماره حساب");
            AddRow(FormatHesabField.Name, "نام صاحب حساب");
            AddRow(FormatHesabField.CDate, "تاریخ چک");
            AddRow(FormatHesabField.Ser, "شماره چک");
            AddRow(FormatHesabField.Ches, "شماره حساب چک");
            AddRow(FormatHesabField.Bsho, "بانک / شعبه");
            AddRow(FormatHesabField.Mab, "مبلغ");
            AddRow(FormatHesabField.Amab, "مبلغ به حروف");
            AddRow(FormatHesabField.City, "شهرستان پرداخت");
        }

        private void AddRow(FormatHesabField field, string caption)
        {
            FormatRows.Add(new FormatHesabRow { Field = field, Caption = caption });
        }

        private void BankCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (NowIsReady)
            {
                LoadRecord();
            }
        }

        private void Btn_Load_Click(object sender, RoutedEventArgs e)
        {
            LoadRecord();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveRecord();
        }

        private void LoadRecord()
        {
            if (!TryGetKeys(out int bankCode, out short kind))
            {
                universControl.PopNotifyShow("بانک و شماره فرمت را وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            var record = dbms.DoGetDataSQL<FORMAT_HESAB>(
                "SELECT * FROM FORMAT_HESAB WHERE BANK = @BANK AND KIND = @KIND",
                new { BANK = bankCode, KIND = kind }).FirstOrDefault();

            if (record == null)
            {
                record = new FORMAT_HESAB { BANK = bankCode, KIND = kind };
            }

            BuildRowsFromRecord(record);
        }

        private void BuildRowsFromRecord(FORMAT_HESAB record)
        {
            BuildDefaultRows();

            ApplyRow(record, FormatHesabField.Date, record.DATE_TOP, record.DATE_LEFT, record.DATE_FONT, record.DATE_SIZE, record.DATE_WITH, record.DATE_P);
            ApplyRow(record, FormatHesabField.Sho, record.SHO_TOP, record.SHO_LEFT, record.SHO_FONT, record.SHO_SIZE, record.SHO_WITH, record.SHO_P);
            ApplyRow(record, FormatHesabField.Shes, record.SHES_TOP, record.SHES_LEFT, record.SHES_FONT, record.SHES_SIZE, record.SHES_WITH, record.SHES_P);
            ApplyRow(record, FormatHesabField.Name, record.NAME_TOP, record.NAME_LEFT, record.NAME_FONT, record.NAME_SIZE, record.NAME_WITH, record.NAME_P);
            ApplyRow(record, FormatHesabField.CDate, record.CDATE_TOP, record.CDATE_LEFT, record.CDATE_FONT, record.CDATE_SIZE, record.CDATE_WITH, record.CDATE_P);
            ApplyRow(record, FormatHesabField.Ser, record.SER_TOP, record.SER_LEFT, record.SER_FONT, record.SER_SIZE, record.SER_WITH, record.SER_P);
            ApplyRow(record, FormatHesabField.Ches, record.CHES_TOP, record.CHES_LEFT, record.CHES_FONT, record.CHES_SIZE, record.CHES_WITH, record.CHES_P);
            ApplyRow(record, FormatHesabField.Bsho, record.BSHO_TOP, record.BSHO_LEFT, record.BSHO_FONT, record.BSHO_SIZE, record.BSHO_WITH, record.BSHO_P);
            ApplyRow(record, FormatHesabField.Mab, record.MAB_TOP, record.MAB_LEFT, record.MAB_FONT, record.MAB_SIZE, record.MAB_WITH, record.MAB_P);
            ApplyRow(record, FormatHesabField.Amab, record.AMAB_TOP, record.AMAB_LEFT, record.AMAB_FONT, record.AMAB_SIZE, record.AMAB_WITH, record.AMAB_P);
            ApplyRow(record, FormatHesabField.City, record.CITY_TOP, record.CITY_LEFT, record.CITY_FONT, record.CITY_SIZE, record.CITY_WITH, record.CITY_P);
        }

        private void ApplyRow(FORMAT_HESAB record, FormatHesabField field, float? top, float? left, string? font, short? size, float? width, bool? print)
        {
            var row = FormatRows.FirstOrDefault(item => item.Field == field);
            if (row == null)
            {
                return;
            }

            row.Top = top;
            row.Left = left;
            row.Font = font;
            row.Size = size;
            row.Width = width;
            row.Print = print;
        }

        private void SaveRecord()
        {
            if (!TryGetKeys(out int bankCode, out short kind))
            {
                universControl.PopNotifyShow("بانک و شماره فرمت را وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            FormatGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true);
            FormatGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            var record = BuildRecordFromRows(bankCode, kind);
            var existingCount = dbms.DoGetDataSQL<int>(
                "SELECT COUNT(1) FROM FORMAT_HESAB WHERE BANK = @BANK AND KIND = @KIND",
                new { BANK = bankCode, KIND = kind }).FirstOrDefault();

            if (existingCount > 0)
            {
                dbms.DoExecuteSQL(@"
UPDATE FORMAT_HESAB SET
DATE_TOP = @DATE_TOP,
DATE_LEFT = @DATE_LEFT,
DATE_FONT = @DATE_FONT,
DATE_SIZE = @DATE_SIZE,
DATE_WITH = @DATE_WITH,
DATE_P = @DATE_P,
SHO_TOP = @SHO_TOP,
SHO_LEFT = @SHO_LEFT,
SHO_FONT = @SHO_FONT,
SHO_SIZE = @SHO_SIZE,
SHO_WITH = @SHO_WITH,
SHO_P = @SHO_P,
SHES_TOP = @SHES_TOP,
SHES_LEFT = @SHES_LEFT,
SHES_FONT = @SHES_FONT,
SHES_SIZE = @SHES_SIZE,
SHES_WITH = @SHES_WITH,
SHES_P = @SHES_P,
NAME_TOP = @NAME_TOP,
NAME_LEFT = @NAME_LEFT,
NAME_FONT = @NAME_FONT,
NAME_SIZE = @NAME_SIZE,
NAME_WITH = @NAME_WITH,
NAME_P = @NAME_P,
CDATE_TOP = @CDATE_TOP,
CDATE_LEFT = @CDATE_LEFT,
CDATE_FONT = @CDATE_FONT,
CDATE_SIZE = @CDATE_SIZE,
CDATE_WITH = @CDATE_WITH,
CDATE_P = @CDATE_P,
SER_TOP = @SER_TOP,
SER_LEFT = @SER_LEFT,
SER_FONT = @SER_FONT,
SER_SIZE = @SER_SIZE,
SER_WITH = @SER_WITH,
SER_P = @SER_P,
CHES_TOP = @CHES_TOP,
CHES_LEFT = @CHES_LEFT,
CHES_FONT = @CHES_FONT,
CHES_SIZE = @CHES_SIZE,
CHES_WITH = @CHES_WITH,
CHES_P = @CHES_P,
BSHO_TOP = @BSHO_TOP,
BSHO_LEFT = @BSHO_LEFT,
BSHO_FONT = @BSHO_FONT,
BSHO_SIZE = @BSHO_SIZE,
BSHO_WITH = @BSHO_WITH,
BSHO_P = @BSHO_P,
MAB_TOP = @MAB_TOP,
MAB_LEFT = @MAB_LEFT,
MAB_FONT = @MAB_FONT,
MAB_SIZE = @MAB_SIZE,
MAB_WITH = @MAB_WITH,
MAB_P = @MAB_P,
AMAB_TOP = @AMAB_TOP,
AMAB_LEFT = @AMAB_LEFT,
AMAB_FONT = @AMAB_FONT,
AMAB_SIZE = @AMAB_SIZE,
AMAB_WITH = @AMAB_WITH,
AMAB_P = @AMAB_P,
CITY_TOP = @CITY_TOP,
CITY_LEFT = @CITY_LEFT,
CITY_FONT = @CITY_FONT,
CITY_SIZE = @CITY_SIZE,
CITY_WITH = @CITY_WITH,
CITY_P = @CITY_P
WHERE BANK = @BANK AND KIND = @KIND", record);
            }
            else
            {
                dbms.DoExecuteSQL(@"
INSERT INTO FORMAT_HESAB
(BANK, KIND, DATE_TOP, DATE_LEFT, DATE_FONT, DATE_SIZE, DATE_WITH, DATE_P,
 SHO_TOP, SHO_LEFT, SHO_FONT, SHO_SIZE, SHO_WITH, SHO_P,
 SHES_TOP, SHES_LEFT, SHES_FONT, SHES_SIZE, SHES_WITH, SHES_P,
 NAME_TOP, NAME_LEFT, NAME_FONT, NAME_SIZE, NAME_WITH, NAME_P,
 CDATE_TOP, CDATE_LEFT, CDATE_FONT, CDATE_SIZE, CDATE_WITH, CDATE_P,
 SER_TOP, SER_LEFT, SER_FONT, SER_SIZE, SER_WITH, SER_P,
 CHES_TOP, CHES_LEFT, CHES_FONT, CHES_SIZE, CHES_WITH, CHES_P,
 BSHO_TOP, BSHO_LEFT, BSHO_FONT, BSHO_SIZE, BSHO_WITH, BSHO_P,
 MAB_TOP, MAB_LEFT, MAB_FONT, MAB_SIZE, MAB_WITH, MAB_P,
 AMAB_TOP, AMAB_LEFT, AMAB_FONT, AMAB_SIZE, AMAB_WITH, AMAB_P,
 CITY_TOP, CITY_LEFT, CITY_FONT, CITY_SIZE, CITY_WITH, CITY_P)
VALUES
(@BANK, @KIND, @DATE_TOP, @DATE_LEFT, @DATE_FONT, @DATE_SIZE, @DATE_WITH, @DATE_P,
 @SHO_TOP, @SHO_LEFT, @SHO_FONT, @SHO_SIZE, @SHO_WITH, @SHO_P,
 @SHES_TOP, @SHES_LEFT, @SHES_FONT, @SHES_SIZE, @SHES_WITH, @SHES_P,
 @NAME_TOP, @NAME_LEFT, @NAME_FONT, @NAME_SIZE, @NAME_WITH, @NAME_P,
 @CDATE_TOP, @CDATE_LEFT, @CDATE_FONT, @CDATE_SIZE, @CDATE_WITH, @CDATE_P,
 @SER_TOP, @SER_LEFT, @SER_FONT, @SER_SIZE, @SER_WITH, @SER_P,
 @CHES_TOP, @CHES_LEFT, @CHES_FONT, @CHES_SIZE, @CHES_WITH, @CHES_P,
 @BSHO_TOP, @BSHO_LEFT, @BSHO_FONT, @BSHO_SIZE, @BSHO_WITH, @BSHO_P,
 @MAB_TOP, @MAB_LEFT, @MAB_FONT, @MAB_SIZE, @MAB_WITH, @MAB_P,
 @AMAB_TOP, @AMAB_LEFT, @AMAB_FONT, @AMAB_SIZE, @AMAB_WITH, @AMAB_P,
 @CITY_TOP, @CITY_LEFT, @CITY_FONT, @CITY_SIZE, @CITY_WITH, @CITY_P)", record);
            }

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1);
        }

        private FORMAT_HESAB BuildRecordFromRows(int bankCode, short kind)
        {
            var record = new FORMAT_HESAB
            {
                BANK = bankCode,
                KIND = kind
            };

            foreach (var row in FormatRows)
            {
                switch (row.Field)
                {
                    case FormatHesabField.Date:
                        record.DATE_TOP = row.Top;
                        record.DATE_LEFT = row.Left;
                        record.DATE_FONT = row.Font;
                        record.DATE_SIZE = row.Size;
                        record.DATE_WITH = row.Width;
                        record.DATE_P = row.Print;
                        break;
                    case FormatHesabField.Sho:
                        record.SHO_TOP = row.Top;
                        record.SHO_LEFT = row.Left;
                        record.SHO_FONT = row.Font;
                        record.SHO_SIZE = row.Size;
                        record.SHO_WITH = row.Width;
                        record.SHO_P = row.Print;
                        break;
                    case FormatHesabField.Shes:
                        record.SHES_TOP = row.Top;
                        record.SHES_LEFT = row.Left;
                        record.SHES_FONT = row.Font;
                        record.SHES_SIZE = row.Size;
                        record.SHES_WITH = row.Width;
                        record.SHES_P = row.Print;
                        break;
                    case FormatHesabField.Name:
                        record.NAME_TOP = row.Top;
                        record.NAME_LEFT = row.Left;
                        record.NAME_FONT = row.Font;
                        record.NAME_SIZE = row.Size;
                        record.NAME_WITH = row.Width;
                        record.NAME_P = row.Print;
                        break;
                    case FormatHesabField.CDate:
                        record.CDATE_TOP = row.Top;
                        record.CDATE_LEFT = row.Left;
                        record.CDATE_FONT = row.Font;
                        record.CDATE_SIZE = row.Size;
                        record.CDATE_WITH = row.Width;
                        record.CDATE_P = row.Print;
                        break;
                    case FormatHesabField.Ser:
                        record.SER_TOP = row.Top;
                        record.SER_LEFT = row.Left;
                        record.SER_FONT = row.Font;
                        record.SER_SIZE = row.Size;
                        record.SER_WITH = row.Width;
                        record.SER_P = row.Print;
                        break;
                    case FormatHesabField.Ches:
                        record.CHES_TOP = row.Top;
                        record.CHES_LEFT = row.Left;
                        record.CHES_FONT = row.Font;
                        record.CHES_SIZE = row.Size;
                        record.CHES_WITH = row.Width;
                        record.CHES_P = row.Print;
                        break;
                    case FormatHesabField.Bsho:
                        record.BSHO_TOP = row.Top;
                        record.BSHO_LEFT = row.Left;
                        record.BSHO_FONT = row.Font;
                        record.BSHO_SIZE = row.Size;
                        record.BSHO_WITH = row.Width;
                        record.BSHO_P = row.Print;
                        break;
                    case FormatHesabField.Mab:
                        record.MAB_TOP = row.Top;
                        record.MAB_LEFT = row.Left;
                        record.MAB_FONT = row.Font;
                        record.MAB_SIZE = row.Size;
                        record.MAB_WITH = row.Width;
                        record.MAB_P = row.Print;
                        break;
                    case FormatHesabField.Amab:
                        record.AMAB_TOP = row.Top;
                        record.AMAB_LEFT = row.Left;
                        record.AMAB_FONT = row.Font;
                        record.AMAB_SIZE = row.Size;
                        record.AMAB_WITH = row.Width;
                        record.AMAB_P = row.Print;
                        break;
                    case FormatHesabField.City:
                        record.CITY_TOP = row.Top;
                        record.CITY_LEFT = row.Left;
                        record.CITY_FONT = row.Font;
                        record.CITY_SIZE = row.Size;
                        record.CITY_WITH = row.Width;
                        record.CITY_P = row.Print;
                        break;
                }
            }

            return record;
        }

        private bool TryGetKeys(out int bankCode, out short kind)
        {
            bankCode = 0;
            kind = 0;

            if (BankCombo.SelectedValue == null || !int.TryParse(BankCombo.SelectedValue.ToString(), out bankCode))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(KindBox.Text) || !short.TryParse(KindBox.Text, out kind))
            {
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class FontItem
        {
            public string? NAME { get; set; }
        }

        public enum FormatHesabField
        {
            Date,
            Sho,
            Shes,
            Name,
            CDate,
            Ser,
            Ches,
            Bsho,
            Mab,
            Amab,
            City
        }

        public class FormatHesabRow : INotifyPropertyChanged
        {
            private float? top;
            private float? left;
            private string? font;
            private short? size;
            private float? width;
            private bool? print;

            public FormatHesabField Field { get; set; }
            public string Caption { get; set; } = string.Empty;

            public float? Top
            {
                get => top;
                set
                {
                    top = value;
                    OnPropertyChanged();
                }
            }

            public float? Left
            {
                get => left;
                set
                {
                    left = value;
                    OnPropertyChanged();
                }
            }

            public string? Font
            {
                get => font;
                set
                {
                    font = value;
                    OnPropertyChanged();
                }
            }

            public short? Size
            {
                get => size;
                set
                {
                    size = value;
                    OnPropertyChanged();
                }
            }

            public float? Width
            {
                get => width;
                set
                {
                    width = value;
                    OnPropertyChanged();
                }
            }

            public bool? Print
            {
                get => print;
                set
                {
                    print = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
