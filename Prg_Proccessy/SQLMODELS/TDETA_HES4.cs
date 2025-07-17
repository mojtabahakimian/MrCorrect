using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TDETA_HES4 : INotifyPropertyChanged, ICloneable
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

        private string? _HESAB;
        public string? HESAB
        {
            get
            {
                if (!string.IsNullOrEmpty(TNUMBER4?.ToString()))
                {
                    _HESAB = $"{N_KOL}-{NUMBER}-{TNUMBER}-{TNUMBER2}-{TNUMBER3}-{TNUMBER4}";
                }
                return _HESAB;
            }
            set { if (_HESAB == value) return; _HESAB = value; OnPropertyChanged("HESAB"); }
        }

        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }

        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private int? _tnumber;
        public int? TNUMBER { get => _tnumber; set { if (_tnumber == value) return; _tnumber = value; OnPropertyChanged("TNUMBER"); } }

        private int? _tnumber2;
        public int? TNUMBER2 { get => _tnumber2; set { if (_tnumber2 == value) return; _tnumber2 = value; OnPropertyChanged("TNUMBER2"); } }

        private int? _tnumber3;
        public int? TNUMBER3 { get => _tnumber3; set { if (_tnumber3 == value) return; _tnumber3 = value; OnPropertyChanged("TNUMBER3"); } }

        private int? _tnumber4;
        public int? TNUMBER4 { get => _tnumber4; set { if (_tnumber4 == value) return; _tnumber4 = value; OnPropertyChanged("TNUMBER4"); } }

        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }

        private string? _tozih;
        public string? TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }

        private double? _bed_bes;
        public double? BED_BES { get => _bed_bes; set { if (_bed_bes == value) return; _bed_bes = value; OnPropertyChanged("BED_BES"); } }

        private string? _address;
        public string? ADDRESS { get => _address; set { if (_address == value) return; _address = value; OnPropertyChanged("ADDRESS"); } }

        private string? _tel;
        public string? TEL { get => _tel; set { if (_tel == value) return; _tel = value; OnPropertyChanged("TEL"); } }

        private string? _code_e;
        public string? CODE_E { get => _code_e; set { if (_code_e == value) return; _code_e = value; OnPropertyChanged("CODE_E"); } }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }

        private string? _ecode;
        public string? ECODE { get => _ecode; set { if (_ecode == value) return; _ecode = value; OnPropertyChanged("ECODE"); } }

        private string? _pcode;
        public string? PCODE { get => _pcode; set { if (_pcode == value) return; _pcode = value; OnPropertyChanged("PCODE"); } }

        private string? _iyalat;
        public string? IYALAT { get => _iyalat; set { if (_iyalat == value) return; _iyalat = value; OnPropertyChanged("IYALAT"); } }

        private string? _city;
        public string? CITY { get => _city; set { if (_city == value) return; _city = value; OnPropertyChanged("CITY"); } }

        private string? _mcodem;
        public string? MCODEM { get => _mcodem; set { if (_mcodem == value) return; _mcodem = value; OnPropertyChanged("MCODEM"); } }

        private int? _cust_cod;
        public int? CUST_COD { get => _cust_cod; set { if (_cust_cod == value) return; _cust_cod = value; OnPropertyChanged("CUST_COD"); } }

        private string? _mobile;
        public string? MOBILE { get => _mobile; set { if (_mobile == value) return; _mobile = value; OnPropertyChanged("MOBILE"); } }

        private string? _route_name;
        public string? ROUTE_NAME { get => _route_name; set { if (_route_name == value) return; _route_name = value; OnPropertyChanged("ROUTE_NAME"); } }

        private double? _longitude;
        public double? Longitude { get => _longitude; set { if (_longitude == value) return; _longitude = value; OnPropertyChanged("Longitude"); } }

        private double? _latitude;
        public double? Latitude { get => _latitude; set { if (_latitude == value) return; _latitude = value; OnPropertyChanged("Latitude"); } }

        private int? _ostanid;
        public int? OSTANID { get => _ostanid; set { if (_ostanid == value) return; _ostanid = value; OnPropertyChanged("OSTANID"); } }

        private int? _shahrid;
        public int? SHAHRID { get => _shahrid; set { if (_shahrid == value) return; _shahrid = value; OnPropertyChanged("SHAHRID"); } }

        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }

        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private int? _tob;
        public int? tob { get => _tob; set { if (_tob == value) return; _tob = value; OnPropertyChanged("tob"); } }

    }
}
