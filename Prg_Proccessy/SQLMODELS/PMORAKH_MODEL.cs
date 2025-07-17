using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PMORAKH_MODEL : INotifyPropertyChanged, ICloneable
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
        private int? _code;
        public int? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private string? _CODE_NAME;
        public string? CODE_NAME { get => _CODE_NAME; set { if (_CODE_NAME == value) return; _CODE_NAME = value; OnPropertyChanged("CODE_NAME"); } }

        private long? _modate;
        public long? MODATE { get => _modate; set { if (_modate == value) return; _modate = value; OnPropertyChanged("MODATE"); } }
        private int? _morakhday;
        public int? MORAKHDAY { get => _morakhday; set { if (_morakhday == value) return; _morakhday = value; OnPropertyChanged("MORAKHDAY"); } }
        private int? _mand;
        public int? MAND { get => _mand; set { if (_mand == value) return; _mand = value; OnPropertyChanged("MAND"); } }

        private int _kindm;
        public int KINDM { get => _kindm; set { if (_kindm == value) return; _kindm = value; OnPropertyChanged("KINDM"); } }

        private string? _molah;
        public string? MOLAH { get => _molah; set { if (_molah == value) return; _molah = value; OnPropertyChanged("MOLAH"); } }

        private long? _mostdate;
        public long? MOSTDATE { get => _mostdate; set { if (_mostdate == value) return; _mostdate = value; OnPropertyChanged("MOSTDATE"); } }
        private long? _moendate;
        public long? MOENDATE { get => _moendate; set { if (_moendate == value) return; _moendate = value; OnPropertyChanged("MOENDATE"); } }
        private bool? _okf;
        public bool? OKF { get => _okf; set { if (_okf == value) return; _okf = value; OnPropertyChanged("OKF"); } }
        private int? _idnum;
        public int? IDNUM { get => _idnum; set { if (_idnum == value) return; _idnum = value; OnPropertyChanged("IDNUM"); } }
        private int? _sgn1usid;
        public int? sgn1usid { get => _sgn1usid; set { if (_sgn1usid == value) return; _sgn1usid = value; OnPropertyChanged("sgn1usid"); } }
        private int? _sgn2usid;
        public int? sgn2usid { get => _sgn2usid; set { if (_sgn2usid == value) return; _sgn2usid = value; OnPropertyChanged("sgn2usid"); } }
        private int? _sgn3usid;
        public int? sgn3usid { get => _sgn3usid; set { if (_sgn3usid == value) return; _sgn3usid = value; OnPropertyChanged("sgn3usid"); } }
        private bool? _sgn1;
        public bool? SGN1 { get => _sgn1; set { if (_sgn1 == value) return; _sgn1 = value; OnPropertyChanged("SGN1"); } }
        private bool? _sgn2;
        public bool? SGN2 { get => _sgn2; set { if (_sgn2 == value) return; _sgn2 = value; OnPropertyChanged("SGN2"); } }
        private bool? _sgn3;
        public bool? SGN3 { get => _sgn3; set { if (_sgn3 == value) return; _sgn3 = value; OnPropertyChanged("SGN3"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
