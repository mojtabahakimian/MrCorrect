using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
using System.Windows.Media.Imaging;
using Wins.WinOther;

namespace Prg_UI.Wins.WinSetting
{
    /// <summary>
    /// Interaction logic for USERS.xaml
    /// </summary>
    public partial class USERS : Window
    {
        private readonly CL_CCNNMANAGER dbms = new();
        private readonly UniversControl universControl = new();
        private readonly SALGROUP_MODEL signatureProxy = new();

        public ObservableCollection<COMBOYMODEL> GroupItems { get; } = new()
        {
            new COMBOYMODEL { ID = 1, NAME = "مديران" },
            new COMBOYMODEL { ID = 2, NAME = "حسابداران" },
            new COMBOYMODEL { ID = 3, NAME = "بازرگاني" },
            new COMBOYMODEL { ID = 4, NAME = "انبار" },
            new COMBOYMODEL { ID = 5, NAME = "كارگزيني" },
            new COMBOYMODEL { ID = 6, NAME = "هتل" },
            new COMBOYMODEL { ID = 7, NAME = "تالار" },
            new COMBOYMODEL { ID = 8, NAME = "ريموت" },
            new COMBOYMODEL { ID = 9, NAME = "آفلاين" },
            new COMBOYMODEL { ID = 11, NAME = "ويزيتور" },
            new COMBOYMODEL { ID = 12, NAME = "سفير مديران" }
        };

        public ObservableCollection<HesabItem> HesabItems { get; } = new();

        private byte[]? _signatureBytes;
        public bool NowIsReady { get; private set; }

        public USERS()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Header Window
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            var packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
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

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CL_HESABDARI.AMALIYAT_USER(GetType().Name);
                CL_HESABDARI.SETSECURITY(GetType().Name, "USERS", new WindowInteropHelper(this).Handle, GetType().Name);
            }
            catch
            {
                // در صورت بروز خطا در امنیت، پنجره توسط SETSECURITY بسته می‌شود.
            }

            LoadHesabItems();
            UserNameTextBox.Focus();
        }

        private void Window_ContentRendered(object sender, EventArgs e) => NowIsReady = true;

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (ConfirmButton.IsFocused)
                {
                    return;
                }

                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        #region Hesab helpers
        private void LoadHesabItems()
        {
            try
            {
                const string sql = @"
SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes,
       NAME AS nam
FROM dbo.TDETA_HES
ORDER BY NAME";

                var results = dbms.DoGetDataSQL<HesabItem>(sql).ToList();
                HesabItems.Clear();
                foreach (var item in results)
                {
                    item.nam = item.nam?.FixPersianChars();
                    HesabItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShowUp($"خطا در دریافت لیست حساب ها: {ex.Message}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
            }
        }

        private void SetSelectedHes(string? hesCode)
        {
            if (string.IsNullOrWhiteSpace(hesCode))
            {
                HesCombo.SelectedValue = null;
                HesCodeCombo.SelectedValue = null;
                return;
            }

            HesCombo.SelectedValue = hesCode;
            HesCodeCombo.SelectedValue = hesCode;
        }

        private void EnsureHesItemExists(HesabItem? item)
        {
            if (item == null)
            {
                return;
            }

            if (!HesabItems.Any(x => string.Equals(x.hes, item.hes, StringComparison.OrdinalIgnoreCase)))
            {
                item.nam = item.nam?.FixPersianChars();
                HesabItems.Add(item);
            }
        }

        private void HesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            if (HesCombo.SelectedValue is string hes)
            {
                HesCodeCombo.SelectedValue = hes;
            }

            if (HesCodeCombo.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HesCodeCombo.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    HesCodeCombo.SelectedValue = null;
                }
                //if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
                //{
                //    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(HesCodeCombo.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.HesCodeCombo.SelectedValue.ToString())) == false)
                //    {
                //        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                //        msgwin.ShowDialog();
                //        HesCodeCombo.SelectedValue = null;
                //    }
                //}
                if (CL_HESABDARI.BLOCKEDCUST(HesCodeCombo.SelectedValue.ToString()))
                {
                    HesCodeCombo.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

        }

        //private void HesCodeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (!NowIsReady)
        //    {
        //        return;
        //    }

        //    if (HesCodeCombo.SelectedValue is string hes)
        //    {
        //        HesCombo.SelectedValue = hes;
        //    }
        //}

        private void HesCodeCombo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            var text = HesCodeCombo.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var match = HesabItems.FirstOrDefault(x => string.Equals(x.hes, text, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                SetSelectedHes(match.hes);
                return;
            }

            if (int.TryParse(text, out var tnumber) && Baseknow.BEDEHKAR.HasValue)
            {
                try
                {
                    const string sql = @"
                                        SELECT TOP (1)
                                               RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes,
                                               NAME AS nam
                                        FROM dbo.TDETA_HES
                                        WHERE N_KOL = @NKOL AND NUMBER = 1 AND TNUMBER = @TNUMBER";

                    var candidate = dbms.DoGetDataSQL<HesabItem>(sql, new { NKOL = Convert.ToInt32(Baseknow.BEDEHKAR.Value), TNUMBER = tnumber }).FirstOrDefault();
                    EnsureHesItemExists(candidate);
                    SetSelectedHes(candidate?.hes);
                }
                catch (Exception ex)
                {
                    universControl.PopNotifyShowUp($"خطا در بررسی حساب: {ex.Message}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
                }
            }
        }
        #endregion

        #region Signature helpers
        private void CaptureSignatureButton_Click(object sender, RoutedEventArgs e)
        {
            signatureProxy.EMZA = _signatureBytes;
            var win = new WIN_SIGN_CAPTURE(signatureProxy)
            {
                Owner = this
            };
            win.ShowDialog();

            _signatureBytes = signatureProxy.EMZA;
            UpdateSignaturePreview();
        }

        private void UploadSignatureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "انتخاب تصویر امضا",
                    Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                    FilterIndex = 1,
                    Multiselect = false
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                if (!CL_LMethods.ValidateImageFile(dialog.FileName, out var error, 3))
                {
                    new Msgwin(false, error).ShowDialog();
                    return;
                }

                _signatureBytes = CL_LMethods.ConvertFileToByte(dialog.FileName);
                UpdateSignaturePreview();
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShowUp($"خطا در بارگذاری تصویر امضا: {ex.Message}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
            }
        }

        private void ClearSignatureButton_Click(object sender, RoutedEventArgs e)
        {
            _signatureBytes = null;
            UpdateSignaturePreview();
        }

        private void UpdateSignaturePreview()
        {
            if (_signatureBytes is { Length: > 0 })
            {
                try
                {
                    SignatureImage.Source = CL_LMethods.ByteArrayToBitmapImage(_signatureBytes);
                    SignatureImage.Visibility = Visibility.Visible;
                    SignaturePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    SignatureImage.Source = null;
                    SignatureImage.Visibility = Visibility.Collapsed;
                    SignaturePlaceholder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SignatureImage.Source = null;
                SignatureImage.Visibility = Visibility.Collapsed;
                SignaturePlaceholder.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            string userNameInput = (UserNameTextBox.Text ?? string.Empty).Trim().FixPersianChars();
            string password1Input = (PasswordBox1.Password ?? string.Empty).Trim();
            string password2Input = (PasswordBox2.Password ?? string.Empty).Trim();
            string? hesValue = HesCombo.SelectedValue as string ?? HesCodeCombo.SelectedValue as string;
            int? selectedGroup = GroupCombo.SelectedValue as int?;

            if (string.IsNullOrWhiteSpace(userNameInput))
            {
                universControl.PopNotifyShowUp("خطا: نام کاربر صحیح نیست", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                UserNameTextBox.Focus();
                return;
            }

            if (!string.Equals(password1Input, password2Input, StringComparison.Ordinal))
            {
                universControl.PopNotifyShowUp("خطا: رمزهای عبور یکسان نیستند", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                PasswordBox1.Clear();
                PasswordBox2.Clear();
                PasswordBox1.Focus();
                return;
            }

            if (password1Input.Length > 40)
            {
                universControl.PopNotifyShowUp("طول رمز عبور باید کمتر از ۴۰ کاراکتر باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                PasswordBox1.Focus();
                return;
            }

            if (selectedGroup is null)
            {
                universControl.PopNotifyShowUp("لطفا گروه کاربری را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                GroupCombo.Focus();
                return;
            }

            if (!CheckTabletLicenses(selectedGroup.Value))
            {
                return;
            }

            try
            {
                long newId = (dbms.DoGetDataSQL<long?>("SELECT MAX(IDD) FROM dbo.SALA_DTL").FirstOrDefault() ?? 0) + 1;
                string encodedName = CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(userNameInput));
                string encodedPassword = CL_HESABDARI.CODEPAL(password1Input);

                var duplicateCount = dbms.DoGetDataSQL<int?>(
                    "SELECT COUNT(1) FROM dbo.SALA_DTL WHERE SAL_NAME = @SN",
                    new { SN = encodedName }).FirstOrDefault();

                if (duplicateCount.GetValueOrDefault() > 0)
                {
                    universControl.PopNotifyShowUp("کاربری با این نام قبلاً ثبت شده است", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    return;
                }

                var insertCount = dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.SALA_DTL (SAL_NAME, PSAL_NAME, GRSAL, IDD, HES, EMZA)
                      VALUES (@SAL_NAME, @PSAL_NAME, @GRSAL, @IDD, @HES, @EMZA)",
                    new
                    {
                        SAL_NAME = encodedName,
                        PSAL_NAME = encodedPassword,
                        GRSAL = selectedGroup,
                        IDD = newId,
                        HES = string.IsNullOrWhiteSpace(hesValue) ? null : hesValue,
                        EMZA = _signatureBytes
                    });

                if (insertCount <= 0)
                {
                    universControl.PopNotifyShowUp("ثبت کاربر جدید انجام نشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    return;
                }

                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.SAL_CHEK (USERCO, OBJECT, RUN)
                      SELECT @IDD AS USERCO, IDH AS OBJECT, 0 AS RUN FROM dbo.TFORMS",
                    new { IDD = newId });

                dbms.DoExecuteSQL("INSERT INTO dbo.SIGN (USERCO) VALUES (@IDD)", new { IDD = newId });

                new Msgwin(false, "کاربر جدید با موفقیت ثبت شد", "#FF1AAA2C").ShowDialog();
                //DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShowUp($"خطا در ذخیره اطلاعات: {ex.Message}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 5);
            }
        }

        private bool CheckTabletLicenses(int selectedGroup)
        {
            if (selectedGroup != 11 && selectedGroup != 12)
            {
                return true;
            }

            string tinData = Baseknow.tindata ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tinData) || tinData.Length < 7)
            {
                universControl.PopNotifyShowUp("برای استفاده از این گروه لازم است مجوز معتبر تهیه شود", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
                return false;
            }

            int allowedCount = 0;
            try
            {
                if (selectedGroup == 11)
                {
                    allowedCount = ParseSafeSubstring(tinData, 0, 3);
                }
                else if (selectedGroup == 12)
                {
                    allowedCount = ParseSafeSubstring(tinData, 4, 3);
                }

                int currentCount = dbms.DoGetDataSQL<int?>(
                    "SELECT COUNT(1) FROM dbo.SALA_DTL WHERE GRSAL = @GRP",
                    new { GRP = selectedGroup }).FirstOrDefault() ?? 0;

                if (allowedCount > 0 && currentCount >= allowedCount)
                {
                    string message = selectedGroup == 11
                        ? "تعداد کاربران تبلت تکمیل است؛ امکان تعریف کاربر جدید وجود ندارد"
                        : "تعداد کاربران سفیر مدیران تکمیل است؛ امکان تعریف کاربر جدید وجود ندارد";
                    universControl.PopNotifyShowUp(message, Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
                    return false;
                }

                if (allowedCount <= 0)
                {
                    universControl.PopNotifyShowUp("برای استفاده از این گروه لازم است مجوز معتبر تهیه شود", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
                    return false;
                }
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShowUp($"خطا در بررسی مجوز: {ex.Message}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 4);
                return false;
            }

            return true;
        }

        private static int ParseSafeSubstring(string source, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(source) || source.Length < startIndex + length)
            {
                return 0;
            }

            string part = source.Substring(startIndex, length);
            return int.TryParse(part, out var value) ? value : 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
    }

    public class HesabItem
    {
        public string hes { get; set; }
        public string nam { get; set; }
        public string DisplayName => string.IsNullOrWhiteSpace(nam) ? hes : $"{nam} ({hes})";
    }
}