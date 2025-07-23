using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CUST_HESAB_COMBINED : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private string? _hes;
        public string? hes { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("hes"); } }

        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }

        private string? _NAME_HES;
        public string? NAME_HES { get => _NAME_HES; set { if (_NAME_HES == value) return; _NAME_HES = value; OnPropertyChanged("NAME_HES"); } }
        public override string ToString() => NAME;
    }
}
