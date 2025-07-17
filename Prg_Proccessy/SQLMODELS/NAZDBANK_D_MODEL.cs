using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// لیست چک های نزد و بانک و صندوق دریافتی
    /// </summary>
    public class NAZDBANK_D_MODEL : INotifyPropertyChanged
    {
        private double? _N_SERI;
        private int? _BANK;
        private long? _DATE_S;
        private long? _DATE;
        private string? _SHOBEH;
        private double? _MABL;
        private string? _NAME_TAH;
        private string? _N_HESAB;
        private double? _N_S;
        private string? _NAMES;
        private double? _RADIF;
        private int? _N_KOL;
        private int? _N_MOIN;
        private int? _N_KOL2;
        private int? _N_MOIN2;
        private int? _N_KOL3;
        private int? _N_MOIN3;
        private int? _BKK;
        private int? _N_TAF;
        private int? _N_TAF2;
        private int? _N_TAF3;
        private int? _VAZ;
        private string? _Expr1;
        private int? _SANDUGH;
        private int? _CUST_KIND;
        private string? _USER_NAME;
        private int? _DEPATMAN;
        private int? _SHIFT;
        private int? _KIND;
        private string? _HES1;
        private string? _HES2;
        private string? _HES3;
        private string? _ESTELAM;
        private DateTime? _CRT;
        private int? _UID;
        private string? _SAYADI;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) // Method to raise PropertyChanged event
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        public double? N_SERI
        {
            get { return _N_SERI; }
            set { _N_SERI = value; OnPropertyChanged(nameof(N_SERI)); }
        }

        public int? BANK
        {
            get { return _BANK; }
            set { _BANK = value; OnPropertyChanged(nameof(BANK)); }
        }

        public long? DATE_S
        {
            get { return _DATE_S; }
            set { _DATE_S = value; OnPropertyChanged(nameof(DATE_S)); }
        }

        public long? DATE
        {
            get { return _DATE; }
            set { _DATE = value; OnPropertyChanged(nameof(DATE)); }
        }

        public string? SHOBEH
        {
            get { return _SHOBEH; }
            set { _SHOBEH = value; OnPropertyChanged(nameof(SHOBEH)); }
        }

        public double? MABL
        {
            get { return _MABL; }
            set { _MABL = value; OnPropertyChanged(nameof(MABL)); }
        }

        public string? NAME_TAH
        {
            get { return _NAME_TAH; }
            set { _NAME_TAH = value; OnPropertyChanged(nameof(NAME_TAH)); }
        }

        public string? N_HESAB
        {
            get { return _N_HESAB; }
            set { _N_HESAB = value; OnPropertyChanged(nameof(N_HESAB)); }
        }

        public double? N_S
        {
            get { return _N_S; }
            set { _N_S = value; OnPropertyChanged(nameof(N_S)); }
        }

        public string? NAMES
        {
            get { return _NAMES; }
            set { _NAMES = value; OnPropertyChanged(nameof(NAMES)); }
        }

        public double? RADIF
        {
            get { return _RADIF; }
            set { _RADIF = value; OnPropertyChanged(nameof(RADIF)); }
        }

        public int? N_KOL
        {
            get { return _N_KOL; }
            set { _N_KOL = value; OnPropertyChanged(nameof(N_KOL)); }
        }

        public int? N_MOIN
        {
            get { return _N_MOIN; }
            set { _N_MOIN = value; OnPropertyChanged(nameof(N_MOIN)); }
        }

        public int? N_KOL2
        {
            get { return _N_KOL2; }
            set { _N_KOL2 = value; OnPropertyChanged(nameof(N_KOL2)); }
        }

        public int? N_MOIN2
        {
            get { return _N_MOIN2; }
            set { _N_MOIN2 = value; OnPropertyChanged(nameof(N_MOIN2)); }
        }

        public int? N_KOL3
        {
            get { return _N_KOL3; }
            set { _N_KOL3 = value; OnPropertyChanged(nameof(N_KOL3)); }
        }

        public int? N_MOIN3
        {
            get { return _N_MOIN3; }
            set { _N_MOIN3 = value; OnPropertyChanged(nameof(N_MOIN3)); }
        }

        public int? BKK
        {
            get { return _BKK; }
            set { _BKK = value; OnPropertyChanged(nameof(BKK)); }
        }

        public int? N_TAF
        {
            get { return _N_TAF; }
            set { _N_TAF = value; OnPropertyChanged(nameof(N_TAF)); }
        }

        public int? N_TAF2
        {
            get { return _N_TAF2; }
            set { _N_TAF2 = value; OnPropertyChanged(nameof(N_TAF2)); }
        }

        public int? N_TAF3
        {
            get { return _N_TAF3; }
            set { _N_TAF3 = value; OnPropertyChanged(nameof(N_TAF3)); }
        }

        public int? VAZ
        {
            get { return _VAZ; }
            set { _VAZ = value; OnPropertyChanged(nameof(VAZ)); }
        }

        public string? Expr1
        {
            get { return _Expr1; }
            set { _Expr1 = value; OnPropertyChanged(nameof(Expr1)); }
        }

        public int? SANDUGH
        {
            get { return _SANDUGH; }
            set { _SANDUGH = value; OnPropertyChanged(nameof(SANDUGH)); }
        }

        public int? CUST_KIND
        {
            get { return _CUST_KIND; }
            set { _CUST_KIND = value; OnPropertyChanged(nameof(CUST_KIND)); }
        }

        public string? USER_NAME
        {
            get { return _USER_NAME; }
            set { _USER_NAME = value; OnPropertyChanged(nameof(USER_NAME)); }
        }

        public int? DEPATMAN
        {
            get { return _DEPATMAN; }
            set { _DEPATMAN = value; OnPropertyChanged(nameof(DEPATMAN)); }
        }

        public int? SHIFT
        {
            get { return _SHIFT; }
            set { _SHIFT = value; OnPropertyChanged(nameof(SHIFT)); }
        }

        public int? KIND
        {
            get { return _KIND; }
            set { _KIND = value; OnPropertyChanged(nameof(KIND)); }
        }

        public string? HES1
        {
            get { return _HES1; }
            set { _HES1 = value; OnPropertyChanged(nameof(HES1)); }
        }

        public string? HES2
        {
            get { return _HES2; }
            set { _HES2 = value; OnPropertyChanged(nameof(HES2)); }
        }

        public string? HES3
        {
            get { return _HES3; }
            set { _HES3 = value; OnPropertyChanged(nameof(HES3)); }
        }

        public string? ESTELAM
        {
            get { return _ESTELAM; }
            set { _ESTELAM = value; OnPropertyChanged(nameof(ESTELAM)); }
        }

        public DateTime? CRT
        {
            get { return _CRT; }
            set { _CRT = value; OnPropertyChanged(nameof(CRT)); }
        }

        public int? UID
        {
            get { return _UID; }
            set { _UID = value; OnPropertyChanged(nameof(UID)); }
        }

        public string? SAYADI
        {
            get { return _SAYADI; }
            set { _SAYADI = value; OnPropertyChanged(nameof(SAYADI)); }
        }
    }
}
