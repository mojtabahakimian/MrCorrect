using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.Generaly.CL_Generaly;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{
    public partial class REMINDERPANEL : Window
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

        public REMINDERPANEL(long? _personel_ = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (_personel_ != null)
            {
                PERSONEL_PARAM = (long)_personel_;
            }

            ThreadSafeProperties.IsStillWorking = false;
        }
        public long? PERSONEL_PARAM { get; set; } = null;

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


            USERNAME.Text = (string)CL_HESABDARI.UCurrentUser();
            STDATE.Text = Tarikh.FullCurrentDate;
            STTIME.Text = DateTime.Now.ToString("HHmmss");

            #region FORM_LOAD

            COMP_COD.Focus();

            if (PERSONEL_PARAM != null)
            {
                PERSONEL.SelectedValue = PERSONEL_PARAM; PERSONEL.Items.Refresh();
                COMP_COD.Focus();
            }
            else
            {
                //PERSONEL.SelectedValue = Baseknow.USERCOD; PERSONEL.Items.Refresh();
            }

            Application.Current.Windows.OfType<INBOXPAN>().FirstOrDefault()?.Close();

            #endregion

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (e.Key is Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (GRID_MOREUSER.Visibility == Visibility.Visible)
                {
                    GRID_MOREUSER.Visibility = Visibility.Hidden;
                }
            }

            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
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
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "در جریان" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "تمام شده" });
            STATUS.ItemsSource = STATUS_COMBO_DATA;

            MAIN_PERSONS = dbms.DoGetDataSQL<S_USER_SALADTL>($"SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL = 0) AND (IDD <> 1)").ToList();
            for (int i = 0; i < MAIN_PERSONS.Count; i++)
                MAIN_PERSONS[i].SAL_NAME = CL_HESABDARI.DECODEUN(MAIN_PERSONS[i].SAL_NAME.ToString()).Replace("ي", "ی").Replace("ك", "ک");

            foreach (var item in MAIN_PERSONS)
            {
                Users.Add(item);
            }

        }
        private void COMP_COD_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (COMP_COD.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            var CUTSNO_TEX = (TextBox)COMP_COD.Template.FindName("PART_EditableTextBox", COMP_COD);

            if (CUTSNO_TEX.Text.Trim() == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("WIN_MESSAGEPANEL", I_AM_MESSAGEPANEL);
                CMBSearch.ShowDialog();
            }
            //else if (WAS_COMP_COD != CUTSNO_TEX.Text) // متنش با متن قبلی مورد تایید فرق کرده و میخوایم با فقط تایپ شدن جستجو کنه
            else if (DoesTextedOnCOMP_COD is true)
            {
                CL_HESAB_SEARCH.Go_Search_Hesab(CUTSNO_TEX.Text, "WIN_MESSAGEPANEL", I_AM_MESSAGEPANEL);
            }

            if (COMP_COD.SelectedValue is null)
            {
                universControl.PopNotifyShow("تماس گیرنده نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                WAS_COMP_COD = COMP_COD.Text; //اگر انتخاب جدید درست بوده بیا مقداری که باهاش مقدار مورد تایید قبلی رو برای جستجو مقایسه میکنی رو بروز کن
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

        #region MORE_USERS
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
        #endregion

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
                ErrosMessages.Add(new MsgModel { MessageText_U = "متن یادآوری نمیتواد خالی باشد" });
            }


            if (!CL_LMethods.IsValidTime24(STTIME.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "زمان یادآوری صحیح وارد نشده" });
            }

            string date_n_val = STDATE.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
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

        private void STDATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = STDATE.Text.ToRawTarikh();
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
        }
        private void STTIME_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!CL_LMethods.IsValidTime24(STTIME.Text))
            {
                universControl.PopNotifyShow(".زمان یادآوری صحیح وارد نشده", Pop1, Pop1Text1, Pop_Border1);
            }
        }

        private void BTN_SAVEREMIND_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            var parameters = new
            {
                PERSONEL = Convert.ToInt32(PERSONEL.SelectedValue),
                PAYAM = PAYAM.Text,
                STATUS = Convert.ToInt32(STATUS.SelectedValue),
                CTDATE = Convert.ToInt32(Tarikh.FullCurrentDate),
                CTTIME = DateTime.Now,
                USERNAME = USERNAME.Text,
                COMP_COD = COMP_COD.SelectedValue.ToString(),
                STDATE = Convert.ToInt32(STDATE.Text.ToRawTarikh()),
                STTIME = DateTime.Now,
                SMSOK = SMSOK.IsChecked ?? false,
                CRT = DateTime.Now
            };


            if (string.IsNullOrEmpty(IDNUM.Text))
            {
                var _idnum_ = dbms.DoGetDataSQL<int?>(@"INSERT INTO dbo.REMAINDER (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, SMSOK, USERNAME,CTDATE, CTTIME)
                            OUTPUT INSERTED.IDNUM
                            VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @SMSOK, @USERNAME,@CTDATE, @CTTIME)", parameters).FirstOrDefault();

                IDNUM.Text = _idnum_.ToString();
            }
            else
            {
                dbms.DoExecuteSQL(@"UPDATE dbo.REMAINDER 
                            SET PERSONEL = @PERSONEL, COMP_COD = @COMP_COD, PAYAM = @PAYAM, 
                                STATUS = @STATUS, STDATE = @STDATE, STTIME = @STTIME, 
                                SMSOK = @SMSOK, 
                                CTDATE = @CTDATE, 
                                CTTIME = @CTTIME,
                                SMSOK = @SMSOK, 
                                USERNAME = @USERNAME
                            WHERE IDNUM = @IDNUM",
                                    new { parameters, IDNUM = int.Parse(IDNUM.Text) });
            }

            if (GotUsers != null)
            {
                foreach (var row in GotUsers)
                {
                    if (row.IDD != Convert.ToInt32(PERSONEL.SelectedValue))
                    {

                        var additionalParameters = new
                        {
                            PERSONEL = row.IDD,
                            PAYAM = parameters.PAYAM,
                            STATUS = parameters.STATUS,
                            CTDATE = parameters.CTDATE,
                            CTTIME = parameters.CTTIME,
                            USERNAME = parameters.USERNAME,
                            COMP_COD = parameters.COMP_COD,
                            STDATE = parameters.STDATE,
                            STTIME = parameters.STTIME,
                            SMSOK = parameters.SMSOK,
                            CRT = DateTime.Now
                        };

                        dbms.DoExecuteSQL(@"INSERT INTO dbo.REMAINDER (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, CTDATE, CTTIME, SMSOK, USERNAME)
                            VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @CTDATE, @CTTIME, @SMSOK, @USERNAME)", additionalParameters);
                    }
                }
            }

            new Msgwin(false, "یادآوری ثبت شد.").ShowDialog();
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ThreadSafeProperties.IsStillWorking = false;
        }
    }
}
