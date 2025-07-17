using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TAKHFIF_DEF_DTL : INotifyPropertyChanged, ICloneable
    {
        public TAKHFIF_DEF_DTL()
        {
            TPERS = 0;
            TMAB = 0;
            AZMAB = 0;
            TAMAB = 0;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        private int? _tid;
        public int? TID { get => _tid; set { if (_tid == value) return; _tid = value; OnPropertyChanged("TID"); } }

        private double? _azmab;
        public double? AZMAB { get => _azmab; set { if (_azmab == value) return; _azmab = value; OnPropertyChanged("AZMAB"); } }

        private double? _tamab;
        public double? TAMAB { get => _tamab; set { if (_tamab == value) return; _tamab = value; OnPropertyChanged("TAMAB"); } }

        private double? _tpers;
        public double? TPERS { get => _tpers; set { if (_tpers == value) return; _tpers = value; OnPropertyChanged("TPERS"); } }

        private double? _tmab;
        public double? TMAB { get => _tmab; set { if (_tmab == value) return; _tmab = value; OnPropertyChanged("TMAB"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
