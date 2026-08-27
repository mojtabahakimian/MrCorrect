using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class KALAS : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private string? _bargah;
        public string? BARGAH { get => _bargah; set { if (_bargah == value) return; _bargah = value; OnPropertyChanged("BARGAH"); } }
        private string? _anbname;
        public string? ANBNAME { get => _anbname; set { if (_anbname == value) return; _anbname = value; OnPropertyChanged("ANBNAME"); } }
        private double? _number1;
        public double? NUMBER1 { get => _number1; set { if (_number1 == value) return; _number1 = value; OnPropertyChanged("NUMBER1"); } }
        private long? _date_n;
        public long? DATE_N { get => _date_n; set { if (_date_n == value) return; _date_n = value; OnPropertyChanged("DATE_N"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private string? _custname;
        public string? CUSTNAME { get => _custname; set { if (_custname == value) return; _custname = value; OnPropertyChanged("CUSTNAME"); } }
        private string? _molah;
        public string? MOLAH { get => _molah; set { if (_molah == value) return; _molah = value; OnPropertyChanged("MOLAH"); } }
        private int? _anbarf;
        public int? ANBARF { get => _anbarf; set { if (_anbarf == value) return; _anbarf = value; OnPropertyChanged("ANBARF"); } }
        private double? _fnumco;
        public double? FNUMCO { get => _fnumco; set { if (_fnumco == value) return; _fnumco = value; OnPropertyChanged("FNUMCO"); } }
        private double? _megh;
        public double? MEGH { get => _megh; set { if (_megh == value) return; _megh = value; OnPropertyChanged("MEGH"); } }
        private double? _meghk;
        public double? MEGHk { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged("MEGHk"); } }
        private double? _megh_mar;
        public double? MEGH_MAR { get => _megh_mar; set { if (_megh_mar == value) return; _megh_mar = value; OnPropertyChanged("MEGH_MAR"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private string? _kala;
        public string? kala { get => _kala; set { if (_kala == value) return; _kala = value; OnPropertyChanged("kala"); } }
        private double? _mabl_k;
        public double? MABL_K { get => _mabl_k; set { if (_mabl_k == value) return; _mabl_k = value; OnPropertyChanged("MABL_K"); } }
        private double? _sanad_no;
        public double? SANAD_NO { get => _sanad_no; set { if (_sanad_no == value) return; _sanad_no = value; OnPropertyChanged("SANAD_NO"); } }
        private double? _cust_no;
        public double? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }
        private string? _vahedname;
        public string? VAHEDNAME { get => _vahedname; set { if (_vahedname == value) return; _vahedname = value; OnPropertyChanged("VAHEDNAME"); } }
        private string? _grpname;
        public string? GRPNAME { get => _grpname; set { if (_grpname == value) return; _grpname = value; OnPropertyChanged("GRPNAME"); } }
        private long? _code;
        public long? code { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("code"); } }
        private string? _hes;
        public string? hes { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("hes"); } }
        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }
        private string? _shname;
        public string? SHNAME { get => _shname; set { if (_shname == value) return; _shname = value; OnPropertyChanged("SHNAME"); } }
        private string? _custkname;
        public string? CUSTKNAME { get => _custkname; set { if (_custkname == value) return; _custkname = value; OnPropertyChanged("CUSTKNAME"); } }
        private string? _depname;
        public string? DEPNAME { get => _depname; set { if (_depname == value) return; _depname = value; OnPropertyChanged("DEPNAME"); } }
        private string? _mandah;
        public string? MANDAH { get => _mandah; set { if (_mandah == value) return; _mandah = value; OnPropertyChanged("MANDAH"); } }
        private int? _shift_id;
        public int? SHIFT_ID { get => _shift_id; set { if (_shift_id == value) return; _shift_id = value; OnPropertyChanged("SHIFT_ID"); } }
        private int? _depatman;
        public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged("DEPATMAN"); } }
        private int? _cust_cod;
        public int? CUST_COD { get => _cust_cod; set { if (_cust_cod == value) return; _cust_cod = value; OnPropertyChanged("CUST_COD"); } }
        private int? _tagcode;
        public int? TAGCODE { get => _tagcode; set { if (_tagcode == value) return; _tagcode = value; OnPropertyChanged("TAGCODE"); } }
        private double? _grpcode;
        public double? GRPCODE { get => _grpcode; set { if (_grpcode == value) return; _grpcode = value; OnPropertyChanged("GRPCODE"); } }
        private int? _anbarcode;
        public int? ANBARCODE { get => _anbarcode; set { if (_anbarcode == value) return; _anbarcode = value; OnPropertyChanged("ANBARCODE"); } }
        private int? _vahcode;
        public int? VAHCODE { get => _vahcode; set { if (_vahcode == value) return; _vahcode = value; OnPropertyChanged("VAHCODE"); } }
        private long? _id;
        public long? id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("id"); } }
        private double? _mas;
        public double? MAS { get => _mas; set { if (_mas == value) return; _mas = value; OnPropertyChanged("MAS"); } }
        private string? _n_rasid;
        public string? N_RASID { get => _n_rasid; set { if (_n_rasid == value) return; _n_rasid = value; OnPropertyChanged("N_RASID"); } }
        private string? _n_fani;
        public string? N_FANI { get => _n_fani; set { if (_n_fani == value) return; _n_fani = value; OnPropertyChanged("N_FANI"); } }
        private string? _sharayet;
        public string? SHARAYET { get => _sharayet; set { if (_sharayet == value) return; _sharayet = value; OnPropertyChanged("SHARAYET"); } }
        private double? _imbaa;
        public double? IMBAA { get => _imbaa; set { if (_imbaa == value) return; _imbaa = value; OnPropertyChanged("IMBAA"); } }
        private string? _hmbaa;
        public string? HMBAA { get => _hmbaa; set { if (_hmbaa == value) return; _hmbaa = value; OnPropertyChanged("HMBAA"); } }
        private double? _tamir;
        public double? TAMIR { get => _tamir; set { if (_tamir == value) return; _tamir = value; OnPropertyChanged("TAMIR"); } }
        private int? _ticmbaa;
        public int? TICMBAA { get => _ticmbaa; set { if (_ticmbaa == value) return; _ticmbaa = value; OnPropertyChanged("TICMBAA"); } }
        private bool? _okf;
        public bool? OKF { get => _okf; set { if (_okf == value) return; _okf = value; OnPropertyChanged("OKF"); } }
        private string? _tozih;
        public string? TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }
        private double? _b_sef;
        public double? B_SEF { get => _b_sef; set { if (_b_sef == value) return; _b_sef = value; OnPropertyChanged("B_SEF"); } }
        private double? _n_sef;
        public double? N_SEF { get => _n_sef; set { if (_n_sef == value) return; _n_sef = value; OnPropertyChanged("N_SEF"); } }
        private double? _min_m;
        public double? MIN_M { get => _min_m; set { if (_min_m == value) return; _min_m = value; OnPropertyChanged("MIN_M"); } }
        private double? _max_m;
        public double? MAX_M { get => _max_m; set { if (_max_m == value) return; _max_m = value; OnPropertyChanged("MAX_M"); } }
        private double? _radah;
        public double? RADAH { get => _radah; set { if (_radah == value) return; _radah = value; OnPropertyChanged("RADAH"); } }
        private short? _kindk;
        public short? KINDK { get => _kindk; set { if (_kindk == value) return; _kindk = value; OnPropertyChanged("KINDK"); } }
        private double? _mabl_f;
        public double? MABL_F { get => _mabl_f; set { if (_mabl_f == value) return; _mabl_f = value; OnPropertyChanged("MABL_F"); } }
        private int? _depart;
        public int? DEPART { get => _depart; set { if (_depart == value) return; _depart = value; OnPropertyChanged("DEPART"); } }
        private bool? _cmbaa;
        public bool? CMBAA { get => _cmbaa; set { if (_cmbaa == value) return; _cmbaa = value; OnPropertyChanged("CMBAA"); } }
        private double? _vazn;
        public double? vazn { get => _vazn; set { if (_vazn == value) return; _vazn = value; OnPropertyChanged("vazn"); } }
        private double? _n_taf;
        public double? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }
        private double? _totalarz;
        public double? TOTALARZ { get => _totalarz; set { if (_totalarz == value) return; _totalarz = value; OnPropertyChanged("TOTALARZ"); } }
        private double? _n_kol;
        public double? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private double? _n_moin;
        public double? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }
        private int? _mm;
        public int? MM { get => _mm; set { if (_mm == value) return; _mm = value; OnPropertyChanged("MM"); } }
        private double? _khfr;
        public double? KHFR { get => _khfr; set { if (_khfr == value) return; _khfr = value; OnPropertyChanged("KHFR"); } }
        private double? _ghfr;
        public double? GHFR { get => _ghfr; set { if (_ghfr == value) return; _ghfr = value; OnPropertyChanged("GHFR"); } }
        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }
        private int? _vahed;
        public int? VAHED { get => _vahed; set { if (_vahed == value) return; _vahed = value; OnPropertyChanged("VAHED"); } }
        private int? _sader;
        public int? SADER { get => _sader; set { if (_sader == value) return; _sader = value; OnPropertyChanged("SADER"); } }
        private double? _arzd;
        public double? ARZD { get => _arzd; set { if (_arzd == value) return; _arzd = value; OnPropertyChanged("ARZD"); } }
        private int? _arzkind;
        public int? ARZKIND { get => _arzkind; set { if (_arzkind == value) return; _arzkind = value; OnPropertyChanged("ARZKIND"); } }
        private long? _cddate;
        public long? CDDATE { get => _cddate; set { if (_cddate == value) return; _cddate = value; OnPropertyChanged("CDDATE"); } }
        private int? _cdtime;
        public int? CDTIME { get => _cdtime; set { if (_cdtime == value) return; _cdtime = value; OnPropertyChanged("CDTIME"); } }
        private long? _okdate;
        public long? OKDATE { get => _okdate; set { if (_okdate == value) return; _okdate = value; OnPropertyChanged("OKDATE"); } }
        private int? _oktime;
        public int? OKTIME { get => _oktime; set { if (_oktime == value) return; _oktime = value; OnPropertyChanged("OKTIME"); } }
        private double? _avrage;
        public double? AVRAGE { get => _avrage; set { if (_avrage == value) return; _avrage = value; OnPropertyChanged("AVRAGE"); } }
        private double? _mabrial;
        public double? mabrial { get => _mabrial; set { if (_mabrial == value) return; _mabrial = value; OnPropertyChanged("mabrial"); } }
        private int? _anbaras;
        public int? ANBARAS { get => _anbaras; set { if (_anbaras == value) return; _anbaras = value; OnPropertyChanged("ANBARAS"); } }
        private string? _ecode;
        public string? ECODE { get => _ecode; set { if (_ecode == value) return; _ecode = value; OnPropertyChanged("ECODE"); } }
        private string? _pcode;
        public string? PCODE { get => _pcode; set { if (_pcode == value) return; _pcode = value; OnPropertyChanged("PCODE"); } }
        private string? _iyalat;
        public string? IYALAT { get => _iyalat; set { if (_iyalat == value) return; _iyalat = value; OnPropertyChanged("IYALAT"); } }
        private string? _city;
        public string? CITY { get => _city; set { if (_city == value) return; _city = value; OnPropertyChanged("CITY"); } }
        private double? _tkhn;
        public double? TKHN { get => _tkhn; set { if (_tkhn == value) return; _tkhn = value; OnPropertyChanged("TKHN"); } }
        private string? _col1;
        public string? col1 { get => _col1; set { if (_col1 == value) return; _col1 = value; OnPropertyChanged("col1"); } }
        private string? _col2;
        public string? col2 { get => _col2; set { if (_col2 == value) return; _col2 = value; OnPropertyChanged("col2"); } }
        private string? _col3;
        public string? col3 { get => _col3; set { if (_col3 == value) return; _col3 = value; OnPropertyChanged("col3"); } }
        private string? _col4;
        public string? col4 { get => _col4; set { if (_col4 == value) return; _col4 = value; OnPropertyChanged("col4"); } }
        private string? _col5;
        public string? col5 { get => _col5; set { if (_col5 == value) return; _col5 = value; OnPropertyChanged("col5"); } }
        private string? _col6;
        public string? col6 { get => _col6; set { if (_col6 == value) return; _col6 = value; OnPropertyChanged("col6"); } }
        private int? _col7;
        public int? col7 { get => _col7; set { if (_col7 == value) return; _col7 = value; OnPropertyChanged("col7"); } }
        private int? _col8;
        public int? col8 { get => _col8; set { if (_col8 == value) return; _col8 = value; OnPropertyChanged("col8"); } }
        private int? _col9;
        public int? col9 { get => _col9; set { if (_col9 == value) return; _col9 = value; OnPropertyChanged("col9"); } }
        private string? _coln1;
        public string? coln1 { get => _coln1; set { if (_coln1 == value) return; _coln1 = value; OnPropertyChanged("coln1"); } }
        private string? _coln2;
        public string? coln2 { get => _coln2; set { if (_coln2 == value) return; _coln2 = value; OnPropertyChanged("coln2"); } }
        private string? _coln3;
        public string? coln3 { get => _coln3; set { if (_coln3 == value) return; _coln3 = value; OnPropertyChanged("coln3"); } }
        private string? _coln4;
        public string? coln4 { get => _coln4; set { if (_coln4 == value) return; _coln4 = value; OnPropertyChanged("coln4"); } }
        private string? _coln5;
        public string? coln5 { get => _coln5; set { if (_coln5 == value) return; _coln5 = value; OnPropertyChanged("coln5"); } }
        private string? _coln6;
        public string? coln6 { get => _coln6; set { if (_coln6 == value) return; _coln6 = value; OnPropertyChanged("coln6"); } }
        private string? _coln7;
        public string? coln7 { get => _coln7; set { if (_coln7 == value) return; _coln7 = value; OnPropertyChanged("coln7"); } }
        private string? _coln8;
        public string? coln8 { get => _coln8; set { if (_coln8 == value) return; _coln8 = value; OnPropertyChanged("coln8"); } }
        private string? _coln9;
        public string? coln9 { get => _coln9; set { if (_coln9 == value) return; _coln9 = value; OnPropertyChanged("coln9"); } }
        private string? _address;
        public string? ADDRESS { get => _address; set { if (_address == value) return; _address = value; OnPropertyChanged("ADDRESS"); } }
        private string? _tel;
        public string? TEL { get => _tel; set { if (_tel == value) return; _tel = value; OnPropertyChanged("TEL"); } }
        private string? _code_e;
        public string? CODE_E { get => _code_e; set { if (_code_e == value) return; _code_e = value; OnPropertyChanged("CODE_E"); } }
        private string? _mcodem;
        public string? MCODEM { get => _mcodem; set { if (_mcodem == value) return; _mcodem = value; OnPropertyChanged("MCODEM"); } }
        private string? _mobile;
        public string? MOBILE { get => _mobile; set { if (_mobile == value) return; _mobile = value; OnPropertyChanged("MOBILE"); } }
        private double? _longitude;
        public double? Longitude { get => _longitude; set { if (_longitude == value) return; _longitude = value; OnPropertyChanged("Longitude"); } }
        private double? _latitude;
        public double? Latitude { get => _latitude; set { if (_latitude == value) return; _latitude = value; OnPropertyChanged("Latitude"); } }
        private string? _route_name;
        public string? ROUTE_NAME { get => _route_name; set { if (_route_name == value) return; _route_name = value; OnPropertyChanged("ROUTE_NAME"); } }
        private int? _ostanid;
        public int? OSTANID { get => _ostanid; set { if (_ostanid == value) return; _ostanid = value; OnPropertyChanged("OSTANID"); } }
        private int? _shahrid;
        public int? SHAHRID { get => _shahrid; set { if (_shahrid == value) return; _shahrid = value; OnPropertyChanged("SHAHRID"); } }
        private string? _osname;
        public string? OSNAME { get => _osname; set { if (_osname == value) return; _osname = value; OnPropertyChanged("OSNAME"); } }
        private string? _cityname;
        public string? CITYNAME { get => _cityname; set { if (_cityname == value) return; _cityname = value; OnPropertyChanged("CITYNAME"); } }

        //پورسانتِ پشتِ فاکتور (ویو dbo.KALAS_PORSANT): پورسانت سطحِ فاکتور است و روی سطرهای
        //کالای همان فاکتور تکرار می‌شود؛ اگر فاکتور چند ویزیتور داشته باشد نام‌ها کنار هم و مبالغ جمع می‌آیند.
        private string? _prs_visitor;
        public string? PRS_VISITOR { get => _prs_visitor; set { if (_prs_visitor == value) return; _prs_visitor = value; OnPropertyChanged("PRS_VISITOR"); } }
        private string? _prs_visitor_name;
        public string? PRS_VISITOR_NAME { get => _prs_visitor_name; set { if (_prs_visitor_name == value) return; _prs_visitor_name = value; OnPropertyChanged("PRS_VISITOR_NAME"); } }
        private double? _prs_darsad;
        public double? PRS_DARSAD { get => _prs_darsad; set { if (_prs_darsad == value) return; _prs_darsad = value; OnPropertyChanged("PRS_DARSAD"); } }
        private double? _prs_pursant;
        public double? PRS_PURSANT { get => _prs_pursant; set { if (_prs_pursant == value) return; _prs_pursant = value; OnPropertyChanged("PRS_PURSANT"); } }
    }
}
