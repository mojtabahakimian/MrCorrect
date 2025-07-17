using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class MOGH_SUB_MODEL : INotifyPropertyChanged, ICloneable
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
        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged("DATE_S"); } }
        private int? _monum;
        public int? MONUM { get => _monum; set { if (_monum == value) return; _monum = value; OnPropertyChanged("MONUM"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private double? _hes_k;
        public double? HES_K { get => _hes_k; set { if (_hes_k == value) return; _hes_k = value; OnPropertyChanged("HES_K"); } }
        private double? _hes_m;
        public double? HES_M { get => _hes_m; set { if (_hes_m == value) return; _hes_m = value; OnPropertyChanged("HES_M"); } }
        private double? _hes_t;
        public double? HES_T { get => _hes_t; set { if (_hes_t == value) return; _hes_t = value; OnPropertyChanged("HES_T"); } }
        private string? _sharh;
        public string? SHARH { get => _sharh; set { if (_sharh == value) return; _sharh = value; OnPropertyChanged("SHARH"); } }
        private double? _bed;
        public double? BED { get => _bed; set { if (_bed == value) return; _bed = value; OnPropertyChanged("BED"); } }
        private double? _bes;
        public double? BES { get => _bes; set { if (_bes == value) return; _bes = value; OnPropertyChanged("BES"); } }
        private bool? _tick;
        public bool? TICK { get => _tick; set { if (_tick == value) return; _tick = value; OnPropertyChanged("TICK"); } }
        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }
        private double? _bank;
        public double? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }
        private int? _iddh;
        public int? IDDH { get => _iddh; set { if (_iddh == value) return; _iddh = value; OnPropertyChanged("IDDH"); } }
        private int? _id;
        public int? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

    }
}
