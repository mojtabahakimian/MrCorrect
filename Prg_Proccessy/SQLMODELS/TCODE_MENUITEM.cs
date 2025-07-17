using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCODE_MENUITEM : INotifyPropertyChanged, ICloneable
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
        private double? _code;
        public double? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }

        private byte[] _pic;
        public byte[] pic { get => _pic; set { if (_pic == value) return; _pic = value; OnPropertyChanged("pic"); } }

        private int? _anbar;
        public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }

        private bool? _tic;
        public bool? tic { get => _tic; set { if (_tic == value) return; _tic = value; OnPropertyChanged("tic"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

    }
}
