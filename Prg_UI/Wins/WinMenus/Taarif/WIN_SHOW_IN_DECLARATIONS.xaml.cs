using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    /// <summary>
    /// Interaction logic for WIN_SHOW_IN_DECLARATIONS.xaml
    /// </summary>
    public partial class WIN_SHOW_IN_DECLARATIONS : Window
    {
        public WIN_SHOW_IN_DECLARATIONS(int pgid, string code)
        {
            InitializeComponent();
            this.DataContext = this;
            _pgid = pgid;
            _code = code;
            LoadData();
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

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            FILL_ALL_COMBOBOXES();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        private int _pgid;
        private string _code;

        public class PriceDeclarationDto
        {
            public int PEPID { get; set; }
            public string PEPNAME { get; set; }
            public int PEPDATE { get; set; }
            public double PRICE1 { get; set; }
        }

        public class DiscountDeclarationDto
        {
            public int PEID { get; set; }
            public string PENAME { get; set; }
            public int PEDATE { get; set; }
            public float EXCEPTION_TF1 { get; set; }
            public float EXCEPTION_TF2 { get; set; }
        }

       

        private void LoadData()
        {
            // Load Price Declarations
            string priceSql = @"
                SELECT DISTINCT H.PEPID, H.PEPNAME, H.PEPDATE, D.PRICE1 
                FROM PRICE_ELAMIE H 
                INNER JOIN PRICE_ELAMIE_DTL D ON H.PEPID = D.PEPID 
                WHERE D.PGID = @PGID
                ORDER BY H.PEPDATE DESC";

            var priceData = dbms.DoGetDataSQL<PriceDeclarationDto>(priceSql, new { PGID = _pgid }).ToList();
            DG_PRICE.ItemsSource = priceData;

            // Load Discount Declarations (Exceptions)
            string discountSql = @"
                SELECT DISTINCT H.PEID, H.PENAME, H.PEDATE, E.EXCEPTION_TF1, E.EXCEPTION_TF2 
                FROM PRICE_ELAMIETF H 
                INNER JOIN PRICE_ELAMIETF_DTL D ON H.PEID = D.PEID 
                INNER JOIN PRICE_ELAMIETF_EXCEPTION E ON D.PETID = E.PETID 
                WHERE E.CODE = @CODE
                ORDER BY H.PEDATE DESC";

            var discountData = dbms.DoGetDataSQL<DiscountDeclarationDto>(discountSql, new { CODE = _code }).ToList();
            DG_DISCOUNT.ItemsSource = discountData;
        }
    }
}
