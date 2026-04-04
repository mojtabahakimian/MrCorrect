using DocumentFormat.OpenXml.Drawing;
using Functions;
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
    /// Interaction logic for NEWPASSWORD.xaml
    /// </summary>
    public partial class NEWPASSWORD : Window
    {
        private sealed class UserLookupItem
        {
            public int IDD { get; set; }
            public string SAL_NAME { get; set; } = string.Empty;
            public string DisplayText => $"{IDD} - {SAL_NAME}";
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

        public NEWPASSWORD()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        private bool _canResetOtherUsersPassword;
        private bool _isSyncingPasswordFields;

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

        public Visual I_AM_NEW_PASSWORD { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "NEWPASSWORD", new WindowInteropHelper(this).Handle);

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            //-- تعریف کاربر جدید : USERS = 106
            //-- تغییر نام یا حذف کاربر : USER_CHANGE = 205
            _canResetOtherUsersPassword = CL_HESABDARI.LETSGO("USERS") || CL_HESABDARI.LETSGO("USER_CHANGE");
            LoadUsersForSelection();

            OLDPASS.Focus();
        }

        private void LoadUsersForSelection()
        {
            var allUsers = dbms.DoGetDataSQL<SALA_DTL>("SELECT IDD,SAL_NAME FROM SALA_DTL WHERE IDD IS NOT NULL").ToList();
            var validUsers = allUsers
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.SAL_NAME))
                .Select(x => new UserLookupItem
                {
                    IDD = x.IDD,
                    SAL_NAME = CL_HESABDARI.DECODEUN(x.SAL_NAME) ?? string.Empty
                })
                .OrderBy(x => x.SAL_NAME)
                .ToList();

            if (!_canResetOtherUsersPassword)
            {
                validUsers = validUsers.Where(x => x.IDD == Baseknow.USERCOD).ToList();
                cmbUsers.IsEnabled = false;
            }

            cmbUsers.ItemsSource = validUsers;
            cmbUsers.SelectedValue = Baseknow.USERCOD;
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

        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            int i;
            var oldPassValue = GetPasswordValue(OLDPASS, OLDPASS_Text);
            var newPassValue = GetPasswordValue(pass1, pass1_Text);
            var repeatPassValue = GetPasswordValue(pass2, pass2_Text);

            if (string.IsNullOrEmpty(newPassValue))
            {
                newPassValue = "";
            }

            if (string.IsNullOrEmpty(repeatPassValue))
            {
                repeatPassValue = "";
            }

            if (newPassValue != repeatPassValue)
            {
                universControl.PopNotifyShow(".کلمه عبور جدید یکسان نیست! مجدد سعی کنید.", Pop1, Pop1Text1, Pop_Border1);
                //pass1 = Null;
                //pass2 = Null;
            }
            else if (cmbUsers.SelectedValue == null || !int.TryParse(cmbUsers.SelectedValue.ToString(), out int selectedUserId))
            {
                universControl.PopNotifyShow("!کاربر را انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1);
            }
            else if (newPassValue.Length > 40)
            {
                universControl.PopNotifyShow("!رمز عبور نباید بیشتر از 40 کاراکتر باشد.", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                var rst = dbms.DoGetDataSQL<SALA_DTL>("select * from SALA_DTL where idd = " + selectedUserId).FirstOrDefault();
                if (rst == null)
                {
                    universControl.PopNotifyShow("!آی دی کاربر یا رمز عبور صحیح نمی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    bool isCurrentUser = rst.IDD == Baseknow.USERCOD;
                    bool canChangeWithoutOldPassword = _canResetOtherUsersPassword && !isCurrentUser;
                    bool oldPasswordIsValid = oldPassValue == CL_HESABDARI.DECODEPS(rst.PSAL_NAME);

                    if (canChangeWithoutOldPassword || oldPasswordIsValid)
                    {
                        var NewPass = Strings.Trim(CL_HESABDARI.CODEPAL(newPassValue));
                        SALA_DTL sALA_DTL = new SALA_DTL()
                        {
                            IDD = rst.IDD,
                            PSAL_NAME = NewPass,
                        };
                        //rst.update();
                        dbms.DoExecuteSQL("UPDATE SALA_DTL SET PSAL_NAME = @PSAL_NAME WHERE IDD = @IDD", sALA_DTL);

                        _ = AuditLogger.LogActionAsync(
                            actionType: "PASSWORD_CHANGE",
                            tableName: "SALA_DTL",
                            recordId: rst.IDD.ToString(),
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $"تغییر رمز کاربر. اجراکننده: {Baseknow.UUSER} ({Baseknow.USERCOD}) | کاربر هدف: {rst.SAL_NAME} ({rst.IDD}) | روش: {(canChangeWithoutOldPassword ? "مدیر/بدون رمز قبلی" : "با رمز قبلی")}"
                        );
                    }
                    else
                    {
                        universControl.PopNotifyShow("!آی دی کاربر یا رمز عبور صحیح نمی باشد.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                    new Msgwin(false, "عملیات با موفقیت انجام شد.").ShowDialog();
                    this.Close();
                }
            }
        }

        private static string GetPasswordValue(PasswordBox passwordBox, TextBox passwordTextBox)
        {
            return passwordTextBox.Visibility == Visibility.Visible ? passwordTextBox.Text : passwordBox.Password;
        }

        private void PasswordControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPasswordFields)
            {
                return;
            }

            _isSyncingPasswordFields = true;
            OLDPASS_Text.Text = OLDPASS.Password;
            pass1_Text.Text = pass1.Password;
            pass2_Text.Text = pass2.Password;
            _isSyncingPasswordFields = false;
        }

        private void PasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPasswordFields)
            {
                return;
            }

            _isSyncingPasswordFields = true;
            OLDPASS.Password = OLDPASS_Text.Text;
            pass1.Password = pass1_Text.Text;
            pass2.Password = pass2_Text.Text;
            _isSyncingPasswordFields = false;
        }

        private void ChkShowPassword_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool showPassword = ChkShowPassword.IsChecked == true;
            OLDPASS.Visibility = showPassword ? Visibility.Collapsed : Visibility.Visible;
            pass1.Visibility = showPassword ? Visibility.Collapsed : Visibility.Visible;
            pass2.Visibility = showPassword ? Visibility.Collapsed : Visibility.Visible;

            OLDPASS_Text.Visibility = showPassword ? Visibility.Visible : Visibility.Collapsed;
            pass1_Text.Visibility = showPassword ? Visibility.Visible : Visibility.Collapsed;
            pass2_Text.Visibility = showPassword ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
