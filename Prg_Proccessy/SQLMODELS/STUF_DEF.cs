using System.Globalization;

namespace Prg_Proccessy.SQLMODELS
{
    public class STUF_DEF
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string N_FANI { get; set; }
        public string TOZIH { get; set; }
        public int VAHED { get; set; }
        public string? BARCODE { get; set; }
        public double B_SEF { get; set; }
        public double N_SEF { get; set; }
        public double MIN_M { get; set; }
        public double MAX_M { get; set; }
        public double RADAH { get; set; }
        public Nullable<short> KINDK { get; set; }
        public double MABL_F { get; set; }
        public Nullable<int> DEPART { get; set; }
        public int IDD { get; set; }
        public Nullable<bool> CMBAA { get; set; }
        public Nullable<double> VAZN { get; set; }
        public Nullable<bool> OKF { get; set; }
        public Nullable<double> MENUIT { get; set; }
        public Nullable<double> MEGHTA { get; set; }
        public Nullable<double> MEGHJAY { get; set; }
        public Nullable<int> PGID { get; set; }
        public int mu { get; set; }
        public string? sstid { get; set; }
        public double? vra { get; set; }

        public long? UP_DATE { get; set; }
        public double? UP_TIME { get; set; }
        public string? UP_USER_NAME { get; set; }
        public string? PC_NAME { get; set; }
        public string? IPADD { get; set; }

        public string UpTimeDisplay
        {
            get
            {
                if (UP_TIME.HasValue)
                {
                    try
                    {
                        var dt = DateTime.FromOADate(UP_TIME.Value);
                        var persianCulture = new CultureInfo("fa-IR");
                        persianCulture.DateTimeFormat.Calendar = new GregorianCalendar();
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
    }
}
