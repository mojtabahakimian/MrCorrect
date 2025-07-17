using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_VAHEDS : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private int? _code;
        public int? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }

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
