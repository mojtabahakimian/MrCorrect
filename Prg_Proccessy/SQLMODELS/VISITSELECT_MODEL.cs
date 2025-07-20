using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISISELECT_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private string? _route_name;
        public string? ROUTE_NAME { get => _route_name; set { if (_route_name == value) return; _route_name = value; OnPropertyChanged("ROUTE_NAME"); } }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }
        private string? _iyalat;
        public string? IYALAT { get => _iyalat; set { if (_iyalat == value) return; _iyalat = value; OnPropertyChanged("IYALAT"); } }
        private string? _city;
        public string? CITY { get => _city; set { if (_city == value) return; _city = value; OnPropertyChanged("CITY"); } }
        private string? _district;
        public string? District { get => _district; set { if (_district == value) return; _district = value; OnPropertyChanged("District"); } }
        private bool? _ractive;
        public bool? RACTIVE { get => _ractive; set { if (_ractive == value) return; _ractive = value; OnPropertyChanged("RACTIVE"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
    }
}
