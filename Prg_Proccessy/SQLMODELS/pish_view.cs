using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class pish_view : INotifyPropertyChanged, ICloneable
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
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }
        private int? _anbar;
        public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }
        private double? _number1;
        public double? NUMBER1 { get => _number1; set { if (_number1 == value) return; _number1 = value; OnPropertyChanged("NUMBER1"); } }
        private long? _date_n;
        public long? DATE_N { get => _date_n; set { if (_date_n == value) return; _date_n = value; OnPropertyChanged("DATE_N"); } }
        private string? _tah;
        public string? TAH { get => _tah; set { if (_tah == value) return; _tah = value; OnPropertyChanged("TAH"); } }
        private double? _mas;
        public double? MAS { get => _mas; set { if (_mas == value) return; _mas = value; OnPropertyChanged("MAS"); } }
        private double? _vas;
        public double? VAS { get => _vas; set { if (_vas == value) return; _vas = value; OnPropertyChanged("VAS"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }
        private string? _molah;
        public string? MOLAH { get => _molah; set { if (_molah == value) return; _molah = value; OnPropertyChanged("MOLAH"); } }
        private double? _m_naghd;
        public double? M_NAGHD { get => _m_naghd; set { if (_m_naghd == value) return; _m_naghd = value; OnPropertyChanged("M_NAGHD"); } }
        private double? _mabl_var;
        public double? MABL_VAR { get => _mabl_var; set { if (_mabl_var == value) return; _mabl_var = value; OnPropertyChanged("MABL_VAR"); } }
        private string? _moin_var;
        public string? MOIN_VAR { get => _moin_var; set { if (_moin_var == value) return; _moin_var = value; OnPropertyChanged("MOIN_VAR"); } }
        private double? _mabl_hav;
        public double? MABL_HAV { get => _mabl_hav; set { if (_mabl_hav == value) return; _mabl_hav = value; OnPropertyChanged("MABL_HAV"); } }
        private string? _moin_hav;
        public string? MOIN_HAV { get => _moin_hav; set { if (_moin_hav == value) return; _moin_hav = value; OnPropertyChanged("MOIN_HAV"); } }
        private double? _mabl_haz;
        public double? MABL_HAZ { get => _mabl_haz; set { if (_mabl_haz == value) return; _mabl_haz = value; OnPropertyChanged("MABL_HAZ"); } }
        private string? _moin_haz;
        public string? MOIN_HAZ { get => _moin_haz; set { if (_moin_haz == value) return; _moin_haz = value; OnPropertyChanged("MOIN_HAZ"); } }
        private double? _takhfif;
        public double? TAKHFIF { get => _takhfif; set { if (_takhfif == value) return; _takhfif = value; OnPropertyChanged("TAKHFIF"); } }
        private string? _moin_khf;
        public string? MOIN_KHF { get => _moin_khf; set { if (_moin_khf == value) return; _moin_khf = value; OnPropertyChanged("MOIN_KHF"); } }
        private int? _anbarf;
        public int? ANBARF { get => _anbarf; set { if (_anbarf == value) return; _anbarf = value; OnPropertyChanged("ANBARF"); } }
        private double? _fnumco;
        public double? FNUMCO { get => _fnumco; set { if (_fnumco == value) return; _fnumco = value; OnPropertyChanged("FNUMCO"); } }
        private int? _depatman;
        public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged("DEPATMAN"); } }
        private int? _shift;
        public int? SHIFT { get => _shift; set { if (_shift == value) return; _shift = value; OnPropertyChanged("SHIFT"); } }
        private int? _cust_kind;
        public int? CUST_KIND { get => _cust_kind; set { if (_cust_kind == value) return; _cust_kind = value; OnPropertyChanged("CUST_KIND"); } }
        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }
        private string? _sharayet;
        public string? SHARAYET { get => _sharayet; set { if (_sharayet == value) return; _sharayet = value; OnPropertyChanged("SHARAYET"); } }
        private bool? _sgn1;
        public bool? SGN1 { get => _sgn1; set { if (_sgn1 == value) return; _sgn1 = value; OnPropertyChanged("SGN1"); } }
        private bool? _sgn2;
        public bool? SGN2 { get => _sgn2; set { if (_sgn2 == value) return; _sgn2 = value; OnPropertyChanged("SGN2"); } }
        private bool? _sgn3;
        public bool? SGN3 { get => _sgn3; set { if (_sgn3 == value) return; _sgn3 = value; OnPropertyChanged("SGN3"); } }
        private bool? _sgn4;
        public bool? SGN4 { get => _sgn4; set { if (_sgn4 == value) return; _sgn4 = value; OnPropertyChanged("SGN4"); } }
        private double? _mbaa;
        public double? MBAA { get => _mbaa; set { if (_mbaa == value) return; _mbaa = value; OnPropertyChanged("MBAA"); } }
        private string? _hmbaa;
        public string? HMBAA { get => _hmbaa; set { if (_hmbaa == value) return; _hmbaa = value; OnPropertyChanged("HMBAA"); } }
        private double? _tamir;
        public double? TAMIR { get => _tamir; set { if (_tamir == value) return; _tamir = value; OnPropertyChanged("TAMIR"); } }
        private bool? _ticmbaa;
        public bool? TICMBAA { get => _ticmbaa; set { if (_ticmbaa == value) return; _ticmbaa = value; OnPropertyChanged("TICMBAA"); } }
        private bool? _tkhf;
        public bool? TKHF { get => _tkhf; set { if (_tkhf == value) return; _tkhf = value; OnPropertyChanged("TKHF"); } }
        private bool? _okf;
        public bool? OKF { get => _okf; set { if (_okf == value) return; _okf = value; OnPropertyChanged("OKF"); } }
        private byte _sader;
        public byte SADER { get => _sader; set { if (_sader == value) return; _sader = value; OnPropertyChanged("SADER"); } }
        private double? _arzd;
        public double? ARZD { get => _arzd; set { if (_arzd == value) return; _arzd = value; OnPropertyChanged("ARZD"); } }
        private byte _arzkind;
        public byte ARZKIND { get => _arzkind; set { if (_arzkind == value) return; _arzkind = value; OnPropertyChanged("ARZKIND"); } }
        private long? _cddate;
        public long? CDDATE { get => _cddate; set { if (_cddate == value) return; _cddate = value; OnPropertyChanged("CDDATE"); } }
        private int? _cdtime;
        public int? CDTIME { get => _cdtime; set { if (_cdtime == value) return; _cdtime = value; OnPropertyChanged("CDTIME"); } }
        private long? _okdate;
        public long? OKDATE { get => _okdate; set { if (_okdate == value) return; _okdate = value; OnPropertyChanged("OKDATE"); } }
        private int? _oktime;
        public int? OKTIME { get => _oktime; set { if (_oktime == value) return; _oktime = value; OnPropertyChanged("OKTIME"); } }
        private bool? _jay;
        public bool? JAY { get => _jay; set { if (_jay == value) return; _jay = value; OnPropertyChanged("JAY"); } }
        private int? _modat_ppid;
        public int? MODAT_PPID { get => _modat_ppid; set { if (_modat_ppid == value) return; _modat_ppid = value; OnPropertyChanged("MODAT_PPID"); } }
        private int? _pepid;
        public int? PEPID { get => _pepid; set { if (_pepid == value) return; _pepid = value; OnPropertyChanged("PEPID"); } }
        private int? _peid;
        public int? PEID { get => _peid; set { if (_peid == value) return; _peid = value; OnPropertyChanged("PEID"); } }
        private int? _sgn1usid;
        public int? sgn1usid { get => _sgn1usid; set { if (_sgn1usid == value) return; _sgn1usid = value; OnPropertyChanged("sgn1usid"); } }
        private int? _sgn2usid;
        public int? sgn2usid { get => _sgn2usid; set { if (_sgn2usid == value) return; _sgn2usid = value; OnPropertyChanged("sgn2usid"); } }
        private int? _sgn3usid;
        public int? sgn3usid { get => _sgn3usid; set { if (_sgn3usid == value) return; _sgn3usid = value; OnPropertyChanged("sgn3usid"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        #region _HAVALEHA_MODEL_
        public string NAME { get; set; }
        #endregion

    }
}
