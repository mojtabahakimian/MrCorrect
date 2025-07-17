using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_STUFGROUP : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private double? _code;
        public double? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }

        private byte[] _pic;
        public byte[] pic { get => _pic; set { if (_pic == value) return; _pic = value; OnPropertyChanged("pic"); } }

        private bool? _tic;
        public bool? tic { get => _tic; set { if (_tic == value) return; _tic = value; OnPropertyChanged("tic"); } }

        private double? _meghta;
        public double? MEGHTA { get => _meghta; set { if (_meghta == value) return; _meghta = value; OnPropertyChanged("MEGHTA"); } }

        private double? _meghjay;
        public double? MEGHJAY { get => _meghjay; set { if (_meghjay == value) return; _meghjay = value; OnPropertyChanged("MEGHJAY"); } }

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
