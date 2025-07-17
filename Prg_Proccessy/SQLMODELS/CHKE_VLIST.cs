using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CHKE_VLIST : INotifyPropertyChanged, ICloneable
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
        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }
        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }
        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged("DATE_S"); } }
        private long? _date;
        public long? DATE { get => _date; set { if (_date == value) return; _date = value; OnPropertyChanged("DATE"); } }
        private string? _shobeh;
        public string? SHOBEH { get => _shobeh; set { if (_shobeh == value) return; _shobeh = value; OnPropertyChanged("SHOBEH"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private string? _name_tah;
        public string? NAME_TAH { get => _name_tah; set { if (_name_tah == value) return; _name_tah = value; OnPropertyChanged("NAME_TAH"); } }
        private string? _n_hesab;
        public string? N_HESAB { get => _n_hesab; set { if (_n_hesab == value) return; _n_hesab = value; OnPropertyChanged("N_HESAB"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }
        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }
        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private int? _n_moin;
        public int? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }
        private int? _n_kol2;
        public int? N_KOL2 { get => _n_kol2; set { if (_n_kol2 == value) return; _n_kol2 = value; OnPropertyChanged("N_KOL2"); } }
        private int? _n_moin2;
        public int? N_MOIN2 { get => _n_moin2; set { if (_n_moin2 == value) return; _n_moin2 = value; OnPropertyChanged("N_MOIN2"); } }
        private int? _n_kol3;
        public int? N_KOL3 { get => _n_kol3; set { if (_n_kol3 == value) return; _n_kol3 = value; OnPropertyChanged("N_KOL3"); } }
        private int? _n_moin3;
        public int? N_MOIN3 { get => _n_moin3; set { if (_n_moin3 == value) return; _n_moin3 = value; OnPropertyChanged("N_MOIN3"); } }
        private int? _n_taf;
        public int? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }
        private int? _n_taf2;
        public int? N_TAF2 { get => _n_taf2; set { if (_n_taf2 == value) return; _n_taf2 = value; OnPropertyChanged("N_TAF2"); } }
        private int? _n_taf3;
        public int? N_TAF3 { get => _n_taf3; set { if (_n_taf3 == value) return; _n_taf3 = value; OnPropertyChanged("N_TAF3"); } }
        private string? _bkk;
        public string? BKK { get => _bkk; set { if (_bkk == value) return; _bkk = value; OnPropertyChanged("BKK"); } }
        private int? _vaz;
        public int? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged("VAZ"); } }
        private int? _kind;
        public int? KIND { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("KIND"); } }

    }
}
