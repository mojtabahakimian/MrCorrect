using MaterialDesignThemes.Wpf;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    /// <summary>
    /// Interaction logic for RAAS.xaml
    /// </summary>
    public partial class RAAS : Window
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
        public ObservableCollection<RAAS_MODEL> RAAS_DATA { get; set; } = new ObservableCollection<RAAS_MODEL>();

        private long _baseDate;

        public RAAS()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            USERID.Text = Baseknow.USERCOD.ToString();
            Text0.Text = Tarikh.FullCurrentDate.ToString();
            _baseDate = Convert.ToInt64(Text0.Text.ToRawTarikh());
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "RAAS", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(RAAS_SUB, "RAAS");

            var rows = dbms.DoGetDataSQL<RAAS_MODEL>($"SELECT * FROM dbo.RAAS WHERE USERID = {Baseknow.USERCOD}").ToList();
            foreach (var item in rows)
            {
                RAAS_DATA.Add(item);
            }
            UpdateSums();
        }

        private void Text0_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RAAS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (sender is not DataGrid grid) return;
            var isNew = e.Row.IsNewItem;

            var binding = (e.Column as DataGridBoundColumn)?.Binding as Binding;
            var mappingName = binding?.Path?.Path;

            var record = e.Row.Item as RAAS_MODEL;
            if (record == null || mappingName == null) return;

            switch (mappingName)
            {
                case nameof(RAAS_MODEL.DATE_S):
                    if (e.EditingElement is TextBox txt && long.TryParse(txt.Text, out var dt))
                    {
                        record.DATE_S = dt;
                        record.ROOS = (int)CL_HESABDARI.DIFF(_baseDate, record.DATE_S ?? 0);
                        record.MABK = (record.ROOS ?? 0) * (record.MABL ?? 0);
                    }
                    break;

                case nameof(RAAS_MODEL.MABL):
                    if (e.EditingElement is TextBox txtMabl && long.TryParse(txtMabl.Text, out var mabl))
                    {
                        record.MABL = mabl;
                        record.MABK = (record.ROOS ?? 0) * (record.MABL ?? 0);
                    }
                    break;
            }

            UpdateSums();
        }

        private void RAAS_SUB_CANCEL_EDIT(object sender, DataGridCellEditEndingEventArgs e = null)
        {
            if (sender is DataGrid DG_SUB)
            {
                ////DG_SUB.Dispatcher.BeginInvoke(() =>
                DG_SUB.Dispatcher.InvokeAsync(() =>
                {
                    DG_SUB.CellEditEnding -= RAAS_SUB_CellEditEnding;

                    DG_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    DG_SUB.CancelEdit(DataGridEditingUnit.Row);

                    DG_SUB.CellEditEnding += RAAS_SUB_CellEditEnding;

                    if (e is not null)
                    {
                        var ERGI = e.Row.GetIndex();
                        if (ERGI > -1)
                        {
                            DG_SUB.SelectedIndex = ERGI;
                        }
                        e.EditingElement.Focus();
                    }
                });
            }
        }


        private void RAAS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var row = e.Row.Item as RAAS_MODEL;
            if (row == null) return;

            if ((row.DATE_S is null && row.ROOS is null))
            {
                return;
            }


            if (ConstructorRowDetector.IsPristine(row)) { RAAS_SUB_CANCEL_EDIT(RAAS_SUB, default); return; }



            row.ROOS = (int)CL_HESABDARI.DIFF(_baseDate, row.DATE_S ?? 0);
            row.MABK = (row.ROOS ?? 0) * (row.MABL ?? 0);
            row.USERID = Baseknow.USERCOD;

            var baseParams = new { row.DATE_S, row.MABL, row.ROOS, row.MABK, row.USERID };
            if (row.ID == null || row.ID <= 0)
            {
                var sql = @"INSERT INTO RAAS (DATE_S, MABL, ROOS, MABK, USERID)
                            VALUES (@DATE_S, @MABL, @ROOS, @MABK, @USERID);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var newId = dbms.DoGetDataSQL<int>(sql, baseParams).FirstOrDefault();
                if (newId > 0) row.ID = newId;
            }
            else
            {
                var sql = @"UPDATE RAAS SET DATE_S=@DATE_S, MABL=@MABL, ROOS=@ROOS, MABK=@MABK, USERID=@USERID WHERE ID=@ID";
                var updateParams = new { row.ID, baseParams.DATE_S, baseParams.MABL, baseParams.ROOS, baseParams.MABK, baseParams.USERID };
                dbms.DoExecuteSQL(sql, updateParams);
            }

            UpdateSums();
        }

        private void RAAS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control) ||
                (e.Key == Key.Insert && Keyboard.Modifiers == ModifierKeys.Shift))
            {
                e.Handled = true;
                PasteRaasRowsFromClipboard();
                return;
            }

            if (e.Key != Key.Delete || Keyboard.Modifiers != ModifierKeys.None) return;
            e.Handled = true;

            RAAS_SUB_CANCEL_EDIT(RAAS_SUB, default);

            // SelectedItems can contain CollectionView.NewItemPlaceholder while a
            // new row is being edited.  It is not a RAAS_MODEL and Cast<T>()
            // throws before any selected records can be deleted.
            var rows = RAAS_SUB.SelectedItems.OfType<RAAS_MODEL>().ToList();
            if (rows.Count == 0) return;

            foreach (var row in rows)
            {
                if (row.ID != null)
                {
                    dbms.DoExecuteSQL("DELETE FROM dbo.RAAS WHERE ID=@ID", new { ID = row.ID });
                }
                RAAS_DATA.Remove(row);
            }

            UpdateSums();
        }

        private void PasteRaasRowsFromClipboard()
        {
            string clipboardText;
            try
            {
                if (!Clipboard.ContainsText())
                {
                    ShowPasteError("کلیپ‌بورد حاوی متن نیست.");
                    return;
                }

                clipboardText = Clipboard.GetText();
            }
            catch (Exception)
            {
                ShowPasteError("خواندن اطلاعات از کلیپ‌بورد امکان‌پذیر نیست.");
                return;
            }

            var lines = clipboardText
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
            {
                ShowPasteError("کلیپ‌بورد خالی است.");
                return;
            }

            if (lines.Count > 10000)
            {
                ShowPasteError("حداکثر ۱۰٬۰۰۰ ردیف را می‌توان در هر مرحله Paste کرد.");
                return;
            }

            var parsedRows = new List<RAAS_MODEL>();
            var errors = new List<string>();
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var cells = lines[lineIndex].Split('\t').Select(cell => cell.Trim()).ToArray();
                if (lineIndex == 0 && IsRaasClipboardHeader(cells))
                {
                    continue;
                }

                if (cells.Length != 2 && cells.Length != 4)
                {
                    errors.Add($"ردیف {lineIndex + 1}: تعداد ستون‌ها باید ۲ (تاریخ و مبلغ) یا ۴ ستون خروجی جدول باشد.");
                    continue;
                }

                if (!TryParseRaasDate(cells[0], out var dueDate))
                {
                    errors.Add($"ردیف {lineIndex + 1}: تاریخ «{cells[0]}» معتبر نیست.");
                    continue;
                }

                if (!TryParseRaasAmount(cells[1], out var amount))
                {
                    errors.Add($"ردیف {lineIndex + 1}: مبلغ «{cells[1]}» باید عدد صحیح و بزرگ‌تر از صفر باشد.");
                    continue;
                }

                var days = (int)CL_HESABDARI.DIFF(_baseDate, dueDate);
                parsedRows.Add(new RAAS_MODEL
                {
                    DATE_S = dueDate,
                    MABL = amount,
                    ROOS = days,
                    MABK = (double)days * amount,
                    USERID = Baseknow.USERCOD
                });
            }

            // Paste is deliberately atomic from the user's point of view: a bad
            // row must not leave a partially imported list beside valid rows.
            if (errors.Count > 0 || parsedRows.Count == 0)
            {
                ShowPasteError(errors.Count > 0
                    ? string.Join(Environment.NewLine, errors.Take(20)) +
                      (errors.Count > 20 ? $"{Environment.NewLine}... و {errors.Count - 20} خطای دیگر" : string.Empty)
                    : "هیچ ردیف قابل قبولی در کلیپ‌بورد وجود ندارد.");
                return;
            }

            try
            {
                using var transaction = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);
                foreach (var row in parsedRows)
                {
                    var newId = transaction.SqlQueryCtc<int>(
                        @"INSERT INTO RAAS (DATE_S, MABL, ROOS, MABK, USERID)
                          VALUES (@DATE_S, @MABL, @ROOS, @MABK, @USERID);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new { row.DATE_S, row.MABL, row.ROOS, row.MABK, row.USERID }).FirstOrDefault();
                    if (newId <= 0)
                        throw new InvalidOperationException("Database did not return an ID for the pasted row.");

                    row.ID = newId;
                }

                transaction.DoCommit(false);
            }
            catch (Exception)
            {
                ShowPasteError("ذخیره اطلاعات Paste شده انجام نشد؛ هیچ ردیفی به جدول اضافه نشد.");
                return;
            }

            foreach (var row in parsedRows)
                RAAS_DATA.Add(row);

            UpdateSums();
            RAAS_SUB.SelectedItem = parsedRows[^1];
            RAAS_SUB.ScrollIntoView(parsedRows[^1]);
        }

        private static bool IsRaasClipboardHeader(string[] cells)
        {
            if (cells.Length != 2 && cells.Length != 4) return false;

            var dateHeader = cells[0].Replace(" ", string.Empty);
            var amountHeader = cells[1].Replace(" ", string.Empty);
            return (dateHeader.Contains("تاریخ") || dateHeader.Contains("سررسید")) &&
                   amountHeader.Contains("مبلغ");
        }

        private static bool TryParseRaasDate(string value, out long date)
        {
            date = 0;
            var normalized = CL_LMethods.NormalizeDigits(value ?? string.Empty).Trim();
            if (normalized.Length == 0 || normalized.Any(ch => !char.IsDigit(ch) && ch != '/' && ch != '-' && ch != '_'))
                return false;

            var rawDate = normalized.ToRawTarikh();
            return long.TryParse(rawDate, out date) && Tarikh.IsValidedDate(rawDate);
        }

        private static bool TryParseRaasAmount(string value, out long amount)
        {
            var normalized = CL_LMethods.NormalizeDigits(value ?? string.Empty)
                .Replace(",", string.Empty)
                .Replace("٬", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty)
                .Trim();

            return long.TryParse(normalized, out amount) && amount > 0;
        }

        private static void ShowPasteError(string message)
        {
            new Prg_UI.HelperWins.Msgwin(false, message).Show();
        }

        private void UpdateSums()
        {
            var sumMabl = RAAS_DATA.Sum(x => x.MABL ?? 0);
            var sumMabk = RAAS_DATA.Sum(x => x.MABK ?? 0);
            Text6.Text = sumMabl.ToString("N0");
            SMABK.Text = sumMabk.ToString("N0");
            if (sumMabl != 0)
                MABL.Text = (sumMabk / sumMabl).ToString("N2");
            else
                MABL.Text = "0";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    if (RAAS_SUB != null && RAAS_SUB.IsKeyboardFocusWithin)
                    {
                        if (RAAS_SUB.SelectedIndex == RAAS_SUB.Items.Count - 2 && RAAS_SUB.CurrentColumn?.DisplayIndex == CL_LMethods.GetLastColumn(RAAS_SUB)?.DisplayIndex)
                        {
                            RAAS_SUB.SelectedIndex = RAAS_SUB.Items.Count - 1;
                            RAAS_SUB.CurrentCell = new DataGridCellInfo(RAAS_SUB.SelectedItem, RAAS_SUB.Columns[0]);
                            RAAS_SUB.BeginEdit();

                            //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                            var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                            if (focusedWindow != null)
                            {
                                focusedWindow.Activate();
                                focusedWindow.Focus();
                            }

                            return;
                        }
                    }
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            catch { /*ignore*/ }
        }

        private void Text0_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (long.TryParse(Text0.Text, out var dt))
            {
                _baseDate = dt;
                foreach (var item in RAAS_DATA)
                {
                    item.ROOS = (int)CL_HESABDARI.DIFF(_baseDate, item.DATE_S ?? 0);
                    item.MABK = (item.ROOS ?? 0) * (item.MABL ?? 0);
                }
                UpdateSums();
                RAAS_SUB.Items.Refresh();
            }
        }
    }
}
