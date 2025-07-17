using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;

namespace Wins.WinOther
{
    public partial class WIN_SearchDEPART : Window
    {
        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
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
            Close();
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
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WIN_SearchDEPART(string mytextsearch, Visual _WIN_)
        {
            InitializeComponent();

            MyTextSearch = mytextsearch;

            THEWIN = _WIN_;

            this.DataContext = this;
        }
        public string MyTextSearch { get; set; } = string.Empty;
        public Visual THEWIN { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // تعریف کوئری پارامتری
            string query = @"
             SELECT * FROM dbo.DEPART WHERE 
                DEPNAME LIKE @search1 
             OR DEPNAME LIKE @search2 
             OR DEPNAME LIKE @search3";
            // تنظیم پارامترهای جستجو به صورت ایمن
            var parameters = new
            {
                search1 = "%" + CL_LMethods.ReplacePerArab(MyTextSearch, true) + "%",
                search2 = "%" + MyTextSearch + "%",
                search3 = "%" + CL_LMethods.ReplacePerArab(MyTextSearch, false) + "%"
            };
            // اجرای کوئری و دریافت نتایج
            var RST = dbms.DoGetDataSQL<DEPART_MODEL>(query, parameters).ToList();
            DEPART_SUB.ItemsSource = RST;

            //Cause Bug:
            //var shart = $" (DEPNAME LIKE N'%{CL_LMethods.ReplacePerArab(MyTextSearch, true)}%' OR DEPNAME LIKE N'%{MyTextSearch}%' OR DEPNAME LIKE N'%{CL_LMethods.ReplacePerArab(MyTextSearch, false)}%') ";
            //var shart2 = $" (DEPNAME LIKE N'%{CL_LMethods.ReplacePerArab(MyTextSearch, false)}%' OR DEPNAME LIKE N'%{MyTextSearch}%' OR DEPNAME LIKE N'%{CL_LMethods.ReplacePerArab(MyTextSearch, true)}%') ";
            //var RST = dbms.DoGetDataSQL<DEPART_MODEL>($"SELECT * FROM dbo.DEPART WHERE (({shart}) OR ({shart2})) ").ToList();

            DEPART_SUB.ItemsSource = RST;
            
            DEPART_SUB.Focus();
            DEPART_SUB.SelectedIndex = 0;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                e.Handled = true;
                SetSelectedValueTo();
            }
            else if ((e.Key is Key.Escape || e.SystemKey == Key.Escape) && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
            }
        }
        private void DEPART_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }
        private void DEPART_SUB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSelectedValueTo();
        }
        private void SetSelectedValueTo()
        {
            var ROW = DEPART_SUB.SelectedItem as DEPART_MODEL;
            if (DEPART_SUB.Items.Count > 0 && ROW != null && ROW?.DEPATMAN != null)
            {
                switch (THEWIN)
                {
                    case HEAD_LST_PISHFROOSH2:
                        (THEWIN as HEAD_LST_PISHFROOSH2).DEPATMAN.SelectedValue = ROW.DEPATMAN;
                        (THEWIN as HEAD_LST_PISHFROOSH2).DEPATMAN.Items.Refresh();
                        break;

                    default: break;
                }
                this.Close();
            }
        }
    }
}
