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
        private int _dbRowCount = 0;
        public ZF_IVO_EXTENDED(int _Id, Visual _YOUR_VL_WIN)
        {
            InitializeComponent();
            Id = _Id;
            WIN_COME = _YOUR_VL_WIN;

            _rows.CollectionChanged += (s, e) => Btn_DelRow.IsEnabled = _rows.Count > 1;
        }

        private bool _updatingChecks;

        private System.Windows.Controls.CheckBox[] ColumnChecks() => new[]
        {
            Chk_FLD1, Chk_FLD2, Chk_FLD3, Chk_FLD4, Chk_FLD5, Chk_FLD6, Chk_FLD7,
            Chk_FLD8, Chk_FLD9, Chk_FLD10, Chk_FLD11, Chk_FLD12, Chk_FLD13, Chk_FLD14
        };

        private void Chk_All_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_updatingChecks || !IsInitialized) return;
            _updatingChecks = true;
            bool check = Chk_All.IsChecked == true;
            foreach (var chk in ColumnChecks())
                chk.IsChecked = check;
            _updatingChecks = false;
        }

        private void ColumnCheck_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_updatingChecks) return;
            _updatingChecks = true;
            var checks = ColumnChecks();
            // Indeterminate when only some columns are ticked
            Chk_All.IsChecked = checks.All(c => c.IsChecked == true) ? true
                              : checks.All(c => c.IsChecked != true) ? (bool?)false
                              : null;
            _updatingChecks = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            foreach (var chk in ColumnChecks())
            {
                chk.Checked += ColumnCheck_CheckedChanged;
                chk.Unchecked += ColumnCheck_CheckedChanged;
            }

            if (Id > 0)
            {
                var rows = dbms.DoGetDataSQL<IVO_EXTENDED_CSHARP>("SELECT * FROM IVO_EXTENDED WHERE id=" + Id);
                foreach (var row in rows)
                    _rows.Add(row);
            }

            if (_rows.Count == 0)
                _rows.Add(new IVO_EXTENDED_CSHARP { id = Id });

            // Remember how many rows came from DB so we never treat them as "empty user input"
            _dbRowCount = _rows.Count;

            DG_Rows.ItemsSource = _rows;
        }

        private void Btn_AddRow_Click(object sender, RoutedEventArgs e)
        {
            // Only block adding a new row if the last row was added by the user this session AND is still empty
            bool lastRowIsNewAndEmpty = _rows.Count > _dbRowCount && IsRowEmpty(_rows.Last());
            if (lastRowIsNewAndEmpty)
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
            if (!(DG_Rows.SelectedItem is IVO_EXTENDED_CSHARP selected))
            {
                new Msgwin(false, "ابتدا یک سطر را انتخاب کنید").ShowDialog();
                return;
            }

            var confirm = new Msgwin(true, "آیا از حذف این سطر اطمینان دارید؟"); confirm.ShowDialog();
            if (confirm.DialogResult == true)
            {
                _rows.Remove(selected);
                new Msgwin(false, "حتما در انتها دکمه ذخیره را بزنید تا حذف کامل اعمال شود.").Show();
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DG_Rows.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            // Use InvariantCulture to avoid locale decimal-separator issues (e.g. fa-IR: "1,5" instead of "1.5")
            string V(double? v) => (v ?? 0).ToString(CultureInfo.InvariantCulture);

            // DB rows are always saved; only new rows added this session are filtered if empty
            var rowsToSave = _rows
                .Where((r, i) => i < _dbRowCount || !IsRowEmpty(r))
                .ToList();

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

                // No column ticked => nothing to push into MOLAH/SHARAYET; keep their current value
                switch (string.IsNullOrEmpty(paramsString) ? null : WIN_COME)
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
            // Only the columns the user has ticked are included; with multiple rows the
            // value of each ticked column is aggregated (summed) into a single entry.
            var parts = new System.Collections.Generic.List<string>();

            void AddIfChecked(System.Windows.Controls.CheckBox chk, string label, Func<IVO_EXTENDED_CSHARP, double?> get)
            {
                if (chk.IsChecked != true) return;
                double total = rows.Sum(r => get(r) ?? 0);
                parts.Add(label + ":" + total.ToString(CultureInfo.InvariantCulture));
            }

            AddIfChecked(Chk_FLD1, "چربي", r => r.FLD1);
            AddIfChecked(Chk_FLD2, "ماده خشک", r => r.FLD2);
            AddIfChecked(Chk_FLD3, "رطوبت", r => r.FLD3);
            AddIfChecked(Chk_FLD4, "پي اچ", r => r.FLD4);
            AddIfChecked(Chk_FLD5, "نمک", r => r.FLD5);
            AddIfChecked(Chk_FLD6, "دانسيته", r => r.FLD6);
            AddIfChecked(Chk_FLD7, "پروتئين", r => r.FLD7);
            AddIfChecked(Chk_FLD8, "انجماد", r => r.FLD8);
            AddIfChecked(Chk_FLD9, "اسيد", r => r.FLD9);
            AddIfChecked(Chk_FLD10, "الکل", r => r.FLD10);
            AddIfChecked(Chk_FLD11, "کلي فرم", r => r.FLD11);
            AddIfChecked(Chk_FLD12, "استاف", r => r.FLD12);
            AddIfChecked(Chk_FLD13, "اشيرشيا", r => r.FLD13);
            AddIfChecked(Chk_FLD14, "ذرات سوخته", r => r.FLD14);

            return string.Join("- ", parts);
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
