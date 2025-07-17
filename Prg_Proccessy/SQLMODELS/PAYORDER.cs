namespace Prg_Proccessy.SQLMODELS
{
    public class PAYORDER
    {
        public int IDD { get; set; }
        public double MABLS { get; set; }
        public string BABAT { get; set; }
        public string CUST_NO { get; set; }
        public string CUST_NO_TXT { get; set; }
        public string ORDERER { get; set; }
        public long? PAYDATE { get; set; }
        public string TOZIH { get; set; }
        public int CACHTIC { get; set; }
        public double CHACHMABL { get; set; }
        public int CHEKTIC { get; set; }
        public double CHEKMABL { get; set; }
        public int CHEKFAGH { get; set; }
        public int CHEKDIST { get; set; }
        public int CHEKMTIC { get; set; }
        public double CHEKMMABL { get; set; }
        public int CHEKMFAGH { get; set; }
        public int CHEKMDIST { get; set; }
        public string USERNAME { get; set; }
        public int USERCO { get; set; }
        public int SGN1 { get; set; }
        public int SGN2 { get; set; }
        public int SGN3 { get; set; }
        public int? sgn1usid { get; set; }
        public int? sgn2usid { get; set; }
        public int? sgn3usid { get; set; }
        public DateTime? DTT { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
    }

}
