using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CHREC_HP : INotifyPropertyChanged, ICloneable
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
        private long? _date;
        public long? DATE { get => _date; set { if (_date == value) return; _date = value; OnPropertyChanged("DATE"); } }
        private string? _molah;
        public string? MOLAH { get => _molah; set { if (_molah == value) return; _molah = value; OnPropertyChanged("MOLAH"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private int? _idh;
        public int? IDH { get => _idh; set { if (_idh == value) return; _idh = value; OnPropertyChanged("IDH"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
