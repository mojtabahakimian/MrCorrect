using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class DTL_MANF_SUB_DAY : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _prgid;
        public int? PRGID { get => _prgid; set { if (_prgid == value) return; _prgid = value; OnPropertyChanged("PRGID"); } }
        private string? _codb;
        public string? CODB { get => _codb; set { if (_codb == value) return; _codb = value; OnPropertyChanged("CODB"); } }
        private int? _vahed;
        public int? VAHED { get => _vahed; set { if (_vahed == value) return; _vahed = value; OnPropertyChanged("VAHED"); } }
        private double? _megh;
        public double? MEGH { get => _megh; set { if (_megh == value) return; _megh = value; OnPropertyChanged("MEGH"); } }
        private double? _meghk;
        public double? MEGHK { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged("MEGHK"); } }
        private double? _pert;
        public double? PERT { get => _pert; set { if (_pert == value) return; _pert = value; OnPropertyChanged("PERT"); } }
        private double? _kolmav;
        public double? KOLMAV { get => _kolmav; set { if (_kolmav == value) return; _kolmav = value; OnPropertyChanged("KOLMAV"); } }
        private bool? _pased;
        public bool? PASED { get => _pased; set { if (_pased == value) return; _pased = value; OnPropertyChanged("PASED"); } }
        private int? _radah;
        public int? RADAH { get => _radah; set { if (_radah == value) return; _radah = value; OnPropertyChanged("RADAH"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private double? _sumofmabl;
        public double? SumOfMABL { get => _sumofmabl; set { if (_sumofmabl == value) return; _sumofmabl = value; OnPropertyChanged("SumOfMABL"); } }
        private double? _sumofmablk;
        public double? SumOfMABLK { get => _sumofmablk; set { if (_sumofmablk == value) return; _sumofmablk = value; OnPropertyChanged("SumOfMABLK"); } }
    }
}
