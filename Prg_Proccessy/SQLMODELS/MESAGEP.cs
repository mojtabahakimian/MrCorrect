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

        private int? _snooze_count;
        public int? SNOOZE_COUNT
        {
            get => _snooze_count;
            set
            {
                if (_snooze_count == value) return;
                _snooze_count = value;
            }
        }

        private DateTime? _last_notify_time;
        public DateTime? LAST_NOTIFY_TIME
        {
            get => _last_notify_time;
            set
            {
                if (_last_notify_time == value) return;
                _last_notify_time = value;
            }
        }

        public string? NAME { get; set; }
        public string? hes { get; set; }
    }
}
