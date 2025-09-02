using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Prg_UI.HelperWins;
using System;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_Proccessy.Generaly;
using static Functions.InventoryManager;
using Stimulsoft.Base;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using Stimulsoft.Report.Components;
using Prg_UI.Wins.WinMenus.Checkha;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for F_MENU_SERILA.xaml
    /// </summary>
    public partial class F_MENU_SERILA : Window
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

        public F_MENU_SERILA()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public bool ChangeIsHappend { get; private set; } = false;

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

                //DETAIL_VOSUL_SUB.IsReadOnly = !ican;
            }
        }

        public static bool IsNull(object p)
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

        public bool NowIsReady { get; private set; }
        public object OpenArgs { get; set; }

        public string _sql_query { get; set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!Command5.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DT1.Focus();
        }

        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DT1.Text) && string.IsNullOrEmpty(DT2.Text))
            {
                _sql_query = " SELECT * FROM dbo.CHEK_PLIST";
            }
            if (string.IsNullOrEmpty(DT1.Text) && !string.IsNullOrEmpty(DT2.Text))
            {
                _sql_query = $"SELECT * FROM dbo.CHEK_PLIST WHERE (N_SERI <= {DT2.Text})";
            }
            if (!string.IsNullOrEmpty(DT1.Text) && string.IsNullOrEmpty(DT2.Text))
            {
                _sql_query = $"SELECT * FROM dbo.CHEK_PLIST WHERE (N_SERI >= {DT1.Text})";
            }
            if (!string.IsNullOrEmpty(DT1.Text) && !string.IsNullOrEmpty(DT2.Text))
            {
                _sql_query = $"SELECT * FROM dbo.CHEK_PLIST WHERE (N_SERI >=  {DT1.Text} AND N_SERI <= {DT2.Text})";
            }
            if (_sql_query is null)
            {
                Msgwin msgwin = new Msgwin(false, "پارامتر ها کافی نیست!");
                msgwin.ShowDialog();
                return;
            }
            else
            {
                new CHEK_SERIAL_CONTROL(_sql_query).ShowDialog();
            }

        }
    }
}
