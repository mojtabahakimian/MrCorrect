using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using Wins.WinMenus.SALARY;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.PublicVRB;

namespace Prg_UI.Wins.WinOther
{
    public partial class SelectUser : Window
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
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private string ServerFilter { get; set; }
        private Window TheWindow { get; set; }
        public SelectUser(string _arg, IntPtr windowHandle)
        {
            TheWindow = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            if (!string.IsNullOrEmpty(_arg))
            {
                //ServerFilter = $" AND SAL_NAME LIKE N'%{_arg}%'";
                ServerFilter = _arg;
            }
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var QRE1 = dbms.DoGetDataSQL<S_USER_SALADTL>($"SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL = 0) AND (IDD <> 1)").ToList();
            for (int i = 0; i < QRE1.Count; i++)
                QRE1[i].SAL_NAME = CL_HESABDARI.DECODEUN(QRE1[i].SAL_NAME.ToString()).Replace("ي", "ی").Replace("ك", "ک");


            QRE1 = QRE1.Where(x => x.SAL_NAME.ToLower().Contains(ServerFilter.Trim().ToLower())).ToList();
            DGR_SUN_USER.ItemsSource = QRE1;
        }
        private void SetSelectedRow()
        {
            if (DGR_SUN_USER.SelectedIndex > -1 && !(DGR_SUN_USER.SelectedItem is null))
            {
                //اگر اتوماسیون هست
                if (TheWindow.GetType().Name == "MAIN")
                {
                    try
                    {
                        MAIN.MAIN_INST.PERSONEL.SelectedValue = null;
                        MAIN.MAIN_INST.PERSONEL.SelectedValue = (DGR_SUN_USER.SelectedItem as S_USER_SALADTL).IDD;
                        MAIN.MAIN_INST.PERSONEL.Items.Refresh();
                    }
                    catch (Exception)
                    {
                        //کبموباکس مجری
                        MAIN.MAIN_INST.rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL=0)").ToList();
                        foreach (var item_person in MAIN.MAIN_INST.rst_personel)
                            item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);

                        MAIN.MAIN_INST.PERSONEL.SelectedValue = null;
                        MAIN.MAIN_INST.PERSONEL.SelectedValue = (DGR_SUN_USER.SelectedItem as S_USER_SALADTL).IDD;
                        MAIN.MAIN_INST.PERSONEL.Items.Refresh();
                    }
                }

                var MY_ELEMENT = TextBoxFormat.FindVisualChildren<ComboBox>(TheWindow).Where(x => x.Name != null && x.Name.ToString() == "PERSONEL").FirstOrDefault();
                if (MY_ELEMENT is null)
                {
                    new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
                }
                else
                {
                    MY_ELEMENT.SelectedValue = null;
                    MY_ELEMENT.SelectedValue = (DGR_SUN_USER.SelectedItem as S_USER_SALADTL).IDD;
                    MY_ELEMENT.Items.Refresh();
                    //Close();
                }

                Close();
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key is Key.Enter)
            {
                SetSelectedRow();
            }
            else if (e.Key is Key.Escape)
            {
                Close();
            }
        }
        private void DGR_SUN_USER_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            SetSelectedRow();
        }
    }
}
