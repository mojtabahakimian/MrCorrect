using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinSetting
{
    /// <summary>
    /// Interaction logic for USER_CHANGE.xaml
    /// </summary>
    public partial class USER_CHANGE : Window
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

        public USER_CHANGE()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        UniversControl universControl = new UniversControl();

        public Visual I_AM_USER_CHANGE { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "NEWPASSWORD", new WindowInteropHelper(this).Handle);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Command5.IsFocused)
                {
                    //Enter Key Continue
                }
                else
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {

            // 1) خواندن و نرمال‌سازی ورودی‌ها
            string userNameInput = (usname?.Text ?? string.Empty).Trim().Replace("ي", "ی").Replace("ك", "ک");
            string passwordInput = (pass1?.Text ?? string.Empty)
                                    .Trim()
                                    .Replace("ي", "ی")
                                    .Replace("ك", "ک");
            passwordInput = passwordInput.Trim().Replace("ي", "ی").Replace("ك", "ک");
            string newUserInput = (NEWUSER?.Text ?? string.Empty).Trim().Replace("ي", "ی").Replace("ك", "ک");

            if (string.IsNullOrWhiteSpace(userNameInput))
            {
                new Msgwin(false, "خطا: نام كاربر صحيح نيست").Show();
                return;
            }

            if (passwordInput.Length > 40)
            {
                new Msgwin(false, "خطا: طول رمز بايد كمتر از 40 كاراكتر باشد مانند: 110ab").Show();
                return;
            }

            // 2) تشخیص «مدیر سیستم»
            string currentUser = Convert.ToString(CL_HESABDARI.UCurrentUser()) ?? string.Empty;
            currentUser = currentUser.Replace("ي", "ی").Replace("ك", "ک");
            bool currentIsAdmin = currentUser.StartsWith("مدیر سیستم", StringComparison.OrdinalIgnoreCase);

            bool targetIsAdminByName =
                userNameInput.Equals("مديرسيستم", StringComparison.OrdinalIgnoreCase) ||
                userNameInput.Equals("مدیر سیستم", StringComparison.OrdinalIgnoreCase) ||
                userNameInput.Equals("Administer", StringComparison.OrdinalIgnoreCase);

            if (currentIsAdmin && targetIsAdminByName)
            {
                new Msgwin(false, "اين كاربر قابل حذف يا تغيير نام نيست").Show();
                return;
            }

            // 3) جستجوی رکورد کاربر هدف در SALA_DTL بر اساس SAL_NAME (رعایت کدگذاری پروژه)
            // در MrCorrect کاربرها را برای نمایش DECODEUN می‌کنیم، ولی در دیتابیس SAL_NAME به‌صورت کُد شده نگه‌داری می‌شود. :contentReference[oaicite:3]{index=3}
            string encodedUserName = CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(userNameInput)).Trim();
            var row = dbms.DoGetDataSQL<SALA_DTL>(
                "SELECT TOP(1) SAL_NAME, PSAL_NAME, IDD FROM SALA_DTL WHERE SAL_NAME = @SN",
                new { SN = encodedUserName }).FirstOrDefault();

            if (row == null)
            {
                new Msgwin(false, "نام كاربر يا رمز عبور صحيح نمي باشد.").Show();
                return;
            }

            // 4) اگر کاربر جاری مدیر سیستم نیست، باید رمز را چک کنیم (مطابق VBA: pass1 = DECODEPS(PSAL_NAME))
            if (!currentIsAdmin)
            {
                string realPassword = CL_HESABDARI.DECODEPS(row.PSAL_NAME?.ToString() ?? string.Empty); // الگوی پروژه در لاگین هم دیده می‌شود. :contentReference[oaicite:4]{index=4}
                if (!string.Equals(realPassword, passwordInput, StringComparison.Ordinal))
                {
                    new Msgwin(false, "نام كاربر يا رمز عبور صحيح نمي باشد.").Show();
                    return;
                }

                // حتی برای غیرمدیر‌ها هم هدف «مدیر سیستم» نباید تغییر/حذف شود
                if (targetIsAdminByName)
                {
                    new Msgwin(false, "اين كاربر قابل حذف يا تغيير نام نيست").Show();
                    return;
                }
            }

            // 5) حذف یا تغییر نام
            if (string.IsNullOrWhiteSpace(newUserInput))
            {
                // حذف
                var aff = dbms.DoExecuteSQL("DELETE FROM SALA_DTL WHERE IDD = @ID", new { ID = row.IDD });
                if (aff <= 0)
                {
                    new Msgwin(false, "عمليات ناموفق بود").Show();
                }
                else
                {
                    new Msgwin(false, "كاربر حذف شد").Show();
                }
            }
            else
            {
                // تغییر نام → ذخیره به فرم کُد شده (CODESAL(Fixp(...)))
                string encodedNewName = CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(newUserInput)).Trim();
                var aff = dbms.DoExecuteSQL("UPDATE SALA_DTL SET SAL_NAME = @SN WHERE IDD = @ID",
                                            new { SN = encodedNewName, ID = row.IDD });
                if (aff <= 0)
                {
                    new Msgwin(false, "عمليات ناموفق بود").Show();
                }
                else
                {
                    new Msgwin(false, "نام كاربر با موفقيت اصلاح شد").Show();
                }
            }

            // در VBA در یکی از مسیرها فرم بسته می‌شد؛ اگر همین رفتار را می‌خواهی:
            this.Close();

        }
    }
}
