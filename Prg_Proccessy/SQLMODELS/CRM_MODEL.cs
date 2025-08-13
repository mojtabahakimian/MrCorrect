using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class COPMANES : INotifyPropertyChanged, ICloneable
    {
        public object Clone() => this.MemberwiseClone();
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int? _id;
        public int? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(ID)); } }

        private string? _companyName;
        public string? COMPANY_NAME { get => _companyName; set { if (_companyName == value) return; _companyName = value; OnPropertyChanged(nameof(COMPANY_NAME)); } }

        private string? _city;
        public string? CITY { get => _city; set { if (_city == value) return; _city = value; OnPropertyChanged(nameof(CITY)); } }

        private string? _manager;
        public string? MANAGER { get => _manager; set { if (_manager == value) return; _manager = value; OnPropertyChanged(nameof(MANAGER)); } }

        private string? _factTel;
        public string? FACT_TEL { get => _factTel; set { if (_factTel == value) return; _factTel = value; OnPropertyChanged(nameof(FACT_TEL)); } }

        private string? _mobile;
        public string? MOBILE { get => _mobile; set { if (_mobile == value) return; _mobile = value; OnPropertyChanged(nameof(MOBILE)); } }

        private int? _pernum;
        public int? PERNUM { get => _pernum; set { if (_pernum == value) return; _pernum = value; OnPropertyChanged(nameof(PERNUM)); } }

        private string? _statusFact;
        public string? STATUS_FACT { get => _statusFact; set { if (_statusFact == value) return; _statusFact = value; OnPropertyChanged(nameof(STATUS_FACT)); } }

        private string? _products;
        public string? PRODUCTS { get => _products; set { if (_products == value) return; _products = value; OnPropertyChanged(nameof(PRODUCTS)); } }

        private string? _addr;
        public string? ADDR { get => _addr; set { if (_addr == value) return; _addr = value; OnPropertyChanged(nameof(ADDR)); } }

        private string? _accountant;
        public string? ACCOUNTANT { get => _accountant; set { if (_accountant == value) return; _accountant = value; OnPropertyChanged(nameof(ACCOUNTANT)); } }

        private string? _software;
        public string? SOFTWARE { get => _software; set { if (_software == value) return; _software = value; OnPropertyChanged(nameof(SOFTWARE)); } }

        private string? _espPerson;
        public string? ESP_PERSON { get => _espPerson; set { if (_espPerson == value) return; _espPerson = value; OnPropertyChanged(nameof(ESP_PERSON)); } }

        private string? _reagent;
        public string? REAGENT { get => _reagent; set { if (_reagent == value) return; _reagent = value; OnPropertyChanged(nameof(REAGENT)); } }

        private int? _status;
        public int? STATUS { get => _status; set { if (_status == value) return; _status = value; OnPropertyChanged(nameof(STATUS)); } }

        private string? _comment;
        public string? COMMENT { get => _comment; set { if (_comment == value) return; _comment = value; OnPropertyChanged(nameof(COMMENT)); } }

        private DateTime? _dateSabt;
        public DateTime? DATE_SABT { get => _dateSabt; set { if (_dateSabt == value) return; _dateSabt = value; OnPropertyChanged(nameof(DATE_SABT)); } }

        private string? _userName;
        public string? USER_NAME { get => _userName; set { if (_userName == value) return; _userName = value; OnPropertyChanged(nameof(USER_NAME)); } }

        private byte[]? _pic;
        public byte[]? PIC { get => _pic; set { if (_pic == value) return; _pic = value; OnPropertyChanged(nameof(PIC)); } }

        private int? _dt;
        public int? DT { get => _dt; set { if (_dt == value) return; _dt = value; OnPropertyChanged(nameof(DT)); } }

        private int? _userid;
        public int? USERID { get => _userid; set { if (_userid == value) return; _userid = value; OnPropertyChanged(nameof(USERID)); } }

        private double? _longitude;
        public double? LONGITUDE { get => _longitude; set { if (_longitude == value) return; _longitude = value; OnPropertyChanged(nameof(LONGITUDE)); } }

        private double? _latitude;
        public double? LATITUDE { get => _latitude; set { if (_latitude == value) return; _latitude = value; OnPropertyChanged(nameof(LATITUDE)); } }

        private int? _ostanid;
        public int? OSTANID { get => _ostanid; set { if (_ostanid == value) return; _ostanid = value; OnPropertyChanged(nameof(OSTANID)); } }

        private int? _shahrid;
        public int? SHAHRID { get => _shahrid; set { if (_shahrid == value) return; _shahrid = value; OnPropertyChanged(nameof(SHAHRID)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        private int? _idcn;
        public int? IDCN { get => _idcn; set { if (_idcn == value) return; _idcn = value; OnPropertyChanged(nameof(IDCN)); } }
    }
    public class CRMEVENTS : INotifyPropertyChanged, ICloneable
    {
        public object Clone() => this.MemberwiseClone();
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int? _idde;
        public int? IDDE { get => _idde; set { if (_idde == value) return; _idde = value; OnPropertyChanged(nameof(IDDE)); } }

        private string? _companyName;
        public string? COMPANY_NAME { get => _companyName; set { if (_companyName == value) return; _companyName = value; OnPropertyChanged(nameof(COMPANY_NAME)); } }

        private int? _infoDate;
        public int? INFO_DATE { get => _infoDate; set { if (_infoDate == value) return; _infoDate = value; OnPropertyChanged(nameof(INFO_DATE)); } }

        private int? _infoTime;
        public int? INFO_TIME { get => _infoTime; set { if (_infoTime == value) return; _infoTime = value; OnPropertyChanged(nameof(INFO_TIME)); } }

        private string? _saler;
        public string? SALER { get => _saler; set { if (_saler == value) return; _saler = value; OnPropertyChanged(nameof(SALER)); } }

        private string? _buyer;
        public string? BUYER { get => _buyer; set { if (_buyer == value) return; _buyer = value; OnPropertyChanged(nameof(BUYER)); } }

        private string? _comment;
        public string? COMMENT { get => _comment; set { if (_comment == value) return; _comment = value; OnPropertyChanged(nameof(COMMENT)); } }

        private int? _nextDate;
        public int? NEXT_DATE { get => _nextDate; set { if (_nextDate == value) return; _nextDate = value; OnPropertyChanged(nameof(NEXT_DATE)); } }

        private int? _nextTime;
        public int? NEXT_TIME { get => _nextTime; set { if (_nextTime == value) return; _nextTime = value; OnPropertyChanged(nameof(NEXT_TIME)); } }

        private int? _status;
        public int? STATUS { get => _status; set { if (_status == value) return; _status = value; OnPropertyChanged(nameof(STATUS)); } }

        private byte[]? _pic;
        public byte[]? PIC { get => _pic; set { if (_pic == value) return; _pic = value; OnPropertyChanged(nameof(PIC)); } }

        private int? _idc;
        public int? IDC { get => _idc; set { if (_idc == value) return; _idc = value; OnPropertyChanged(nameof(IDC)); } }

        private string? _payam;
        public string? PAYAM { get => _payam; set { if (_payam == value) return; _payam = value; OnPropertyChanged(nameof(PAYAM)); } }

        private int? _miting;
        public int? MITING { get => _miting; set { if (_miting == value) return; _miting = value; OnPropertyChanged(nameof(MITING)); } }

        private int? _userid;
        public int? USERID { get => _userid; set { if (_userid == value) return; _userid = value; OnPropertyChanged(nameof(USERID)); } }

        private DateTime? _cdateti;
        public DateTime? CDATETI { get => _cdateti; set { if (_cdateti == value) return; _cdateti = value; OnPropertyChanged(nameof(CDATETI)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }
    }
}
