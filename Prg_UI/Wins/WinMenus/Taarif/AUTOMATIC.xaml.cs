using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Syncfusion.XlsIO.Parser.Biff_Records.Charts.ChartAlrunsRecord;

namespace Wins.WinMenus.Taarif
{
    public partial class AUTOMATIC : Window
    {
        public ObservableCollection<TOTA_HES> TOTA_HES_SOURCE { get; set; } = new ObservableCollection<TOTA_HES>();
        public ObservableCollection<HESAB_PAIR_HES> HESAB_SOURCE { get; set; } = new ObservableCollection<HESAB_PAIR_HES>();
        public ObservableCollection<AMC1> HESAB_SOURCE2 { get; set; } = new ObservableCollection<AMC1>();
        public AUTOMATIC()
        {
            InitializeComponent();
        }
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
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();

        #region LOCALMODEL
        public class AMC1
        {
            public string? Expr1 { get; set; }
            public string? NAME { get; set; }
            public override string ToString() => Expr1;
        }
        #endregion

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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;

                SANDOGH.IsReadOnly = !_ican;
                SANDOGH.IsEnabled = _ican;
                SANDOGH2.IsReadOnly = !_ican;
                SANDOGH2.IsEnabled = _ican;
                BANKHA.IsReadOnly = !_ican;
                BANKHA.IsEnabled = _ican;
                BANKHA2.IsReadOnly = !_ican;
                BANKHA2.IsEnabled = _ican;
                BESTANKAR.IsReadOnly = !_ican;
                BESTANKAR.IsEnabled = _ican;
                BESTANKAR2.IsReadOnly = !_ican;
                BESTANKAR2.IsEnabled = _ican;
                BEDEHKAR.IsReadOnly = !_ican;
                BEDEHKAR.IsEnabled = _ican;
                BEDEHKAR2.IsReadOnly = !_ican;
                BEDEHKAR2.IsEnabled = _ican;
                KHARID.IsReadOnly = !_ican;
                KHARID.IsEnabled = _ican;
                KHARID2.IsReadOnly = !_ican;
                KHARID2.IsEnabled = _ican;
                MKHARID.IsReadOnly = !_ican;
                MKHARID.IsEnabled = _ican;
                MKHARID2.IsReadOnly = !_ican;
                MKHARID2.IsEnabled = _ican;
                TKHARID.IsReadOnly = !_ican;
                TKHARID.IsEnabled = _ican;
                TKHARID2.IsReadOnly = !_ican;
                TKHARID2.IsEnabled = _ican;
                HKHARID.IsReadOnly = !_ican;
                HKHARID.IsEnabled = _ican;
                HKHARID2.IsReadOnly = !_ican;
                HKHARID2.IsEnabled = _ican;
                FROSH.IsReadOnly = !_ican;
                FROSH.IsEnabled = _ican;
                FROSH2.IsReadOnly = !_ican;
                FROSH2.IsEnabled = _ican;
                MFROSH.IsReadOnly = !_ican;
                MFROSH.IsEnabled = _ican;
                MFROSH2.IsReadOnly = !_ican;
                MFROSH2.IsEnabled = _ican;
                TFROSH.IsReadOnly = !_ican;
                TFROSH.IsEnabled = _ican;
                TFROSH2.IsReadOnly = !_ican;
                TFROSH2.IsEnabled = _ican;
                HFROSH.IsReadOnly = !_ican;
                HFROSH.IsEnabled = _ican;
                HFROSH2.IsReadOnly = !_ican;
                HFROSH2.IsEnabled = _ican;
                PERSONEL.IsReadOnly = !_ican;
                PERSONEL.IsEnabled = _ican;
                PERVAM.IsReadOnly = !_ican;
                PERVAM.IsEnabled = _ican;
                MOGODIA.IsReadOnly = !_ican;
                MOGODIA.IsEnabled = _ican;
                MOGODIA2.IsReadOnly = !_ican;
                MOGODIA2.IsEnabled = _ican;
                DARAM.IsReadOnly = !_ican;
                DARAM.IsEnabled = _ican;
                DARAM2.IsReadOnly = !_ican;
                DARAM2.IsEnabled = _ican;
                HDARAM.IsReadOnly = !_ican;
                HDARAM.IsEnabled = _ican;
                HDARAM2.IsReadOnly = !_ican;
                HDARAM2.IsEnabled = _ican;
                ADA.IsReadOnly = !_ican;
                ADA.IsEnabled = _ican;
                APA.IsReadOnly = !_ican;
                APA.IsEnabled = _ican;
                ADV.IsReadOnly = !_ican;
                ADV.IsEnabled = _ican;
                APV.IsReadOnly = !_ican;
                APV.IsEnabled = _ican;
                HAVALAH.IsReadOnly = !_ican;
                HAVALAH.IsEnabled = _ican;
                HAVALAH2.IsReadOnly = !_ican;
                HAVALAH2.IsEnabled = _ican;
                HAZ_TOL.IsReadOnly = !_ican;
                HAZ_TOL.IsEnabled = _ican;
                HAZ_TOL2.IsReadOnly = !_ican;
                HAZ_TOL2.IsEnabled = _ican;
                PHAZ_TOL.IsReadOnly = !_ican;
                PHAZ_TOL.IsEnabled = _ican;
                PHAZ_TOL2.IsReadOnly = !_ican;
                PHAZ_TOL2.IsEnabled = _ican;
                PJHAZ_TOL1.IsReadOnly = !_ican;
                PJHAZ_TOL1.IsEnabled = _ican;
                PJHAZ_TOL12.IsReadOnly = !_ican;
                PJHAZ_TOL12.IsEnabled = _ican;
                PPDAST.IsReadOnly = !_ican;
                PPDAST.IsEnabled = _ican;
                PPDAST1.IsReadOnly = !_ican;
                PPDAST1.IsEnabled = _ican;
                PPSAR.IsReadOnly = !_ican;
                PPSAR.IsEnabled = _ican;
                PPSAR1.IsReadOnly = !_ican;
                PPSAR1.IsEnabled = _ican;
                CONKAL.IsReadOnly = !_ican;
                CONKAL.IsEnabled = _ican;
                CONKAL2.IsReadOnly = !_ican;
                CONKAL2.IsEnabled = _ican;
                GHEYMAT.IsReadOnly = !_ican;
                GHEYMAT.IsEnabled = _ican;
                GHEYMAT1.IsReadOnly = !_ican;
                GHEYMAT1.IsEnabled = _ican;
                AMALKARD.IsReadOnly = !_ican;
                AMALKARD.IsEnabled = _ican;
                AMALKARD1.IsReadOnly = !_ican;
                AMALKARD1.IsEnabled = _ican;
                PKHARID.IsReadOnly = !_ican;
                PKHARID.IsEnabled = _ican;
                PKHARID2.IsReadOnly = !_ican;
                PKHARID2.IsEnabled = _ican;
                HESMBAA.IsReadOnly = !_ican;
                HESMBAA.IsEnabled = _ican;
                HPOR.IsReadOnly = !_ican;
                HPOR.IsEnabled = _ican;
                hesnaghd.IsReadOnly = !_ican;
                hesnaghd.IsEnabled = _ican;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "AUTO", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            SourceReLoad();

            FILL_ALL_COMBOBOXES();

            this.AllowEdits = false;
        }
        private void SourceReLoad()
        {
            var RST = dbms.DoGetDataSQL<TOTA_HES>($"SELECT NUMBER, NAME FROM TOTA_HES ORDER BY NAME").ToList();
            foreach (var item in RST)
            {
                TOTA_HES_SOURCE.Add(item);
            }

            var HST = dbms.DoGetDataSQL<HESAB_PAIR_HES>($"SELECT hes, NAME FROM CUST_HESAB").ToList();
            foreach (var row in HST)
            {
                HESAB_SOURCE.Add(row);
            }

            var HST2 = dbms.DoGetDataSQL<AMC1>($"SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS Expr1, NAME FROM TDETA_HES ORDER BY NAME").ToList();
            foreach (var row in HST2)
            {
                HESAB_SOURCE2.Add(row);
            }

        }
        private void FILL_ALL_COMBOBOXES()
        {
            SANDOGH.ItemsSource = TOTA_HES_SOURCE;
            SANDOGH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT SANDOGH FROM dbo.SAZMAN").FirstOrDefault();
            SANDOGH2.ItemsSource = SANDOGH.ItemsSource;

            BANKHA.ItemsSource = TOTA_HES_SOURCE;
            BANKHA.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT BANKHA FROM dbo.SAZMAN").FirstOrDefault();
            BANKHA2.ItemsSource = BANKHA.ItemsSource;

            BESTANKAR.ItemsSource = TOTA_HES_SOURCE;
            BESTANKAR.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT BESTANKAR FROM dbo.SAZMAN").FirstOrDefault();
            BESTANKAR2.ItemsSource = BESTANKAR.ItemsSource;

            BEDEHKAR.ItemsSource = TOTA_HES_SOURCE;
            BEDEHKAR.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT BEDEHKAR FROM dbo.SAZMAN").FirstOrDefault();
            BEDEHKAR2.ItemsSource = BEDEHKAR.ItemsSource;

            KHARID.ItemsSource = TOTA_HES_SOURCE;
            KHARID.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT KHARID FROM dbo.SAZMAN").FirstOrDefault();
            KHARID2.ItemsSource = KHARID.ItemsSource;

            MKHARID.ItemsSource = TOTA_HES_SOURCE;
            MKHARID.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT MKHARID FROM dbo.SAZMAN").FirstOrDefault();
            MKHARID2.ItemsSource = MKHARID.ItemsSource;

            TKHARID.ItemsSource = TOTA_HES_SOURCE;
            TKHARID.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT TKHARID FROM dbo.SAZMAN").FirstOrDefault();
            TKHARID2.ItemsSource = TKHARID.ItemsSource;

            HKHARID.ItemsSource = TOTA_HES_SOURCE;
            HKHARID.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT HKHARID FROM dbo.SAZMAN").FirstOrDefault();
            HKHARID2.ItemsSource = HKHARID.ItemsSource;

            FROSH.ItemsSource = TOTA_HES_SOURCE;
            FROSH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT FROSH FROM dbo.SAZMAN").FirstOrDefault();
            FROSH2.ItemsSource = FROSH.ItemsSource;

            MFROSH.ItemsSource = TOTA_HES_SOURCE;
            MFROSH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT MFROSH FROM dbo.SAZMAN").FirstOrDefault();
            MFROSH2.ItemsSource = MFROSH.ItemsSource;

            TFROSH.ItemsSource = TOTA_HES_SOURCE;
            TFROSH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT TFROSH FROM dbo.SAZMAN").FirstOrDefault();
            TFROSH2.ItemsSource = TFROSH.ItemsSource;

            HFROSH.ItemsSource = TOTA_HES_SOURCE;
            HFROSH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT HFROSH FROM dbo.SAZMAN").FirstOrDefault();
            HFROSH2.ItemsSource = HFROSH.ItemsSource;

            //dbms.DoGetDataSQL<string?>($"SELECT TOP (1) hes FROM dbo.CUST_HESAB WHERE hes = N'{_PERSONEL_}'").FirstOrDefault();
            PERSONEL.ItemsSource = HESAB_SOURCE;
            var _PERSONEL_ = dbms.DoGetDataSQL<string?>("SELECT PERSONEL FROM dbo.SAZMAN").FirstOrDefault();
            PERSONEL.SelectedValue = _PERSONEL_;

            PERVAM.ItemsSource = HESAB_SOURCE;
            var _PERVAM_ = dbms.DoGetDataSQL<string?>("SELECT PERVAM FROM dbo.SAZMAN").FirstOrDefault();
            PERVAM.SelectedValue = _PERVAM_;

            MOGODIA.ItemsSource = TOTA_HES_SOURCE;
            MOGODIA.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT MOGODIA FROM dbo.SAZMAN").FirstOrDefault();
            MOGODIA2.ItemsSource = MOGODIA.ItemsSource;

            DARAM.ItemsSource = TOTA_HES_SOURCE;
            DARAM.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT DARAM FROM dbo.SAZMAN").FirstOrDefault();
            DARAM2.ItemsSource = DARAM.ItemsSource;

            HDARAM.ItemsSource = TOTA_HES_SOURCE;
            HDARAM.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT HDARAM FROM dbo.SAZMAN").FirstOrDefault();
            HDARAM2.ItemsSource = HDARAM.ItemsSource;

            ADA.ItemsSource = HESAB_SOURCE2;
            var _ADA_ = dbms.DoGetDataSQL<string?>("SELECT ADA FROM dbo.SAZMAN").FirstOrDefault();
            ADA.SelectedValue = _ADA_;

            APA.ItemsSource = HESAB_SOURCE2;
            var _APA_ = dbms.DoGetDataSQL<string?>("SELECT APA FROM dbo.SAZMAN").FirstOrDefault();
            APA.SelectedValue = _APA_;

            ADV.ItemsSource = HESAB_SOURCE2;
            var _ADV_ = dbms.DoGetDataSQL<string?>("SELECT ADV FROM dbo.SAZMAN").FirstOrDefault();
            ADV.SelectedValue = _ADV_;

            APV.ItemsSource = HESAB_SOURCE2;
            var _APV_ = dbms.DoGetDataSQL<string?>("SELECT APV FROM dbo.SAZMAN").FirstOrDefault();
            APV.SelectedValue = _APV_;

            HAVALAH.ItemsSource = TOTA_HES_SOURCE;
            HAVALAH.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT HAVALAH FROM dbo.SAZMAN").FirstOrDefault();
            HAVALAH2.ItemsSource = HAVALAH.ItemsSource;

            HAZ_TOL.ItemsSource = TOTA_HES_SOURCE;
            HAZ_TOL.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT HAZ_TOL FROM dbo.SAZMAN").FirstOrDefault();
            HAZ_TOL2.ItemsSource = HAZ_TOL.ItemsSource;

            PHAZ_TOL.ItemsSource = TOTA_HES_SOURCE;
            PHAZ_TOL.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT PHAZ_TOL FROM dbo.SAZMAN").FirstOrDefault();
            PHAZ_TOL2.ItemsSource = PHAZ_TOL.ItemsSource;

            PJHAZ_TOL1.ItemsSource = TOTA_HES_SOURCE;
            PJHAZ_TOL1.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT PJHAZ_TOL1 FROM dbo.SAZMAN").FirstOrDefault();
            PJHAZ_TOL12.ItemsSource = PJHAZ_TOL1.ItemsSource;

            PPDAST.ItemsSource = TOTA_HES_SOURCE;
            PPDAST.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT PPDAST FROM dbo.SAZMAN").FirstOrDefault();
            PPDAST1.ItemsSource = PPDAST.ItemsSource;

            PPSAR.ItemsSource = TOTA_HES_SOURCE;
            PPSAR.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT PPSAR FROM dbo.SAZMAN").FirstOrDefault();
            PPSAR1.ItemsSource = PPSAR.ItemsSource;

            CONKAL.ItemsSource = TOTA_HES_SOURCE;
            CONKAL.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT CONKAL FROM dbo.SAZMAN").FirstOrDefault();
            CONKAL2.ItemsSource = CONKAL.ItemsSource;

            GHEYMAT.ItemsSource = TOTA_HES_SOURCE;
            GHEYMAT.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT GHEYMAT FROM dbo.SAZMAN").FirstOrDefault();
            GHEYMAT1.ItemsSource = GHEYMAT.ItemsSource;

            AMALKARD.ItemsSource = TOTA_HES_SOURCE;
            AMALKARD.SelectedValue = dbms.DoGetDataSQL<double?>($"SELECT AMALKARD FROM dbo.SAZMAN").FirstOrDefault();
            AMALKARD1.ItemsSource = AMALKARD.ItemsSource;

            PKHARID.ItemsSource = TOTA_HES_SOURCE;
            PKHARID.SelectedValue = dbms.DoGetDataSQL<int?>($"SELECT PKHARID FROM dbo.SAZMAN").FirstOrDefault();
            PKHARID2.ItemsSource = PKHARID.ItemsSource;

            HESMBAA.ItemsSource = HESAB_SOURCE;
            var _HESMBAA_ = dbms.DoGetDataSQL<string?>("SELECT HESMBAA FROM dbo.SAZMAN").FirstOrDefault();
            HESMBAA.SelectedValue = _HESMBAA_;

            HPOR.ItemsSource = HESAB_SOURCE2;
            var _HPOR_ = dbms.DoGetDataSQL<string?>("SELECT HPOR FROM dbo.SAZMAN").FirstOrDefault();
            HPOR.SelectedValue = _HPOR_;

            hesnaghd.ItemsSource = HESAB_SOURCE2;
            var _hesnaghd_ = dbms.DoGetDataSQL<string?>("SELECT hesnaghd FROM dbo.SAZMAN").FirstOrDefault();
            hesnaghd.SelectedValue = _hesnaghd_;
        }
        private void ESLAH_BTN_Click(object sender, RoutedEventArgs e)
        {
            //if ((bool)Baseknow.TRANSF)
            dbms.DoExecuteSQL("INSERT INTO dbo.TR_SAZMAN (UNIVERSITY_CO, NAME, CITY, MANAGER, MOAVEN, ZIHESAB, AMINAMVAL, YEA, SANAD, GHAYM, KALA, PERSON, DIG, WAR, LST, TFTPAGE, TFSAZMAN,TFADDRESS, TFTEL, TFCODE_E, WIDTH_D, HIGH_D, CPI, SANDOGH, BANKHA, BESTANKAR, BEDEHKAR, KHARID, MKHARID, TKHARID, HKHARID, FROSH, MFROSH,TFROSH, HFROSH, MOGODIA, MOGODIP, DARAM, HDARAM, HKOL, ADA, APA, ADV, HAVALAH, CTRL_TS, F_ANBARF, GH_PK, L_NUMBER, SF_G, TAR_KM, BACKPATH,TKHF, HAZ_TOL, PJHAZ_TOL1, PHAZ_TOL, GHEYMAT, PPDAST, PPSAR, AMALKARD, PERSONEL, PERVAM, CONKAL, EMZA, HNAH, HEZA, HPAD, HOLA, HKHA, HJAZ,HRAN, HSAY, HCON, HSHI, HAZEDAR, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, SAGHFH, MAND, MOJU, SA_HOGH, SA_40EZ, SA_EZAF, SA_PADA,SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, HAZTOLID, HAZFROOSH, HAZKHADAMAT, PISHDAR, DEFANB, DEFTKH, ECONM,FRUP, UPDDATE, FINALS, PSANDHES, SANAVP, BON, ISO_FROOSH, ISO_KHAREED, ISO_MAVAD, ISO_TOLID, ISO_MAVADSAYER, SANAT, CODEVIEW, PKHARID," + " SIGN, BARCOD, SAGHF, SERVERNAM, TENDAR, LECOL1, LECOL2, LECOL3, LECOL4, LKCOL1, HESMBAA, ECODE, PCODE, IYALAT, MCODEM, HPOR, SAGHF2, OPTIONSS, CTL_DT, LOCKFAP, LOCKFSI, TRANSF, OKF, ARSESH, RMOG, APV, HOTCOD, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, STTOL, STFRB, STBKH, STMO, STKHA, SNDKH, UP_DATE, UP_TIME, UP_USER_NAME, PC_NAME, IPADD) SELECT     UNIVERSITY_CO, NAME, CITY, MANAGER, MOAVEN, ZIHESAB, AMINAMVAL, YEA, SANAD, GHAYM, KALA, PERSON, DIG, WAR, LST, TFTPAGE, TFSAZMAN,TFADDRESS, TFTEL, TFCODE_E, WIDTH_D, HIGH_D, CPI, SANDOGH, BANKHA, BESTANKAR, BEDEHKAR, KHARID, MKHARID, TKHARID, HKHARID, FROSH, MFROSH,TFROSH, HFROSH, MOGODIA, MOGODIP, DARAM, HDARAM, HKOL, ADA, APA, ADV, HAVALAH, CTRL_TS, F_ANBARF, GH_PK, L_NUMBER, SF_G, TAR_KM, BACKPATH,TKHF, HAZ_TOL, PJHAZ_TOL1, PHAZ_TOL, GHEYMAT, PPDAST, PPSAR, AMALKARD, PERSONEL, PERVAM, CONKAL, EMZA, HNAH, HEZA, HPAD, HOLA, HKHA, HJAZ," + " HRAN, HSAY, HCON, HSHI, HAZEDAR, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, SAGHFH, MAND, MOJU, SA_HOGH, SA_40EZ, SA_EZAF, SA_PADA, SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, HAZTOLID, HAZFROOSH, HAZKHADAMAT, PISHDAR, DEFANB, DEFTKH, ECONM, FRUP, UPDDATE, FINALS, PSANDHES, SANAVP, BON, ISO_FROOSH, ISO_KHAREED, ISO_MAVAD, ISO_TOLID, ISO_MAVADSAYER, SANAT, CODEVIEW, PKHARID, SIGN, BARCOD, SAGHF, SERVERNAM, TENDAR, LECOL1, LECOL2, LECOL3, LECOL4, LKCOL1, HESMBAA, ECODE, PCODE, IYALAT, MCODEM, HPOR, SAGHF2,OPTIONSS, CTL_DT, LOCKFAP, LOCKFSI, TRANSF, OKF, ARSESH, RMOG, APV, HOTCOD, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, STTOL, STFRB, STBKH , STMO, STKHA, SNDKH ," + Tarikh.FullCurrentDate + " AS Expr2 , " + Tarikh.GET_OADATE_DAO() + "  AS Expr1,'" + CL_HESABDARI.UCurrentUser() + "' AS EXPR3,'" + CL_HESABDARI.CurrentMachineName() + "' AS EXPR4 , '" + CL_HESABDARI.GETIPADD() + "' AS EXPR5 FROM         dbo.SAZMAN ");
            this.AllowEdits = true;
        }
        private void SAVE_BTN_Click(object sender, RoutedEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "كاربر گرامي تغيير دادن حسابهاي خودگردان تاثيرات زيادي روي صدور اتوماتيك  اسناد دارد بنابر اين دقت كنيد كه عمليات را درست انجام داده باشيد .آيا ذخيره انجام شود");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                SaveUpdateSazman();

                new Msgwin(false, "ذخیره انجام شد.").ShowDialog();
            }
        }
        private void SaveUpdateSazman()
        {
            string updateQuery = @"
            UPDATE dbo.SAZMAN
            SET
            SANDOGH = @SANDOGH,
            BANKHA = @BANKHA,
            BESTANKAR = @BESTANKAR,
            BEDEHKAR = @BEDEHKAR,
            KHARID = @KHARID,
            MKHARID = @MKHARID,
            TKHARID = @TKHARID,
            HKHARID = @HKHARID,
            FROSH = @FROSH,
            MFROSH = @MFROSH,
            TFROSH = @TFROSH,
            HFROSH = @HFROSH,
            PERSONEL = @PERSONEL,
            PERVAM = @PERVAM,
            MOGODIA = @MOGODIA,
            DARAM = @DARAM,
            HDARAM = @HDARAM,
            ADA = @ADA,
            APA = @APA,
            ADV = @ADV,
            APV = @APV,
            HAVALAH = @HAVALAH,
            HAZ_TOL = @HAZ_TOL,
            PHAZ_TOL = @PHAZ_TOL,
            PJHAZ_TOL1 = @PJHAZ_TOL1,
            PPDAST = @PPDAST,
            PPSAR = @PPSAR,
            CONKAL = @CONKAL,
            GHEYMAT = @GHEYMAT,
            AMALKARD = @AMALKARD,
            PKHARID = @PKHARID,
            HESMBAA = @HESMBAA,
            HPOR = @HPOR,
            hesnaghd = @hesnaghd
            ";

            var parameters = new
            {
                SANDOGH = (double?)SANDOGH.SelectedValue,
                BANKHA = (double?)BANKHA.SelectedValue,
                BESTANKAR = (double?)BESTANKAR.SelectedValue,
                BEDEHKAR = (double?)BEDEHKAR.SelectedValue,
                KHARID = (double?)KHARID.SelectedValue,
                MKHARID = (double?)MKHARID.SelectedValue,
                TKHARID = (double?)TKHARID.SelectedValue,
                HKHARID = (double?)HKHARID.SelectedValue,
                FROSH = (double?)FROSH.SelectedValue,
                MFROSH = (double?)MFROSH.SelectedValue,
                TFROSH = (double?)TFROSH.SelectedValue,
                HFROSH = (double?)HFROSH.SelectedValue,
                PERSONEL = (string)PERSONEL.SelectedValue,
                PERVAM = (string)PERVAM.SelectedValue,
                MOGODIA = (double?)MOGODIA.SelectedValue,
                DARAM = (double?)DARAM.SelectedValue,
                HDARAM = (double?)HDARAM.SelectedValue,
                ADA = (string)ADA.SelectedValue,
                APA = (string)APA.SelectedValue,
                ADV = (string)ADV.SelectedValue,
                APV = (string)APV.SelectedValue,
                HAVALAH = (double?)HAVALAH.SelectedValue,
                HAZ_TOL = (double?)HAZ_TOL.SelectedValue,
                PHAZ_TOL = (double?)PHAZ_TOL.SelectedValue,
                PJHAZ_TOL1 = (double?)PJHAZ_TOL1.SelectedValue,
                PPDAST = (double?)PPDAST.SelectedValue,
                PPSAR = (double?)PPSAR.SelectedValue,
                CONKAL = (double?)CONKAL.SelectedValue,
                GHEYMAT = (double?)GHEYMAT.SelectedValue,
                AMALKARD = (double?)AMALKARD.SelectedValue,
                PKHARID = (int?)PKHARID.SelectedValue,
                HESMBAA = (string)HESMBAA.SelectedValue,
                HPOR = (string)HPOR.SelectedValue,
                hesnaghd = (string)hesnaghd.SelectedValue
            };

            dbms.DoExecuteSQL(updateQuery, parameters);
        }
    }
}
