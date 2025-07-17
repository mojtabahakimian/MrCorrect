using AUTO_BAZ.Functions;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AUTO_BAZ.HelperWins
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
        /// <summary>
        /// Red :#FFFF0000   Black : #FF000000
        /// </summary>
        /// <param name="_isyesno">آیا به صورت بله یا خیر است ؟</param>
        /// <param name="_txtmsg">متن پیغام شما</param>
        /// <param name="_rang">رنگ دلخواه متن شما</param>
        /// <param name="_isbigtxt">آیا متن زیادی دارد ؟</param>
        public Msgwin(bool _isyesno, string _txtmsg, string _rang = "", bool _isbigtxt = false)
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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Topmost = false;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            UIElement uie = e.OriginalSource as UIElement;
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
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
        }

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
