using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_MAP : INotifyPropertyChanged, ICloneable
    {
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        private int? _mpp;
        public int? MPP { get => _mpp; set { if (_mpp == value) return; _mpp = value; OnPropertyChanged("MPP"); } }

        private int? _mpcode;
        public int? MPCODE { get => _mpcode; set { if (_mpcode == value) return; _mpcode = value; OnPropertyChanged("MPCODE"); } }

        private string? _mpname;
        public string? MPNAME { get => _mpname; set { if (_mpname == value) return; _mpname = value; OnPropertyChanged("MPNAME"); } }

        private int? _osco;
        public int? OSCO { get => _osco; set { if (_osco == value) return; _osco = value; OnPropertyChanged("OSCO"); } }

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
