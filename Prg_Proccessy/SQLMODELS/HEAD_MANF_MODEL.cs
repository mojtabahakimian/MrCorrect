using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class HEAD_MANF_MODEL : ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }

        public int FNUMB { get; set; }
        public string CODE { get; set; }
        public string NAME_CODE { get; set; }
        public long? DATE_ACTIV { get; set; }
        public double IMBIBE_MANF { get; set; }
        public double IMBIBE_SAR { get; set; }
        public double? GHEYMAT { get; set; }
        public string NAMES { get; set; }
        public int? N_KOL { get; set; }
        public int? NUMBER { get; set; }
        public int? TNUMBER { get; set; }
        public double SA_HOUR { get; set; }
        public int SA_NHOU { get; set; }
        public string TOZIH { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
        public long ID { get; set; }
    }
}
