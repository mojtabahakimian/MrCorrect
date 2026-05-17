using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinMenus.SANATI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class OTHER_DTL : Window
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<OTHER_DTL_SUB_MONITOR> OTHER_DTL_DATA = new ObservableCollection<OTHER_DTL_SUB_MONITOR>();
        UniversControl universControl = new UniversControl();
        public double NUMBER { get; set; }
        public int G_NameCodeDefaultValue { get; set; }
        public byte OpenArgs { get; set; }
        public byte TAG = 1;
        //G_Flage = 0 ---> Insert
        //G_Flage = 1 ---> Update
        public byte G_Flag = 0;
        public double? G_CAM_KALY = 0;
        public double? G_CAM_POOR = 0;
        public string RecordSource { get; set; }
        public int CURRENT_COLUMN_INDEX { get; set; }
        public bool RADIF_COLUMN_TabStop { get; set; }

        private string KINDF;

        public Visual Win_US { get; set; }
        public double SMBAA { get; private set; }
        public class _CITIES1_
        {
            public int? OSCODE { get; set; }
            public string? OSNAME { get; set; }
        }

        /// <summary>
        /// IAM_head_lst_haval = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
        /// </summary>
        public OTHER_DTL(byte _openArgs, Visual _YOUR_VL_WIN)
        {
            InitializeComponent();
            OpenArgs = _openArgs;
            Win_US = _YOUR_VL_WIN;
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (OpenArgs == 2) // حواله ای
            {
                TAG = 2;
            }

            if (OpenArgs == 11) //برگه خروج سایر مواد از انبار
            {
                TAG = OpenArgs;
            }

            Form_Open();
            FillCombo();
            NAME_CODE_LOADITEM();
            Loaded_OtherDTL();
        }
        bool IsNull(object hTAF2) => hTAF2 == null ? true : false;

        void CAMIUN_NUM_AfterUpdate()
        {
            CAMIUN_NUM.PreviewLostKeyboardFocus -= CAMIUN_NUM_PreviewLostKeyboardFocus;

            // Safety check
            if (CAMIUN_NUM.SelectedItem == null || CAMIUN_NUM.SelectedValue == null)
            {
                return;
            }

            // ✅ استفاده از parameterized query برای جلوگیری از SQL Injection
            var rst = dbms.DoGetDataSQL<QRE_KH_05>(
                "SELECT TOP 100 PERCENT * FROM OTHER_DTL WHERE CAMIUN_NUM = @CamiunNum ORDER BY NUMBER DESC",
                new { CamiunNum = CAMIUN_NUM.SelectedValue.ToString() }
            ).ToList();

            if (rst.Count > 0)
            {
                // ✅ یک‌بار FirstOrDefault صدا زده شود
                var record = rst.FirstOrDefault();

                // پر کردن فیلدها با بررسی null و empty
                DRIVER_MOB.Text = !string.IsNullOrEmpty(record.DRIVER_MOB) ? record.DRIVER_MOB : "";
                CAMIUN.Text = !string.IsNullOrEmpty(record.CAMIUN) ? record.CAMIUN : "";

                // منطق اصلاح‌شده برای DRIVER (بر اساس منطق احتمالی VBA)
                DRIVER.Text = !string.IsNullOrEmpty(record.DRIVER) ? record.DRIVER : "";

                // بررسی مقدار عددی
                CAM_KHALY.Text = (record.CAM_KHALY.HasValue && record.CAM_KHALY.Value > 0)
                    ? record.CAM_KHALY.Value.ToString()
                    : "";
            }
            else
            {
                // پاک کردن فیلدها
                DRIVER_MOB.Text = "";
                CAMIUN.Text = "";
                DRIVER.Text = "";
                CAM_KHALY.Text = "";
                // ⚠️ CAMIUN_NUM را پاک نکنید چون کاربر انتخاب کرده
            }

            INSERTVAZN();
            Dispatcher.BeginInvoke(new Action(() => { CAMIUN.Focus(); }));

            //if (CAMIUN_NUM.SelectedItem != null && CAMIUN_NUM.SelectedValue != null)
            //{
            //    var rst = dbms.DoGetDataSQL<QRE_KH_05>("SELECT   TOP 100 PERCENT * FROM OTHER_DTL WHERE CAMIUN_NUM = '" + CAMIUN_NUM.SelectedValue.ToString() + "' ORDER BY NUMBER DESC").ToList();
            //    if (rst.Count > 0)
            //    {
            //        _ = !string.IsNullOrWhiteSpace(rst.FirstOrDefault().DRIVER_MOB) ? DRIVER_MOB.Text = rst.FirstOrDefault().DRIVER_MOB : DRIVER_MOB.Text = "";
            //        _ = !string.IsNullOrWhiteSpace(rst.FirstOrDefault().CAMIUN) ? CAMIUN.Text = rst.FirstOrDefault().CAMIUN : CAMIUN.Text = "";
            //        _ = !string.IsNullOrWhiteSpace(rst.FirstOrDefault().DRIVER) ? DRIVER.Text = rst.FirstOrDefault().DRIVER : DRIVER.Text = "";
            //        _ = rst.FirstOrDefault().CAM_KHALY > 0 ? CAM_KHALY.Text = rst.FirstOrDefault().CAM_KHALY.ToString() : CAM_KHALY.Text = "";
            //    }
            //    else
            //    {
            //        DRIVER_MOB.Text = "";
            //        CAMIUN_NUM.Text = "";
            //        CAMIUN.Text = "";
            //        CAM_KHALY.Text = "";
            //    }
            //    INSERTVAZN();

            //}
            CAMIUN_NUM.PreviewLostKeyboardFocus += CAMIUN_NUM_PreviewLostKeyboardFocus;
        }
        void DRIVER_AfterUpdate()
        {
            // var rst = new ADODB.Recordset();
            if (DRIVER.SelectedItem != null && DRIVER.SelectedValue != null)
            {
                var rst = dbms.DoGetDataSQL<QRE_KH_05>("SELECT   TOP 100 PERCENT * FROM OTHER_DTL WHERE DRIVER = '" + DRIVER.SelectedValue.ToString() + "' ORDER BY NUMBER DESC").ToList();
                if (rst.Count > 0)
                {
                    _ = rst.FirstOrDefault().DRIVER_MOB != null ? DRIVER_MOB.Text = rst.FirstOrDefault().DRIVER_MOB.ToString() : DRIVER_MOB.Text = "";
                    _ = rst.FirstOrDefault().CAMIUN_NUM != null ? CAMIUN_NUM.Text = rst.FirstOrDefault().CAMIUN_NUM.ToString() : CAMIUN_NUM.Text = "";
                    _ = rst.FirstOrDefault().CAMIUN != null ? CAMIUN.Text = rst.FirstOrDefault().CAMIUN.ToString() : CAMIUN.Text = "";
                    _ = rst.FirstOrDefault().CAM_KHALY > 0 ? CAM_KHALY.Text = rst.FirstOrDefault().CAM_KHALY.ToString() : CAM_KHALY.Text = "";
                }
                else
                {
                    DRIVER_MOB.Text = "";
                    CAMIUN_NUM.Text = "";
                    CAMIUN.Text = "";
                    CAM_KHALY.Text = "";
                }
                //DoCmd.RunCommand(acCmdSaveRecord);
                ReGetData();
                //DRIVER_MOB.Focus();
            }
        }
        /// <summary>
        /// برای فرم های دیگر مثل فاکتور فروش و خرید این رویداد به کار می آید
        /// </summary>
        void Form_Open()
        {
            INSERTVAZN();
        }

        void MAGHSAD_DblClick()
        {
            //DoCmd.OpenForm("TCODE_CODING", acFormDS, default, default, default, acDialog, 1);
        }
        private void INSERTVAZN()
        {
            VAZNH_COLUMN.Visibility = Visibility.Hidden; //VAZNH.ColumnHidden = true;
            if (Win_US is HEAD_LST_PISHFROOSH2)
            {
                //DoCmd.Close(acForm, "HEAD_LST_FROOSH22");
                if (OpenArgs == 1)
                {
                    var _TG_ = 20;
                    var _NUMBER_ = (Win_US as HEAD_LST_PISHFROOSH2).NUMBER.Text;
                    TAG = Convert.ToByte(_TG_);
                    NUMBER = Convert.ToDouble(_NUMBER_);

                    RecordSource = $"SELECT * FROM OTHER_DTL WHERE TAG = {_TG_} and NUMBER = " + _NUMBER_;
                    //var rst = dbms.DoGetDataSQL<QRE_KH_07>("SELECT  dbo.INVO_LST.CODE ,dbo.INVO_LST.RADIF FROM  dbo.INVO_LST LEFT OUTER JOIN   dbo.OTHER_DTL_SUB ON dbo.INVO_LST.NUMBER = dbo.OTHER_DTL_SUB.NUMBER AND dbo.INVO_LST.TAG = dbo.OTHER_DTL_SUB.TAGG AND   dbo.INVO_LST.CODE = dbo.OTHER_DTL_SUB.CODE WHERE (dbo.INVO_LST.NUMBER = " + _NUMBER_ + $") And (dbo.INVO_LST.TAG = {_TG_}) And (dbo.OTHER_DTL_SUB.NUMBER Is Null)").ToList();
                    //for (int i = 0; i < rst.Count; i++)
                    //{
                    //    var _RADIF_ = "NULL";
                    //    if (rst[i].RADIF != null)
                    //    {
                    //        _RADIF_ = rst[i].RADIF.ToString();
                    //    }

                    //    dbms.DoExecuteSQL($@"INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF)
                    //                                        VALUES({_NUMBER_}, {_TG_}, N'{rst[i].CODE}',{_RADIF_})");
                    //}

                    dbms.DoExecuteSQL(
                   "INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                   "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                   "WHERE i.NUMBER = " + _NUMBER_ + $" AND i.TAG = {_TG_} " +
                   "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) " +
                   "GROUP BY i.NUMBER, i.TAG, i.CODE");

                    dbms.DoExecuteSQL($"DELETE FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = {_TG_}) AND (NUMBER = " + _NUMBER_ + ") AND (NOT (CODE IN   (SELECT     CODE  FROM dbo.INVO_LST   WHERE     (NUMBER = " + _NUMBER_ + $") AND (TAG = {_TG_}))))");
                    // this.OTHER_DTL_SUB_SUB.Requery();
                }
                KINDF = "PISH";
            }
            else if (Win_US is HAVALE_EXIT_SAYER)
            {
                KINDF = "HAV_SAYER";
                if (OpenArgs == 11)
                {
                    var _TG_ = 11;
                    var _NUMBER_ = (Win_US as HAVALE_EXIT_SAYER).NUMBER.Text;
                    TAG = Convert.ToByte(_TG_);
                    NUMBER = Convert.ToDouble(_NUMBER_);

                    RecordSource = $"SELECT * FROM OTHER_DTL WHERE TAG = {_TG_} and NUMBER = " + _NUMBER_;

                    dbms.DoExecuteSQL(
                      "INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                      "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                      "WHERE i.NUMBER = " + _NUMBER_ + $" AND i.TAG = {_TG_} " +
                      "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) " +
                      "GROUP BY i.NUMBER, i.TAG, i.CODE");

                    dbms.DoExecuteSQL($"DELETE FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = {_TG_}) AND (NUMBER = " + _NUMBER_ + ") AND (NOT (CODE IN   (SELECT     CODE  FROM dbo.INVO_LST   WHERE     (NUMBER = " + _NUMBER_ + $") AND (TAG = {_TG_}))))");
                }
            }
            else if (Win_US is HEAD_LST_HAVL)
            {
                //DoCmd.Close(acForm, "HEAD_LST_FROOSH22");
                RecordSource = "SELECT * FROM OTHER_DTL WHERE TAG = 2 AND NUMBER = " + (Win_US as HEAD_LST_HAVL).NUMBER.Text;
                KINDF = "HAV";
            }
            else if (Win_US is HEAD_LST_FROOSH22)
            {
                KINDF = "FROOSH22";
                //if (!Forms["head_lst_froosh22"].Form.AllowEdits)
                //{
                //    DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "كليد اصلاح را بزنيد تا فاكتور قابل اصلاح باشد");
                //    DoCmd.Close(acForm, this.NAME);
                //}
                //else
                if (OpenArgs == 1)
                {
                    var _TG_ = 2;
                    var _NUMBER_ = (Win_US as HEAD_LST_FROOSH22).NUMBER.Text;
                    TAG = Convert.ToByte(_TG_);
                    NUMBER = Convert.ToDouble(_NUMBER_);

                    RecordSource = $"SELECT * FROM OTHER_DTL WHERE TAG = {_TG_} and NUMBER = " + _NUMBER_;
                    //var rst = dbms.DoGetDataSQL<QRE_KH_07>("SELECT  dbo.INVO_LST.CODE ,dbo.INVO_LST.RADIF FROM  dbo.INVO_LST LEFT OUTER JOIN   dbo.OTHER_DTL_SUB ON dbo.INVO_LST.NUMBER = dbo.OTHER_DTL_SUB.NUMBER AND dbo.INVO_LST.TAG = dbo.OTHER_DTL_SUB.TAGG AND   dbo.INVO_LST.CODE = dbo.OTHER_DTL_SUB.CODE WHERE (dbo.INVO_LST.NUMBER = " + _NUMBER_ + $") And (dbo.INVO_LST.TAG = {_TG_}) And (dbo.OTHER_DTL_SUB.NUMBER Is Null)").ToList();
                    //for (int i = 0; i < rst.Count; i++)
                    //{
                    //    var _RADIF_ = "NULL";
                    //    if (rst[i].RADIF != null)
                    //    {
                    //        _RADIF_ = rst[i].RADIF.ToString();
                    //    }

                    //    dbms.DoExecuteSQL($@"INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF)
                    //                                        VALUES({_NUMBER_}, {_TG_}, N'{rst[i].CODE}',{_RADIF_})");
                    //}

                    dbms.DoExecuteSQL(
                               "INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                               "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                               "WHERE i.NUMBER = " + _NUMBER_ + $" AND i.TAG = {_TG_} " +
                               "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) " +
                               "GROUP BY i.NUMBER, i.TAG, i.CODE");

                    dbms.DoExecuteSQL($"DELETE FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = {_TG_}) AND (NUMBER = " + _NUMBER_ + ") AND (NOT (CODE IN   (SELECT     CODE  FROM dbo.INVO_LST   WHERE     (NUMBER = " + _NUMBER_ + $") AND (TAG = {_TG_}))))");
                    // this.OTHER_DTL_SUB_SUB.Requery();
                }
            }
            else if (Win_US is HEAD_LST_RASID)
            {
                KINDF = "RASID";
                VAZNH_COLUMN.Visibility = Visibility.Visible; //ColumnHidden = false;
                //if (!Forms["head_lst_rasid"].Form.AllowEdits)
                //{
                //    DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "كليد اصلاح را بزنيد تا رسيد قابل اصلاح باشد");
                //    DoCmd.Close(acForm, this.NAME);
                //}
                /*else*/
                if (OpenArgs == 1)
                {
                    var _TG_ = 1;
                    var _NUMBER_ = (Win_US as HEAD_LST_RASID).NUMBER.Text;
                    TAG = Convert.ToByte(_TG_);
                    NUMBER = Convert.ToDouble(_NUMBER_);

                    RecordSource = $"SELECT * FROM OTHER_DTL WHERE TAG = {_TG_} and NUMBER = " + _NUMBER_;
                    //var rst = dbms.DoGetDataSQL<QRE_KH_07>("SELECT  dbo.INVO_LST.CODE ,dbo.INVO_LST.RADIF FROM  dbo.INVO_LST LEFT OUTER JOIN   dbo.OTHER_DTL_SUB ON dbo.INVO_LST.NUMBER = dbo.OTHER_DTL_SUB.NUMBER AND dbo.INVO_LST.TAG = dbo.OTHER_DTL_SUB.TAGG AND   dbo.INVO_LST.CODE = dbo.OTHER_DTL_SUB.CODE WHERE (dbo.INVO_LST.NUMBER = " + _NUMBER_ + $") And (dbo.INVO_LST.TAG = {_TG_}) And (dbo.OTHER_DTL_SUB.NUMBER Is Null)").ToList();
                    //for (int i = 0; i < rst.Count; i++)
                    //{
                    //    var _RADIF_ = "NULL";
                    //    if (rst[i].RADIF != null)
                    //    {
                    //        _RADIF_ = rst[i].RADIF.ToString();
                    //    }

                    //    dbms.DoExecuteSQL($@"INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF)
                    //                                        VALUES({_NUMBER_}, {_TG_}, N'{rst[i].CODE}',{_RADIF_})");
                    //}

                    dbms.DoExecuteSQL(
                      "INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                      "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                      "WHERE i.NUMBER = " + _NUMBER_ + $" AND i.TAG = {_TG_} " +
                      "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) " +
                      "GROUP BY i.NUMBER, i.TAG, i.CODE");

                    dbms.DoExecuteSQL($"DELETE FROM dbo.OTHER_DTL_SUB WHERE     (TAGG = {_TG_}) AND (NUMBER = " + _NUMBER_ + ") AND (NOT (CODE IN   (SELECT     CODE  FROM dbo.INVO_LST   WHERE     (NUMBER = " + _NUMBER_ + $") AND (TAG = {_TG_}))))");
                    // this.OTHER_DTL_SUB_SUB.Requery();
                }
            }
            else
            {
                //DoCmd.Close(acForm, this.NAME);
            }
        }

        //تابع های که اضافه شده
        /// <summary>
        /// پر کردن تمام کمبوباکس ها در لود پروژه
        /// </summary>
        void FillCombo()
        {
            DRIVER.SelectionChanged -= DRIVER_SelectionChanged;
            DRIVER.ItemsSource = dbms.DoGetDataSQL<Driver_Combo>("SELECT TOP 100 PERCENT DRIVER FROM dbo.OTHER_DTL GROUP BY DRIVER ORDER BY DRIVER").ToList();
            DRIVER.DisplayMemberPath = "DRIVER";
            DRIVER.SelectedValuePath = "DRIVER";
            DRIVER.SelectedItem = null;
            DRIVER.SelectionChanged += DRIVER_SelectionChanged;

            CAMIUN_NUM.SelectionChanged -= CAMIUN_NUM_SelectionChanged;
            CAMIUN_NUM.ItemsSource = dbms.DoGetDataSQL<Camiun_Num_Combo>("SELECT TOP 100 PERCENT CAMIUN_NUM FROM OTHER_DTL GROUP BY CAMIUN_NUM ORDER BY CAMIUN_NUM").ToList();
            CAMIUN_NUM.DisplayMemberPath = "CAMIUN_NUM";
            CAMIUN_NUM.SelectedValuePath = "CAMIUN_NUM";
            CAMIUN_NUM.SelectedItem = null;
            CAMIUN_NUM.SelectionChanged += CAMIUN_NUM_SelectionChanged;

            CAMIUN.ItemsSource = dbms.DoGetDataSQL<Camiun_Combo>("SELECT TOP 100 PERCENT CAMIUN FROM OTHER_DTL GROUP BY CAMIUN ORDER BY CAMIUN").ToList();
            CAMIUN.DisplayMemberPath = "CAMIUN";
            CAMIUN.SelectedValuePath = "CAMIUN";
            CAMIUN.SelectedItem = null;

            MAGHSAD.ItemsSource = dbms.DoGetDataSQL<_CITIES1_>("SELECT OSCODE, OSNAME FROM dbo.TCOD_OSTAN ORDER BY OSNAME").ToList();
            MAGHSAD.SelectedValuePath = "OSCODE";
            MAGHSAD.DisplayMemberPath = "OSNAME";
            MAGHSAD.SelectedItem = null;
        }
        /// <summary>
        /// تابع برای بازخوانی دیتاگرید
        /// </summary>
        void ReGetData()
        {
            if (NUMBER > 0)
            {
                var query = dbms.DoGetDataSQL<OTHER_DTL_SUB_MONITOR>($@"SELECT dbo.OTHER_DTL_SUB.NUMBER, dbo.OTHER_DTL_SUB.TAGG, dbo.OTHER_DTL_SUB.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.OTHER_DTL_SUB.CAM_KHALY, dbo.OTHER_DTL_SUB.CAM_POOR, 
                                                                             dbo.OTHER_DTL_SUB.MEGHk, dbo.OTHER_DTL_SUB.TOZIH, dbo.OTHER_DTL_SUB.RADIF, dbo.OTHER_DTL_SUB.VAZNH, dbo.OTHER_DTL_SUB.CRT, dbo.OTHER_DTL_SUB.UID
                                                                             FROM dbo.OTHER_DTL_SUB INNER JOIN
                                                                             dbo.STUF_DEF ON dbo.OTHER_DTL_SUB.CODE = dbo.STUF_DEF.CODE
                                                                             WHERE (dbo.OTHER_DTL_SUB.TAGG = {TAG}) AND dbo.OTHER_DTL_SUB.NUMBER =" + NUMBER).ToList();
                OTHER_DTL_DATA?.Clear();
                foreach (var item in query)
                    OTHER_DTL_DATA.Add(item);

                OTHER_DTL_SUB_SUB.ItemsSource = OTHER_DTL_DATA;
            }
        }
        /// <summary>
        /// برای پر کردن کالا در دیتاگرید
        /// </summary>
        void NAME_CODE_LOADITEM()
        {
            var query = dbms.DoGetDataSQL<Custom_NameCode>("SELECT STUF_DEF.CODE, STUF_DEF.NAME FROM STUF_DEF ORDER BY STUF_DEF.NAME").ToList();
            if (query.Count > 0)
            {
                G_NameCodeDefaultValue = Convert.ToInt32(query.FirstOrDefault().CODE);
                Baseknow.NAME_CODE = G_NameCodeDefaultValue;
                CODE_COLUMN.ItemsSource = query;
            }
        }
        /// <summary>
        /// افزودن و ویرایش با کمک متغییر G_Flage مدیریت می شود
        /// </summary>
        bool IsSavedHeader_OTHER_DTL()
        {
            double? L_CAM_KHALY = null;
            double? L_CAM_POOR = null;
            if (double.TryParse(CAM_KHALY.Text, out double parsedKhaly)) L_CAM_KHALY = parsedKhaly;
            if (double.TryParse(CAM_POOR.Text, out double parsedPoor)) L_CAM_POOR = parsedPoor;

            var param = new
            {
                Number = NUMBER,
                Tag = TAG,
                RequestNo = string.IsNullOrWhiteSpace(REQUEST_NO.Text) ? null : REQUEST_NO.Text,
                Barnameh = string.IsNullOrWhiteSpace(BARNAMEH.Text) ? null : BARNAMEH.Text,
                Driver = string.IsNullOrWhiteSpace(DRIVER.Text) ? null : DRIVER.Text,
                DriverMob = string.IsNullOrWhiteSpace(DRIVER_MOB.Text) ? null : DRIVER_MOB.Text,
                CamiunNum = string.IsNullOrWhiteSpace(CAMIUN_NUM.Text) ? null : CAMIUN_NUM.Text,
                Maghsad = MAGHSAD.SelectedValue,
                CamKhaly = L_CAM_KHALY,
                CamPoor = L_CAM_POOR,
                Tozih = string.IsNullOrWhiteSpace(TOZIH.Text) ? null : TOZIH.Text,
                Camiun = string.IsNullOrWhiteSpace(CAMIUN.Text) ? null : CAMIUN.Text
            };

            string query = @"MERGE dbo.OTHER_DTL WITH (HOLDLOCK) AS Target
                             USING (SELECT @Number AS NUMBER, @Tag AS TAG) AS Source
                                ON Target.NUMBER = Source.NUMBER AND Target.TAG = Source.TAG
                              WHEN MATCHED THEN
                                   UPDATE SET
                                          REQUEST_NO = @RequestNo,
                                          BARNAMEH = @Barnameh,
                                          DRIVER = @Driver,
                                          DRIVER_MOB = @DriverMob,
                                          CAMIUN_NUM = @CamiunNum,
                                          MAGHSAD = @Maghsad,
                                          CAM_KHALY = @CamKhaly,
                                          CAM_POOR = @CamPoor,
                                          TOZIH = @Tozih,
                                          CAMIUN = @Camiun
                              WHEN NOT MATCHED THEN
                                   INSERT (NUMBER, TAG, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN)
                                   VALUES (@Number, @Tag, @RequestNo, @Barnameh, @Driver, @DriverMob, @CamiunNum, @Maghsad, @CamKhaly, @CamPoor, @Tozih, @Camiun);";
            dbms.DoExecuteSQL(query, param);
            return true;
        }
        /// <summary>
        /// OtherDtl_Sub فقط ویرایش می شود
        /// </summary>
        void Save_OTHERDTL_SUB_Detail()
        {
            if (OTHER_DTL_SUB_SUB?.ItemsSource == null)
            {
                universControl.PopNotifyShow(".مقادیر خالی سطر نمی توان ذخیره کرد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            foreach (OTHER_DTL_SUB_MONITOR item in OTHER_DTL_SUB_SUB.ItemsSource)
            {
                if (item.NUMBER >= 0 && item.CAM_POOR >= 0 && item.CAM_KHALY >= 0 && item.MEGHk >= 0 && item.VAZNH >= 0)
                {
                    var param = new
                    {
                        CamKhaly = item.CAM_KHALY,
                        CamPoor = item.CAM_POOR,
                        Meghk = item.MEGHk,
                        Tozih = string.IsNullOrWhiteSpace(item.TOZIH) ? null : item.TOZIH,
                        Radif = item.RADIF,
                        Vaznh = item.VAZNH,
                        Number = NUMBER,
                        Tag = TAG,
                        Code = item.CODE
                    };

                    string query = @"UPDATE dbo.OTHER_DTL_SUB  SET
                                  CAM_KHALY = @CamKhaly,
                                  CAM_POOR = @CamPoor,
                                  MEGHk = @Meghk,
                                  TOZIH = @Tozih,
                                  RADIF = @Radif,
                                  VAZNH = @Vaznh
                                  WHERE NUMBER = @Number AND TAGG = @Tag AND CODE = @Code";
                    dbms.DoExecuteSQL(query, param);
                }
                else
                {
                    universControl.PopNotifyShow(".مقادیر سطرها را بصورت صحیح وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                }
            }
        }
        /// <summary>
        /// بازخوانی اطلاعات ثبت شده در زمان لود برنامه
        /// </summary>
        void Loaded_OtherDTL()
        {
            var query = dbms.DoGetDataSQL<OTHER_DTL_CSHARP>("SELECT * FROM dbo.OTHER_DTL WHERE NUMBER=" + NUMBER + $" AND TAG={TAG}");
            if (query.Any())
            {
                var result = query.First();
                REQUEST_NO.Text = result.REQUEST_NO;
                BARNAMEH.Text = result.BARNAMEH;
                DRIVER.Text = result.DRIVER;
                DRIVER_MOB.Text = result.DRIVER_MOB;
                CAMIUN_NUM.Text = result.CAMIUN_NUM;
                MAGHSAD.SelectedValue = result.MAGHSAD;
                CAM_KHALY.Text = result.CAM_KHALY.ToString();
                CAM_POOR.Text = result.CAM_POOR.ToString();
                TOZIH.Text = result.TOZIH;
                CAMIUN.Text = result.CAMIUN;
                G_Flag = 1;
            }
        }
        void ManageColumnsTabindex(object sender, KeyEventArgs e, string BND_NAME, bool TF)
        {
            if (OTHER_DTL_SUB_SUB.SelectedIndex < 0)
                return;
            var FOUND_COL_INDEX = OTHER_DTL_SUB_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == BND_NAME).DisplayIndex;
            // get the current cell
            var THECELL = e.OriginalSource as DataGridCell;
            //MyDataGrid1.Columns[0].IsHitTestVisible = false;

            //CELL
            var rowContainer = OTHER_DTL_SUB_SUB.ItemContainerGenerator.ContainerFromIndex(OTHER_DTL_SUB_SUB.SelectedIndex) as DataGridRow;
            DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);

            DataGridCell cell2 = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(FOUND_COL_INDEX);
            if (cell2 == null)
            {
                OTHER_DTL_SUB_SUB.ScrollIntoView(rowContainer, OTHER_DTL_SUB_SUB.Columns[CURRENT_COLUMN_INDEX]);
                THECELL = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            }
            else
            {
                THECELL = cell2;
            }
            //CELL
            if (!(THECELL is null))
            {
                THECELL.IsTabStop = TF;
                //e.Handled = true;
            }
        }
        void SpaceRemvo(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        void AccepterOnlyNumber(TextBox TXBX, TextCompositionEventArgs e, bool CanEnterDecimals = false)
        {
            if (CanEnterDecimals)
            {
                if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text))
                && (Convert.ToChar(e.TextComposition.Text) != '.'))
                {
                    e.Handled = true;
                }
                // only allow one decimal point
                if ((Convert.ToChar(e.TextComposition.Text) == '.') && (TXBX.Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (e.TextComposition.Text != "")
                {
                    if (!char.IsDigit(Convert.ToChar(e.TextComposition.Text)))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void Command27_Click(object sender, RoutedEventArgs e)
        {
            //var rst = new ADODB.Recordset();
            //var RST2 = new ADODB.Recordset();
            if (!(string.IsNullOrEmpty(DRIVER.Text) && CAMIUN_NUM.SelectedValue == null))
            {
                SMBAA = 0;

                switch (KINDF ?? "")
                {
                    case "PISH":
                        {
                            var rst = dbms.DoGetDataSQL<QRE_KH_06>("SELECT * FROM OTHER_DTL_sub WHERE tagg= 20 and MEGHk > 0 and NUMBER = " + NUMBER).ToList();
                            if (rst.Count > 0)
                            {
                                for (int i = 0; i < rst.Count; i++)
                                {
                                    var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST where tag = 20 and NUMBER = " + NUMBER + " and code = '" + rst[i].CODE + "'").ToList();
                                    if (RST2.Count == 1)
                                    {
                                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                           MEGH ={rst[i].MEGHk} ,
                                                                           MEGHk = {rst[i].MEGHk} ,
                                                                           MABL_K = {Math.Round((double)(rst[i].MEGHk * RST2[i].MABL))},
                                                                           N_MOIN = {Math.Round((double)(rst[i].MEGHk * RST2[i].MABL * RST2[i].N_KOL / 100))} 
                                                                           WHERE TAG = 20 AND NUMBER = " + NUMBER + " AND CODE = '" + rst[i].CODE + "'" + $"  AND id = {RST2.FirstOrDefault().id}  ");
                                        // RST2.update();
                                    }
                                    // rst.MoveNext();
                                }
                                if ((bool)(Win_US as HEAD_LST_PISHFROOSH2).TICMBAA.IsChecked)
                                {
                                    var rst3 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST WHERE NUMBER = " + NUMBER + " AND TAG = 20").ToList();
                                    for (int i = 0; i < rst3.Count; i++)
                                    {
                                        var RST2 = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + rst.FirstOrDefault().CODE + "'").ToList();
                                        if (RST2.Count > 0)
                                        {
                                            if ((bool)RST2[i].CMBAA)
                                            {
                                                rst3[i].IMBAA = Math.Round((double)((rst3[i].MABL_K - rst3[i].N_MOIN) / CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                                SMBAA = SMBAA + Math.Round((double)((rst3[i].MABL_K - rst3[i].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                            }
                                            else
                                            {
                                                rst3.FirstOrDefault().IMBAA = 0;
                                            }
                                        }
                                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET IMBAA = {rst3[i].IMBAA} WHERE NUMBER = {NUMBER} AND TAG = 20 AND id = {rst3[i].id}");
                                        //rst.update();
                                        //rst.MoveNext();
                                    }
                                    if (SMBAA != Convert.ToDouble((Win_US as HEAD_LST_PISHFROOSH2).MBAA.Text) && SMBAA > 0)
                                    {
                                        (Win_US as HEAD_LST_PISHFROOSH2).MBAA.Text = SMBAA.ToString();
                                    }
                                }
                            }
                            //Forms["HEAD_LST_PISHFROOSH2"]["INVO_LST_PISHFROOSH_SUB"].Requery();
                            break;
                        }
                    case "FROOSH22":
                        {
                            var rst = dbms.DoGetDataSQL<QRE_KH_06>("SELECT * FROM OTHER_DTL_sub WHERE tagg= 2 and MEGHk > 0 and NUMBER = " + NUMBER).ToList();
                            if (rst.Count > 0)
                            {
                                for (int i = 0; i < rst.Count; i++) //while (/*!rst.EOF 1 == 1)
                                {
                                    var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("select * from invo_lst where tag = 2 and NUMBER = " + NUMBER + " and code = '" + rst[i].CODE + "'").ToList();
                                    if (RST2.Count == 1)
                                    {
                                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                           MEGH ={rst[i].MEGHk} ,
                                                                           MEGHk = {rst[i].MEGHk} ,
                                                                           MABL_K = {Math.Round((double)(rst[i].MEGHk * RST2[i].MABL))},
                                                                           N_MOIN = {Math.Round((double)(rst[i].MEGHk * RST2[i].MABL * RST2[i].N_KOL / 100))} 
                                                                           WHERE TAG = 2 AND NUMBER = " + NUMBER + " AND CODE = '" + rst[i].CODE + "'" + $"  AND id = {RST2.FirstOrDefault().id}  ");

                                        //RST2.update();
                                    }
                                    // rst.MoveNext();
                                }
                                if ((bool)(Win_US as HEAD_LST_FROOSH22).TICMBAA.IsChecked)
                                {
                                    var rst3 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST WHERE NUMBER = " + NUMBER + " AND TAG = 2").ToList();
                                    for (int i = 0; i < rst3.Count; i++)
                                    {
                                        var RST2 = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + rst.FirstOrDefault().CODE + "'").ToList();
                                        if (RST2.Count > 0)
                                        {
                                            if ((bool)RST2[i].CMBAA)
                                            {
                                                rst3[i].IMBAA = Math.Round((double)((rst3[i].MABL_K - rst3[i].N_MOIN) / CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                                SMBAA = SMBAA + Math.Round((double)((rst3[i].MABL_K - rst3[i].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                            }
                                            else
                                            {
                                                rst3.FirstOrDefault().IMBAA = 0;
                                            }
                                        }
                                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET IMBAA = {rst3[i].IMBAA} WHERE NUMBER = {NUMBER} AND TAG = 2 AND id = {rst3[i].id}");
                                        //rst.update();
                                        //rst.MoveNext();
                                    }
                                    if (SMBAA != Convert.ToDouble((Win_US as HEAD_LST_FROOSH22).MBAA.Text) && SMBAA > 0)
                                    {
                                        (Win_US as HEAD_LST_FROOSH22).MBAA.Text = SMBAA.ToString();
                                        (Win_US as HEAD_LST_FROOSH22).HMBAA.Text = Baseknow.HESMBAA;
                                    }
                                }
                            }
                            // Forms["HEAD_LST_FROOSH22"]["INVO_LST_sub"].Requery();
                            break;
                        }
                    case "RASID":
                        {
                            var rst = dbms.DoGetDataSQL<QRE_KH_06>("SELECT * FROM OTHER_DTL_sub WHERE tagg= 1 and MEGHk > 0 and NUMBER = " + NUMBER).ToList();
                            if (rst.Count > 0)
                            {
                                for (int i = 0; i < rst.Count; i++)
                                {
                                    var RST2 = dbms.DoGetDataSQL<INVO_LST_FACTOR22>("SELECT * FROM INVO_LST WHERE tag = 1 and NUMBER = " + NUMBER + " and code = '" + rst[i].CODE + "'").ToList();
                                    if (RST2.Count == 1)
                                    {
                                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                           MEGH = {rst[i].VAZNH} ,
                                                                           MEGHk = {rst[i].VAZNH} ,
                                                                           MEGH_R = {rst[i].VAZNH} ,
                                                                           MABL_K = {Math.Round((double)(rst[i].VAZNH * RST2[0].MABL))}
                                                                           WHERE tag = 1 AND NUMBER = " + NUMBER + " and code = '" + rst[i].CODE + "'" + $" AND id = {RST2.FirstOrDefault().id} ");
                                        // RST2.update();
                                    }
                                    //rst.MoveNext();
                                }
                                universControl.PopNotifyShow(".وزن کالاها به حواله انتقال یافت", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                            }
                            // Forms["HEAD_LST_RASID"]["INVO_LST_RASID_SUB"].Requery();
                            break;
                        }
                    case "HAV_SAYER":
                        {
                            ////var rst = dbms.DoGetDataSQL<QRE_KH_06>("SELECT * FROM OTHER_DTL_sub WHERE tagg= 11 and MEGHk > 0 and NUMBER = " + NUMBER).ToList();
                            ////if (rst.Count > 0)
                            ////{
                            ////    for (int i = 0; i < rst.Count; i++) //while (/*!rst.EOF 1 == 1)
                            ////    {
                            ////        var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("select * from invo_lst where tag = 11 and NUMBER = " + NUMBER + " and code = '" + rst[i].CODE + "'").ToList();
                            ////        if (RST2.Count == 1)
                            ////        {
                            ////            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                            ////                                               MEGH ={rst[i].MEGHk} ,
                            ////                                               MEGHk = {rst[i].MEGHk} ,
                            ////                                               MABL_K = {Math.Round((double)(rst[i].MEGHk * RST2[0].MABL))},
                            ////                                               N_MOIN = {Math.Round((double)(rst[i].MEGHk * RST2[0].MABL * RST2[0].N_KOL / 100))} 
                            ////                                               WHERE TAG = 11 AND NUMBER = " + NUMBER + " AND CODE = '" + rst[i].CODE + "'" + $"  AND id = {RST2.FirstOrDefault().id}  ");

                            ////            //RST2.update();
                            ////        }
                            ////        // rst.MoveNext();
                            ////    }
                            ////}
                            break;
                        }
                }
                //DoCmd.Close(acForm, this.NAME);
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveAllSuccess();
        }

        private bool SaveAllSuccess()
        {
            if (CAMIUN_NUM.Text?.Length > 100)
            {
                universControl.PopNotifyShowUp(".شماره ماشین نباید بیشتر از 100 کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (REQUEST_NO.Text?.Length > 10)
            {
                universControl.PopNotifyShowUp(".شماره درخواست نباید بیشتر از ۱۰ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (BARNAMEH.Text?.Length > 25)
            {
                universControl.PopNotifyShowUp(".شماره بارنامه نباید بیشتر از ۲۵ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (DRIVER.Text?.Length > 50)
            {
                universControl.PopNotifyShowUp(".نام راننده نباید بیشتر از ۵۰ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (DRIVER_MOB.Text?.Length > 25)
            {
                universControl.PopNotifyShowUp(".موبایل راننده نباید بیشتر از ۲۵ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (CAMIUN.Text?.Length > 50)
            {
                universControl.PopNotifyShowUp(".نوع ماشین نباید بیشتر از ۵۰ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }
            if (TOZIH.Text?.Length > 150)
            {
                universControl.PopNotifyShowUp(".توضیحات نباید بیشتر از ۱۵۰ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return false;
            }

            try
            {
                if (IsSavedHeader_OTHER_DTL()) //Succeed Saved Header
                {
                    if (OTHER_DTL_DATA.Any())
                    {
                        Save_OTHERDTL_SUB_Detail();
                    }
                    G_Flag = 1;
                    universControl.PopNotifyShow(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
                else
                {
                    universControl.PopNotifyShowUp(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                }
            }
            catch (Exception ex)
            {

                CL_LMethods.DoWriteMyLog("Error In SaveAllSuccess() Method In Other_Dtl_CSharp", ex);
                new Msgwin(false, "خطایی وجود دارد و امکان ذخیره نیست مجددا تلاش کنید").Show();
                return false;
            }

            return true;
        }

        private void DRIVER_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void CAMIUN_NUM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ManageColumnsTabindex(sender, e, "RADIF", RADIF_COLUMN_TabStop);
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!SaveAllSuccess())
            {
                Msgwin msgwin = new Msgwin(true, "آیتم های وارد شده صحیح نیست و ذخیره نشده آیا از بستن پنجره مطمئن هستید ؟");
                msgwin.ShowDialog();

                if (msgwin.DialogResult != true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void REQUEST_NO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void OTHER_DTL_SUB_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.SortMemberPath.Contains("CAM_KHALY"))
            {
                var dgv = (OTHER_DTL_SUB_MONITOR)OTHER_DTL_SUB_SUB.SelectedItem;
                G_CAM_KALY = dgv.CAM_KHALY;
            }
            if (e.Column.SortMemberPath.Contains("CAM_POOR"))
            {
                var dgv = (OTHER_DTL_SUB_MONITOR)OTHER_DTL_SUB_SUB.SelectedItem;
                G_CAM_POOR = dgv.CAM_POOR;
                if (G_CAM_KALY > G_CAM_POOR)
                {
                    new Msgwin(false, "وزن خالی نمی تواند بیشتر از وزن پر باشد").ShowDialog();
                    dgv.MEGHk = 0;
                }
                else
                    dgv.MEGHk = G_CAM_POOR - G_CAM_KALY;
            }
        }
        private void OTHER_DTL_SUB_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                if (!OTHER_DTL_SUB_SUB.IsEnabled || OTHER_DTL_SUB_SUB.IsReadOnly || !IsVisible) { return; }

                var editableCollectionView = OTHER_DTL_SUB_SUB.Items as IEditableCollectionView;
                if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
                {
                    try { editableCollectionView.CancelEdit(); } catch { }
                }

                _ = AuditLogger.LogActionAsync(
                      actionType: "DELETE",
                      tableName: "سایر اطلاعات Ctrl + G",
                      recordId: NUMBER.ToString(),
                      oldValue: TAG.ToString(),
                      newValue: null,
                      additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (OTHER_DTL_DATA.Count > 0)
                {
                    if (OTHER_DTL_SUB_SUB.SelectedItems != null && OTHER_DTL_SUB_SUB.SelectedItems.Count > 0)
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            List<MsgModel> ErrosMessages = new List<MsgModel>();
                            for (int i = 0; i < OTHER_DTL_SUB_SUB.SelectedItems.Count; i++)
                            {
                                var item = OTHER_DTL_SUB_SUB.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(OTHER_DTL_SUB_SUB, item)) // Check if the item is a new placeholder Row
                                {
                                    OTHER_DTL_DATA.Remove((OTHER_DTL_SUB_MONITOR)item);
                                    continue; // Skip deletion for new placeholder items
                                }

                                var _NUMBER_ = item.GetType().GetProperty("NUMBER").GetValue(item);
                                var _TAGG_ = item.GetType().GetProperty("TAGG").GetValue(item);
                                var _CODE_ = item.GetType().GetProperty("CODE").GetValue(item);

                                if (_NUMBER_ != null && _TAGG_ != null && _CODE_ != null)
                                {
                                    try
                                    {
                                        dbms.DoExecuteSQL($@"DELETE FROM dbo.OTHER_DTL_SUB WHERE NUMBER = {_NUMBER_} AND TAGG = {_TAGG_} AND CODE = {_CODE_}");
                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 547)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                                        }
                                        else
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                                    }
                                }
                            }

                            if (ErrosMessages.Any())
                            {
                                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                                new MsgListwin(false, ErrosMessages).ShowDialog();
                            }

                            ReGetData();
                        }
                    }
                }
            }
        }
        private void OTHER_DTL_SUB_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var REND_ROW = e.Row.Item as OTHER_DTL_SUB_MONITOR;
            var AllisWell = true;
            if (REND_ROW is null)
            {
                universControl.PopNotifyShow(".بعضی از مقادیر سطر خالی یا غیر مجاز است", Pop1, Pop1Text1, Pop_Border1);
                AllisWell = false;
            }
            else if (
                          string.IsNullOrEmpty(REND_ROW.CODE.ToStringNullSafe()) ||
                          string.IsNullOrEmpty(REND_ROW.NAME_CODE.ToStringNullSafe()) ||
                          string.IsNullOrEmpty(REND_ROW.CAM_KHALY.ToStringNullSafe()) ||
                          string.IsNullOrEmpty(REND_ROW.CAM_POOR.ToStringNullSafe()) ||
                          string.IsNullOrEmpty(REND_ROW.VAZNH.ToStringNullSafe()) ||
                          string.IsNullOrEmpty(REND_ROW.MEGHk.ToStringNullSafe())
                      //||
                      //REND_ROW.CAM_KHALY is 0 ||
                      //REND_ROW.CAM_POOR is 0 ||
                      //REND_ROW.VAZNH is 0 ||
                      //REND_ROW.MEGHk is 0
                      )
            {
                universControl.PopNotifyShow(".بعضی از مقادیر سطر خالی یا غیر مجاز است", Pop1, Pop1Text1, Pop_Border1);
                AllisWell = false;
            }
            if (AllisWell)
            {
                /////DGR_SUB_INVOLST.CommitEdit();
            }
            else
            {
                e.Cancel = true;
                return;
            }
        }

        private void REQUEST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(REQUEST_NO.Text)) { REQUEST_NO.Text = "0"; }
        }
        private void REQUEST_NO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(REQUEST_NO, e);
        }
        private void BARNAMEH_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void BARNAMEH_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(DRIVER_MOB, e);
        }
        private void BARNAMEH_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(BARNAMEH.Text)) { BARNAMEH.Text = "0"; }
        }
        private void DRIVER_MOB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void DRIVER_MOB_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(DRIVER_MOB.Text)) { DRIVER_MOB.Text = "0"; }
        }
        private void DRIVER_MOB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(DRIVER_MOB, e);
        }
        private void CAM_KHALY_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void CAM_KHALY_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(CAM_KHALY.Text)) { CAM_KHALY.Text = "0"; }
        }
        private void CAM_KHALY_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            AccepterOnlyNumber(CAM_KHALY, e);
        }
        private void CAM_POOR_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            SpaceRemvo(sender, e);
        }
        private void CAM_POOR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(CAM_POOR.Text)) { CAM_POOR.Text = "0"; }
        }
        private void CAM_POOR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //AccepterOnlyNumber(CAM_POOR.Text, e);
        }


        private void Command26_Click(object sender, RoutedEventArgs e)
        {
            //DoCmd.RunCommand(acCmdSaveRecord);
            if (OTHER_DTL_DATA.Count > 0)
            {
                universControl.PopNotifyShowUp("از قبل داده وجود دارد , آنرا حذف یا اصلاح کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                return;
            }

            Save_Click(null, null);

            if (!(string.IsNullOrEmpty((DRIVER.Template.FindName("PART_EditableTextBox", DRIVER) as TextBox).Text) && string.IsNullOrEmpty((CAMIUN.Template.FindName("PART_EditableTextBox", CAMIUN) as TextBox).Text)))
            {
                switch (KINDF ?? "")
                {
                    case "PISH":
                        {
                            //dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) SELECT     NUMBER, TAG, CODE, RADIF  FROM         dbo.INVO_LST WHERE  TAG = 20 AND NUMBER = " + this.NUMBER);
                            dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF)" +
                                " SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                                "WHERE i.TAG = 20 AND i.NUMBER = " + this.NUMBER +
                                " AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) GROUP BY i.NUMBER, i.TAG, i.CODE");

                            break;
                        }
                    case "HAV_SAYER":
                        {
                            dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                                "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                                "WHERE i.TAG = 11 AND i.NUMBER = " + this.NUMBER + " " +
                                "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) GROUP BY i.NUMBER, i.TAG, i.CODE");
                            break;
                        }
                    case "HAV":
                        {
                            //dbms.DoExecuteSQL("DELETE FROM dbo.OTHER_DTL_SUB WHERE TAGG = 2 AND NUMBER = " + this.NUMBER);
                            //dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) SELECT     NUMBER, TAG, CODE, RADIF  FROM         dbo.INVO_LST WHERE  TAG = 2 AND NUMBER = " + this.NUMBER);
                            dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                                "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i " +
                                "WHERE i.TAG = 2 AND i.NUMBER = " + this.NUMBER + " " +
                                "AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) GROUP BY i.NUMBER, i.TAG, i.CODE");
                            break;
                        }
                    case "FROOSH22":
                        {
                            //dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) SELECT     NUMBER, TAG, CODE, RADIF  FROM         dbo.INVO_LST WHERE  TAG = 2 AND NUMBER = " + this.NUMBER);
                            dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) " +
                                "SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i" +
                                " WHERE i.TAG = 2 AND i.NUMBER = " + this.NUMBER +
                                " AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) GROUP BY i.NUMBER, i.TAG, i.CODE");
                            break;
                        }
                    case "RASID":
                        {
                            //dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF) SELECT     NUMBER, TAG, CODE, RADIF  FROM         dbo.INVO_LST WHERE  TAG = 1 AND NUMBER = " + this.NUMBER);
                            dbms.DoExecuteSQL("INSERT INTO dbo.OTHER_DTL_SUB (NUMBER, TAGG, CODE, RADIF)" +
                                " SELECT i.NUMBER, i.TAG, i.CODE, MIN(i.RADIF) FROM dbo.INVO_LST i" +
                                " WHERE i.TAG = 1 AND i.NUMBER = " + this.NUMBER +
                                " AND NOT EXISTS (SELECT 1 FROM dbo.OTHER_DTL_SUB s WHERE s.NUMBER = i.NUMBER AND s.TAGG = i.TAG AND s.CODE = i.CODE) GROUP BY i.NUMBER, i.TAG, i.CODE");
                            break;
                        }
                }
                ReGetData();
                //this.OTHER_DTL_SUB_SUB.Requery();
            }
            else
            {
                universControl.PopNotifyShow("ابتدا مشخصات راننده و كاميون را وارد كنيد ", Pop1, Pop1Text1, Pop_Border1);
            }
        }

        private void DRIVER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DRIVER.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox CUTSNO_TEX = (TextBox)DRIVER.Template.FindName("PART_EditableTextBox", DRIVER);
            if (CUTSNO_TEX is null)
            {
                return;
            }

            DRIVER_AfterUpdate();
        }

        private void CAMIUN_NUM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CAMIUN_NUM.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CAMIUN_NUM_TEX = (TextBox)CAMIUN_NUM.Template.FindName("PART_EditableTextBox", CAMIUN_NUM);
            if (CAMIUN_NUM_TEX is null)
            {
                return;
            }

            CAMIUN_NUM_AfterUpdate();
        }

        private void CAMIUN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
