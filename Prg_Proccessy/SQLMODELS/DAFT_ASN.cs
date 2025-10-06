using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class DAFT_ASN : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _firstnum;
        public int? FIRSTNUM { get => _firstnum; set { if (_firstnum == value) return; _firstnum = value; OnPropertyChanged("FIRSTNUM"); } }
        private int? _booknum;
        public int? BOOKNUM { get => _booknum; set { if (_booknum == value) return; _booknum = value; OnPropertyChanged("BOOKNUM"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private bool? _IsRowLocked;
        public bool? IsRowLocked
        {
            get
            {
                _IsRowLocked = FIRSTNUM > 0;
                return _IsRowLocked; 
            }
        }
    }
}