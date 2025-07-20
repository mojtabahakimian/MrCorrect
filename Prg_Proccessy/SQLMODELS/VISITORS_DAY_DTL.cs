using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISITORS_DAY_DTL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        public VISITORS_DAY_DTL()
        {
            TOPLACE = false;
            RACTIVE = true;
        }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }

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

        private long? _vdate;
        public long? VDATE { get => _vdate; set { if (_vdate == value) return; _vdate = value; OnPropertyChanged("VDATE"); } }
        private string? _coust_no;
        public string? COUST_NO { get => _coust_no; set { if (_coust_no == value) return; _coust_no = value; OnPropertyChanged("COUST_NO"); } }
        private DateTime? _cdate;
        public DateTime? CDATE { get => _cdate; set { if (_cdate == value) return; _cdate = value; OnPropertyChanged("CDATE"); } }
        private bool? _ractive;
        public bool? RACTIVE { get => _ractive; set { if (_ractive == value) return; _ractive = value; OnPropertyChanged("RACTIVE"); } }
        private string? _class;
        public string? CLASS { get => _class; set { if (_class == value) return; _class = value; OnPropertyChanged("CLASS"); } }
        private bool? _toplace;
        public bool? TOPLACE { get => _toplace; set { if (_toplace == value) return; _toplace = value; OnPropertyChanged("TOPLACE"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}
