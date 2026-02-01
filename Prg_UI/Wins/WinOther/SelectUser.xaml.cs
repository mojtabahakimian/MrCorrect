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
                ServerFilter = BuildServerFilter(_arg);
            }
            InitializeComponent();
        }
        public SelectUser(string _arg)
        {
            if (!string.IsNullOrEmpty(_arg))
            {
                //ServerFilter = $" AND SAL_NAME LIKE N'%{_arg}%'";
                ServerFilter = BuildServerFilter(_arg);
            }
            InitializeComponent();
        }
        private static string BuildServerFilter(string filterOrText)
        {
            if (LooksLikeSqlFilter(filterOrText))
            {
                return filterOrText;
            }

            string text = filterOrText.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string coded = EscapeSqlLike(CL_HESABDARI.CODESAL(text));
            string fixedPersian = EscapeSqlLike(CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(text)));
            string fixedPersianAlt = EscapeSqlLike(CL_HESABDARI.CODESAL(CL_HESABDARI.Fixpi(text)));

            return "sal_name like N'%" + coded + "%' or " +
                   "sal_name like N'%" + fixedPersian + "%' or " +
                   "sal_name like N'%" + fixedPersianAlt + "%'";
        }

        private static bool LooksLikeSqlFilter(string filterOrText)
        {
            if (string.IsNullOrWhiteSpace(filterOrText))
            {
                return false;
            }

            string lower = filterOrText.Trim().ToLowerInvariant();
            return lower.Contains(" like ")
                || lower.Contains(" and ")
                || lower.Contains(" or ")
                || lower.Contains("sal_name")
                || lower.Contains("psal_name")
                || lower.Contains("grsal")
                || lower.Contains("enabl")
                || lower.Contains("idd")
                || lower.Contains("=")
                || lower.Contains("<")
                || lower.Contains(">");
        }

        private static string EscapeSqlLike(string value)
        {
            return value.Replace("'", "''");
        }
        public S_USER_SALADTL? SelectedPersonel { get; set; } = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // کوئری پایه: کاربران فعال و غیر سیستمی (IDD <> 1)
                string sql = "SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL = 0) AND (IDD <> 1)";

                // اگر فیلتری از سمت UserControl آمده، آن را اضافه کن
                // UserControl قبلاً فیلتر را امن کرده است (Sanitized)
                if (!string.IsNullOrWhiteSpace(ServerFilter))
                {
                    sql += $" AND ({ServerFilter})";
                }

                // مرتب‌سازی برای نمایش بهتر
                sql += " ORDER BY SAL_NAME";

                // اجرای کوئری (بدون پارامتر چون فیلتر شامل لیترال‌هاست)
                var list = dbms.DoGetDataSQL<S_USER_SALADTL>(sql, new { }).ToList();

                // دیکد کردن نام‌ها (Legacy Decoding)
                foreach (var item in list)
                {
                    if (item.SAL_NAME != null)
                    {
                        item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME)
                                        .Replace("ي", "ی")
                                        .Replace("ك", "ک");
                    }
                }

                DGR_SUN_USER.ItemsSource = list;
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری لیست کاربران").Show();
                return;
            }

            DGR_SUN_USER.Focus();
            DGR_SUN_USER.SelectedIndex = 0;
        }
        private void SetSelectedRow()
        {
            if (DGR_SUN_USER.SelectedIndex > -1 && !(DGR_SUN_USER.SelectedItem is null))
            {
                ComboBox? MY_ELEMENT = null;
                if (TheWindow != null)
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
                    if (TheWindow != null)
                    {
                        MY_ELEMENT = TextBoxFormat.FindVisualChildren<ComboBox>(TheWindow).Where(x => x.Name != null && x.Name.ToString() == "PERSONEL").FirstOrDefault();
                    }
                }

                if (DGR_SUN_USER.SelectedItem is S_USER_SALADTL SelectedUserValueItem)
                {
                    if (MY_ELEMENT != null) //Called via Ui Widnow
                    {
                        MY_ELEMENT.SelectedValue = null;
                        MY_ELEMENT.SelectedValue = SelectedUserValueItem.IDD;
                        MY_ELEMENT.Items.Refresh();
                    }

                    SelectedPersonel = SelectedUserValueItem; //Just Get Value
                }

                Close();
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                e.Handled = true;
                SetSelectedRow();
            }
            else if (e.Key is Key.Escape)
            {
                e.Handled = true;
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
