using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class FROOSH_COUNT : INotifyPropertyChanged, ICloneable
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
        private int? _tedad;
        public int? TEDAD { get => _tedad; set { if (_tedad == value) return; _tedad = value; OnPropertyChanged("TEDAD"); } }
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private double? _smeghk;
        public double? SMEGHk { get => _smeghk; set { if (_smeghk == value) return; _smeghk = value; OnPropertyChanged("SMEGHk"); } }
        private int? _depatman;
        public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged("DEPATMAN"); } }
        private string? _depname;
        public string? DEPNAME { get => _depname; set { if (_depname == value) return; _depname = value; OnPropertyChanged("DEPNAME"); } }
        private double? _smabl;
        public double? SMABL { get => _smabl; set { if (_smabl == value) return; _smabl = value; OnPropertyChanged("SMABL"); } }

    }
}
