using DocumentFormat.OpenXml.Drawing;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
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

        // کاربرانی که می‌توانند رمز عبور هر کاربر دیگری را بدون نیاز به رمز قدیمی تغییر دهند
        // گروه 1 = مدیران، گروه 12 = سفیر مدیران
        private bool _isPrivileged;

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

            usname.Text = Baseknow.USERCOD.ToString();
            usna.Text = CL_HESABDARI.GETUSERNAME(Convert.ToInt32(usname.Text));

            // بررسی اینکه آیا کاربر جاری دسترسی تغییر رمز دیگران را دارد
            _isPrivileged = Baseknow.UGRP == "1" || Baseknow.UGRP == "12";

            if (_isPrivileged)
            {
                // مدیران می‌توانند آی‌دی هر کاربری را وارد کنند
                usname.IsReadOnly = false;
                // رمز قدیمی برای مدیران الزامی نیست
                OLDPASS.IsEnabled = false;
                OLDPASS.Tag = "ADMIN_SKIP";
                LblOldPass.Content = "رمز عبور قدیمی (ادمین - اختیاری) : ";
                LblPrivileged.Visibility = Visibility.Visible;
                usname.Focus();
            }
            else
            {
                usname.IsReadOnly = true;
                LblPrivileged.Visibility = Visibility.Collapsed;
                OLDPASS.Focus();
            }
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
            if (string.IsNullOrEmpty(pass1.Text))
            {
                pass1.Text = "";
            }

            if (string.IsNullOrEmpty(pass2.Text))
            {
                pass2.Text = "";
            }

            if (pass1.Text != pass2.Text)
            {
                universControl.PopNotifyShow(".کلمه عبور جدید یکسان نیست! مجدد سعی کنید.", Pop1, Pop1Text1, Pop_Border1);
            }
            else if (string.IsNullOrEmpty(usname.Text))
            {
                universControl.PopNotifyShow("!کد کاربری صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
            }
            else if (pass1.Text.Length > 40)
            {
                universControl.PopNotifyShow("!رمز عبور نباید بیشتر از 40 کاراکتر باشد.", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                var rst = dbms.DoGetDataSQL<SALA_DTL>("select * from SALA_DTL where idd = " + usname.Text).FirstOrDefault();
                if (rst == null)
                {
                    universControl.PopNotifyShow("!آی دی کاربر یا رمز عبور صحیح نمی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    // مدیران بدون نیاز به رمز قدیمی مجاز به تغییر هستند
                    bool authorized = _isPrivileged || (OLDPASS.Text == CL_HESABDARI.DECODEPS(rst.PSAL_NAME));

                    if (authorized)
                    {
                        var NewPass = Strings.Trim(CL_HESABDARI.CODEPAL(pass1.Text));
                        SALA_DTL sALA_DTL = new SALA_DTL()
                        {
                            IDD = rst.IDD,
                            PSAL_NAME = NewPass,
                        };
                        dbms.DoExecuteSQL("UPDATE SALA_DTL SET PSAL_NAME = @PSAL_NAME WHERE IDD = @IDD", sALA_DTL);

                        // لاگ تغییر رمز عبور
                        string targetUserName = CL_HESABDARI.GETUSERNAME(rst.IDD);
                        string additionalInfo = _isPrivileged && rst.IDD != Baseknow.USERCOD
                            ? $"تغییر رمز توسط مدیر - کاربر هدف: {targetUserName} (ID:{rst.IDD})"
                            : $"تغییر رمز توسط خود کاربر - کاربر: {targetUserName} (ID:{rst.IDD})";

                        _ = AuditLogger.LogActionAsync(
                            actionType: "PASSWORD_CHANGE",
                            tableName: "SALA_DTL",
                            recordId: rst.IDD.ToString(),
                            oldValue: (object)null,
                            newValue: (object)null,
                            additionalInfo: additionalInfo
                        );
                    }
                    else
                    {
                        universControl.PopNotifyShow("!آی دی کاربر یا رمز عبور صحیح نمی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    }

                    this.Close();
                }
            }
        }

        private void usname_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            usna.Text = CL_HESABDARI.GETUSERNAME(Convert.ToInt32(usname.Text));
        }
    }
}
