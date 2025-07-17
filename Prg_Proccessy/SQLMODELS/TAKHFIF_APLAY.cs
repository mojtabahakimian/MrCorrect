using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TAKHFIF_APLAY : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private int? _tid;
        public int? TID { get => _tid; set { if (_tid == value) return; _tid = value; OnPropertyChanged("TID"); } }

        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private double? _kind;
        public double? KIND { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("KIND"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }
    }
}
