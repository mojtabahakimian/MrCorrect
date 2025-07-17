using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class DEPART_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private int? _depatman;
        public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged("DEPATMAN"); } }

        private string? _depname;
        public string? DEPNAME { get => _depname; set { if (_depname == value) return; _depname = value; OnPropertyChanged("DEPNAME"); } }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }

        private string? _depart;
        public string? DEPART { get => _depart; set { if (_depart == value) return; _depart = value; OnPropertyChanged("DEPART"); } }

        private string? _pcode;
        public string? PCODE { get => _pcode; set { if (_pcode == value) return; _pcode = value; OnPropertyChanged("PCODE"); } }
        private string? _bbc;
        public string? BBC { get => _bbc; set { if (_bbc == value) return; _bbc = value; OnPropertyChanged("BBC"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
