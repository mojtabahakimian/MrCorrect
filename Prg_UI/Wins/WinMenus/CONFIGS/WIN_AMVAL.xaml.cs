using DocumentFormat.OpenXml.Drawing.Charts;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Syncfusion.XlsIO.Implementation.Collections.RowStorage;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_AMVAL : Window
    {
        public WIN_AMVAL(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
                BARCHASB.Text = number_to_open.ToString();
            }
        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private NavigationManager<AMVAL_DTL> _navigationManager;

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
        //private int? _navigationManager.CurrentRecord.BARCHASB;

        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                ANAME.IsReadOnly = !ican;
                CALMETHOD.IsEnabled = ican;
                RATE.IsReadOnly = !ican;
                HESAB.IsEnabled = ican;
                HESAB2.IsEnabled = ican;
                KHDATE.IsReadOnly = !ican;
                PRICE.IsReadOnly = !ican;
                PLACE.IsEnabled = ican;
                DESCRIPTION.IsReadOnly = !ican;
                ASTATUS.IsEnabled = ican;
                BTN_SAVE.IsEnabled = ican;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            _navigationManager = new NavigationManager<AMVAL_DTL>(
                dbms,
                x => x.BARCHASB.ToString(), // property selector
                $"SELECT * FROM dbo.amval ORDER BY barchasb", //All Record of The Table
                /*on navigation get ever record where*/ x => $"SELECT TOP 1 barchasb, aname, calmethod, rate, hesab, khdate, price, place, arzesh, astatus, Description, USER_NAME, CRT, UID FROM dbo.amval WHERE barchasb = {x.BARCHASB} ", //On Change for One Record
                Convert.ToDouble(BARCHASB.Text)
            );
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

        }
        private void FILL_ALL_COMBOBOXES()
        {
            var items0 = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "مستقیم"),
                new KeyValuePair<int, string>(2, "نزولی")
            };
            CALMETHOD.ItemsSource = items0;

            var items1 = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "در حال استفاده"),
                new KeyValuePair<int, string>(2, "راکد"),
                new KeyValuePair<int, string>(3, "مفقودی"),
                new KeyValuePair<int, string>(4, "در دست تعمیر"),
                new KeyValuePair<int, string>(5, "فروخته شده"),
                new KeyValuePair<int, string>(10, "نامشخص")
            };
            ASTATUS.ItemsSource = items1;
            ASTATUS.SelectedValue = 1; // Default

            var hesabList = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
            HESAB.ItemsSource = hesabList?.ToList();
            HESAB2.ItemsSource = HESAB.ItemsSource;

            string sql = "SELECT DISTINCT place FROM dbo.AMVAL WHERE place IS NOT NULL ORDER BY place";
            var result = dbms.DoGetDataSQL<string>(sql);
            PLACE.ItemsSource = result?.ToList();
        }

        private bool OnInsertRecord(AMVAL_DTL record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<AMVAL_DTL>($"SELECT TOP 1 * FROM dbo.amval WHERE barchasb = {BARCHASB.Text} ").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(AMVAL_DTL record)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
            }
            else if (record == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                BTN_SAVE.IsEnabled = false;

                //_navigationManager.CurrentRecord.BARCHASB = record.BARCHASB;

                BARCHASB.Text = record.BARCHASB?.ToString() ?? "";
                ANAME.Text = record.ANAME ?? "";
                CALMETHOD.SelectedValue = record.CALMETHOD;
                RATE.Text = record.RATE?.ToString("0.00") ?? "";
                HESAB.SelectedValue = record.HESAB ?? "";
                KHDATE.Text = record.KHDATE ?? "";
                PRICE.Text = record.PRICE?.ToString("#,0") ?? "";
                PLACE.Text = record.PLACE ?? "";
                ARZESH.Text = record.ARZESH?.ToString("#,0") ?? "";
                ASTATUS.SelectedValue = record.ASTATUS ?? 1;
                DESCRIPTION.Text = record.DESCRIPTION ?? "";

                Form_Current();
            }
        }

        private void ClearFreshAll()
        {
            AllowEdits = true;

            BARCHASB.Clear();
            ANAME.Clear();
            CALMETHOD.SelectedIndex = -1;
            RATE.Text = "0";
            HESAB.SelectedIndex = -1;
            KHDATE.Clear();
            PRICE.Text = "0";
            PLACE.SelectedIndex = -1;
            ARZESH.Text = "0";
            ASTATUS.SelectedValue = 1;
            DESCRIPTION.Clear();
            PIC.Source = null;
            //_navigationManager.CurrentRecord.BARCHASB = null;
            GetDefaultFocus();
        }

        private void GetDefaultFocus()
        {
            BARCHASB.Focus(); BARCHASB.SelectAll();
        }

        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<AMVAL_DTL>($"SELECT TOP 1 * FROM dbo.amval WHERE barchasb = {BARCHASB.Text} ").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }
        private void Form_Current()
        {
            // Clear image first
            PIC.Source = null;

            if (!_navigationManager.IsNewRecord)
            {
                // Load image if exists
                if (!string.IsNullOrEmpty(Baseknow.BACKPATH) && _navigationManager.CurrentRecord.BARCHASB.HasValue)
                {
                    LoadMatchingImageAsync(_navigationManager.CurrentRecord.BARCHASB.ToString());
                }

                if (_navigationManager.CurrentRecord.BARCHASB.HasValue && _navigationManager.CurrentRecord.BARCHASB.Value > 0)
                {
                    AllowDeletions = false;
                    AllowEdits = false;
                    BTN_ESLAH.IsEnabled = true;
                }
                else
                {
                    AllowDeletions = true;
                    AllowEdits = true;
                    BTN_ESLAH.IsEnabled = true;
                }
            }
            else
            {
                AllowDeletions = true;
                AllowEdits = true;
                BTN_ESLAH.IsEnabled = false;
            }
        }

        private async void LoadMatchingImageAsync(string barchasbCode)
        {
            try
            {
                var pathExists = await Task.Run(() => Directory.Exists(Baseknow.BACKPATH));
                if (!pathExists) return;

                string imageFolderPath = Path.Combine(Baseknow.BACKPATH, "amval");
                string imagePath = await Task.Run(() => CL_LMethods.FindImageFile(imageFolderPath, barchasbCode));

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 280;
                    image.UriSource = new Uri(imagePath);
                    image.EndInit();
                    image.Freeze();

                    Dispatcher.Invoke(() => PIC.Source = image);
                }
            }
            catch { }
        }


        private void RATE_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-format rate as decimal
            if (decimal.TryParse(RATE.Text.Replace(",", ""), out decimal rate))
            {
                if (rate > 100) rate = 100;
                if (rate < 0) rate = 0;
            }
        }

        private bool HeaderIsValid()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(ANAME.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام دارایی الزامی است" });
            }

            if (string.IsNullOrWhiteSpace(BARCHASB.Text) || BARCHASB.Text == "0")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد برچست نمیتواند خالی باشد" });
            }

            if (CALMETHOD.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "روش محاسبه استهلاک الزامی است" });
            }

            if (string.IsNullOrWhiteSpace(KHDATE.Text.ToRawTarikh()))
            {
                //ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ خرید الزامی است" });
            }
            else
            {
                if (!Tarikh.IsValidedDate(KHDATE.Text.ToRawTarikh()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
                }
                else
                {
                    //if (!Tarikh.IsSyncedDateNow(KHDATE.Text, (bool)Baseknow.CTL_DT))
                    //{
                    //    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    //}
                }
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }

            return true;
        }
        private void InsertRecordAsync()
        {
            string sql = @"
             INSERT INTO dbo.AMVAL 
             (barchasb,aname, calmethod, rate, hesab, khdate, price, place, arzesh, astatus, Description, USER_NAME, UID)
             VALUES 
             (@barchasb,@aname, @calmethod, @rate, @hesab, @khdate, @price, @place, @arzesh, @astatus, @Description, @USER_NAME, @UID);";

            var parameters = new
            {
                barchasb = BARCHASB.Text.Trim(),
                aname = ANAME.Text.Trim(),
                calmethod = CALMETHOD.SelectedValue ?? 1,
                rate = double.TryParse(RATE.Text.Replace(",", ""), out double rate) ? rate : 0.0,
                hesab = HESAB.SelectedValue?.ToString(),
                khdate = string.IsNullOrWhiteSpace(KHDATE.Text.ToRawTarikh()) ? (long?)null : long.Parse(KHDATE.Text.ToRawTarikh().Trim()),
                price = double.TryParse(PRICE.Text.Replace(",", ""), out double price) ? price : (double?)null,
                place = PLACE.Text.Trim(),
                arzesh = long.TryParse(ARZESH.Text.Replace(",", ""), out long arzesh) ? arzesh : (long?)null,
                astatus = ASTATUS.SelectedValue ?? 1,
                Description = DESCRIPTION.Text.Trim(),
                USER_NAME = Baseknow.UUSER,
                UID = Baseknow.USERCOD
            };

            var result = dbms.DoExecuteSQL(sql, parameters);
            //var newBarchasb = result?.FirstOrDefault();

            //if (newBarchasb > 0)
            //{
            //    _navigationManager.CurrentRecord.BARCHASB = (int?)newBarchasb;
            //    BARCHASB.Text = newBarchasb.ToString();
            //}
        }

        private void UpdateRecordAsync()
        {
            // بررسی وجود BARCHASB
            if (!_navigationManager.CurrentRecord.BARCHASB.HasValue)
            {
                new Msgwin(false, "شناسه رکورد معتبر نیست").ShowDialog();
                return;
            }

            // Archive old record if TRANSF enabled (اگر جدول TR_AMVAL وجود دارد)
            // فرض می‌کنیم ساختار TR_AMVAL مشابه AMVAL است
            string archiveSql = @"
            INSERT INTO dbo.TR_AMVAL 
            (barchasb, aname, calmethod, rate, hesab, khdate, price, place, arzesh, 
             astatus, Description, USER_NAME, CRT, UID, UP_DATE, UP_TIME, UP_USER_NAME, PC_NAME, IPADD)
            SELECT 
                barchasb, 
                aname, 
                calmethod, 
                rate, 
                hesab, 
                khdate, 
                price, 
                place, 
                arzesh,
                astatus, 
                Description, 
                USER_NAME, 
                CRT, 
                UID,
                @UP_DATE,           -- تاریخ آرشیو
                @UP_TIME,           -- زمان آرشیو
                @UP_USER_NAME,      -- کاربر آرشیو‌کننده
                @PC_NAME,           -- نام کامپیوتر
                @IPADD              -- IP Address
            FROM dbo.AMVAL
            WHERE barchasb = @barchasb";

            dbms.DoExecuteSQL(archiveSql, new
            {
                barchasb = _navigationManager.CurrentRecord.BARCHASB.Value,
                UP_DATE = Tarikh.FullCurrentDate,                    // تاریخ شمسی فعلی
                UP_TIME = DateTime.Now.TimeOfDay.TotalSeconds,       // زمان به ثانیه
                UP_USER_NAME = Baseknow.UUSER,                       // نام کاربر
                PC_NAME = Environment.MachineName,                   // نام کامپیوتر
                IPADD = CL_HESABDARI.GETIPADD()                      // IP Address
            });

            // Update current record
            string updateSql = @"
            UPDATE dbo.AMVAL SET
                aname = @aname,
                calmethod = @calmethod,
                rate = @rate,
                hesab = @hesab,
                khdate = @khdate,
                price = @price,
                place = @place,
                arzesh = @arzesh,
                astatus = @astatus,
                Description = @Description,
                USER_NAME = @USER_NAME
            WHERE barchasb = @barchasb";

            var parameters = new
            {
                barchasb = _navigationManager.CurrentRecord.BARCHASB.Value,
                aname = ANAME.Text.Trim(),
                calmethod = CALMETHOD.SelectedValue ?? 1,
                rate = double.TryParse(RATE.Text.Replace(",", ""), out double rate) ? rate : 0.0,
                hesab = HESAB.SelectedValue?.ToString(),
                khdate = string.IsNullOrWhiteSpace(KHDATE.Text.ToRawTarikh()) ? (long?)null : long.Parse(KHDATE.Text.ToRawTarikh().Trim()),
                price = double.TryParse(PRICE.Text.Replace(",", ""), out double price) ? price : (double?)null,
                place = PLACE.Text.Trim(),
                arzesh = long.TryParse(ARZESH.Text.Replace(",", ""), out long arzesh) ? arzesh : (long?)null,
                astatus = ASTATUS.SelectedValue ?? 1,
                Description = DESCRIPTION.Text.Trim(),
                USER_NAME = Baseknow.UUSER,
            };

            var rowsAffected = dbms.DoExecuteSQL(updateSql, parameters);

        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            try
            {
                if (!HeaderIsValid()) return;

                if (_navigationManager.IsNewRecord)
                {
                    InsertRecordAsync();
                    _navigationManager.IsNewRecord = false;
                    RefreshAfterUpdate();
                }
                else
                {
                    UpdateRecordAsync();
                }

                universControl.PopNotifyShowUp(".ذخیره با موفقیت انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات تکراری وارد شده").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در ذخیره").Show();
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_ESLAH.IsEnabled || _navigationManager.IsNewRecord) { return; }

            string archiveSql = @"
                        INSERT INTO dbo.TR_AMVAL 
                        (barchasb, aname, calmethod, rate, hesab, khdate, price, place, arzesh, 
                         astatus, Description, USER_NAME, UP_TIME, UP_DATE, UP_USER_NAME, PC_NAME, IPADD)
                        SELECT barchasb, aname, calmethod, rate, hesab, khdate, price, place, arzesh,
                               astatus, Description, USER_NAME, @UP_TIME, @UP_DATE, @UP_USER_NAME, @PC_NAME, @IPADD
                        FROM dbo.AMVAL
                        WHERE barchasb = @barchasb";

            dbms.DoExecuteSQL(archiveSql, new
            {
                barchasb = _navigationManager.CurrentRecord.BARCHASB.Value,
                UP_TIME = DateTime.Now.TimeOfDay.TotalSeconds,
                UP_DATE = Tarikh.FullCurrentDate,
                UP_USER_NAME = Baseknow.UUSER,
                PC_NAME = Environment.MachineName,
                IPADD = CL_HESABDARI.GETIPADD()
            });

            AllowDeletions = true;
            AllowEdits = true;

            if (Baseknow.UUSER != "Administer")
            {
                string updateUserSql = "UPDATE dbo.AMVAL SET USER_NAME = @USER_NAME WHERE barchasb = @barchasb";
                dbms.DoExecuteSQL(updateUserSql, new
                {
                    USER_NAME = Baseknow.UUSER,
                    barchasb = _navigationManager.CurrentRecord.BARCHASB.Value
                });
            }

        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible || _navigationManager.IsNewRecord) { return; }

            _ = AuditLogger.LogActionAsync(
                                  actionType: "Delete",
                                  tableName: "جمعداری اموال",
                                  recordId: BARCHASB.Text,
                                  oldValue: default,
                                  newValue: _navigationManager.CurrentRecord,
                                  additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            if (!string.IsNullOrEmpty(BARCHASB.Text) && BARCHASB.Text != "0" && !string.IsNullOrEmpty(BARCHASB.Text) && BARCHASB.Text != "0")
            {
                try
                {
                    string sql = "DELETE FROM dbo.AMVAL WHERE barchasb = @barchasb";
                    dbms.DoExecuteSQL(sql, new { barchasb = Convert.ToDouble(BARCHASB.Text) });
                    _navigationManager.DeleteCurrentRecord(); //Refresh Record Source
                }
                catch (SqlException ex)
                {
                    if (e != null)
                    {
                        e.Handled = true;
                    }

                    if (ex.Number == 547)
                    {
                        new Msgwin(false, "این برگه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                        return;
                    }
                    else
                    {
                        new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog(); return;
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                }
            }
        }
    }
}
