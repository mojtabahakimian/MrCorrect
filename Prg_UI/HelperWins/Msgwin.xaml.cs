using MaterialDesignThemes.Wpf;
using Prg_UI.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.Generaly.CL_Generaly;

namespace Prg_UI.HelperWins
{

    public partial class Msgwin : Window
    {
        #region HEAD
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

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        private static Msgwin activeInstance;
        public bool IsYesNo { get; set; }
        public string TxtMsg { get; set; }
        public string Rang { get; set; }
        public bool IsBigTxt { get; set; }

        public class MSGCAPTIONMODEL
        {
            public string? YES_CAPTION { get; set; }
            public string? NO_CAPTION { get; set; }
            public string? OK_CAPTION { get; set; }
        }

        /// <summary>
        /// Red :#FFFF0000   Black : #FF000000
        /// </summary>
        /// <param name="_isyesno">آیا به صورت بله یا خیر است ؟</param>
        /// <param name="_txtmsg">متن پیغام شما</param>
        /// <param name="_rang">رنگ دلخواه متن شما</param>
        /// <param name="_isbigtxt">آیا متن زیادی دارد ؟</param>
        public Msgwin(bool _isyesno, string _txtmsg, string _rang = "", bool _isbigtxt = false, MSGCAPTIONMODEL? _MSGCAP_ = null)
        {
            IsYesNo = _isyesno;
            TxtMsg = _txtmsg;
            Rang = _rang;
            IsBigTxt = _isbigtxt;

            if (activeInstance != null)
                activeInstance.Close();

            activeInstance = this;
            activeInstance.Closed += (sender, e) => activeInstance = null;

            InitializeComponent();

            if (_MSGCAP_ != null)
            {
                if (_MSGCAP_?.YES_CAPTION != null)
                {
                    Btn_yes.Content = _MSGCAP_.YES_CAPTION;
                }
                if (_MSGCAP_?.NO_CAPTION != null)
                {
                    Btn_no.Content = _MSGCAP_.NO_CAPTION;
                }
                if (_MSGCAP_?.OK_CAPTION != null)
                {
                    Btn_SeeOK.Content = _MSGCAP_.OK_CAPTION;
                }
            }

            //فقط تایید OK
            if (IsYesNo != true)
            {
                if (IsBigTxt == true)
                {
                    Btn_yes.Visibility = Visibility.Hidden;
                    Btn_no.Visibility = Visibility.Hidden;
                    MsgTextNote.Visibility = Visibility.Hidden;

                    Btn_SeeOK.Visibility = Visibility.Visible;
                    MsgTextBig.Visibility = Visibility.Visible;
                    MsgTextBig.Text = TxtMsg;
                }
                else
                {
                    Btn_yes.Visibility = Visibility.Hidden;
                    Btn_no.Visibility = Visibility.Hidden;
                    MsgTextBig.Visibility = Visibility.Hidden;

                    Btn_SeeOK.Visibility = Visibility.Visible;
                    MsgTextNote.Visibility = Visibility.Visible;
                    MsgTextNote.Text = TxtMsg;
                }

                if (!string.IsNullOrEmpty(Rang))
                {
                    var bc = new BrushConverter();//#FFFF0000
                    MsgTextNote.Foreground = (Brush)bc.ConvertFrom(Rang);
                }
            }
            //بله یا خیر Yes and No
            if (IsYesNo == true)
            {
                if (IsBigTxt == true)
                {
                    //بله یا خیر با متن بزرگ که از سمت راست شروع میشه
                    Btn_SeeOK.Visibility = Visibility.Hidden;
                    MsgTextNote.Visibility = Visibility.Hidden;

                    MsgTextBig.Visibility = Visibility.Visible;
                    Btn_yes.Visibility = Visibility.Visible;
                    Btn_no.Visibility = Visibility.Visible;
                    MsgTextBig.Text = TxtMsg;
                }
                else
                {
                    //نمایش متن به صورت بزرگ اما مدیریت شده نشان داده شود
                    Btn_SeeOK.Visibility = Visibility.Hidden;
                    MsgTextBig.Visibility = Visibility.Hidden;

                    Btn_yes.Visibility = Visibility.Visible;
                    Btn_no.Visibility = Visibility.Visible;
                    MsgTextNote.Visibility = Visibility.Visible;
                    MsgTextNote.Text = TxtMsg;
                }
                if (!string.IsNullOrEmpty(Rang))
                {
                    var bc = new BrushConverter();//#FFFF0000
                    MsgTextBig.Foreground = (Brush)bc.ConvertFrom(Rang);
                }
            }

            try { ThreadSafeProperties.IsStillWorking = true; } catch { }
        }


        // ...

        public static void ShowSequential(string message)
        {
            if (activeInstance != null)
                activeInstance.Close();

            activeInstance = new Msgwin(false, message);
            activeInstance.Closed += (sender, e) => activeInstance = null;

            activeInstance.ShowDialog();
        }

        //For YesorNO {
        private void Btn_yes_Click(object sender, RoutedEventArgs e)
        {
            //Say Yes
            DialogResult = true; // YES
            Close();
        }
        private void Btn_no_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // NO
            Close();
        }
        //For YesorNO }

        //I Saw its OK
        private void Btn_SeeOK_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;


            //this.Activate();
            //this.Focus();
            //Keyboard.Focus(this);

            //ForceWindowToForeground();

            //if (MsgTextNote.Visibility == Visibility.Visible && MsgTextNote.IsEnabled)
            //{
            //    MsgTextNote.Focus();
            //}
            //else if (MsgTextBig.Visibility == Visibility.Visible && MsgTextBig.IsEnabled)
            //{
            //    MsgTextBig.Focus();
            //}
        }
        /// <summary>
        /// Force the window to come to foreground even if system focus rules are strict.
        /// </summary>
        private void ForceWindowToForeground()
        {
            var interopHelper = new WindowInteropHelper(this);
            IntPtr hwnd = interopHelper.Handle;

            if (hwnd == IntPtr.Zero)
                return;

            SetForegroundWindow(hwnd);
            ShowWindow(hwnd, SW_RESTORE);
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Topmost = false;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            UIElement? uie = e.OriginalSource as UIElement;
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                try
                {
                    if (Btn_yes.IsFocused)
                    {
                        Btn_yes_Click(null, null);
                    }
                    else if (Btn_SeeOK.IsFocused)
                    {
                        Btn_SeeOK_Click(null, null);
                    }
                    else if (Btn_no.IsFocused)
                    {
                        Btn_no_Click(null, null);
                    }
                    else
                    {
                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                }
                catch { }
            }
        }

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            try { ThreadSafeProperties.IsStillWorking = false; } catch { }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }
    }

}
