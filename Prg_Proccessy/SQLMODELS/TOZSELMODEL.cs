using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TOZSELMODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private bool _IsSelected = false;
        public bool IsSelected { get => _IsSelected; set { if (_IsSelected == value) return; _IsSelected = value; OnPropertyChanged("IsSelected"); } }
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private long? _date_n;
        public long? DATE_N { get => _date_n; set { if (_date_n == value) return; _date_n = value; OnPropertyChanged("DATE_N"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private string? _hes;
        public string? hes { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("hes"); } }
        private string? _address;
        public string? ADDRESS { get => _address; set { if (_address == value) return; _address = value; OnPropertyChanged("ADDRESS"); } }
    }
}
