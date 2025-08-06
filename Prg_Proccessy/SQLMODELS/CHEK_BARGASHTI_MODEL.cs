using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// مدل نمایش چک های برگشتی
    /// </summary>
    public class CHEK_BARGASHTI_MODEL : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged(nameof(N_SERI)); } }

        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged(nameof(BANK)); } }

        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged(nameof(DATE_S)); } }

        private long? _date;
        public long? DATE { get => _date; set { if (_date == value) return; _date = value; OnPropertyChanged(nameof(DATE)); } }

        private string? _shobeh;
        public string? SHOBEH { get => _shobeh; set { if (_shobeh == value) return; _shobeh = value; OnPropertyChanged(nameof(SHOBEH)); } }

        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged(nameof(MABL)); } }

        private string? _name_tah;
        public string? NAME_TAH { get => _name_tah; set { if (_name_tah == value) return; _name_tah = value; OnPropertyChanged(nameof(NAME_TAH)); } }

        private string? _n_hesab;
        public string? N_HESAB { get => _n_hesab; set { if (_n_hesab == value) return; _n_hesab = value; OnPropertyChanged(nameof(N_HESAB)); } }

        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged(nameof(N_S)); } }

        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged(nameof(N_KOL)); } }

        private int? _n_moin;
        public int? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged(nameof(N_MOIN)); } }

        private int? _n_kol2;
        public int? N_KOL2 { get => _n_kol2; set { if (_n_kol2 == value) return; _n_kol2 = value; OnPropertyChanged(nameof(N_KOL2)); } }

        private int? _n_moin2;
        public int? N_MOIN2 { get => _n_moin2; set { if (_n_moin2 == value) return; _n_moin2 = value; OnPropertyChanged(nameof(N_MOIN2)); } }

        private int? _n_kol3;
        public int? N_KOL3 { get => _n_kol3; set { if (_n_kol3 == value) return; _n_kol3 = value; OnPropertyChanged(nameof(N_KOL3)); } }

        private int? _n_moin3;
        public int? N_MOIN3 { get => _n_moin3; set { if (_n_moin3 == value) return; _n_moin3 = value; OnPropertyChanged(nameof(N_MOIN3)); } }

        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged(nameof(NUMBER)); } }

        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged(nameof(TAG)); } }

        private double? _anbar;
        public double? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged(nameof(ANBAR)); } }

        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged(nameof(RADIF)); } }

        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged(nameof(CUST_NO)); } }

        private int? _vaz;
        public int? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged(nameof(VAZ)); } }

        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged(nameof(NAMES)); } }

        private long? _dt;
        public long? DT { get => _dt; set { if (_dt == value) return; _dt = value; OnPropertyChanged(nameof(DT)); } }
    }
}
