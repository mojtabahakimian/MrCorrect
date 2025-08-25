using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Functions.SMSService.SmsServiceFactory;

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
        public WIN_SAZMAN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; private set; }

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

                #region GENERAL_TAB
                WIDTH_D.IsReadOnly = !ican;
                HIGH_D.IsReadOnly = !ican;
                BACKPATH.IsReadOnly = !ican;
                TFADDRESS.IsReadOnly = !ican;
                TFTEL.IsReadOnly = !ican;
                SERVERNAM.IsReadOnly = !ican;
                OPTIONSS.IsReadOnly = !ican;
                
                EMZA_CANVAS.IsEnabled = ican;
                BTN_UPIMAGE.IsEnabled = ican;
                BTN_SAVE_G.IsEnabled = ican;
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

            var RST = dbms.DoGetDataSQL<SAZMAN>($"SELECT SMS_USERNAME,SMS_PASSWORD ,SMS_LIBKEY , SMS_TSMSHOST , DSMS , PRMFR , SMSACT , SMS_OWNER , SMSTYPE ," +
                $"WIDTH_D,HIGH_D,BACKPATH,TFADDRESS,TFTEL,SERVERNAM,EMZA,OPTIONSS " +
                $" FROM dbo.SAZMAN ").FirstOrDefault();

            if (RST != null)
            {
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
            if (!BTN_SAVE.IsEnabled) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از ذخیره تغیرات مطمئن هستید؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult == false)
            {
                return;
            }

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
                SMSPINFO.LINE_NUMBER = Convert.ToInt64(SMS_TSMSHOST.Text);

                if ((bool)RB_SMSIR.IsChecked)
                {
                    SMSPINFO.API_KEY = SMS_LIBKEY.Password;
                }
            }
            #endregion

            #region GENERAL
            {
                try
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

                    universControl.PopNotifyShowUp("." + "ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                    ChangeIsHappend = false;
                }
                catch (Exception ex)
                {
                    new Msgwin(false, $"خطا هنگام ذخیره‌سازی").ShowDialog();
                }
            }
            #endregion

            universControl.PopNotifyShowUp("." + "ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            ChangeIsHappend = false;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SMS_PASSWORD_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
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

        }

        private void BTN_ESLAH_G_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}