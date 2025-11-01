using Functions;
using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Functions.SMSService.MSGMODEL;

namespace Wins.WinMenus.CONFIGS
{
    public partial class SMS_FORMAT : Window
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
        public SMS_FORMAT()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<SMS_FORMATS_MODEL> SMS_FORMATS_MODEL_DATA { get; set; } = new ObservableCollection<SMS_FORMATS_MODEL>();

        UniversControl universControl = new UniversControl();

        #region LOCALMODEL
        public class CMBMODEL
        {
            public string STR_ID { get; set; }
            public string STR_NAME { get; set; }
        }
        #endregion

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool DG_FORMAT_SUB_IsFocused { get; private set; }
        public bool NewRecord { get; set; }
        public long? CURRENT_ROW_SMS_FORMATS_MODEL_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; }

        public string? SMS_FORMATS_MODEL_ENTERED_VALUE_ROW { get; private set; }
        public SMS_FORMATS_MODEL? SMS_FORMATS_MODEL_CURRENT_ROW_ITEMS { get; private set; }
        public SMS_FORMATS_MODEL? SMS_FORMATS_MODEL_WAS_ROW_ITEM { get; private set; }

        public ObservableCollection<CMBMODEL> OFILEDS_COMBO_DATA { get; set; } = new ObservableCollection<CMBMODEL>();

        public List<CMBMODEL> RST_SMS_FIELDS = new List<CMBMODEL>
        {
            new CMBMODEL { STR_ID = "CUST_NO1", STR_NAME = "نام مشتری" },
            new CMBMODEL { STR_ID = "CUST_NO", STR_NAME = "كد مشتری" },
            new CMBMODEL { STR_ID = "MOLAH", STR_NAME = "ملاحظات" },
            new CMBMODEL { STR_ID = "NUMBER", STR_NAME = "شماره فاكتور" },
            new CMBMODEL { STR_ID = "DATE_N", STR_NAME = "تاريخ فاكتور" },
            new CMBMODEL { STR_ID = "MABL_K", STR_NAME = "مبلغ كل فاكتور" },
            new CMBMODEL { STR_ID = "TAKHFIF", STR_NAME = "تخفيفات" },
            new CMBMODEL { STR_ID = "MABL_HAZ", STR_NAME = "هزينه" },
            new CMBMODEL { STR_ID = "MBAA", STR_NAME = "ماليات و عوارض" },
            new CMBMODEL { STR_ID = "GHABEL", STR_NAME = "مبلغ قابل پرداخت" },
            new CMBMODEL { STR_ID = "NPAR", STR_NAME = "جمع مبلغ پرداختي" },
            new CMBMODEL { STR_ID = "MANF", STR_NAME = "مانده فاكتور" },
            new CMBMODEL { STR_ID = "MANH", STR_NAME = "مانده حساب" },
            new CMBMODEL { STR_ID = "MEGH_K", STR_NAME = "مقدار كل كالا ها" },
            new CMBMODEL { STR_ID = "VAZN", STR_NAME = "وزن بار" },
            new CMBMODEL { STR_ID = "NULL", STR_NAME = "هيچي(خالي)" },
            new CMBMODEL { STR_ID = "ENTER", STR_NAME = "رفتن به اول سطر بعد(اينتر)" },
            new CMBMODEL { STR_ID = "SPC", STR_NAME = "فاصله" },
            new CMBMODEL { STR_ID = "REQUEST_NO", STR_NAME = "شماره درخواست" },
            new CMBMODEL { STR_ID = "BARNAMEH", STR_NAME = "شماره بارنامه" },
            new CMBMODEL { STR_ID = "DRIVER", STR_NAME = "نام راننده" },
            new CMBMODEL { STR_ID = "DRIVER_MOB", STR_NAME = "موبايل راننده" },
            new CMBMODEL { STR_ID = "CAMIUN_NUM", STR_NAME = "شماره ماشين" },
            new CMBMODEL { STR_ID = "TOZIH", STR_NAME = "توضيحات" }
        };

        public TABKINDS TabItemSelected { get; set; } = TABKINDS.FORUSH;
        public enum TABKINDS
        {
            FORUSH = 1,
            KHARID = 2,
            BACK_FORUSH = 3,
            BACK_KHARID = 4,
            SANAD_DARYAFT = 5,
            SANAD_PARDAKHT = 6,
            GET_CHECK = 7,
            PAY_CHECK = 8,
            PAZIRESH_REPAIR = 9,
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
            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            FILL_ALL_COMBOBOXES();

            DG_FORMAT_SUB_ReGetData(TABKINDS.FORUSH);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (PAYAM.IsFocused)
                {
                    return;
                }

                e.Handled = true;

                try
                {
                    if (sender is DataGrid DG)
                    {
                        if (DG.IsKeyboardFocusWithin)
                        {
                            if (DG.CurrentColumn != null)
                            {
                                int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                                bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                                bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                                if (isLastColumn)
                                {
                                    // If it's the last column, move focus to the first cell of next row
                                    if (isLastRow)
                                    {
                                        // Add focus to new row if needed
                                        DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                        var DEFINDEX = 0;
                                        int? defaultcolumnindex = DG.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "RADIF")?.DisplayIndex;
                                        if (defaultcolumnindex is null || defaultcolumnindex < 0)
                                            DEFINDEX = 0;
                                        else
                                            DEFINDEX = (int)defaultcolumnindex;

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DEFINDEX]);

                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            DG.BeginEdit();
                                        }), DispatcherPriority.Background);

                                        return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /*ignore*/ }



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
            //SMS_FIELS_COLUMN.ItemsSource = RST_SMS_FIELDS;

            OFILEDS_COMBO_DATA?.Clear();
            foreach (var item in RST_SMS_FIELDS)
            {
                OFILEDS_COMBO_DATA?.Add(item);
            }

            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //});
        }
        private async void BTN_SENDSMS_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SENDSMS.IsEnabled) { return; }

            if (string.IsNullOrEmpty(PAYAM.Text))
            {
                new Msgwin(false, "متن پیام نمیتواند خالی باشد").ShowDialog();
                return;
            }
            if (string.IsNullOrEmpty(NUM.Text))
            {
                new Msgwin(false, "شماره موبایل نمیتواند خالی باشد").ShowDialog();
                return;
            }
            else if (CL_LMethods.IsValidMobileNumber(PAYAM.Text))
            {
                new Msgwin(false, "شماره موبایل صحیح نیست").ShowDialog();
                return;
            }

            try
            {
                var SMSAC = new CL_SMSAC();

                List<SmsResultRecord> resultRecords = null;
                long idSms = 0;

                using (var httpClient = new HttpClient())
                {
                    // Get the user's decision from the UI thread.
                    bool shouldSendSms = await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا پیامک ارسال شود ؟");
                        msgwin.ShowDialog();
                        // Return true if DialogResult is true, otherwise false.
                        return msgwin.DialogResult == true;
                    });
                    if (!shouldSendSms)
                    {
                        // Exit the entire event handler if the user canceled.
                        return;
                    }

                    this.IsEnabled = false;

                    SmsManager smsManager = new SmsManager(SMSPINFO.SERVICE_TYPE, SMSPINFO.API_KEY, SMSPINFO.USERNAME, SMSPINFO.PASSWORD, SMSPINFO.LINE_NUMBER, httpClient);

                    //Final Send SMS
                    BulkSendResult bulkResult = await smsManager.SendBulkSms(PAYAM.Text, new[] { NUM.Text });
                    resultRecords = SmsResultProcessor.ConvertToRecords(bulkResult);
                    if ((bool)(resultRecords.FirstOrDefault()?.IsSentSuccess))
                    {
                        if (resultRecords.FirstOrDefault()?.MessageId != null)
                        {
                            idSms = Convert.ToInt32(resultRecords.FirstOrDefault()?.MessageId);
                        }
                    }

                    _ = AuditLogger.LogActionAsync(
                        actionType: "ارسال پیامک",
                        tableName: "فرمت پیامک",
                        recordId: NUM.Text,
                        oldValue: PAYAM.Text,
                        newValue: idSms.ToStringNullSafe(),
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    this.IsEnabled = true;

                    if (idSms < 0)
                    {
                        string resultSummary = SmsResultProcessor.GenerateResultSummary(bulkResult);
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            new Msgwin(false, resultSummary).Show();
                        });
                    }
                }

                if (resultRecords != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                {
                    new Msgwin(false, $"پیام شما با موفقیت ارسال شد \n" +
                        $"به کد رهگیری {idSms} \n" +
                        $"لطفاً توجه فرمایید که زمان رسیدن پیام ممکن است بین ۱۰ ثانیه تا ۵ دقیقه متغیر باشد، بسته به شرایط و شلوغی.").ShowDialog();
                }
                else
                {
                    if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                    {
                        new Msgwin(false, resultRecords?.FirstOrDefault()?.ErrorMessage).ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "پیام به دلیل خطا ارسال نشد!").ShowDialog();
                    }
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ارسال پیام , پیام ارسال نشد!").ShowDialog();
            }
        }

        private void UpdatePayamText()
        {
            StringBuilder messageBuilder = new StringBuilder();

            // Optional: Order the rows by RADIF (or another criteria) if desired
            foreach (var row in SMS_FORMATS_MODEL_DATA.OrderBy(r => r.RADIF))
            {
                switch (row.SMS_FIELS?.ToString())
                {
                    case "ENTER":
                        messageBuilder.AppendLine(row.SMS_CAPTION.ToStringNullSafe());
                        break;
                    case "null":
                    case "Null":
                    case "NULL":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe());
                        break;
                    case "SPC":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " ");
                        break;
                    case "CUST_NO1":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " شركت قائم رايانه عرش");
                        break;
                    case "CUST_NO":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 115-1-110");
                        break;
                    case "MOLAH":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " راننده حسن");
                        break;
                    case "NUMBER":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 2531");
                        break;
                    case "DATE_N":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 1391/06/02");
                        break;
                    case "MABL_K":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 2,115,200 ريال");
                        break;
                    case "TAKHFIF":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 200 ريال");
                        break;
                    case "MABL_HAZ":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 800 ريال");
                        break;
                    case "MBAA":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 105,790 ريال");
                        break;
                    case "GHABEL":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 2,221,590 ريال");
                        break;
                    case "NPAR":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 0 ريال");
                        break;
                    case "MANF":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 2,221,590 ريال");
                        break;
                    case "MANH":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 4,765,000 ريال");
                        break;
                    case "MEGH_K":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 105");
                        break;
                    case "VAZN":
                        messageBuilder.Append(row.SMS_CAPTION.ToStringNullSafe() + " 2150");
                        break;

                    default:
                        messageBuilder.Append("" + row.SMS_CAPTION.ToStringNullSafe() + "");

                        break;
                }

                // Add a Unicode line separator (U+2028) after each row's output.
                //messageBuilder.Append("\u2028");

                //messageBuilder.Append("\u202B" + "\u202C"); // Wrap with RLE and PDF

                ////Corret:
                ///یک Zero-Width Space (فضای بدون عرض) یک کاراکتر کنترلی در استاندارد Unicode است که با کد نقطه U+200B مشخص می‌شود. این کاراکتر به طور ظاهری هیچ فضایی ایجاد نمی‌کند—یعنی در نمایش، هیچ فاصله یا شکاف قابل مشاهده‌ای ایجاد نمی‌کند—اما به موتور رندرینگ متن یک نقطه تقسیم (delimiter) می‌دهد.
                ///Append a zero - width space as an invisible delimiter between rows
                //messageBuilder.Append("\u200B");

            }
            PAYAM.Text = messageBuilder.ToStringNullSafe();
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

        private void DG_FORMAT_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid DG_FORMAT_SUB)
            {
                if (NowIsReady && DG_FORMAT_SUB.SelectedItem != null)
                {
                    if (DG_FORMAT_SUB.Items.Count > 0)
                        CURRENT_ROW_SMS_FORMATS_MODEL_INDEX = DG_FORMAT_SUB.SelectedIndex;

                    if (!(e is null) && DG_FORMAT_SUB.SelectedItem is not null)
                    {
                        if (DG_FORMAT_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                        {
                            SMS_FORMATS_MODEL_WAS_ROW_ITEM = ((SMS_FORMATS_MODEL)DG_FORMAT_SUB.SelectedItem).Clone() as SMS_FORMATS_MODEL;
                        }
                    }
                }
            }

        }
        private void DG_FORMAT_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (sender is DataGrid DG_FORMAT_SUB)
                {
                    if (!(DG_FORMAT_SUB.Items.Count < 1) && !(DG_FORMAT_SUB.SelectedItem is null))
                    {
                        CURRENT_ROW_SMS_FORMATS_MODEL_INDEX = DG_FORMAT_SUB.SelectedIndex;
                    }
                }
                //IF IS NOT NULL

            }
        }
        private void DG_FORMAT_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }
        private void DG_FORMAT_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                DG_FORMAT_SUB_IsFocused = false;
            }
            else
            {
                DG_FORMAT_SUB_IsFocused = true;
            }
        }
        public void DG_FORMAT_SUB_ReGetData(TABKINDS TABITEM)
        {
            SMS_FORMATS_MODEL_DATA?.Clear();
            var records = dbms.DoGetDataSQL<SMS_FORMATS_MODEL>($"SELECT SMSTY, RADIF, SMS_FIELS, SMS_CAPTION, CRT, UID, ID FROM SMS_FORMATS WHERE SMSTY = {(int)TABITEM} ORDER BY RADIF").ToList();
            foreach (var item in records)
            {
                SMS_FORMATS_MODEL_DATA?.Add(item);
            }

            UpdatePayamText();
        }
        private void DG_FORMAT_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is DataGrid DG_FORMAT_SUB)
            {
                string CURRENT_COLUMN_NAME = "";
                if (DG_FORMAT_SUB.CurrentCell.Column is not null)
                {
                    CURRENT_COLUMN_NAME = DG_FORMAT_SUB.CurrentCell.Column?.SortMemberPath;
                }

                if (e.Key == Key.Delete)
                {
                    e.Handled = true;
                    BTN_DELETE_Click(null, null);
                }

                if (e.Key == Key.Add)
                {
                    if (CURRENT_COLUMN_NAME is "")
                    {
                        e.Handled = true;
                        var text = "000";
                        var target = Keyboard.FocusedElement;
                        var routedEvent = TextCompositionManager.TextInputEvent;

                        target.RaiseEvent(
                            new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, target, text))
                            { RoutedEvent = routedEvent });
                    }
                }
                if (e.Key == Key.Subtract)
                {
                    if (CURRENT_COLUMN_NAME is "")
                    {
                        e.Handled = true;
                        var text = "00";
                        var target = Keyboard.FocusedElement;
                        var routedEvent = TextCompositionManager.TextInputEvent;

                        target.RaiseEvent(
                            new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, target, text))
                            { RoutedEvent = routedEvent });
                    }
                }
            }

        }
        private void DG_FORMAT_SUB_CANCEL_EDIT(DataGrid DG_FORMAT_SUB, DataGridEditingUnit? _RC_ = null)
        {
            DG_FORMAT_SUB.Dispatcher.InvokeAsync(() =>
            {
                DG_FORMAT_SUB.CellEditEnding -= DG_FORMAT_SUB_CellEditEnding;
                DG_FORMAT_SUB.RowEditEnding -= DG_FORMAT_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_FORMAT_SUB.CancelEdit();
                }
                else
                {
                    DG_FORMAT_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DG_FORMAT_SUB.RowEditEnding += DG_FORMAT_SUB_RowEditEnding;
                DG_FORMAT_SUB.CellEditEnding += DG_FORMAT_SUB_CellEditEnding;
            });
        }
        private void DG_FORMAT_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (sender is DataGrid DG_FORMAT_SUB)
            {
                #region REFILL_CURRENTS
                DataGridRow row1 = e.Row;
                int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
                ComboBox Comboval = null; TextBox TexboVal = null;
                if (!(e.EditingElement is null) && e.EditingElement is TextBox)
                {
                    TexboVal = (TextBox)e.EditingElement;
                }
                if (!(e.EditingElement is null))
                {
                    Comboval = e.EditingElement as ComboBox;
                }
                if (!ReferenceEquals(Comboval, null))
                {
                    SMS_FORMATS_MODEL_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
                }
                else if (!ReferenceEquals(TexboVal, null))
                {
                    SMS_FORMATS_MODEL_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
                }

                SMS_FORMATS_MODEL_CURRENT_ROW_ITEMS = e.Row.Item as SMS_FORMATS_MODEL;
                #endregion

                if (e.Column.SortMemberPath == "RADIF")
                {
                    if (string.IsNullOrEmpty(SMS_FORMATS_MODEL_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                    {
                        universControl.PopNotifyShow("شماره ردیف نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                        DG_FORMAT_SUB_CANCEL_EDIT(DG_FORMAT_SUB, DataGridEditingUnit.Cell);
                    }
                }

                if (e.Column.SortMemberPath == "SMS_CAPTION")
                {
                    if (string.IsNullOrEmpty(SMS_FORMATS_MODEL_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                    {
                        universControl.PopNotifyShow("عنوان فیلد نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                        DG_FORMAT_SUB_CANCEL_EDIT(DG_FORMAT_SUB, DataGridEditingUnit.Cell);
                    }
                }

            }

        }
        private void DG_FORMAT_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (sender is DataGrid DG_FORMAT_SUB)
            {
                if (!(e is null) && DG_FORMAT_SUB.SelectedItem is not null)
                {
                    if (DG_FORMAT_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        SMS_FORMATS_MODEL_WAS_ROW_ITEM = ((SMS_FORMATS_MODEL)DG_FORMAT_SUB.SelectedItem).Clone() as SMS_FORMATS_MODEL;
                    }
                }
            }

        }
        private void DG_FORMAT_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            if (sender is DataGrid DG_FORMAT_SUB)
            {
                var ROW = e.Row.Item as SMS_FORMATS_MODEL;
                if (!BodyIsValid(ROW, DG_FORMAT_SUB))
                {
                    DG_FORMAT_SUB_CANCEL_EDIT(DG_FORMAT_SUB);
                    return;
                }

                long? ID = null;
                try
                {
                    if (ROW?.ID == null) //INSERT
                    {
                        //OUTPUT INSERTED.ID
                        ID = dbms.DoGetDataSQL<long?>(@$"INSERT INTO [SMS_FORMATS] ([SMSTY], [RADIF] ,[SMS_FIELS], [SMS_CAPTION])
                                                         OUTPUT INSERTED.ID VALUES
                                                         ({(int)TabItemSelected},{ROW.RADIF}, N'{ROW.SMS_FIELS}', N'{ROW.SMS_CAPTION}' )").FirstOrDefault();
                        if (ID != null)
                        {
                            ROW.ID = ID;
                        }
                    }
                    else //UPDATE
                    {
                        dbms.DoExecuteSQL(@$"
                                            UPDATE [SMS_FORMATS]
                                            SET 
                                                 [RADIF] = N'{ROW.RADIF}', 
                                                 [SMS_FIELS] = N'{ROW.SMS_FIELS}', 
                                                 [SMS_CAPTION] = N'{ROW.SMS_CAPTION}'
                                            OUTPUT INSERTED.ID
                                            WHERE [SMSTY] = {(int)TabItemSelected} AND ID = {ROW.ID}");

                    }

                    UpdatePayamText();
                }
                catch (SqlException ex)
                {
                    DG_FORMAT_SUB_CANCEL_EDIT(DG_FORMAT_SUB);

                    if (ex.Number == 2601 || ex.Number == 2627)
                    {
                        new Msgwin(false, "این سطر حاوی داده تکراری است , آنرا اصلاح کنید").ShowDialog();
                        return;
                    }
                    else
                    {
                        new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                    }
                }

            }

        }
        private bool BodyIsValid(SMS_FORMATS_MODEL _row, DataGrid DG_FORMAT_SUB)
        {
            var ROW = _row;

            var errors = (from object i in DG_FORMAT_SUB.ItemsSource
                          let c = DG_FORMAT_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!byte.TryParse(ROW?.RADIF.ToStringNullSafe(), out byte _) || ROW?.RADIF == 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ردیف صحیح وارد نشده" });
            }
            else
            {
                //if (ROW?.ID == null && ROW?.RADIF > 0)
                //{
                //    bool IsRadifExist = SMS_FORMATS_MODEL_DATA.Any(item => item.RADIF == ROW?.RADIF);
                //    if (IsRadifExist)
                //    {
                //        ErrosMessages.Add(new MsgModel { MessageText_U = "این شماره ردیف وارد شده از قبل وجود دارد , آنرا تغییر دهید" });
                //    }
                //}
            }

            if (string.IsNullOrEmpty(ROW?.SMS_CAPTION))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "عنوان فیلد نمیتواند خالی باشد" });
            }
            else if (ROW?.SMS_CAPTION.Length > 100)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "عنوان فیلد نمیتواند بیش از 100 کاراکتر باشد" });
            }

            if (string.IsNullOrEmpty(ROW?.SMS_FIELS))
            {
                //ErrosMessages.Add(new MsgModel { MessageText_U = "بخش فیلد انتخالی نمیتواند خالی باشد" });
            }
            else if (ROW?.SMS_FIELS.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بخش فیلد انتخالی نمیتواند بیش از 50 کاراکتر باشد" });
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        public DataGrid GetFocusedDataGrid()
        {
            // Get the element that currently has keyboard focus in the active window.
            //IInputElement focusedElement = FocusManager.GetFocusedElement(Application.Current.Windows.OfType<Window>().FirstOrDefault());
            IInputElement focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement == null)
                return null;

            // Traverse up the visual tree until we find a DataGrid.
            DependencyObject current = focusedElement as DependencyObject;
            while (current != null)
            {
                if (current is DataGrid dataGrid)
                    return dataGrid;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_DELETE.IsEnabled) { return; }

            var TheElement = GetFocusedDataGrid();

            if (TheElement is DataGrid DG_FORMAT_SUB)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    if (SMS_FORMATS_MODEL_DATA.Count > 0) //Any Sub Items ?
                    {
                        if (!(DG_FORMAT_SUB?.SelectedItems is null))
                        {
                            bool IsDeletedSomething = false;
                            List<MsgModel> ErrosMessages = new List<MsgModel>();

                            var editableCollectionView = DG_FORMAT_SUB.Items as IEditableCollectionView;
                            if (editableCollectionView != null && editableCollectionView.IsEditingItem) { editableCollectionView.CommitEdit(); }

                            _ = AuditLogger.LogActionAsync(
                                             actionType: "DELETE",
                                             tableName: "فرمت اس ام اس",
                                             recordId: TabItemSelected.ToStringNullSafe(),
                                             oldValue: "",
                                             newValue: null,
                                             additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < DG_FORMAT_SUB.SelectedItems.Count; i++)
                            {
                                var item = DG_FORMAT_SUB.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(DG_FORMAT_SUB, item))
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                long? _ID_ = (long?)item.GetType().GetProperty("ID").GetValue(item);

                                if (_ID_ != null)
                                {
                                    try
                                    {
                                        IsDeletedSomething = true;

                                        dbms.DoExecuteSQL($@"DELETE FROM dbo.SMS_FORMATS WHERE ID = {_ID_}");

                                        SMS_FORMATS_MODEL_DATA.Remove((SMS_FORMATS_MODEL)item);

                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 547)
                                        {
                                            //e.Handled = true;
                                            ErrosMessages.Add(new MsgModel { MessageText_U = $"سطر مورد نظر دارای گردش است و نمیتوان آنرا حذف کرد" });
                                        }
                                    }
                                }
                            }

                            if (ErrosMessages.Count > 0)
                            {
                                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                                new MsgListwin(false, ErrosMessages).ShowDialog();

                                return;
                            }

                            if (IsDeletedSomething)
                            {
                                UpdatePayamText();
                            }
                        }

                    }
                }
            }


        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems)
            {
                if (added is TabItem addedTab)
                {
                    if (addedTab.Tag != null)
                    {
                        TabItemSelected = (TABKINDS)Convert.ToByte(addedTab.Tag);

                        if (NowIsReady)
                        {
                            DG_FORMAT_SUB_ReGetData(TabItemSelected);
                        }
                    }
                }
            }

        }
    }
}
