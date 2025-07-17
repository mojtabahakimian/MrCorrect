namespace Prg_Proccessy.SQLMODELS
{
    public class MESAGEP
    {
        public int? IDNUM { get; set; }
        public int? PERSONEL { get; set; }
        public string? PAYAM { get; set; }
        public int? STATUS { get; set; }
        public int? STDATE { get; set; }
        public int? STTIME { get; set; }
        public string? USERNAME { get; set; }
        public string? COMP_COD { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
        public bool? IsNotifyCalled { get; set; } = false;

        public string? NAME { get; set; }
        public string? hes { get; set; }
    }
}
