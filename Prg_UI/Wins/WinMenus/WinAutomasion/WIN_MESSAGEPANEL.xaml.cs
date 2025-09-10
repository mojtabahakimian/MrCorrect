using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Syncfusion.UI.Xaml.Diagram;
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
using System.Windows.Media;
using static Prg_Proccessy.Generaly.CL_Generaly;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{
    public partial class WIN_MESSAGEPANEL : Window
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

        public WIN_MESSAGEPANEL(long? _personel_ = null, string? _payam_ = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (_personel_ != null)
            {
                PERSONEL_PARAM = (long)_personel_;
            }
            if (_payam_ != null)
            {
                PAYAM_PARAM = _payam_;
            }

            ThreadSafeProperties.IsStillWorking = true;
        }
        public long PERSONEL_PARAM { get; set; }
        public string PAYAM_PARAM { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; set; } = new ObservableCollection<CutsomStatus_Model>();
        public string WAS_COMP_COD { get; set; }
        public string? DEFAULTVAL_COMPCODE { get; private set; }
        public bool DoesTextedOnCOMP_COD { get; set; } = false;
        public ObservableCollection<CUST_HESAB> COMP_COD_DATA { get; set; } = new ObservableCollection<CUST_HESAB>();
        public Visual I_AM_MESSAGEPANEL { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        List<S_USER_SALADTL> MAIN_PERSONS;
        public ObservableCollection<S_USER_SALADTL> Users { get; set; } = new ObservableCollection<S_USER_SALADTL>();
        public ObservableCollection<S_USER_SALADTL> GotUsers { get; set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_MESSAGEPANEL = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();

            #region FORM_LOAD

            COMP_COD.Focus();
            USERNAME.Text = (string)CL_HESABDARI.UCurrentUser();

            STDATE.Text = Tarikh.SlashyFullDate;
            STTIME.Text = DateTime.Now.ToString("HH:mm");

            //var lastPersonel = dbms.DoGetDataSQL<string>($"SELECT TOP 1 PERSONEL FROM dbo.MESAGEP WHERE (USERNAME = N'{USERNAME.Text}') ORDER BY IDNUM DESC").FirstOrDefault();
            //if (!string.IsNullOrEmpty(lastPersonel))
            //{
            //    PERSONEL.SelectedValue = lastPersonel; PERSONEL.Items.Refresh();
            //}


            if (this.Tag?.ToString() == "1")
            {
                PERSONEL.SelectedValue = PERSONEL_PARAM; PERSONEL.Items.Refresh();
                COMP_COD.Focus();

                Application.Current.Windows.OfType<INBOXPAN>().FirstOrDefault()?.Close();
            }
            else if (this.Tag?.ToString() == "2")
            {
                PERSONEL.SelectedValue = PERSONEL_PARAM; PERSONEL.Items.Refresh();
                COMP_COD.Focus();
            }
            else if (this.Tag?.ToString() == "3")
            {
                PERSONEL.SelectedValue = PERSONEL_PARAM; PERSONEL.Items.Refresh();
                PAYAM.Text = PAYAM_PARAM;
                Application.Current.Windows.OfType<INBOXPAN>().FirstOrDefault()?.Close();
                PERSONEL.Focus();
            }
            else if (this.Tag?.ToString() == "4")
            {
                PERSONEL.SelectedValue = PERSONEL_PARAM; PERSONEL.Items.Refresh();
                PAYAM.Text = PAYAM_PARAM;
                PERSONEL.Focus();
            }

            Command33.IsEnabled = CL_HESABDARI.LETSGO("SMS");
            #endregion

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!PAYAM.IsFocused)
                {
                    e.Handled = true;

                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }

            if (e.Key is Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (GRID_MOREUSER.Visibility == Visibility.Visible)
                {
                    GRID_MOREUSER.Visibility = Visibility.Hidden;
                }
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
            //کبموباکس مجری
            //var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, USERCO FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD} AND ENABL = 0").ToList();

            //bool IsHameh = false;
            //foreach (var rows in rst_personel)
            //{
            //    if (string.IsNullOrEmpty(rows?.SAL_NAME))
            //    {
            //        IsHameh = true; break;
            //    }
            //    else
            //    {
            //        rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
            //    }
            //}

            //if (IsHameh)
            //{
            //    rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();
            //    foreach (var rows in rst_personel)
            //    {
            //        if (!string.IsNullOrEmpty(rows?.SAL_NAME))
            //        {
            //            rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
            //        }
            //    }
            //}

            //کبموباکس مجری
            string sql = @"
               SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD as USERCO
               FROM SALA_DTL sd
               LEFT JOIN USER_PERSONEL_ORDER uo 
                    ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
               WHERE sd.ENABL = 0
               ORDER BY
                    CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                    uo.SORT_ORDER, sd.SAL_NAME";
            var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);

            //مجری در دیتاگرید
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.SelectedValue = null;
            PERSONEL.SelectedValue = Baseknow.USERCOD;


            DEFAULTVAL_COMPCODE = dbms.DoGetDataSQL<string>($"SELECT HES FROM SALA_DTL WHERE IDD = {Baseknow.USERCOD}").FirstOrDefault();
            COMP_COD_DATA?.Clear();
            var compcod = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT TOP(1) hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'{DEFAULTVAL_COMPCODE}' ORDER BY NAME ").ToList();
            for (int i = 0; i < compcod.Count; i++)
            {
                COMP_COD_DATA.Add(compcod[i]);
                WAS_COMP_COD = compcod[i].NAME;
            }
            COMP_COD.SelectedValue = DEFAULTVAL_COMPCODE; COMP_COD.Items.Refresh();



            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "انجام نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "انجام شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "لغو شده" });
            STATUS.ItemsSource = STATUS_COMBO_DATA;

            MAIN_PERSONS = dbms.DoGetDataSQL<S_USER_SALADTL>($"SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD,HES FROM SALA_DTL WHERE (ENABL = 0) AND (IDD <> 1)").ToList();
            for (int i = 0; i < MAIN_PERSONS.Count; i++)
                MAIN_PERSONS[i].SAL_NAME = CL_HESABDARI.DECODEUN(MAIN_PERSONS[i].SAL_NAME.ToString()).Replace("ي", "ی").Replace("ك", "ک");

            foreach (var item in MAIN_PERSONS)
            {
                Users.Add(item);
            }

        }
        private void COMP_COD_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            if (COMP_COD.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            var CUTSNO_TEX = (TextBox)COMP_COD.Template.FindName("PART_EditableTextBox", COMP_COD);
            if (CUTSNO_TEX == null)
                return;

            string CurrentText = CUTSNO_TEX.Text?.Trim() ?? string.Empty;

            if (CurrentText == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("WIN_MESSAGEPANEL", I_AM_MESSAGEPANEL);
                CMBSearch.ShowDialog();
            }
            //else if (WAS_COMP_COD != CUTSNO_TEX.Text) // متنش با متن قبلی مورد تایید فرق کرده و میخوایم با فقط تایپ شدن جستجو کنه
            else if (DoesTextedOnCOMP_COD == true)
            {
                CL_HESAB_SEARCH.Go_Search_Hesab(CurrentText, "WIN_MESSAGEPANEL", I_AM_MESSAGEPANEL);
            }

            if (COMP_COD.SelectedValue == null)
            {
                universControl.PopNotifyShow("تماس گیرنده نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                WAS_COMP_COD = COMP_COD.Text; //اگر انتخاب جدید درست بوده بیا مقداری که باهاش مقدار مورد تایید قبلی رو برای جستجو مقایسه میکنی رو بروز کن
            }
            DoesTextedOnCOMP_COD = false;


            // Safe check for blocked account
            if (!string.IsNullOrWhiteSpace(CurrentText))
            {
                try
                {
                    if (CL_HESABDARI.BLOCKEDMK(CurrentText))
                    {
                        COMP_COD.SelectedValue = null;
                        COMP_COD.SelectedValue = WAS_COMP_COD;
                        COMP_COD.Items.Refresh();

                        new Msgwin(false, "تماس (حساب) گیرنده مورد نظر مسدود است!").ShowDialog();
                    }
                }
                catch (FormatException)
                {
                    // Handle invalid format specifically
                    COMP_COD.SelectedValue = WAS_COMP_COD;
                    COMP_COD.Items.Refresh();
                    new Msgwin(false, "فرمت کد تماس (حساب) گیرنده نامعتبر است").ShowDialog();
                }
                catch (Exception)
                {
                    COMP_COD.SelectedValue = WAS_COMP_COD;
                    COMP_COD.Items.Refresh();
                    new Msgwin(false, "خطا در بررسی وضعیت تماس (حساب) گیرنده").ShowDialog();
                }
            }

        }
        private void COMP_COD_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete || e.Key is Key.Back)
            {
                DoesTextedOnCOMP_COD = true;
            }
        }
        private void COMP_COD_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DoesTextedOnCOMP_COD = true;
        }

        private void BTN_GIRANDEGAN_MORE_Click(object sender, RoutedEventArgs e)
        {
            if (GRID_MOREUSER.Visibility == Visibility.Hidden || GRID_MOREUSER.Visibility == Visibility.Collapsed)
            {
                GRID_MOREUSER.Visibility = Visibility.Visible;
            }
            else
            {
                GRID_MOREUSER.Visibility = Visibility.Hidden;
            }
        }

        int ClickCounter = 1;
        private void BTN_SELECTALL_Click(object sender, RoutedEventArgs e)
        {
            ClickCounter++;

            if (ClickCounter % 2 == 0)
            {
                foreach (var item in Users)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in Users)
                {
                    item.IsSelected = false;
                }
            }

            CheckListBox.Items.Refresh();
        }
        private List<S_USER_SALADTL> WasSelectedItems { get; set; } = new List<S_USER_SALADTL>();
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            string searchText = SearchBox.Text.ToLower();

            // Store the current selection state
            WasSelectedItems = Users.Where(u => u.IsSelected).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                var FilteredData = new ObservableCollection<S_USER_SALADTL>(MAIN_PERSONS.Where(u => u.SAL_NAME.ToLower().Contains(searchText)));

                Users.Clear();
                foreach (var item in FilteredData)
                {
                    Users.Add(item);
                }
            }
            else
            {
                Users.Clear();
                foreach (var item in MAIN_PERSONS)
                {
                    Users.Add(item);
                }

                // Restore the selection state
                foreach (var selectedItem in WasSelectedItems)
                {
                    var user = Users.FirstOrDefault(u => u.IDD == selectedItem.IDD);
                    if (user != null)
                    {
                        user.IsSelected = true;
                    }
                }
            }
        }
        private void BTN_APPLY_Click(object sender, RoutedEventArgs e)
        {
            GotUsers = new ObservableCollection<S_USER_SALADTL>(Users.Where(x => x.IsSelected));

            GRID_MOREUSER.Visibility = Visibility.Hidden;
        }


        private bool HeaderIsValid(bool _ShowMessage_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(PERSONEL.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مجری نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(COMP_COD.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تماس گیرنده نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(STATUS.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت نمیتواند خالی باشد" });
            }

            if (string.IsNullOrEmpty(PAYAM.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "متن پیام نمیتواد خالی باشد" });
            }

            if (ErrosMessages.Count > 0)
            {
                if (_ShowMessage_)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }
        private void BTN_SENDMATN_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            var parameters = new
            {
                PERSONEL = PERSONEL.SelectedValue,
                COMP_COD = COMP_COD.SelectedValue,
                PAYAM = PAYAM.Text,
                STATUS = STATUS.SelectedValue,
                STDATE = STDATE.Text.ToRawTarikh(),
                STTIME = DateTime.Now.ToString("HHmm") /*STTIME.Text*/,
                USERNAME = USERNAME.Text,
                IsNotifyCalled = false,
                UID = Baseknow.USERCOD
            };

            if (string.IsNullOrEmpty(IDNUM.Text))
            {
                var _idnum_ = dbms.DoGetDataSQL<int?>(@"INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME, UID, IsNotifyCalled)
                                    OUTPUT INSERTED.IDNUM
                                    VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME, @UID,@IsNotifyCalled)", parameters).FirstOrDefault();

                IDNUM.Text = _idnum_.ToString();
            }
            else
            {
                dbms.DoExecuteSQL(@"UPDATE dbo.MESAGEP 
                                    SET PERSONEL = @PERSONEL, COMP_COD = @COMP_COD, PAYAM = @PAYAM, 
                                        STATUS = @STATUS, STDATE = @STDATE, STTIME = @STTIME, 
                                        USERNAME = @USERNAME, UID = @UID , IsNotifyCalled = !IsNotifyCalled
                                    WHERE IDNUM = @IDNUM",
                                    new { parameters, IDNUM = IDNUM.Text });
            }

            if (GotUsers != null)
            {
                foreach (var row in GotUsers)
                {
                    if (row.IDD != Convert.ToInt32(PERSONEL.SelectedValue))
                    {
                        var newParameters = new
                        {
                            PERSONEL = row.IDD /*USERCO*/,
                            COMP_COD = COMP_COD.SelectedValue,
                            PAYAM = PAYAM.Text,
                            STATUS = STATUS.SelectedValue,
                            STDATE = STDATE.Text.ToRawTarikh(),
                            STTIME = DateTime.Now.ToString("HHmm"),
                            USERNAME = USERNAME.Text,
                            UID = Baseknow.USERCOD
                        };

                        dbms.DoExecuteSQL(@"INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME, UID)
                                    VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME, @UID)", newParameters);
                    }
                }
            }


            new Msgwin(false, "پيام ارسال شد....!").ShowDialog();
            this.Close();
        }
        private async void Command33_Click(object sender, RoutedEventArgs e)
        {
            Command33.IsEnabled = false;

            if (!HeaderIsValid())
            {
                Command33.IsEnabled = true;
                return;
            }

            Process Prc = ProcLoader.Start();

            var parameters = new
            {
                PERSONEL = PERSONEL.SelectedValue,
                COMP_COD = COMP_COD.SelectedValue,
                PAYAM = PAYAM.Text,
                STATUS = STATUS.SelectedValue,
                STDATE = STDATE.Text.ToRawTarikh(),
                STTIME = DateTime.Now.ToString("HHmm") /*STTIME.Text*/,
                USERNAME = USERNAME.Text,
                IsNotifyCalled = false,
                UID = Baseknow.USERCOD
            };

            int newIdnum;
            if (string.IsNullOrEmpty(IDNUM.Text))
            {
                newIdnum = dbms.DoGetDataSQL<int>(@"
                        INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME, UID , IsNotifyCalled)
                        OUTPUT INSERTED.IDNUM
                        VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME, @UID , @IsNotifyCalled);
                        SELECT SCOPE_IDENTITY();", parameters).FirstOrDefault();

                IDNUM.Text = newIdnum.ToString();
            }
            else
            {
                await dbms.DoExecuteSQLAsync(@"
                        UPDATE dbo.MESAGEP 
                        SET PERSONEL = @PERSONEL, COMP_COD = @COMP_COD, PAYAM = @PAYAM, 
                            STATUS = @STATUS, STDATE = @STDATE, STTIME = @STTIME, 
                            USERNAME = @USERNAME, UID = @UID, IsNotifyCalled = @IsNotifyCalled
                        WHERE IDNUM = @IDNUM",
                    new { parameters, IDNUM = int.Parse(IDNUM.Text) });
                newIdnum = int.Parse(IDNUM.Text);
            }


            // Send SMS
            var SMSAC = new CL_SMSAC();
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (GotUsers != null)
            {
                if ((bool)Baseknow.PRMFR)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا پیامک ارسال شود ؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult == false)
                    {
                        return;
                    }
                }

                foreach (var row in GotUsers)
                {
                    if (row.IDD != Convert.ToInt32(PERSONEL.SelectedValue))
                    {
                        try
                        {
                            var newParameters = new
                            {
                                PERSONEL = row.IDD /*USERCO*/,
                                COMP_COD = COMP_COD.SelectedValue,
                                PAYAM = PAYAM.Text,
                                STATUS = STATUS.SelectedValue,
                                STDATE = STDATE.Text.ToRawTarikh(),
                                STTIME = DateTime.Now.ToString("HHmm"),
                                USERNAME = USERNAME.Text,
                                UID = Baseknow.USERCOD
                            };

                            int additionalIdnum = (int)await dbms.DoExecuteSQLAsync(@"INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME, UID)
                                                                                  VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME, @UID);
                                                                                  SELECT SCOPE_IDENTITY();", newParameters);

                            var _THEPERSON_ = CL_HESABDARI.GETUSERNAME((int)row.IDD);

                            var RESULT = await SMSAC.ErselSmsAsync(_THEPERSON_, $"{PAYAM.Text} - {USERNAME.Text}", additionalIdnum, 1, false, true);

                            List<MSGMODEL.SmsResultRecord>? resultRecords = null;
                            if (RESULT != null)
                            {
                                resultRecords = SmsResultProcessor.ConvertToRecords(RESULT);
                            }
                            if (RESULT != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                            {
                                //new Msgwin(false, "پيام ارسال شد....!").ShowDialog();
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = $"پیام به این شخص : {row.SAL_NAME} {resultRecords?.FirstOrDefault()?.ErrorMessage} " });
                                }
                                else
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = $"پیام به این شخص : {row.SAL_NAME} با خطا مواجه شد ! " });
                                }
                            }
                        }
                        catch (Exception)
                        {
                            ErrosMessages.Add(new MsgModel { MessageText_U = $"خطا در انجام عملیات ارسال پیام به این شخص : {row.SAL_NAME} ! " });
                        }
                    }
                }
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
            }

            try
            {
                var RESULT0 = await SMSAC.ErselSmsAsync(CL_HESABDARI.GETUSERCO((int)PERSONEL.SelectedValue), $"{PAYAM.Text} - {USERNAME.Text}", newIdnum, 1, false);
                List<MSGMODEL.SmsResultRecord>? resultRecords = null;

                if (RESULT0 != null)
                {
                    resultRecords = SmsResultProcessor.ConvertToRecords(RESULT0);
                }
                if (RESULT0 != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                {
                    new Msgwin(false, $"پيام {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ارسال شد....!").ShowDialog();
                }
                else
                {
                    if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                    {
                        new Msgwin(false, $"{resultRecords?.FirstOrDefault()?.ErrorMessage} {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ").ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, $"پیام به خاطر خطا {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ارسال نشد!").ShowDialog();
                    }

                }
            }
            catch (Exception)
            {
                new Msgwin(false, $"خطا در انجام عملیات ارسال پیام {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} , پیام ارسال نشد!").ShowDialog();
            }

            Command33.IsEnabled = true;
            ProcLoader.Stop(Prc);
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ThreadSafeProperties.IsStillWorking = false;
        }
    }
}