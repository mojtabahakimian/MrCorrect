using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_MARKAZHAZ : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private int? _mhaz_no;
        public int? MHAZ_NO { get => _mhaz_no; set { if (_mhaz_no == value) return; _mhaz_no = value; OnPropertyChanged("MHAZ_NO"); } }

        private string? _mhazname;
        public string? MHAZNAME { get => _mhazname; set { if (_mhazname == value) return; _mhazname = value; OnPropertyChanged("MHAZNAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
