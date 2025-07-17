using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
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
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.WinDEFAULT
{
    public partial class DEFAULT : Window
    {
        bool NowisReady = false;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public DEFAULT()
        {
            InitializeComponent();
        }
        #region HeaderWindow
        //Header Window Begin
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CL_LMethods.GoExitTheApplication();
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void lbl_budget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        //Header Window End;
        #endregion
        private partial class Custom_DEPART
        {
            public Int32 DEPATMAN { get; set; }
            public string DEPNAME { get; set; }
        }
        private partial class Custom_VEAHED
        {
            public Nullable<int> TFSAZMAN { get; set; }
        }
        private partial class Custom2_SHIFT
        {
            public Nullable<int> SHIFT { get; set; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int UCOD = Convert.ToInt32(Baseknow.USERCOD);
            var vahedy = dbms.DoGetDataSQL<Custom_VEAHED>($"SELECT TFSAZMAN FROM DEFAULTDEP WHERE USERID = {UCOD}").FirstOrDefault();
            var shifty = dbms.DoGetDataSQL<Custom2_SHIFT>($"SELECT SHIFT FROM DEFAULTDEP WHERE USERID = {UCOD}").FirstOrDefault();

            //VAHEDJARI
            TFSAZMAN_CMB.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            TFSAZMAN_CMB.DisplayMemberPath = "DEPNAME";
            TFSAZMAN_CMB.SelectedValuePath = "DEPATMAN";
            TFSAZMAN_CMB.SelectedIndex = 0;
            TFSAZMAN_CMB.SelectedItem = 0;

            //SHIFT
            SHIFT_CMB.ItemsSource = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID,SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            SHIFT_CMB.DisplayMemberPath = "SHNAME";
            SHIFT_CMB.SelectedValuePath = "SHIFT_ID";
            SHIFT_CMB.SelectedIndex = 0;
            SHIFT_CMB.SelectedItem = 0;



            var RecordSource = dbms.DoGetDataSQL<DEFAULTDEP>($"SELECT TFSAZMAN, SHIFT, USERID FROM [DEFAULTDEP] WHERE USERID = {Baseknow.USERCOD}").ToList();
            if (RecordSource.Count == 0)
            {
                dbms.DoExecuteSQL("insert into [DEFAULTDEP](USERID,TFSAZMAN,SHIFT)  values (" + Baseknow.USERCOD + ",1,1)");
                //this.Refresh;
            }

            TFSAZMAN_CMB.SelectedValue = vahedy.TFSAZMAN;
            SHIFT_CMB.SelectedValue = shifty.SHIFT;

            if (!CL_HESABDARI.LETSGO("DEFA")) // اپراتور دسترسی ندارد قفل کن
            {
                CL_LMethods.AllowEdits(default_grid, false, DEFAULT_VSH_SAVE);
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowisReady = true;
        }
        private Int32 TFSAZMAN_VAL()
        {
            Int32 R = 0;
            R = Convert.ToInt32(TFSAZMAN_CMB.SelectedValue);
            return R;
        }
        private Int32 SHIFT_VAL()
        {
            Int32 R1 = 0;
            R1 = Convert.ToInt32(SHIFT_CMB.SelectedValue);
            return R1;
        }
        private void DEFAULT_VSH_SAVE_Click(object sender, RoutedEventArgs e)
        {
            int UCOD = 0;
            UCOD = BASEKNOW_USERCOD();
            var RST_VAHED = dbms.DoGetDataSQL<DEFAULTDEP>($"SELECT TFSAZMAN, SHIFT, USERID FROM DEFAULTDEP WHERE USERID = {UCOD}").FirstOrDefault();
            if (RST_VAHED != null)
            {
                CL_Generaly.SHIFT_OF_USER = SHIFT_VAL();
                CL_Generaly.VAHED_OF_USER = TFSAZMAN_VAL();

                dbms.DoExecuteSQL($"UPDATE DEFAULTDEP SET TFSAZMAN = {TFSAZMAN_VAL()},SHIFT = {SHIFT_VAL()} WHERE USERID = {UCOD}");

                WinBase WindowAsli = new WinBase();
                Close();
                WindowAsli.ShowDialog();
            }
        }
        private int BASEKNOW_USERCOD()
        {
            return Convert.ToInt32(Baseknow.USERCOD);
        }
        private void TFSAZMAN_CMB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex < 0)
            {
                Msgwin msgwin = new Msgwin(false, "لطفا آیتیمی از لیست انتخاب کنید"); msgwin.ShowDialog();
                TFSAZMAN_CMB.IsDropDownOpen = true;
            }
        }
        private void SHIFT_CMB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex < 0)
            {
                Msgwin msgwin = new Msgwin(false, "لطفا آیتیمی از لیست انتخاب کنید"); msgwin.ShowDialog();
                SHIFT_CMB.IsDropDownOpen = true;
            }
        }
    }
}
