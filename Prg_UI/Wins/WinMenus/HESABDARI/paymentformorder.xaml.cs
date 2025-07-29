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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.HESABDARI
{
    public partial class paymentformorder : Window
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

        public paymentformorder(double? number_to_open = null, bool is_read_only_mode = false)
        {
            InitializeComponent();

            this.DataContext = this;

            IS_READ_ONLY_MODE = is_read_only_mode;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool NO_IsFocused { get; private set; }

        private bool _NewRecord_;

        public bool NewRecord
        {
            get
            {
                if (string.IsNullOrEmpty(IDD.Text) || IDD.Text == "0")
                {
                    _NewRecord_ = true;
                }
                else
                {
                    _NewRecord_ = false;
                }
                return _NewRecord_;
            }
            set { _NewRecord_ = value; }
        }
        public long? CURRENT_ROW_NO_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        List<COMBOPERSONEL> rst_personel = null;

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
        public Visual I_AM_PAYORRDER { get; private set; }
        public double Meidnum { get; private set; }
        public bool IS_READ_ONLY_MODE { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_PAYORRDER = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            if (IS_READ_ONLY_MODE)
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
                PERSONEL.IsEnabled = false;
                BTN_SAVE.IsEnabled = false;
            }
            else
            {
                if (string.IsNullOrEmpty(IDD.Text) || IDD.Text == "0")
                {
                    SGN1.IsEnabled = false;
                    SGN2.IsEnabled = false;
                    SGN3.IsEnabled = false;
                    PERSONEL.IsEnabled = false;
                }
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F8 || e.SystemKey == Key.F8))
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
            //}

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
            //var RST_HESABS = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
            var RST_HESABS = new List<Custom_CUST_HESAB>();
            CUST_NO.ItemsSource = RST_HESABS;
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;

            ORDERER.ItemsSource = RST_HESABS;
            ORDERERID.ItemsSource = ORDERER.ItemsSource;

            //کبموباکس مجری
            string sql = @"
               SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD
               FROM SALA_DTL sd
               LEFT JOIN USER_PERSONEL_ORDER uo 
                    ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
               WHERE sd.ENABL = 0
               ORDER BY
                    CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                    uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";

            //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.SelectedValue = Baseknow.USERCOD;
            //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

        }
        public void Form_Current()
        {
        }
        public void ReGetMasterData()
        {
            PAYDATE.Focus();
            PAYDATE.Text = Tarikh.FullCurrentDate;
            PAYDATE.SelectAll();

            USERNAME.Text = Baseknow.UUSER;

            #region Form_Load

            if (true)
            {
                var _HES_ = CL_HESABDARI.GETUSERCO((int)Baseknow.USERCOD);
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + _HES_ + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;
                    if (!((List<Custom_CUST_HESAB>)ORDERER.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)ORDERER.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    ORDERER.SelectedValue = null;
                    ORDERER.SelectedValue = thevalue;
                    ORDERER.Items.Refresh();
                }
            }

            if (!IsNull(NUMBER_TO_OPEN))
            {
                var RST = dbms.DoGetDataSQL<PAYORDER>("SELECT * FROM PAYORDER WHERE IDD = " + NUMBER_TO_OPEN).FirstOrDefault();
                if (RST != null)
                {
                    this.IDD.Text = RST.IDD.ToString();
                    this.MABLS.Text = RST.MABLS.ToString();
                    this.BABAT.Text = RST.BABAT;
                    if (RST.CUST_NO != null)
                    {
                        var _HES_ = RST.CUST_NO;
                        var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + _HES_ + "'").FirstOrDefault();
                        if (data is not null && !string.IsNullOrEmpty(data.hes))
                        {
                            string thevalue = data.hes;
                            if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                            }
                            CUST_NO.SelectedValue = null;
                            CUST_NO.SelectedValue = thevalue;
                            CUST_NO.Items.Refresh();
                        }
                    }
                    if (RST.ORDERER != null)
                    {
                        var _HES_ = RST.ORDERER;
                        var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + _HES_ + "'").FirstOrDefault();
                        if (data is not null && !string.IsNullOrEmpty(data.hes))
                        {
                            string thevalue = data.hes;
                            if (!((List<Custom_CUST_HESAB>)ORDERER.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)ORDERER.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                            }
                            ORDERER.SelectedValue = null;
                            ORDERER.SelectedValue = thevalue;
                            ORDERER.Items.Refresh();
                        }
                    }

                    this.CUST_NO_TXT.Text = RST.CUST_NO_TXT;
                    this.PAYDATE.Text = RST.PAYDATE.ToString();
                    this.TOZIH.Text = RST.TOZIH;
                    this.CACHTIC.IsChecked = Convert.ToBoolean(RST.CACHTIC);
                    this.CHACHMABL.Text = RST.CHACHMABL.ToString();
                    this.CHEKTIC.IsChecked = Convert.ToBoolean(RST.CHEKTIC);
                    this.CHEKMABL.Text = RST.CHEKMABL.ToString();
                    this.CHEKFAGH.Text = RST.CHEKFAGH.ToString();
                    this.CHEKDIST.Text = RST.CHEKDIST.ToString();
                    this.CHEKMTIC.IsChecked = Convert.ToBoolean(RST.CHEKMTIC);
                    this.CHEKMMABL.Text = RST.CHEKMMABL.ToString();
                    this.CHEKMFAGH.Text = RST.CHEKMFAGH.ToString();
                    this.CHEKMDIST.Text = RST.CHEKMDIST.ToString();
                    this.USERNAME.Text = RST.USERNAME;


                    SGN1.IsChecked = Convert.ToBoolean(RST.SGN1);
                    SGN2.IsChecked = Convert.ToBoolean(RST.SGN2);
                    SGN3.IsChecked = Convert.ToBoolean(RST.SGN3);

                    SGN1usid.Tag = Convert.ToInt32(RST.sgn1usid);
                    SGN2usid.Tag = Convert.ToInt32(RST.sgn2usid);
                    SGN3usid.Tag = Convert.ToInt32(RST.sgn3usid);

                    SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == RST?.sgn1usid)?.SAL_NAME;
                    SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == RST?.sgn2usid)?.SAL_NAME;
                    SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == RST?.sgn3usid)?.SAL_NAME;
                }
                else
                {
                    new Msgwin(false, "چنین درخواست پرداختی وجود ندارد !").ShowDialog();
                    this.Close();
                    return;
                }
            }
            //if ((bool)Baseknow.SIGN)
            //{
            //    this.SGN1.Visibility = true;
            //    this.SGN2.Visibility = true;
            //    this.SGN3.Visibility = true;
            //    this.sgn1usid.Visibility = true;
            //    this.sgn2usid.Visibility = true;
            //    this.sgn3usid.Visibility = true;
            //    this.PERSONEL.Visibility = true;
            //}
            CL_HESABDARI.LetSigneTick(this.GetType().Name, 100, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            #endregion
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!Tarikh.IsValidedDate(PAYDATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
            }

            if (string.IsNullOrEmpty(BABAT.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بابت نمي تواند خالي باشد" });
            }

            if (CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب دريافت کننده  نمي تواند خالي باشد" });
            }
            if (ORDERER.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام درخواست کننده  نمي تواند خالي باشد" });
            }

            if (Convert.ToBoolean(this.CACHTIC.IsChecked) && CHACHMABL.Text == "0")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "پرداخت نقدي تيک خورده ولي مبلغ آن مشخص نشده است" });
            }

            if (Convert.ToBoolean(this.CHEKTIC.IsChecked) && CHEKMABL.Text == "0")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "پرداخت چک شرکت تيک خورده ولي مبلغ آن مشخص نشده است" });
            }

            if (Convert.ToBoolean(this.CHEKMTIC.IsChecked) && CHEKMMABL.Text == "0")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "پرداخت چک مشتري تيک خورده ولي مبلغ آن مشخص نشده است" });
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
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (!SAVEPAY()) //Is Success?
            {
                return;
            }

            SGN1.IsEnabled = true;
            SGN2.IsEnabled = true;
            SGN3.IsEnabled = true;
            PERSONEL.IsEnabled = true;

            NewRecord = false;
            ChangeIsHappend = false;

            if (sender != null)
            {
                universControl.PopNotifyShow($"درخواست پرداخت با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }
        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO is not ComboBox comboBox) return;
            if (comboBox.IsEditable && e.OriginalSource is not TextBox) return;

            TextBox editableTextBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (editableTextBox == null) return; // Safety check

            if (CUST_NO.SelectedValue != null && (CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == editableTextBox.Text)
            {
                return;
            }

            CL_LMethods.GetSearchedValueCustomer(comboBox, "paymentformorder_CUST_NO", default, dbms, I_AM_PAYORRDER);


        }

        private void ORDERER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ORDERER is not ComboBox comboBox) return;
            if (comboBox.IsEditable && e.OriginalSource is not TextBox) return;

            TextBox editableTextBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (editableTextBox == null) return; // Safety check

            if (ORDERER.SelectedValue != null && (ORDERER.SelectedItem as Custom_CUST_HESAB)?.NAME == editableTextBox.Text)
            {
                return;
            }

            CL_LMethods.GetSearchedValueCustomer(comboBox, "paymentformorder_ORDERER", default, dbms, I_AM_PAYORRDER);
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                SGN1.IsChecked = !SGN1.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;

            BTN_SAVE_Click(null, null);

            //if (!SAVEPAY())
            //{
            //    return;
            //}
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
            SHARH = "'درخواست پرداخت شماره: " + IDD.Text + " مورخ " + Strings.Format(Convert.ToInt64(Convert.ToInt64(PAYDATE.Text.ToRawTarikh())), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + " " + CUST_NO.SelectedValue + " بابت :" + this.BABAT.Text + " توسط " + CL_HESABDARI.GETTAFNAME(this.ORDERER.SelectedValue.ToString()) + " به مبلغ :" + this.MABLS.Text + "','" + this.CUST_NO.SelectedValue + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {

                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100,GETDATE()," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            dbms.DoExecuteSQL($"UPDATE dbo.payorder SET SGN1usid={SGN1usid.Tag ?? "NULL"}, SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} WHERE IDD = {IDD.Text}");

            if (Convert.ToBoolean(this.SGN1.IsChecked) || Convert.ToBoolean(this.SGN2.IsChecked) || Convert.ToBoolean(this.SGN3.IsChecked))
            {
                this.Command81.IsEnabled = true;
                this.Command77.IsEnabled = true;
            }
            else
            {
                this.Command81.IsEnabled = false;
                this.Command77.IsEnabled = false;
            }
            if (!SAVEPAY())
            {
                return;
            }

        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                SGN2.IsChecked = !SGN2.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;

            BTN_SAVE_Click(null, null);
            //if (!SAVEPAY())
            //{
            //    return;
            //}
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
            SHARH = "'درخواست پرداخت شماره: " + this.IDD.Text + " مورخ " + Strings.Format(Convert.ToInt64(Convert.ToInt64(PAYDATE.Text.ToRawTarikh())), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + " " + CUST_NO.SelectedValue + " بابت :" + this.BABAT.Text + " توسط " + CL_HESABDARI.GETTAFNAME(this.ORDERER.SelectedValue.ToString()) + " به مبلغ :" + this.MABLS.Text + "','" + this.CUST_NO.SelectedValue + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN2.IsChecked), " :امضا شد2 ", " :امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {

                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100,GETDATE()," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN2.IsChecked), " : امضا شد2 ", " :امضا برداشته شد2 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            dbms.DoExecuteSQL($"UPDATE dbo.payorder SET SGN2usid={SGN2usid.Tag ?? "NULL"}, SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} WHERE IDD = {IDD.Text}");

            if (Convert.ToBoolean(this.SGN1.IsChecked) || Convert.ToBoolean(this.SGN2.IsChecked) || Convert.ToBoolean(this.SGN3.IsChecked))
            {
                this.Command81.IsEnabled = true;
                this.Command77.IsEnabled = true;
            }
            else
            {
                this.Command81.IsEnabled = false;
                this.Command77.IsEnabled = false;
            }
            if (!SAVEPAY())
            {
                return;
            }
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                SGN3.IsChecked = !SGN3.IsChecked;
                e.Handled = true;
                return;
            }

            double MID;
            string SHARH;

            BTN_SAVE_Click(null, null);

            //if (!SAVEPAY())
            //{
            //    return;
            //}
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
            SHARH = "'درخواست پرداخت شماره: " + this.IDD.Text + " مورخ " + Strings.Format(Convert.ToInt64(Convert.ToInt64(PAYDATE.Text.ToRawTarikh())), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + " " + CUST_NO.SelectedValue + " بابت :" + this.BABAT.Text + " توسط " + CL_HESABDARI.GETTAFNAME(this.ORDERER.SelectedValue.ToString()) + " به مبلغ :" + this.MABLS.Text + "','" + this.CUST_NO.SelectedValue + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN3.IsChecked), " :امضا شد3 ", " :امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {

                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100,GETDATE()," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(IDD.Text), 100);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf(Convert.ToBoolean(SGN3.IsChecked), " : امضا شد3 ", " :امضا برداشته شد3 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",100," + this.IDD.Text + ",100 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;

            SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            dbms.DoExecuteSQL($"UPDATE dbo.payorder SET SGN3usid={SGN3usid.Tag ?? "NULL"}, SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} WHERE IDD = {IDD.Text}");

            if (Convert.ToBoolean(this.SGN1.IsChecked) || Convert.ToBoolean(this.SGN2.IsChecked) || Convert.ToBoolean(this.SGN3.IsChecked))
            {
                this.Command81.IsEnabled = true;
                this.Command77.IsEnabled = true;
            }
            else
            {
                this.Command81.IsEnabled = false;
                this.Command77.IsEnabled = false;
            }
            if (!SAVEPAY())
            {
                return;
            }
        }
        private bool IsNull(object? hTAF2)
        {
            string _inputy = hTAF2.ToStringNullSafe();
            if (string.IsNullOrEmpty(_inputy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool SAVEPAY()
        {
            if (!HeaderIsValid())
            {
                return false;
            }

            try
            {
                //var payOrder = new PAYORDER
                //{
                //    MABLS = float.Parse(this.MABLS.Text),
                //    BABAT = string.IsNullOrEmpty(this.BABAT.Text) ? "" : this.BABAT.Text,
                //    CUST_NO = string.IsNullOrEmpty(this.CUST_NO.SelectedValue.ToStringNullSafe()) ? "" : this.CUST_NO.SelectedValue.ToStringNullSafe(),
                //    CUST_NO_TXT = string.IsNullOrEmpty(this.CUST_NO_TXT.Text) ? "" : this.CUST_NO_TXT.Text,
                //    ORDERER = string.IsNullOrEmpty(this.ORDERER.SelectedValue.ToStringNullSafe()) ? "" : this.ORDERER.SelectedValue.ToStringNullSafe(),
                //    PAYDATE = long.Parse(this.PAYDATE.Text.ToRawTarikh()), // Assuming ToRawTarikh() converts to long
                //    TOZIH = string.IsNullOrEmpty(this.TOZIH.Text) ? "" : this.TOZIH.Text,
                //    CACHTIC = Convert.ToInt32(this.CACHTIC.IsChecked),
                //    CHACHMABL = float.Parse(this.CHACHMABL.Text),
                //    CHEKTIC = Convert.ToInt32(this.CHEKTIC.IsChecked),
                //    CHEKMABL = float.Parse(this.CHEKMABL.Text),
                //    CHEKFAGH = int.Parse(this.CHEKFAGH.Text),
                //    CHEKDIST = int.Parse(this.CHEKDIST.Text),
                //    CHEKMTIC = Convert.ToInt32(this.CHEKMTIC.IsChecked),
                //    CHEKMMABL = float.Parse(this.CHEKMMABL.Text),
                //    CHEKMFAGH = int.Parse(this.CHEKMFAGH.Text),
                //    CHEKMDIST = int.Parse(this.CHEKMDIST.Text),
                //    USERNAME = this.USERNAME.Text,
                //    USERCO = int.Parse(Baseknow.USERCOD.ToString()),
                //    SGN1 = Convert.ToInt32(SGN1.IsChecked),
                //    SGN2 = Convert.ToInt32(SGN2.IsChecked),
                //    SGN3 = Convert.ToInt32(SGN3.IsChecked),
                //    sgn1usid = string.IsNullOrEmpty(this.SGN1usid.Text) ? (int?)null : int.Parse(this.SGN1usid.Text),
                //    sgn2usid = string.IsNullOrEmpty(this.SGN2usid.Text) ? (int?)null : int.Parse(this.SGN2usid.Text),
                //    sgn3usid = string.IsNullOrEmpty(this.SGN3usid.Text) ? (int?)null : int.Parse(this.SGN3usid.Text),
                //};

                var payOrder = new PAYORDER();

                if (double.TryParse(this.MABLS.Text, out double mabls))
                    payOrder.MABLS = mabls;

                payOrder.BABAT = this.BABAT.Text ?? "";
                payOrder.CUST_NO = this.CUST_NO.SelectedValue?.ToStringNullSafe() ?? "";
                payOrder.CUST_NO_TXT = this.CUST_NO_TXT.Text ?? "";
                payOrder.ORDERER = this.ORDERER.SelectedValue?.ToStringNullSafe() ?? "";

                if (long.TryParse(this.PAYDATE.Text.ToRawTarikh(), out long paydate))
                    payOrder.PAYDATE = paydate;

                payOrder.TOZIH = this.TOZIH.Text ?? "";
                payOrder.CACHTIC = this.CACHTIC.IsChecked == true ? 1 : 0;

                if (double.TryParse(this.CHACHMABL.Text, out double chachmabl))
                    payOrder.CHACHMABL = chachmabl;

                payOrder.CHEKTIC = this.CHEKTIC.IsChecked == true ? 1 : 0;

                if (double.TryParse(this.CHEKMABL.Text, out double chekmabl))
                    payOrder.CHEKMABL = chekmabl;

                if (int.TryParse(this.CHEKFAGH.Text, out int chekfagh))
                    payOrder.CHEKFAGH = chekfagh;

                if (int.TryParse(this.CHEKDIST.Text, out int chekdist))
                    payOrder.CHEKDIST = chekdist;

                payOrder.CHEKMTIC = this.CHEKMTIC.IsChecked == true ? 1 : 0;

                if (double.TryParse(this.CHEKMMABL.Text, out double chekmmabl))
                    payOrder.CHEKMMABL = chekmmabl;

                if (int.TryParse(this.CHEKMFAGH.Text, out int chekmfagh))
                    payOrder.CHEKMFAGH = chekmfagh;

                if (int.TryParse(this.CHEKMDIST.Text, out int chekmdist))
                    payOrder.CHEKMDIST = chekmdist;



                if (int.TryParse(Baseknow.USERCOD.ToString(), out int userco))
                    payOrder.USERCO = userco;

                payOrder.SGN1 = this.SGN1.IsChecked == true ? 1 : 0;
                payOrder.SGN2 = this.SGN2.IsChecked == true ? 1 : 0;
                payOrder.SGN3 = this.SGN3.IsChecked == true ? 1 : 0;

                if (SGN1usid.Tag != null)
                {
                    payOrder.sgn1usid = Convert.ToInt32(SGN1usid.Tag);
                }
                if (SGN2usid.Tag != null)
                {
                    payOrder.sgn2usid = Convert.ToInt32(SGN2usid.Tag);
                }
                if (SGN3usid.Tag != null)
                {
                    payOrder.sgn3usid = Convert.ToInt32(SGN3usid.Tag);
                }

                if (string.IsNullOrEmpty(this.IDD.Text) || this.IDD.Text == "0")
                {
                    payOrder.USERNAME = (string)CL_HESABDARI.UCurrentUser();

                    // Insert new record
                    string insertSql = @"
                    INSERT INTO [dbo].[payorder]
                    (MABLS, BABAT, CUST_NO, CUST_NO_TXT, ORDERER, PAYDATE, TOZIH, CACHTIC, CHACHMABL, CHEKTIC, CHEKMABL, CHEKFAGH, CHEKDIST, CHEKMTIC, CHEKMMABL, CHEKMFAGH, CHEKMDIST, USERNAME, USERCO, SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, UID)
                    VALUES
                    (@MABLS, @BABAT, @CUST_NO, @CUST_NO_TXT, @ORDERER, @PAYDATE, @TOZIH, @CACHTIC, @CHACHMABL, @CHEKTIC, @CHEKMABL, @CHEKFAGH, @CHEKDIST, @CHEKMTIC, @CHEKMMABL, @CHEKMFAGH, @CHEKMDIST, @USERNAME, @USERCO, @SGN1, @SGN2, @SGN3, @sgn1usid, @sgn2usid, @sgn3usid, @UID);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                    var idd = dbms.DoGetDataSQL<int>(insertSql, payOrder).FirstOrDefault();
                    this.IDD.Text = idd.ToString();
                }
                else
                {
                    payOrder.USERNAME = USERNAME.Text;

                    // Update existing record
                    payOrder.IDD = int.Parse(this.IDD.Text);
                    string updateSql = @"
                    UPDATE [dbo].[payorder]
                    SET MABLS = @MABLS,
                        BABAT = @BABAT,
                        CUST_NO = @CUST_NO,
                        CUST_NO_TXT = @CUST_NO_TXT,
                        ORDERER = @ORDERER,
                        PAYDATE = @PAYDATE,
                        TOZIH = @TOZIH,
                        CACHTIC = @CACHTIC,
                        CHACHMABL = @CHACHMABL,
                        CHEKTIC = @CHEKTIC,
                        CHEKMABL = @CHEKMABL,
                        CHEKFAGH = @CHEKFAGH,
                        CHEKDIST = @CHEKDIST,
                        CHEKMTIC = @CHEKMTIC,
                        CHEKMMABL = @CHEKMMABL,
                        CHEKMFAGH = @CHEKMFAGH,
                        CHEKMDIST = @CHEKMDIST,
                        USERNAME = @USERNAME,
                        USERCO = @USERCO,
                        SGN1 = @SGN1,
                        SGN2 = @SGN2,
                        SGN3 = @SGN3,
                        sgn1usid = @sgn1usid,
                        sgn2usid = @sgn2usid,
                        sgn3usid = @sgn3usid,
                        UID = @UID
                    WHERE IDD = @IDD";
                    dbms.DoExecuteSQL(updateSql, payOrder);
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در ثبت درخواست پرداخت , اطلاعات ذخیره نشد!").ShowDialog();
                return false;
            }

            NewRecord = false;

            return true;
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PERSONEL.SelectedItem == null || PERSONEL.SelectedValue == null)
            {
                e.Handled = true;
                return;
            }

            if (NewRecord)
            {
                BTN_SAVE_Click(null, null);
            }

            string SelectedTextCMB = ((COMBOPERSONEL)PERSONEL.SelectedItem).SAL_NAME.ToStringNullSafe();

            Meidnum = CL_HESABDARI.PERSONELUpdate(100, Convert.ToDouble(IDD.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'درخواست پرداخت شماره: " + IDD.Text + " مورخ " + Strings.Format(Convert.ToInt64(PAYDATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + " " + CUST_NO.SelectedValue + " بابت :" + BABAT.Text + " توسط " + CL_HESABDARI.GETTAFNAME(ORDERER.SelectedValue.ToString()) + " به مبلغ :" + MABLS.Text + "','" + CUST_NO.SelectedValue + "'");
            universControl.PopNotifyShow($"ارجاع داده به {SelectedTextCMB} شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void Command77_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord) { return; }

        }
        private void Command81_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord) { return; }

        }

        private void BTN_NEW_PAYORDER_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از ایجاد درخواست پرداخت جدید اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }
            }

            IS_READ_ONLY_MODE = false;
            NUMBER_TO_OPEN = null;

            IDD.Text = null; //شماره
            PAYDATE.Text = null; //تاریخ
            MABLS.Text = "0"; //مبلغ
            BABAT.Text = null; //بابت
            CUST_NO.SelectedValue = null; //حساب دریافت کننده
            CUST_NO2.SelectedValue = null; //حساب دریافت کننده
            CUST_NO_TXT.Text = null; //دریافت کننده(نماینده)
            TOZIH.Text = null; //توضیحات
            ORDERER.SelectedValue = null; ORDERERID.SelectedValue = null; //درخواست کننده

            CACHTIC.IsChecked = false; //نقد
            CHEKTIC.IsChecked = false; //چک شرکت
            CHEKMTIC.IsChecked = false; //چک مشتری
            CHACHMABL.Text = "0"; //مبلغ نقد
            CHEKMABL.Text = "0"; //مبلغ چک شرکت
            CHEKFAGH.Text = "0"; // تعداد فقره چک شرکت
            CHEKDIST.Text = "0"; // تعداد فقره چک شرکت با فاصله روز
            CHEKMMABL.Text = "0"; //مبلغ چک مشتری
            CHEKMFAGH.Text = "0"; //تعداد فقره چک مشتری
            CHEKMDIST.Text = "0"; //تعداد فقره چک مشتری با فاصله روز

            SGN1.IsChecked = false; //مدیر واحد
            SGN2.IsChecked = false;
            SGN3.IsChecked = false;
            SGN1usid.Text = null; SGN1usid.Tag = null;
            SGN2usid.Text = null; SGN2usid.Tag = null;
            SGN3usid.Text = null; SGN3usid.Tag = null;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            ReGetMasterData();
        }

        private void BTN_INFO_RECIVER_Click(object sender, RoutedEventArgs e)
        {
            if (CUST_NO.SelectedValue != null)
            {
                //string custNo = CUST_NO.SelectedValue.ToString();
                //var customer = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT * FROM CUST_HESAB WHERE HES = '{custNo}'").FirstOrDefault();
                //if (customer != null)
                //{
                //    string info = "";
                //    if (!string.IsNullOrEmpty(customer.hes)) info += $"حساب: {customer.hes}\n";
                //    if (!string.IsNullOrEmpty(customer.NAME)) info += $"نام حساب: {customer.NAME}\n";
                //    if (!string.IsNullOrEmpty(customer.MCODEM)) info += $"کدملی: {customer.MCODEM}\n";
                //    if (!string.IsNullOrEmpty(customer.ADDRESS)) info += $"آدرس: {customer.ADDRESS}\n";
                //    if (!string.IsNullOrEmpty(customer.TEL)) info += $"تلفن: {customer.TEL}\n";
                //    if (!string.IsNullOrEmpty(customer.ECODE)) info += $"کد اقتصادی: {customer.ECODE}\n";
                //    if (!string.IsNullOrEmpty(customer.PCODE)) info += $"کد پستی: {customer.PCODE}\n";
                //    if (!string.IsNullOrEmpty(customer.IYALAT)) info += $"استان: {customer.IYALAT}\n";
                //    if (!string.IsNullOrEmpty(customer.CITY)) info += $"شهر: {customer.CITY}\n";
                //    if (!string.IsNullOrEmpty(customer.TOZIH)) info += $"توضیح: {customer.TOZIH}\n";
                //    if (!string.IsNullOrEmpty(customer.MOBILE)) info += $"موبایل: {customer.MOBILE}\n";
                //    if (customer.OSTANID != 0) info += $"کد استان: {customer.OSTANID}\n";
                //    if (customer.SHAHRID != 0) info += $"کد شهر: {customer.SHAHRID}\n";

                //    new Msgwin(false, info).ShowDialog();
                //}

                string hesValue = CUST_NO.SelectedValue.ToString();
                var info = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT * FROM dbo.CUST_HESAB WHERE hes = N'{hesValue}'").FirstOrDefault();
                if (info != null)
                {
                    var sb = new System.Text.StringBuilder();

                    void Add(string label, string? value)
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                            sb.AppendLine($"{label} : {value}");
                    }
                    void AddNum(string label, int? value)
                    {
                        if (value.HasValue)
                            sb.AppendLine($"{label}: {value}");
                    }
                    void AddNumD(string label, double? value)
                    {
                        if (value.HasValue)
                            sb.AppendLine($"{label}: {value}");
                    }

                    Add("نام:\n", info?.NAME);
                    Add("کد حساب", info?.hes);
                    Add("کد ملی", info?.MCODEM);
                    Add("آدرس", info?.ADDRESS);
                    Add("تلفن", info?.TEL);
                    Add("موبایل", info?.MOBILE);
                    Add("کد اقتصادی", info?.ECODE);
                    Add("کد پستی", info?.PCODE);
                    Add("استان", info?.IYALAT);
                    Add("شهر", info?.CITY);
                    Add("توضیح", info?.TOZIH);
                    Add("مسیر", info?.ROUTE_NAME);
                    AddNum("کد استان", info?.OSTANID);
                    AddNum("کد شهر", info?.SHAHRID);

                    string message = sb.ToString();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = "اطلاعاتی برای نمایش وجود ندارد.";
                    }

                    new Msgwin(false, message, "", true).ShowDialog();
                }
                else
                {
                    universControl.PopNotifyShowUp($"اطلاعاتی برای این حساب یافت نشد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
            }

        }
    }
}
