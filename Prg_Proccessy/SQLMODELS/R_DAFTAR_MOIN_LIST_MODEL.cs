using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class R_DAFTAR_MOIN_LIST_MODEL : INotifyPropertyChanged
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }

        private int? @_base;
        public int? @base { get => @_base; set { if (@_base == value) return; @_base = value; OnPropertyChanged("base"); } }

        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged("DATE_S"); } }

        private int? _hes_k;
        public int? HES_K { get => _hes_k; set { if (_hes_k == value) return; _hes_k = value; OnPropertyChanged("HES_K"); } }

        private int? _hes_m;
        public int? HES_M { get => _hes_m; set { if (_hes_m == value) return; _hes_m = value; OnPropertyChanged("HES_M"); } }

        private int? _hes_t;
        public int? HES_T { get => _hes_t; set { if (_hes_t == value) return; _hes_t = value; OnPropertyChanged("HES_T"); } }

        private int? _hes_t2;
        public int? HES_T2 { get => _hes_t2; set { if (_hes_t2 == value) return; _hes_t2 = value; OnPropertyChanged("HES_T2"); } }

        private string? _sharh;
        public string? SHARH { get => _sharh; set { if (_sharh == value) return; _sharh = value; OnPropertyChanged("SHARH"); } }

        private double? _bed;
        public double? BED { get => _bed; set { if (_bed == value) return; _bed = value; OnPropertyChanged("BED"); } }

        private double? _bes;
        public double? BES { get => _bes; set { if (_bes == value) return; _bes = value; OnPropertyChanged("BES"); } }

        private double? _mand;
        public double? MAND { get => _mand; set { if (_mand == value) return; _mand = value; OnPropertyChanged("MAND"); } }

        private long? _id;
        public long? id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("id"); } }

        private double? _no_s;
        public double? NO_S { get => _no_s; set { if (_no_s == value) return; _no_s = value; OnPropertyChanged("NO_S"); } }

        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }

        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }

        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }

        private double? _arzd;
        public double? ARZD { get => _arzd; set { if (_arzd == value) return; _arzd = value; OnPropertyChanged("ARZD"); } }

        private int? _hes_t3;
        public int? HES_T3 { get => _hes_t3; set { if (_hes_t3 == value) return; _hes_t3 = value; OnPropertyChanged("HES_T3"); } }

        private int? _hes_t4;
        public int? HES_T4 { get => _hes_t4; set { if (_hes_t4 == value) return; _hes_t4 = value; OnPropertyChanged("HES_T4"); } }

        private string? _tafziln;
        public string? TAFZILN { get => _tafziln; set { if (_tafziln == value) return; _tafziln = value; OnPropertyChanged("TAFZILN"); } }

        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }

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
