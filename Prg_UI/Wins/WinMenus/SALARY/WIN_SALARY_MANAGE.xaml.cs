using AUTO_BAZ.Functions;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Prg_UI.Wins.WinMenus.SALARY
{

    // قبل از کلاس WIN_SALARY_MANAGE — یک‌بار در همان فایل CS
    public class InverseBoolConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is bool b ? !b : (object)true;
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is bool b ? !b : (object)false;
    }

    public partial class WIN_SALARY_MANAGE : Window, INotifyPropertyChanged
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        // --- Properties for Binding ---
        public ObservableCollection<EmployeeModel> Employees { get; set; }
        public ObservableCollection<AttendanceModel> Attendance { get; set; }
        public ObservableCollection<AdvanceModel> Advances { get; set; }
        public ObservableCollection<PayrollModel> Payroll { get; set; }

        private ObservableCollection<TaxBracketModel> _taxBrackets;
        public ObservableCollection<TaxBracketModel> TaxBrackets
        {
            get => _taxBrackets;
            set { _taxBrackets = value; OnPropertyChanged(nameof(TaxBrackets)); }
        }
        private ICollectionView _taxBracketsView;
        public ICollectionView TaxBracketsView
        {
            get => _taxBracketsView;
            set { _taxBracketsView = value; OnPropertyChanged(nameof(TaxBracketsView)); }
        }

        private short _selectedTaxYearFilter = 0;


        private ObservableCollection<JobModel> _jobs;
        public ObservableCollection<JobModel> Jobs
        {
            get => _jobs;
            set { _jobs = value; OnPropertyChanged(nameof(Jobs)); }
        }

        private ObservableCollection<WorkshopModel> _workshops;
        public ObservableCollection<WorkshopModel> Workshops
        {
            get => _workshops;
            set { _workshops = value; OnPropertyChanged(nameof(Workshops)); }
        }

        private ObservableCollection<WorkshopModel> _workshopsWithAllOption;
        public ObservableCollection<WorkshopModel> WorkshopsWithAllOption
        {
            get => _workshopsWithAllOption;
            set { _workshopsWithAllOption = value; OnPropertyChanged(nameof(WorkshopsWithAllOption)); }
        }

        private WorkshopModel _currentWorkshop = new WorkshopModel();
        public WorkshopModel CurrentWorkshop
        {
            get => _currentWorkshop;
            set { _currentWorkshop = value; OnPropertyChanged(nameof(CurrentWorkshop)); }
        }

        private WorkshopAccModel _currentWorkshopAcc = new WorkshopAccModel();
        public WorkshopAccModel CurrentWorkshopAcc
        {
            get => _currentWorkshopAcc;
            set { _currentWorkshopAcc = value; OnPropertyChanged(nameof(CurrentWorkshopAcc)); }
        }

        // --- Properties for Item Definition (PAY2_ITEM_DEF) ---
        private ObservableCollection<Pay2ItemDefModel> _itemDefs;
        public ObservableCollection<Pay2ItemDefModel> ItemDefs
        {
            get => _itemDefs;
            set { _itemDefs = value; OnPropertyChanged(nameof(ItemDefs)); }
        }

        private Pay2ItemDefModel _currentItemDef = new Pay2ItemDefModel();
        public Pay2ItemDefModel CurrentItemDef
        {
            get => _currentItemDef;
            set
            {
                _currentItemDef = value;
                OnPropertyChanged(nameof(CurrentItemDef));
                OnPropertyChanged(nameof(ItemDefFormTitle));      // اضافه
                OnPropertyChanged(nameof(ItemDefSaveButtonText)); // اضافه
            }
        }

        private bool _isEditingItemDef;
        public bool IsEditingItemDef
        {
            get => _isEditingItemDef;
            set
            {
                _isEditingItemDef = value;
                OnPropertyChanged(nameof(IsEditingItemDef));
                OnPropertyChanged(nameof(IsItemCodeEnabled));
                OnPropertyChanged(nameof(ItemDefFormTitle));      // اضافه
                OnPropertyChanged(nameof(ItemDefSaveButtonText)); // اضافه
            }
        }

        // =========================================================================
        // --- PAY2_ADVANCE_EXCL (Advance Exceptions) Properties & Logic ---
        // =========================================================================

        private ObservableCollection<Pay2AdvanceExclModel> _employeeAdvanceExcls;
        public ObservableCollection<Pay2AdvanceExclModel> EmployeeAdvanceExcls
        {
            get => _employeeAdvanceExcls;
            set { _employeeAdvanceExcls = value; OnPropertyChanged(nameof(EmployeeAdvanceExcls)); }
        }

        private Pay2AdvanceExclModel _currentAdvanceExcl = new Pay2AdvanceExclModel();
        public Pay2AdvanceExclModel CurrentAdvanceExcl
        {
            get => _currentAdvanceExcl;
            set { _currentAdvanceExcl = value; OnPropertyChanged(nameof(CurrentAdvanceExcl)); }
        }

        private string _advanceExclModalTitle;
        public string AdvanceExclModalTitle
        {
            get => _advanceExclModalTitle;
            set { _advanceExclModalTitle = value; OnPropertyChanged(nameof(AdvanceExclModalTitle)); }
        }

        private bool _isEditingAdvanceExcl;
        public bool IsEditingAdvanceExcl
        {
            get => _isEditingAdvanceExcl;
            set { _isEditingAdvanceExcl = value; OnPropertyChanged(nameof(IsEditingAdvanceExcl)); }
        }

        private Pay2EmployeeModel _selectedAdvanceExclEmployee;

        public class Pay2AdvanceExclModel : INotifyPropertyChanged
        {
            private int _exclId;
            private int _empId;
            private long _periodDate;
            private long _exclAmount;
            private string _reason;
            private float? _deedNS;

            public int EXCL_ID { get => _exclId; set { _exclId = value; OnPropertyChanged(nameof(EXCL_ID)); } }
            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public long PERIOD_DATE { get => _periodDate; set { _periodDate = value; OnPropertyChanged(nameof(PERIOD_DATE)); } }
            public long EXCL_AMOUNT { get => _exclAmount; set { _exclAmount = value; OnPropertyChanged(nameof(EXCL_AMOUNT)); } }
            public string REASON { get => _reason; set { _reason = value; OnPropertyChanged(nameof(REASON)); } }
            public float? DEED_N_S { get => _deedNS; set { _deedNS = value; OnPropertyChanged(nameof(DEED_N_S)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowAdvanceExclModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedAdvanceExclEmployee = emp;
                AdvanceExclModalTitle = $"استثناهای مساعده: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";

                ResetAdvanceExclForm();
                LoadEmployeeAdvanceExcls(emp.EMP_ID);

                ModalAdvanceExclManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeAdvanceExcls(int empId)
        {
            try
            {
                string sql = @"SELECT EXCL_ID, EMP_ID, PERIOD_DATE, EXCL_AMOUNT, REASON, DEED_N_S 
                       FROM PAY2_ADVANCE_EXCL 
                       WHERE EMP_ID = @Id 
                       ORDER BY PERIOD_DATE DESC";
                var data = dbms.DoGetDataSQL<Pay2AdvanceExclModel>(sql, new { Id = empId });
                EmployeeAdvanceExcls = new ObservableCollection<Pay2AdvanceExclModel>(data ?? Enumerable.Empty<Pay2AdvanceExclModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری استثناهای مساعده: " + ex.Message);
            }
        }

        private void SaveAdvanceExcl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentAdvanceExcl.PERIOD_DATE < 13000000)
                {
                    MessageBox.Show("ماه اعمال استثنا نامعتبر است (مثال صحیح: 14030100).", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CurrentAdvanceExcl.EXCL_AMOUNT <= 0)
                {
                    MessageBox.Show("مبلغ کسر باید بزرگتر از صفر باشد.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(CurrentAdvanceExcl.REASON))
                {
                    MessageBox.Show("ذکر دلیل استثنا الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentAdvanceExcl.EMP_ID = _selectedAdvanceExclEmployee.EMP_ID;

                if (CurrentAdvanceExcl.EXCL_ID == 0)
                {
                    string insertSql = @"
                INSERT INTO PAY2_ADVANCE_EXCL 
                    (EMP_ID, PERIOD_DATE, EXCL_AMOUNT, REASON, DEED_N_S, CREATED_AT, CREATED_BY)
                VALUES 
                    (@EMP_ID, @PERIOD_DATE, @EXCL_AMOUNT, @REASON, @DEED_N_S, GETDATE(), @User)";

                    dbms.DoExecuteSQL(insertSql, new
                    {
                        CurrentAdvanceExcl.EMP_ID,
                        CurrentAdvanceExcl.PERIOD_DATE,
                        CurrentAdvanceExcl.EXCL_AMOUNT,
                        CurrentAdvanceExcl.REASON,
                        CurrentAdvanceExcl.DEED_N_S,
                        User = Prg_Proccessy.MODELS.Baseknow.USERCOD // Assuming you have this accessible as in other methods
                    });

                    ShowToast("استثنای مساعده جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string updateSql = @"
                UPDATE PAY2_ADVANCE_EXCL 
                SET PERIOD_DATE = @PERIOD_DATE, 
                    EXCL_AMOUNT = @EXCL_AMOUNT, 
                    REASON = @REASON, 
                    DEED_N_S = @DEED_N_S
                WHERE EXCL_ID = @EXCL_ID";

                    dbms.DoExecuteSQL(updateSql, CurrentAdvanceExcl);
                    ShowToast("استثنای مساعده با موفقیت ویرایش شد.");
                }

                ResetAdvanceExclForm();
                LoadEmployeeAdvanceExcls(_selectedAdvanceExclEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره اطلاعات استثنا: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditAdvanceExcl_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2AdvanceExclModel excl)
            {
                CurrentAdvanceExcl = new Pay2AdvanceExclModel
                {
                    EXCL_ID = excl.EXCL_ID,
                    EMP_ID = excl.EMP_ID,
                    PERIOD_DATE = excl.PERIOD_DATE,
                    EXCL_AMOUNT = excl.EXCL_AMOUNT,
                    REASON = excl.REASON,
                    DEED_N_S = excl.DEED_N_S
                };
                IsEditingAdvanceExcl = true;
            }
        }

        private void CancelEditAdvanceExcl_Click(object sender, RoutedEventArgs e)
        {
            ResetAdvanceExclForm();
        }

        private void DeleteAdvanceExcl_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2AdvanceExclModel excl)
            {
                var result = MessageBox.Show("آیا از حذف این استثنای مساعده اطمینان دارید؟ با این کار این مبلغ مجدداً در محاسبات کسر خواهد شد.", "حذف استثنا", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_ADVANCE_EXCL WHERE EXCL_ID = @Id", new { Id = excl.EXCL_ID });
                        ShowToast("استثنای مساعده با موفقیت حذف شد.");
                        LoadEmployeeAdvanceExcls(_selectedAdvanceExclEmployee.EMP_ID);

                        if (CurrentAdvanceExcl.EXCL_ID == excl.EXCL_ID)
                            ResetAdvanceExclForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف استثنا: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetAdvanceExclForm()
        {
            // گرفتن تاریخ روز از کلاس شمسی موجود شما و تبدیل به YYYYMM00
            var dateNowString = Tarikh.FullCurrentDate.Replace("/", "");
            long defaultDate = 14030100;
            if (dateNowString.Length >= 6)
            {
                long.TryParse(dateNowString.Substring(0, 6) + "00", out defaultDate);
            }

            CurrentAdvanceExcl = new Pay2AdvanceExclModel { PERIOD_DATE = defaultDate };
            IsEditingAdvanceExcl = false;
        }

        private void CloseAdvanceExclModal_Click(object sender, RoutedEventArgs e)
        {
            ModalAdvanceExclManage.Visibility = Visibility.Collapsed;
        }

        public bool IsItemCodeEnabled => !IsEditingItemDef || !CurrentItemDef.IS_SYSTEM;
        public string ItemDefFormTitle => IsEditingItemDef ? $"ویرایش آیتم: {CurrentItemDef.ITEM_NAME}" : "افزودن آیتم جدید";
        public string ItemDefSaveButtonText => IsEditingItemDef ? "ذخیره تغییرات" : "ثبت آیتم جدید";

        // --- Decrees Properties (PAY2_DECREE & PAY2_DECREE_LINE) ---
        private ObservableCollection<Pay2DecreeModel> _employeeDecrees;
        public ObservableCollection<Pay2DecreeModel> EmployeeDecrees
        {
            get => _employeeDecrees;
            set { _employeeDecrees = value; OnPropertyChanged(nameof(EmployeeDecrees)); }
        }

        private Pay2DecreeModel _currentDecree = new Pay2DecreeModel();
        public Pay2DecreeModel CurrentDecree
        {
            get => _currentDecree;
            set { _currentDecree = value; OnPropertyChanged(nameof(CurrentDecree)); }
        }

        private Pay2DecreeModel _selectedDecree;
        public Pay2DecreeModel SelectedDecree
        {
            get => _selectedDecree;
            set { _selectedDecree = value; OnPropertyChanged(nameof(SelectedDecree)); }
        }

        private string _decreeModalTitle;
        public string DecreeModalTitle
        {
            get => _decreeModalTitle;
            set { _decreeModalTitle = value; OnPropertyChanged(nameof(DecreeModalTitle)); }
        }

        private bool _isEditingDecree;
        public bool IsEditingDecree
        {
            get => _isEditingDecree;
            set { _isEditingDecree = value; OnPropertyChanged(nameof(IsEditingDecree)); }
        }

        private ObservableCollection<Pay2DecreeLineModel> _decreeLines;
        public ObservableCollection<Pay2DecreeLineModel> DecreeLines
        {
            get => _decreeLines;
            set { _decreeLines = value; OnPropertyChanged(nameof(DecreeLines)); }
        }

        private Pay2DecreeLineModel _currentDecreeLine = new Pay2DecreeLineModel();
        public Pay2DecreeLineModel CurrentDecreeLine
        {
            get => _currentDecreeLine;
            set { _currentDecreeLine = value; OnPropertyChanged(nameof(CurrentDecreeLine)); }
        }

        private bool _isEditingDecreeLine;
        public bool IsEditingDecreeLine
        {
            get => _isEditingDecreeLine;
            set { _isEditingDecreeLine = value; OnPropertyChanged(nameof(IsEditingDecreeLine)); }
        }

        // --- Properties and Logic for Overrides (PAY2_OVERRIDE) ---
        private ObservableCollection<Pay2OverrideModel> _employeeOverrides;
        public ObservableCollection<Pay2OverrideModel> EmployeeOverrides
        {
            get => _employeeOverrides;
            set { _employeeOverrides = value; OnPropertyChanged(nameof(EmployeeOverrides)); }
        }

        private Pay2OverrideModel _currentOverride = new Pay2OverrideModel();
        public Pay2OverrideModel CurrentOverride
        {
            get => _currentOverride;
            set { _currentOverride = value; OnPropertyChanged(nameof(CurrentOverride)); }
        }

        private string _overrideModalTitle;
        public string OverrideModalTitle
        {
            get => _overrideModalTitle;
            set { _overrideModalTitle = value; OnPropertyChanged(nameof(OverrideModalTitle)); }
        }

        private bool _isEditingOverride;
        public bool IsEditingOverride
        {
            get => _isEditingOverride;
            set { _isEditingOverride = value; OnPropertyChanged(nameof(IsEditingOverride)); OnPropertyChanged(nameof(IsOverridePrimaryKeyEnabled)); }
        }

        // Prevent changing Primary Key (ITEM_ID & VALID_FROM) during update to avoid composite PK violation
        public bool IsOverridePrimaryKeyEnabled => !IsEditingOverride;

        // =========================================================================
        // --- PAY2_LEAVE (Leave Requests) Properties & Logic ---
        // =========================================================================

        private ObservableCollection<Pay2LeaveModel> _employeeLeaves;
        public ObservableCollection<Pay2LeaveModel> EmployeeLeaves
        {
            get => _employeeLeaves;
            set { _employeeLeaves = value; OnPropertyChanged(nameof(EmployeeLeaves)); }
        }

        private Pay2LeaveModel _currentLeaveRequest = new Pay2LeaveModel();
        public Pay2LeaveModel CurrentLeaveRequest
        {
            get => _currentLeaveRequest;
            set { _currentLeaveRequest = value; OnPropertyChanged(nameof(CurrentLeaveRequest)); }
        }

        private string _leaveRequestModalTitle;
        public string LeaveRequestModalTitle
        {
            get => _leaveRequestModalTitle;
            set { _leaveRequestModalTitle = value; OnPropertyChanged(nameof(LeaveRequestModalTitle)); }
        }

        private bool _isEditingLeaveRequest;
        public bool IsEditingLeaveRequest
        {
            get => _isEditingLeaveRequest;
            set { _isEditingLeaveRequest = value; OnPropertyChanged(nameof(IsEditingLeaveRequest)); }
        }

        private Pay2EmployeeModel _selectedLeaveReqEmployee;

        public class Pay2LeaveModel : INotifyPropertyChanged
        {
            // ── فیلدهای خصوصی ──────────────────────────────────────────
            private int _levId;
            private int _empId;
            private byte _levType = 1;
            private long _requestDate;
            private long _startDate;
            private long _endDate;
            private short _reqDays = 1;
            private byte _reqHours = 0;
            private byte _reqMinutes = 0;
            private string _description;
            private byte _status = 1;

            // ── Property های اصلی (نام دقیق DDL) ───────────────────────
            public int LEV_ID
            {
                get => _levId;
                set { _levId = value; OnPropertyChanged(nameof(LEV_ID)); }
            }

            public int EMP_ID
            {
                get => _empId;
                set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); }
            }

            public byte LEV_TYPE
            {
                get => _levType;
                set { _levType = value; OnPropertyChanged(nameof(LEV_TYPE)); OnPropertyChanged(nameof(LeaveTypeText)); }
            }

            public long REQUEST_DATE
            {
                get => _requestDate;
                set { _requestDate = value; OnPropertyChanged(nameof(REQUEST_DATE)); }
            }

            public long START_DATE
            {
                get => _startDate;
                set { _startDate = value; OnPropertyChanged(nameof(START_DATE)); }
            }

            public long END_DATE
            {
                get => _endDate;
                set { _endDate = value; OnPropertyChanged(nameof(END_DATE)); }
            }

            public short REQ_DAYS
            {
                get => _reqDays;
                set { _reqDays = value; OnPropertyChanged(nameof(REQ_DAYS)); OnPropertyChanged(nameof(TotalMinutes)); }
            }

            public byte REQ_HOURS
            {
                get => _reqHours;
                set { _reqHours = value; OnPropertyChanged(nameof(REQ_HOURS)); OnPropertyChanged(nameof(TotalMinutes)); }
            }

            public byte REQ_MINUTES
            {
                get => _reqMinutes;
                set { _reqMinutes = value; OnPropertyChanged(nameof(REQ_MINUTES)); OnPropertyChanged(nameof(TotalMinutes)); }
            }

            public string DESCRIPTION
            {
                get => _description;
                set { _description = value; OnPropertyChanged(nameof(DESCRIPTION)); }
            }

            public byte STATUS
            {
                get => _status;
                set { _status = value; OnPropertyChanged(nameof(STATUS)); OnPropertyChanged(nameof(StatusText)); }
                //                    ↑ این خط StatusText رو هم آپدیت می‌کنه
            }

            // ── Property های محاسباتی (فقط نمایش — نه در SQL) ──────────
            public int TotalMinutes => REQ_DAYS * 440 + REQ_HOURS * 60 + REQ_MINUTES;

            public string LeaveTypeText => LEV_TYPE switch
            {
                1 => "استحقاقی",
                2 => "استعلاجی",
                3 => "بدون حقوق",
                4 => "زایمان",
                5 => "مأموریت",
                _ => "نامشخص"
            };

            public string StatusText => STATUS switch  // ← این همونیه که DataGrid نشون میده
            {
                1 => "ثبت اولیه",
                2 => "تأیید درخواست",
                3 => "تأیید مدیر واحد",
                4 => "تأیید مدیرعامل",
                _ => "نامشخص"
            };

            // ── INotifyPropertyChanged ───────────────────────────────────
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowLeaveRequestModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedLeaveReqEmployee = emp;
                LeaveRequestModalTitle = $"ثبت و مدیریت مرخصی: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";
                ResetLeaveRequestForm();
                LoadEmployeeLeaveRequests(emp.EMP_ID);
                ModalLeaveRequestManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeLeaveRequests(int empId)
        {
            try
            {
                string sql = "SELECT * FROM PAY2_LEAVE WHERE EMP_ID = @Id ORDER BY START_DATE DESC";
                var data = dbms.DoGetDataSQL<Pay2LeaveModel>(sql, new { Id = empId });
                EmployeeLeaves = new ObservableCollection<Pay2LeaveModel>(data ?? Enumerable.Empty<Pay2LeaveModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری لیست مرخصی‌ها: " + ex.Message);
            }
        }

        private void SaveLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentLeaveRequest.START_DATE <= 13000000 || CurrentLeaveRequest.END_DATE <= 13000000)
                {
                    MessageBox.Show("فرمت تاریخ شروع و پایان نامعتبر است. فرمت صحیح: 14030101", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CurrentLeaveRequest.TotalMinutes <= 0)
                {
                    MessageBox.Show(
                        "مدت مرخصی باید بزرگتر از صفر باشد.\nحداقل یک روز یا چند ساعت وارد کنید.",
                        "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentLeaveRequest.EMP_ID = _selectedLeaveReqEmployee.EMP_ID;

                // تاریخ درخواست = همان تاریخ شروع (یا می‌توانید از تاریخ امروز استفاده کنید)
                if (CurrentLeaveRequest.REQUEST_DATE <= 13000000)
                    CurrentLeaveRequest.REQUEST_DATE = CurrentLeaveRequest.START_DATE;

                if (CurrentLeaveRequest.LEV_ID == 0)
                {
                    string insertSql = @"
                INSERT INTO PAY2_LEAVE 
                    (EMP_ID, LEV_TYPE, REQUEST_DATE, START_DATE, END_DATE, 
                     REQ_DAYS, REQ_HOURS, REQ_MINUTES, DESCRIPTION, STATUS, CREATED_AT)
                VALUES 
                    (@EMP_ID, @LEV_TYPE, @REQUEST_DATE, @START_DATE, @END_DATE,
                     @REQ_DAYS, @REQ_HOURS, @REQ_MINUTES, @DESCRIPTION, @STATUS, GETDATE())";
                    dbms.DoExecuteSQL(insertSql, CurrentLeaveRequest);
                    ShowToast("درخواست مرخصی جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string updateSql = @"
                UPDATE PAY2_LEAVE 
                SET LEV_TYPE    = @LEV_TYPE,
                    START_DATE  = @START_DATE,
                    END_DATE    = @END_DATE,
                    REQ_DAYS    = @REQ_DAYS,
                    REQ_HOURS   = @REQ_HOURS,
                    REQ_MINUTES = @REQ_MINUTES,
                    DESCRIPTION = @DESCRIPTION,
                    STATUS      = @STATUS
                WHERE LEV_ID = @LEV_ID";
                    dbms.DoExecuteSQL(updateSql, CurrentLeaveRequest);
                    ShowToast("درخواست مرخصی با موفقیت ویرایش شد.");
                }

                ResetLeaveRequestForm();
                LoadEmployeeLeaveRequests(_selectedLeaveReqEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره مرخصی: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Tag به جای CommandParameter
            if (sender is Button btn && btn.Tag is Pay2LeaveModel leave)
            {
                CurrentLeaveRequest = new Pay2LeaveModel
                {
                    LEV_ID = leave.LEV_ID,
                    EMP_ID = leave.EMP_ID,
                    LEV_TYPE = leave.LEV_TYPE,
                    REQUEST_DATE = leave.REQUEST_DATE,
                    START_DATE = leave.START_DATE,
                    END_DATE = leave.END_DATE,
                    REQ_DAYS = leave.REQ_DAYS,
                    REQ_HOURS = leave.REQ_HOURS,
                    REQ_MINUTES = leave.REQ_MINUTES,
                    DESCRIPTION = leave.DESCRIPTION,
                    STATUS = leave.STATUS,
                };
                IsEditingLeaveRequest = true;
            }
        }

        private void CancelEditLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            ResetLeaveRequestForm();
        }

        private void DeleteLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Tag به جای CommandParameter
            if (sender is Button btn && btn.Tag is Pay2LeaveModel leave)
            {
                var result = MessageBox.Show(
                    $"آیا از حذف این مرخصی (شروع از: {leave.START_DATE}) اطمینان دارید؟",
                    "حذف مرخصی", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_LEAVE WHERE LEV_ID = @Id", new { Id = leave.LEV_ID });
                        ShowToast("مرخصی با موفقیت حذف شد.");
                        LoadEmployeeLeaveRequests(_selectedLeaveReqEmployee.EMP_ID);
                        if (CurrentLeaveRequest.LEV_ID == leave.LEV_ID)
                            ResetLeaveRequestForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف مرخصی: " + ex.Message, "خطای سیستمی",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void ResetLeaveRequestForm()
        {
            var dateNowString = Tarikh.FullCurrentDate.Replace("/", "");
            long.TryParse(dateNowString, out long defaultDate);

            CurrentLeaveRequest = new Pay2LeaveModel
            {
                START_DATE = defaultDate,
                END_DATE = defaultDate,
                REQ_DAYS = 0,   // ✅ صفر — تا کاربر مجبور باشه مقدار وارد کنه
                LEV_TYPE = 1,
                STATUS = 1,
            };
            IsEditingLeaveRequest = false;
        }

        private void CloseLeaveReqModal_Click(object sender, RoutedEventArgs e)
        {
            ModalLeaveRequestManage.Visibility = Visibility.Collapsed;
        }

        private Pay2EmployeeModel _selectedOverrideEmployee;

        public class Pay2OverrideModel : INotifyPropertyChanged
        {
            private int _empId;
            private int _itemId;
            private string _itemName;
            private bool? _insOv;
            private bool? _taxOv;
            private byte? _basisOv;
            private long _validFrom;
            private long? _validTo;
            private string _reason;

            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public int ITEM_ID { get => _itemId; set { _itemId = value; OnPropertyChanged(nameof(ITEM_ID)); } }
            public string ITEM_NAME { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ITEM_NAME)); } }

            public bool? INS_OV { get => _insOv; set { _insOv = value; OnPropertyChanged(nameof(INS_OV)); OnPropertyChanged(nameof(InsCombo)); } }
            public bool? TAX_OV { get => _taxOv; set { _taxOv = value; OnPropertyChanged(nameof(TAX_OV)); OnPropertyChanged(nameof(TaxCombo)); } }
            public byte? BASIS_OV { get => _basisOv; set { _basisOv = value; OnPropertyChanged(nameof(BASIS_OV)); OnPropertyChanged(nameof(BasisCombo)); } }

            public long VALID_FROM { get => _validFrom; set { _validFrom = value; OnPropertyChanged(nameof(VALID_FROM)); } }
            public long? VALID_TO { get => _validTo; set { _validTo = value; OnPropertyChanged(nameof(VALID_TO)); } }
            public string REASON { get => _reason; set { _reason = value; OnPropertyChanged(nameof(REASON)); } }

            public int InsCombo { get => INS_OV == null ? 0 : (INS_OV == true ? 1 : 2); set { INS_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int TaxCombo { get => TAX_OV == null ? 0 : (TAX_OV == true ? 1 : 2); set { TAX_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int BasisCombo { get => BASIS_OV == null ? 0 : (BASIS_OV == 1 ? 1 : 2); set { BASIS_OV = value == 0 ? (byte?)null : (byte)value; } }

            public string InsText => INS_OV == null ? "بدون تغییر" : (INS_OV == true ? "مشمول" : "معاف");
            public string TaxText => TAX_OV == null ? "بدون تغییر" : (TAX_OV == true ? "مشمول" : "معاف");
            public string BasisText => BASIS_OV == null ? "بدون تغییر" : (BASIS_OV == 1 ? "روزانه" : "ماهیانه");

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowOverrideModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedOverrideEmployee = emp;
                OverrideModalTitle = $"استثناهای مشمولیت: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";

                LoadItemDefs(); // Ensure ITEMS are loaded for ComboBox
                ResetOverrideForm();
                LoadEmployeeOverrides(emp.EMP_ID);

                ModalOverrideManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeOverrides(int empId)
        {
            try
            {
                string sql = @"SELECT O.EMP_ID, O.ITEM_ID, I.ITEM_NAME, O.INS_OV, O.TAX_OV, O.BASIS_OV, 
                                      O.VALID_FROM, O.VALID_TO, O.REASON
                               FROM PAY2_OVERRIDE O
                               INNER JOIN PAY2_ITEM_DEF I ON O.ITEM_ID = I.ITEM_ID
                               WHERE O.EMP_ID = @Id
                               ORDER BY O.VALID_FROM DESC, I.SORT_ORDER ASC";
                var data = dbms.DoGetDataSQL<Pay2OverrideModel>(sql, new { Id = empId });
                EmployeeOverrides = new ObservableCollection<Pay2OverrideModel>(data ?? Enumerable.Empty<Pay2OverrideModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری استثناها: " + ex.Message);
            }
        }

        private void SaveOverride_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentOverride.ITEM_ID <= 0)
                {
                    MessageBox.Show("لطفاً آیتم حقوقی مورد نظر را انتخاب کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CurrentOverride.VALID_FROM <= 0)
                {
                    MessageBox.Show("تاریخ شروع اعتبار الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentOverride.EMP_ID = _selectedOverrideEmployee.EMP_ID;

                if (!IsEditingOverride)
                {
                    var exists = dbms.DoGetDataSQL<int>(
                        "SELECT COUNT(1) FROM PAY2_OVERRIDE WHERE EMP_ID = @EMP_ID AND ITEM_ID = @ITEM_ID AND VALID_FROM = @VALID_FROM",
                        new { CurrentOverride.EMP_ID, CurrentOverride.ITEM_ID, CurrentOverride.VALID_FROM }).FirstOrDefault();

                    if (exists > 0)
                    {
                        MessageBox.Show("این استثنا با همین تاریخ شروع قبلاً برای این شخص ثبت شده است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string insertSql = @"
                        INSERT INTO PAY2_OVERRIDE 
                            (EMP_ID, ITEM_ID, INS_OV, TAX_OV, BASIS_OV, VALID_FROM, VALID_TO, REASON, CREATED_AT, CREATED_BY)
                        VALUES 
                            (@EMP_ID, @ITEM_ID, @INS_OV, @TAX_OV, @BASIS_OV, @VALID_FROM, @VALID_TO, @REASON, GETDATE(), @User)";

                    dbms.DoExecuteSQL(insertSql, new
                    {
                        CurrentOverride.EMP_ID,
                        CurrentOverride.ITEM_ID,
                        CurrentOverride.INS_OV,
                        CurrentOverride.TAX_OV,
                        CurrentOverride.BASIS_OV,
                        CurrentOverride.VALID_FROM,
                        CurrentOverride.VALID_TO,
                        CurrentOverride.REASON,
                        User = Baseknow.USERCOD
                    });

                    ShowToast("استثنای جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string updateSql = @"
                        UPDATE PAY2_OVERRIDE 
                        SET INS_OV = @INS_OV, 
                            TAX_OV = @TAX_OV, 
                            BASIS_OV = @BASIS_OV, 
                            VALID_TO = @VALID_TO, 
                            REASON = @REASON
                        WHERE EMP_ID = @EMP_ID AND ITEM_ID = @ITEM_ID AND VALID_FROM = @VALID_FROM";

                    dbms.DoExecuteSQL(updateSql, CurrentOverride);
                    ShowToast("استثنای مشمولیت با موفقیت ویرایش شد.");
                }

                ResetOverrideForm();
                LoadEmployeeOverrides(_selectedOverrideEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره اطلاعات استثنا: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditOverride_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2OverrideModel ovr)
            {
                CurrentOverride = new Pay2OverrideModel
                {
                    EMP_ID = ovr.EMP_ID,
                    ITEM_ID = ovr.ITEM_ID,
                    INS_OV = ovr.INS_OV,
                    TAX_OV = ovr.TAX_OV,
                    BASIS_OV = ovr.BASIS_OV,
                    VALID_FROM = ovr.VALID_FROM,
                    VALID_TO = ovr.VALID_TO,
                    REASON = ovr.REASON
                };
                IsEditingOverride = true;
            }
        }

        private void CancelEditOverride_Click(object sender, RoutedEventArgs e)
        {
            ResetOverrideForm();
        }

        private void DeleteOverride_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2OverrideModel ovr)
            {
                var result = MessageBox.Show($"آیا از حذف این استثنا (برای آیتم {ovr.ITEM_NAME}) اطمینان دارید؟", "حذف استثنا", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_OVERRIDE WHERE EMP_ID = @EMP_ID AND ITEM_ID = @ITEM_ID AND VALID_FROM = @VALID_FROM",
                            new { ovr.EMP_ID, ovr.ITEM_ID, ovr.VALID_FROM });

                        ShowToast("استثنای مشمولیت با موفقیت حذف شد.");
                        LoadEmployeeOverrides(_selectedOverrideEmployee.EMP_ID);

                        if (CurrentOverride.ITEM_ID == ovr.ITEM_ID && CurrentOverride.VALID_FROM == ovr.VALID_FROM)
                            ResetOverrideForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف استثنا: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetOverrideForm()
        {
            var dateNowString = Prg_Proccessy.FUNCTIONS.Tarikh.FullCurrentDate.Replace("/", "");
            long.TryParse(dateNowString, out long defaultDate);

            CurrentOverride = new Pay2OverrideModel { VALID_FROM = defaultDate };
            IsEditingOverride = false;
        }

        private Pay2EmployeeModel _selectedDecreeEmployee;

        public class Pay2DecreeModel : INotifyPropertyChanged
        {
            private int _decId;
            private int _empId;
            private int _wsId;
            private long _issuedDate;
            private long _effFrom;
            private long? _effTo;
            private byte? _eduLevel;
            private string _marital = "1";
            private bool? _isManager = false;
            private int? _tmplId;
            private bool _isConfirmed = false;
            private string _templateName;

            private string _notes;

            public int DEC_ID { get => _decId; set { _decId = value; OnPropertyChanged(nameof(DEC_ID)); } }
            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public long ISSUED_DATE { get => _issuedDate; set { _issuedDate = value; OnPropertyChanged(nameof(ISSUED_DATE)); } }
            public long EFF_FROM { get => _effFrom; set { _effFrom = value; OnPropertyChanged(nameof(EFF_FROM)); } }
            public long? EFF_TO { get => _effTo; set { _effTo = value; OnPropertyChanged(nameof(EFF_TO)); } }
            public byte? EDU_LEVEL { get => _eduLevel; set { _eduLevel = value; OnPropertyChanged(nameof(EDU_LEVEL)); } }
            public string MARITAL { get => _marital; set { _marital = value; OnPropertyChanged(nameof(MARITAL)); } }
            public bool? IS_MANAGER { get => _isManager; set { _isManager = value; OnPropertyChanged(nameof(IS_MANAGER)); } }
            public int? TMPL_ID { get => _tmplId; set { _tmplId = value; OnPropertyChanged(nameof(TMPL_ID)); } }
            public bool IS_CONFIRMED { get => _isConfirmed; set { _isConfirmed = value; OnPropertyChanged(nameof(IS_CONFIRMED)); } }
            public string TemplateName { get => _templateName; set { _templateName = value; OnPropertyChanged(nameof(TemplateName)); } }

            public string NOTES
            {
                get => _notes;
                set { _notes = value; OnPropertyChanged(nameof(NOTES)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class Pay2DecreeLineModel : INotifyPropertyChanged
        {
            private int _decId;
            private int _itemId;
            private string _itemName;
            private long _amount;
            private bool? _insOv;
            private bool? _taxOv;
            private byte? _basisOv;

            public int DEC_ID { get => _decId; set { _decId = value; OnPropertyChanged(nameof(DEC_ID)); } }
            public int ITEM_ID { get => _itemId; set { _itemId = value; OnPropertyChanged(nameof(ITEM_ID)); } }
            public string ITEM_NAME { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ITEM_NAME)); } }
            public long AMOUNT { get => _amount; set { _amount = value; OnPropertyChanged(nameof(AMOUNT)); } }

            public bool? INS_OV { get => _insOv; set { _insOv = value; OnPropertyChanged(nameof(INS_OV)); OnPropertyChanged(nameof(InsCombo)); } }
            public bool? TAX_OV { get => _taxOv; set { _taxOv = value; OnPropertyChanged(nameof(TAX_OV)); OnPropertyChanged(nameof(TaxCombo)); } }
            public byte? BASIS_OV { get => _basisOv; set { _basisOv = value; OnPropertyChanged(nameof(BASIS_OV)); OnPropertyChanged(nameof(BasisCombo)); } }

            public int InsCombo { get => INS_OV == null ? 0 : (INS_OV == true ? 1 : 2); set { INS_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int TaxCombo { get => TAX_OV == null ? 0 : (TAX_OV == true ? 1 : 2); set { TAX_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int BasisCombo { get => BASIS_OV == null ? 0 : (BASIS_OV == 1 ? 1 : 2); set { BASIS_OV = value == 0 ? (byte?)null : (byte)value; } }

            public string InsText => INS_OV == null ? "پایه" : (INS_OV == true ? "مشمول" : "معاف");
            public string TaxText => TAX_OV == null ? "پایه" : (TAX_OV == true ? "مشمول" : "معاف");
            public string BasisText => BASIS_OV == null ? "پایه" : (BASIS_OV == 1 ? "روزانه" : "ماهیانه");

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class Pay2ItemDefModel : INotifyPropertyChanged
        {
            private int _itemId;
            private string _itemCode;
            private string _itemName;
            private byte _itemType = 1;
            private byte _calcBasis = 2;
            private bool _insSubject = true;
            private bool _taxSubject = true;
            private bool _isSystem = false;
            private bool _showInSlip = true;
            private short _sortOrder = 100;
            private bool _isActive = true;
            private string _notes;

            public int ITEM_ID { get => _itemId; set { _itemId = value; OnPropertyChanged(nameof(ITEM_ID)); } }
            public string ITEM_CODE { get => _itemCode; set { _itemCode = value; OnPropertyChanged(nameof(ITEM_CODE)); } }
            public string ITEM_NAME { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ITEM_NAME)); } }
            public byte ITEM_TYPE { get => _itemType; set { _itemType = value; OnPropertyChanged(nameof(ITEM_TYPE)); OnPropertyChanged(nameof(ItemTypeText)); } }
            public byte CALC_BASIS { get => _calcBasis; set { _calcBasis = value; OnPropertyChanged(nameof(CALC_BASIS)); OnPropertyChanged(nameof(CalcBasisText)); } }
            public bool INS_SUBJECT { get => _insSubject; set { _insSubject = value; OnPropertyChanged(nameof(INS_SUBJECT)); } }
            public bool TAX_SUBJECT { get => _taxSubject; set { _taxSubject = value; OnPropertyChanged(nameof(TAX_SUBJECT)); } }
            public bool IS_SYSTEM { get => _isSystem; set { _isSystem = value; OnPropertyChanged(nameof(IS_SYSTEM)); OnPropertyChanged(nameof(CanDelete)); } }
            public bool SHOW_IN_SLIP { get => _showInSlip; set { _showInSlip = value; OnPropertyChanged(nameof(SHOW_IN_SLIP)); } }
            public short SORT_ORDER { get => _sortOrder; set { _sortOrder = value; OnPropertyChanged(nameof(SORT_ORDER)); } }
            public bool IS_ACTIVE { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IS_ACTIVE)); } }
            public string NOTES { get => _notes; set { _notes = value; OnPropertyChanged(nameof(NOTES)); } }

            // فیلدهای INS_BASE_DAYS و PAY_BASE_DAYS — اضافه‌شده v6
            private byte _insBaseDays = 1;
            private byte _payBaseDays = 2;

            public byte INS_BASE_DAYS
            {
                get => _insBaseDays;
                set { _insBaseDays = value; OnPropertyChanged(nameof(INS_BASE_DAYS)); OnPropertyChanged(nameof(InsBaseDaysText)); }
            }
            public byte PAY_BASE_DAYS
            {
                get => _payBaseDays;
                set { _payBaseDays = value; OnPropertyChanged(nameof(PAY_BASE_DAYS)); OnPropertyChanged(nameof(PayBaseDaysText)); }
            }

            // computed display props برای گرید
            public string InsBaseDaysText => INS_BASE_DAYS == 1 ? "DAYS" : "DAYSB";
            public string PayBaseDaysText => PAY_BASE_DAYS == 1 ? "DAYS" : "DAYSB";
            public string InsText => INS_SUBJECT ? "مشمول" : "معاف";
            public string TaxText => TAX_SUBJECT ? "مشمول" : "معاف";
            public string InsBgColor => INS_SUBJECT ? "#ecfdf5" : "#fef2f2";
            public string TaxBgColor => TAX_SUBJECT ? "#ecfdf5" : "#fef2f2";
            public string InsTextColor => INS_SUBJECT ? "#059669" : "#ef4444";
            public string TaxTextColor => TAX_SUBJECT ? "#059669" : "#ef4444";
            public string ActiveText => IS_ACTIVE ? "فعال" : "غیرفعال";
            public string ActiveBgColor => IS_ACTIVE ? "#ecfdf5" : "#fef2f2";
            public string ActiveTextColor => IS_ACTIVE ? "#059669" : "#ef4444";
            public string SystemText => IS_SYSTEM ? "بله" : "خیر";
            public string SystemBgColor => IS_SYSTEM ? "#eff6ff" : "#f1f5f9";
            public string SystemTextColor => IS_SYSTEM ? "#2563eb" : "#94a3b8";

            public string ItemTypeText => ITEM_TYPE == 1 ? "پرداختی ثابت" : ITEM_TYPE == 2 ? "پرداختی متغیر" : ITEM_TYPE == 3 ? "کسر ثابت" : ITEM_TYPE == 4 ? "کسر متغیر" : "آگاهی (نمایش)";
            public string CalcBasisText => CALC_BASIS == 1 ? "روزانه" : "ماهیانه";
            public bool CanDelete => !IS_SYSTEM;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        // --- Decrees Logic ---

        private void ShowDecreeModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedDecreeEmployee = emp;
                DecreeModalTitle = $"احکام کارگزینی: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";

                LoadItemTemplatesForCombo(); // اطمینان از بارگذاری قالب‌ها
                ResetDecreeForm();
                LoadEmployeeDecrees(emp.EMP_ID);
                ModalDecreeManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadItemTemplatesForCombo()
        {
            //if (ItemTemplates == null || !ItemTemplates.Any())
            {
                string sql = "SELECT TMPL_ID, TMPL_NAME FROM PAY2_ITEM_TEMPLATE WHERE IS_ACTIVE = 1";
                var data = dbms.DoGetDataSQL<Pay2ItemTemplateModel>(sql);
                ItemTemplates = new ObservableCollection<Pay2ItemTemplateModel>(data ?? Enumerable.Empty<Pay2ItemTemplateModel>());
            }
        }

        private void LoadEmployeeDecrees(int empId)
        {
            try
            {
                string sql = @"SELECT D.DEC_ID, D.EMP_ID, D.WS_ID, D.ISSUED_DATE, D.EFF_FROM, D.EFF_TO, 
                                      D.EDU_LEVEL, D.MARITAL, D.IS_MANAGER, D.TMPL_ID, D.IS_CONFIRMED,
                                      T.TMPL_NAME AS TemplateName
                               FROM PAY2_DECREE D
                               LEFT JOIN PAY2_ITEM_TEMPLATE T ON D.TMPL_ID = T.TMPL_ID
                               WHERE D.EMP_ID = @Id
                               ORDER BY D.EFF_FROM DESC";
                var data = dbms.DoGetDataSQL<Pay2DecreeModel>(sql, new { Id = empId });
                EmployeeDecrees = new ObservableCollection<Pay2DecreeModel>(data ?? Enumerable.Empty<Pay2DecreeModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری احکام: " + ex.Message);
            }
        }

        // ── متد جدید: بستن EFF_TO آخرین حکم فعال ──────────────────────
        // بعد از ResetDecreeForm() قرار می‌گیرد
        private void AutoCloseLastDecree(int empId, long newEffFrom)
        {
            try
            {
                // پیدا کردن آخرین حکم فعال (EFF_TO == NULL) قبل از حکم جدید
                string sqlFind = @"
            SELECT TOP 1 DEC_ID 
            FROM PAY2_DECREE
            WHERE EMP_ID = @EmpId
              AND IS_CONFIRMED = 1
              AND EFF_TO IS NULL
              AND EFF_FROM < @NewEffFrom
            ORDER BY EFF_FROM DESC";

                var prevDecId = dbms.DoGetDataSQL<int>(sqlFind, new { EmpId = empId, NewEffFrom = newEffFrom })
                                    .FirstOrDefault();

                if (prevDecId > 0)
                {
                    // EFF_TO = EFF_FROM_جدید - 1 روز (در تاریخ شمسی: کاهش YYYYMMDD)
                    long prevTo = DecrementShamsiDate(newEffFrom);
                    dbms.DoExecuteSQL(
                        "UPDATE PAY2_DECREE SET EFF_TO = @PrevTo WHERE DEC_ID = @Id",
                        new { PrevTo = prevTo, Id = prevDecId });
                }
            }
            catch (Exception ex)
            {
                // غیر بلاک‌کننده — فقط لاگ
                ShowToast("هشدار: بستن EFF_TO حکم قبلی: " + ex.Message);
            }
        }

        // تابع کمکی: کاهش یک روز از تاریخ شمسی BIGINT (YYYYMMDD)
        private long DecrementShamsiDate(long shamsiDate)
        {
            int y = (int)(shamsiDate / 10000);
            int m = (int)((shamsiDate % 10000) / 100);
            int d = (int)(shamsiDate % 100);

            d--;
            if (d < 1)
            {
                m--;
                if (m < 1) { m = 12; y--; }
                d = (m <= 6) ? 31 : (m <= 11) ? 30 : 29; // ماه‌های شمسی
            }
            return y * 10000L + m * 100 + d;
        }

        private void SaveDecree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentDecree.ISSUED_DATE <= 0 || CurrentDecree.EFF_FROM <= 0)
                {
                    MessageBox.Show("تاریخ صدور و تاریخ شروع اجرا الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentDecree.EMP_ID = _selectedDecreeEmployee.EMP_ID;
                CurrentDecree.WS_ID = _selectedDecreeEmployee.WS_ID;

                if (CurrentDecree.DEC_ID == 0)
                {
                    // درج حکم جدید
                    string sqlInsert = @"
                        INSERT INTO PAY2_DECREE 
                            (EMP_ID, WS_ID, ISSUED_DATE, EFF_FROM, EFF_TO, EDU_LEVEL, MARITAL, IS_MANAGER, TMPL_ID, IS_CONFIRMED, CREATED_AT, CREATED_BY,NOTES)
                        OUTPUT INSERTED.DEC_ID
                        VALUES 
                            (@EMP_ID, @WS_ID, @ISSUED_DATE, @EFF_FROM, @EFF_TO, @EDU_LEVEL, @MARITAL, @IS_MANAGER, @TMPL_ID, @IS_CONFIRMED, GETDATE(), @User,@NOTES)";

                    var newDecId = dbms.DoGetDataSQL<int>(sqlInsert, new
                    {
                        CurrentDecree.EMP_ID,
                        CurrentDecree.WS_ID,
                        CurrentDecree.ISSUED_DATE,
                        CurrentDecree.EFF_FROM,
                        CurrentDecree.EFF_TO,
                        CurrentDecree.EDU_LEVEL,
                        CurrentDecree.MARITAL,
                        CurrentDecree.IS_MANAGER,
                        CurrentDecree.TMPL_ID,
                        CurrentDecree.IS_CONFIRMED,
                        CurrentDecree.NOTES,
                        User = Baseknow.USERCOD
                    }).FirstOrDefault();

                    // تولید هوشمند اقلام بر اساس قالب
                    if (newDecId > 0 && CurrentDecree.TMPL_ID.HasValue && CurrentDecree.TMPL_ID.Value > 0)
                    {
                        string sqlLines = @"
                            INSERT INTO PAY2_DECREE_LINE (DEC_ID, ITEM_ID, AMOUNT, INS_OV, TAX_OV, BASIS_OV)
                            SELECT @NewDecId, ITEM_ID, DEF_AMOUNT, INS_OV, TAX_OV, BASIS_OV
                            FROM PAY2_ITEM_TMPL_LINE
                            WHERE TMPL_ID = @TMPL_ID";
                        dbms.DoExecuteSQL(sqlLines, new { NewDecId = newDecId, TMPL_ID = CurrentDecree.TMPL_ID.Value });
                    }
                    AutoCloseLastDecree(_selectedDecreeEmployee.EMP_ID, CurrentDecree.EFF_FROM);

                    ShowToast("حکم جدید به همراه اقلام ثبت شد.");
                }
                else
                {
                    // آپدیت حکم
                    string sqlUpdate = @"
        UPDATE PAY2_DECREE 
        SET ISSUED_DATE    = @ISSUED_DATE,
            EFF_FROM       = @EFF_FROM,
            EFF_TO         = @EFF_TO,
            EDU_LEVEL      = @EDU_LEVEL,
            MARITAL        = @MARITAL,
            IS_MANAGER     = @IS_MANAGER,
            NOTES = @NOTES,
            IS_CONFIRMED   = @IS_CONFIRMED,
            CONFIRMED_BY   = CASE WHEN @IS_CONFIRMED = 1 THEN @User ELSE CONFIRMED_BY END,
            CONFIRMED_AT   = CASE WHEN @IS_CONFIRMED = 1 THEN GETDATE() ELSE CONFIRMED_AT END
        WHERE DEC_ID = @DEC_ID";

                    dbms.DoExecuteSQL(sqlUpdate, new
                    {
                        CurrentDecree.DEC_ID,
                        CurrentDecree.ISSUED_DATE,
                        CurrentDecree.EFF_FROM,
                        CurrentDecree.EFF_TO,
                        CurrentDecree.EDU_LEVEL,
                        CurrentDecree.MARITAL,
                        CurrentDecree.IS_MANAGER,
                        CurrentDecree.IS_CONFIRMED,
                        CurrentDecree.NOTES,
                        User = Baseknow.USERCOD
                    });
                    ShowToast("اطلاعات حکم بروزرسانی شد.");
                }

                ResetDecreeForm();
                LoadEmployeeDecrees(_selectedDecreeEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره حکم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDecree_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2DecreeModel dec)
            {
                CurrentDecree = new Pay2DecreeModel
                {
                    DEC_ID = dec.DEC_ID,
                    EMP_ID = dec.EMP_ID,
                    WS_ID = dec.WS_ID,
                    ISSUED_DATE = dec.ISSUED_DATE,
                    EFF_FROM = dec.EFF_FROM,
                    EFF_TO = dec.EFF_TO,
                    EDU_LEVEL = dec.EDU_LEVEL,
                    MARITAL = dec.MARITAL,
                    IS_MANAGER = dec.IS_MANAGER,
                    TMPL_ID = dec.TMPL_ID,
                    IS_CONFIRMED = dec.IS_CONFIRMED
                };
                IsEditingDecree = true;
            }
        }

        private void DeleteDecree_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2DecreeModel dec)
            {
                var result = MessageBox.Show($"آیا از حذف این حکم (شروع: {dec.EFF_FROM}) اطمینان دارید؟ تمامی اقلام ریالی متصل نیز حذف خواهند شد.", "حذف حکم", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // جداول با ON DELETE CASCADE پاک می‌شوند، در غیر این صورت باید اول LINE ها پاک شوند.
                        dbms.DoExecuteSQL("DELETE FROM PAY2_DECREE_LINE WHERE DEC_ID = @Id", new { Id = dec.DEC_ID });
                        dbms.DoExecuteSQL("DELETE FROM PAY2_DECREE WHERE DEC_ID = @Id", new { Id = dec.DEC_ID });
                        ShowToast("حکم کارگزینی با موفقیت حذف شد.");
                        LoadEmployeeDecrees(_selectedDecreeEmployee.EMP_ID);
                        if (CurrentDecree.DEC_ID == dec.DEC_ID) ResetDecreeForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف حکم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditDecree_Click(object sender, RoutedEventArgs e) => ResetDecreeForm();

        private void ResetDecreeForm()
        {
            var dateNowString = Tarikh.FullCurrentDate.Replace("/", "");
            long.TryParse(dateNowString, out long defaultDate);

            CurrentDecree = new Pay2DecreeModel { ISSUED_DATE = defaultDate, EFF_FROM = defaultDate };
            IsEditingDecree = false;
        }

        // --- Decree Lines Logic ---

        private void ManageDecreeLines_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2DecreeModel dec)
            {
                SelectedDecree = dec;
                LoadItemDefsForDecree(); // جهت ComboBox
                ResetDecreeLineForm();
                LoadDecreeLines(dec.DEC_ID);

                ModalDecreeManage.Visibility = Visibility.Collapsed;
                ModalDecreeLineManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadDecreeLines(int decId)
        {
            try
            {
                string sql = @"SELECT L.DEC_ID, L.ITEM_ID, I.ITEM_NAME, L.AMOUNT, L.INS_OV, L.TAX_OV, L.BASIS_OV
                               FROM PAY2_DECREE_LINE L
                               INNER JOIN PAY2_ITEM_DEF I ON L.ITEM_ID = I.ITEM_ID
                               WHERE L.DEC_ID = @Id
                               ORDER BY I.SORT_ORDER";
                var data = dbms.DoGetDataSQL<Pay2DecreeLineModel>(sql, new { Id = decId });
                DecreeLines = new ObservableCollection<Pay2DecreeLineModel>(data ?? Enumerable.Empty<Pay2DecreeLineModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در واکشی اقلام حکم: " + ex.Message);
            }
        }

        private void SaveDecreeLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentDecreeLine.ITEM_ID == 0)
                {
                    MessageBox.Show("انتخاب آیتم حقوقی الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentDecreeLine.DEC_ID = SelectedDecree.DEC_ID;

                if (!IsEditingDecreeLine)
                {
                    var exists = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_DECREE_LINE WHERE DEC_ID = @DEC_ID AND ITEM_ID = @ITEM_ID",
                        new { CurrentDecreeLine.DEC_ID, CurrentDecreeLine.ITEM_ID }).FirstOrDefault();

                    if (exists > 0)
                    {
                        MessageBox.Show("این آیتم قبلاً در این حکم ثبت شده است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sql = @"INSERT INTO PAY2_DECREE_LINE (DEC_ID, ITEM_ID, AMOUNT, INS_OV, TAX_OV, BASIS_OV)
                                   VALUES (@DEC_ID, @ITEM_ID, @AMOUNT, @INS_OV, @TAX_OV, @BASIS_OV)";
                    dbms.DoExecuteSQL(sql, CurrentDecreeLine);
                    ShowToast("آیتم جدید به حکم اضافه شد.");
                }
                else
                {
                    string sql = @"UPDATE PAY2_DECREE_LINE 
                                   SET AMOUNT = @AMOUNT, INS_OV = @INS_OV, TAX_OV = @TAX_OV, BASIS_OV = @BASIS_OV
                                   WHERE DEC_ID = @DEC_ID AND ITEM_ID = @ITEM_ID";
                    dbms.DoExecuteSQL(sql, CurrentDecreeLine);
                    ShowToast("آیتم حکم با موفقیت ویرایش شد.");
                }

                ResetDecreeLineForm();
                LoadDecreeLines(SelectedDecree.DEC_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره آیتم حکم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDecreeLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2DecreeLineModel line)
            {
                CurrentDecreeLine = new Pay2DecreeLineModel
                {
                    DEC_ID = line.DEC_ID,
                    ITEM_ID = line.ITEM_ID,
                    AMOUNT = line.AMOUNT,
                    INS_OV = line.INS_OV,
                    TAX_OV = line.TAX_OV,
                    BASIS_OV = line.BASIS_OV
                };
                IsEditingDecreeLine = true;
            }
        }

        private void DeleteDecreeLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2DecreeLineModel line)
            {
                var res = MessageBox.Show("آیا از حذف این آیتم ریالی از حکم اطمینان دارید؟", "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_DECREE_LINE WHERE DEC_ID = @DEC_ID AND ITEM_ID = @ITEM_ID", new { line.DEC_ID, line.ITEM_ID });
                        ShowToast("آیتم از حکم حذف شد.");
                        LoadDecreeLines(SelectedDecree.DEC_ID);
                        if (CurrentDecreeLine.ITEM_ID == line.ITEM_ID) ResetDecreeLineForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف آیتم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditDecreeLine_Click(object sender, RoutedEventArgs e) => ResetDecreeLineForm();

        private void ResetDecreeLineForm()
        {
            CurrentDecreeLine = new Pay2DecreeLineModel();
            IsEditingDecreeLine = false;
        }

        private void CloseDecreeLineModal_Click(object sender, RoutedEventArgs e)
        {
            ModalDecreeLineManage.Visibility = Visibility.Collapsed;
            ModalDecreeManage.Visibility = Visibility.Visible;
        }
        private void ShowItemDefModal_Click(object sender, RoutedEventArgs e)
        {
            //مدیریت آیتم‌های حقوقی (PAY2_ITEM_DEF)
            ResetItemDefForm();
            LoadItemDefs();
            ModalItemDefManage.Visibility = Visibility.Visible;
        }

        private void LoadItemDefs()
        {
            try
            {
                string sql = "SELECT * FROM PAY2_ITEM_DEF ORDER BY SORT_ORDER, ITEM_CODE";
                var data = dbms.DoGetDataSQL<Pay2ItemDefModel>(sql);
                ItemDefs = new ObservableCollection<Pay2ItemDefModel>(data ?? Enumerable.Empty<Pay2ItemDefModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری آیتم‌های حقوقی: " + ex.Message);
            }
        }

        private void LoadItemDefsForDecree()
        {
            try
            {
                // فقط آیتم‌های پرداختی (نه کسورات خودکار سیستمی)
                string sql = @"
            SELECT ITEM_ID, ITEM_CODE, ITEM_NAME, ITEM_TYPE, CALC_BASIS,
                   INS_SUBJECT, TAX_SUBJECT, INS_BASE_DAYS, PAY_BASE_DAYS, SORT_ORDER
            FROM PAY2_ITEM_DEF
            WHERE IS_ACTIVE = 1
              AND ITEM_TYPE IN (1, 2)          -- فقط پرداختی ثابت و متغیر
              AND ITEM_CODE NOT IN ('INS_DED','TAX_DED','LOAN_DED','ADVANCE_DED')
            ORDER BY SORT_ORDER, ITEM_NAME";

                var data = dbms.DoGetDataSQL<Pay2ItemDefModel>(sql);
                ItemDefs = new ObservableCollection<Pay2ItemDefModel>(data ?? Enumerable.Empty<Pay2ItemDefModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری آیتم‌های حکم: " + ex.Message);
            }
        }
        private void SaveItemDef_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentItemDef.ITEM_CODE) || string.IsNullOrWhiteSpace(CurrentItemDef.ITEM_NAME))
                {
                    MessageBox.Show("کد و نام نمایشی آیتم الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CurrentItemDef.ITEM_ID == 0)
                {
                    int count = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_ITEM_DEF WHERE ITEM_CODE = @ITEM_CODE", new { CurrentItemDef.ITEM_CODE }).FirstOrDefault();
                    if (count > 0)
                    {
                        MessageBox.Show("کد آیتم (انگلیسی) تکراری است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sql = @"INSERT INTO PAY2_ITEM_DEF 
    (ITEM_CODE, ITEM_NAME, ITEM_TYPE, CALC_BASIS, 
     INS_BASE_DAYS, PAY_BASE_DAYS,
     INS_SUBJECT, TAX_SUBJECT, IS_SYSTEM, SHOW_IN_SLIP, 
     SORT_ORDER, IS_ACTIVE, NOTES, CREATED_AT, CREATED_BY)
    VALUES (@ITEM_CODE, @ITEM_NAME, @ITEM_TYPE, @CALC_BASIS, 
            @INS_BASE_DAYS, @PAY_BASE_DAYS,
            @INS_SUBJECT, @TAX_SUBJECT, @IS_SYSTEM, @SHOW_IN_SLIP, 
            @SORT_ORDER, @IS_ACTIVE, @NOTES, GETDATE(), @User)";

                    dbms.DoExecuteSQL(sql, new
                    {
                        CurrentItemDef.ITEM_CODE,
                        CurrentItemDef.ITEM_NAME,
                        CurrentItemDef.ITEM_TYPE,
                        CurrentItemDef.CALC_BASIS,
                        CurrentItemDef.INS_BASE_DAYS,  // جدید
                        CurrentItemDef.PAY_BASE_DAYS,  // جدید
                        CurrentItemDef.INS_SUBJECT,
                        CurrentItemDef.TAX_SUBJECT,
                        CurrentItemDef.IS_SYSTEM,
                        CurrentItemDef.SHOW_IN_SLIP,
                        CurrentItemDef.SORT_ORDER,
                        CurrentItemDef.IS_ACTIVE,
                        CurrentItemDef.NOTES,
                        User = Baseknow.USERCOD
                    });
                    ShowToast("آیتم حقوقی جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string sql = @"UPDATE PAY2_ITEM_DEF SET
    ITEM_NAME = @ITEM_NAME, ITEM_TYPE = @ITEM_TYPE, CALC_BASIS = @CALC_BASIS,
    INS_BASE_DAYS = @INS_BASE_DAYS, PAY_BASE_DAYS = @PAY_BASE_DAYS,
    INS_SUBJECT = @INS_SUBJECT, TAX_SUBJECT = @TAX_SUBJECT, 
    SHOW_IN_SLIP = @SHOW_IN_SLIP, SORT_ORDER = @SORT_ORDER, 
    IS_ACTIVE = @IS_ACTIVE, NOTES = @NOTES
    WHERE ITEM_ID = @ITEM_ID";

                    dbms.DoExecuteSQL(sql, CurrentItemDef);
                    ShowToast("آیتم حقوقی با موفقیت ویرایش شد.");
                }

                ResetItemDefForm();
                LoadItemDefs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره آیتم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditItemDef_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemDefModel item)
            {
                CurrentItemDef = new Pay2ItemDefModel
                {
                    ITEM_ID = item.ITEM_ID,
                    ITEM_CODE = item.ITEM_CODE,
                    ITEM_NAME = item.ITEM_NAME,
                    ITEM_TYPE = item.ITEM_TYPE,
                    CALC_BASIS = item.CALC_BASIS,
                    INS_BASE_DAYS = item.INS_BASE_DAYS,  // جدید
                    PAY_BASE_DAYS = item.PAY_BASE_DAYS,  // جدید
                    INS_SUBJECT = item.INS_SUBJECT,
                    TAX_SUBJECT = item.TAX_SUBJECT,
                    IS_SYSTEM = item.IS_SYSTEM,
                    SHOW_IN_SLIP = item.SHOW_IN_SLIP,
                    SORT_ORDER = item.SORT_ORDER,
                    IS_ACTIVE = item.IS_ACTIVE,
                    NOTES = item.NOTES
                };
                IsEditingItemDef = true;
            }
        }

        private void DeleteItemDef_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemDefModel item)
            {
                if (item.IS_SYSTEM)
                {
                    MessageBox.Show("آیتم‌های سیستمی، هسته اصلی محاسبه حقوق هستند و قابل حذف نمی‌باشند.", "اخطار امنیتی", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                var res = MessageBox.Show($"آیا از حذف آیتم «{item.ITEM_NAME}» اطمینان دارید؟", "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_ITEM_DEF WHERE ITEM_ID = @Id", new { Id = item.ITEM_ID });
                        ShowToast("آیتم حقوقی با موفقیت حذف شد.");
                        LoadItemDefs();
                        if (CurrentItemDef.ITEM_ID == item.ITEM_ID) ResetItemDefForm();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 547)
                    {
                        MessageBox.Show("این آیتم در احکام پرسنل یا قالب‌های حقوقی در حال استفاده است و قابل حذف نیست. برای عدم استفاده، تیک 'آیتم فعال است' را بردارید.", "خطای وابستگی اطلاعات", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف آیتم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditItemDef_Click(object sender, RoutedEventArgs e) => ResetItemDefForm();
        private void GridItemDefs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Pay2ItemDefModel selected)
            {
                CurrentItemDef = new Pay2ItemDefModel
                {
                    ITEM_ID = selected.ITEM_ID,
                    ITEM_CODE = selected.ITEM_CODE,
                    ITEM_NAME = selected.ITEM_NAME,
                    ITEM_TYPE = selected.ITEM_TYPE,
                    CALC_BASIS = selected.CALC_BASIS,
                    INS_BASE_DAYS = selected.INS_BASE_DAYS,
                    PAY_BASE_DAYS = selected.PAY_BASE_DAYS,
                    INS_SUBJECT = selected.INS_SUBJECT,
                    TAX_SUBJECT = selected.TAX_SUBJECT,
                    IS_SYSTEM = selected.IS_SYSTEM,
                    SHOW_IN_SLIP = selected.SHOW_IN_SLIP,
                    SORT_ORDER = selected.SORT_ORDER,
                    IS_ACTIVE = selected.IS_ACTIVE,
                    NOTES = selected.NOTES
                };
                IsEditingItemDef = true;
            }
        }
        private void ResetItemDefForm()
        {
            CurrentItemDef = new Pay2ItemDefModel();
            IsEditingItemDef = false;
        }

        // --- Properties and Logic for Templates (PAY2_ITEM_TEMPLATE) ---
        private ObservableCollection<Pay2ItemTemplateModel> _itemTemplates;
        public ObservableCollection<Pay2ItemTemplateModel> ItemTemplates
        {
            get => _itemTemplates;
            set { _itemTemplates = value; OnPropertyChanged(nameof(ItemTemplates)); }
        }

        private Pay2ItemTemplateModel _currentTemplate = new Pay2ItemTemplateModel();
        public Pay2ItemTemplateModel CurrentTemplate
        {
            get => _currentTemplate;
            set { _currentTemplate = value; OnPropertyChanged(nameof(CurrentTemplate)); }
        }

        private bool _isEditingTemplate;
        public bool IsEditingTemplate
        {
            get => _isEditingTemplate;
            set { _isEditingTemplate = value; OnPropertyChanged(nameof(IsEditingTemplate)); }
        }

        public class Pay2ItemTemplateModel : INotifyPropertyChanged
        {
            private int _tmplId;
            private string _tmplCode;
            private string _tmplName;
            private int _wsId = 0;
            private bool _isActive = true;
            private string _notes;
            private string _workshopName;

            public int TMPL_ID { get => _tmplId; set { _tmplId = value; OnPropertyChanged(nameof(TMPL_ID)); } }
            public string TMPL_CODE { get => _tmplCode; set { _tmplCode = value; OnPropertyChanged(nameof(TMPL_CODE)); } }
            public string TMPL_NAME { get => _tmplName; set { _tmplName = value; OnPropertyChanged(nameof(TMPL_NAME)); } }
            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public bool IS_ACTIVE { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IS_ACTIVE)); } }
            public string NOTES { get => _notes; set { _notes = value; OnPropertyChanged(nameof(NOTES)); } }
            public string WorkshopName { get => _workshopName; set { _workshopName = value; OnPropertyChanged(nameof(WorkshopName)); } }

            public string DisplayWorkshop => WS_ID == 0 ? "همه کارگاه‌ها" : WorkshopName;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowTemplateModal_Click(object sender, RoutedEventArgs e)
        {
            //مدیریت قالب‌های حکم (PAY2_ITEM_TEMPLATE)
            LoadWorkshopsForLookup();
            ResetTemplateForm();
            LoadTemplates();
            ModalTemplateManage.Visibility = Visibility.Visible;
        }

        private void LoadTemplates()
        {
            try
            {
                string sql = @"SELECT T.TMPL_ID, T.TMPL_CODE, T.TMPL_NAME, ISNULL(T.WS_ID, 0) AS WS_ID, 
                                      T.IS_ACTIVE, T.NOTES, W.WS_NAME AS WorkshopName
                               FROM PAY2_ITEM_TEMPLATE T
                               LEFT JOIN PAY2_WORKSHOP W ON T.WS_ID = W.WS_ID
                               ORDER BY T.TMPL_ID DESC";
                var data = dbms.DoGetDataSQL<Pay2ItemTemplateModel>(sql);
                ItemTemplates = new ObservableCollection<Pay2ItemTemplateModel>(data ?? Enumerable.Empty<Pay2ItemTemplateModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری قالب‌های حکم: " + ex.Message);
            }
        }

        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentTemplate.TMPL_CODE) || string.IsNullOrWhiteSpace(CurrentTemplate.TMPL_NAME))
                {
                    MessageBox.Show("کد و نام قالب الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? wsIdDb = CurrentTemplate.WS_ID == 0 ? (int?)null : CurrentTemplate.WS_ID;

                if (CurrentTemplate.TMPL_ID == 0)
                {
                    int count = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_ITEM_TEMPLATE WHERE TMPL_CODE = @TMPL_CODE", new { CurrentTemplate.TMPL_CODE }).FirstOrDefault();
                    if (count > 0)
                    {
                        MessageBox.Show("کد قالب (انگلیسی) تکراری است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sql = @"INSERT INTO PAY2_ITEM_TEMPLATE (TMPL_CODE, TMPL_NAME, WS_ID, IS_ACTIVE, NOTES) 
                                   VALUES (@TMPL_CODE, @TMPL_NAME, @WS_ID, @IS_ACTIVE, @NOTES)";
                    dbms.DoExecuteSQL(sql, new { CurrentTemplate.TMPL_CODE, CurrentTemplate.TMPL_NAME, WS_ID = wsIdDb, CurrentTemplate.IS_ACTIVE, CurrentTemplate.NOTES });
                    ShowToast("قالب حکم جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string sql = @"UPDATE PAY2_ITEM_TEMPLATE 
                                   SET TMPL_CODE = @TMPL_CODE, TMPL_NAME = @TMPL_NAME, WS_ID = @WS_ID, IS_ACTIVE = @IS_ACTIVE, NOTES = @NOTES 
                                   WHERE TMPL_ID = @TMPL_ID";
                    dbms.DoExecuteSQL(sql, new { CurrentTemplate.TMPL_CODE, CurrentTemplate.TMPL_NAME, WS_ID = wsIdDb, CurrentTemplate.IS_ACTIVE, CurrentTemplate.NOTES, CurrentTemplate.TMPL_ID });
                    ShowToast("قالب حکم با موفقیت ویرایش شد.");
                }

                ResetTemplateForm();
                LoadTemplates();
                LoadItemTemplatesForCombo(); // ← sync نگه داشتن cache
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره قالب: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemTemplateModel tmpl)
            {
                CurrentTemplate = new Pay2ItemTemplateModel
                {
                    TMPL_ID = tmpl.TMPL_ID,
                    TMPL_CODE = tmpl.TMPL_CODE,
                    TMPL_NAME = tmpl.TMPL_NAME,
                    WS_ID = tmpl.WS_ID,
                    IS_ACTIVE = tmpl.IS_ACTIVE,
                    NOTES = tmpl.NOTES
                };
                IsEditingTemplate = true;
            }
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemTemplateModel tmpl)
            {
                var res = MessageBox.Show($"آیا از حذف قالب «{tmpl.TMPL_NAME}» اطمینان دارید؟ تمام آیتم‌های متصل به این قالب نیز حذف خواهند شد.", "تایید حذف قالب", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_ITEM_TEMPLATE WHERE TMPL_ID = @Id", new { Id = tmpl.TMPL_ID });
                        ShowToast("قالب با موفقیت حذف شد.");
                        LoadTemplates();
                        if (CurrentTemplate.TMPL_ID == tmpl.TMPL_ID) ResetTemplateForm();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 547)
                    {
                        MessageBox.Show("این قالب در احکام پرسنل در حال استفاده است و قابل حذف نیست. فقط می‌توانید آن را غیرفعال کنید.", "خطای وابستگی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف قالب: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditTemplate_Click(object sender, RoutedEventArgs e) => ResetTemplateForm();

        private void ResetTemplateForm()
        {
            CurrentTemplate = new Pay2ItemTemplateModel();
            IsEditingTemplate = false;
        }

        // --- Properties and Logic for Template Lines (PAY2_ITEM_TMPL_LINE) ---
        private ObservableCollection<Pay2ItemTmplLineModel> _templateLines;
        public ObservableCollection<Pay2ItemTmplLineModel> TemplateLines
        {
            get => _templateLines;
            set { _templateLines = value; OnPropertyChanged(nameof(TemplateLines)); }
        }

        private Pay2ItemTmplLineModel _currentTemplateLine = new Pay2ItemTmplLineModel();
        public Pay2ItemTmplLineModel CurrentTemplateLine
        {
            get => _currentTemplateLine;
            set { _currentTemplateLine = value; OnPropertyChanged(nameof(CurrentTemplateLine)); }
        }

        private Pay2ItemTemplateModel _selectedMasterTemplate;
        public Pay2ItemTemplateModel SelectedMasterTemplate
        {
            get => _selectedMasterTemplate;
            set { _selectedMasterTemplate = value; OnPropertyChanged(nameof(SelectedMasterTemplate)); }
        }

        private bool _isEditingTemplateLine;
        public bool IsEditingTemplateLine
        {
            get => _isEditingTemplateLine;
            set { _isEditingTemplateLine = value; OnPropertyChanged(nameof(IsEditingTemplateLine)); }
        }

        public class Pay2ItemTmplLineModel : INotifyPropertyChanged
        {
            private int _tmplId;
            private int _itemId;
            private string _itemName;
            private long _defAmount = 0;
            private bool? _insOv;
            private bool? _taxOv;
            private byte? _basisOv;

            public int TMPL_ID { get => _tmplId; set { _tmplId = value; OnPropertyChanged(nameof(TMPL_ID)); } }
            public int ITEM_ID { get => _itemId; set { _itemId = value; OnPropertyChanged(nameof(ITEM_ID)); } }
            public string ITEM_NAME { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ITEM_NAME)); } }
            public long DEF_AMOUNT { get => _defAmount; set { _defAmount = value; OnPropertyChanged(nameof(DEF_AMOUNT)); } }

            public bool? INS_OV { get => _insOv; set { _insOv = value; OnPropertyChanged(nameof(INS_OV)); OnPropertyChanged(nameof(InsCombo)); } }
            public bool? TAX_OV { get => _taxOv; set { _taxOv = value; OnPropertyChanged(nameof(TAX_OV)); OnPropertyChanged(nameof(TaxCombo)); } }
            public byte? BASIS_OV { get => _basisOv; set { _basisOv = value; OnPropertyChanged(nameof(BASIS_OV)); OnPropertyChanged(nameof(BasisCombo)); } }

            public int InsCombo { get => INS_OV == null ? 0 : (INS_OV == true ? 1 : 2); set { INS_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int TaxCombo { get => TAX_OV == null ? 0 : (TAX_OV == true ? 1 : 2); set { TAX_OV = value == 0 ? (bool?)null : (value == 1); } }
            public int BasisCombo { get => BASIS_OV == null ? 0 : (BASIS_OV == 1 ? 1 : 2); set { BASIS_OV = value == 0 ? (byte?)null : (byte)value; } }

            public string InsText => INS_OV == null ? "پایه" : (INS_OV == true ? "مشمول" : "معاف");
            public string TaxText => TAX_OV == null ? "پایه" : (TAX_OV == true ? "مشمول" : "معاف");
            public string BasisText => BASIS_OV == null ? "پایه" : (BASIS_OV == 1 ? "روزانه" : "ماهیانه");

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ManageTemplateLines_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemTemplateModel tmpl)
            {
                SelectedMasterTemplate = tmpl;
                LoadItemDefs(); // For ComboBox
                ResetTemplateLineForm();
                LoadTemplateLines(tmpl.TMPL_ID);
                ModalTemplateLineManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadTemplateLines(int tmplId)
        {
            try
            {
                string sql = @"SELECT L.TMPL_ID, L.ITEM_ID, I.ITEM_NAME, L.DEF_AMOUNT, L.INS_OV, L.TAX_OV, L.BASIS_OV
                               FROM PAY2_ITEM_TMPL_LINE L
                               INNER JOIN PAY2_ITEM_DEF I ON L.ITEM_ID = I.ITEM_ID
                               WHERE L.TMPL_ID = @Id
                               ORDER BY I.SORT_ORDER";
                var data = dbms.DoGetDataSQL<Pay2ItemTmplLineModel>(sql, new { Id = tmplId });
                TemplateLines = new ObservableCollection<Pay2ItemTmplLineModel>(data ?? Enumerable.Empty<Pay2ItemTmplLineModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در واکشی آیتم‌های قالب: " + ex.Message);
            }
        }

        private void SaveTemplateLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentTemplateLine.ITEM_ID == 0)
                {
                    MessageBox.Show("انتخاب آیتم حقوقی الزامی است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentTemplateLine.TMPL_ID = SelectedMasterTemplate.TMPL_ID;

                if (!IsEditingTemplateLine)
                {
                    int count = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_ITEM_TMPL_LINE WHERE TMPL_ID = @TMPL_ID AND ITEM_ID = @ITEM_ID",
                        new { CurrentTemplateLine.TMPL_ID, CurrentTemplateLine.ITEM_ID }).FirstOrDefault();
                    if (count > 0)
                    {
                        MessageBox.Show("این آیتم قبلاً در این قالب ثبت شده است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sql = @"INSERT INTO PAY2_ITEM_TMPL_LINE (TMPL_ID, ITEM_ID, DEF_AMOUNT, INS_OV, TAX_OV, BASIS_OV)
                                   VALUES (@TMPL_ID, @ITEM_ID, @DEF_AMOUNT, @INS_OV, @TAX_OV, @BASIS_OV)";
                    dbms.DoExecuteSQL(sql, CurrentTemplateLine);
                    ShowToast("آیتم جدید به قالب اضافه شد.");
                }
                else
                {
                    string sql = @"UPDATE PAY2_ITEM_TMPL_LINE 
                                   SET DEF_AMOUNT = @DEF_AMOUNT, INS_OV = @INS_OV, TAX_OV = @TAX_OV, BASIS_OV = @BASIS_OV
                                   WHERE TMPL_ID = @TMPL_ID AND ITEM_ID = @ITEM_ID";
                    dbms.DoExecuteSQL(sql, CurrentTemplateLine);
                    ShowToast("آیتم قالب با موفقیت ویرایش شد.");
                }

                ResetTemplateLineForm();
                LoadTemplateLines(SelectedMasterTemplate.TMPL_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره آیتم قالب: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTemplateLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemTmplLineModel line)
            {
                CurrentTemplateLine = new Pay2ItemTmplLineModel
                {
                    TMPL_ID = line.TMPL_ID,
                    ITEM_ID = line.ITEM_ID,
                    DEF_AMOUNT = line.DEF_AMOUNT,
                    INS_OV = line.INS_OV,
                    TAX_OV = line.TAX_OV,
                    BASIS_OV = line.BASIS_OV
                };
                IsEditingTemplateLine = true;
            }
        }

        private void DeleteTemplateLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ItemTmplLineModel line)
            {
                var res = MessageBox.Show("آیا از حذف این آیتم از قالب اطمینان دارید؟", "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_ITEM_TMPL_LINE WHERE TMPL_ID = @TMPL_ID AND ITEM_ID = @ITEM_ID", new { line.TMPL_ID, line.ITEM_ID });
                        ShowToast("آیتم از قالب حذف شد.");
                        LoadTemplateLines(SelectedMasterTemplate.TMPL_ID);
                        if (CurrentTemplateLine.ITEM_ID == line.ITEM_ID) ResetTemplateLineForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف آیتم: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditTemplateLine_Click(object sender, RoutedEventArgs e) => ResetTemplateLineForm();

        private void ResetTemplateLineForm()
        {
            CurrentTemplateLine = new Pay2ItemTmplLineModel();
            IsEditingTemplateLine = false;
        }

        // =========================================================================
        // --- PAY2_LOAN (Employee Loans & Schedules) Properties & Logic ---
        // =========================================================================

        private ObservableCollection<Pay2LoanModel> _employeeLoans;
        public ObservableCollection<Pay2LoanModel> EmployeeLoans
        {
            get => _employeeLoans;
            set { _employeeLoans = value; OnPropertyChanged(nameof(EmployeeLoans)); }
        }

        private Pay2LoanModel _currentLoan = new Pay2LoanModel();
        public Pay2LoanModel CurrentLoan
        {
            get => _currentLoan;
            set { _currentLoan = value; OnPropertyChanged(nameof(CurrentLoan)); }
        }

        private string _loanModalTitle;
        public string LoanModalTitle
        {
            get => _loanModalTitle;
            set { _loanModalTitle = value; OnPropertyChanged(nameof(LoanModalTitle)); }
        }

        private bool _isEditingLoan;
        public bool IsEditingLoan
        {
            get => _isEditingLoan;
            set { _isEditingLoan = value; OnPropertyChanged(nameof(IsEditingLoan)); }
        }

        private Pay2EmployeeModel _selectedLoanEmployee;

        public class Pay2LoanModel : INotifyPropertyChanged
        {
            private int _loanId;
            private int _empId;
            private int _wsId;
            private byte _loanType = 1;
            private long _loanDate;
            private long _amount;
            private long _installment;
            private short _totalInst;
            private short _paidInst;
            private long _firstPay;
            private string _purpose;
            private bool _isActive = true;

            public int LOAN_ID { get => _loanId; set { _loanId = value; OnPropertyChanged(nameof(LOAN_ID)); } }
            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public byte LOAN_TYPE { get => _loanType; set { _loanType = value; OnPropertyChanged(nameof(LOAN_TYPE)); OnPropertyChanged(nameof(LoanTypeText)); } }
            public long LOAN_DATE { get => _loanDate; set { _loanDate = value; OnPropertyChanged(nameof(LOAN_DATE)); } }
            public long AMOUNT { get => _amount; set { _amount = value; OnPropertyChanged(nameof(AMOUNT)); OnPropertyChanged(nameof(BALANCE)); } }
            public long INSTALLMENT { get => _installment; set { _installment = value; OnPropertyChanged(nameof(INSTALLMENT)); OnPropertyChanged(nameof(BALANCE)); } }
            public short TOTAL_INST { get => _totalInst; set { _totalInst = value; OnPropertyChanged(nameof(TOTAL_INST)); } }
            public short PAID_INST { get => _paidInst; set { _paidInst = value; OnPropertyChanged(nameof(PAID_INST)); OnPropertyChanged(nameof(BALANCE)); } }
            public long FIRST_PAY { get => _firstPay; set { _firstPay = value; OnPropertyChanged(nameof(FIRST_PAY)); } }
            public string PURPOSE { get => _purpose; set { _purpose = value; OnPropertyChanged(nameof(PURPOSE)); } }
            public bool IS_ACTIVE { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IS_ACTIVE)); } }

            public string LoanTypeText => LOAN_TYPE == 1 ? "قرض‌الحسنه" : LOAN_TYPE == 2 ? "رفاهی" : LOAN_TYPE == 3 ? "ضروری" : LOAN_TYPE == 4 ? "مسکن" : "سایر";
            public long BALANCE => AMOUNT - (PAID_INST * INSTALLMENT);

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowLoanModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedLoanEmployee = emp;
                LoanModalTitle = $"مدیریت وام‌ها: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";

                ResetLoanForm();
                LoadEmployeeLoans(emp.EMP_ID);

                ModalLoanManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeLoans(int empId)
        {
            try
            {
                string sql = @"SELECT LOAN_ID, EMP_ID, WS_ID, LOAN_TYPE, LOAN_DATE, AMOUNT, INSTALLMENT, 
                                      TOTAL_INST, PAID_INST, FIRST_PAY, PURPOSE, IS_ACTIVE
                               FROM PAY2_LOAN 
                               WHERE EMP_ID = @Id 
                               ORDER BY LOAN_DATE DESC";
                var data = dbms.DoGetDataSQL<Pay2LoanModel>(sql, new { Id = empId });
                EmployeeLoans = new ObservableCollection<Pay2LoanModel>(data ?? Enumerable.Empty<Pay2LoanModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری لیست وام‌ها: " + ex.Message);
            }
        }

        private void SaveLoan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentLoan.LOAN_DATE <= 13000000)
                {
                    MessageBox.Show("تاریخ اعطای وام نامعتبر است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CurrentLoan.FIRST_PAY <= 13000000)
                {
                    MessageBox.Show("ماه شروع بازپرداخت نامعتبر است. فرمت صحیح: 14030100", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (CurrentLoan.AMOUNT <= 0 || CurrentLoan.INSTALLMENT <= 0 || CurrentLoan.TOTAL_INST <= 0)
                {
                    MessageBox.Show("مبالغ و تعداد اقساط باید بزرگتر از صفر باشند.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentLoan.EMP_ID = _selectedLoanEmployee.EMP_ID;
                CurrentLoan.WS_ID = _selectedLoanEmployee.WS_ID;

                if (CurrentLoan.LOAN_ID == 0)
                {
                    string insertSql = @"
                        INSERT INTO PAY2_LOAN 
                            (EMP_ID, WS_ID, LOAN_TYPE, LOAN_DATE, AMOUNT, INSTALLMENT, TOTAL_INST, PAID_INST, FIRST_PAY, PURPOSE, IS_ACTIVE, CREATED_AT, CREATED_BY)
                        OUTPUT INSERTED.LOAN_ID
                        VALUES 
                            (@EMP_ID, @WS_ID, @LOAN_TYPE, @LOAN_DATE, @AMOUNT, @INSTALLMENT, @TOTAL_INST, 0, @FIRST_PAY, @PURPOSE, @IS_ACTIVE, GETDATE(), @User)";

                    var newLoanId = dbms.DoGetDataSQL<int>(insertSql, new
                    {
                        CurrentLoan.EMP_ID,
                        CurrentLoan.WS_ID,
                        CurrentLoan.LOAN_TYPE,
                        CurrentLoan.LOAN_DATE,
                        CurrentLoan.AMOUNT,
                        CurrentLoan.INSTALLMENT,
                        CurrentLoan.TOTAL_INST,
                        CurrentLoan.FIRST_PAY,
                        CurrentLoan.PURPOSE,
                        CurrentLoan.IS_ACTIVE,
                        User = Prg_Proccessy.MODELS.Baseknow.USERCOD
                    }).FirstOrDefault();

                    if (newLoanId > 0)
                    {
                        GenerateLoanSchedule(newLoanId, CurrentLoan.FIRST_PAY, CurrentLoan.TOTAL_INST, 0, CurrentLoan.INSTALLMENT);
                    }

                    ShowToast("وام جدید با موفقیت ثبت و اقساط آن تولید شد.");
                }
                else
                {
                    string updateSql = @"
                        UPDATE PAY2_LOAN 
                        SET LOAN_TYPE = @LOAN_TYPE, 
                            LOAN_DATE = @LOAN_DATE, 
                            AMOUNT = @AMOUNT, 
                            INSTALLMENT = @INSTALLMENT, 
                            TOTAL_INST = @TOTAL_INST, 
                            FIRST_PAY = @FIRST_PAY, 
                            PURPOSE = @PURPOSE, 
                            IS_ACTIVE = @IS_ACTIVE
                        WHERE LOAN_ID = @LOAN_ID";

                    dbms.DoExecuteSQL(updateSql, CurrentLoan);

                    // حذف اقساط پرداخت نشده جهت بازتولید بر اساس تنظیمات جدید
                    dbms.DoExecuteSQL("DELETE FROM PAY2_LOAN_SCHED WHERE LOAN_ID = @LOAN_ID AND RUN_ID IS NULL AND PAID_AT IS NULL", new { CurrentLoan.LOAN_ID });

                    // تولید مابقی اقساط
                    GenerateLoanSchedule(CurrentLoan.LOAN_ID, CurrentLoan.FIRST_PAY, CurrentLoan.TOTAL_INST, CurrentLoan.PAID_INST, CurrentLoan.INSTALLMENT);

                    ShowToast("وام با موفقیت ویرایش و اقساط باقیمانده بروزرسانی شد.");
                }

                ResetLoanForm();
                LoadEmployeeLoans(_selectedLoanEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره وام: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateLoanSchedule(int loanId, long firstPayPeriod, short totalInst, short paidInst, long installmentAmount)
        {
            string insertSchedSql = @"INSERT INTO PAY2_LOAN_SCHED (LOAN_ID, INST_NUM, DUE_PERIOD, AMOUNT) 
                                      VALUES (@LOAN_ID, @INST_NUM, @DUE_PERIOD, @AMOUNT)";

            for (short i = (short)(paidInst + 1); i <= totalInst; i++)
            {
                long duePeriod = AddMonthsToPeriodDate(firstPayPeriod, i - 1);
                dbms.DoExecuteSQL(insertSchedSql, new
                {
                    LOAN_ID = loanId,
                    INST_NUM = i,
                    DUE_PERIOD = duePeriod,
                    AMOUNT = installmentAmount
                });
            }
        }

        // تابع کمکی برای جمع ماه‌ها با فرمت شمسی (YYYYMM00)
        private long AddMonthsToPeriodDate(long periodDate, int monthsToAdd)
        {
            int year = (int)(periodDate / 10000);
            int month = (int)((periodDate / 100) % 100);
            month += monthsToAdd;

            while (month > 12)
            {
                month -= 12;
                year++;
            }
            while (month < 1)
            {
                month += 12;
                year--;
            }

            return (year * 10000L) + (month * 100L);
        }

        private void EditLoan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2LoanModel loan)
            {
                CurrentLoan = new Pay2LoanModel
                {
                    LOAN_ID = loan.LOAN_ID,
                    EMP_ID = loan.EMP_ID,
                    WS_ID = loan.WS_ID,
                    LOAN_TYPE = loan.LOAN_TYPE,
                    LOAN_DATE = loan.LOAN_DATE,
                    AMOUNT = loan.AMOUNT,
                    INSTALLMENT = loan.INSTALLMENT,
                    TOTAL_INST = loan.TOTAL_INST,
                    PAID_INST = loan.PAID_INST,
                    FIRST_PAY = loan.FIRST_PAY,
                    PURPOSE = loan.PURPOSE,
                    IS_ACTIVE = loan.IS_ACTIVE
                };
                IsEditingLoan = true;
            }
        }

        private void CancelEditLoan_Click(object sender, RoutedEventArgs e)
        {
            ResetLoanForm();
        }

        private void DeleteLoan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2LoanModel loan)
            {
                if (loan.PAID_INST > 0)
                {
                    MessageBox.Show("این وام دارای اقساط پرداخت شده است و قابل حذف فیزیکی نیست. لطفاً تیک 'فعال' آن را بردارید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("آیا از حذف کامل این وام و اقساط آن اطمینان دارید؟", "حذف وام", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_LOAN_SCHED WHERE LOAN_ID = @Id", new { Id = loan.LOAN_ID });
                        dbms.DoExecuteSQL("DELETE FROM PAY2_LOAN WHERE LOAN_ID = @Id", new { Id = loan.LOAN_ID });

                        ShowToast("وام با موفقیت حذف شد.");
                        LoadEmployeeLoans(_selectedLoanEmployee.EMP_ID);

                        if (CurrentLoan.LOAN_ID == loan.LOAN_ID) ResetLoanForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف وام: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetLoanForm()
        {
            var dateNowString = Tarikh.FullCurrentDate.Replace("/", "");
            long.TryParse(dateNowString, out long defaultDate);

            long defaultPeriod = 14030100;
            if (dateNowString.Length >= 6)
                long.TryParse(dateNowString.Substring(0, 6) + "00", out defaultPeriod);

            CurrentLoan = new Pay2LoanModel { LOAN_DATE = defaultDate, FIRST_PAY = defaultPeriod };
            IsEditingLoan = false;
        }

        private void CloseLoanModal_Click(object sender, RoutedEventArgs e)
        {
            ModalLoanManage.Visibility = Visibility.Collapsed;
        }

        private ObservableCollection<Pay2LeaveBalModel> _employeeLeaveBalances;
        public ObservableCollection<Pay2LeaveBalModel> EmployeeLeaveBalances
        {
            get => _employeeLeaveBalances;
            set { _employeeLeaveBalances = value; OnPropertyChanged(nameof(EmployeeLeaveBalances)); }
        }

        private Pay2LeaveBalModel _currentLeaveBal = new Pay2LeaveBalModel();
        public Pay2LeaveBalModel CurrentLeaveBal
        {
            get => _currentLeaveBal;
            set { _currentLeaveBal = value; OnPropertyChanged(nameof(CurrentLeaveBal)); }
        }

        private string _leaveBalModalTitle;
        public string LeaveBalModalTitle
        {
            get => _leaveBalModalTitle;
            set { _leaveBalModalTitle = value; OnPropertyChanged(nameof(LeaveBalModalTitle)); }
        }

        private bool _isEditingLeaveBal;
        public bool IsEditingLeaveBal
        {
            get => _isEditingLeaveBal;
            set { _isEditingLeaveBal = value; OnPropertyChanged(nameof(IsEditingLeaveBal)); }
        }

        private Pay2EmployeeModel _selectedLeaveBalEmployee;

        private ObservableCollection<Pay2EmployeeModel> _realEmployees;
        public ObservableCollection<Pay2EmployeeModel> RealEmployees
        {
            get => _realEmployees;
            set { _realEmployees = value; OnPropertyChanged(nameof(RealEmployees)); }
        }

        private Pay2EmployeeModel _currentEmployee = new Pay2EmployeeModel();
        public Pay2EmployeeModel CurrentEmployee
        {
            get => _currentEmployee;
            set { _currentEmployee = value; OnPropertyChanged(nameof(CurrentEmployee)); }
        }

        private ObservableCollection<Pay2ContractModel> _employeeContracts;
        public ObservableCollection<Pay2ContractModel> EmployeeContracts
        {
            get => _employeeContracts;
            set { _employeeContracts = value; OnPropertyChanged(nameof(EmployeeContracts)); }
        }

        private Pay2ContractModel _currentContract = new Pay2ContractModel();
        public Pay2ContractModel CurrentContract
        {
            get => _currentContract;
            set { _currentContract = value; OnPropertyChanged(nameof(CurrentContract)); }
        }

        private string _contractModalTitle;
        public string ContractModalTitle
        {
            get => _contractModalTitle;
            set { _contractModalTitle = value; OnPropertyChanged(nameof(ContractModalTitle)); }
        }

        private bool _isEditingContract;
        public bool IsEditingContract
        {
            get => _isEditingContract;
            set { _isEditingContract = value; OnPropertyChanged(nameof(IsEditingContract)); }
        }

        private Pay2EmployeeModel _selectedContractEmployee;

        public class Pay2ContractModel : INotifyPropertyChanged
        {
            private int _conId;
            private int _empId;
            private int _conType = 1;
            private long _startDate;
            private long? _endDate;
            private long? _trialEnd;
            private decimal _weeklyHours = 44.0m;
            private string _notes;

            public int CON_ID { get => _conId; set { _conId = value; OnPropertyChanged(nameof(CON_ID)); } }
            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public int CON_TYPE { get => _conType; set { _conType = value; OnPropertyChanged(nameof(CON_TYPE)); OnPropertyChanged(nameof(ConTypeText)); } }
            public long START_DATE { get => _startDate; set { _startDate = value; OnPropertyChanged(nameof(START_DATE)); } }
            public long? END_DATE { get => _endDate; set { _endDate = value; OnPropertyChanged(nameof(END_DATE)); } }
            public long? TRIAL_END { get => _trialEnd; set { _trialEnd = value; OnPropertyChanged(nameof(TRIAL_END)); } }
            public decimal WEEKLY_HOURS { get => _weeklyHours; set { _weeklyHours = value; OnPropertyChanged(nameof(WEEKLY_HOURS)); } }
            public string NOTES { get => _notes; set { _notes = value; OnPropertyChanged(nameof(NOTES)); } }

            public string ConTypeText => CON_TYPE == 1 ? "دائم" : CON_TYPE == 2 ? "موقت" : CON_TYPE == 3 ? "پیمانی" : "ساعتی";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowContractsModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedContractEmployee = emp;
                ContractModalTitle = $"مدیریت قراردادها: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";
                ResetContractForm();
                LoadEmployeeContracts(emp.EMP_ID);
                ModalContractManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeContracts(int empId)
        {
            try
            {
                string sql = "SELECT * FROM PAY2_CONTRACT WHERE EMP_ID = @Id ORDER BY START_DATE DESC";
                var data = dbms.DoGetDataSQL<Pay2ContractModel>(sql, new { Id = empId });
                EmployeeContracts = new ObservableCollection<Pay2ContractModel>(data ?? Enumerable.Empty<Pay2ContractModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری قراردادها: " + ex.Message);
            }
        }

        private void SaveContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- Validation: تاریخ شروع ---
                if (!Tarikh.IsValidedDate(CurrentContract.START_DATE.ToStringNullSafe()))
                {
                    MessageBox.Show("تاریخ شروع قرارداد نامعتبر است.\nفرمت صحیح: YYYYMMDD — مثال: 14030615",
                                    "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Validation: تاریخ پایان (اگر وارد شده) ---
                if (CurrentContract.END_DATE.HasValue && CurrentContract.END_DATE > 0
                    && !Tarikh.IsValidedDate(CurrentContract.END_DATE.Value.ToStringNullSafe()))
                {
                    MessageBox.Show("تاریخ پایان قرارداد نامعتبر است.\nفرمت صحیح: YYYYMMDD — مثال: 14050615",
                                    "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Validation: پایان آزمایشی (اگر وارد شده) ---
                if (CurrentContract.TRIAL_END.HasValue && CurrentContract.TRIAL_END > 0
                    && !Tarikh.IsValidedDate(CurrentContract.TRIAL_END.Value.ToStringNullSafe()))
                {
                    MessageBox.Show("تاریخ پایان دوره آزمایشی نامعتبر است.\nفرمت صحیح: YYYYMMDD — مثال: 14031001",
                                    "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Validation: تاریخ پایان باید بعد از شروع باشد ---
                if (CurrentContract.END_DATE.HasValue && CurrentContract.END_DATE > 0
                    && CurrentContract.END_DATE < CurrentContract.START_DATE)
                {
                    MessageBox.Show("تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد.",
                                    "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Validation: ساعت هفتگی ---
                if (CurrentContract.WEEKLY_HOURS <= 0 || CurrentContract.WEEKLY_HOURS > 168)
                {
                    MessageBox.Show("ساعت کار هفتگی نامعتبر است (مقدار مجاز: ۱ تا ۱۶۸).",
                                    "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- Null safety: تاریخ‌های اختیاری ---
                if (CurrentContract.END_DATE == 0) CurrentContract.END_DATE = null;
                if (CurrentContract.TRIAL_END == 0) CurrentContract.TRIAL_END = null;

                CurrentContract.EMP_ID = _selectedContractEmployee.EMP_ID;

                if (CurrentContract.CON_ID == 0)
                {
                    string insertSql = @"
                INSERT INTO PAY2_CONTRACT
                    (EMP_ID, CON_TYPE, START_DATE, END_DATE, TRIAL_END, WEEKLY_HOURS, NOTES, CREATED_AT)
                VALUES
                    (@EMP_ID, @CON_TYPE, @START_DATE, @END_DATE, @TRIAL_END, @WEEKLY_HOURS, @NOTES, GETDATE())";

                    dbms.DoExecuteSQL(insertSql, CurrentContract);
                    ShowToast("قرارداد جدید با موفقیت ثبت شد.");
                }
                else
                {
                    string updateSql = @"
                UPDATE PAY2_CONTRACT
                SET CON_TYPE     = @CON_TYPE,
                    START_DATE   = @START_DATE,
                    END_DATE     = @END_DATE,
                    TRIAL_END    = @TRIAL_END,
                    WEEKLY_HOURS = @WEEKLY_HOURS,
                    NOTES        = @NOTES
                WHERE CON_ID = @CON_ID";

                    dbms.DoExecuteSQL(updateSql, CurrentContract);
                    ShowToast("قرارداد با موفقیت ویرایش شد.");
                }

                ResetContractForm();
                LoadEmployeeContracts(_selectedContractEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره قرارداد: " + ex.Message,
                                "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditContract_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ContractModel con)
            {
                CurrentContract = new Pay2ContractModel
                {
                    CON_ID = con.CON_ID,
                    EMP_ID = con.EMP_ID,
                    CON_TYPE = con.CON_TYPE,
                    START_DATE = con.START_DATE,
                    END_DATE = con.END_DATE,
                    TRIAL_END = con.TRIAL_END,
                    WEEKLY_HOURS = con.WEEKLY_HOURS,
                    NOTES = con.NOTES
                };
                IsEditingContract = true;
            }
        }

        private void CancelEditContract_Click(object sender, RoutedEventArgs e)
        {
            ResetContractForm();
        }

        private void DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2ContractModel con)
            {
                var result = MessageBox.Show($"آیا از حذف این قرارداد (شروع: {con.START_DATE}) اطمینان دارید؟", "حذف قرارداد", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_CONTRACT WHERE CON_ID = @Id", new { Id = con.CON_ID });
                        ShowToast("قرارداد با موفقیت حذف شد.");
                        LoadEmployeeContracts(_selectedContractEmployee.EMP_ID);
                        if (CurrentContract.CON_ID == con.CON_ID) ResetContractForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف قرارداد: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetContractForm()
        {
            CurrentContract = new Pay2ContractModel();
            IsEditingContract = false;
        }

        private void LoadEmployees()
        {
            try
            {
                LoadWorkshopsForLookup();
                LoadJobsForLookup();

                string sql = @"
    SELECT 
        e.EMP_ID, e.EMP_CODE, e.WS_ID,
        e.FIRST_NAME, e.LAST_NAME, e.FATHER_NAME,
        e.NATIONAL_CODE, e.ID_NUMBER, e.BIRTH_PLACE, e.BIRTH_DATE,
        e.GENDER, e.NATIONALITY, e.IS_JANBAZ, e.MARITAL,
        e.HIRE_DATE, e.FIRE_DATE,
        e.JOB_ID, e.UNIT, e.EDU_LEVEL,
        e.INS_CODE, e.INS_TYPE,
        e.TAX_EXEMPT, e.REGION_DEPRIVATION,
        e.ACC_T, e.CARD_NO, e.MOBILE, e.BANK_ACC, e.IBAN,
        e.IS_ACTIVE, e.NOTES,
        w.WS_NAME AS WorkshopName,
        j.JOB_NAME AS JobName
    FROM PAY2_EMPLOYEE e
    LEFT JOIN PAY2_WORKSHOP w ON e.WS_ID = w.WS_ID
    LEFT JOIN PAY2_JOB j ON e.JOB_ID = j.JOB_ID
    ORDER BY e.EMP_ID DESC";

                var data = dbms.DoGetDataSQL<Pay2EmployeeModel>(sql);
                RealEmployees = new ObservableCollection<Pay2EmployeeModel>(data ?? Enumerable.Empty<Pay2EmployeeModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری لیست پرسنل: " + ex.Message);
            }
        }

        private void LoadWorkshopsForLookup()
        {
            if (Workshops == null || !Workshops.Any())
            {
                string sql = "SELECT WS_ID, WS_CODE, WS_NAME FROM PAY2_WORKSHOP WHERE IS_ACTIVE = 1";
                var data = dbms.DoGetDataSQL<WorkshopModel>(sql)?.ToList() ?? new System.Collections.Generic.List<WorkshopModel>();
                Workshops = new ObservableCollection<WorkshopModel>(data);

                var allOptionList = new System.Collections.Generic.List<WorkshopModel> { new WorkshopModel { WS_ID = 0, WS_NAME = "همه کارگاه‌ها" } };
                allOptionList.AddRange(data);
                WorkshopsWithAllOption = new ObservableCollection<WorkshopModel>(allOptionList);
            }
        }

        private void LoadJobsForLookup()
        {
            if (Jobs == null || !Jobs.Any())
            {
                string sql = "SELECT JOB_ID, JOB_NAME FROM PAY2_JOB WHERE IS_ACTIVE = 1";
                var data = dbms.DoGetDataSQL<JobModel>(sql);
                Jobs = new ObservableCollection<JobModel>(data ?? Enumerable.Empty<JobModel>());
            }
        }

        private void ShowNewEmpModal_Click(object sender, RoutedEventArgs e)
        {
            CurrentEmployee = new Pay2EmployeeModel
            {
                IS_ACTIVE = true,
                GENDER = "1",
                MARITAL = "2",
                NATIONALITY = 1,
                INS_TYPE = 1,
                IS_JANBAZ = false,
                TAX_EXEMPT = false,
                REGION_DEPRIVATION = 0
            };
            ModalNewEmp.Visibility = Visibility.Visible;
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                CurrentEmployee = new Pay2EmployeeModel
                {
                    EMP_ID = emp.EMP_ID,
                    EMP_CODE = emp.EMP_CODE,
                    WS_ID = emp.WS_ID,
                    FIRST_NAME = emp.FIRST_NAME,
                    LAST_NAME = emp.LAST_NAME,
                    FATHER_NAME = emp.FATHER_NAME,
                    NATIONAL_CODE = emp.NATIONAL_CODE,
                    ID_NUMBER = emp.ID_NUMBER,
                    BIRTH_PLACE = emp.BIRTH_PLACE,
                    BIRTH_DATE = emp.BIRTH_DATE,
                    GENDER = emp.GENDER,
                    NATIONALITY = emp.NATIONALITY,
                    IS_JANBAZ = emp.IS_JANBAZ,
                    MARITAL = emp.MARITAL,
                    HIRE_DATE = emp.HIRE_DATE,
                    FIRE_DATE = emp.FIRE_DATE,
                    JOB_ID = emp.JOB_ID,
                    UNIT = emp.UNIT,
                    EDU_LEVEL = emp.EDU_LEVEL,
                    INS_CODE = emp.INS_CODE,
                    INS_TYPE = emp.INS_TYPE,
                    TAX_EXEMPT = emp.TAX_EXEMPT,
                    REGION_DEPRIVATION = emp.REGION_DEPRIVATION,
                    ACC_T = emp.ACC_T,
                    CARD_NO = emp.CARD_NO,
                    MOBILE = emp.MOBILE,
                    BANK_ACC = emp.BANK_ACC,
                    IBAN = emp.IBAN,
                    IS_ACTIVE = emp.IS_ACTIVE,
                    NOTES = emp.NOTES
                };
                ModalNewEmp.Visibility = Visibility.Visible;
            }
        }

        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentEmployee.EMP_CODE)) { MessageBox.Show("وارد کردن کد پرسنلی الزامی است."); return; }
                if (string.IsNullOrWhiteSpace(CurrentEmployee.FIRST_NAME)) { MessageBox.Show("وارد کردن نام الزامی است."); return; }
                if (string.IsNullOrWhiteSpace(CurrentEmployee.LAST_NAME)) { MessageBox.Show("وارد کردن نام خانوادگی الزامی است."); return; }
                if (CurrentEmployee.WS_ID <= 0) { MessageBox.Show("انتخاب کارگاه محل خدمت الزامی است."); return; }
                if (CurrentEmployee.HIRE_DATE <= 0) { MessageBox.Show("وارد کردن تاریخ شروع به کار (به فرمت شمسی مثلا 14030101) الزامی است."); return; }

                if (CurrentEmployee.EMP_ID == 0)
                {
                    var exists = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_EMPLOYEE WHERE EMP_CODE = @EMP_CODE", new { CurrentEmployee.EMP_CODE }).FirstOrDefault();
                    if (exists > 0)
                    {
                        MessageBox.Show("کد پرسنلی وارد شده تکراری است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // ─── INSERT ───────────────────────────────────────────────────
                    string sql = @"
    INSERT INTO PAY2_EMPLOYEE 
        (EMP_CODE, WS_ID, FIRST_NAME, LAST_NAME, FATHER_NAME,
         NATIONAL_CODE, ID_NUMBER, BIRTH_PLACE, BIRTH_DATE,
         GENDER, NATIONALITY, IS_JANBAZ, MARITAL,
         HIRE_DATE, FIRE_DATE, JOB_ID, UNIT, EDU_LEVEL,
         INS_CODE, INS_TYPE, TAX_EXEMPT, REGION_DEPRIVATION,
         ACC_T, CARD_NO, MOBILE, BANK_ACC, IBAN,
         IS_ACTIVE, NOTES, CREATED_BY)
    VALUES 
        (@EMP_CODE, @WS_ID, @FIRST_NAME, @LAST_NAME, @FATHER_NAME,
         @NATIONAL_CODE, @ID_NUMBER, @BIRTH_PLACE, @BIRTH_DATE,
         @GENDER, @NATIONALITY, @IS_JANBAZ, @MARITAL,
         @HIRE_DATE, @FIRE_DATE, @JOB_ID, @UNIT, @EDU_LEVEL,
         @INS_CODE, @INS_TYPE, @TAX_EXEMPT, @REGION_DEPRIVATION,
         @ACC_T, @CARD_NO, @MOBILE, @BANK_ACC, @IBAN,
         @IS_ACTIVE, @NOTES, @User)";

                    dbms.DoExecuteSQL(sql, new
                    {
                        CurrentEmployee.EMP_CODE,
                        CurrentEmployee.WS_ID,
                        CurrentEmployee.FIRST_NAME,
                        CurrentEmployee.LAST_NAME,
                        CurrentEmployee.FATHER_NAME,
                        CurrentEmployee.NATIONAL_CODE,
                        CurrentEmployee.ID_NUMBER,
                        CurrentEmployee.BIRTH_PLACE,
                        CurrentEmployee.BIRTH_DATE,
                        CurrentEmployee.GENDER,
                        CurrentEmployee.NATIONALITY,
                        CurrentEmployee.IS_JANBAZ,
                        CurrentEmployee.MARITAL,
                        CurrentEmployee.HIRE_DATE,
                        CurrentEmployee.FIRE_DATE,
                        CurrentEmployee.JOB_ID,
                        CurrentEmployee.UNIT,
                        CurrentEmployee.EDU_LEVEL,
                        CurrentEmployee.INS_CODE,
                        CurrentEmployee.INS_TYPE,
                        CurrentEmployee.TAX_EXEMPT,
                        CurrentEmployee.REGION_DEPRIVATION,
                        CurrentEmployee.ACC_T,
                        CurrentEmployee.CARD_NO,
                        CurrentEmployee.MOBILE,
                        CurrentEmployee.BANK_ACC,
                        CurrentEmployee.IBAN,
                        CurrentEmployee.IS_ACTIVE,
                        CurrentEmployee.NOTES,
                        User = Baseknow.USERCOD
                    });
                }
                else
                {
                    // ─── UPDATE ───────────────────────────────────────────────────
                    string sql = @"
    UPDATE PAY2_EMPLOYEE SET
        EMP_CODE           = @EMP_CODE,
        WS_ID              = @WS_ID,
        FIRST_NAME         = @FIRST_NAME,
        LAST_NAME          = @LAST_NAME,
        FATHER_NAME        = @FATHER_NAME,
        NATIONAL_CODE      = @NATIONAL_CODE,
        ID_NUMBER          = @ID_NUMBER,
        BIRTH_PLACE        = @BIRTH_PLACE,
        BIRTH_DATE         = @BIRTH_DATE,
        GENDER             = @GENDER,
        NATIONALITY        = @NATIONALITY,
        IS_JANBAZ          = @IS_JANBAZ,
        MARITAL            = @MARITAL,
        HIRE_DATE          = @HIRE_DATE,
        FIRE_DATE          = @FIRE_DATE,
        JOB_ID             = @JOB_ID,
        UNIT               = @UNIT,
        EDU_LEVEL          = @EDU_LEVEL,
        INS_CODE           = @INS_CODE,
        INS_TYPE           = @INS_TYPE,
        TAX_EXEMPT         = @TAX_EXEMPT,
        REGION_DEPRIVATION = @REGION_DEPRIVATION,
        ACC_T              = @ACC_T,
        CARD_NO            = @CARD_NO,
        MOBILE             = @MOBILE,
        BANK_ACC           = @BANK_ACC,
        IBAN               = @IBAN,
        IS_ACTIVE          = @IS_ACTIVE,
        NOTES              = @NOTES
    WHERE EMP_ID = @EMP_ID";

                    dbms.DoExecuteSQL(sql, CurrentEmployee);
                }

                CloseAllModals();
                ShowToast("اطلاعات پرسنل با موفقیت ذخیره شد.");
                LoadEmployees();
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601)
            {
                MessageBox.Show("کد پرسنلی یا کد ملی وارد شده قبلاً در سیستم ثبت شده است (تکراری است).", "خطای یکتایی مقادیر", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره اطلاعات پرسنل: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Pay2EmployeeModel : INotifyPropertyChanged
        {
            public int EMP_ID { get; set; }
            public string EMP_CODE { get; set; }
            public int WS_ID { get; set; }
            public string FIRST_NAME { get; set; }
            public string LAST_NAME { get; set; }
            public string NATIONAL_CODE { get; set; }
            public string GENDER { get; set; } = "1";
            public string MARITAL { get; set; } = "2";
            public long HIRE_DATE { get; set; } = Convert.ToInt64(Tarikh.FullCurrentDate);
            public int? JOB_ID { get; set; }
            public string INS_CODE { get; set; }
            public string? ACC_T { get; set; }
            public bool IS_ACTIVE { get; set; } = true;

            // ─── مشخصات فردی تکمیلی ───────────────────────────────────────
            public string FATHER_NAME { get; set; }
            public string ID_NUMBER { get; set; }
            public string BIRTH_PLACE { get; set; }
            public long? BIRTH_DATE { get; set; }
            public byte NATIONALITY { get; set; } = 1;     // 1=ایرانی، 2=خارجی
            public bool IS_JANBAZ { get; set; } = false;

            // ─── اشتغال تکمیلی ───────────────────────────────────────────
            public long? FIRE_DATE { get; set; }
            public byte? UNIT { get; set; }
            public byte? EDU_LEVEL { get; set; }

            // ─── بیمه تکمیلی ─────────────────────────────────────────────
            public byte INS_TYPE { get; set; } = 1;        // 1=معمولی، 2=تبصره‌ای، 3=معاف

            // ─── مالیات ──────────────────────────────────────────────────
            public bool TAX_EXEMPT { get; set; } = false;
            public byte REGION_DEPRIVATION { get; set; } = 0;

            // ─── تماس و پرداخت ───────────────────────────────────────────
            public string CARD_NO { get; set; }
            public string MOBILE { get; set; }
            public string BANK_ACC { get; set; }
            public string IBAN { get; set; }

            // ─── یادداشت ─────────────────────────────────────────────────
            public string NOTES { get; set; }

            public string WorkshopName { get; set; }
            public string JobName { get; set; }

            public string StatusText => IS_ACTIVE ? "فعال" : "غیرفعال";
            public string StatusBgColor => IS_ACTIVE ? "#ecfdf5" : "#fef2f2";
            public string StatusTextColor => IS_ACTIVE ? "#059669" : "#ef4444";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _isToastVisible;
        public bool IsToastVisible
        {
            get => _isToastVisible;
            set { _isToastVisible = value; OnPropertyChanged(nameof(IsToastVisible)); }
        }

        private string _toastMessage;
        public string ToastMessage
        {
            get => _toastMessage;
            set { _toastMessage = value; OnPropertyChanged(nameof(ToastMessage)); }
        }

        private bool _isCalcEnabled = true;
        public bool IsCalcEnabled
        {
            get => _isCalcEnabled;
            set { _isCalcEnabled = value; OnPropertyChanged(nameof(IsCalcEnabled)); }
        }

        private string _statusText = "پیش‌نویس";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }


        public WIN_SALARY_MANAGE()
        {
            InitializeComponent();
            DataContext = this;
            LoadMockData();
        }

        private void LoadMockData()
        {
            Employees = new ObservableCollection<EmployeeModel>
            {
                new EmployeeModel { Code="1050", FullName="علی احمدی", Job="تکنسین تولید", AccT="T-1050" },
                new EmployeeModel { Code="1051", FullName="سارا رضایی", Job="کارشناس فروش", AccT="T-1051" },
                new EmployeeModel { Code="1052", FullName="محمد کریمی", Job="مدیر مالی", AccT="T-1052" },
                new EmployeeModel { Code="1053", FullName="زهرا موسوی", Job="نیروی خدماتی", AccT="T-1053" }
            };

            Attendance = new ObservableCollection<AttendanceModel>
            {
                new AttendanceModel { Code="1050", FullName="علی احمدی", Days="31", DaysB="31", OtNormal="24.5", LeaveDays="1" },
                new AttendanceModel { Code="1051", FullName="سارا رضایی", Days="31", DaysB="31", OtNormal="12", LeaveDays="2.5" },
                new AttendanceModel { Code="1052", FullName="محمد کریمی", Days="29", DaysB="29", OtNormal="5", LeaveDays="0" }
            };

            Advances = new ObservableCollection<AdvanceModel>
            {
                new AdvanceModel { Code="1050", FullName="علی احمدی", RawBalanceText="۷۵,۰۰۰,۰۰۰", AdvanceText="۷۵,۰۰۰,۰۰۰" },
                new AdvanceModel { Code="1051", FullName="سارا رضایی", RawBalanceText="۲۰,۰۰۰,۰۰۰", AdvanceText="۱۵,۰۰۰,۰۰۰" },
                new AdvanceModel { Code="1052", FullName="محمد کریمی", RawBalanceText="-۵,۰۰۰,۰۰۰", AdvanceText="0" }
            };

            Payroll = new ObservableCollection<PayrollModel>
            {
                new PayrollModel { FullName="علی احمدی", Gross="۱۸۵,۰۰۰,۰۰۰", InsW="۱۲,۹۵۰,۰۰۰", Tax="۸,۵۰۰,۰۰۰", Advance="۷۵,۰۰۰,۰۰۰", Net="۸۸,۵۵۰,۰۰۰" },
                new PayrollModel { FullName="سارا رضایی", Gross="۲۲۰,۰۰۰,۰۰۰", InsW="۱۵,۴۰۰,۰۰۰", Tax="۱۲,۵۰۰,۰۰۰", Advance="۱۵,۰۰۰,۰۰۰", Net="۱۷۷,۱۰۰,۰۰۰" }
            };
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (TabDashboard == null) return;

            var radio = sender as RadioButton;
            var tag = radio?.Tag?.ToString();

            TabDashboard.Visibility = Visibility.Collapsed;
            TabWorkshops.Visibility = Visibility.Collapsed;
            TabEmployees.Visibility = Visibility.Collapsed;
            TabAttendance.Visibility = Visibility.Collapsed;
            TabAdvances.Visibility = Visibility.Collapsed;
            TabPayroll.Visibility = Visibility.Collapsed;
            TabSettings.Visibility = Visibility.Collapsed;

            TabItemDefs.Visibility = Visibility.Collapsed;



            switch (tag)
            {
                case "dashboard": TabDashboard.Visibility = Visibility.Visible; TxtHeaderTitle.Text = "داشبورد"; break;
                case "workshops":
                    TabWorkshops.Visibility = Visibility.Visible;
                    LoadWorkshops();
                    TxtHeaderTitle.Text = "مدیریت کارگاه‌ها و حسابداری"; break;
                case "employees":
                    TabEmployees.Visibility = Visibility.Visible;
                    LoadEmployees();
                    TxtHeaderTitle.Text = "پرسنل و احکام";
                    break;
                case "attendance": TabAttendance.Visibility = Visibility.Visible; TxtHeaderTitle.Text = "کارکرد و مرخصی"; break;
                case "advances": TabAdvances.Visibility = Visibility.Visible; TxtHeaderTitle.Text = "مساعده هوشمند"; break;
                case "payroll": TabPayroll.Visibility = Visibility.Visible; TxtHeaderTitle.Text = "محاسبه حقوق"; break;
                case "settings":
                    TabSettings.Visibility = Visibility.Visible;
                    LoadAllPay2Settings();
                    TxtHeaderTitle.Text = "تنظیمات مرکزی (PAY2_CONFIG)";
                    break;

                case "itemdefs":
                    TabItemDefs.Visibility = Visibility.Visible;
                    LoadItemDefs();
                    ResetItemDefForm();
                    TxtHeaderTitle.Text = "تعریف آیتم‌های حقوقی (PAY2_ITEM_DEF)";
                    break;
            }

        }

        // =========================================================================
        // --- PAY2_PERIOD & PAY2_RUN (Payroll Engine) Properties & Logic ---
        // =========================================================================

        private int _selectedPayrollWsId;
        public int SelectedPayrollWsId
        {
            get => _selectedPayrollWsId;
            set { _selectedPayrollWsId = value; OnPropertyChanged(nameof(SelectedPayrollWsId)); }
        }

        private long _payrollSearchDate = 14030100;
        public long PayrollSearchDate
        {
            get => _payrollSearchDate;
            set { _payrollSearchDate = value; OnPropertyChanged(nameof(PayrollSearchDate)); }
        }

        private Pay2PeriodModel _currentPayrollPeriod;
        public Pay2PeriodModel CurrentPayrollPeriod
        {
            get => _currentPayrollPeriod;
            set { _currentPayrollPeriod = value; OnPropertyChanged(nameof(CurrentPayrollPeriod)); }
        }

        private bool _isPeriodOpened;
        public bool IsPeriodOpened
        {
            get => _isPeriodOpened;
            set { _isPeriodOpened = value; OnPropertyChanged(nameof(IsPeriodOpened)); OnPropertyChanged(nameof(IsPeriodNotOpened)); }
        }

        public bool IsPeriodNotOpened => !_isPeriodOpened;

        private ObservableCollection<Pay2RunLineDisplayModel> _runLines;
        public ObservableCollection<Pay2RunLineDisplayModel> RunLines
        {
            get => _runLines;
            set { _runLines = value; OnPropertyChanged(nameof(RunLines)); }
        }

        public class Pay2PeriodModel : INotifyPropertyChanged
        {
            private int _perId;
            private int _wsId;
            private long _periodDate;
            private byte _status;

            public int PER_ID { get => _perId; set { _perId = value; OnPropertyChanged(nameof(PER_ID)); } }
            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public long PERIOD_DATE { get => _periodDate; set { _periodDate = value; OnPropertyChanged(nameof(PERIOD_DATE)); } }
            public byte STATUS { get => _status; set { _status = value; OnPropertyChanged(nameof(STATUS)); OnPropertyChanged(nameof(StatusText)); } }

            public string StatusText => STATUS == 1 ? "باز / در حال کارکرد" : STATUS == 2 ? "بسته / آماده محاسبه" : STATUS == 3 ? "محاسبه شده" : "سند حسابداری صادر شده";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // جایگزینی و ارتقای مدل Pay2RunLineDisplayModel (حدود خطوط 477)
        public class Pay2RunLineDisplayModel : INotifyPropertyChanged
        {
            public int RUN_ID { get; set; }
            public int EMP_ID { get; set; }
            public string EMP_CODE { get; set; }
            public string FULL_NAME { get; set; }
            public decimal WORK_DAYS { get; set; }
            public long GROSS_PAY { get; set; }
            public long INS_WORKER { get; set; }
            public long TAX_AMOUNT { get; set; }
            public long ADVANCE_DED { get; set; }
            public long TOTAL_DED { get; set; }
            public long NET_PAY { get; set; }

            // ▼▼▼ این ۵ خط باید تغییر کنند ▼▼▼
            public string GROSS_PAY_STR => GROSS_PAY.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string INS_WORKER_STR => INS_WORKER.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string TAX_AMOUNT_STR => TAX_AMOUNT.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string ADVANCE_DED_STR => ADVANCE_DED.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string NET_PAY_STR => NET_PAY.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));

            public event PropertyChangedEventHandler PropertyChanged;
        }

        // اضافه کردن مدل‌های جدید فیش حقوقی
        public class PayslipItemModel
        {
            public string ItemName { get; set; }
            public long Amount { get; set; }
            public string AmountStr => Amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
        }

        public class RunDetailTempModel
        {
            public long AMOUNT { get; set; }
            public string ITEM_NAME { get; set; }
            public byte ITEM_TYPE { get; set; }
        }

        public class PayslipDisplayModel : INotifyPropertyChanged
        {
            public string FullName { get; set; }
            public string EmpCode { get; set; }
            public string PeriodName { get; set; }
            public string WorkshopName { get; set; }
            public decimal WorkDays { get; set; }
            public long TotalEarnings { get; set; }
            public long TotalDeductions { get; set; }
            public long NetPay { get; set; }

            public string TotalEarningsStr => TotalEarnings.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string TotalDeductionsStr => TotalDeductions.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
            public string NetPayStr => NetPay.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));

            public string WorkDaysStr => WorkDays.ToString("0.##");

            public ObservableCollection<PayslipItemModel> Earnings { get; set; } = new ObservableCollection<PayslipItemModel>();
            public ObservableCollection<PayslipItemModel> Deductions { get; set; } = new ObservableCollection<PayslipItemModel>();

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private PayslipDisplayModel _currentPayslip;
        public PayslipDisplayModel CurrentPayslip
        {
            get => _currentPayslip;
            set { _currentPayslip = value; OnPropertyChanged(nameof(CurrentPayslip)); }
        }

        // متد کلیک روی دکمه مشاهده فیش
        private void ViewPayslip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2RunLineDisplayModel runLine)
            {
                try
                {
                    var payslip = new PayslipDisplayModel
                    {
                        FullName = runLine.FULL_NAME,
                        EmpCode = runLine.EMP_CODE,
                        TotalEarnings = runLine.GROSS_PAY,
                        TotalDeductions = runLine.TOTAL_DED,
                        NetPay = runLine.NET_PAY,
                        WorkDays = runLine.WORK_DAYS
                    };

                    // خواندن اطلاعات ماه و کارگاه
                    var ws = Workshops.FirstOrDefault(w => w.WS_ID == SelectedPayrollWsId);
                    payslip.WorkshopName = ws?.WS_NAME ?? "کارگاه نامشخص";
                    payslip.PeriodName = PayrollSearchDate.ToString();

                    // خواندن ریز آیتم‌های حقوق (PAY2_RUN_DETAIL) با مدل strongly-typed
                    string sqlDetails = @"
                        SELECT D.AMOUNT, I.ITEM_NAME, I.ITEM_TYPE
                        FROM PAY2_RUN_DETAIL D
                        INNER JOIN PAY2_ITEM_DEF I ON D.ITEM_ID = I.ITEM_ID
                        WHERE D.RUN_ID = @RUN_ID AND D.EMP_ID = @EMP_ID
                        ORDER BY I.SORT_ORDER ASC";

                    var detailRows = dbms.DoGetDataSQL<RunDetailTempModel>(sqlDetails, new { RUN_ID = runLine.RUN_ID, EMP_ID = runLine.EMP_ID });

                    if (detailRows != null)
                    {
                        foreach (var row in detailRows)
                        {
                            if (row.AMOUNT > 0)
                            {
                                var itemModel = new PayslipItemModel { ItemName = row.ITEM_NAME, Amount = row.AMOUNT };

                                // Type 1, 2 = پرداختی | Type 3, 4 = کسورات
                                if (row.ITEM_TYPE == 1 || row.ITEM_TYPE == 2)
                                {
                                    payslip.Earnings.Add(itemModel);
                                }
                                else if (row.ITEM_TYPE == 3 || row.ITEM_TYPE == 4)
                                {
                                    payslip.Deductions.Add(itemModel);
                                }
                            }
                        }
                    }

                    CurrentPayslip = payslip;
                    ModalPayslip.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در بارگذاری جزئیات فیش: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintPayslip_Click(object sender, RoutedEventArgs e)
        {
            // اتصال به Stimulsoft Reports برای چاپ فیش حقوقی در آینده در این متد قرار می‌گیرد.
            ShowToast("امکان چاپ فیش حقوقی به زودی فعال خواهد شد.");
        }

        private async void SyncAdvances_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.Content = "در حال خواندن اسناد...";

            await Task.Delay(1500);

            if (btn != null) btn.Content = "بروزرسانی از حسابداری";
            ShowToast("اطلاعات با موفقیت از سیستم حسابداری (DEED_DTL) بروزرسانی شد.");
        }

        // --- متغیرهای کلاسی برای نگهداری وضعیت اجرای جاری موتور حقوق ---
        private int _currentRunId = 0;
        private byte _currentRunStatus = 0;

        public class RunInfoModel
        {
            public int RUN_ID { get; set; }
            public byte STATUS { get; set; }
        }

        // =========================================================================
        // --- PAY2_SETTLEMENT (Employee Settlement) Properties & Logic ---
        // =========================================================================

        private ObservableCollection<Pay2SettlementModel> _employeeSettlements;
        public ObservableCollection<Pay2SettlementModel> EmployeeSettlements
        {
            get => _employeeSettlements;
            set { _employeeSettlements = value; OnPropertyChanged(nameof(EmployeeSettlements)); }
        }

        private SettlementInputModel _settlementInput = new SettlementInputModel();
        public SettlementInputModel SettlementInput
        {
            get => _settlementInput;
            set { _settlementInput = value; OnPropertyChanged(nameof(SettlementInput)); }
        }

        private string _settlementModalTitle;
        public string SettlementModalTitle
        {
            get => _settlementModalTitle;
            set { _settlementModalTitle = value; OnPropertyChanged(nameof(SettlementModalTitle)); }
        }

        private Pay2EmployeeModel _selectedSettlementEmployee;

        public class SettlementInputModel : INotifyPropertyChanged
        {
            private long _settleDate;
            private long _endDate;
            private long _otherIncome = 0;
            private long _otherDed = 0;
            private long _prevCredit = 0;

            public long SETTLE_DATE { get => _settleDate; set { _settleDate = value; OnPropertyChanged(nameof(SETTLE_DATE)); } }
            public long END_DATE { get => _endDate; set { _endDate = value; OnPropertyChanged(nameof(END_DATE)); } }
            public long OTHER_INCOME { get => _otherIncome; set { _otherIncome = value; OnPropertyChanged(nameof(OTHER_INCOME)); } }
            public long OTHER_DED { get => _otherDed; set { _otherDed = value; OnPropertyChanged(nameof(OTHER_DED)); } }
            public long PREV_CREDIT { get => _prevCredit; set { _prevCredit = value; OnPropertyChanged(nameof(PREV_CREDIT)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class Pay2SettlementModel : INotifyPropertyChanged
        {
            public int SET_ID { get; set; }
            public int EMP_ID { get; set; }
            public long SETTLE_DATE { get; set; }
            public long END_DATE { get; set; }
            public int SENIORITY_DAYS { get; set; }
            public decimal SENIORITY_YEARS { get; set; }
            public long LAST_SALARY { get; set; }
            public decimal LEAVE_BAL_DAYS { get; set; }
            public long EIDI { get; set; }
            public long BON { get; set; }
            public long LEAVE_PAY { get; set; }
            public long SANAVAT { get; set; }
            public long PREV_CREDIT { get; set; }
            public long OTHER_INCOME { get; set; }
            public long PREV_DEBIT { get; set; }
            public long EIDI_TAX { get; set; }
            public long LOAN_BALANCE { get; set; }
            public long OTHER_DED { get; set; }
            public byte STATUS { get; set; }
            public long TOTAL_INCOME { get; set; }
            public long TOTAL_DED { get; set; }
            public long NET_SETTLE { get; set; }

            public string StatusText => STATUS == 1 ? "پیش‌نویس" : STATUS == 2 ? "تأیید نهایی" : "سند صادر شده";
            public string StatusColor => STATUS == 1 ? "#d97706" : STATUS == 2 ? "#059669" : "#2563eb";
            public bool CanEdit => STATUS == 1;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowSettlementModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedSettlementEmployee = emp;
                SettlementModalTitle = $"تسویه حساب پرسنل: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";

                ResetSettlementForm();
                LoadEmployeeSettlements(emp.EMP_ID);

                ModalSettlementManage.Visibility = Visibility.Visible;
            }
        }

        private void ResetSettlementForm()
        {
            var dateNowString = Tarikh.FullCurrentDate.Replace("/", "");
            long.TryParse(dateNowString, out long defaultDate);

            SettlementInput = new SettlementInputModel
            {
                SETTLE_DATE = defaultDate,
                END_DATE = defaultDate,
                OTHER_DED = 0,
                OTHER_INCOME = 0,
                PREV_CREDIT = 0
            };
        }

        private void LoadEmployeeSettlements(int empId)
        {
            try
            {
                string sql = @"
                    SELECT SET_ID, EMP_ID, SETTLE_DATE, END_DATE, SENIORITY_DAYS, SENIORITY_YEARS, LAST_SALARY, 
                           LEAVE_BAL_DAYS, EIDI, BON, LEAVE_PAY, SANAVAT, PREV_CREDIT, OTHER_INCOME, 
                           PREV_DEBIT, EIDI_TAX, LOAN_BALANCE, OTHER_DED, STATUS, 
                           TOTAL_INCOME, TOTAL_DED, NET_SETTLE
                    FROM PAY2_SETTLEMENT 
                    WHERE EMP_ID = @Id 
                    ORDER BY SETTLE_DATE DESC";

                var data = dbms.DoGetDataSQL<Pay2SettlementModel>(sql, new { Id = empId });
                EmployeeSettlements = new ObservableCollection<Pay2SettlementModel>(data ?? Enumerable.Empty<Pay2SettlementModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری لیست تسویه‌ها: " + ex.Message);
            }
        }

        private void CalculateSettlement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SettlementInput.SETTLE_DATE <= 13000000 || SettlementInput.END_DATE <= 13000000)
                {
                    MessageBox.Show("تاریخ تسویه و تاریخ پایان کار نامعتبر است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existsDraft = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_SETTLEMENT WHERE EMP_ID = @Id AND STATUS = 1", new { Id = _selectedSettlementEmployee.EMP_ID }).FirstOrDefault();
                if (existsDraft > 0)
                {
                    MessageBox.Show("این پرسنل یک تسویه حساب پیش‌نویس دارد. لطفاً ابتدا آن را حذف یا نهایی کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string sql = @"
                    DECLARE @newId INT; 
                    EXEC SP_PAY2_CALC_SETTLE 
                        @EMP_ID = @EMP_ID, 
                        @WS_ID = @WS_ID, 
                        @SETTLE_DATE = @SETTLE_DATE, 
                        @END_DATE = @END_DATE, 
                        @PREV_CREDIT = @PREV_CREDIT, 
                        @OTHER_INCOME = @OTHER_INCOME, 
                        @OTHER_DED = @OTHER_DED, 
                        @CALC_BY = @User, 
                        @NEW_SET_ID = @newId OUTPUT;";

                dbms.DoExecuteSQL(sql, new
                {
                    EMP_ID = _selectedSettlementEmployee.EMP_ID,
                    WS_ID = _selectedSettlementEmployee.WS_ID,
                    SETTLE_DATE = SettlementInput.SETTLE_DATE,
                    END_DATE = SettlementInput.END_DATE,
                    PREV_CREDIT = SettlementInput.PREV_CREDIT,
                    OTHER_INCOME = SettlementInput.OTHER_INCOME,
                    OTHER_DED = SettlementInput.OTHER_DED,
                    User = Baseknow.USERCOD
                });

                ShowToast("تسویه حساب با موفقیت محاسبه و به عنوان پیش‌نویس ثبت شد.");
                LoadEmployeeSettlements(_selectedSettlementEmployee.EMP_ID);
                LoadEmployees(); // به روزرسانی گرید پرسنل برای نمایش وضعیت غیرفعال
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در محاسبه تسویه حساب: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinalizeSettlement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2SettlementModel settle)
            {
                var result = MessageBox.Show("آیا از تأیید نهایی این تسویه حساب اطمینان دارید؟ تغییر پس از تأیید ممکن نیست.", "تأیید نهایی", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("EXEC SP_PAY2_FINALIZE_SETTLE @SET_ID = @Id, @APPROVED_BY = @User", new { Id = settle.SET_ID, User = Baseknow.USERCOD });
                        ShowToast("تسویه حساب با موفقیت نهایی شد.");
                        LoadEmployeeSettlements(_selectedSettlementEmployee.EMP_ID);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در نهایی‌سازی تسویه: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteSettlement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2SettlementModel settle)
            {
                var result = MessageBox.Show("آیا از حذف این پیش‌نویس تسویه حساب اطمینان دارید؟ با این کار پرسنل مجدداً در حالت فعال قرار می‌گیرد.", "حذف تسویه", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // حذف تسویه
                        dbms.DoExecuteSQL("DELETE FROM PAY2_SETTLEMENT WHERE SET_ID = @Id", new { Id = settle.SET_ID });

                        // برگرداندن وضعیت پرسنل به حالت فعال (چون محاسبه تسویه باعث غیرفعال شدن می‌شود)
                        dbms.DoExecuteSQL("UPDATE PAY2_EMPLOYEE SET IS_ACTIVE = 1, FIRE_DATE = NULL WHERE EMP_ID = @Id", new { Id = _selectedSettlementEmployee.EMP_ID });

                        ShowToast("تسویه حساب با موفقیت حذف شد و پرسنل فعال گردید.");
                        LoadEmployeeSettlements(_selectedSettlementEmployee.EMP_ID);
                        LoadEmployees(); // به روزرسانی گرید اصلی
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف تسویه حساب: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CloseSettlementModal_Click(object sender, RoutedEventArgs e)
        {
            ModalSettlementManage.Visibility = Visibility.Collapsed;
        }

        private void BtnLoadPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPayrollWsId <= 0 || PayrollSearchDate <= 13000000)
            {
                MessageBox.Show("لطفاً کارگاه و ماه معتبر (مثال: 14030100) را انتخاب کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string sqlPeriod = "SELECT * FROM PAY2_PERIOD WHERE WS_ID = @WsId AND PERIOD_DATE = @Date";
                var period = dbms.DoGetDataSQL<Pay2PeriodModel>(sqlPeriod, new { WsId = SelectedPayrollWsId, Date = PayrollSearchDate }).FirstOrDefault();

                CurrentPayrollPeriod = period;
                IsPeriodOpened = (period != null);

                if (IsPeriodOpened)
                {
                    LoadRunLines(period.PER_ID);
                    StatusText = period.StatusText;
                }
                else
                {
                    RunLines = new ObservableCollection<Pay2RunLineDisplayModel>();
                    _currentRunId = 0;
                    _currentRunStatus = 0;
                    StatusText = "دوره باز نشده است";
                }
            }
            catch (Exception ex)
            {
                ShowToast("خطا در فراخوانی دوره: " + ex.Message);
            }
        }

        private void BtnOpenPeriod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = @"
                    DECLARE @newId INT; 
                    EXEC SP_PAY2_NEW_PERIOD @WS_ID=@WsId, @PERIOD_DATE=@Date, @HOLIDAY_DAYS=0, @OPENED_BY=@User, @NEW_PER_ID=@newId OUTPUT;
                    SELECT @newId;";

                dbms.DoExecuteSQL(sql, new
                {
                    WsId = SelectedPayrollWsId,
                    Date = PayrollSearchDate,
                    User = Baseknow.USERCOD
                });

                ShowToast("دوره ماهیانه با موفقیت ایجاد و باز شد.");
                BtnLoadPayroll_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در باز کردن دوره: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRunLines(int periodId)
        {
            // ۱. یافتن آخرین شناسه اجرا برای این دوره
            string runSql = "SELECT TOP 1 RUN_ID, STATUS FROM PAY2_RUN WHERE PER_ID = @PER_ID AND IS_LATEST = 1 ORDER BY RUN_ID DESC";
            var runInfo = dbms.DoGetDataSQL<RunInfoModel>(runSql, new { PER_ID = periodId }).FirstOrDefault();

            if (runInfo != null)
            {
                _currentRunId = runInfo.RUN_ID;
                _currentRunStatus = runInfo.STATUS;
            }
            else
            {
                _currentRunId = 0;
                _currentRunStatus = 0;
            }

            // ۲. بارگذاری ریز نتایج
            string sql = @"
                SELECT R.RUN_ID, E.EMP_ID, E.EMP_CODE, E.FIRST_NAME + ' ' + E.LAST_NAME AS FULL_NAME,
                       L.WORK_DAYS, L.GROSS_PAY, L.INS_WORKER, L.TAX_AMOUNT, L.ADVANCE_DED, L.TOTAL_DED, L.NET_PAY
                FROM PAY2_RUN_LINE L
                INNER JOIN PAY2_EMPLOYEE E ON L.EMP_ID = E.EMP_ID
                INNER JOIN PAY2_RUN R ON L.RUN_ID = R.RUN_ID
                WHERE R.PER_ID = @PER_ID AND R.IS_LATEST = 1
                ORDER BY E.LAST_NAME, E.FIRST_NAME";

            var data = dbms.DoGetDataSQL<Pay2RunLineDisplayModel>(sql, new { PER_ID = periodId });
            RunLines = new ObservableCollection<Pay2RunLineDisplayModel>(data ?? Enumerable.Empty<Pay2RunLineDisplayModel>());
        }

        private async void RunPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPayrollPeriod == null) return;

            IsCalcEnabled = false;
            var btn = sender as Button;
            if (btn != null) btn.Content = "در حال پردازش موتور حقوق...";

            int isRerun = (_currentRunId > 0) ? 1 : 0;

            try
            {
                await Task.Run(() =>
                {
                    string sql = @"
                        DECLARE @newId INT; 
                        EXEC SP_PAY2_CALC_RUN 
                            @WS_ID = @WsId, 
                            @PER_ID = @PerId, 
                            @PAYROLL_N_S = 0, 
                            @CALC_BY = @User, 
                            @IS_RERUN = @IsRerun, 
                            @NEW_RUN_ID = @newId OUTPUT;";

                    dbms.DoExecuteSQL(sql, new
                    {
                        WsId = CurrentPayrollPeriod.WS_ID,
                        PerId = CurrentPayrollPeriod.PER_ID,
                        User = Baseknow.USERCOD,
                        IsRerun = isRerun
                    });
                });

                ShowToast("محاسبه حقوق پرسنل با موفقیت انجام شد.");
                BtnLoadPayroll_Click(null, null); // بارگذاری مجدد نتایج و به‌روزرسانی شناسه اجرا
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // فیلتر کردن پیام خطای اصلی دیتابیس و نادیده گرفتن اخطارهای Warning (مثل خطاهای null aggregate)
                string cleanMsg = sqlEx.Errors[0].Message;

                MessageBox.Show("توقف محاسبه توسط موتور دیتابیس:\n\n" + cleanMsg,
                                "اعتبارسنجی قوانین کسب‌وکار",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در پردازش موتور حقوق: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsCalcEnabled = true;
                if (btn != null) btn.Content = "اجرای موتور / بازمحاسبه";
            }
        }

        private void BtnRevertPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRunId <= 0)
            {
                MessageBox.Show("هیچ اجرای محاسبه‌ای برای این دوره یافت نشد.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("آیا از برگشت این محاسبه به حالت پیش‌نویس اطمینان دارید؟ نتایج قبلی پاک خواهند شد.", "برگشت محاسبه", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    dbms.DoExecuteSQL("EXEC SP_PAY2_REVERT_RUN @RUN_ID = @RunId, @REVERT_BY = @User",
                        new { RunId = _currentRunId, User = Baseknow.USERCOD });

                    ShowToast("محاسبه با موفقیت برگشت داده شد.");
                    BtnLoadPayroll_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در برگشت محاسبه: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FinalizePayroll_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRunId <= 0)
            {
                MessageBox.Show("ابتدا باید محاسبه حقوق را اجرا کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentRunStatus != 1)
            {
                MessageBox.Show("این محاسبه قبلاً نهایی شده و یا سند آن صادر شده است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("آیا از نهایی کردن این محاسبه اطمینان دارید؟ با این کار دوره قفل شده و تغییرات جدید اعمال نخواهد شد.", "تایید نهایی", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // ۱. نهایی کردن ران
                    dbms.DoExecuteSQL("EXEC SP_PAY2_FINALIZE_RUN @RUN_ID = @RunId, @FINAL_BY = @User",
                        new { RunId = _currentRunId, User = Baseknow.USERCOD });

                    // ۲. بستن دوره
                    dbms.DoExecuteSQL("EXEC SP_PAY2_CLOSE_PERIOD @PER_ID = @PerId, @CLOSE_BY = @User",
                        new { PerId = CurrentPayrollPeriod.PER_ID, User = Baseknow.USERCOD });

                    ShowToast("اجرای حقوق نهایی شد و دوره با موفقیت بسته شد.");
                    BtnLoadPayroll_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در نهایی‌سازی دوره: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveModal_Click(object sender, RoutedEventArgs e)
        {
            CloseAllModals();
            ShowToast("عملیات با موفقیت انجام شد.");
        }

        private void ShowAdvSettingsModal_Click(object sender, RoutedEventArgs e) => ModalAdvSettings.Visibility = Visibility.Visible;

        private void ShowTaxBracketModal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = "SELECT * FROM PAY2_TAX_BRACKET ORDER BY TAX_YEAR DESC, SORT_ORDER ASC";
                var data = dbms.DoGetDataSQL<TaxBracketModel>(sql);

                TaxBrackets = data != null
                    ? new ObservableCollection<TaxBracketModel>(data)
                    : new ObservableCollection<TaxBracketModel>();

                // ساخت CollectionView برای فیلتر
                TaxBracketsView = CollectionViewSource.GetDefaultView(TaxBrackets);
                TaxBracketsView.Filter = TaxBracketYearFilter;
                TaxBrackets.CollectionChanged += (s, ev) => UpdateTaxBracketSummary();

                // بازسازی ComboBox سال‌ها
                RebuildTaxYearCombo();
                UpdateTaxBracketSummary();

                ModalTaxBracket.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری مالیات پلکانی: " + ex.Message);
            }
        }

        private void SaveTaxBrackets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TaxBrackets == null || !TaxBrackets.Any()) return;

                var validBrackets = TaxBrackets
                    .Where(b => b.TAX_YEAR > 1300 && b.UPPER_LIMIT > 0 && b.RATE_PCT > 0)
                    .ToList();

                if (!validBrackets.Any())
                {
                    ShowToast("هیچ ردیف معتبری برای ذخیره وجود ندارد.");
                    return;
                }

                // اعتبارسنجی: SORT_ORDER تکراری در یک سال
                var duplicates = validBrackets
                    .GroupBy(b => new { b.TAX_YEAR, b.SORT_ORDER })
                    .Any(g => g.Count() > 1);

                if (duplicates)
                {
                    ShowToast("خطا: شماره پله تکراری در یک سال وجود دارد. لطفاً اصلاح کنید.");
                    return;
                }

                // محاسبه FIXED_TAX per سال
                var groupedByYear = validBrackets.GroupBy(b => b.TAX_YEAR);
                foreach (var group in groupedByYear)
                {
                    long fixedTaxAccumulator = 0;
                    long previousLimit = 0;
                    foreach (var bracket in group.OrderBy(b => b.SORT_ORDER))
                    {
                        bracket.FIXED_TAX = fixedTaxAccumulator;
                        if (bracket.UPPER_LIMIT > previousLimit)
                            fixedTaxAccumulator += (long)((bracket.UPPER_LIMIT - previousLimit) * ((double)bracket.RATE_PCT / 100.0));
                        previousLimit = bracket.UPPER_LIMIT;
                    }
                }

                // ✅ اصلاح باگ: فقط سال‌های موجود در ویرایش را حذف کن (نه همه سال‌ها)
                var yearsToUpdate = validBrackets.Select(b => (int)b.TAX_YEAR).Distinct().ToList();
                foreach (var year in yearsToUpdate)
                {
                    dbms.DoExecuteSQL(
                        "DELETE FROM PAY2_TAX_BRACKET WHERE TAX_YEAR = @Year",
                        new { Year = year });
                }

                string insertSql = @"INSERT INTO PAY2_TAX_BRACKET
                               (TAX_YEAR, UPPER_LIMIT, RATE_PCT, FIXED_TAX, SORT_ORDER)
                             VALUES
                               (@TAX_YEAR, @UPPER_LIMIT, @RATE_PCT, @FIXED_TAX, @SORT_ORDER)";
                dbms.DoExecuteSQL(insertSql, validBrackets);

                ShowToast($"پله‌های مالیاتی {yearsToUpdate.Count} سال با موفقیت ذخیره شدند.");
                ShowTaxBracketModal_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره مالیات پلکانی: " + ex.Message,
                                "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool TaxBracketYearFilter(object item)
        {
            if (_selectedTaxYearFilter == 0) return true;
            return item is TaxBracketModel m && m.TAX_YEAR == _selectedTaxYearFilter;
        }

        private void RebuildTaxYearCombo()
        {
            CmbTaxYear.Items.Clear();
            var allItem = new ComboBoxItem { Content = "همه سال‌ها", Tag = (short)0 };
            CmbTaxYear.Items.Add(allItem);

            var years = TaxBrackets.Select(b => b.TAX_YEAR).Distinct().OrderByDescending(y => y);
            foreach (var year in years)
            {
                CmbTaxYear.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year });
            }
            CmbTaxYear.SelectedIndex = 0;
        }

        private void CmbTaxYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTaxYear.SelectedItem is ComboBoxItem item)
            {
                _selectedTaxYearFilter = (short)(item.Tag ?? 0);
                TaxBracketsView?.Refresh();
                UpdateTaxBracketSummary();
            }
        }

        private void UpdateTaxBracketSummary()
        {
            if (TxtTaxBracketSummary == null || TaxBrackets == null) return;
            var visible = _selectedTaxYearFilter == 0
                ? TaxBrackets.ToList()
                : TaxBrackets.Where(b => b.TAX_YEAR == _selectedTaxYearFilter).ToList();
            TxtTaxBracketSummary.Text = $"تعداد پله‌های نمایش‌داده‌شده: {visible.Count}" +
                (_selectedTaxYearFilter > 0 ? $"  |  سال: {_selectedTaxYearFilter}" : "  |  همه سال‌ها");
        }

        private void CopyFromPrevYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedTaxYearFilter == 0)
                {
                    ShowToast("ابتدا یک سال را از فیلتر انتخاب کنید تا پله‌های آن از سال قبل کپی شوند.");
                    return;
                }

                short sourceYear = (short)(_selectedTaxYearFilter - 1);
                var sourceBrackets = TaxBrackets.Where(b => b.TAX_YEAR == sourceYear).ToList();

                if (!sourceBrackets.Any())
                {
                    ShowToast($"پله‌ای برای سال {sourceYear} یافت نشد.");
                    return;
                }

                // حذف پله‌های سال مقصد (در صورت وجود)
                var toRemove = TaxBrackets.Where(b => b.TAX_YEAR == _selectedTaxYearFilter).ToList();
                foreach (var r in toRemove) TaxBrackets.Remove(r);

                // کپی با سال جدید
                foreach (var src in sourceBrackets)
                {
                    TaxBrackets.Add(new TaxBracketModel
                    {
                        TAX_YEAR = _selectedTaxYearFilter,
                        SORT_ORDER = src.SORT_ORDER,
                        UPPER_LIMIT = src.UPPER_LIMIT,
                        RATE_PCT = src.RATE_PCT,
                        FIXED_TAX = src.FIXED_TAX
                    });
                }

                TaxBracketsView?.Refresh();
                UpdateTaxBracketSummary();
                ShowToast($"{sourceBrackets.Count} پله از سال {sourceYear} برای سال {_selectedTaxYearFilter} کپی شد.");
            }
            catch (Exception ex)
            {
                ShowToast("خطا در کپی سال: " + ex.Message);
            }
        }

        private void CloseModals_Click(object sender, RoutedEventArgs e) => CloseAllModals();

        private void CloseAllModals()
        {
            ModalNewEmp.Visibility = Visibility.Collapsed;
            ModalAdvSettings.Visibility = Visibility.Collapsed;
            ModalPayslip.Visibility = Visibility.Collapsed;
            ModalTaxBracket.Visibility = Visibility.Collapsed;
            ModalJobManage.Visibility = Visibility.Collapsed;
            ModalContractManage.Visibility = Visibility.Collapsed;
            ModalLeaveBalManage.Visibility = Visibility.Collapsed;
            ModalItemDefManage.Visibility = Visibility.Collapsed;
            ModalTemplateManage.Visibility = Visibility.Collapsed;
            ModalTemplateLineManage.Visibility = Visibility.Collapsed;
            ModalDecreeLineManage.Visibility = Visibility.Collapsed;
            ModalDecreeManage.Visibility = Visibility.Collapsed;
            ModalOverrideManage.Visibility = Visibility.Collapsed;
            ModalAdvanceExclManage.Visibility = Visibility.Collapsed;
            ModalLoanManage.Visibility = Visibility.Collapsed;
            ModalSettlementManage.Visibility = Visibility.Collapsed;
        }

        private void ShowLeaveBalModal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2EmployeeModel emp)
            {
                _selectedLeaveBalEmployee = emp;
                LeaveBalModalTitle = $"مانده مرخصی: {emp.FIRST_NAME} {emp.LAST_NAME} (کد: {emp.EMP_CODE})";
                ResetLeaveBalForm();
                LoadEmployeeLeaveBalances(emp.EMP_ID);
                ModalLeaveBalManage.Visibility = Visibility.Visible;
            }
        }

        private void LoadEmployeeLeaveBalances(int empId)
        {
            try
            {
                string sql = "SELECT * FROM PAY2_LEAVE_BAL WHERE EMP_ID = @Id ORDER BY YEAR DESC";
                var data = dbms.DoGetDataSQL<Pay2LeaveBalModel>(sql, new { Id = empId });
                EmployeeLeaveBalances = new ObservableCollection<Pay2LeaveBalModel>(data ?? Enumerable.Empty<Pay2LeaveBalModel>());
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری سوابق مرخصی: " + ex.Message);
            }
        }

        private void SaveLeaveBal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentLeaveBal.YEAR < 1300 || CurrentLeaveBal.YEAR > 1500)
                {
                    MessageBox.Show("سال وارد شده نامعتبر است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentLeaveBal.EMP_ID = _selectedLeaveBalEmployee.EMP_ID;

                if (!IsEditingLeaveBal)
                {
                    var exists = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM PAY2_LEAVE_BAL WHERE EMP_ID = @EMP_ID AND YEAR = @YEAR",
                                                        new { CurrentLeaveBal.EMP_ID, CurrentLeaveBal.YEAR }).FirstOrDefault();
                    if (exists > 0)
                    {
                        MessageBox.Show($"مانده مرخصی برای سال {CurrentLeaveBal.YEAR} قبلاً ثبت شده است. برای تغییر، آن را ویرایش کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string insertSql = @"
                        INSERT INTO PAY2_LEAVE_BAL (EMP_ID, YEAR, ENTITLEMENT_MIN, USED_MIN, CARRIED_IN_MIN, CARRIED_OUT_MIN, UPDATED_AT)
                        VALUES (@EMP_ID, @YEAR, @ENTITLEMENT_MIN, @USED_MIN, @CARRIED_IN_MIN, @CARRIED_OUT_MIN, GETDATE())";

                    dbms.DoExecuteSQL(insertSql, CurrentLeaveBal);
                    ShowToast("مانده مرخصی با موفقیت ثبت شد.");
                }
                else
                {
                    string updateSql = @"
                        UPDATE PAY2_LEAVE_BAL 
                        SET ENTITLEMENT_MIN = @ENTITLEMENT_MIN, 
                            USED_MIN = @USED_MIN, 
                            CARRIED_IN_MIN = @CARRIED_IN_MIN, 
                            CARRIED_OUT_MIN = @CARRIED_OUT_MIN,
                            UPDATED_AT = GETDATE()
                        WHERE EMP_ID = @EMP_ID AND YEAR = @YEAR";

                    dbms.DoExecuteSQL(updateSql, CurrentLeaveBal);
                    ShowToast("مانده مرخصی با موفقیت ویرایش شد.");
                }

                ResetLeaveBalForm();
                LoadEmployeeLeaveBalances(_selectedLeaveBalEmployee.EMP_ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره مانده مرخصی: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditLeaveBal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2LeaveBalModel bal)
            {
                CurrentLeaveBal = new Pay2LeaveBalModel
                {
                    EMP_ID = bal.EMP_ID,
                    YEAR = bal.YEAR,
                    ENTITLEMENT_MIN = bal.ENTITLEMENT_MIN,
                    USED_MIN = bal.USED_MIN,
                    CARRIED_IN_MIN = bal.CARRIED_IN_MIN,
                    CARRIED_OUT_MIN = bal.CARRIED_OUT_MIN
                };
                IsEditingLeaveBal = true;
            }
        }

        private void CancelEditLeaveBal_Click(object sender, RoutedEventArgs e)
        {
            ResetLeaveBalForm();
        }

        private void DeleteLeaveBal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Pay2LeaveBalModel bal)
            {
                var result = MessageBox.Show($"آیا از حذف سابقه مرخصی سال {bal.YEAR} اطمینان دارید؟", "حذف سابقه", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM PAY2_LEAVE_BAL WHERE EMP_ID = @EMP_ID AND YEAR = @YEAR", new { EMP_ID = bal.EMP_ID, YEAR = bal.YEAR });
                        ShowToast("سابقه مرخصی با موفقیت حذف شد.");
                        LoadEmployeeLeaveBalances(_selectedLeaveBalEmployee.EMP_ID);
                        if (CurrentLeaveBal.YEAR == bal.YEAR) ResetLeaveBalForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطا در حذف سابقه: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetLeaveBalForm()
        {
            var currentPersianYear = Tarikh.FullCurrentDate.Substring(0, 4);
            short yearParsed = 1403;
            short.TryParse(currentPersianYear, out yearParsed);

            CurrentLeaveBal = new Pay2LeaveBalModel { YEAR = yearParsed };
            IsEditingLeaveBal = false;
        }

        private async void ShowToast(string message)
        {
            ToastMessage = message;
            IsToastVisible = true;
            await Task.Delay(3000);
            IsToastVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void LoadWorkshops()
        {
            try
            {
                string sql = "SELECT WS_ID, WS_CODE, WS_NAME, NATIONAL_ID, SOCIAL_INS_CODE, TAX_CODE, ADDRESS, PHONE, IS_ACTIVE, INS_MODE FROM PAY2_WORKSHOP ORDER BY WS_ID";
                var data = dbms.DoGetDataSQL<WorkshopModel>(sql);
                Workshops = new ObservableCollection<WorkshopModel>(data ?? Enumerable.Empty<WorkshopModel>());

                if (CurrentWorkshop == null || CurrentWorkshop.WS_ID == 0)
                {
                    BtnNewWorkshop_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                ShowToast("خطا در واکشی لیست کارگاه‌ها: " + ex.Message);
            }
        }

        private void WorkshopsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WorkshopModel selected)
            {
                CurrentWorkshop = new WorkshopModel
                {
                    WS_ID = selected.WS_ID,
                    WS_CODE = selected.WS_CODE,
                    WS_NAME = selected.WS_NAME,
                    NATIONAL_ID = selected.NATIONAL_ID,
                    SOCIAL_INS_CODE = selected.SOCIAL_INS_CODE,
                    TAX_CODE = selected.TAX_CODE,
                    ADDRESS = selected.ADDRESS,
                    PHONE = selected.PHONE,
                    IS_ACTIVE = selected.IS_ACTIVE,
                    INS_MODE = selected.INS_MODE,
                };

                LoadWorkshopAccount(selected.WS_ID);
            }
        }

        private class AccRowMap { public string ACC_KEY { get; set; } public string ACC_CODE { get; set; } }

        private void LoadWorkshopAccount(int wsId)
        {
            try
            {
                string sql = "SELECT ACC_KEY, ACC_CODE FROM PAY2_WORKSHOP_ACC WHERE WS_ID = @Id";
                var accounts = dbms.DoGetDataSQL<AccRowMap>(sql, new { Id = wsId });

                var accModel = new WorkshopAccModel { WS_ID = wsId };
                if (accounts != null)
                {
                    foreach (var row in accounts)
                    {
                        switch (row.ACC_KEY)
                        {
                            case "ADV_HES_K": accModel.ADV_HES_K = row.ACC_CODE; break;
                            case "ADV_HES_M": accModel.ADV_HES_M = row.ACC_CODE; break;
                            case "SAL_HES_K": accModel.SAL_HES_K = row.ACC_CODE; break; // legacy key
                            case "SALARY_EXP": accModel.SALARY_EXP = row.ACC_CODE; break;
                            case "SALARY_PAYABLE": accModel.SALARY_PAYABLE = row.ACC_CODE; break;
                            case "INS_EXP": accModel.INS_EXP = row.ACC_CODE; break;
                            case "INS_PAYABLE": accModel.INS_PAYABLE = row.ACC_CODE; break;
                            case "TAX_PAYABLE": accModel.TAX_PAYABLE = row.ACC_CODE; break;
                        }
                    }
                }

                CurrentWorkshopAcc = accModel;
            }
            catch (Exception ex)
            {
                ShowToast("خطا در واکشی تنظیمات حسابداری کارگاه: " + ex.Message);
            }
        }

        private void BtnNewWorkshop_Click(object sender, RoutedEventArgs e)
        {
            CurrentWorkshop = new WorkshopModel();
            CurrentWorkshopAcc = new WorkshopAccModel();
            if (GridWorkshops != null) GridWorkshops.SelectedItem = null;
        }

        private void SaveWorkshop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentWorkshop.WS_CODE))
                {
                    MessageBox.Show("کد کارگاه نمی‌تواند خالی باشد.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(CurrentWorkshop.WS_NAME))
                {
                    MessageBox.Show("نام کارگاه نمی‌تواند خالی باشد.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int newOrUpdatedWsId = CurrentWorkshop.WS_ID;

                if (CurrentWorkshop.WS_ID == 0)
                {
                    var exists = dbms.DoGetDataSQL<int>(
                        "SELECT COUNT(1) FROM PAY2_WORKSHOP WHERE WS_CODE = @WS_CODE",
                        new { WS_CODE = CurrentWorkshop.WS_CODE }).FirstOrDefault();

                    if (exists > 0)
                    {
                        MessageBox.Show("این کد کارگاه قبلاً در سیستم ثبت شده است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string insertSql = @"
                INSERT INTO PAY2_WORKSHOP (WS_CODE, WS_NAME, NATIONAL_ID, SOCIAL_INS_CODE, TAX_CODE, ADDRESS, PHONE, IS_ACTIVE, INS_MODE, CREATED_BY)
                OUTPUT INSERTED.WS_ID
                VALUES (@WS_CODE, @WS_NAME, @NATIONAL_ID, @SOCIAL_INS_CODE, @TAX_CODE, @ADDRESS, @PHONE, @IS_ACTIVE, @INS_MODE, @User)";

                    var insertedId = dbms.DoGetDataSQL<int>(insertSql, new
                    {
                        CurrentWorkshop.WS_CODE,
                        CurrentWorkshop.WS_NAME,
                        CurrentWorkshop.NATIONAL_ID,
                        CurrentWorkshop.SOCIAL_INS_CODE,
                        CurrentWorkshop.TAX_CODE,
                        CurrentWorkshop.ADDRESS,
                        CurrentWorkshop.PHONE,
                        CurrentWorkshop.IS_ACTIVE,
                        CurrentWorkshop.INS_MODE,
                        User = Baseknow.USERCOD
                    }).FirstOrDefault();

                    if (insertedId > 0) newOrUpdatedWsId = insertedId;
                }
                else
                {
                    string updateSql = @"
                UPDATE PAY2_WORKSHOP
                SET WS_CODE          = @WS_CODE,
                    WS_NAME          = @WS_NAME,
                    NATIONAL_ID      = @NATIONAL_ID,
                    SOCIAL_INS_CODE  = @SOCIAL_INS_CODE,
                    TAX_CODE         = @TAX_CODE,
                    ADDRESS          = @ADDRESS,
                    PHONE            = @PHONE,
                    IS_ACTIVE        = @IS_ACTIVE,
                    INS_MODE         = @INS_MODE
                WHERE WS_ID = @WS_ID";

                    dbms.DoExecuteSQL(updateSql, new
                    {
                        CurrentWorkshop.WS_CODE,
                        CurrentWorkshop.WS_NAME,
                        CurrentWorkshop.NATIONAL_ID,
                        CurrentWorkshop.SOCIAL_INS_CODE,
                        CurrentWorkshop.TAX_CODE,
                        CurrentWorkshop.ADDRESS,
                        CurrentWorkshop.PHONE,
                        CurrentWorkshop.IS_ACTIVE,
                        CurrentWorkshop.INS_MODE,
                        CurrentWorkshop.WS_ID
                    });
                }

                // ذخیره همه سرفصل‌های PAY2_WORKSHOP_ACC
                string accSql = @"
            IF EXISTS (SELECT 1 FROM PAY2_WORKSHOP_ACC WHERE WS_ID = @WS_ID AND ACC_KEY = @ACC_KEY)
                UPDATE PAY2_WORKSHOP_ACC SET ACC_CODE = @ACC_CODE WHERE WS_ID = @WS_ID AND ACC_KEY = @ACC_KEY
            ELSE
                INSERT INTO PAY2_WORKSHOP_ACC (WS_ID, ACC_KEY, ACC_CODE) VALUES (@WS_ID, @ACC_KEY, @ACC_CODE)";

                var accEntries = new[]
                {
            ("ADV_HES_K",      CurrentWorkshopAcc.ADV_HES_K),
            ("ADV_HES_M",      CurrentWorkshopAcc.ADV_HES_M),
            ("SALARY_EXP",     CurrentWorkshopAcc.SALARY_EXP),
            ("SALARY_PAYABLE", CurrentWorkshopAcc.SALARY_PAYABLE),
            ("INS_EXP",        CurrentWorkshopAcc.INS_EXP),
            ("INS_PAYABLE",    CurrentWorkshopAcc.INS_PAYABLE),
            ("TAX_PAYABLE",    CurrentWorkshopAcc.TAX_PAYABLE),
        };

                foreach (var (key, code) in accEntries)
                {
                    if (!string.IsNullOrWhiteSpace(code))
                        dbms.DoExecuteSQL(accSql, new { WS_ID = newOrUpdatedWsId, ACC_KEY = key, ACC_CODE = code.Trim() });
                }

                ShowToast("اطلاعات کارگاه و تنظیمات حسابداری با موفقیت ذخیره شد.");
                Workshops = null; // force reload for LoadWorkshopsForLookup cache
                LoadWorkshops();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره اطلاعات کارگاه: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // کلیدهایی که به ComboBox تبدیل شده‌اند — مقدار از Tag آیتم انتخابی خوانده می‌شود
        private static readonly HashSet<string> _comboConfigKeys = new HashSet<string>
        {
            "MONTH_DAYS_MODE", "MID_MONTH_PRORATE", "SHIFT_MODE", "ROUND_MODE",
            "INS_CEILING_APPLY", "INS_TAB56_UNEMP",
            "TAX_DEDUCT_INS", "TAX_DEPRIVATION_APPLY",
            "ADV_ENABLED", "ADV_SCOPE", "ADV_USE_HES_T_FILTER", "ADV_MIN_POSITIVE",
            "LEAVE_ANNUAL_DAYS", "LEAVE_CARRYOVER_MAX",
            "BONUS_MODE", "SENIORITY_MODE",
            "ITEM_DEF_MIN_ROLE", "CONFIG_MIN_ROLE"
        };

        private void LoadAllPay2Settings()
        {
            try
            {
                string sql = "SELECT CFG_KEY, CFG_VALUE FROM PAY2_CONFIG";
                var settings = dbms.DoGetDataSQL<ConfigModel>(sql).ToList();

                if (settings == null) return;

                foreach (var cfg in settings)
                {
                    if (_comboConfigKeys.Contains(cfg.CFG_KEY))
                    {
                        // ComboBox — انتخاب آیتمی که Tag آن با CFG_VALUE برابر است
                        var combo = this.FindName(cfg.CFG_KEY) as ComboBox;
                        if (combo != null)
                        {
                            foreach (ComboBoxItem item in combo.Items)
                            {
                                if (item.Tag?.ToString() == cfg.CFG_VALUE)
                                {
                                    combo.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // TextBox — فیلدهای عددی و متنی آزاد
                        var textBox = this.FindName(cfg.CFG_KEY) as TextBox;
                        if (textBox != null)
                            textBox.Text = cfg.CFG_VALUE;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowToast("خطا در واکشی تنظیمات: " + ex.Message);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] keys = {
                    "MONTH_DAYS_MODE", "MID_MONTH_PRORATE", "OT_NORMAL_MULT", "OT_HOLIDAY_MULT",
                    "OT_HOUR_BASE", "SHIFT_MODE", "ROUND_MODE",

                    "INS_WORKER_RATE", "INS_EMPLOYER_RATE", "INS_UNEMP_RATE", "INS_CEILING_APPLY", "INS_CEILING_MONTHLY",
                    "INS_EXEMPT_COUNT", "INS_JANBAZ_RATE", "INS_TAB56_UNEMP",

                    "TAX_YEAR", "TAX_EXEMPT_MONTHLY", "TAX_DEDUCT_INS", "TAX_DEPRIVATION_APPLY",

                    "ADV_ENABLED", "ADV_SCOPE", "ADV_USE_HES_T_FILTER", "ADV_MIN_POSITIVE",

                    "LEAVE_ANNUAL_DAYS", "LEAVE_MINS_PER_DAY", "LEAVE_CARRYOVER_MAX",

                    "BONUS_MODE", "BONUS_CUSTOM_DAYS", "MIN_WAGE_DAILY", "MIN_WAGE_MONTHLY", "EIDI_MIN_DAYS", "EIDI_MAX_DAYS", "SENIORITY_MODE", "SENIORITY_FIXED_AMT",

                    "ITEM_DEF_MIN_ROLE", "CONFIG_MIN_ROLE"
                };

                int successCount = 0;
                string updateSql = @"
                            UPDATE PAY2_CONFIG 
                            SET CFG_VALUE = @Val, 
                                CHANGED_AT = GETDATE(), 
                                CHANGED_BY = @User,
                                CHANGE_NOTE = N'بروزرسانی از پنل مدیریت'
                            WHERE CFG_KEY = @Key";

                foreach (var key in keys)
                {
                    string value = null;

                    if (_comboConfigKeys.Contains(key))
                    {
                        // ComboBox — اول چک می‌کنیم آیا از لیست انتخاب شده
                        // اگر نه (کاربر دستی تایپ کرده) از Text استفاده می‌کنیم
                        var combo = this.FindName(key) as ComboBox;
                        if (combo != null)
                        {
                            if (combo.SelectedItem is ComboBoxItem selected)
                                value = selected.Tag?.ToString();       // انتخاب از dropdown
                            else if (!string.IsNullOrWhiteSpace(combo.Text))
                                value = combo.Text.Trim();              // ورود دستی
                        }
                    }
                    else
                    {
                        // TextBox — خواندن متن مستقیم
                        var textBox = this.FindName(key) as TextBox;
                        if (textBox != null)
                            value = textBox.Text.Trim();
                    }

                    if (value != null)
                    {
                        dbms.DoExecuteSQL(updateSql, new
                        {
                            Val = value,
                            Key = key,
                            User = Baseknow.USERCOD
                        });
                        successCount++;
                    }
                }

                ShowToast($"تنظیمات مرکزی با موفقیت بروزرسانی شد ({successCount} مورد)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره داده‌ها: " + ex.Message, "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowJobModal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = "SELECT JOB_ID, JOB_CODE, JOB_NAME, JOB_GROUP, IS_ACTIVE FROM PAY2_JOB ORDER BY JOB_ID";
                var data = dbms.DoGetDataSQL<JobModel>(sql);

                Jobs = data != null ? new ObservableCollection<JobModel>(data) : new ObservableCollection<JobModel>();

                ModalJobManage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowToast("خطا در بارگذاری لیست مشاغل: " + ex.Message);
            }
        }

        private void SaveJobs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Jobs == null) return;

                var validJobs = Jobs
                    .Where(j => !string.IsNullOrWhiteSpace(j.JOB_CODE) && !string.IsNullOrWhiteSpace(j.JOB_NAME))
                    .ToList();

                // اعتبارسنجی: JOB_CODE تکراری در لیست جاری
                var duplicateCodes = validJobs
                    .GroupBy(j => j.JOB_CODE.Trim().ToUpper())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateCodes.Any())
                {
                    MessageBox.Show(
                        "کد شغل زیر تکراری است:\n" + string.Join("، ", duplicateCodes),
                        "خطای اعتبارسنجی", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // شناسایی ردیف‌های حذف‌شده
                var existingIds = (dbms.DoGetDataSQL<JobIdDto>("SELECT JOB_ID FROM PAY2_JOB")
                                  ?? Enumerable.Empty<JobIdDto>())
                                  .Select(x => x.JOB_ID).ToList();

                var currentIds = validJobs.Where(j => j.JOB_ID > 0).Select(j => j.JOB_ID).ToList();
                var deletedIds = existingIds.Except(currentIds).ToList();

                foreach (var id in deletedIds)
                {
                    // چک FK قبل از حذف — بدون exception-based flow
                    var isUsed = (dbms.DoGetDataSQL<CountDto>(
                        "SELECT COUNT(*) AS CNT FROM PAY2_EMPLOYEE WHERE JOB_ID = @Id AND IS_ACTIVE = 1",
                        new { Id = id })?.FirstOrDefault()?.CNT ?? 0) > 0;

                    if (isUsed)
                        dbms.DoExecuteSQL("UPDATE PAY2_JOB SET IS_ACTIVE = 0 WHERE JOB_ID = @Id", new { Id = id });
                    else
                        dbms.DoExecuteSQL("DELETE FROM PAY2_JOB WHERE JOB_ID = @Id", new { Id = id });
                }

                // INSERT / UPDATE
                foreach (var job in validJobs)
                {
                    // اعتبارسنجی: JOB_CODE تکراری در دیتابیس
                    var codeExists = (dbms.DoGetDataSQL<CountDto>(
                        "SELECT COUNT(*) AS CNT FROM PAY2_JOB WHERE UPPER(JOB_CODE) = UPPER(@Code) AND JOB_ID <> @Id",
                        new { Code = job.JOB_CODE.Trim(), Id = job.JOB_ID })
                        ?.FirstOrDefault()?.CNT ?? 0) > 0;

                    if (codeExists)
                    {
                        MessageBox.Show(
                            $"کد شغل «{job.JOB_CODE}» قبلاً در دیتابیس ثبت شده است.",
                            "خطای اعتبارسنجی", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (job.JOB_ID == 0)
                    {
                        dbms.DoExecuteSQL(@"
                    INSERT INTO PAY2_JOB (JOB_CODE, JOB_NAME, JOB_GROUP, IS_ACTIVE)
                    VALUES (@JOB_CODE, @JOB_NAME, @JOB_GROUP, @IS_ACTIVE)",
                            new
                            {
                                JOB_CODE = job.JOB_CODE.Trim(),
                                JOB_NAME = job.JOB_NAME.Trim(),
                                JOB_GROUP = job.JOB_GROUP?.Trim(),
                                IS_ACTIVE = job.IS_ACTIVE ? 1 : 0
                            });
                    }
                    else
                    {
                        dbms.DoExecuteSQL(@"
                    UPDATE PAY2_JOB
                    SET JOB_CODE  = @JOB_CODE,
                        JOB_NAME  = @JOB_NAME,
                        JOB_GROUP = @JOB_GROUP,
                        IS_ACTIVE = @IS_ACTIVE
                    WHERE JOB_ID = @JOB_ID",
                            new
                            {
                                JOB_CODE = job.JOB_CODE.Trim(),
                                JOB_NAME = job.JOB_NAME.Trim(),
                                JOB_GROUP = job.JOB_GROUP?.Trim(),
                                IS_ACTIVE = job.IS_ACTIVE ? 1 : 0,
                                JOB_ID = job.JOB_ID
                            });
                    }
                }

                ShowToast("اطلاعات مشاغل با موفقیت ذخیره شد.");

                // force-reload برای ComboBox پرسنل
                _jobs = null;

                ShowJobModal_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره مشاغل: " + ex.Message,
                    "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Pay2LeaveBalModel : INotifyPropertyChanged
        {
            private int _empId;
            private short _year;
            private int _entitlementMin = 11440;
            private int _usedMin = 0;
            private int _carriedInMin = 0;
            private int _carriedOutMin = 0;

            public int EMP_ID { get => _empId; set { _empId = value; OnPropertyChanged(nameof(EMP_ID)); } }
            public short YEAR { get => _year; set { _year = value; OnPropertyChanged(nameof(YEAR)); } }
            public int ENTITLEMENT_MIN { get => _entitlementMin; set { _entitlementMin = value; OnPropertyChanged(nameof(ENTITLEMENT_MIN)); OnPropertyChanged(nameof(BALANCE_MIN)); OnPropertyChanged(nameof(BALANCE_DAYS)); } }
            public int USED_MIN { get => _usedMin; set { _usedMin = value; OnPropertyChanged(nameof(USED_MIN)); OnPropertyChanged(nameof(BALANCE_MIN)); OnPropertyChanged(nameof(BALANCE_DAYS)); } }
            public int CARRIED_IN_MIN { get => _carriedInMin; set { _carriedInMin = value; OnPropertyChanged(nameof(CARRIED_IN_MIN)); OnPropertyChanged(nameof(BALANCE_MIN)); OnPropertyChanged(nameof(BALANCE_DAYS)); } }
            public int CARRIED_OUT_MIN { get => _carriedOutMin; set { _carriedOutMin = value; OnPropertyChanged(nameof(CARRIED_OUT_MIN)); } }

            public int BALANCE_MIN => ENTITLEMENT_MIN + CARRIED_IN_MIN - USED_MIN;
            public decimal BALANCE_DAYS => BALANCE_MIN / 440m;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class ConfigModel { public string CFG_KEY { get; set; } public string CFG_VALUE { get; set; } }
        public class EmployeeModel { public string Code { get; set; } public string FullName { get; set; } public string Job { get; set; } public string AccT { get; set; } }
        public class AttendanceModel { public string Code { get; set; } public string FullName { get; set; } public string Days { get; set; } public string DaysB { get; set; } public string OtNormal { get; set; } public string LeaveDays { get; set; } }
        public class AdvanceModel { public string Code { get; set; } public string FullName { get; set; } public string RawBalanceText { get; set; } public string AdvanceText { get; set; } }
        public class PayrollModel { public string FullName { get; set; } public string Gross { get; set; } public string InsW { get; set; } public string Tax { get; set; } public string Advance { get; set; } public string Net { get; set; } }

        public class TaxBracketModel : INotifyPropertyChanged
        {
            private int _brkId;
            private short _taxYear;
            private long _upperLimit;
            private decimal _ratePct;
            private long _fixedTax;
            private short _sortOrder;

            public int BRK_ID { get => _brkId; set { _brkId = value; OnPropertyChanged(nameof(BRK_ID)); } }
            public short TAX_YEAR { get => _taxYear; set { _taxYear = value; OnPropertyChanged(nameof(TAX_YEAR)); } }
            public long UPPER_LIMIT { get => _upperLimit; set { _upperLimit = value; OnPropertyChanged(nameof(UPPER_LIMIT)); } }
            public decimal RATE_PCT { get => _ratePct; set { _ratePct = value; OnPropertyChanged(nameof(RATE_PCT)); } }
            public long FIXED_TAX { get => _fixedTax; set { _fixedTax = value; OnPropertyChanged(nameof(FIXED_TAX)); } }
            public short SORT_ORDER { get => _sortOrder; set { _sortOrder = value; OnPropertyChanged(nameof(SORT_ORDER)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class WorkshopModel : INotifyPropertyChanged
        {
            private int _wsId;
            private string _wsCode;
            private string _wsName;
            private string _nationalId;
            private string _socialInsCode;
            private string _taxCode;
            private string _address;
            private string _phone;

            private bool _isActive = true;
            private int _insMode = 1;

            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public string WS_CODE { get => _wsCode; set { _wsCode = value; OnPropertyChanged(nameof(WS_CODE)); } }
            public string WS_NAME { get => _wsName; set { _wsName = value; OnPropertyChanged(nameof(WS_NAME)); } }
            public string NATIONAL_ID { get => _nationalId; set { _nationalId = value; OnPropertyChanged(nameof(NATIONAL_ID)); } }
            public string SOCIAL_INS_CODE { get => _socialInsCode; set { _socialInsCode = value; OnPropertyChanged(nameof(SOCIAL_INS_CODE)); } }
            public string TAX_CODE { get => _taxCode; set { _taxCode = value; OnPropertyChanged(nameof(TAX_CODE)); } }
            public string ADDRESS { get => _address; set { _address = value; OnPropertyChanged(nameof(ADDRESS)); } }
            public string PHONE { get => _phone; set { _phone = value; OnPropertyChanged(nameof(PHONE)); } }

            public bool IS_ACTIVE { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IS_ACTIVE)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(StatusBgColor)); OnPropertyChanged(nameof(StatusTextColor)); } }
            public int INS_MODE { get => _insMode; set { _insMode = value; OnPropertyChanged(nameof(INS_MODE)); OnPropertyChanged(nameof(InsModeText)); } }

            // Computed display props
            public string StatusText => IS_ACTIVE ? "فعال" : "غیرفعال";
            public string StatusBgColor => IS_ACTIVE ? "#ecfdf5" : "#fef2f2";
            public string StatusTextColor => IS_ACTIVE ? "#059669" : "#ef4444";
            public string InsModeText => INS_MODE == 2 ? "تبصره ماده ۷" : "معمولی";


            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class WorkshopAccModel : INotifyPropertyChanged
        {
            private int _wsId;
            private string _advHesK;
            private string _advHesM;
            private string _salHesK;

            private string _salaryExp;
            private string _salaryPayable;
            private string _insExp;
            private string _insPayable;
            private string _taxPayable;

            public int WS_ID { get => _wsId; set { _wsId = value; OnPropertyChanged(nameof(WS_ID)); } }
            public string ADV_HES_K { get => _advHesK; set { _advHesK = value; OnPropertyChanged(nameof(ADV_HES_K)); } }
            public string ADV_HES_M { get => _advHesM; set { _advHesM = value; OnPropertyChanged(nameof(ADV_HES_M)); } }
            public string SAL_HES_K { get => _salHesK; set { _salHesK = value; OnPropertyChanged(nameof(SAL_HES_K)); } }

            public string SALARY_EXP { get => _salaryExp; set { _salaryExp = value; OnPropertyChanged(nameof(SALARY_EXP)); } }
            public string SALARY_PAYABLE { get => _salaryPayable; set { _salaryPayable = value; OnPropertyChanged(nameof(SALARY_PAYABLE)); } }
            public string INS_EXP { get => _insExp; set { _insExp = value; OnPropertyChanged(nameof(INS_EXP)); } }
            public string INS_PAYABLE { get => _insPayable; set { _insPayable = value; OnPropertyChanged(nameof(INS_PAYABLE)); } }
            public string TAX_PAYABLE { get => _taxPayable; set { _taxPayable = value; OnPropertyChanged(nameof(TAX_PAYABLE)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class JobModel : INotifyPropertyChanged
        {
            private int _jobId;
            private string _jobCode;
            private string _jobName;
            private string _jobGroup;
            private bool _isActive = true;

            public int JOB_ID { get => _jobId; set { _jobId = value; OnPropertyChanged(nameof(JOB_ID)); } }
            public string JOB_CODE { get => _jobCode; set { _jobCode = value; OnPropertyChanged(nameof(JOB_CODE)); } }
            public string JOB_NAME { get => _jobName; set { _jobName = value; OnPropertyChanged(nameof(JOB_NAME)); } }
            public string JOB_GROUP { get => _jobGroup; set { _jobGroup = value; OnPropertyChanged(nameof(JOB_GROUP)); } }
            public bool IS_ACTIVE { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IS_ACTIVE)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private class JobIdDto
        {
            public int JOB_ID { get; set; }
        }
        private class CountDto
        {
            public int CNT { get; set; }
        }
        private void BtnOpenAttendance_Click(object sender, RoutedEventArgs e)
        {
            // ساخت یک نمونه از فرم ورود کارکرد
            var attendanceWindow = new WIN_SALARY_ATTENDANCE();

            // باز کردن فرم به صورت مدال (پنجره فرزند)
            attendanceWindow.Owner = this; // تنظیم این پنجره به عنوان والد
            attendanceWindow.ShowDialog();
        }
    }
}
