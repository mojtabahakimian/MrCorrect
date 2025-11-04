namespace Prg_Proccessy.SQLMODELS
{
    public class GENERAL_OPTIONS
    {
        public string? OptionName { get; set; }     //-- نام منحصر به فرد تنظیم (مثلا: ShiryBasketCode)
        public string? OptionValue { get; set; }  //-- مقدار تنظیم
        public string? Description { get; set; }  //-- توضیحات (اختیاری، برای راهنمایی)
        public DateTime LastUpdated { get; set; } //-- تاریخ آخرین بروزرسانی
        public DateTime? CRT { get; set; }          //-- تاریخ ایجاد (Create Time)
        public int? UID { get; set; }

    }
}