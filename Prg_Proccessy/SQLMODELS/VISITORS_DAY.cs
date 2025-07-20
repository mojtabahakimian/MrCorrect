using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISITORS_DAY : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }
        private long? _vdate;
        public long? VDATE { get => _vdate; set { if (_vdate == value) return; _vdate = value; OnPropertyChanged("VDATE"); } }
        private DateTime? _cdate;
        public DateTime? CDATE { get => _cdate; set { if (_cdate == value) return; _cdate = value; OnPropertyChanged("CDATE"); } }
        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }
        private bool? _okf;
        public bool? OKF { get => _okf; set { if (_okf == value) return; _okf = value; OnPropertyChanged("OKF"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}
