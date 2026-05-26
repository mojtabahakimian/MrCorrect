using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PGET_LST : INotifyPropertyChanged, ICloneable, IDataErrorInfo
    {
        public PGET_LST()
        {
            ARZD = 1;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(MABL))
                {
                    // if MABL is null or is an invalid value, return the error string
                    if (MABL == null)
                    {
                        return "مقدار مبلغ نباید خالی باشد.";
                    }
                }
                return null;
            }
        }

        // Return any model-level error here (often left empty)
        public string Error => null;

        private bool _HasAttachment;
        public bool HasAttachment { get => _HasAttachment; set { if (_HasAttachment == value) return; _HasAttachment = value; OnPropertyChanged("HasAttachment"); } }

        private double? _mabl;
        public double? MABL
        {
            get => _mabl; 
            set
            {
                if (_mabl == value) return; _mabl = value;
                OnPropertyChanged("MABL");
            }
        }

        private int? _id;
        public int? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private long? _date;
        public long? DATE { get => _date; set { if (_date == value) return; _date = value; OnPropertyChanged("DATE"); } }

        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

        private int? _no_am;
        public int? NO_AM { get => _no_am; set { if (_no_am == value) return; _no_am = value; OnPropertyChanged("NO_AM"); } }

        private double? _nahva;
        public double? NAHVA { get => _nahva; set { if (_nahva == value) return; _nahva = value; OnPropertyChanged("NAHVA"); } }

        private int? _fhes_k;
        public int? FHES_K { get => _fhes_k; set { if (_fhes_k == value) return; _fhes_k = value; OnPropertyChanged("FHES_K"); } }

        private int? _fhes_m;
        public int? FHES_M { get => _fhes_m; set { if (_fhes_m == value) return; _fhes_m = value; OnPropertyChanged("FHES_M"); } }

        private int? _fhes_t;
        public int? FHES_T { get => _fhes_t; set { if (_fhes_t == value) return; _fhes_t = value; OnPropertyChanged("FHES_T"); } }

        private int? _thes_k;
        public int? THES_K { get => _thes_k; set { if (_thes_k == value) return; _thes_k = value; OnPropertyChanged("THES_K"); } }

        private int? _thes_m;
        public int? THES_M { get => _thes_m; set { if (_thes_m == value) return; _thes_m = value; OnPropertyChanged("THES_M"); } }

        private int? _thes_t;
        public int? THES_T { get => _thes_t; set { if (_thes_t == value) return; _thes_t = value; OnPropertyChanged("THES_T"); } }

        private string? _sharh;
        public string? SHARH { get => _sharh; set { if (_sharh == value) return; _sharh = value; OnPropertyChanged("SHARH"); } }



        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }

        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }

        private int? _mhaz_no;
        public int? MHAZ_NO { get => _mhaz_no; set { if (_mhaz_no == value) return; _mhaz_no = value; OnPropertyChanged("MHAZ_NO"); } }

        private int? _idh;
        public int? IDH { get => _idh; set { if (_idh == value) return; _idh = value; OnPropertyChanged("IDH"); } }

        private string? _fhes;
        public string? FHES { get => _fhes; set { if (_fhes == value) return; _fhes = value; OnPropertyChanged("FHES"); } }

        private string? _thes;
        public string? THES { get => _thes; set { if (_thes == value) return; _thes = value; OnPropertyChanged("THES"); } }


        private string? _name_fhes;
        public string? NAME_FHES { get => _name_fhes; set { if (_name_fhes == value) return; _name_fhes = value; OnPropertyChanged("NAME_FHES"); } }

        private string? _name_thes;
        public string? NAME_THES { get => _name_thes; set { if (_name_thes == value) return; _name_thes = value; OnPropertyChanged("NAME_THES"); } }

        private double? _arzd;
        public double? ARZD { get => _arzd; set { if (_arzd == value) return; _arzd = value; OnPropertyChanged("ARZD"); } }

        private int? _fhes_t2;
        public int? FHES_T2 { get => _fhes_t2; set { if (_fhes_t2 == value) return; _fhes_t2 = value; OnPropertyChanged("FHES_T2"); } }

        private int? _thes_t2;
        public int? THES_T2 { get => _thes_t2; set { if (_thes_t2 == value) return; _thes_t2 = value; OnPropertyChanged("THES_T2"); } }

        private int? _fhes_t3;
        public int? FHES_T3 { get => _fhes_t3; set { if (_fhes_t3 == value) return; _fhes_t3 = value; OnPropertyChanged("FHES_T3"); } }

        private int? _thes_t3;
        public int? THES_T3 { get => _thes_t3; set { if (_thes_t3 == value) return; _thes_t3 = value; OnPropertyChanged("THES_T3"); } }

        private int? _fhes_t4;
        public int? FHES_T4 { get => _fhes_t4; set { if (_fhes_t4 == value) return; _fhes_t4 = value; OnPropertyChanged("FHES_T4"); } }

        private int? _thes_t4;
        public int? THES_T4 { get => _thes_t4; set { if (_thes_t4 == value) return; _thes_t4 = value; OnPropertyChanged("THES_T4"); } }

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