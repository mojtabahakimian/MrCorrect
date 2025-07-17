using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Wins.WinMenus.HESABDARI
{
    public partial class DEED_SERCH_CREATE : Window
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
        public DEED_SERCH_CREATE(string _QUERY_)
        {
            InitializeComponent();

            TQUERY = _QUERY_;

            this.DataContext = this;
        }
        public string TQUERY { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<SEARCH_ON_SANAD> SEARCH_ON_SANAD_DATA { get; set; } = new ObservableCollection<SEARCH_ON_SANAD>();
        public bool NowIsReady { get; private set; }

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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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
            FILL_ALL_COMBOBOXES();

            ReGetMasterData();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        public void ReGetMasterData()
        {
            if (string.IsNullOrEmpty(TQUERY))
            {
                SEARCH_ON_SANAD_DATA?.Clear();
                var MasterHead = dbms.DoGetDataSQL<SEARCH_ON_SANAD>($" SELECT * FROM dbo.[SEARCH_ON SANAD] ").ToList();
                foreach (var item in MasterHead)
                {
                    SEARCH_ON_SANAD_DATA.Add(item);
                }
            }
            else
            {
                SEARCH_ON_SANAD_DATA?.Clear();
                var MasterHead = dbms.DoGetDataSQL<SEARCH_ON_SANAD>($"  SELECT * FROM dbo.[SEARCH_ON SANAD] WHERE {TQUERY} ").ToList();
                foreach (var item in MasterHead)
                {
                    SEARCH_ON_SANAD_DATA.Add(item);
                }
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
        }
    }
}
