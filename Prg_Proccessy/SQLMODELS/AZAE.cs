namespace Prg_Proccessy.SQLMODELS
{
    public class AZAE
    {
        // [HES] is NOT NULL in the database, so it's not nullable in the model.
        // However, since it's a char type, consider trimming it when using.
        public string? HES { get; set; }
        public long? TOPETEB { get; set; }
        public bool TOPASN { get; set; } // [bit] NOT NULL, so it's not nullable.
        public bool ENDIS { get; set; } // [bit] NOT NULL, so it's not nullable.
        public long TOPMEGH { get; set; } // [bigint] NOT NULL, so it's not nullable.
        public Single JAMZARIB { get; set; }
        public Single EMTIAZ { get; set; } //float
        public int? gradeid { get; set; }
        public DateTime? DTTIM { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
        public long ID { get; set; }
    }
}
