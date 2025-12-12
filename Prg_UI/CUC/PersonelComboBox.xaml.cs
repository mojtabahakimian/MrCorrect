using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.CUC
{
    public class PersonelSearchEventArgs : EventArgs
    {
        public string SearchText { get; }
        public string SqlFilterClause { get; }
        public int? ResultId { get; set; } // Parent این را پر می‌کند

        public PersonelSearchEventArgs(string searchText, string filter)
        {
            SearchText = searchText;
            SqlFilterClause = filter;
        }
    }

    // آرگومان برای وقتی انتخاب قطعی شد
    public class PersonelSelectedEventArgs : EventArgs
    {
        public COMBOPERSONEL SelectedItem { get; }
        public int SelectedId { get; }

        public PersonelSelectedEventArgs(COMBOPERSONEL item, int id)
        {
            SelectedItem = item;
            SelectedId = id;
        }
    }

    public partial class PersonelComboBox : UserControl
    {
        // دسترسی به دیتابیس (سراسری یا تزریق شده)
        private readonly CL_CCNNMANAGER dbms;

        // رویدادها
        public event EventHandler<PersonelSearchEventArgs> SearchRequested;
        public event EventHandler<PersonelSelectedEventArgs> PersonelSelected;

        // فیلدهای کنترلی UX
        private bool _suppressSelectionChanged;
        private bool _suppressLostFocus;
        private bool _isSearchDialogActive;
        private string _lastDismissedSearchText = string.Empty;
        private int? _lastCommittedId = null;

        public PersonelComboBox()
        {
            InitializeComponent();
            dbms = new CL_CCNNMANAGER(); // یا هر روش دسترسی که دارید
        }

        // --- Public Methods & Properties ---

        public int? SelectedValue
        {
            get => cmbPersonel.SelectedValue as int?;
            set => cmbPersonel.SelectedValue = value;
        }

        public void LoadData(int userId)
        {
            try
            {
                string sql = @"
                SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD
                FROM SALA_DTL sd
                LEFT JOIN USER_PERSONEL_ORDER uo 
                    ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
                WHERE sd.ENABL = 0
                ORDER BY
                    CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                    uo.SORT_ORDER, sd.SAL_NAME";

                var list = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = userId }).ToList();

                // دیکد کردن نام‌ها طبق منطق لگاسی
                foreach (var item in list)
                {
                    item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME);
                }

                cmbPersonel.ItemsSource = list;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("خطا در بارگذاری لیست پرسنل: " + ex.Message);
            }
        }

        public void FocusInput()
        {
            cmbPersonel.Focus();
        }

        // --- Internal Logic ---

        private void TriggerSelectionConfirmed()
        {
            if (cmbPersonel.SelectedValue == null) return;
            if (cmbPersonel.SelectedItem is not COMBOPERSONEL selectedItem) return;
            if (!selectedItem.IDD.HasValue) return;

            int currentId = selectedItem.IDD.Value;

            // جلوگیری از فایر شدن تکراری برای یک ID ثابت
            if (_lastCommittedId == currentId) return;

            _lastCommittedId = currentId;

            // خبر دادن به Parent
            PersonelSelected?.Invoke(this, new PersonelSelectedEventArgs(selectedItem, currentId));
        }

        private void cmbPersonel_DropDownClosed(object sender, EventArgs e)
        {
            if (cmbPersonel.SelectedValue != null)
            {
                TriggerSelectionConfirmed();
            }
        }


        private void cmbPersonel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged) return;

            // اگر کاربر انتخاب را پاک کرد، متغیر محافظ را ریست کن
            if (cmbPersonel.SelectedValue == null)
            {
                _lastCommittedId = null;
            }
        }
        // --- Internal Logic ---

        // این متد جدید، قلب تپنده لاجیک شماست که هم با اینتر و هم با لاست‌فوکوس صدا زده می‌شود
        private void ProcessInput()
        {
            // 1. اگر همین الان آیتمی به درستی انتخاب شده، فقط تایید کن
            if (cmbPersonel.SelectedValue != null)
            {
                TriggerSelectionConfirmed();
                return;
            }

            // 2. اگر متنی تایپ شده که انتخاب نشده است
            string text = GetComboBoxText().Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            // اگر کاربر قبلاً این متن را جستجو کرده و کنسل کرده، دوباره مزاحم نشو
            // (مگر اینکه کاربر صراحتاً اینتر زده باشد - این شرط را در LostFocus مدیریت می‌کنیم نه اینجا)
            // اما برای سادگی فعلا اینجا می‌گذاریم بماند، مگر اینکه بخواهید با اینتر حتماً دوباره سرچ کند
            if (string.Equals(text, _lastDismissedSearchText, StringComparison.Ordinal))
            {
                // اگر کاربر صراحتاً اینتر زده باشد، شاید بخواهد دوباره تلاش کند؟
                // اگر می‌خواهید با اینتر زورکی سرچ کند، این شرط را بردارید یا فلگ خاص بگذارید.
                // فعلاً طبق منطق قبلی رها می‌کنیم.
                // return; 
            }

            try
            {
                // A. تلاش برای پیدا کردن بر اساس ID عددی (Direct ID Search)
                if (int.TryParse(text, out int id))
                {
                    int? rst = dbms.DoGetDataSQL<int?>("select idd from sala_dtl where idd = @Idd", new { Idd = id }).FirstOrDefault();

                    if (rst != null)
                    {
                        SetSelectedIdSafe(rst.Value);
                        _lastDismissedSearchText = string.Empty;
                        TriggerSelectionConfirmed();
                        return;
                    }
                }

                // B. درخواست جستجو از Parent (Search Request)
                string filter =
                    "sal_name like N'%" + CL_HESABDARI.CODESAL(text) + "%' or " +
                    "sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixp(text)) + "%' or " +
                    "sal_name like N'%" + CL_HESABDARI.CODESAL(CL_HESABDARI.Fixpi(text)) + "%'";

                var args = new PersonelSearchEventArgs(text, filter);

                _isSearchDialogActive = true;

                // *** فراخوانی رویداد سمت Parent ***
                SearchRequested?.Invoke(this, args);

                // بررسی نتیجه‌ای که Parent ست کرده است
                if (args.ResultId.HasValue)
                {
                    // Parent یک ID معتبر برگرداند
                    SetSelectedIdSafe(args.ResultId.Value);
                    _lastDismissedSearchText = string.Empty;
                    TriggerSelectionConfirmed();
                }
                else
                {
                    // Parent چیزی برنگرداند (کنسل کرد)
                    _lastDismissedSearchText = text;
                }
            }
            catch (Exception)
            {
                // لاگ یا نمایش خطا
            }
            finally
            {
                _isSearchDialogActive = false;
                RefocusInputSafe();
            }
        }

        private void cmbPersonel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // تغییر مهم: چه انتخاب شده باشد چه نباشد، لاجیک پردازش را صدا بزن
                ProcessInput();

                // جلوگیری از صدای دینگ و رفتار پیش‌فرض
                //e.Handled = true;
            }
        }

        private void cmbPersonel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_suppressLostFocus) return;
            if (_isSearchDialogActive) return;
            if (!cmbPersonel.IsEditable) return;

            // فقط چک کردن Dismissed Text برای LostFocus مهم است تا لوپ نیفتد
            string text = GetComboBoxText().Trim();
            if (string.Equals(text, _lastDismissedSearchText, StringComparison.Ordinal) && cmbPersonel.SelectedValue == null)
                return;

            ProcessInput();
        }

        // --- Helpers ---

        private void SetSelectedIdSafe(int id)
        {
            _suppressSelectionChanged = true;
            cmbPersonel.SelectedValue = id;
            _suppressSelectionChanged = false;
        }

        private string GetComboBoxText()
        {
            cmbPersonel.ApplyTemplate();
            var tb = cmbPersonel.Template?.FindName("PART_EditableTextBox", cmbPersonel) as TextBox;
            return tb != null ? (tb.Text ?? "") : (cmbPersonel.Text ?? "");
        }





        private void RefocusInputSafe()
        {
            _suppressLostFocus = true;
            try
            {
                cmbPersonel.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        cmbPersonel.Focus();
                        var tb = cmbPersonel.Template?.FindName("PART_EditableTextBox", cmbPersonel) as TextBox;
                        if (tb != null)
                        {
                            tb.Focus();
                            tb.CaretIndex = tb.Text?.Length ?? 0;
                            tb.Select(tb.CaretIndex, 0);
                        }
                    }
                    finally
                    {
                        _suppressLostFocus = false;
                    }
                }));
            }
            catch { }
        }
    }
}
