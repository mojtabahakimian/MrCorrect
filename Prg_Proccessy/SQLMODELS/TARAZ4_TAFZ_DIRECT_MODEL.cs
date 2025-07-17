using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TARAZ4_TAFZ_DIRECT_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private int? _tnumber;
        public int? TNUMBER { get => _tnumber; set { if (_tnumber == value) return; _tnumber = value; OnPropertyChanged("TNUMBER"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private double? _sumofbed;
        public double? SumOfBED { get => _sumofbed; set { if (_sumofbed == value) return; _sumofbed = value; OnPropertyChanged("SumOfBED"); } }
        private double? _sumofbes;
        public double? SumOfBES { get => _sumofbes; set { if (_sumofbes == value) return; _sumofbes = value; OnPropertyChanged("SumOfBES"); } }
        private string? _moin;
        public string? MOIN { get => _moin; set { if (_moin == value) return; _moin = value; OnPropertyChanged("MOIN"); } }
        private string? _tafzil;
        public string? TAFZIL { get => _tafzil; set { if (_tafzil == value) return; _tafzil = value; OnPropertyChanged("TAFZIL"); } }
        private double? _bed;
        public double? bed { get => _bed; set { if (_bed == value) return; _bed = value; OnPropertyChanged("bed"); } }
        private double? _bes;
        public double? bes { get => _bes; set { if (_bes == value) return; _bes = value; OnPropertyChanged("bes"); } }
        private string? _kolnam;
        public string? KOLNAM { get => _kolnam; set { if (_kolnam == value) return; _kolnam = value; OnPropertyChanged("KOLNAM"); } }
        private string? _moiname;
        public string? MOINAME { get => _moiname; set { if (_moiname == value) return; _moiname = value; OnPropertyChanged("MOINAME"); } }
        private string? _tafname;
        public string? TAFNAME { get => _tafname; set { if (_tafname == value) return; _tafname = value; OnPropertyChanged("TAFNAME"); } }
        private string? _koljam;
        public string? KOLjam { get => _koljam; set { if (_koljam == value) return; _koljam = value; OnPropertyChanged("KOLjam"); } }
        private string? _moinjam;
        public string? MOINJAM { get => _moinjam; set { if (_moinjam == value) return; _moinjam = value; OnPropertyChanged("MOINJAM"); } }
        private string? _ecode;
        public string? ECODE { get => _ecode; set { if (_ecode == value) return; _ecode = value; OnPropertyChanged("ECODE"); } }
    }
}
