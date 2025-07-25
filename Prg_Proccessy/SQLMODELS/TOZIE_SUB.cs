using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TOZIE_SUB : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _tid;
        public int? TID { get => _tid; set { if (_tid == value) return; _tid = value; OnPropertyChanged("TID"); } }
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private DateTime? _cdate;
        public DateTime? CDATE { get => _cdate; set { if (_cdate == value) return; _cdate = value; OnPropertyChanged("CDATE"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private string? _name_hes;
        public string? NAME_HES
        {
            get => _name_hes;
            set
            {
                if (_name_hes == value) return;
                _name_hes = value;
                OnPropertyChanged(nameof(NAME_HES));
            }
        }

        private long? _date_n;
        public long? DATE_N { get => _date_n; set { if (_date_n == value) return; _date_n = value; OnPropertyChanged("DATE_N"); } }
        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }

        private TOZIE_SUB _backupCopy;
        private bool _inEdit;
        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (TOZIE_SUB)this.Clone();
            _inEdit = true;
        }
        void IEditableObject.EndEdit()
        {
            // commit: just forget the backup
            _backupCopy = null;
            _inEdit = false;
        }
        void IEditableObject.CancelEdit()
        {
            if (!_inEdit) return;
            // restore all of your properties from the backup
            TID = _backupCopy.TID;
            NUMBER = _backupCopy.NUMBER;
            CDATE = _backupCopy.CDATE;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;
            CUST_NO = _backupCopy.CUST_NO;
            NAME_HES = _backupCopy.NAME_HES;
            DATE_N = _backupCopy.DATE_N;

            _inEdit = false;
        }
    }
}
