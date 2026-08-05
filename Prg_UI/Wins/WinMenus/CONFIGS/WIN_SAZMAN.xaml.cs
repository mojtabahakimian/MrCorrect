using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.Checkha;
using Prg_UI.Wins.WinMenus.CONFIGS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Functions.SMSService.SmsServiceFactory;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.CONFIGS
{
    public partial class WIN_SAZMAN : Window
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

        public ObservableCollection<DAFT_ASN> DAFT_ASN_DATA { get; set; } = new ObservableCollection<DAFT_ASN>();

        public WIN_SAZMAN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; set; }

        #region LOCALMODEL

        #endregion


        private byte[] _uploadedImageData = null;

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

                #region GENERAL_TAB
                WIDTH_D.IsReadOnly = !ican;
                HIGH_D.IsReadOnly = !ican;
                BACKPATH.IsReadOnly = !ican;
                TFADDRESS.IsReadOnly = !ican;
                TFTEL.IsReadOnly = !ican;
                SERVERNAM.IsReadOnly = !ican;
                //OPTIONSS.IsReadOnly = !ican;

                EMZA_CANVAS.IsEnabled = ican;
                BTN_UPIMAGE.IsEnabled = ican;
                BTN_SAVE_G.IsEnabled = ican;
                BTN_SAVE.IsEnabled = ican;
                BTN_MORE.IsEnabled = ican;
                #endregion

                #region ASNAD_VA_HESAB
                GRP_PERSON.IsEnabled = ican;  //اشخاص مشخص شده در حسابها

                var RST = dbms.DoGetDataSQL<int?>("SELECT TOP 1 NUMBER FROM HEAD_LST WHERE TAG = 13").FirstOrDefault();
                if (RST == null)
                {
                    GRP_TKHF.IsEnabled = ican; //تخفیفات فاکتورها
                }
                else
                {
                    GRP_TKHF.IsEnabled = false; //تخفیفات فاکتورها
                }

                GRP_SANAD.IsEnabled = ican; //سند زدن

                SIGN.IsEnabled = ican; //گردش كاری و فرمها با امضاء عمل شود
                CTL_DT.IsEnabled = ican; //تاريخ فقط مربوط به همين سال باشد
                SNDKH.IsEnabled = ican; //سند ها روزانه باشد
                SAGHF.IsEnabled = ican; //سقف اعتبار 1
                SAGHF2.IsEnabled = ican; //سقف اعتبار 2

                CPI.IsReadOnly = !ican; //تاریخ قابل برگشت
                ARSESH.IsReadOnly = !ican;  //درصد مالیات بر ارزش افزوده
                TFTPAGE.IsReadOnly = !ican; //فاصله از پائين كاغذ در چاپ سند :

                //دفتر اسناد رسمی
                DAFT_ASN_SUB.IsReadOnly = !ican;
                #endregion

                #region FACTOR_VA_ANBAR
                GRP_GHAYM.IsEnabled = ican;
                GRP_KALA.IsEnabled = ican;
                GRP_FRUP.IsEnabled = ican;
                GRP_WAR.IsEnabled = ican;
                GRP_LST.IsEnabled = ican;
                GRP_UPDDATE.IsEnabled = ican;

                MOJU.IsEnabled = ican;
                RMOG.IsEnabled = ican;
                DIG.IsReadOnly = !ican;
                TFCODE_E.IsReadOnly = !ican;
                CODEVIEW.IsEnabled = ican;
                MAND.IsEnabled = ican;
                SERFACB.IsEnabled = ican;
                LOCKFAP.IsEnabled = ican;
                DEFANB.IsEnabled = ican;
                DEFTKH.IsEnabled = ican;
                TRANSF.IsEnabled = ican;
                #endregion

                #region SALARY
                HAZEDAR.IsEnabled = ican;
                HAZTOLID.IsEnabled = ican;
                HAZFROOSH.IsEnabled = ican;
                HAZKHADAMAT.IsEnabled = ican;
                EDABIM.IsEnabled = ican;
                HAZBIM.IsEnabled = ican;
                BESHO.IsEnabled = ican;
                BEDMOS.IsEnabled = ican;
                PARDAKH.IsEnabled = ican;
                HAZMALI.IsEnabled = ican;
                PSANDHES.IsEnabled = ican;
                SANAVP.IsReadOnly = !ican;
                SAGHFH.IsReadOnly = !ican;
                HEZA.IsEnabled = ican;
                HPAD.IsEnabled = ican;
                HOLA.IsEnabled = ican;
                HKHA.IsEnabled = ican;
                HNAH.IsEnabled = ican;
                HJAZ.IsEnabled = ican;
                HRAN.IsEnabled = ican;
                HCON.IsEnabled = ican;
                HSAY.IsEnabled = ican;
                HSHI.IsEnabled = ican;
                HBON.IsEnabled = ican;
                HTAHOL.IsEnabled = ican;

                BTN_MORE_SALARY.IsEnabled = ican;
                BTN_TABLEMALIAT.IsEnabled = ican;
                #endregion

                #region SANATI
                ECONM.IsEnabled = ican;
                SANAT.IsEnabled = ican;

                // شماره شروع‌ها
                STFR.IsReadOnly = !ican;
                STKH.IsReadOnly = !ican;
                STHFR.IsReadOnly = !ican;
                STHKH.IsReadOnly = !ican;
                STENT.IsReadOnly = !ican;
                STKHS.IsReadOnly = !ican;
                STKHH.IsReadOnly = !ican;
                STTOL.IsReadOnly = !ican;
                STFRB.IsReadOnly = !ican;
                STBKH.IsReadOnly = !ican;
                STMO.IsReadOnly = !ican;
                STKHA.IsReadOnly = !ican;

                // چک‌باکس‌های عوامل حقوق
                SA_HOGH.IsEnabled = ican;
                SA_40EZ.IsEnabled = ican;
                SA_EZAF.IsEnabled = ican;
                SA_PADA.IsEnabled = ican;
                SA_HOLA.IsEnabled = ican;
                SA_KHAR.IsEnabled = ican;
                SA_NAHA.IsEnabled = ican;
                SA_JAZB.IsEnabled = ican;
                SA_RAND.IsEnabled = ican;
                SA_COND.IsEnabled = ican;
                SA_SAYE.IsEnabled = ican;
                SA_23BI.IsEnabled = ican;
                GRP_FINALS.IsEnabled = ican;
                #endregion

                #region SMS_TAB
                RB_TUBA.IsEnabled = ican;
                RB_SMSIR.IsEnabled = ican;
                SMSACT.IsEnabled = ican;
                PRMFR.IsEnabled = ican;
                DSMS.IsEnabled = ican;
                SMS_PASSWORD.IsEnabled = ican;

                if (Convert.ToBoolean(RB_SMSIR.IsChecked))
                {
                    SMS_LIBKEY.IsEnabled = ican;
                }

                SMS_USERNAME.IsReadOnly = !ican;
                SMS_TSMSHOST.IsReadOnly = !ican;
                SMS_OWNER.IsReadOnly = !ican;
                #endregion

                #region ISO
                ISO_FROOSH.IsReadOnly = !ican;
                ISO_KHAREED.IsReadOnly = !ican;
                ISO_MAVAD.IsReadOnly = !ican;
                ISO_TOLID.IsReadOnly = !ican;
                ISO_MAVADSAYER.IsReadOnly = !ican;
                ISO_DTOLID.IsReadOnly = !ican;
                #endregion

                #region CRM_SETTING
                IT1.IsReadOnly = !ican; IT2.IsReadOnly = !ican; IT3.IsReadOnly = !ican;
                IT4.IsReadOnly = !ican; IT5.IsReadOnly = !ican; IT6.IsReadOnly = !ican;
                IT7.IsReadOnly = !ican; IT8.IsReadOnly = !ican; IT9.IsReadOnly = !ican;
                IS1.IsReadOnly = !ican; IS2.IsReadOnly = !ican; IS3.IsReadOnly = !ican;
                IS4.IsReadOnly = !ican; IS5.IsReadOnly = !ican; IS6.IsReadOnly = !ican;
                IS7.IsReadOnly = !ican; IS8.IsReadOnly = !ican; IS9.IsReadOnly = !ican;
                IS10.IsReadOnly = !ican; IS11.IsReadOnly = !ican; IS12.IsReadOnly = !ican;
                IS14.IsReadOnly = !ican; IS15.IsReadOnly = !ican; IS16.IsReadOnly = !ican;
                #endregion

                #region SSM
                HDARKASRTAKHF.IsEnabled = ican;
                SSMTRTAKM.IsReadOnly = !ican;
                SSMTRTAGM.IsReadOnly = !ican;
                SSMSNDAUTO.IsEnabled = ican;
                SSMTBMON.IsReadOnly = !ican;
                SSMDARSAD.IsReadOnly = !ican;
                HDARKASRTAKHF.IsEnabled = ican;
                #endregion

                #region MODIAN
                PRIVIATEKEY.IsReadOnly = !ican;
                Dcertificate.IsReadOnly = !ican;
                MEMORYID.IsReadOnly = !ican;
                MEMORYIDsand.IsReadOnly = !ican;
                #endregion
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SYS", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded) { this.Close(); return; }

            var smsTypeColumn = dbms.DoGetDataSQL<int>("SELECT CASE WHEN COL_LENGTH('dbo.SAZMAN', 'SMSTYPE') IS NULL THEN 0 ELSE 1 END").FirstOrDefault() == 1 ? "SMSTYPE" : "'TSMS' AS SMSTYPE";
            var RST = dbms.DoGetDataSQL<SAZMAN>($"SELECT SMS_USERNAME,SMS_PASSWORD ,SMS_LIBKEY , SMS_TSMSHOST , DSMS , PRMFR , SMSACT , SMS_OWNER , {smsTypeColumn} ," +
                $"WIDTH_D,HIGH_D,BACKPATH,TFADDRESS,TFTEL,SERVERNAM,EMZA,OPTIONSS " + //عمومی
                $"PERSON,SANAD,TKHF,SIGN,CTL_DT,SNDKH,SAGHF,SAGHF2,CPI,ARSESH,TFTPAGE, " + //اسناد و حساب
                $"GHAYM,KALA,FRUP,WAR,UPDDATE,LST,MOJU,RMOG,DIG,TFCODE_E,CODEVIEW,MAND,SERFACB,LOCKFAP,DEFANB,DEFTKH,TRANSF, " + //فاکتور انبار
                $"HAZEDAR, HAZTOLID, HAZFROOSH, HAZKHADAMAT, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, PSANDHES, SANAVP, SAGHFH, HEZA, HPAD, HOLA, HKHA, HNAH, HJAZ, HRAN, HCON, HSAY, HSHI, HBON, HTAHOL, " + //حقوق
                $"FINALS,ECONM, SANAT, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, STTOL, STFRB, STBKH, STMO, STKHA, SA_HOGH, SA_40EZ, SA_EZAF, SA_PADA, SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, " + //Sanati
                $"ISO_FROOSH,ISO_KHAREED,ISO_MAVAD,ISO_TOLID,ISO_MAVADSAYER,ISO_DTOLID ," + //ISO
                $" IT1, IT2, IT3, IT4, IT5, IT6, IT7, IT8, IT9, IS1, IS2, IS3, IS4, IS5, IS6, IS7, IS8, IS9, IS10, IS11, IS12, IS14, IS15, IS16 ," + //CRM Setting
                $" SSMTRTAKM, SSMTRTAGM, SSMSNDAUTO, SSMTBMON, SSMDARSAD, HDARKASRTAKHF ," + //SSM
                $" PRIVIATEKEY,Dcertificate,MEMORYID,MEMORYIDsand " + //مودیان
                $" FROM dbo.SAZMAN ").FirstOrDefault();

            if (RST != null)
            {
                #region GENERAL_TAB
                WIDTH_D.Text = RST?.WIDTH_D;
                HIGH_D.Text = RST?.HIGH_D;
                BACKPATH.Text = RST?.BACKPATH;
                TFADDRESS.Text = RST?.TFADDRESS;
                TFTEL.Text = RST?.TFTEL;
                SERVERNAM.Text = RST?.SERVERNAM;
                OPTIONSS.Text = RST?.OPTIONSS;

                // Try to load existing signature
                if (RST?.EMZA != null && RST.EMZA.Length > 0)
                {
                    try
                    {
                        // First try to load the image directly
                        BitmapImage bitmapSource = CL_LMethods.ByteArrayToBitmapImage(RST.EMZA);

                        if (bitmapSource != null)
                        {
                            // We successfully loaded the image
                            ImageBrush brush = new ImageBrush(bitmapSource);
                            brush.Stretch = Stretch.Uniform;
                            EMZA_CANVAS.Background = brush;

                            // Disable drawing when showing an existing image
                            EMZA_CANVAS.EditingMode = InkCanvasEditingMode.None;
                            _uploadedImageData = RST.EMZA;
                        }
                        else
                        {
                            // If loading fails, try to load as InkCanvas strokes if applicable
                            try
                            {
                                using (MemoryStream ms = new MemoryStream(RST.EMZA))
                                {
                                    EMZA_CANVAS.Strokes.Clear();
                                    EMZA_CANVAS.Strokes = new System.Windows.Ink.StrokeCollection(ms);
                                }
                            }
                            catch
                            {
                                // If all attempts fail, just show a white background
                                EMZA_CANVAS.Background = Brushes.White;
                                EMZA_CANVAS.Strokes.Clear();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در بارگذاری تصویر از دیتابیس").ShowDialog();
                        EMZA_CANVAS.Background = Brushes.White;
                        EMZA_CANVAS.Strokes.Clear();
                    }
                }
                #endregion

                #region ASNAD_VA_HESAB_TAB
                //دفتر اسناد رسمی
                DAFT_ASN_DATA?.Clear();
                var RST_DAFT_ASN = dbms.DoGetDataSQL<DAFT_ASN>($"SELECT * FROM dbo.DAFT_ASN").ToList();
                foreach (var item in RST_DAFT_ASN)
                {
                    DAFT_ASN_DATA?.Add(item);
                }

                SetGrpOption(GRP_PERSON, (int)(RST?.PERSON ?? -1)); //اشخاص مشخص شده در حسابها
                SetGrpOption(GRP_TKHF, RST?.TKHF ?? -1); //تخفیفات فاکتورها
                SetGrpOption(GRP_SANAD, RST?.SANAD ?? -1); //سند زدن

                SIGN.IsChecked = RST?.SIGN ?? false; //گردش كاری و فرمها با امضاء عمل شود
                CTL_DT.IsChecked = RST?.CTL_DT ?? false; //تاريخ فقط مربوط به همين سال باشد
                SNDKH.IsChecked = RST?.SNDKH ?? false; //سند ها روزانه باشد
                SAGHF.IsChecked = RST?.SAGHF ?? false; //سقف اعتبار 1
                SAGHF2.IsChecked = RST?.SAGHF2 ?? false; //سقف اعتبار 2

                CPI.Text = RST?.CPI?.ToStringNullSafe(); //تاریخ قابل برگشت
                ARSESH.Text = RST?.ARSESH.ToStringNullSafe(); //درصد مالیات بر ارزش افزوده
                TFTPAGE.Text = RST?.TFTPAGE.ToStringNullSafe(); //فاصله از پائين كاغذ در چاپ سند :
                #endregion

                #region FACTOR_VA_ANBAR_TAB
                SetGrpOption(GRP_GHAYM, RST?.GHAYM ?? -1);
                SetGrpOption(GRP_KALA, RST?.KALA ?? -1);
                SetGrpOption(GRP_FRUP, Convert.ToInt32(RST?.FRUP));
                SetGrpOption(GRP_WAR, (int)(RST?.WAR));
                SetGrpOption(GRP_LST, (int)(RST?.LST));
                SetGrpOption(GRP_UPDDATE, Convert.ToInt32(RST?.UPDDATE));

                MOJU.IsChecked = RST.MOJU;
                RMOG.IsChecked = RST.RMOG;
                DIG.Text = RST.DIG?.ToString();
                TFCODE_E.Text = RST.TFCODE_E;
                CODEVIEW.IsChecked = RST.CODEVIEW == 1; // smallint to bool
                MAND.IsChecked = RST.MAND;
                SERFACB.IsChecked = RST.SERFACB;
                LOCKFAP.IsChecked = RST.LOCKFAP;
                DEFANB.SelectedValue = RST.DEFANB; DEFANB.Items.Refresh();
                DEFTKH.SelectedValue = RST.DEFTKH; DEFTKH.Items.Refresh();
                TRANSF.IsChecked = RST.TRANSF;
                #endregion

                #region SALARY_TAB
                HAZEDAR.SelectedValue = RST.HAZEDAR;
                HAZTOLID.SelectedValue = RST.HAZTOLID;
                HAZFROOSH.SelectedValue = RST.HAZFROOSH;
                HAZKHADAMAT.SelectedValue = RST.HAZKHADAMAT;
                EDABIM.SelectedValue = RST.EDABIM;
                HAZBIM.SelectedValue = RST.HAZBIM;
                BESHO.SelectedValue = RST.BESHO;
                BEDMOS.SelectedValue = RST.BEDMOS;
                PARDAKH.SelectedValue = RST.PARDAKH;
                HAZMALI.SelectedValue = RST.HAZMALI;
                PSANDHES.SelectedValue = RST.PSANDHES;

                SANAVP.Text = RST.SANAVP?.ToString();
                SAGHFH.Text = RST.SAGHFH?.ToString();

                HEZA.IsChecked = RST.HEZA;
                HPAD.IsChecked = RST.HPAD;
                HOLA.IsChecked = RST.HOLA;
                HKHA.IsChecked = RST.HKHA;
                HNAH.IsChecked = RST.HNAH;
                HJAZ.IsChecked = RST.HJAZ;
                HRAN.IsChecked = RST.HRAN;
                HCON.IsChecked = RST.HCON;
                HSAY.IsChecked = RST.HSAY;
                HSHI.IsChecked = RST.HSHI;
                HBON.IsChecked = RST.HBON ?? false;
                HTAHOL.IsChecked = RST.HTAHOL ?? false;
                #endregion

                #region SANATI_TAB
                ECONM.IsChecked = RST.ECONM;
                SANAT.IsChecked = RST.SANAT;
                // شماره شروع‌ها
                STFR.Text = RST.STFR?.ToString();
                STKH.Text = RST.STKH?.ToString();
                STHFR.Text = RST.STHFR?.ToString();
                STHKH.Text = RST.STHKH?.ToString();
                STENT.Text = RST.STENT?.ToString();
                STKHS.Text = RST.STKHS?.ToString();
                STKHH.Text = RST.STKHH?.ToString();
                STTOL.Text = RST.STTOL?.ToString();
                STFRB.Text = RST.STFRB?.ToString();
                STBKH.Text = RST.STBKH?.ToString();
                STMO.Text = RST.STMO?.ToString();
                STKHA.Text = RST.STKHA?.ToString();

                // چک‌باکس‌های عوامل حقوق
                SA_HOGH.IsChecked = RST.SA_HOGH;
                SA_40EZ.IsChecked = RST.SA_40EZ;
                SA_EZAF.IsChecked = RST.SA_EZAF;
                SA_PADA.IsChecked = RST.SA_PADA;
                SA_HOLA.IsChecked = RST.SA_HOLA;
                SA_KHAR.IsChecked = RST.SA_KHAR;
                SA_NAHA.IsChecked = RST.SA_NAHA;
                SA_JAZB.IsChecked = RST.SA_JAZB;
                SA_RAND.IsChecked = RST.SA_RAND;
                SA_COND.IsChecked = RST.SA_COND;
                SA_SAYE.IsChecked = RST.SA_SAYE;
                SA_23BI.IsChecked = RST.SA_23BI;

                SetGrpOption(GRP_FINALS, Convert.ToInt32(RST.FINALS));
                #endregion

                #region SMS_TAB
                SMS_USERNAME.Text = RST?.SMS_USERNAME;
                SMS_PASSWORD.Password = RST?.SMS_PASSWORD;
                SMS_TSMSHOST.Text = RST?.SMS_TSMSHOST; //Line Number
                SMS_LIBKEY.Password = RST?.SMS_LIBKEY; //Api Key

                SMS_OWNER.Text = RST?.SMS_OWNER;

                DSMS.IsChecked = (RST?.DSMS ?? false);
                PRMFR.IsChecked = (RST?.PRMFR ?? false);
                SMSACT.IsChecked = (RST?.SMSACT ?? false);

                switch (RST?.SMSTYPE)
                {
                    case "TSMS":
                        RB_TUBA.IsChecked = true;
                        break;

                    case "SMSIR":
                        RB_SMSIR.IsChecked = true;
                        break;

                    default: break;
                }
                #endregion

                #region ISO_TAB
                ISO_FROOSH.Text = RST.ISO_FROOSH;
                ISO_KHAREED.Text = RST.ISO_KHAREED;
                ISO_MAVAD.Text = RST.ISO_MAVAD;
                ISO_TOLID.Text = RST.ISO_TOLID;
                ISO_MAVADSAYER.Text = RST.ISO_MAVADSAYER;
                ISO_DTOLID.Text = RST.ISO_DTOLID;
                #endregion

                #region CRM_SETTING_TAB
                // عناوین آیتم‌ها
                IT1.Text = RST.IT1; IT2.Text = RST.IT2; IT3.Text = RST.IT3;
                IT4.Text = RST.IT4; IT5.Text = RST.IT5; IT6.Text = RST.IT6;
                IT7.Text = RST.IT7; IT8.Text = RST.IT8; IT9.Text = RST.IT9;
                // عناوین ستون‌ها
                IS1.Text = RST.IS1; IS2.Text = RST.IS2; IS3.Text = RST.IS3;
                IS4.Text = RST.IS4; IS5.Text = RST.IS5; IS6.Text = RST.IS6;
                IS7.Text = RST.IS7; IS8.Text = RST.IS8; IS9.Text = RST.IS9;
                IS10.Text = RST.IS10; IS11.Text = RST.IS11; IS12.Text = RST.IS12;
                IS14.Text = RST.IS14; IS15.Text = RST.IS15; IS16.Text = RST.IS16;
                #endregion

                #region SSM
                SSMTRTAKM.Text = RST.SSMTRTAKM?.ToString();
                SSMTRTAGM.Text = RST.SSMTRTAGM?.ToString();
                SSMSNDAUTO.IsChecked = RST.SSMSNDAUTO.HasValue && RST.SSMSNDAUTO != 0;
                SSMTBMON.Text = RST.SSMTBMON?.ToString();
                SSMDARSAD.Text = RST.SSMDARSAD?.ToString();
                HDARKASRTAKHF.SelectedValue = RST.HDARKASRTAKHF;
                #endregion

                #region MOADIAN
                PRIVIATEKEY.Text = RST.PRIVIATEKEY;
                Dcertificate.Text = RST.Dcertificate;
                MEMORYID.Text = RST.MEMORYID;
                MEMORYIDsand.Text = RST.MEMORYIDsand;
                #endregion
            }

            FILL_ALL_COMBOBOXES();

            Form_Current();
        }

        private void Form_Current()
        {
            AllowEdits = false;
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
            //پیش فرض انبار
            DEFANB.ItemsSource = dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT CODE, NAMES FROM TCOD_ANBAR").ToList();

            //پیش فرض نوع مشتری
            DEFTKH.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();

            //تب حقوق حساب های حقوق دستمزد:
            var hesabList = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
            var comboBoxes = new[] { HAZEDAR, HAZTOLID, HAZFROOSH, HAZKHADAMAT, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, PSANDHES, HDARKASRTAKHF };
            foreach (var cmb in comboBoxes)
            {
                cmb.ItemsSource = hesabList;
            }
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

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled || !AllowEdits) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از ذخیره تغیرات مطمئن هستید؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult == false) { return; }

            try
            {
                #region GENERAL
                {
                    {
                        var parameters = new
                        {
                            EMZA = _uploadedImageData,
                            WIDTH_D = WIDTH_D.Text.Trim(),
                            HIGH_D = HIGH_D.Text,
                            BACKPATH = BACKPATH.Text.Trim(),
                            TFADDRESS = TFADDRESS.Text.Trim(),
                            TFTEL = TFTEL.Text.Trim(),
                            SERVERNAM = SERVERNAM.Text.Trim(),
                        };
                        string sql = "UPDATE [dbo].[SAZMAN] SET [EMZA] = @EMZA," +
                            "WIDTH_D = @WIDTH_D," +
                            "HIGH_D = @HIGH_D," +
                            "BACKPATH = @BACKPATH," +
                            "TFADDRESS = @TFADDRESS," +
                            "TFTEL = @TFTEL," +
                            "SERVERNAM = @SERVERNAM";
                        dbms.DoExecuteSQL(sql, parameters);
                    }

                }
                #endregion

                #region ASNAD_VA_HESAB
                {
                    int? selectedGrpPerson = GetSelectedGrpOption(GRP_PERSON);
                    int? selectedGrpTkhf = GetSelectedGrpOption(GRP_TKHF);
                    int? selectedGrpSanad = GetSelectedGrpOption(GRP_SANAD);
                    bool sign = SIGN.IsChecked ?? false;
                    bool ctlDt = CTL_DT.IsChecked ?? false;
                    bool sndkh = SNDKH.IsChecked ?? false;
                    bool saghf = SAGHF.IsChecked ?? false;
                    bool saghf2 = SAGHF2.IsChecked ?? false;
                    float.TryParse(CPI.Text, out float cpiValue);
                    byte.TryParse(ARSESH.Text, out byte arseshValue);
                    float.TryParse(TFTPAGE.Text, out float tftpageValue);
                    var parameters = new
                    {
                        GrpPerson = selectedGrpPerson,
                        GrpTkhf = selectedGrpTkhf,
                        GrpSanad = selectedGrpSanad,
                        Sign = sign,
                        CtlDt = ctlDt,
                        Sndkh = sndkh,
                        Cpi = cpiValue,
                        Arsesh = arseshValue,
                        Tftpage = tftpageValue,
                        Saghf = saghf,
                        Saghf2 = saghf2,
                    };
                    string sql = "UPDATE dbo.SAZMAN SET " +
                                 "PERSON = @GrpPerson, " +
                                 "TKHF = @GrpTkhf, " +
                                 "SANAD = @GrpSanad, " +
                                 "SIGN = @Sign, " +
                                 "CTL_DT = @CtlDt, " +
                                 "SNDKH = @Sndkh, " +
                                 "CPI = @Cpi, " +
                                 "ARSESH = @Arsesh, " +
                                 "TFTPAGE = @Tftpage, " +
                                 "SAGHF = @Saghf, " +
                                 "SAGHF2 = @Saghf2 ";
                    var result = dbms.DoExecuteSQL(sql, parameters);
                }
                #endregion

                #region FACTOR_VA_ANBAR
                var asnadParameters = new
                {
                    GHAYM = GetSelectedGrpOption(GRP_GHAYM),
                    KALA = GetSelectedGrpOption(GRP_KALA),
                    FRUP = GetSelectedGrpOption(GRP_FRUP),
                    WAR = GetSelectedGrpOption(GRP_WAR),
                    UPDDATE = GetSelectedGrpOption(GRP_UPDDATE),
                    LST = GetSelectedGrpOption(GRP_LST),
                    MOJU = MOJU.IsChecked,
                    RMOG = RMOG.IsChecked,
                    DIG = int.TryParse(DIG.Text, out int digVal) ? digVal : (int?)null,
                    TFCODE_E = TFCODE_E.Text,
                    CODEVIEW = CODEVIEW.IsChecked,
                    MAND = MAND.IsChecked,
                    SERFACB = SERFACB.IsChecked,
                    LOCKFAP = LOCKFAP.IsChecked,
                    DEFANB = (int?)DEFANB.SelectedValue,
                    DEFTKH = (int?)DEFTKH.SelectedValue,
                    TRANSF = TRANSF.IsChecked,
                };
                string asnadSql = @"UPDATE SAZMAN SET 
                                GHAYM = @GHAYM, KALA = @KALA, FRUP = @FRUP, WAR = @WAR, UPDDATE = @UPDDATE, LST = @LST,
                                MOJU = @MOJU, RMOG = @RMOG, DIG = @DIG, TFCODE_E = @TFCODE_E, CODEVIEW = @CODEVIEW,
                                MAND = @MAND, SERFACB = @SERFACB, LOCKFAP = @LOCKFAP, DEFANB = @DEFANB,
                                DEFTKH = @DEFTKH, TRANSF = @TRANSF";
                dbms.DoExecuteSQL(asnadSql, asnadParameters);
                #endregion

                #region SALARY
                var payrollParameters = new
                {
                    HAZEDAR = HAZEDAR.SelectedValue,
                    HAZTOLID = HAZTOLID.SelectedValue,
                    HAZFROOSH = HAZFROOSH.SelectedValue,
                    HAZKHADAMAT = HAZKHADAMAT.SelectedValue,
                    EDABIM = EDABIM.SelectedValue,
                    HAZBIM = HAZBIM.SelectedValue,
                    BESHO = BESHO.SelectedValue,
                    BEDMOS = BEDMOS.SelectedValue,
                    PARDAKH = PARDAKH.SelectedValue,
                    HAZMALI = HAZMALI.SelectedValue,
                    PSANDHES = PSANDHES.SelectedValue,
                    SANAVP = int.TryParse(SANAVP.Text, out int sanavpVal) ? sanavpVal : (int?)null,
                    SAGHFH = int.TryParse(SAGHFH.Text, out int saghfhVal) ? saghfhVal : (int?)null,
                    HEZA = HEZA.IsChecked,
                    HPAD = HPAD.IsChecked,
                    HOLA = HOLA.IsChecked,
                    HKHA = HKHA.IsChecked,
                    HNAH = HNAH.IsChecked,
                    HJAZ = HJAZ.IsChecked,
                    HRAN = HRAN.IsChecked,
                    HCON = HCON.IsChecked,
                    HSAY = HSAY.IsChecked,
                    HSHI = HSHI.IsChecked,
                    HBON = HBON.IsChecked,
                    HTAHOL = HTAHOL.IsChecked,
                };
                string payrollSql = @"UPDATE SAZMAN SET 
                    HAZEDAR=@HAZEDAR, HAZTOLID=@HAZTOLID, HAZFROOSH=@HAZFROOSH, HAZKHADAMAT=@HAZKHADAMAT,
                    EDABIM=@EDABIM, HAZBIM=@HAZBIM, BESHO=@BESHO, BEDMOS=@BEDMOS, PARDAKH=@PARDAKH,
                    HAZMALI=@HAZMALI, PSANDHES=@PSANDHES, SANAVP=@SANAVP, SAGHFH=@SAGHFH, HEZA=@HEZA,
                    HPAD=@HPAD, HOLA=@HOLA, HKHA=@HKHA, HNAH=@HNAH, HJAZ=@HJAZ, HRAN=@HRAN,
                    HCON=@HCON, HSAY=@HSAY, HSHI=@HSHI, HBON=@HBON, HTAHOL=@HTAHOL";
                dbms.DoExecuteSQL(payrollSql, payrollParameters);
                #endregion

                #region SANATI
                var sanatiParameters = new
                {
                    ECONM = ECONM.IsChecked ?? false,
                    SANAT = SANAT.IsChecked ?? false,
                    STFR = int.TryParse(STFR.Text, out int stfr) ? stfr : (int?)null,
                    STKH = int.TryParse(STKH.Text, out int stkh) ? stkh : (int?)null,
                    STHFR = int.TryParse(STHFR.Text, out int sthfr) ? sthfr : (int?)null,
                    STHKH = int.TryParse(STHKH.Text, out int sthkh) ? sthkh : (int?)null,
                    STENT = int.TryParse(STENT.Text, out int stent) ? stent : (int?)null,
                    STKHS = int.TryParse(STKHS.Text, out int stkhs) ? stkhs : (int?)null,
                    STKHH = int.TryParse(STKHH.Text, out int stkhh) ? stkhh : (int?)null,
                    STTOL = int.TryParse(STTOL.Text, out int sttol) ? sttol : (int?)null,
                    STFRB = int.TryParse(STFRB.Text, out int stfrb) ? stfrb : (int?)null,
                    STBKH = int.TryParse(STBKH.Text, out int stbkh) ? stbkh : (int?)null,
                    STMO = int.TryParse(STMO.Text, out int stmo) ? stmo : (int?)null,
                    STKHA = int.TryParse(STKHA.Text, out int stkha) ? stkha : (int?)null,
                    SA_HOGH = SA_HOGH.IsChecked ?? false,
                    SA_40EZ = SA_40EZ.IsChecked ?? false,
                    SA_EZAF = SA_EZAF.IsChecked ?? false,
                    SA_PADA = SA_PADA.IsChecked ?? false,
                    SA_HOLA = SA_HOLA.IsChecked ?? false,
                    SA_KHAR = SA_KHAR.IsChecked ?? false,
                    SA_NAHA = SA_NAHA.IsChecked ?? false,
                    SA_JAZB = SA_JAZB.IsChecked ?? false,
                    SA_RAND = SA_RAND.IsChecked ?? false,
                    SA_COND = SA_COND.IsChecked ?? false,
                    SA_SAYE = SA_SAYE.IsChecked ?? false,
                    SA_23BI = SA_23BI.IsChecked ?? false,
                    FINALS = GetSelectedGrpOption(GRP_FINALS)
                };
                string sanatiSql = @"UPDATE SAZMAN SET 
                    ECONM=@ECONM, SANAT=@SANAT, STFR=@STFR, STKH=@STKH, STHFR=@STHFR, STHKH=@STHKH,
                    STENT=@STENT, STKHS=@STKHS, STKHH=@STKHH, STTOL=@STTOL, STFRB=@STFRB, STBKH=@STBKH,
                    STMO=@STMO, STKHA=@STKHA, SA_HOGH=@SA_HOGH, SA_40EZ=@SA_40EZ, SA_EZAF=@SA_EZAF,
                    SA_PADA=@SA_PADA, SA_HOLA=@SA_HOLA, SA_KHAR=@SA_KHAR, SA_NAHA=@SA_NAHA, SA_JAZB=@SA_JAZB,
                    SA_RAND=@SA_RAND, SA_COND=@SA_COND, SA_SAYE=@SA_SAYE, SA_23BI=@SA_23BI, FINALS=@FINALS";
                dbms.DoExecuteSQL(sanatiSql, sanatiParameters);
                #endregion

                #region SMS
                {
                    string _SMS_TYPE_ = "TSMS";
                    if ((bool)RB_TUBA.IsChecked)
                    {
                        _SMS_TYPE_ = "TSMS";
                        SMSPINFO.SERVICE_TYPE = SmsServiceType.TsmsUrl;
                    }
                    else if ((bool)RB_SMSIR.IsChecked)
                    {
                        _SMS_TYPE_ = "SMSIR";
                        SMSPINFO.SERVICE_TYPE = SmsServiceType.SmsIr;
                    }

                    string sql = @"
                UPDATE dbo.SAZMAN 
                SET 
                    SMSTYPE = @smsType,
                    SMSACT = @smsAct,
                    PRMFR = @prmfr,
                    DSMS = @dsms,
                    SMS_USERNAME = @smsUsername,
                    SMS_PASSWORD = @smsPassword,
                    SMS_TSMSHOST = @smsTsmshost,
                    SMS_LIBKEY = @smsLibkey,
                    SMS_OWNER = @smsOwner";

                    var parameters = new
                    {
                        smsType = _SMS_TYPE_,
                        smsAct = Convert.ToByte(SMSACT.IsChecked),
                        prmfr = Convert.ToByte(PRMFR.IsChecked),
                        dsms = Convert.ToByte(DSMS.IsChecked),
                        smsUsername = SMS_USERNAME.Text,
                        smsPassword = SMS_PASSWORD.Password,
                        smsTsmshost = SMS_TSMSHOST.Text,
                        smsLibkey = SMS_LIBKEY.Password,
                        smsOwner = SMS_OWNER.Text
                    };

                    dbms.DoExecuteSQL(sql, parameters);

                    SMSPINFO.USERNAME = SMS_USERNAME.Text;
                    SMSPINFO.PASSWORD = SMS_PASSWORD.Password;
                    if (!string.IsNullOrWhiteSpace(SMS_TSMSHOST.Text))
                    {
                        SMSPINFO.LINE_NUMBER = Convert.ToInt64(SMS_TSMSHOST.Text);
                    }

                    if ((bool)RB_SMSIR.IsChecked)
                    {
                        SMSPINFO.API_KEY = SMS_LIBKEY.Password;
                    }
                }
                #endregion

                #region ISO
                var isoParameters = new
                {
                    ISO_FROOSH = ISO_FROOSH.Text,
                    ISO_KHAREED = ISO_KHAREED.Text,
                    ISO_MAVAD = ISO_MAVAD.Text,
                    ISO_TOLID = ISO_TOLID.Text,
                    ISO_MAVADSAYER = ISO_MAVADSAYER.Text,
                    ISO_DTOLID = ISO_DTOLID.Text
                };
                string isoSql = @"UPDATE SAZMAN SET 
                    ISO_FROOSH = @ISO_FROOSH, 
                    ISO_KHAREED = @ISO_KHAREED, 
                    ISO_MAVAD = @ISO_MAVAD, 
                    ISO_TOLID = @ISO_TOLID, 
                    ISO_MAVADSAYER = @ISO_MAVADSAYER, 
                    ISO_DTOLID = @ISO_DTOLID";
                dbms.DoExecuteSQL(isoSql, isoParameters);
                #endregion

                #region CRM_SETTING
                {
                    var parameters = new
                    {
                        IT1 = IT1.Text,
                        IT2 = IT2.Text,
                        IT3 = IT3.Text,
                        IT4 = IT4.Text,
                        IT5 = IT5.Text,
                        IT6 = IT6.Text,
                        IT7 = IT7.Text,
                        IT8 = IT8.Text,
                        IT9 = IT9.Text,
                        IS1 = IS1.Text,
                        IS2 = IS2.Text,
                        IS3 = IS3.Text,
                        IS4 = IS4.Text,
                        IS5 = IS5.Text,
                        IS6 = IS6.Text,
                        IS7 = IS7.Text,
                        IS8 = IS8.Text,
                        IS9 = IS9.Text,
                        IS10 = IS10.Text,
                        IS11 = IS11.Text,
                        IS12 = IS12.Text,
                        IS14 = IS14.Text,
                        IS15 = IS15.Text,
                        IS16 = IS16.Text,
                    };
                    string sql = @"UPDATE SAZMAN SET 
                                 IT1=@IT1, IT2=@IT2, IT3=@IT3, IT4=@IT4, IT5=@IT5, IT6=@IT6, IT7=@IT7, IT8=@IT8, IT9=@IT9,
                                 IS1=@IS1, IS2=@IS2, IS3=@IS3, IS4=@IS4, IS5=@IS5, IS6=@IS6, IS7=@IS7, IS8=@IS8, IS9=@IS9,
                                 IS10=@IS10, IS11=@IS11, IS12=@IS12, IS14=@IS14, IS15=@IS15, IS16=@IS16";
                    dbms.DoExecuteSQL(sql, parameters);
                }
                #endregion

                #region SSM
                var ssmParameters = new
                {
                    SSMTRTAKM = int.TryParse(SSMTRTAKM.Text, out int ssmtrtakm) ? ssmtrtakm : (int?)null,
                    SSMTRTAGM = int.TryParse(SSMTRTAGM.Text, out int ssmtrtagm) ? ssmtrtagm : (int?)null,
                    SSMSNDAUTO = (SSMSNDAUTO.IsChecked ?? false) ? 1 : 0,
                    SSMTBMON = int.TryParse(SSMTBMON.Text, out int ssmtbmon) ? ssmtbmon : (int?)null,
                    SSMDARSAD = float.TryParse(SSMDARSAD.Text, out float ssmdarsad) ? ssmdarsad : (float?)null,
                    HDARKASRTAKHF = HDARKASRTAKHF.SelectedValue
                };
                string ssmSql = @"UPDATE SAZMAN SET 
                    SSMTRTAKM=@SSMTRTAKM, SSMTRTAGM=@SSMTRTAGM, SSMSNDAUTO=@SSMSNDAUTO,
                    SSMTBMON=@SSMTBMON, SSMDARSAD=@SSMDARSAD, HDARKASRTAKHF=@HDARKASRTAKHF";
                dbms.DoExecuteSQL(ssmSql, ssmParameters);
                #endregion

                #region MOADIAN
                var moadianparameters = new
                {
                    PRIVIATEKEY = PRIVIATEKEY.Text,
                    Dcertificate = Dcertificate.Text,
                    MEMORYID = MEMORYID.Text,
                    MEMORYIDsand = MEMORYIDsand.Text,
                };
                string sqlmoadian = @"UPDATE SAZMAN SET 
                                 PRIVIATEKEY = @PRIVIATEKEY, 
                                 Dcertificate = @Dcertificate, 
                                 MEMORYID = @MEMORYID, MEMORYIDsand = @MEMORYIDsand";
                dbms.DoExecuteSQL(sqlmoadian, moadianparameters);
                #endregion


                try { Baseknow.GetInitTheApp(); } catch { } //Refresh Baseknow Data
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا هنگام ذخیره‌سازی").ShowDialog();
                return;
            }


            universControl.PopNotifyShowUp("." + "ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            ChangeIsHappend = false;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {

        }

        private int? GetSelectedGrpOption(GroupBox Grb)
        {
            //var selectedRadio = Grb.Content is UniformGrid grid
            //    ? grid.Children.OfType<RadioButton>()
            //        .FirstOrDefault(rb => rb.IsChecked == true)
            //    : null;

            //if (selectedRadio != null && selectedRadio.Tag != null)
            //{
            //    return Convert.ToInt32(selectedRadio.Tag);
            //}

            var panel = Grb.Content as Panel;
            if (panel == null) return null;

            var selectedRadio = panel.Children.OfType<RadioButton>()
                                       .FirstOrDefault(rb => rb.IsChecked == true);

            return selectedRadio?.Tag != null ? Convert.ToInt32(selectedRadio.Tag) : (int?)null;
        }
        private void SetGrpOption(GroupBox Grb, int tagValue)
        {
            //if (Grb.Content is UniformGrid grid)
            //{
            //    var radioToCheck = grid.Children.OfType<RadioButton>()
            //        .FirstOrDefault(rb => rb.Tag != null &&
            //            Convert.ToInt32(rb.Tag) == tagValue);

            //    if (radioToCheck != null)
            //    {
            //        radioToCheck.IsChecked = true;
            //    }
            //}

            // پیدا کردن StackPanel یا UniformGrid داخل GroupBox
            var panel = Grb.Content as Panel;
            if (panel == null) return;

            var radioButton = panel.Children.OfType<RadioButton>()
                                   .FirstOrDefault(rb => rb.Tag != null && Convert.ToInt32(rb.Tag) == tagValue);

            if (radioButton != null)
            {
                radioButton.IsChecked = true;
            }
        }

        private void BTN_SMSFORMAT_Click(object sender, RoutedEventArgs e)
        {
            var ThereOpenIsWindowBefore = Application.Current.Windows.OfType<SMS_FORMAT>().Any();
            if (ThereOpenIsWindowBefore)
            {
                new Msgwin(false, "این پنجره از قبل هنوز باز است").ShowDialog();
                return;
            }

            new SMS_FORMAT().Show();
        }
        private void CREATE_TR_SAZAMN()
        {
            dbms.DoExecuteSQL(@"CREATE TABLE [dbo].[TR_SAZMAN](
                                	[UP_DATE] [BIGINT] NULL,
                                	[UP_TIME] [FLOAT] NULL,
                                	[UP_USER_NAME] [NVARCHAR](40) NULL,
                                	[PC_NAME] [NVARCHAR](50) NULL,
                                	[IPADD] [NVARCHAR](50) NULL,
                                	[UNIVERSITY_CO] [FLOAT] NULL,
                                	[NAME] [NVARCHAR](50) NULL,
                                	[CITY] [NVARCHAR](20) NULL,
                                	[MANAGER] [NVARCHAR](40) NULL,
                                	[MOAVEN] [NVARCHAR](40) NULL,
                                	[ZIHESAB] [NVARCHAR](40) NULL,
                                	[AMINAMVAL] [NVARCHAR](40) NULL,
                                	[YEA] [SMALLINT] NULL,
                                	[SANAD] [SMALLINT] NULL,
                                	[GHAYM] [SMALLINT] NULL,
                                	[KALA] [SMALLINT] NULL,
                                	[PERSON] [SMALLINT] NULL,
                                	[DIG] [FLOAT] NULL,
                                	[WAR] [FLOAT] NULL,
                                	[LST] [FLOAT] NULL,
                                	[TFTPAGE] [FLOAT] NULL,
                                	[TFSAZMAN] [NVARCHAR](40) NULL,
                                	[TFADDRESS] [NVARCHAR](150) NULL,
                                	[TFTEL] [NVARCHAR](60) NULL,
                                	[TFCODE_E] [NVARCHAR](13) NULL,
                                	[WIDTH_D] [NVARCHAR](200) NULL,
                                	[HIGH_D] [NVARCHAR](400) NULL,
                                	[CPI] [FLOAT] NULL,
                                	[SANDOGH] [FLOAT] NULL,
                                	[BANKHA] [FLOAT] NULL,
                                	[BESTANKAR] [FLOAT] NULL,
                                	[BEDEHKAR] [FLOAT] NULL,
                                	[KHARID] [FLOAT] NULL,
                                	[MKHARID] [FLOAT] NULL,
                                	[TKHARID] [FLOAT] NULL,
                                	[HKHARID] [FLOAT] NULL,
                                	[FROSH] [FLOAT] NULL,
                                	[MFROSH] [FLOAT] NULL,
                                	[TFROSH] [FLOAT] NULL,
                                	[HFROSH] [FLOAT] NULL,
                                	[MOGODIA] [FLOAT] NULL,
                                	[MOGODIP] [FLOAT] NULL,
                                	[DARAM] [FLOAT] NULL,
                                	[HDARAM] [FLOAT] NULL,
                                	[HKOL] [FLOAT] NULL,
                                	[ADA] [NVARCHAR](20) NULL,
                                	[APA] [NVARCHAR](20) NULL,
                                	[ADV] [NVARCHAR](20) NULL,
                                	[HAVALAH] [FLOAT] NULL,
                                	[CTRL_TS] [FLOAT] NULL,
                                	[F_ANBARF] [FLOAT] NULL,
                                	[GH_PK] [FLOAT] NULL,
                                	[L_NUMBER] [FLOAT] NULL,
                                	[SF_G] [FLOAT] NULL,
                                	[TAR_KM] [FLOAT] NULL,
                                	[BACKPATH] [NVARCHAR](50) NULL,
                                	[TKHF] [INT] NULL,
                                	[HAZ_TOL] [FLOAT] NULL,
                                	[PJHAZ_TOL1] [FLOAT] NULL,
                                	[PHAZ_TOL] [FLOAT] NULL,
                                	[GHEYMAT] [FLOAT] NULL,
                                	[PPDAST] [FLOAT] NULL,
                                	[PPSAR] [FLOAT] NULL,
                                	[AMALKARD] [FLOAT] NULL,
                                	[PERSONEL] [NVARCHAR](40) NULL,
                                	[PERVAM] [NVARCHAR](40) NULL,
                                	[CONKAL] [FLOAT] NULL,
                                	[EMZA] [IMAGE] NULL,
                                	[HNAH] [BIT] NOT NULL,
                                	[HEZA] [BIT] NOT NULL,
                                	[HPAD] [BIT] NOT NULL,
                                	[HOLA] [BIT] NOT NULL,
                                	[HKHA] [BIT] NOT NULL,
                                	[HJAZ] [BIT] NOT NULL,
                                	[HRAN] [BIT] NOT NULL,
                                	[HSAY] [BIT] NOT NULL,
                                	[HCON] [BIT] NOT NULL,
                                	[HSHI] [BIT] NOT NULL,
                                	[HAZEDAR] [NVARCHAR](20) NULL,
                                	[EDABIM] [NVARCHAR](20) NULL,
                                	[HAZBIM] [NVARCHAR](20) NULL,
                                	[BESHO] [NVARCHAR](20) NULL,
                                	[BEDMOS] [NVARCHAR](20) NULL,
                                	[PARDAKH] [NVARCHAR](20) NULL,
                                	[HAZMALI] [NVARCHAR](20) NULL,
                                	[SAGHFH] [INT] NULL,
                                	[MAND] [BIT] NOT NULL,
                                	[MOJU] [BIT] NOT NULL,
                                	[SA_HOGH] [BIT] NOT NULL,
                                	[SA_40EZ] [BIT] NOT NULL,
                                	[SA_EZAF] [BIT] NOT NULL,
                                	[SA_PADA] [BIT] NOT NULL,
                                	[SA_HOLA] [BIT] NOT NULL,
                                	[SA_KHAR] [BIT] NOT NULL,
                                	[SA_NAHA] [BIT] NOT NULL,
                                	[SA_JAZB] [BIT] NOT NULL,
                                	[SA_RAND] [BIT] NOT NULL,
                                	[SA_COND] [BIT] NOT NULL,
                                	[SA_SAYE] [BIT] NOT NULL,
                                	[SA_23BI] [BIT] NOT NULL,
                                	[HAZTOLID] [NVARCHAR](20) NULL,
                                	[HAZFROOSH] [NVARCHAR](20) NULL,
                                	[HAZKHADAMAT] [NVARCHAR](20) NULL,
                                	[PISHDAR] [FLOAT] NULL,
                                	[DEFANB] [SMALLINT] NULL,
                                	[DEFTKH] [SMALLINT] NULL,
                                	[ECONM] [BIT] NULL,
                                	[FRUP] [BIT] NULL,
                                	[UPDDATE] [BIT] NULL,
                                	[FINALS] [BIT] NULL,
                                	[PSANDHES] [NVARCHAR](20) NULL,
                                	[SANAVP] [INT] NULL,
                                	[BON] [INT] NULL,
                                	[ISO_FROOSH] [NVARCHAR](20) NULL,
                                	[ISO_KHAREED] [NVARCHAR](20) NULL,
                                	[ISO_MAVAD] [NVARCHAR](20) NULL,
                                	[ISO_TOLID] [NVARCHAR](20) NULL,
                                	[ISO_MAVADSAYER] [NVARCHAR](20) NULL,
                                	[SANAT] [BIT] NULL,
                                	[CODEVIEW] [SMALLINT] NULL,
                                	[PKHARID] [INT] NULL,
                                	[SIGN] [BIT] NULL,
                                	[BARCOD] [BIT] NULL,
                                	[SAGHF] [BIT] NULL,
                                	[SERVERNAM] [NVARCHAR](50) NULL,
                                	[TENDAR] [BIT] NULL,
                                	[LECOL1] [NVARCHAR](50) NULL,
                                	[LECOL2] [NVARCHAR](50) NULL,
                                	[LECOL3] [NVARCHAR](50) NULL,
                                	[LECOL4] [NVARCHAR](50) NULL,
                                	[LKCOL1] [NVARCHAR](50) NULL,
                                	[HESMBAA] [NVARCHAR](20) NULL,
                                	[ECODE] [NVARCHAR](20) NULL,
                                	[PCODE] [NVARCHAR](10) NULL,
                                	[IYALAT] [NVARCHAR](20) NULL,
                                	[MCODEM] [NVARCHAR](20) NULL,
                                	[HPOR] [NVARCHAR](20) NULL,
                                	[SAGHF2] [BIT] NULL,
                                	[OPTIONSS] [NVARCHAR](100) NULL,
                                	[CTL_DT] [BIT] NULL,
                                	[LOCKFAP] [BIT] NULL,
                                	[LOCKFSI] [BIT] NULL,
                                	[TRANSF] [BIT] NULL,
                                	[OKF] [BIT] NULL,
                                	[ARSESH] [TINYINT] NULL,
                                	[RMOG] [BIT] NULL,
                                	[APV] [NVARCHAR](20) NULL,
                                	[HOTCOD] [NVARCHAR](8) NULL,
                                	[STFR] [INT] NULL,
                                	[STKH] [INT] NULL,
                                	[STHFR] [INT] NULL,
                                	[STHKH] [INT] NULL,
                                	[STENT] [INT] NULL,
                                	[STKHS] [INT] NULL,
                                	[STKHH] [INT] NULL,
                                	[STTOL] [INT] NULL,
                                	[STFRB] [INT] NULL,
                                	[STBKH] [INT] NULL,
                                	[STMO] [INT] NULL,
                                	[STKHA] [INT] NULL,
                                	[SNDKH] [BIT] NULL,
                                	[IDD] [INT] IDENTITY(1,1) NOT NULL,
                                	[SMS_USERNAME] [NVARCHAR](50) NULL,
                                	[SMS_PASSWORD] [NVARCHAR](50) NULL,
                                	[SMS_LIBKEY] [NVARCHAR](150) NULL,
                                	[SMS_TSMSHOST] [NVARCHAR](50) NULL,
                                	[SMS_ProxyUserName] [NVARCHAR](50) NULL,
                                	[SMS_ProxyPassword] [NVARCHAR](50) NULL,
                                	[SMS_ProxyServer] [NVARCHAR](50) NULL,
                                	[SMS_ProxyPort] [NVARCHAR](50) NULL,
                                	[SMS_FirewallUserName] [NVARCHAR](50) NULL,
                                	[SMS_FirewallPassword] [NVARCHAR](50) NULL,
                                	[SMS_FirewallHost] [NVARCHAR](50) NULL,
                                	[SMS_FirewallPort] [NVARCHAR](50) NULL,
                                	[SMS_FirewallType] [NVARCHAR](50) NULL,
                                	[DSMS] [BIT] NULL,
                                	[SMS_OWNER] [NVARCHAR](200) NULL,
                                	[PRMFR] [BIT] NULL,
                                	[HESDESK] [NVARCHAR](20) NULL,
                                	[ISO_DTOLID] [NVARCHAR](20) NULL,
                                	[SERFACB] [BIT] NULL,
                                	[HBON] [BIT] NULL,
                                	[CRT] [DATETIME] NULL,
                                	[UID] [INT] NULL,
                                 CONSTRAINT [PK_TR_SAZMAN] PRIMARY KEY CLUSTERED 
                                (
                                	[IDD] ASC
                                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
                                ALTER TABLE [dbo].[TR_SAZMAN] ADD  DEFAULT (1) FOR [HBON]
                                ALTER TABLE [dbo].[TR_SAZMAN] ADD  DEFAULT (GETDATE()) FOR [CRT]");
        }
        private void BTN_ESLAH_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            int exists = dbms.DoGetDataSQL<int>(@"SELECT CASE WHEN OBJECT_ID('dbo.TR_SAZMAN', 'U') IS NOT NULL THEN 1 ELSE 0 END").FirstOrDefault();
            if (!Convert.ToBoolean(exists))
            {
                CREATE_TR_SAZAMN();
            }


            string sql = @"
                    INSERT INTO dbo.TR_SAZMAN (
                        UNIVERSITY_CO, NAME, CITY, MANAGER, MOAVEN, ZIHESAB, AMINAMVAL, YEA, SANAD, GHAYM, KALA, PERSON, DIG, WAR, LST, 
                        TFTPAGE, TFSAZMAN, TFADDRESS, TFTEL, TFCODE_E, WIDTH_D, HIGH_D, CPI, SANDOGH, BANKHA, BESTANKAR, BEDEHKAR, 
                        KHARID, MKHARID, TKHARID, HKHARID, FROSH, MFROSH, TFROSH, HFROSH, MOGODIA, MOGODIP, DARAM, HDARAM, HKOL, 
                        ADA, APA, ADV, HAVALAH, CTRL_TS, F_ANBARF, GH_PK, L_NUMBER, SF_G, TAR_KM, BACKPATH, TKHF, HAZ_TOL, PJHAZ_TOL1, 
                        PHAZ_TOL, GHEYMAT, PPDAST, PPSAR, AMALKARD, PERSONEL, PERVAM, CONKAL, EMZA, HNAH, HEZA, HPAD, HOLA, HKHA, HJAZ, 
                        HRAN, HSAY, HCON, HSHI, HAZEDAR, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, SAGHFH, MAND, MOJU, SA_HOGH, 
                        SA_40EZ, SA_EZAF, SA_PADA, SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, HAZTOLID, 
                        HAZFROOSH, HAZKHADAMAT, PISHDAR, DEFANB, DEFTKH, ECONM, FRUP, UPDDATE, FINALS, PSANDHES, SANAVP, BON, ISO_FROOSH, 
                        ISO_KHAREED, ISO_MAVAD, ISO_TOLID, ISO_MAVADSAYER, SANAT, CODEVIEW, PKHARID, SIGN, BARCOD, SAGHF, SERVERNAM, 
                        TENDAR, LECOL1, LECOL2, LECOL3, LECOL4, LKCOL1, HESMBAA, ECODE, PCODE, IYALAT, MCODEM, HPOR, SAGHF2, OPTIONSS, 
                        CTL_DT, LOCKFAP, LOCKFSI, TRANSF, OKF, ARSESH, RMOG, APV, HOTCOD, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, 
                        STTOL, STFRB, STBKH, STMO, STKHA, SNDKH, UP_DATE, UP_TIME, UP_USER_NAME, PC_NAME, IPADD
                    ) 
                    SELECT 
                        UNIVERSITY_CO, NAME, CITY, MANAGER, MOAVEN, ZIHESAB, AMINAMVAL, YEA, SANAD, GHAYM, KALA, PERSON, DIG, WAR, LST, 
                        TFTPAGE, TFSAZMAN, TFADDRESS, TFTEL, TFCODE_E, WIDTH_D, HIGH_D, CPI, SANDOGH, BANKHA, BESTANKAR, BEDEHKAR, 
                        KHARID, MKHARID, TKHARID, HKHARID, FROSH, MFROSH, TFROSH, HFROSH, MOGODIA, MOGODIP, DARAM, HDARAM, HKOL, 
                        ADA, APA, ADV, HAVALAH, CTRL_TS, F_ANBARF, GH_PK, L_NUMBER, SF_G, TAR_KM, BACKPATH, TKHF, HAZ_TOL, PJHAZ_TOL1, 
                        PHAZ_TOL, GHEYMAT, PPDAST, PPSAR, AMALKARD, PERSONEL, PERVAM, CONKAL, EMZA, HNAH, HEZA, HPAD, HOLA, HKHA, HJAZ, 
                        HRAN, HSAY, HCON, HSHI, HAZEDAR, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, SAGHFH, MAND, MOJU, SA_HOGH, 
                        SA_40EZ, SA_EZAF, SA_PADA, SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, HAZTOLID, 
                        HAZFROOSH, HAZKHADAMAT, PISHDAR, DEFANB, DEFTKH, ECONM, FRUP, UPDDATE, FINALS, PSANDHES, SANAVP, BON, ISO_FROOSH, 
                        ISO_KHAREED, ISO_MAVAD, ISO_TOLID, ISO_MAVADSAYER, SANAT, CODEVIEW, PKHARID, SIGN, BARCOD, SAGHF, SERVERNAM, 
                        TENDAR, LECOL1, LECOL2, LECOL3, LECOL4, LKCOL1, HESMBAA, ECODE, PCODE, IYALAT, MCODEM, HPOR, SAGHF2, OPTIONSS, 
                        CTL_DT, LOCKFAP, LOCKFSI, TRANSF, OKF, ARSESH, RMOG, APV, HOTCOD, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, 
                        STTOL, STFRB, STBKH, STMO, STKHA, SNDKH, @Expr2 AS Expr2, @Expr1 AS Expr1, @Expr3 AS EXPR3, @Expr4 AS EXPR4, @Expr5 AS EXPR5 
                    FROM dbo.SAZMAN";

            var parameters = new
            {
                Expr2 = CL_HESABDARI.FARSIDATE(), // Assuming FARSIDATE method formats Persian Date
                Expr1 = dt.ToOADate(), // Converts DateTime to OLE Automation format like VBA
                Expr3 = CL_HESABDARI.UCurrentUser(), // Logged-in user
                Expr4 = CL_HESABDARI.CurrentMachineName(), // Machine Name
                Expr5 = CL_HESABDARI.GETIPADD() // IP Address
            };

            dbms.DoExecuteSQL(sql, parameters);

            // Enable UI elements after execution
            AllowEdits = true;
            this.AllowDeletions = true;
            this.AllowEdits = true;

            BTN_SAVE.IsEnabled = true;
        }

        #region SMS
        private void RB_SMSIR_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(RB_SMSIR.IsChecked))
            {
                SMS_LIBKEY.IsEnabled = true;
            }

        }
        private void RB_TUBA_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(RB_TUBA.IsChecked))
            {
                SMS_LIBKEY.IsEnabled = false;
            }
        }
        #endregion

        #region GENERAL
        private void BTN_ESLAH_G_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SMS_PASSWORD_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
            }
        }

        private void BTN_UPIMAGE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "انتخاب تصویر",
                    Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                    FilterIndex = 1,
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {

                    // Validate image file size (maximum 10 MB)
                    if (!CL_LMethods.ValidateImageFile(openFileDialog.FileName, out string errorMessage, 3))
                    {
                        new Msgwin(false, errorMessage).ShowDialog();
                        return;
                    }

                    // Read the selected file
                    byte[] fileBytes = CL_LMethods.ConvertFileToByte(openFileDialog.FileName);

                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        new Msgwin(false, "خطا در خواندن فایل").ShowDialog();
                        return;
                    }


                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp")
                    {
                        try
                        {
                            // Display the uploaded image
                            BitmapImage bitmapSource = CL_LMethods.ByteArrayToBitmapImage(fileBytes);

                            // Optionally compress the image
                            byte[] compressedBytes = CL_LMethods.CompressImage(bitmapSource, 50);
                            _uploadedImageData = compressedBytes;

                            // Display the uploaded (or compressed) image
                            bitmapSource = CL_LMethods.ByteArrayToBitmapImage(compressedBytes);
                            EMZA_CANVAS.Strokes.Clear();
                            EMZA_CANVAS.Background = new ImageBrush(bitmapSource);

                            if (bitmapSource != null)
                            {
                                EMZA_CANVAS.Strokes.Clear();
                                ImageBrush brush = new ImageBrush(bitmapSource);
                                brush.Stretch = Stretch.Uniform;
                                EMZA_CANVAS.Background = brush;

                                // Disable drawing when showing an uploaded image
                                EMZA_CANVAS.EditingMode = InkCanvasEditingMode.None;

                                universControl.PopNotifyShowUp(
                                    "تصویر با موفقیت بارگذاری شد. برای ذخیره نهایی دکمه «ذخیره» را فشار دهید.",
                                    Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue);
                            }
                            else
                            {
                                new Msgwin(false, "فایل تصویر معتبر نیست یا قابل پردازش نمی‌باشد.").ShowDialog();
                            }
                        }
                        catch (Exception ex)
                        {
                            new Msgwin(false, $"خطا در پردازش تصویر: {ex.Message}").ShowDialog();
                        }
                    }
                    else
                    {
                        new Msgwin(false, "فایل انتخاب شده یک تصویر معتبر نیست. لطفاً یک تصویر با فرمت JPG، PNG یا BMP انتخاب کنید.").ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری تصویر").ShowDialog();
            }
        }
        private void BTN_MORE_Click(object sender, RoutedEventArgs e)
        {
            new WIN_OPTIONS().ShowDialog();
        }
        #endregion

        #region SANAD_VA_HESAB
        private void DAFT_ASN_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DAFT_ASN_SUB.Dispatcher.InvokeAsync(() =>
            {
                DAFT_ASN_SUB.CellEditEnding -= DAFT_ASN_SUB_CellEditEnding;
                DAFT_ASN_SUB.RowEditEnding -= DAFT_ASN_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DAFT_ASN_SUB.CancelEdit();
                }
                else
                {
                    DAFT_ASN_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DAFT_ASN_SUB.RowEditEnding += DAFT_ASN_SUB_RowEditEnding;
                DAFT_ASN_SUB.CellEditEnding += DAFT_ASN_SUB_CellEditEnding;
            });
        }
        private void DAFT_ASN_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!e.Row.IsNewItem)
            {
                e.Cancel = true;
                DAFT_ASN_SUB_CANCEL_EDIT();
            }
        }
        private void DAFT_ASN_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
        }
        private bool IsBookNumInUseAsync(int? bookNum)
        {
            if (bookNum == null) return false;

            string sql = "SELECT 1 FROM dbo.PAY_GETD WHERE ANBAR = @BookNum";
            var result = dbms.DoGetDataSQL<int>(sql, new { BookNum = bookNum });

            // If the query returns any rows, 'Any()' will be true.
            return result.Any();
        }

        private void DAFT_ASN_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var TheRow = e.Row.Item as DAFT_ASN;
            if (ConstructorRowDetector.IsPristine(TheRow)) { DAFT_ASN_SUB_CANCEL_EDIT(); return; }


            string sql = "INSERT INTO dbo.DAFT_ASN (BOOKNUM, FIRSTNUM, UID) VALUES (@BOOKNUM, @FIRSTNUM, @UID)";
            dbms.DoExecuteSQL(sql, new { TheRow.BOOKNUM, TheRow.FIRSTNUM, UID = Baseknow.USERCOD });

        }
        private void DAFT_ASN_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!DAFT_ASN_SUB.IsEnabled || DAFT_ASN_SUB.IsReadOnly) { return; }

            if (e.Key == Key.Delete)
            {
                var editableCollectionView = DAFT_ASN_SUB.Items as IEditableCollectionView;
                if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
                {
                    //to avoid any error because user might leave edited (not cimmitted) cell in DataGrid
                    try { editableCollectionView.CancelEdit(); } catch { }
                }

                if (DAFT_ASN_SUB.SelectedItem is DAFT_ASN itemToDelete)
                {
                    if (IsBookNumInUseAsync(itemToDelete.BOOKNUM))
                    {
                        new Msgwin(false, "این رکورد قابل حذف نیست زیرا شماره دفتر آن در اسناد استفاده شده است.").Show();
                        e.Handled = true;
                        return;
                    }

                    _ = AuditLogger.LogActionAsync(
                        actionType: "Delete",
                        tableName: "مشخصات سیستم : اسنادو حساب",
                        recordId: default,
                        oldValue: default,
                        newValue: itemToDelete,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    Msgwin msgwin = new Msgwin(true, "آیا از حذف این رکورد مطمئن هستید؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        string sql = "DELETE FROM dbo.DAFT_ASN WHERE BOOKNUM = @BOOKNUM";
                        dbms.DoExecuteSQL(sql, new { itemToDelete.BOOKNUM });
                        DAFT_ASN_DATA.Remove(itemToDelete); // Remove from the UI.
                    }
                    e.Handled = true;
                }
            }
        }
        #endregion

        private void BTN_TABLEMALIAT_Click(object sender, RoutedEventArgs e)
        {
            new WIN_TCOD_ZARMALIAT().ShowDialog();
        }

        private void BTN_MORE_SALARY_Click(object sender, RoutedEventArgs e)
        {
            new WIN_SAZMAN_2().ShowDialog();
        }
    }
}