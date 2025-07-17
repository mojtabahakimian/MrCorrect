using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class STUF_FSK : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public STUF_FSK()
        {
            MANDAH_A = 0;
            FI_A = 0;
            MABL_A = 0;
            MOGODI_A = 0;
            MIN_M = 0;
        }
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private int? _anbar;
        public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }

        private double? _mogodi_a;
        public double? MOGODI_A { get => _mogodi_a; set { if (_mogodi_a == value) return; _mogodi_a = value; OnPropertyChanged("MOGODI_A"); } }

        private double? _fi_a;
        public double? FI_A { get => _fi_a; set { if (_fi_a == value) return; _fi_a = value; OnPropertyChanged("FI_A"); } }

        private double? _mabl_a;
        public double? MABL_A { get => _mabl_a; set { if (_mabl_a == value) return; _mabl_a = value; OnPropertyChanged("MABL_A"); } }

        private double? _mandah_a;
        public double? MANDAH_A { get => _mandah_a; set { if (_mandah_a == value) return; _mandah_a = value; OnPropertyChanged("MANDAH_A"); } }

        private double? _vaz;
        public double? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged("VAZ"); } }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }

        private string? _position;
        public string? POSITION { get => _position; set { if (_position == value) return; _position = value; OnPropertyChanged("POSITION"); } }

        private double? _b_sef;
        public double? B_SEF { get => _b_sef; set { if (_b_sef == value) return; _b_sef = value; OnPropertyChanged("B_SEF"); } }

        private double? _n_sef;
        public double? N_SEF { get => _n_sef; set { if (_n_sef == value) return; _n_sef = value; OnPropertyChanged("N_SEF"); } }

        private double? _min_m;
        public double? MIN_M { get => _min_m; set { if (_min_m == value) return; _min_m = value; OnPropertyChanged("MIN_M"); } }

        private double? _max_m;
        public double? MAX_M { get => _max_m; set { if (_max_m == value) return; _max_m = value; OnPropertyChanged("MAX_M"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
