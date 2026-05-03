using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Data.SqlClient;
using Prg_SendInvoice.CNNMANAGER;

namespace Prg_UI.Wins.WinMenus.SALARY
{
    public partial class WIN_SALARY_ATTENDANCE : Window
    {
        private readonly CL_CCNNMANAGER _dbms = new CL_CCNNMANAGER();
        private int _currentPeriodId = 0;

        public WIN_SALARY_ATTENDANCE()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWorkshops();
            // Default period suggestion (e.g., current Shamsi month start)
            TxtPeriodDate.Text = "14030100";
        }

        private void LoadWorkshops()
        {
            try
            {
                string sql = "SELECT WS_ID, WS_NAME FROM PAY2_WORKSHOP WHERE IS_ACTIVE = 1 ORDER BY WS_NAME";
                var workshops = _dbms.DoGetDataSQL<WorkshopDto>(sql).ToList();
                CmbWorkshops.ItemsSource = workshops;
                if (workshops.Any()) CmbWorkshops.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری کارگاه‌ها:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (CmbWorkshops.SelectedValue == null)
            {
                MessageBox.Show("لطفاً یک کارگاه انتخاب کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtPeriodDate.Text) || TxtPeriodDate.Text.Length != 8 || !long.TryParse(TxtPeriodDate.Text, out long periodDate))
            {
                MessageBox.Show("تاریخ دوره نامعتبر است. فرمت صحیح: 14030700", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int wsId = (int)CmbWorkshops.SelectedValue;
            LoadOrInitializePeriod(wsId, periodDate);
        }

        private void LoadOrInitializePeriod(int wsId, long periodDate)
        {
            try
            {
                // 1. Get or Create Period
                string periodSql = @"
                    IF NOT EXISTS (SELECT 1 FROM PAY2_PERIOD WHERE WS_ID = @WsId AND PERIOD_DATE = @PeriodDate)
                    BEGIN
                        INSERT INTO PAY2_PERIOD (WS_ID, PERIOD_DATE, HOLIDAY_DAYS, TENDAR_APPLY, STATUS, OPENED_AT)
                        VALUES (@WsId, @PeriodDate, 0, 0, 1, GETDATE());
                    END
                    
                    SELECT PER_ID, HOLIDAY_DAYS, TENDAR_APPLY, STATUS FROM PAY2_PERIOD 
                    WHERE WS_ID = @WsId AND PERIOD_DATE = @PeriodDate;";

                var periodInfo = _dbms.DoGetDataSQL<PeriodInfoDto>(periodSql, new { WsId = wsId, PeriodDate = periodDate }).FirstOrDefault();

                if (periodInfo == null)
                {
                    MessageBox.Show("خطا در ایجاد یا دریافت اطلاعات دوره ماهیانه.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentPeriodId = periodInfo.PER_ID;
                TxtHolidayDays.Text = periodInfo.HOLIDAY_DAYS.ToString();
                ChkTendarApply.IsChecked = periodInfo.TENDAR_APPLY;

                if (periodInfo.STATUS != 1)
                {
                    TxtStatus.Text = "دوره بسته شده یا محاسبه شده است. تغییرات اعمال نخواهد شد.";
                    BtnSave.IsEnabled = false;
                    DgAttendance.IsReadOnly = true;
                }
                else
                {
                    TxtStatus.Text = "دوره باز است. امکان ویرایش وجود دارد.";
                    BtnSave.IsEnabled = true;
                    DgAttendance.IsReadOnly = false;
                }

                // 2. Load Attendance Data joined with Employees
                string attSql = @"
                    SELECT 
                        e.EMP_ID,
                        e.EMP_CODE,
                        e.LAST_NAME + ' ' + e.FIRST_NAME AS FULL_NAME,
                        ISNULL(a.WORK_DAYS, 0) AS WORK_DAYS,
                        ISNULL(a.DAYS_TOLID, 0) AS DAYS_TOLID,
                        ISNULL(a.DAYS_EDARI, 0) AS DAYS_EDARI,
                        ISNULL(a.DAYS_KHADAMAT, 0) AS DAYS_KHADAMAT,
                        ISNULL(a.DAYS_FOROSH, 0) AS DAYS_FOROSH,
                        ISNULL(a.OT_NORMAL_H, 0) AS OT_NORMAL_H,
                        ISNULL(a.OT_HOLIDAY_H, 0) AS OT_HOLIDAY_H,
                        ISNULL(a.OT_ADMIN_H, 0) AS OT_ADMIN_H,
                        ISNULL(a.LEAVE_DAYS, 0) AS LEAVE_DAYS,
                        ISNULL(a.ABSENT_DAYS, 0) AS ABSENT_DAYS,
                        ISNULL(a.MISSION_DAYS, 0) AS MISSION_DAYS,
                        ISNULL(a.DAYS, 0) AS DAYS,
                        ISNULL(a.DAYSB, 0) AS DAYSB,
                        ISNULL(a.FRID_COUNT, 0) AS FRID_COUNT,
                        ISNULL(a.TDAYS, 0) AS TDAYS,
                        ISNULL(a.PERF_AMOUNT, 0) AS PERF_AMOUNT,
                        ISNULL(a.TRANSP_AMOUNT, 0) AS TRANSP_AMOUNT,
                        ISNULL(a.KASR_OTHER, 0) AS KASR_OTHER,
                        ISNULL(a.LOCKED, 0) AS LOCKED
                    FROM PAY2_EMPLOYEE e
                    LEFT JOIN PAY2_ATTENDANCE a ON e.EMP_ID = a.EMP_ID AND a.PER_ID = @PerId
                    WHERE e.WS_ID = @WsId AND e.IS_ACTIVE = 1
                    ORDER BY e.LAST_NAME, e.FIRST_NAME;";

                var attendanceList = _dbms.DoGetDataSQL<AttendanceEntryDto>(attSql, new { PerId = _currentPeriodId, WsId = wsId }).ToList();
                DgAttendance.ItemsSource = attendanceList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری کارکرد:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPeriodId == 0 || DgAttendance.ItemsSource == null)
            {
                MessageBox.Show("هیچ داده‌ای برای ذخیره وجود ندارد. ابتدا لیست را بارگذاری کنید.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!byte.TryParse(TxtHolidayDays.Text, out byte holidayDays))
            {
                MessageBox.Show("تعداد روزهای تعطیل نامعتبر است.", "اخطار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var items = DgAttendance.ItemsSource as List<AttendanceEntryDto>;
            if (items == null || !items.Any()) return;

            try
            {
                // Update Period Header
                string updatePeriodSql = "UPDATE PAY2_PERIOD SET HOLIDAY_DAYS = @HolidayDays, TENDAR_APPLY = @TendarApply WHERE PER_ID = @PerId;";
                _dbms.DoExecuteSQL(updatePeriodSql, new { HolidayDays = holidayDays, TendarApply = ChkTendarApply.IsChecked ?? false, PerId = _currentPeriodId });

                // Update/Insert Attendance lines using MERGE statement
                string mergeSql = @"
                    MERGE PAY2_ATTENDANCE AS target
                    USING (SELECT @PER_ID AS PER_ID, @EMP_ID AS EMP_ID) AS source
                    ON target.PER_ID = source.PER_ID AND target.EMP_ID = source.EMP_ID
                    WHEN MATCHED THEN
                        UPDATE SET 
                            WORK_DAYS = @WORK_DAYS, DAYS_TOLID = @DAYS_TOLID, DAYS_EDARI = @DAYS_EDARI, 
                            DAYS_KHADAMAT = @DAYS_KHADAMAT, DAYS_FOROSH = @DAYS_FOROSH,
                            OT_NORMAL_H = @OT_NORMAL_H, OT_HOLIDAY_H = @OT_HOLIDAY_H, OT_ADMIN_H = @OT_ADMIN_H,
                            LEAVE_DAYS = @LEAVE_DAYS, ABSENT_DAYS = @ABSENT_DAYS, MISSION_DAYS = @MISSION_DAYS,
                            DAYS = @DAYS, DAYSB = @DAYSB, FRID_COUNT = @FRID_COUNT, TDAYS = @TDAYS,
                            PERF_AMOUNT = @PERF_AMOUNT, TRANSP_AMOUNT = @TRANSP_AMOUNT, KASR_OTHER = @KASR_OTHER,
                            SOURCE = 1, CREATED_AT = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (PER_ID, EMP_ID, WORK_DAYS, DAYS_TOLID, DAYS_EDARI, DAYS_KHADAMAT, DAYS_FOROSH,
                                OT_NORMAL_H, OT_HOLIDAY_H, OT_ADMIN_H, LEAVE_DAYS, ABSENT_DAYS, MISSION_DAYS,
                                DAYS, DAYSB, FRID_COUNT, TDAYS, PERF_AMOUNT, TRANSP_AMOUNT, KASR_OTHER, SOURCE)
                        VALUES (@PER_ID, @EMP_ID, @WORK_DAYS, @DAYS_TOLID, @DAYS_EDARI, @DAYS_KHADAMAT, @DAYS_FOROSH,
                                @OT_NORMAL_H, @OT_HOLIDAY_H, @OT_ADMIN_H, @LEAVE_DAYS, @ABSENT_DAYS, @MISSION_DAYS,
                                @DAYS, @DAYSB, @FRID_COUNT, @TDAYS, @PERF_AMOUNT, @TRANSP_AMOUNT, @KASR_OTHER, 1);";

                int successCount = 0;
                foreach (var item in items)
                {
                    var p = new
                    {
                        PER_ID = _currentPeriodId,
                        EMP_ID = item.EMP_ID,
                        WORK_DAYS = item.WORK_DAYS,
                        DAYS_TOLID = item.DAYS_TOLID,
                        DAYS_EDARI = item.DAYS_EDARI,
                        DAYS_KHADAMAT = item.DAYS_KHADAMAT,
                        DAYS_FOROSH = item.DAYS_FOROSH,
                        OT_NORMAL_H = item.OT_NORMAL_H,
                        OT_HOLIDAY_H = item.OT_HOLIDAY_H,
                        OT_ADMIN_H = item.OT_ADMIN_H,
                        LEAVE_DAYS = item.LEAVE_DAYS,
                        ABSENT_DAYS = item.ABSENT_DAYS,
                        MISSION_DAYS = item.MISSION_DAYS,
                        DAYS = item.DAYS,
                        DAYSB = item.DAYSB,
                        FRID_COUNT = item.FRID_COUNT,
                        TDAYS = item.TDAYS,
                        PERF_AMOUNT = item.PERF_AMOUNT,
                        TRANSP_AMOUNT = item.TRANSP_AMOUNT,
                        KASR_OTHER = item.KASR_OTHER
                    };

                    _dbms.DoExecuteSQL(mergeSql, p);
                    successCount++;
                }

                MessageBox.Show($"اطلاعات کارکرد با موفقیت ذخیره شد. ({successCount} رکورد)", "عملیات موفق", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException sqlEx)
            {
                string msg = sqlEx.Message;
                if (msg.Contains("CK_ATT_DAYSB") || msg.Contains("CK_ATT_DAYS"))
                    msg = "خطای محدودیت منطقی: کارکرد اسمی یا رسمی (DAYS / DAYSB) نمی‌تواند بزرگتر از 'روز کارکرد کل' باشد.";

                MessageBox.Show($"خطا در ذخیره اطلاعات پایگاه داده:\n{msg}", "خطای دیتابیس", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطای نامشخص در ذخیره‌سازی:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Data Transfer Objects (DTOs) ---
        public class WorkshopDto
        {
            public int WS_ID { get; set; }
            public string WS_NAME { get; set; }
        }

        public class PeriodInfoDto
        {
            public int PER_ID { get; set; }
            public byte HOLIDAY_DAYS { get; set; }
            public bool TENDAR_APPLY { get; set; }
            public byte STATUS { get; set; }
        }

        public class AttendanceEntryDto : INotifyPropertyChanged
        {
            private decimal _workDays;
            private decimal _daysTolid;
            private decimal _daysEdari;
            private decimal _otNormalH;
            private decimal _otHolidayH;
            private decimal _days;
            private decimal _daysB;
            private decimal _leaveDays;
            private decimal _absentDays;
            private byte _fridCount;
            private long _perfAmount;
            private long _kasrOther;

            public int EMP_ID { get; set; }
            public string EMP_CODE { get; set; }
            public string FULL_NAME { get; set; }
            public bool LOCKED { get; set; }

            // Other fields initialized to 0 implicitly
            public decimal DAYS_KHADAMAT { get; set; }
            public decimal DAYS_FOROSH { get; set; }
            public decimal OT_ADMIN_H { get; set; }
            public decimal MISSION_DAYS { get; set; }
            public decimal TDAYS { get; set; }
            public long TRANSP_AMOUNT { get; set; }

            public decimal WORK_DAYS { get => _workDays; set { _workDays = value; OnPropertyChanged(nameof(WORK_DAYS)); } }
            public decimal DAYS_TOLID { get => _daysTolid; set { _daysTolid = value; OnPropertyChanged(nameof(DAYS_TOLID)); } }
            public decimal DAYS_EDARI { get => _daysEdari; set { _daysEdari = value; OnPropertyChanged(nameof(DAYS_EDARI)); } }
            public decimal OT_NORMAL_H { get => _otNormalH; set { _otNormalH = value; OnPropertyChanged(nameof(OT_NORMAL_H)); } }
            public decimal OT_HOLIDAY_H { get => _otHolidayH; set { _otHolidayH = value; OnPropertyChanged(nameof(OT_HOLIDAY_H)); } }
            public decimal DAYS { get => _days; set { _days = value; OnPropertyChanged(nameof(DAYS)); } }
            public decimal DAYSB { get => _daysB; set { _daysB = value; OnPropertyChanged(nameof(DAYSB)); } }
            public decimal LEAVE_DAYS { get => _leaveDays; set { _leaveDays = value; OnPropertyChanged(nameof(LEAVE_DAYS)); } }
            public decimal ABSENT_DAYS { get => _absentDays; set { _absentDays = value; OnPropertyChanged(nameof(ABSENT_DAYS)); } }
            public byte FRID_COUNT { get => _fridCount; set { _fridCount = value; OnPropertyChanged(nameof(FRID_COUNT)); } }
            public long PERF_AMOUNT { get => _perfAmount; set { _perfAmount = value; OnPropertyChanged(nameof(PERF_AMOUNT)); } }
            public long KASR_OTHER { get => _kasrOther; set { _kasrOther = value; OnPropertyChanged(nameof(KASR_OTHER)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}