namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// Master Main Top 1
    /// </summary>
    public class Q_BEDEHBESTANH_MAIN
    {
        public string? TAFZIL { get; set; }
        public int? HES_K { get; set; }
        public int? HES_M { get; set; }
        public double? SumOfBED { get; set; }
        public double? SumOfBES { get; set; }
        public double? BEDBES { get; set; }
        public string? NAME { get; set; }
        public string? MOIN { get; set; }
        public double? BEDM { get; set; }
        public double? BESM { get; set; }
        public int? HES_T { get; set; }
        public string? ADDRESS { get; set; }
        public string? TEL { get; set; }
        public string? CODE_E { get; set; }
        public string? TOZIH { get; set; }
        public string? HES { get; set; }
        public string? ECODE { get; set; }
        public int? CUST_COD { get; set; }
        public string? ROUTE_NAME { get; set; }
        public string? VCOD { get; set; }
        public string? VNAME { get; set; }
        public int? HES_T2 { get; set; }
        public int? HES_T3 { get; set; }
        public string? tafname { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? HES_T4 { get; set; }
        public double? charjm { get; set; }
        public double? mah { get; set; }
        public string? MOBILE { get; set; }
        public int? OSTANID { get; set; }
        public int? SHAHRID { get; set; }
        public long? LAST_DEED_DATE { get; set; }
        public int? BALANCE_ORIGIN_NS { get; set; }
        public int? BALANCE_ORIGIN_DTL_ID { get; set; }
        public int? DAYS_FROM_BALANCE_ORIGIN
        {
            get
            {
                if (!LAST_DEED_DATE.HasValue || LAST_DEED_DATE.Value <= 0) return null;
                try
                {
                    long d = LAST_DEED_DATE.Value;
                    int year = (int)(d / 10000);
                    int month = (int)((d % 10000) / 100);
                    int day = (int)(d % 100);
                    var pc = new System.Globalization.PersianCalendar();
                    var originDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    return (int)(System.DateTime.Today - originDate).TotalDays;
                }
                catch { return null; }
            }
        }
    }
}
