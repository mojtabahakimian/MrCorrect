using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TOZIE : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _tid;
        public int? TID { get => _tid; set { if (_tid == value) return; _tid = value; OnPropertyChanged("TID"); } }
        private long? _tdate;
        public long? TDATE { get => _tdate; set { if (_tdate == value) return; _tdate = value; OnPropertyChanged("TDATE"); } }
        private string? _tdriver;
        public string? TDRIVER { get => _tdriver; set { if (_tdriver == value) return; _tdriver = value; OnPropertyChanged("TDRIVER"); } }
        private string? _tcity;
        public string? TCITY { get => _tcity; set { if (_tcity == value) return; _tcity = value; OnPropertyChanged("TCITY"); } }
        private string? _tmamur;
        public string? TMAMUR { get => _tmamur; set { if (_tmamur == value) return; _tmamur = value; OnPropertyChanged("TMAMUR"); } }
        private DateTime? _cdate;
        public DateTime? CDATE { get => _cdate; set { if (_cdate == value) return; _cdate = value; OnPropertyChanged("CDATE"); } }
        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private TOZIE _backupCopy;
        private bool _inEdit;
        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (TOZIE)this.Clone();
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
            TDATE = _backupCopy.TDATE;
            TDRIVER = _backupCopy.TDRIVER;
            TCITY = _backupCopy.TCITY;
            TMAMUR = _backupCopy.TMAMUR;
            CDATE = _backupCopy.CDATE;
            USER_NAME = _backupCopy.USER_NAME;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;

            _inEdit = false;
        }
    }
}
