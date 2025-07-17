using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_MAP_GRP : INotifyPropertyChanged, ICloneable
    {
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        private int? _mpp;
        public int? MPP { get => _mpp; set { if (_mpp == value) return; _mpp = value; OnPropertyChanged("MPP"); } }

        private string? _mpname;
        public string? MPNAME { get => _mpname; set { if (_mpname == value) return; _mpname = value; OnPropertyChanged("MPNAME"); } }

        private int? _sizef;
        public int? SIZEF { get => _sizef; set { if (_sizef == value) return; _sizef = value; OnPropertyChanged("SIZEF"); } }

        private int? _startf;
        public int? STARTF { get => _startf; set { if (_startf == value) return; _startf = value; OnPropertyChanged("STARTF"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
