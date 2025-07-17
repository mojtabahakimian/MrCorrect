using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CHREC_LSP_Q : INotifyPropertyChanged, ICloneable
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
        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }
        private string? _shobeh;
        public string? SHOBEH { get => _shobeh; set { if (_shobeh == value) return; _shobeh = value; OnPropertyChanged("SHOBEH"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private long? _pdts;
        public long? PDTS { get => _pdts; set { if (_pdts == value) return; _pdts = value; OnPropertyChanged("PDTS"); } }
        private int? _n_kol3;
        public int? N_KOL3 { get => _n_kol3; set { if (_n_kol3 == value) return; _n_kol3 = value; OnPropertyChanged("N_KOL3"); } }
        private int? _n_moin3;
        public int? N_MOIN3 { get => _n_moin3; set { if (_n_moin3 == value) return; _n_moin3 = value; OnPropertyChanged("N_MOIN3"); } }
        private int? _n_moin;
        public int? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }
        private int? _n_taf;
        public int? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }
        private int? _n_taf3;
        public int? N_TAF3 { get => _n_taf3; set { if (_n_taf3 == value) return; _n_taf3 = value; OnPropertyChanged("N_TAF3"); } }
        private string? _hes1;
        public string? HES1 { get => _hes1; set { if (_hes1 == value) return; _hes1 = value; OnPropertyChanged("HES1"); } }


        private string? _bank_name;
        public string? BANK_NAME { get => _bank_name; set { if (_bank_name == value) return; _bank_name = value; OnPropertyChanged("BANK_NAME"); } }

        private string? _hes1_name;
        public string? HES1_NAME { get => _hes1_name; set { if (_hes1_name == value) return; _hes1_name = value; OnPropertyChanged("HES1_NAME"); } }
    }
}
