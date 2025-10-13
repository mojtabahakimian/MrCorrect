using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class DEED_DTL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        private DEED_DTL _backupCopy;
        private bool _inEdit;

        #region IEditableObject Implementation
        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (DEED_DTL)this.Clone();
            _inEdit = true;
        }
        public void EndEdit()
        {
            if (!_inEdit) return;
            _backupCopy = null;
            _inEdit = false;
        }
        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            N_S = _backupCopy.N_S;
            RADIF = _backupCopy.RADIF;
            HES_K = _backupCopy.HES_K;
            NAME_HES = _backupCopy.NAME_HES;
            HES_M = _backupCopy.HES_M;
            HES_T = _backupCopy.HES_T;
            SHARH = _backupCopy.SHARH;
            BED = _backupCopy.BED;
            BES = _backupCopy.BES;
            N_SERI = _backupCopy.N_SERI;
            BANK = _backupCopy.BANK;
            NUMBER = _backupCopy.NUMBER;
            TAG = _backupCopy.TAG;
            HES = _backupCopy.HES;
            id = _backupCopy.id;
            ARZD = _backupCopy.ARZD;
            MHAZ_NO = _backupCopy.MHAZ_NO;
            HES_T2 = _backupCopy.HES_T2;
            HES_T3 = _backupCopy.HES_T3;
            HES_T4 = _backupCopy.HES_T4;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;
            SBED = _backupCopy.SBED;
            SBES = _backupCopy.SBES;

            _inEdit = false;
            _backupCopy = null;
        }
        #endregion

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public DEED_DTL()
        {
            BED = 0;
            BES = 0;
        }

        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }

        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

        private int? _hes_k;
        public int? HES_K { get => _hes_k; set { if (_hes_k == value) return; _hes_k = value; OnPropertyChanged("HES_K"); } }

        private string? _NAME_HES;
        public string? NAME_HES { get => _NAME_HES; set { if (_NAME_HES == value) return; _NAME_HES = value; OnPropertyChanged("NAME_HES"); } }

        private int? _hes_m;
        public int? HES_M { get => _hes_m; set { if (_hes_m == value) return; _hes_m = value; OnPropertyChanged("HES_M"); } }

        private int? _hes_t;
        public int? HES_T { get => _hes_t; set { if (_hes_t == value) return; _hes_t = value; OnPropertyChanged("HES_T"); } }

        private string? _sharh;
        public string? SHARH { get => _sharh; set { if (_sharh == value) return; _sharh = value; OnPropertyChanged("SHARH"); } }

        private double? _bed;
        public double? BED { get => _bed; set { if (_bed == value) return; _bed = value; OnPropertyChanged("BED"); } }

        private double? _bes;
        public double? BES { get => _bes; set { if (_bes == value) return; _bes = value; OnPropertyChanged("BES"); } }

        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }

        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }

        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }

        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }

        private long? _id;
        public long? id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("id"); } }

        private double? _arzd;
        public double? ARZD { get => _arzd; set { if (_arzd == value) return; _arzd = value; OnPropertyChanged("ARZD"); } }

        private int? _mhaz_no;
        public int? MHAZ_NO { get => _mhaz_no; set { if (_mhaz_no == value) return; _mhaz_no = value; OnPropertyChanged("MHAZ_NO"); } }

        private int? _hes_t2;
        public int? HES_T2 { get => _hes_t2; set { if (_hes_t2 == value) return; _hes_t2 = value; OnPropertyChanged("HES_T2"); } }

        private int? _hes_t3;
        public int? HES_T3 { get => _hes_t3; set { if (_hes_t3 == value) return; _hes_t3 = value; OnPropertyChanged("HES_T3"); } }

        private int? _hes_t4;
        public int? HES_T4 { get => _hes_t4; set { if (_hes_t4 == value) return; _hes_t4 = value; OnPropertyChanged("HES_T4"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private string? _SBED;
        public string? SBED { get => _SBED; set { if (_SBED == value) return; _SBED = value; OnPropertyChanged("SBED"); } }


        private string? _SBES;
        public string? SBES { get => _SBES; set { if (_SBES == value) return; _SBES = value; OnPropertyChanged("SBES"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }

    }
}
