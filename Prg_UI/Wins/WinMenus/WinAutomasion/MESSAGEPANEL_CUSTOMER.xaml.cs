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
using Syncfusion.ProjIO;
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
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{
    public partial class MESSAGEPANEL_CUSTOMER : Window
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

        public MESSAGEPANEL_CUSTOMER(long? _personel_ = null, string? _payam_ = null)
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
        public Visual I_AM_MESSAGEPANEL_CUSTOM { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_MESSAGEPANEL_CUSTOM = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();


            #region FORM_LOAD

            COMP_COD.Focus();
            USERNAME.Text = (string)CL_HESABDARI.UCurrentUser();

            //var lastPersonel = dbms.DoGetDataSQL<string>($"SELECT TOP 1 PERSONEL FROM dbo.MESAGEP WHERE (USERNAME = N'{USERNAME.Text}') ORDER BY IDNUM DESC").FirstOrDefault();
            //if (!string.IsNullOrEmpty(lastPersonel))
            //{
            //    PERSONEL.SelectedValue = lastPersonel; PERSONEL.Items.Refresh();
            //}

            Command33.IsEnabled = CL_HESABDARI.LETSGO("SMS");

            #endregion

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
            //کبموباکس مجری
            //var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, USERCO FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD}").ToList();

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
            //    rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();
            //    foreach (var rows in rst_personel)
            //    {
            //        if (!string.IsNullOrEmpty(rows?.SAL_NAME))
            //        {
            //            rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
            //        }
            //    }
            //}

            var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();
            foreach (var rows in rst_personel)
            {
                if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }


            DEFAULTVAL_COMPCODE = dbms.DoGetDataSQL<string>($"SELECT HES FROM SALA_DTL WHERE IDD = {Baseknow.USERCOD}").FirstOrDefault();
            COMP_COD_DATA?.Clear();
            var compcod = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT TOP(1) hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'{DEFAULTVAL_COMPCODE}' ORDER BY NAME ").ToList();
            for (int i = 0; i < compcod.Count; i++)
            {
                COMP_COD_DATA.Add(compcod[i]);
                WAS_COMP_COD = compcod[i].NAME;
            }
            COMP_COD.SelectedValue = DEFAULTVAL_COMPCODE; COMP_COD.Items.Refresh();


            //مجری در دیتاگرید
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.SelectedValue = null;
            PERSONEL.SelectedValue = Baseknow.USERCOD;

            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "مشاهده نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "مشاهده شده" });
            STATUS.ItemsSource = STATUS_COMBO_DATA;
            STATUS.SelectedValue = 1; STATUS.Items.Refresh();

        }
        private void COMP_COD_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (COMP_COD.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            var CUTSNO_TEX = (TextBox)COMP_COD.Template.FindName("PART_EditableTextBox", COMP_COD);

            if (COMP_COD.SelectedValue is not null)
            {
                if ((COMP_COD.SelectedItem as CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text.Trim() == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("MESSAGEPANEL_CUSTOMER", I_AM_MESSAGEPANEL_CUSTOM);
                CMBSearch.ShowDialog();
            }
            //else if (WAS_COMP_COD != CUTSNO_TEX.Text) // متنش با متن قبلی مورد تایید فرق کرده و میخوایم با فقط تایپ شدن جستجو کنه
            else 
            {
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + CUTSNO_TEX.Text + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;

                    if (!COMP_COD_DATA.Any(item => item?.hes == thevalue))
                    {
                        COMP_COD_DATA.Add(new CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    COMP_COD.SelectedValue = thevalue;
                    COMP_COD.Items.Refresh();
                }
                else
                {
                    CL_HESAB_SEARCH.Go_Search_Hesab(CUTSNO_TEX.Text, "MESSAGEPANEL_CUSTOMER", I_AM_MESSAGEPANEL_CUSTOM);
                }
            }
            
            if (COMP_COD.SelectedValue is null)
            {
                COMP_COD.SelectedValue = WAS_COMP_COD;
                COMP_COD.Items.Refresh();
                universControl.PopNotifyShow("تماس گیرنده نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                WAS_COMP_COD = COMP_COD.SelectedValue.ToString(); //اگر انتخاب جدید درست بوده بیا مقداری که باهاش مقدار مورد تایید قبلی رو برای جستجو مقایسه میکنی رو بروز کن
            }
            DoesTextedOnCOMP_COD = false;

            if (CL_HESABDARI.BLOCKEDMK(CUTSNO_TEX.Text))
            {
                COMP_COD.SelectedValue = null;
                COMP_COD.SelectedValue = WAS_COMP_COD;
                COMP_COD.Items.Refresh();

                new Msgwin(false, "حساب مورد نظر مسدود است!").ShowDialog();
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


        private bool HeaderIsValid(bool _ShowMessage_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //if (string.IsNullOrEmpty(PERSONEL.SelectedValue?.ToStringNullSafe()))
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "مجری نمیتواند خالی باشد" });
            //}
            if (string.IsNullOrEmpty(COMP_COD.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تماس گیرنده نمیتواند خالی باشد" });
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
        private async void Command33_Click(object sender, RoutedEventArgs e)
        {
            Command33.IsEnabled = false;

            if (!HeaderIsValid())
            {
                Command33.IsEnabled = true;
                return;
            }

            var CompcodToSend = COMP_COD.SelectionBoxItem as CUST_HESAB;
            var customerDetails = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT TOP 1 NAME,MOBILE FROM CUST_HESAB WHERE hes = '{COMP_COD.SelectedValue}'").FirstOrDefault();
            if (customerDetails != null)
            {
                if (customerDetails?.MOBILE == null)
                {
                    new Msgwin(false, $"موبایل جهت ارسال پیامک در سرفصل حساب های کل برای این شخص {CompcodToSend?.NAME} تعریف نشده !").Show();
                    return;
                }
                else if (customerDetails?.MOBILE.Length < 10)
                {
                    new Msgwin(false, $"موبایل تعریف شده \"{CompcodToSend?.MOBILE}\" برای این شخص {CompcodToSend?.NAME} صحیح نیست !").Show();
                    return;
                }

            }

            var parameters = new
            {
                PERSONEL = PERSONEL.SelectedValue,
                COMP_COD = COMP_COD.SelectedValue,
                PAYAM = $"SMS:{PAYAM.Text}",
                STATUS = 2 /*STATUS.SelectedValue*/,
                STDATE = Tarikh.FullCurrentDate,
                STTIME = DateTime.Now.ToString("HHmm"),
                USERNAME = USERNAME.Text
            };

            int? newIdnum = null;
            if (string.IsNullOrEmpty(IDNUM.Text))
            {
                newIdnum = dbms.DoGetDataSQL<int>(@"
                        INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME)
                        OUTPUT INSERTED.IDNUM
                        VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME)", parameters).FirstOrDefault();
            }
            else
            {
                dbms.DoExecuteSQL(@"
                        UPDATE dbo.MESAGEP 
                        SET PERSONEL = @PERSONEL, COMP_COD = @COMP_COD, PAYAM = @PAYAM, 
                            STATUS = @STATUS, STDATE = @STDATE, STTIME = @STTIME, 
                            USERNAME = @USERNAME
                        WHERE IDNUM = @IDNUM",
                    new { parameters, IDNUM = int.Parse(IDNUM.Text) });
            }

            if (newIdnum != null)
            {
                IDNUM.Text = newIdnum.ToString();
            }

            // Send SMS
            try
            {
                var SMSAC = new CL_SMSAC();
                var RESULT = await SMSAC.ErselSmsAsync(COMP_COD.SelectedValue.ToString(), $"{PAYAM.Text} - {USERNAME.Text}", Convert.ToInt64(IDNUM.Text), 1, false);

                List<MSGMODEL.SmsResultRecord>? resultRecords = null;
                if (RESULT != null)
                {
                    resultRecords = SmsResultProcessor.ConvertToRecords(RESULT);
                }

                if (RESULT != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                {
                    new Msgwin(false, "پيام ارسال شد....!").ShowDialog();
                }
                else
                {
                    if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                    {
                        new Msgwin(false, resultRecords?.FirstOrDefault()?.ErrorMessage).ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "پیام به خاطر خطا ارسال نشد!").ShowDialog();
                    }
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ارسال پیام , پیام ارسال نشد!").ShowDialog();
            }


            Command33.IsEnabled = true;

            this.Close();
        }

    }
}