using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.SANATI;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class ZF_IVO_EXTENDED : Window
    {
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
                this.DragMove();
            if (e.ClickCount == 2)
                Btn_Max_Click(null, null);
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public int Id { get; set; }
        public Visual WIN_COME { get; set; }

        private ObservableCollection<IVO_EXTENDED_CSHARP> _rows = new ObservableCollection<IVO_EXTENDED_CSHARP>();

        public ZF_IVO_EXTENDED(int _Id, Visual _YOUR_VL_WIN)
        {
            InitializeComponent();
            Id = _Id;
            WIN_COME = _YOUR_VL_WIN;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (Id > 0)
            {
                var rows = dbms.DoGetDataSQL<IVO_EXTENDED_CSHARP>("SELECT * FROM IVO_EXTENDED WHERE id=" + Id);
                foreach (var row in rows)
                    _rows.Add(row);
            }

            if (_rows.Count == 0)
                _rows.Add(new IVO_EXTENDED_CSHARP { id = Id });

            DG_Rows.ItemsSource = _rows;
        }

        private void Btn_AddRow_Click(object sender, RoutedEventArgs e)
        {
            DG_Rows.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
            if (IsRowEmpty(_rows.Last()))
            {
                new Msgwin(false, "لطفاً ابتدا سطر فعلی را پر کنید").ShowDialog();
                return;
            }
            _rows.Add(new IVO_EXTENDED_CSHARP { id = Id });
            DG_Rows.ScrollIntoView(_rows.Last());
        }

        private static bool IsRowEmpty(IVO_EXTENDED_CSHARP r) =>
            (r.FLD1 ?? 0) == 0 && (r.FLD2 ?? 0) == 0 && (r.FLD3 ?? 0) == 0 &&
            (r.FLD4 ?? 0) == 0 && (r.FLD5 ?? 0) == 0 && (r.FLD6 ?? 0) == 0 &&
            (r.FLD7 ?? 0) == 0 && (r.FLD8 ?? 0) == 0 && (r.FLD9 ?? 0) == 0 &&
            (r.FLD10 ?? 0) == 0 && (r.FLD11 ?? 0) == 0 && (r.FLD12 ?? 0) == 0 &&
            (r.FLD13 ?? 0) == 0 && (r.FLD14 ?? 0) == 0;

        private void Btn_DelRow_Click(object sender, RoutedEventArgs e)
        {
            DG_Rows.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
            if (!(DG_Rows.SelectedItem is IVO_EXTENDED_CSHARP selected)) return;

            if (_rows.Count <= 1)
            {
                new Msgwin(false, "حداقل یک سطر باید وجود داشته باشد").ShowDialog();
                return;
            }

            var confirm = MessageBox.Show("آیا از حذف این سطر اطمینان دارید؟", "حذف سطر",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
                _rows.Remove(selected);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DG_Rows.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            // Use InvariantCulture to avoid locale decimal-separator issues (e.g. fa-IR: "1,5" instead of "1.5")
            string V(double? v) => (v ?? 0).ToString(CultureInfo.InvariantCulture);

            var rowsToSave = _rows.Where(r => !IsRowEmpty(r)).ToList();
            if (!rowsToSave.Any())
            {
                new Msgwin(false, "حداقل یک سطر با داده باید وجود داشته باشد").ShowDialog();
                return;
            }

            // Build one atomic SQL batch: if any INSERT fails, XACT_ABORT rolls back the DELETE too
            var sql = new StringBuilder();
            sql.Append("SET XACT_ABORT ON; BEGIN TRANSACTION; ");
            sql.Append($"DELETE FROM dbo.IVO_EXTENDED WHERE id = {Id}; ");
            foreach (var row in rowsToSave)
            {
                sql.Append($@"INSERT INTO dbo.IVO_EXTENDED
                    (id, FLD1, FLD2, FLD3, FLD4, FLD5, FLD6, FLD7, FLD8, FLD9, FLD10, FLD11, FLD12, FLD13, FLD14, CRT, UID)
                    VALUES ({Id},
                        {V(row.FLD1)}, {V(row.FLD2)}, {V(row.FLD3)}, {V(row.FLD4)},
                        {V(row.FLD5)}, {V(row.FLD6)}, {V(row.FLD7)}, {V(row.FLD8)},
                        {V(row.FLD9)}, {V(row.FLD10)}, {V(row.FLD11)}, {V(row.FLD12)},
                        {V(row.FLD13)}, {V(row.FLD14)},
                        GETDATE(), {Baseknow.USERCOD ?? 0}); ");
            }
            sql.Append("COMMIT TRANSACTION;");

            try
            {
                dbms.DoExecuteSQL(sql.ToString());

                string paramsString = BuildParamsString(rowsToSave);

                switch (WIN_COME)
                {
                    case HEAD_LST_KHAREED1:
                        (WIN_COME as HEAD_LST_KHAREED1).MOLAH.Text = paramsString;
                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET MOLAH = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_KHAREED1).NUMBER.Text} AND TAG IN (12)");
                        break;

                    case HEAD_LST_RASID:
                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET SHARAYET = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_RASID).NUMBER.Text} AND TAG IN (1)");
                        break;

                    case HAVALAH_ENTER:
                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET SHARAYET = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HAVALAH_ENTER).NUMBER.Text} AND TAG IN (9)");
                        break;

                    case HEAD_LST_KHADAMAT:
                        (WIN_COME as HEAD_LST_KHADAMAT).MOLAH.Text = paramsString;
                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET MOLAH = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_KHADAMAT).NUMBER.Text} AND TAG IN (14)");
                        break;

                    default: break;
                }

                universControl.PopNotifyShow(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
                return;
            }

            Close();
        }

        private string BuildParamsString(System.Collections.Generic.List<IVO_EXTENDED_CSHARP> rows)
        {
            string RowLine(IVO_EXTENDED_CSHARP r) =>
                "چربي:" + (r.FLD1 ?? 0) +
                "- ماده خشک:" + (r.FLD2 ?? 0) +
                "- رطوبت:" + (r.FLD3 ?? 0) +
                "- پي اچ:" + (r.FLD4 ?? 0) +
                "- نمک:" + (r.FLD5 ?? 0) +
                "- دانسيته:" + (r.FLD6 ?? 0) +
                "- پروتئين:" + (r.FLD7 ?? 0) +
                "- انجماد:" + (r.FLD8 ?? 0) +
                "- اسيد:" + (r.FLD9 ?? 0) +
                "- الکل:" + (r.FLD10 ?? 0) +
                "- کلي فرم:" + (r.FLD11 ?? 0) +
                "- استاف:" + (r.FLD12 ?? 0) +
                "- اشيرشيا:" + (r.FLD13 ?? 0) +
                "- ذرات سوخته:" + (r.FLD14 ?? 0);

            return string.Join(" | ", rows.Select((r, i) =>
                rows.Count > 1 ? $"[{i + 1}] {RowLine(r)}" : RowLine(r)));
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!Save.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
