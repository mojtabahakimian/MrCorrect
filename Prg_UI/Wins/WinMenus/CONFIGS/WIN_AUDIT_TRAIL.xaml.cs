using Dapper;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.AUDIT;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    /// <summary>
    /// مشاهده و ردیابی خطی فعالیت کاربران.
    ///
    /// همه چیز سمت سرور فیلتر و صفحه‌بندی می‌شود؛ هیچ‌وقت کل جدول به حافظه
    /// نمی‌آید. صفحه‌بندی به‌جای OFFSET با کلید (LOG_ID &lt; آخرین) انجام
    /// می‌شود تا صفحه‌های عمیق هم به همان سرعت صفحه‌ی اول باشند.
    /// </summary>
    public partial class WIN_AUDIT_TRAIL : Window
    {
        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null) packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null) packIcon.Kind = PackIconKind.WindowRestore;
                    break;
            }
        }

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        //Header Window End;
        #endregion

        private const int PageSize = 300;

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private long? _lastLogId;
        private bool _busy;

        public ObservableCollection<AuditTrailRow> Rows { get; } = new ObservableCollection<AuditTrailRow>();

        public WIN_AUDIT_TRAIL()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// نام فرم در جدول TFORMS. برای فعال شدن این پنجره باید یک ردیف با
        /// همین نام در TFORMS و دسترسی متناظر در SAL_CHEK ساخته شود.
        /// </summary>
        private const string PermissionFormName = "AUDITTRAIL";

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // این پنجره فعالیت همه‌ی کاربران را به‌همراه IP، نام کامپیوتر و
            // ورودهای ناموفق نشان می‌دهد، پس باید مجوزدار باشد.
            // LETSGO وقتی ردیف دسترسی وجود نداشته باشد false برمی‌گرداند
            // (fail-closed)، پس تا وقتی مدیر دسترسی را تعریف نکرده، هیچ‌کس
            // نمی‌تواند سوابق بقیه را ببیند.
            if (!CL_HESABDARI.LETSGO(PermissionFormName))
            {
                Prg_Proccessy.AUDIT.Audit.Security(
                    AuditAction.AuditViewed,
                    "تلاش برای مشاهده‌ی سوابق بدون دسترسی",
                    formName: this.GetType().Name);

                new Msgwin(false,
                    $"دسترسی مشاهده‌ی سوابق برای شما تعریف نشده است.\n" +
                    $"مدیر سیستم باید فرم «{PermissionFormName}» را در TFORMS و دسترسی آن را در SAL_CHEK تعریف کند.")
                    .ShowDialog();

                this.Close();
                return;
            }

            FillStaticCombos();

            // بازه‌ی پیش‌فرض: از ابتدای امروز.
            var today = DateTime.Now;
            TXT_FROM.Text = ToShamsiText(today);
            TXT_TO.Text = ToShamsiText(today);

            await FillUsersAsync();

            // مشاهده‌ی سوابق خودش یک رویداد حساس است و ثبت می‌شود.
            Audit.Security(AuditAction.AuditViewed, "مشاهده‌ی سوابق فعالیت کاربران",
                           formName: this.GetType().Name);

            await SearchAsync(reset: true);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
            if (e.Key == Key.F5) { e.Handled = true; _ = SearchAsync(reset: true); }
        }

        private void FillStaticCombos()
        {
            CMB_CATEGORY.ItemsSource = new List<LookupItem>
            {
                new LookupItem(null, "همه"),
                new LookupItem((byte)AuditCategory.Navigation, "باز کردن فرم"),
                new LookupItem((byte)AuditCategory.Data,       "تغییر داده"),
                new LookupItem((byte)AuditCategory.Business,   "عملیات کاری"),
                new LookupItem((byte)AuditCategory.Report,     "چاپ و خروجی"),
                new LookupItem((byte)AuditCategory.Auth,       "ورود و خروج"),
                new LookupItem((byte)AuditCategory.Security,   "امنیتی"),
            };
            CMB_CATEGORY.SelectedIndex = 0;

            CMB_ACTION.ItemsSource = new List<TextLookupItem>
            {
                new TextLookupItem(null, "همه"),
                new TextLookupItem(AuditAction.OpenForm, "باز کردن فرم"),
                new TextLookupItem(AuditAction.Insert,   "ثبت"),
                new TextLookupItem(AuditAction.Update,   "ویرایش"),
                new TextLookupItem(AuditAction.Delete,   "حذف"),
                new TextLookupItem(AuditAction.Sign,     "امضا"),
                new TextLookupItem(AuditAction.Unsign,   "برداشتن امضا"),
                new TextLookupItem(AuditAction.Print,    "چاپ"),
                new TextLookupItem(AuditAction.Preview,  "پیش‌نمایش"),
                new TextLookupItem(AuditAction.Export,   "خروجی"),
                new TextLookupItem(AuditAction.Convert,  "تبدیل"),
                new TextLookupItem(AuditAction.Confirm,  "تایید"),
                new TextLookupItem(AuditAction.Login,    "ورود"),
                new TextLookupItem(AuditAction.LoginFailed, "ورود ناموفق"),
                new TextLookupItem(AuditAction.Logout,   "خروج"),
            };
            CMB_ACTION.SelectedIndex = 0;
        }

        /// <summary>
        /// فهرست کاربران از خود جدول نشست خوانده می‌شود، نه از SALA_DTL؛
        /// این‌طور فقط کاربرانی که واقعاً فعالیت داشته‌اند نمایش داده می‌شوند
        /// و نیازی به رمزگشایی نام کاربری هم نیست.
        /// </summary>
        private async Task FillUsersAsync()
        {
            try
            {
                var users = (await dbms.DoGetDataSQLAsync<UserItem>(
                    @"SELECT DISTINCT [USER_ID] AS IDD, [USER_NAME] AS SAL_NAME
                        FROM [dbo].[SYS_AUDIT_SESSION]
                       WHERE [USER_ID] IS NOT NULL
                       ORDER BY [USER_NAME]")).ToList();

                users.Insert(0, new UserItem { IDD = null, SAL_NAME = "همه کاربران" });
                CMB_USER.ItemsSource = users;
                CMB_USER.SelectedIndex = 0;
            }
            catch (Exception)
            {
                CMB_USER.ItemsSource = new List<UserItem> { new UserItem { IDD = null, SAL_NAME = "همه کاربران" } };
                CMB_USER.SelectedIndex = 0;
            }
        }

        private async void BTN_SEARCH_Click(object sender, RoutedEventArgs e) => await SearchAsync(reset: true);

        private async void BTN_MORE_Click(object sender, RoutedEventArgs e) => await SearchAsync(reset: false);

        private async Task SearchAsync(bool reset)
        {
            if (_busy) return;
            _busy = true;

            try
            {
                BTN_SEARCH.IsEnabled = false;
                BTN_MORE.IsEnabled = false;
                LBL_STATUS.Text = "در حال خواندن ...";

                if (reset)
                {
                    Rows.Clear();
                    _lastLogId = null;
                }

                // تاریخ نامعتبر نباید بی‌صدا به «امروز» تبدیل شود: کاربر
                // «1404/1/1» را می‌بیند ولی نتیجه‌ی امروز را می‌گیرد و فکر
                // می‌کند رکوردی وجود ندارد.
                var from = ParseShamsi(TXT_FROM.Text);
                var to = ParseShamsi(TXT_TO.Text);

                if (from is null || to is null)
                {
                    LBL_STATUS.Text = "تاریخ نامعتبر است. قالب درست: 1405/05/17";
                    return;
                }
                if (from > to)
                {
                    LBL_STATUS.Text = "«از تاریخ» بزرگ‌تر از «تا تاریخ» است.";
                    return;
                }

                var fromDate = from.Value;
                var toDate = to.Value.AddDays(1);

                var args = new
                {
                    Take = PageSize,
                    UserId = (CMB_USER.SelectedValue as int?),
                    From = fromDate,
                    To = toDate,
                    Category = (CMB_CATEGORY.SelectedValue as byte?),
                    Action = string.IsNullOrWhiteSpace(CMB_ACTION.SelectedValue as string)
                             ? null : (string?)CMB_ACTION.SelectedValue,
                    Doc = BuildLike(TXT_DOC.Text),
                    Search = BuildLike(TXT_SEARCH.Text),
                    OnlySensitive = CHK_SENSITIVE.IsChecked == true,
                    AfterId = _lastLogId,
                };

                // OPTION (RECOMPILE): چون بیشتر شرط‌ها اختیاری‌اند، یک نقشه‌ی
                // اجرای ذخیره‌شده برای همه‌ی ترکیب‌ها بد است. با RECOMPILE
                // برای هر جستجو نقشه‌ی مناسب همان فیلترها ساخته می‌شود.
                const string sql = @"
SELECT TOP (@Take)
       [LOG_ID], [AT_CLIENT], [AT_SERVER], [DATE_S], [TIME_S],
       [USER_ID], [USER_NAME], [CATEGORY], [SEVERITY], [ACTION],
       [ENTITY], [ENTITY_KEY], [FORM_NAME], [TITLE], [DETAIL],
       [IS_SUCCESS], [SESSION_ID], [MACHINE_NAME], [CLIENT_IP],
       [WIN_USER], [APP_VERSION]
  FROM [dbo].[VW_SYS_AUDIT_TIMELINE]
 WHERE [AT_SERVER] >= @From
   AND [AT_SERVER] <  @To
   AND (@UserId   IS NULL OR [USER_ID]  = @UserId)
   AND (@Category IS NULL OR [CATEGORY] = @Category)
   AND (@Action   IS NULL OR [ACTION]   = @Action)
   AND (@Doc      IS NULL OR [ENTITY_KEY] LIKE @Doc OR [ENTITY] LIKE @Doc)
   AND (@Search   IS NULL OR [TITLE]      LIKE @Search)
   AND (@OnlySensitive = 0 OR [SEVERITY] = 3)
   AND (@AfterId  IS NULL OR [LOG_ID] < @AfterId)
 ORDER BY [LOG_ID] DESC
 OPTION (RECOMPILE)";

                var page = (await dbms.DoGetDataSQLAsync<AuditTrailRow>(sql, args)).ToList();

                foreach (var r in page) Rows.Add(r);

                if (page.Count > 0) _lastLogId = page[page.Count - 1].LOG_ID;

                BTN_MORE.IsEnabled = page.Count == PageSize;

                if (page.Count == 0 && Rows.Count == 0)
                {
                    // «رکوردی یافت نشد» وقتی ساختار اصلاً ساخته نشده گمراه‌کننده
                    // است؛ مدیر باید بفهمد مشکل از دسترسی دیتابیس است نه از فیلتر.
                    LBL_STATUS.Text = AuditService.SchemaReady
                        ? "رکوردی یافت نشد."
                        : "رکوردی یافت نشد — ساختار جدول‌های سابقه ساخته نشده است. " +
                          "کاربر SQL باید دسترسی ایجاد جدول داشته باشد، یا مایگریشن ScriptSqly اجرا شود.";
                }
                else
                {
                    LBL_STATUS.Text =
                        $"{Rows.Count:N0} رکورد نمایش داده شد" +
                        (BTN_MORE.IsEnabled ? " (رکورد بیشتری هست)" : " (پایان نتایج)") +
                        (AuditService.DroppedCount > 0
                            ? $"  |  ⚠ {AuditService.DroppedCount:N0} رویداد در این نشست به‌دلیل پر شدن صف ثبت نشد"
                            : string.Empty);
                }
            }
            catch (Exception ex)
            {
                LBL_STATUS.Text = "خطا در خواندن سوابق: " + ex.Message;

                // اگر قبلاً صفحه‌ای خوانده شده بود، کاربر باید بتواند ادامه
                // را دوباره امتحان کند؛ وگرنه دکمه برای همیشه غیرفعال می‌ماند.
                BTN_MORE.IsEnabled = _lastLogId.HasValue;
            }
            finally
            {
                BTN_SEARCH.IsEnabled = true;
                _busy = false;
            }
        }

        // ── تاریخ شمسی ────────────────────────────────────────────────────

        private static string ToShamsiText(DateTime value)
        {
            var n = Audit.ToPersianDateNumber(value);
            return $"{n / 10000:0000}/{(n / 100) % 100:00}/{n % 100:00}";
        }

        /// <summary>تبدیل «1405/05/17» یا «14050517» به تاریخ میلادی.</summary>
        /// <summary>
        /// آماده کردن متن کاربر برای LIKE.
        ///
        /// ٪ و _ و [ در LIKE معنای ویژه دارند. بدون escape، جست‌وجوی «٪» همه‌ی
        /// سطرها را برمی‌گرداند و «_» هر تک‌نویسه‌ای را می‌گرفت — یعنی نتیجه‌ی
        /// جست‌وجو بی‌ربط می‌شد. ارقام فارسی هم مثل بقیه‌ی فرم یکسان‌سازی
        /// می‌شوند تا جست‌وجوی «۱۲۳۴» سند ۱۲۳۴ را پیدا کند.
        /// </summary>
        private static string? BuildLike(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var t = CL_LMethods.NormalizeDigits(text).Trim();
            var sb = new StringBuilder(t.Length + 8);
            sb.Append('%');
            foreach (var c in t)
            {
                // فقط همین سه نویسه معنای ویژه دارند. با escape شدن «[» دیگر
                // هیچ bracket expressionی باز نمی‌شود، پس «]» و «^» خودبه‌خود
                // نویسه‌ی معمولی‌اند و نباید دستکاری شوند (وگرنه «[^]» ساخته
                // می‌شد که خودش الگوی ناقص است).
                if (c is '%' or '_' or '[') sb.Append('[').Append(c).Append(']');
                else sb.Append(c);
            }
            sb.Append('%');
            return sb.ToString();
        }

        private static DateTime? ParseShamsi(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            // char.IsDigit برای ارقام فارسی (۱۲۳) هم true است ولی int.TryParse
            // آن‌ها را نمی‌پذیرد. کاربر با صفحه‌کلید فارسی «۱۴۰۵/۰۵/۱۷» تایپ
            // می‌کرد و پیام «تاریخ نامعتبر» می‌گرفت. NormalizeDigits همان تابعی
            // است که بقیه‌ی نرم‌افزار برای همین کار استفاده می‌کند.
            var normalized = CL_LMethods.NormalizeDigits(text);
            var digits = new string(normalized.Where(c => c >= '0' && c <= '9').ToArray());
            if (digits.Length != 8) return null;
            if (!int.TryParse(digits, out var n)) return null;

            return Audit.FromPersianDateNumber(n);
        }

        // ── مدل‌ها ────────────────────────────────────────────────────────

        public sealed class UserItem
        {
            public int? IDD { get; set; }
            public string? SAL_NAME { get; set; }
        }

        public sealed class LookupItem
        {
            public LookupItem(byte? value, string title) { Value = value; Title = title; }
            public byte? Value { get; }
            public string Title { get; }
        }

        public sealed class TextLookupItem
        {
            public TextLookupItem(string? value, string title) { Value = value; Title = title; }
            public string? Value { get; }
            public string Title { get; }
        }
    }

    /// <summary>یک سطر از خط زمانی. فقط برای نمایش استفاده می‌شود.</summary>
    public sealed class AuditTrailRow
    {
        public long LOG_ID { get; set; }
        public DateTime? AT_CLIENT { get; set; }
        public DateTime AT_SERVER { get; set; }
        public int? DATE_S { get; set; }
        public int? TIME_S { get; set; }
        public int? USER_ID { get; set; }
        public string? USER_NAME { get; set; }
        public byte CATEGORY { get; set; }
        public byte SEVERITY { get; set; }
        public string? ACTION { get; set; }
        public string? ENTITY { get; set; }
        public string? ENTITY_KEY { get; set; }
        public string? FORM_NAME { get; set; }
        public string? TITLE { get; set; }
        public string? DETAIL { get; set; }
        public bool IS_SUCCESS { get; set; }
        public Guid? SESSION_ID { get; set; }
        public string? MACHINE_NAME { get; set; }
        public string? CLIENT_IP { get; set; }
        public string? WIN_USER { get; set; }
        public string? APP_VERSION { get; set; }

        public string DateText
        {
            get
            {
                // برای ردیف‌های منتقل‌شده از جدول‌های قدیمی، DATE_S خالی است
                // و تاریخ از روی زمان سرور ساخته می‌شود.
                var n = DATE_S.GetValueOrDefault() > 0
                    ? DATE_S!.Value
                    : Audit.ToPersianDateNumber(AT_CLIENT ?? AT_SERVER);
                return n <= 0 ? string.Empty : $"{n / 10000:0000}/{(n / 100) % 100:00}/{n % 100:00}";
            }
        }

        public string TimeText
        {
            get
            {
                var t = TIME_S.GetValueOrDefault() > 0
                    ? TIME_S!.Value
                    : Audit.ToTimeNumber(AT_CLIENT ?? AT_SERVER);
                return $"{t / 10000:00}:{(t / 100) % 100:00}:{t % 100:00}";
            }
        }

        public string CategoryText => CATEGORY switch
        {
            1 => "فرم",
            2 => "داده",
            3 => "عملیات",
            4 => "چاپ",
            5 => "ورود/خروج",
            6 => "امنیتی",
            _ => string.Empty,
        };

        public string ActionText => (ACTION ?? string.Empty) switch
        {
            AuditAction.OpenForm => "باز کردن فرم",
            AuditAction.CloseForm => "بستن فرم",
            AuditAction.Insert => "ثبت",
            AuditAction.Update => "ویرایش",
            AuditAction.Delete => "حذف",
            AuditAction.Sign => "امضا",
            AuditAction.Unsign => "برداشتن امضا",
            AuditAction.Approve => "تایید",
            AuditAction.Unapprove => "لغو تایید",
            AuditAction.Convert => "تبدیل",
            AuditAction.Confirm => "تایید",
            AuditAction.Cancel => "لغو",
            AuditAction.Preview => "پیش‌نمایش",
            AuditAction.Print => "چاپ",
            AuditAction.Export => "خروجی",
            AuditAction.Login => "ورود",
            AuditAction.LoginFailed => "ورود ناموفق",
            AuditAction.Logout => "خروج",
            AuditAction.PasswordChange => "تغییر رمز",
            AuditAction.PermissionChange => "تغییر دسترسی",
            AuditAction.AuditViewed => "مشاهده سوابق",
            AuditAction.ExecProcedure => "اجرای رویه",
            _ => ACTION ?? string.Empty,
        };

        public string DocumentText =>
            string.IsNullOrWhiteSpace(ENTITY_KEY)
                ? (ENTITY ?? string.Empty)
                : $"{ENTITY} {ENTITY_KEY}".Trim();
    }
}
