using Functions;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using static Interfaces.INavigator;
using static Prg_UI.HelperWins.Msgwin;

namespace Wins.WinMenus.Taarif
{
    public partial class KHAD_DEF : Window
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
        public KHAD_DEF()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public CollectionViewSource RecordsKhad { get; set; } = new CollectionViewSource();
        public ObservableCollection<TAKHPERS> TAKHPERS_DATA { get; set; } = new ObservableCollection<TAKHPERS>();

        public TAKHPERS? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TAKHPERS? WAS_ROW_ITEM { get; private set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public int KINDK { get; set; } = 2;
        public bool NewRecord { get; set; } = false;
        public bool ChangeIsHappend { get; private set; } = false;
        public bool TAKHPERS_SUB_IsFocusedIn { get; private set; }

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (TAKHPERS_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = TAKHPERS_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CUST_CO")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _name_code_index = 0;
                    }
                    else
                    {
                        _name_code_index = (int)defaultcolumnindex;
                    }
                }
                return _name_code_index;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend is true)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "KHAD", new WindowInteropHelper(this).Handle, this.GetType().Name); //دسترسی
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            CL_HESABDARI.SETSECURITYSUB(TAKHPERS_SUB, "KALA"); //دسترسی دیتاگرید

            FILL_ALL_COMBOBOXES();

            ReGetData();


            NEWRECORD_BTN_Click(null, null);

            NAM.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //واحد
            VAHED.ItemsSource = dbms.DoGetDataSQL<TCOD_VAHEDS>("SELECT TCOD_VAHEDS.CODE, TCOD_VAHEDS.NAMES FROM TCOD_VAHEDS ORDER BY TCOD_VAHEDS.NAMES").ToList();

            //گروه
            RADAH.ItemsSource = dbms.DoGetDataSQL<TCOD_STUFGROUP>("SELECT TCOD_STUFGROUP.CODE, TCOD_STUFGROUP.NAMES FROM TCOD_STUFGROUP WHERE (((TCOD_STUFGROUP.CODE)=0)) ORDER BY TCOD_STUFGROUP.NAMES").ToList();

            //واحد ارائه کننده خدمات
            DEPART.ItemsSource = dbms.DoGetDataSQL<DEPART_MODEL>("SELECT DEPART.DEPATMAN, DEPART.DEPNAME FROM DEPART ORDER BY DEPART.DEPNAME").ToList();

            //کد نوع مشتری سطر دیتاگرید
            COLUMN_CUST_CO.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUSTKIND.CUST_COD, CUSTKIND.CUSTKNAME FROM CUSTKIND").ToList();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dg = TAKHPERS_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (uie is DataGridCell || (uie as FrameworkElement)?.Parent is DataGridCell)
                    {
                        if (TAKHPERS_SUB_IsFocusedIn)
                        {
                            ///////در این روش کلید تب عمل میکند و حالت فوکوس روی حالت در حال ادیت هست
                            if (TAKHPERS_SUB.SelectedIndex == TAKHPERS_SUB.Items.Count - 2 && TAKHPERS_SUB.CurrentColumn.DisplayIndex == CL_LMethods.GetLastColumn(TAKHPERS_SUB).DisplayIndex)
                            {
                                TAKHPERS_SUB.SelectedIndex = TAKHPERS_SUB.Items.Count - 1;
                                TAKHPERS_SUB.CurrentCell = new DataGridCellInfo(TAKHPERS_SUB.SelectedItem, TAKHPERS_SUB.Columns[NAME_CODE_INDEX_COL]);
                                TAKHPERS_SUB.BeginEdit();
                                return;
                            }
                        }
                    }
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            catch { /*ignore*/ }



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
        private void SAVE_KHAD_Click(object sender, RoutedEventArgs e)
        {
            if (!HeadIsValid())
            {
                return;
            }
            if (!BodyIsValid())
            {
                return;
            }

            double? THECODE = null;
            if (string.IsNullOrEmpty(CODE.Text.ToStringNullSafe()) || Convert.ToDouble(CODE.Text) <= 0)
            {
                THECODE = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(CODE + 1) FROM dbo.STUF_DEF").FirstOrDefault();
                if (THECODE is null || THECODE.ToStringNullSafe() == "NULL")
                {
                    THECODE = 1;
                }
            }
            else
            {
                THECODE = Convert.ToDouble(CODE.Text);
            }

            #region HEADER
            try
            {
                if (true /*STUF_DEF*/ )
                {
                    var rst = dbms.DoGetDataSQL<string?>($"SELECT CODE FROM dbo.STUF_DEF WHERE CODE = N'{CODE.Text}'").FirstOrDefault();
                    if (rst is null)
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.STUF_DEF(CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, CMBAA)
                                         VALUES(N'{THECODE}',
                                         N'{NAM.Text.Trim()}' ,
                                         N'{N_FANI.Text.Trim()}' ,
                                         N'{TOZIH.Text.Trim()}' ,
                                         {VAHED.SelectedValue} ,
                                         0.0 ,
                                         0.0 ,
                                         0.0 ,
                                         0.0 ,
                                         {RADAH.SelectedValue} ,
                                         {KINDK} ,
                                         {MABL_F.Text} ,
                                         {DEPART.SelectedValue} ,
                                         {Convert.ToByte(CMBAA.IsChecked)})");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_DEF
                                         SET CODE = N'{THECODE}', NAME = N'{NAM.Text.Trim()}', N_FANI = N'{N_FANI.Text.Trim()}', 
                                         TOZIH = N'{TOZIH.Text.Trim()}', VAHED = {VAHED.SelectedValue}, B_SEF = 0.0,
                                         N_SEF = 0.0, MIN_M = 0.0, MAX_M = 0.0, RADAH = {RADAH.SelectedValue}, KINDK = {KINDK},
                                         MABL_F = {MABL_F.Text}, DEPART = {DEPART.SelectedValue}, CMBAA = {Convert.ToByte(CMBAA.IsChecked)}
                                         WHERE CODE = N'{CODE.Tag.ToStringNullSafe()}'");
                    }
                }
                CODE.Text = THECODE.ToStringNullSafe();

                #region Form_After_Update
                if (true /*TDETA_HES*/ )
                {
                    var _N_KOL_ = Baseknow.DARAM;
                    var _NUMBER_ = DEPART.SelectedValue;
                    var _TNUMBER_ = CODE.Text;
                    var _NAME_ = NAM.Text;
                    var _BED_BES_ = -1;

                    var rst = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TOP 1 N_KOL, NUMBER,TNUMBER, NAME FROM dbo.TDETA_HES WHERE N_KOL={Baseknow.DARAM} AND NUMBER={DEPART.SelectedValue} AND TNUMBER = {CODE.Text}").FirstOrDefault();
                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_},
                                                 {_TNUMBER_},
                                                 N'{_NAME_}',
                                                 {_BED_BES_} )");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_},
                                                 TNUMBER = {_TNUMBER_}, NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                 WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER} AND TNUMBER = {(rst.TNUMBER)}");
                    }

                }
                if (true /*STUF_FSK*/ )
                {
                    double? CODEY = null;
                    //if (string.IsNullOrEmpty(CODE.Text.ToStringNullSafe()) || Convert.ToDouble(CODE.Text) <= 0)
                    //{
                    //    CODEY = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(CODE + 1) FROM dbo.STUF_FSK").FirstOrDefault();
                    //    if (CODEY is null || CODEY.ToStringNullSafe() == "NULL")
                    //    {
                    //        CODEY = 1;
                    //    }
                    //}
                    //else
                    {
                        CODEY = Convert.ToDouble(CODE.Text);
                    }

                    var rst = dbms.DoGetDataSQL<string?>("SELECT TOP 1 CODE FROM dbo.STUF_FSK WHERE CODE = '" + CODE.Text + "' AND ANBAR = 0 ").FirstOrDefault();
                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.STUF_FSK(CODE, ANBAR)
                                                 VALUES(N'{CODEY}',0)");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_FSK
                                                 SET CODE = N'{CODEY}', ANBAR = 0
                                                 WHERE CODE = N'{rst}' AND ANBAR = 0");
                    }
                }
                if (true /*STUF_STK*/ )
                {
                    double? CODEY = null;
                    //if (string.IsNullOrEmpty(CODE.Text.ToStringNullSafe()) || Convert.ToDouble(CODE.Text) <= 0)
                    //{
                    //    CODEY = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(CODE + 1) FROM dbo.STUF_STK").FirstOrDefault();
                    //    if (CODEY is null || CODEY.ToStringNullSafe() == "NULL")
                    //    {
                    //        CODEY = 1;
                    //    }
                    //}
                    //else
                    {
                        CODEY = Convert.ToDouble(CODE.Text);
                    }

                    var rst = dbms.DoGetDataSQL<string?>("SELECT TOP 1 CODE FROM dbo.STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = 0 ").FirstOrDefault();
                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.STUF_STK(CODE, ANBAR)
                                                 VALUES(N'{CODEY}',0)");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK
                                                 SET CODE = N'{CODEY}', ANBAR = 0
                                                 WHERE CODE = N'{rst}' AND ANBAR = 0");
                    }
                }
                #endregion
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این خدمات دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627)
                {
                    new Msgwin(false, "نام یا کد تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            #endregion

            #region Body
            try
            {
                dbms.DoExecuteSQL($@"DELETE FROM dbo.TAKHPERS WHERE(TAKH_COD = N'{CODE.Text}') "); //TAKH_COD

                foreach (var Row in TAKHPERS_DATA)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TAKHPERS(TAKH_COD, CUST_CO, TAFPER, PRICE_M)
                                         VALUES(N'{CODE.Text}',
                                         {Row.CUST_CO} ,
                                         {Row.TAFPER} ,
                                         {Row.PRICE_M})");

                    #region Form_AfterUpdate_Sub
                    var _N_KOL_ = Baseknow.HDARAM;
                    var _NUMBER_ = Row.CUST_CO;
                    var _TNUMBER_ = Row.TAKH_COD;
                    var _NAME_ = "تخفيف " + NAM.Text;
                    var _BED_BES_ = -1;

                    var rst = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TOP 1 N_KOL, NUMBER,TNUMBER, NAME FROM dbo.TDETA_HES WHERE N_KOL={Baseknow.HDARAM} AND NUMBER={Row.CUST_CO} AND TNUMBER = {Row.TAKH_COD}").FirstOrDefault();
                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_},
                                                 {_TNUMBER_},
                                                 N'{_NAME_}',
                                                 {_BED_BES_} )");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_},
                                                 TNUMBER = {_TNUMBER_}, NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                 WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER} AND TNUMBER = {rst.TNUMBER}");
                    }
                    #endregion
                }
            }
            catch (SqlException ex)
            {
                TAKHPERS_SUB_CANCEL_EDIT();
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این تخفیف دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این تخفیف تکراری وارد شده").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            #endregion

            ReGetData();

            NewRecord = false;

            universControl.PopNotifyShow("ذخیره انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

        } //// SAVE --------------
        private bool HeadIsValid()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //CODE.Text // کد
            if (string.IsNullOrEmpty(NAM.Text.Trim())) //نام خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام خدمات نمی تواند خالی باشد" });
            }
            if (VAHED.SelectedValue is null) //واحد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد نمی تواند خالی باشد" });
            }
            if (RADAH.SelectedValue is null) //گروه
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه نمی تواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(MABL_F.Text) || MABL_F.Text == "0") //قیمیت
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "قیمیت نمی تواند صفر یا خالی باشد" });
            }

            if (NewRecord && !string.IsNullOrEmpty(CODE.Text))
            {
                var rst = dbms.DoGetDataSQL<string?>($"SELECT CODE FROM dbo.STUF_DEF WHERE CODE = N'{CODE.Text}'").FirstOrDefault();
                if (rst is not null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "این کد از قبل وجود دارد , آنرا تغییر دهید" });
                }
            }


            //----------------------------------
            //N_FANI.Text //شماره فنی
            //TOZIH.Text //توضیح
            //DEPART.SelectedValue //واحد ارائه کننده خدمات
            //CMBAA.IsChecked //مالیات بر ارزش افزوده

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private bool BodyIsValid()
        {
            var errors = (from object i in TAKHPERS_SUB.ItemsSource
                          let c = TAKHPERS_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            foreach (var ROW in TAKHPERS_DATA)
            {
                if (string.IsNullOrEmpty(ROW?.CUST_CO.ToStringNullSafe()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "کد نوع مشتری نمی تواند خالی باشد" });
                }
                if (string.IsNullOrEmpty(ROW?.TAFPER.ToStringNullSafe()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف نمی تواند خالی باشد" });
                }
                if (Convert.ToDouble(ROW?.TAFPER) < 0 || Convert.ToDouble(ROW?.TAFPER) > 100)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف صحیح نیست" });
                }
                if (string.IsNullOrEmpty(ROW?.PRICE_M.ToStringNullSafe()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ نمی تواند خالی باشد" });
                }
                if (Convert.ToDouble(ROW?.PRICE_M) < 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ تخفیف نمی تواند منفی باشد" });
                }
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

        private void Command39_Click(object sender, RoutedEventArgs e)
        {

        }
        private void TAKHPERS_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                TAKHPERS_SUB_IsFocusedIn = false;
            }
            else //Is Focus inside of TAKHPERS_SUB
            {
                TAKHPERS_SUB_IsFocusedIn = true;
            }
        }
        private void TAKHPERS_SUB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void TAKHPERS_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void TAKHPERS_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TAKHPERS_SUB.Dispatcher.InvokeAsync(() =>
            {
                TAKHPERS_SUB.CellEditEnding -= TAKHPERS_SUB_CellEditEnding;
                TAKHPERS_SUB.RowEditEnding -= TAKHPERS_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TAKHPERS_SUB.CancelEdit();
                }
                else
                {
                    TAKHPERS_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TAKHPERS_SUB.RowEditEnding += TAKHPERS_SUB_RowEditEnding;
                TAKHPERS_SUB.CellEditEnding += TAKHPERS_SUB_CellEditEnding;
            });
        }
        private void TAKHPERS_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TAKHPERS_SUB.SelectedItem is not null)
            {
                if (TAKHPERS_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TAKHPERS)TAKHPERS_SUB.SelectedItem).Clone() as TAKHPERS;
                }
            }
        }
        private void TAKHPERS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_
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
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            }
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TAKHPERS;
            #endregion

            if (e.Column.SortMemberPath == "CUST_CO") //کد نوع مشتری
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("کد نوع مشتری نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    TAKHPERS_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.CUST_CO = WAS_ROW_ITEM?.CUST_CO;
                }
            }

            if (e.Column.SortMemberPath == "TAFPER") //درصد تخفیف
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("درصد تخفیف نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    TAKHPERS_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.TAFPER = WAS_ROW_ITEM?.TAFPER;
                }
                else if (Convert.ToDouble(ENTERED_VALUE_ROW) < 0 || Convert.ToDouble(ENTERED_VALUE_ROW) > 100)
                {
                    universControl.PopNotifyShow("درصد تخفیف صحیح نیست !", Pop1, Pop1Text1, Pop_Border1);
                    TAKHPERS_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.TAFPER = WAS_ROW_ITEM?.TAFPER;
                }
            }

            if (e.Column.SortMemberPath == "PRICE_M") //مبلغ
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مبلغ نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    TAKHPERS_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.PRICE_M = WAS_ROW_ITEM?.PRICE_M;
                }
            }
        }
        private void TAKHPERS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                }
                else
                {
                    e.Handled = true;
                }
            }
        }
        private void TAKHPERS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as TAKHPERS;
            bool Ok = true;

            if (string.IsNullOrEmpty(ROW?.CUST_CO.ToStringNullSafe()))
            {
                Ok = false;
            }
            if (string.IsNullOrEmpty(ROW?.TAFPER.ToStringNullSafe()))
            {
                Ok = false;
            }
            if (Convert.ToDouble(ROW?.TAFPER) < 0 || Convert.ToDouble(ROW?.TAFPER) > 100)
            {
                Ok = false;
            }
            if (string.IsNullOrEmpty(ROW?.PRICE_M.ToStringNullSafe()))
            {
                Ok = false;
            }
            if (Convert.ToDouble(ROW?.PRICE_M) < 0)
            {
                Ok = false;
            }

            if (!Ok)
            {
                TAKHPERS_SUB_CANCEL_EDIT();
            }
        }



        KHAD_MODEL NewRecordFresh;
        private void MoveReGetData(Jahat jahat, int? custom_postiion = null)
        {
            //Update CurrentViewItem
            if (RecordsKhad.View.CurrentItem != null)
            {
                var HEADER = RecordsKhad.View.CurrentItem as KHAD_MODEL;
                var DBData = dbms.DoGetDataSQL<KHAD_MODEL>($"SELECT CAST(CODE AS int) AS nc, CODE, NAME AS NAM, N_FANI, TOZIH, VAHED, RADAH, KINDK, MABL_F, DEPART, CMBAA FROM STUF_DEF WHERE (KINDK = 2) AND CODE = N'{CODE.Tag.ToStringNullSafe()}' ").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(KHAD_MODEL).GetProperties();
                    foreach (var property in properties)
                    {
                        if (property.CanWrite)
                        {
                            var value = property.GetValue(DBData);
                            property.SetValue(HEADER, value);
                        }
                    }
                    RecordsKhad.View.Refresh();
                }
            }

            switch (jahat)
            {
                case Jahat.FirstItem: //اولین
                    RecordsKhad.View.MoveCurrentToFirst();
                    break;
                case Jahat.BackItem: //قبلی
                    if (RecordsKhad.View.CurrentPosition > 0) //Possible To Back
                    {
                        if (NewRecord)
                        {
                            jahat = Jahat.LastItem;
                            RecordsKhad.View.MoveCurrentToLast();
                        }
                        else
                        {
                            RecordsKhad.View.MoveCurrentToPrevious();
                        }
                    }
                    break;

                case Jahat.NextItem: //بعدی
                    if (RecordsKhad.View.CurrentPosition < ((System.Windows.Data.ListCollectionView)RecordsKhad.View).Count - 1)
                    {
                        RecordsKhad.View.MoveCurrentToNext();
                    }
                    break;

                case Jahat.LastItem: //آخرین
                    RecordsKhad.View.MoveCurrentToLast();
                    break;

                case Jahat.CustomPosition:
                    if (custom_postiion > -1)
                    {
                        RecordsKhad.View.MoveCurrentToPosition((int)custom_postiion);
                    }
                    break;

                case Jahat.NewItem: //جدید خالی
                    if (!NewRecord)
                    {
                        var LastRow = RecordsKhad.View.Cast<KHAD_MODEL>().LastOrDefault();
                        if (LastRow.NAM is not null)
                        {
                            NewRecordFresh = new KHAD_MODEL(); //YOUR_MODEL_BINDS
                            ((ListCollectionView)RecordsKhad.View).AddNewItem(NewRecordFresh);
                        }
                    }
                    RecordsKhad.View.MoveCurrentTo(NewRecordFresh);
                    break;
            }

            if (RecordsKhad.View?.CurrentPosition is not null)
            {
                Current_Rec.Text = Convert.ToString(RecordsKhad.View?.CurrentPosition); //Current Record
            }
            RecCount.Text = Convert.ToString(((ListCollectionView)RecordsKhad.View).Count); //Record Count

            if (RecordsKhad.View?.CurrentItem is not null && jahat != Jahat.NewItem)
            {
                var ROW = RecordsKhad.View.CurrentItem as KHAD_MODEL;

                CODE.Text = ROW?.CODE.ToStringNullSafe();
                CODE.Tag = CODE.Text;

                NAM.Text = ROW?.NAM;
                N_FANI.Text = ROW?.N_FANI;
                TOZIH.Text = ROW?.TOZIH;
                MABL_F.Text = ROW?.MABL_F.ToStringNullSafe();

                VAHED.SelectedValue = ROW?.VAHED;
                RADAH.SelectedValue = ROW?.RADAH;

                if (ROW?.DEPART is null)
                {
                    DEPART.SelectedValue = ROW?.DEPART;
                }
                else
                {
                    DEPART.SelectedValue = CL_Generaly.VAHED_OF_USER;
                    DEPART.Items.Refresh();
                }

                CMBAA.IsChecked = ROW?.CMBAA;

                // Body (Detail):
                TAKHPERS_DATA?.Clear();

                if (!string.IsNullOrEmpty(CODE.Text))
                {
                    if (Convert.ToDouble(CODE.Text) > 0)
                    {
                        var _DATA_ = dbms.DoGetDataSQL<TAKHPERS>($"SELECT TAKH_COD, CUST_CO, TAFPER, PRICE_M, PERS, BLNS, PUT FROM dbo.TAKHPERS WHERE (TAKH_COD = N'{CODE.Text}')").ToList();
                        foreach (var item in _DATA_)
                            TAKHPERS_DATA.Add(item);
                    }
                }

            }

            if (jahat == Jahat.NewItem)
            {
                CODE.Text = null;
                CODE.Tag = null; //برای آپدیت که داشته باشیم کد قبلی چی بوده برای اینکه قابلیت تغییر کد هم داشته باشیم

                NAM.Text = null;
                N_FANI.Text = null;
                TOZIH.Text = null;
                MABL_F.Text = "0";

                VAHED.SelectedIndex = 0;
                RADAH.SelectedIndex = 0;

                DEPART.SelectedValue = CL_Generaly.VAHED_OF_USER;
                DEPART.Items.Refresh();

                CMBAA.IsChecked = false;

                TAKHPERS_DATA?.Clear();
            }
        }
        private void ReGetData()
        {
            //Head (Master):
            var MasterHead = dbms.DoGetDataSQL<KHAD_MODEL>($"SELECT CAST(CODE AS int) AS nc, CODE, NAME AS NAM, N_FANI, TOZIH, VAHED, RADAH, KINDK, MABL_F, DEPART, CMBAA FROM STUF_DEF WHERE (KINDK = 2) ORDER BY NAME").ToList();
            RecordsKhad.Source = MasterHead;
            RecordsKhad.Source = RecordsKhad.View.Cast<KHAD_MODEL>().OrderBy(item => item.nc).ToList();
            if (RecordsKhad.View?.CurrentPosition > 0)
            {
                MoveReGetData(Jahat.CustomPosition, RecordsKhad.View?.CurrentPosition);
            }
            else
            {
                MoveReGetData(Jahat.LastItem);
            }

        }
        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.NewItem);
            NAM.Focus();
            NewRecord = true;
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.LastItem);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.NextItem);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.BackItem);
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.FirstItem);
        }

        private void DELETE_KHAD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NewRecord) { return; }

                Msgwin msgwin = new Msgwin(true, "آیا از حذف این مشتری اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "تعریف خدمات",
                            recordId: CODE.Text,
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    dbms.DoExecuteSQL($@"DELETE FROM STUF_DEF WHERE CODE = N'{CODE.Text}' AND NAME = N'{NAM.Text}' AND KINDK = 2");

                    try
                    {
                        //dbms.DoExecuteSQL("DELETE FROM dbo.STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = 0 AND KINDK = 2");
                        //dbms.DoExecuteSQL("DELETE FROM dbo.STUF_FSK WHERE CODE = '" + CODE.Text + "' AND ANBAR = 0 AND KINDK = 2");
                    }
                    catch { }


                    ReGetData();

                    universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این خدمت دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
            }
        }
    }
}
