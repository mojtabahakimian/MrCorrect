using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.CRM
{
    /// <summary>
    /// Interaction logic for TEL_WIN.xaml (دفترچه تلفن)
    /// </summary>
    public partial class TEL_WIN : Window
    {
        public class TEL_MODEL
        {
            public string HES { get; set; } = "";
            public string NAME { get; set; } = "";
            public string? TEL { get; set; }
            public string? MOBILE { get; set; }
            public string? ADDRESS { get; set; }
            public string? TOZIH { get; set; }
            public string TEL_FULL { get; set; } = "";
        }

        private UniversControl universControl = new UniversControl();
        private CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private List<TEL_MODEL> allRecords = new List<TEL_MODEL>();

        public TEL_WIN()
        {
            InitializeComponent();
        }

        #region Header Window Begin
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupGridNavigation();
            LoadData();
        }

        private void SetupGridNavigation()
        {
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;
        }

        private void OnNavSelectionChanged(object? sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            UpdateNavigationText();
        }

        private void UpdateNavigationText()
        {
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;
            int total = 0;
            int current = 0;
            try
            {
                if (SYNCFUSION_DG.View != null)
                {
                    total = SYNCFUSION_DG.View.Records.Count;
                }
                if (SYNCFUSION_DG.SelectedIndex >= 0)
                {
                    current = SYNCFUSION_DG.SelectedIndex + 1;
                }
            }
            catch { }
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();
                Btn_First_Click(sender, e);
            }
            catch { }
        }

        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    SYNCFUSION_DG.SelectedIndex = 0;
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToHome();
                }
            }
            catch { }
        }

        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex > 0)
                {
                    SYNCFUSION_DG.SelectedIndex--;
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }

        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                {
                    SYNCFUSION_DG.SelectedIndex++;
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }

        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    var lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                    SYNCFUSION_DG.SelectedIndex = lastIndex;
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            TXT_SEARCH_NAME.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.F5)
            {
                LoadData();
            }
        }

        private async void LoadData()
        {
            try
            {
                // کوئری غیرهمزمان جهت جلوگیری از قفل شدن نخ UI هنگام باز شدن پنجره
                string sql = @"
                    SELECT
                        HES,
                        NAME,
                        TEL,
                        MOBILE,
                        ADDRESS,
                        TOZIH,
                        ISNULL(TEL, N' ') + N' | ' + ISNULL(MOBILE, N' ') + N' | ' + ISNULL(ADDRESS, N' ') + N' ' + ISNULL(TOZIH, N' ') AS TEL_FULL
                    FROM dbo.CUST_HESAB
                    ORDER BY NAME";

                var data = await System.Threading.Tasks.Task.Run(() => dbms.DoGetDataSQL<TEL_MODEL>(sql).ToList());
                allRecords = data;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShow("خطا در بارخوانی اطلاعات دفترچه تلفن: " + ex.Message, Pop1, Pop1Text1, Pop_Border1);
            }
        }

        private void ApplyFilter()
        {
            string searchName = (TXT_SEARCH_NAME.Text ?? "").Trim();
            string searchTel = (TXT_SEARCH_TEL.Text ?? "").Trim();

            var filtered = allRecords.AsEnumerable();

            if (!string.IsNullOrEmpty(searchName))
            {
                filtered = filtered.Where(x => x.NAME != null && x.NAME.Contains(searchName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(searchTel))
            {
                filtered = filtered.Where(x =>
                    (x.TEL_FULL != null && x.TEL_FULL.Contains(searchTel, StringComparison.OrdinalIgnoreCase)) ||
                    (x.TEL != null && x.TEL.Contains(searchTel, StringComparison.OrdinalIgnoreCase)) ||
                    (x.MOBILE != null && x.MOBILE.Contains(searchTel, StringComparison.OrdinalIgnoreCase)) ||
                    (x.ADDRESS != null && x.ADDRESS.Contains(searchTel, StringComparison.OrdinalIgnoreCase)) ||
                    (x.TOZIH != null && x.TOZIH.Contains(searchTel, StringComparison.OrdinalIgnoreCase))
                );
            }

            var list = filtered.ToList();
            SYNCFUSION_DG.ItemsSource = list;
            TXT_COUNT.Text = list.Count.ToString("N0");
            UpdateNavigationText();
        }

        private void TXT_SEARCH_NAME_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TXT_SEARCH_TEL_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BTN_NEW_CUST_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, this);
        }

        private void BTN_ESLAH_Click(object sender, RoutedEventArgs e)
        {
            var selected = SYNCFUSION_DG.SelectedItem as TEL_MODEL;
            if (selected != null && !string.IsNullOrEmpty(selected.HES))
            {
                var parts = selected.HES.Split('-');
                string? tnum = parts.Length >= 3 ? parts[2] : selected.HES;
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, this, tnum);
            }
            else
            {
                universControl.PopNotifyShow("لطفاً یک مشتری را برای اصلاح انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1);
            }
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var selected = SYNCFUSION_DG.SelectedItem as TEL_MODEL;
            if (selected != null && !string.IsNullOrEmpty(selected.HES))
            {
                Msgwin msgwin = new Msgwin(true, $"آیا از حذف اطلاعات مشتری «{selected.NAME}» اطمینان دارید؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    try
                    {
                        var parts = selected.HES.Split('-');
                        if (parts.Length >= 3 && int.TryParse(parts[2], out int tnum) && int.TryParse(parts[0], out int nkol) && int.TryParse(parts[1], out int number))
                        {
                            dbms.DoExecuteSQL($"DELETE FROM dbo.TDETA_HES WHERE N_KOL = {nkol} AND NUMBER = {number} AND TNUMBER = {tnum}");
                            universControl.PopNotifyShow("اطلاعات مشتری با موفقیت حذف شد.", Pop1, Pop1Text1, Pop_Border1);
                            LoadData();
                        }
                        else
                        {
                            universControl.PopNotifyShow("کد حساب مشتری معتبر نیست.", Pop1, Pop1Text1, Pop_Border1);
                        }
                    }
                    catch (Exception ex)
                    {
                        universControl.PopNotifyShow("خطا در حذف مشتری: " + ex.Message, Pop1, Pop1Text1, Pop_Border1);
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("لطفاً یک مشتری را برای حذف انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1);
            }
        }
    }
}
