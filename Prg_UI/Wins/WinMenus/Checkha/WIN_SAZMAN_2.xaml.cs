using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Wins.WinSamplesEmpty.Window4;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for WIN_SAZMAN_2.xaml
    /// </summary>
    public partial class WIN_SAZMAN_2 : Window
    {
        public WIN_SAZMAN_2()
        {
            InitializeComponent();

            this.DataContext = this;
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
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

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

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            var sql = $"SELECT lECOL1, lECOL2, lECOL3, lECOL4, lKCOL1 FROM SAZMAN";
            var result = dbms.DoGetDataSQL<SAZMAN>(sql).FirstOrDefault();

            lECOL1.Text = result.LECOL1;
            lECOL2.Text = result.LECOL2;
            lECOL3.Text = result.LECOL3;
            lECOL4.Text = result.LECOL4;
            lKCOL1.Text = result.LKCOL1;

            FILL_ALL_COMBOBOXES();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

        }
        private void FILL_ALL_COMBOBOXES()
        {
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("UPDATE SAZMAN SET ");
            sqlBuilder.Append("lECOL1 = @lECOL1, ");
            sqlBuilder.Append("lECOL2 = @lECOL2, ");
            sqlBuilder.Append("lECOL3 = @lECOL3, ");
            sqlBuilder.Append("lECOL4 = @lECOL4, ");
            sqlBuilder.Append("lKCOL1 = @lKCOL1 ");
            var parameters = new
            {
                lECOL1 = lECOL1.Text,
                lECOL2 = lECOL2.Text,
                lECOL3 = lECOL3.Text,
                lECOL4 = lECOL4.Text,
                lKCOL1 = lKCOL1.Text
            };

            var rowsAffected = dbms.DoExecuteSQL(sqlBuilder.ToString(), parameters);
            if (rowsAffected > 0)
            {
                this.Close();
            }
            else
            {
                new Msgwin(false, "ذخیره انجام نشد !").Show();
                return;
            }

            ChangeIsHappend = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
