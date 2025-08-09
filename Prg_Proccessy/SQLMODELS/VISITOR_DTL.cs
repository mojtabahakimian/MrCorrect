using System.ComponentModel;


namespace Prg_Proccessy.SQLMODELS
{
    public class VISITOR_DTL : INotifyPropertyChanged, IEditableObject
    {
        public VISITOR_DTL()
        {
            STAT = false;
            DARSAD = 0;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private bool _isInEdit;
        private VISITOR_DTL _backup;
        public void BeginEdit()
        {
            if (_isInEdit) return;
            _backup = (VISITOR_DTL)this.Clone(); // نسخهٔ پشتیبان
            _isInEdit = true;
        }
        public void CancelEdit()
        {
            if (!_isInEdit) return;

            // بازگرداندن مقادیر نسخهٔ پشتیبان
            ID = _backup.ID;
            NUMBER = _backup.NUMBER;
            TAG = _backup.TAG;
            CUST_NO = _backup.CUST_NO;
            CUST_NO_NAME = _backup.CUST_NO_NAME;
            DARSAD = _backup.DARSAD;
            PURSANT = _backup.PURSANT;
            TOZIH = _backup.TOZIH;
            STAT = _backup.STAT;
            PORID = _backup.PORID;
            CRT = _backup.CRT;
            UID = _backup.UID;
            LOG = _backup.LOG;

            _isInEdit = false;
            _backup = null;
        }
        public void EndEdit()
        {
            if (!_isInEdit) return;
            _isInEdit = false;
            _backup = null;  // تغییرات تثبیت شده‌اند
        }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }

        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }

        private string? _cust_no_name;
        public string? CUST_NO_NAME { get => _cust_no_name; set { if (_cust_no_name == value) return; _cust_no_name = value; OnPropertyChanged("CUST_NO_NAME"); } }

        private double? _darsad;
        public double? DARSAD { get => _darsad; set { if (_darsad == value) return; _darsad = value; OnPropertyChanged("DARSAD"); } }

        private double? _pursant;
        public double? PURSANT { get => _pursant; set { if (_pursant == value) return; _pursant = value; OnPropertyChanged("PURSANT"); } }

        private string? _tozih;
        public string? TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }

        private bool? _stat;
        public bool? STAT { get => _stat; set { if (_stat == value) return; _stat = value; OnPropertyChanged("STAT"); } }

        private int? _porid;
        public int? PORID { get => _porid; set { if (_porid == value) return; _porid = value; OnPropertyChanged("PORID"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private string? _log;
        public string? LOG { get => _log; set { if (_log == value) return; _log = value; OnPropertyChanged("LOG"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }
    }
}