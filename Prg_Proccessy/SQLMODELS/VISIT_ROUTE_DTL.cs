using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISIT_ROUTE_DTL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        public VISIT_ROUTE_DTL()
        {
            RACTIVE = true;
        }

        private string? _route_name;
        public string? ROUTE_NAME { get => _route_name; set { if (_route_name == value) return; _route_name = value; OnPropertyChanged("ROUTE_NAME"); } }

        private string? _name_hes;
        public string? NAME_HES
        {
            get => _name_hes;
            set
            {
                if (_name_hes == value) return;
                _name_hes = value;
                OnPropertyChanged(nameof(NAME_HES));
            }
        }

        private string? _coust_no;
        public string? COUST_NO { get => _coust_no; set { if (_coust_no == value) return; _coust_no = value; OnPropertyChanged("COUST_NO"); } }
        private bool? _ractive;
        public bool? RACTIVE { get => _ractive; set { if (_ractive == value) return; _ractive = value; OnPropertyChanged("RACTIVE"); } }
        private int? _idr;
        public int? IDR { get => _idr; set { if (_idr == value) return; _idr = value; OnPropertyChanged("IDR"); } }
        private string? _class;
        public string? CLASS { get => _class; set { if (_class == value) return; _class = value; OnPropertyChanged("CLASS"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}