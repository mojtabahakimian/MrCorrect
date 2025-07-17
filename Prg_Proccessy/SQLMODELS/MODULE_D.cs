using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class MODULE_D : INotifyPropertyChanged, ICloneable
    {
        public MODULE_D()
        {
            NESBAT = 1;
            MABL_F = 0;
        }
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private int? _vahed;
        public int? VAHED { get => _vahed; set { if (_vahed == value) return; _vahed = value; OnPropertyChanged("VAHED"); } }

        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

        private double? _nesbat;
        public double? NESBAT { get => _nesbat; set { if (_nesbat == value) return; _nesbat = value; OnPropertyChanged("NESBAT"); } }

        private double? _mabl_f;
        public double? MABL_F { get => _mabl_f; set { if (_mabl_f == value) return; _mabl_f = value; OnPropertyChanged("MABL_F"); } }

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
