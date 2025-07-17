using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class OPANBACCESS : INotifyPropertyChanged, ICloneable
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
        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }
        private int? _anbco;
        public int? ANBCO { get => _anbco; set { if (_anbco == value) return; _anbco = value; OnPropertyChanged("ANBCO"); } }
        private int? _rdf;
        public int? RDF { get => _rdf; set { if (_rdf == value) return; _rdf = value; OnPropertyChanged("RDF"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
