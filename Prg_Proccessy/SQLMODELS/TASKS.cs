using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TASKS : INotifyPropertyChanged, ICloneable
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

        private int? _idnum;
        public int? IDNUM { get => _idnum; set { if (_idnum == value) return; _idnum = value; OnPropertyChanged("IDNUM"); } }

        private int? _gr;
        public int? GR { get => _gr; set { if (_gr == value) return; _gr = value; OnPropertyChanged("GR"); } }

        private int? _personel;
        public int? PERSONEL { get => _personel; set { if (_personel == value) return; _personel = value; OnPropertyChanged("PERSONEL"); } } //IDD


        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } } //For COMP_COD to Display instead of whole data from another table

        private string? _task;
        public string? TASK { get => _task; set { if (_task == value) return; _task = value; OnPropertyChanged("TASK"); } }

        private int? _periority;
        public int? PERIORITY { get => _periority; set { if (_periority == value) return; _periority = value; OnPropertyChanged("PERIORITY"); } }

        private int? _status;
        public int? STATUS { get => _status; set { if (_status == value) return; _status = value; OnPropertyChanged("STATUS"); } }

        private int? _stdate;
        public int? STDATE { get => _stdate; set { if (_stdate == value) return; _stdate = value; OnPropertyChanged("STDATE"); } }

        private int? _sttime;
        public int? STTIME { get => _sttime; set { if (_sttime == value) return; _sttime = value; OnPropertyChanged("STTIME"); } }

        private int? _endate;
        public int? ENDATE { get => _endate; set { if (_endate == value) return; _endate = value; OnPropertyChanged("ENDATE"); } }

        private int? _entime;
        public int? ENTIME { get => _entime; set { if (_entime == value) return; _entime = value; OnPropertyChanged("ENTIME"); } }

        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }

        private string? _comp_cod;
        public string? COMP_COD { get => _comp_cod; set { if (_comp_cod == value) return; _comp_cod = value; OnPropertyChanged("COMP_COD"); } } //hes

        private int? _sumtime;
        public int? SUMTIME { get => _sumtime; set { if (_sumtime == value) return; _sumtime = value; OnPropertyChanged("SUMTIME"); } }

        private byte[] _pic;
        public byte[] pic { get => _pic; set { if (_pic == value) return; _pic = value; OnPropertyChanged("pic"); } }

        private double? _ss;
        public double? ss { get => _ss; set { if (_ss == value) return; _ss = value; OnPropertyChanged("ss"); } }

        private int? _skid;
        public int? skid { get => _skid; set { if (_skid == value) return; _skid = value; OnPropertyChanged("skid"); } }

        private long? _num;
        public long? num { get => _num; set { if (_num == value) return; _num = value; OnPropertyChanged("num"); } }

        private long? _tg;
        public long? tg { get => _tg; set { if (_tg == value) return; _tg = value; OnPropertyChanged("tg"); } }

        private DateTime? _ctim;
        public DateTime? CTIM { get => _ctim; set { if (_ctim == value) return; _ctim = value; OnPropertyChanged("CTIM"); } }

        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }

        private int? _see;
        public int? SEE { get => _see; set { if (_see == value) return; _see = value; OnPropertyChanged("SEE"); } }

        private DateTime? _seet;
        public DateTime? SEET { get => _seet; set { if (_seet == value) return; _seet = value; OnPropertyChanged("SEET"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private bool _IsSelected = false;
        public bool IsSelected { get => _IsSelected; set { if (_IsSelected == value) return; _IsSelected = value; OnPropertyChanged("IsSelected"); } }

        #region MyRegion
        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }
        private string? _events;
        public string? EVENTS { get => _events; set { if (_events == value) return; _events = value; OnPropertyChanged("EVENTS"); } }
        private int? _company;
        public int? COMPANY { get => _company; set { if (_company == value) return; _company = value; OnPropertyChanged("COMPANY"); } }

        private int? _stdatem;
        public int? STDATEm { get => _stdatem; set { if (_stdatem == value) return; _stdatem = value; OnPropertyChanged("STDATEm"); } }
        private int? _sttimem;
        public int? STTIMEm { get => _sttimem; set { if (_sttimem == value) return; _sttimem = value; OnPropertyChanged("STTIMEm"); } }

        private string? _usernamem;
        public string? USERNAMEm { get => _usernamem; set { if (_usernamem == value) return; _usernamem = value; OnPropertyChanged("USERNAMEm"); } }
        private int? _sumtimem;
        public int? SUMTIMEm { get => _sumtimem; set { if (_sumtimem == value) return; _sumtimem = value; OnPropertyChanged("SUMTIMEm"); } }
        private byte[] _picm;
        public byte[] picm { get => _picm; set { if (_picm == value) return; _picm = value; OnPropertyChanged("picm"); } }
        private int? _skidm;
        public int? skidm { get => _skidm; set { if (_skidm == value) return; _skidm = value; OnPropertyChanged("skidm"); } }
        private long? _numm;
        public long? numm { get => _numm; set { if (_numm == value) return; _numm = value; OnPropertyChanged("numm"); } }
        private long? _tgm;
        public long? tgm { get => _tgm; set { if (_tgm == value) return; _tgm = value; OnPropertyChanged("tgm"); } }

        #endregion
    }
}
