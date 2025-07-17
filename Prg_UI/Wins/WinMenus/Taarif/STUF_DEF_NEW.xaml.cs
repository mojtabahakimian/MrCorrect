using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wins.WinMenus.Taarif
{
    public partial class STUF_DEF_NEW : Window
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
        public STUF_DEF_NEW(Visual _YOUR_VL_WIN)
        {
            InitializeComponent();

            WIN_PASSED = _YOUR_VL_WIN;

            this.DataContext = this;
        }

        #region LOCALMODEL
        public class BARCODECOMBO
        {
            public int? MPCODE { get; set; }
            public string? MPNAME { get; set; }
            public int? Expr1 { get; set; }
        }
        #endregion
        public Visual WIN_PASSED { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShowUp("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; private set; }

        private int CC;
        private string TText0;

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

        private bool can;
        public bool AllowEdits
        {
            get { return can; }
            set
            {
                can = value;

                digi.IsReadOnly = !can;
                T1.IsEnabled = can;
                T2.IsEnabled = can;
                T3.IsEnabled = can;
                T4.IsEnabled = can;
                T5.IsEnabled = can;
                T6.IsEnabled = can;
                T7.IsEnabled = can;
                T8.IsEnabled = can;
                T9.IsEnabled = can;
                T10.IsEnabled = can;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            #region Form_Open

            int i = 1;
            var rst = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT  TOP 100 PERCENT MPP, MPNAME FROM dbo.TCOD_MAP_GRP where mpp < 100 ORDER BY MPP").ToList();
            foreach (var item in rst)
            {
                if (this.FindName("L" + i) is Label _LBL_)
                {
                    _LBL_.Content = item.MPNAME; // Set CAPTION
                    _LBL_.Visibility = Visibility.Visible; // Set visibility
                }

                if (this.FindName("T" + i) is ComboBox comboBox)
                {
                    comboBox.Visibility = Visibility.Visible; // Set visibility
                }

                i = i + 1;
            }
            CC = i - 1;

            #endregion

            FILL_ALL_COMBOBOXES();

            if (WIN_PASSED is STUF_DEF_WIN)
            {
                var FORM_STUF_DEF = WIN_PASSED as STUF_DEF_WIN;
                var ExistingN_FANI = FORM_STUF_DEF.N_FANI.Text;
                if (!string.IsNullOrEmpty(ExistingN_FANI.Trim()))
                {
                    DecodeBarcode(ExistingN_FANI);
                }
            }

            T1.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            T1.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME, MPCODE AS Expr1 FROM TCOD_MAP WHERE (MPP = 1) ORDER BY MPNAME").ToList();

            if (T2.Visibility == Visibility.Visible)
            {
                T2.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME, MPCODE AS Expr1 FROM TCOD_MAP WHERE (MPP = 2) ORDER BY MPNAME").ToList();
            }
            if (T3.Visibility == Visibility.Visible)
            {
                T3.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME, MPCODE AS Expr1 FROM TCOD_MAP WHERE (MPP = 3) ORDER BY MPNAME").ToList();
            }
            if (T4.Visibility == Visibility.Visible)
            {
                T4.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME, MPCODE AS Expr1 FROM TCOD_MAP WHERE (MPP = 4)").ToList();
            }
            if (T5.Visibility == Visibility.Visible)
            {
                T5.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP = 5)").ToList();
            }
            if (T6.Visibility == Visibility.Visible)
            {
                T6.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP = 6)").ToList();
            }
            if (T7.Visibility == Visibility.Visible)
            {
                T7.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP = 7)").ToList();
            }
            if (T8.Visibility == Visibility.Visible)
            {
                T8.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP = 8)").ToList();
            }
            if (T9.Visibility == Visibility.Visible)
            {
                T9.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP =9)").ToList();
            }
            if (T10.Visibility == Visibility.Visible)
            {
                T10.ItemsSource = dbms.DoGetDataSQL<BARCODECOMBO>($"SELECT MPCODE, MPNAME FROM TCOD_MAP WHERE (MPP = 8)").ToList();
            }
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }
            return true;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BEGINEW_BTN_Click(object sender, RoutedEventArgs e)
        {
            //از نو
            CODE.Text = null;
            Text0.Text = null;
            TText0 = null;
            T1.IsEnabled = true;
            T2.IsEnabled = false;
            T3.IsEnabled = false;
            T4.IsEnabled = false;
            T5.IsEnabled = false;
            T6.IsEnabled = false;
            T7.IsEnabled = false;
            T8.IsEnabled = false;
            T9.IsEnabled = false;
            T10.IsEnabled = false;
            T1.SelectedValue = null;
            T2.SelectedValue = null;
            T3.SelectedValue = null;
            T4.SelectedValue = null;
            T5.SelectedValue = null;
            T6.SelectedValue = null;
            T7.SelectedValue = null;
            T8.SelectedValue = null;
            T9.SelectedValue = null;
            T10.SelectedValue = null;

            T1.Focus();
        }

        private void GOADD_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (digi.Text == "0" && string.IsNullOrEmpty(digi.Text))
            {
                return;
            }

            if (CODE.Text.Length == 30)
            {
                new Msgwin(false, "تعداد ارقام کد به حداکثر که 30 تا است رسیده و بیش از این امکان اضافه کردن کد نیست !").ShowDialog();
                return;
            }

            //اضافه کن
            var rst = dbms.DoGetDataSQL<int?>("SELECT TOP 100 PERCENT CAST(RIGHT(N_FANI, " + digi.Text + ") AS int) AS ser FROM dbo.STUF_DEF WHERE (LEFT(N_FANI, " + Strings.Len(CODE.Text) + ") = '" + CODE.Text + "') ORDER BY CAST(RIGHT(N_FANI, 4) AS int) DESC").FirstOrDefault();
            if (rst == null)
            {
                //// CODE.Text = string.Concat(TText0, (Math.Pow(10, Convert.ToDouble(digi.Text)) + 1).ToString().AsSpan(1, Convert.ToInt32(digi.Text)));
                CODE.Text = TText0 + Strings.Right(Convert.ToString(Math.Pow(10, Convert.ToDouble(digi.Text))) + 1, Convert.ToInt32(digi.Text));
            }
            else
            {
                CODE.Text = TText0 + Strings.Right(Convert.ToString(Math.Pow(10, Convert.ToDouble(digi.Text)) + rst + 1), Convert.ToInt32(digi.Text));
            }

            ADDTOKALA_BTN.IsEnabled = true;
        }

        private void ADDTOKALA_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CODE.Text))
            {
                new Msgwin(false, "نمیتواند کدینگ خالی اضافه کرد !").ShowDialog();
                return;
            }

            if (WIN_PASSED is STUF_DEF_WIN)
            {
                var FORM_STUF_DEF = WIN_PASSED as STUF_DEF_WIN;

                //انتقال به تعریف کالا
                if (FORM_STUF_DEF.NewRecord)
                {
                    FORM_STUF_DEF.N_FANI.Text = CODE.Text;
                    FORM_STUF_DEF.NAM.Text = Text0.Text;
                    FORM_STUF_DEF.N_FANI.Focus();
                }
                else
                {
                    Msgwin msgwin = new Msgwin(true, "كالاي تعريف شده در فرم تعريف كالا حاوي اطلاعاتي از قبل مي باشد آيا مايليد رونويسي كنم؟."); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        FORM_STUF_DEF.N_FANI.Text = CODE.Text;
                        FORM_STUF_DEF.NAM.Text = Text0.Text;
                        FORM_STUF_DEF.N_FANI.Focus();
                    }
                }

                this.Close();
            }

        }

        private void T1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T1.SelectedValue == null)
            {
                return;
            }

            //T1_AfterUpdate
            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 1)").FirstOrDefault();
            ////CODE.Text = Strings.Right(T1.SelectedValue.ToStringNullSafe() + Math.Pow(10, (double)rst0.SIZEF), (int)rst0.SIZEF);

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T1.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text = result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 1) and  MPCODE = " + T1.SelectedValue).FirstOrDefault();
            Text0.Text = rst.MPNAME;
            if (CC == 1)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T2.IsEnabled = true;
            }
        }

        private void T2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T2.SelectedValue == null)
            {
                return;
            }

            //T2_AfterUpdate
            T1_SelectionChanged(null, null);
            T1.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 2)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T2.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 2) and  MPCODE = " + T2.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 2)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T3.IsEnabled = true;
            }
        }

        private void T3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T3.SelectedValue == null)
            {
                return;
            }
            //T3_AfterUpdate
            T2_SelectionChanged(null, null);
            T2.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 3)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T3.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 3) and  MPCODE = " + T3.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 3)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T4.IsEnabled = true;
            }

        }

        private void T4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T4.SelectedValue == null)
            {
                return;
            }

            //T4_AfterUpdate
            T3_SelectionChanged(null, null);
            T3.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 4)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T4.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 4) and  MPCODE = " + T4.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 4)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T5.IsEnabled = true;
            }

        }

        private void T5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T5.SelectedValue == null)
            {
                return;
            }

            //T5_AfterUpdate
            T4_SelectionChanged(null, null);
            T4.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 5)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T5.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 5) and  MPCODE = " + T5.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 5)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T6.IsEnabled = true;
            }
        }

        private void T6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T6.SelectedValue == null)
            {
                return;
            }

            //T6_AfterUpdate
            T5_SelectionChanged(null, null);
            T5.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 6)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T6.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 6) and  MPCODE = " + T6.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 6)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T7.IsEnabled = true;
            }
        }

        private void T7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T7.SelectedValue == null)
            {
                return;
            }

            //T7_AfterUpdate
            T6_SelectionChanged(null, null);
            T6.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 7)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T7.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 7) and  MPCODE = " + T7.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 7)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T8.IsEnabled = true;
            }
        }

        private void T8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T8.SelectedValue == null)
            {
                return;
            }

            //T8_AfterUpdate
            T7_SelectionChanged(null, null);
            T7.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 8)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T8.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 8) and  MPCODE = " + T8.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 8)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T9.IsEnabled = true;
            }
        }

        private void T9_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T9.SelectedValue == null)
            {
                return;
            }

            //T9_AfterUpdate
            T8_SelectionChanged(null, null);
            T8.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 9)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T9.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 9) and  MPCODE = " + T9.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 9)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
            else
            {
                T10.IsEnabled = true;
            }
        }

        private void T10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (T10.SelectedValue == null)
            {
                return;
            }

            //T10_AfterUpdate
            T9_SelectionChanged(null, null);
            T9.IsEnabled = false;

            var rst0 = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE (MPP = 10)").FirstOrDefault();

            int SIZEF = Convert.ToInt32(rst0.SIZEF);
            double powResult = Math.Pow(10, SIZEF);
            double calculation = Convert.ToDouble(T10.SelectedValue.ToStringNullSafe()) + powResult;
            string result = calculation.ToString();
            CODE.Text += result.Substring(Math.Max(0, result.Length - SIZEF));


            var rst = dbms.DoGetDataSQL<TCOD_MAP>("SELECT     TOP 100 PERCENT MPP, MPNAME, MPCODE FROM dbo.TCOD_MAP WHERE     (MPP = 10) and  MPCODE = " + T10.SelectedValue).FirstOrDefault();
            Text0.Text = Text0.Text + "  " + rst.MPNAME;

            if (CC == 10)
            {
                TText0 = CODE.Text;
                GOADD_BTN.IsEnabled = true;
                GOADD_BTN_Click(null, null); //اضافه کن
            }
        }

        private void digi_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(digi.Text.Trim()) || string.IsNullOrWhiteSpace(digi.Text))
            {
                digi.Text = "1";
            }
        }

        private void DecodeBarcode(string barcode)
        {
            // ابتدا همه ComboBox ها را خالی و غیرفعال می‌کنیم
            BEGINEW_BTN_Click(null, null);

            if (string.IsNullOrEmpty(barcode))
                return;

            // گرفتن لیست گروه‌ها به ترتیب MPP 
            var mapGroups = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT TOP 100 PERCENT MPP, MPNAME, SIZEF, STARTF FROM dbo.TCOD_MAP_GRP WHERE MPP < 100 ORDER BY MPP").ToList();

            int currentPosition = 0;

            foreach (var group in mapGroups)
            {
                // اگر طول باقیمانده بارکد کافی نیست، خارج می‌شویم
                if (currentPosition + group.SIZEF > barcode.Length)
                    break;

                // جدا کردن بخش مربوط به این گروه از بارکد
                string section = barcode.Substring(currentPosition, (int)group.SIZEF);
                int sectionCode;

                // تبدیل بخش به عدد با حذف صفرهای ابتدایی
                if (int.TryParse(section, out sectionCode))
                {
                    // پیدا کردن کد معادل در TCOD_MAP
                    var mapItem = dbms.DoGetDataSQL<TCOD_MAP>($"SELECT TOP 1 MPCODE, MPNAME FROM TCOD_MAP WHERE MPP = {group.MPP} AND MPCODE = {sectionCode}").FirstOrDefault();

                    if (mapItem != null)
                    {
                        // پیدا کردن ComboBox مربوطه و تنظیم مقدار آن
                        ComboBox relatedCombo = this.FindName($"T{group.MPP}") as ComboBox;
                        if (relatedCombo != null)
                        {
                            relatedCombo.IsEnabled = true;
                            relatedCombo.SelectedValue = mapItem.MPCODE;
                        }
                    }
                }

                currentPosition += (int)group.SIZEF;
            }

            // تنظیم TText0 برای امکان اضافه کردن سریال
            TText0 = barcode;
            CODE.Text = barcode;
            GOADD_BTN.IsEnabled = true;

            AllowEdits = false;
        }

    }
}
