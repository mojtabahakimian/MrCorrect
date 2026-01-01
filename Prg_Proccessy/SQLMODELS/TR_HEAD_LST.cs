using System.Globalization;

namespace Prg_Proccessy.SQLMODELS
{
    public class TR_HEAD_LST
    {
        public double? NUMBER1 { get; set; }
        public double? NUMBER { get; set; }
        public double? TAG { get; set; }
        public long? DATE_N { get; set; }
        public double? MAS { get; set; }
        public double? N_S { get; set; }
        public string? CUST_NO { get; set; }
        public string? TAH { get; set; }
        public string? NAME { get; set; }
        public string? MOLAH { get; set; }
        public double? M_NAGHD { get; set; }
        public double? MABL_VAR { get; set; }
        public string? MOIN_VAR { get; set; }
        public double? MABL_HAV { get; set; }
        public string? MOIN_HAV { get; set; }
        public double? MABL_HAZ { get; set; }
        public string? MOIN_HAZ { get; set; }
        public double? TAKHFIF { get; set; }
        public string? MOIN_KHF { get; set; }
        public int? DEPATMAN { get; set; }
        public int? SHIFT { get; set; }
        public int? CUST_KIND { get; set; }
        public string? USER_NAME { get; set; }
        public string? SHARAYET { get; set; }
        public double? MBAA { get; set; }
        public string? HMBAA { get; set; }
        public double? TAMIR { get; set; }
        public bool? TICMBAA { get; set; }
        public bool? TKHF { get; set; }
        public bool? OKF { get; set; }
        public bool? JAY { get; set; }
        public int? MODAT_PPID { get; set; }
        public int? PEPID { get; set; }
        public int? PEID { get; set; }
        public bool? SGN1 { get; set; }
        public bool? SGN2 { get; set; }
        public bool? SGN3 { get; set; }
        public int? sgn1usid { get; set; }
        public int? sgn2usid { get; set; }
        public int? sgn3usid { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
        public string? DEPNAME { get; set; }
        public string? SHNAME { get; set; }
        public string? CUSTKNAME { get; set; }
        public string? PEPNAME { get; set; }
        public string? PENAME { get; set; }
        public string? PPAME { get; set; }

        public long? UP_DATE { get; set; }

        public double? UP_TIME { get; set; } //string
        // پراپرتی نمایشی (رشته فرمت شده)
        public string UpTimeDisplay
        {
            get
            {
                if (UP_TIME.HasValue)
                {
                    try
                    {
                        var dt = DateTime.FromOADate(UP_TIME.Value);

                        // ایجاد کالچر فارسی
                        var persianCulture = new CultureInfo("fa-IR");

                        // اجبار به استفاده از تقویم میلادی (برای اینکه سال 2025 باشد نه 1404)
                        persianCulture.DateTimeFormat.Calendar = new GregorianCalendar();

                        // فرمت‌دهی: روز/ماه/سال ساعت:دقیقه:ثانیه ب.ظ
                        return dt.ToString("yyyy/MM/dd hh:mm:ss tt", persianCulture);
                    }
                    catch
                    {
                        return "";
                    }
                }
                return "";
            }
        }
        public string? UP_USER_NAME { get; set; }
        public string? PC_NAME { get; set; }
        public string? IPADD { get; set; }

        public int? ANBAR { get; set; }
        public int? ANBARF { get; set; }
    }
}
