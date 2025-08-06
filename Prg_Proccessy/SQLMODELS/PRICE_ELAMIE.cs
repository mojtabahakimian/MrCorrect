using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PRICE_ELAMIE : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _pepid;
        public int? PEPID { get => _pepid; set { if (_pepid == value) return; _pepid = value; OnPropertyChanged("PEPID"); } }
        private string? _pepname;
        public string? PEPNAME { get => _pepname; set { if (_pepname == value) return; _pepname = value; OnPropertyChanged("PEPNAME"); } }
        private int? _pepdate;
        public int? PEPDATE { get => _pepdate; set { if (_pepdate == value) return; _pepdate = value; OnPropertyChanged("PEPDATE"); } }
        private DateTime? _tr_date;
        public DateTime? TR_DATE { get => _tr_date; set { if (_tr_date == value) return; _tr_date = value; OnPropertyChanged("TR_DATE"); } }
        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }
        private bool? _sgn1;
        public bool? SGN1 { get => _sgn1; set { if (_sgn1 == value) return; _sgn1 = value; OnPropertyChanged("SGN1"); } }
        private bool? _sgn2;
        public bool? SGN2 { get => _sgn2; set { if (_sgn2 == value) return; _sgn2 = value; OnPropertyChanged("SGN2"); } }
        private bool? _sgn3;
        public bool? SGN3 { get => _sgn3; set { if (_sgn3 == value) return; _sgn3 = value; OnPropertyChanged("SGN3"); } }
        private bool? _sgn4;
        public bool? SGN4 { get => _sgn4; set { if (_sgn4 == value) return; _sgn4 = value; OnPropertyChanged("SGN4"); } }
        private int? _pepdepart;
        public int? PEPDEPART { get => _pepdepart; set { if (_pepdepart == value) return; _pepdepart = value; OnPropertyChanged("PEPDEPART"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}
