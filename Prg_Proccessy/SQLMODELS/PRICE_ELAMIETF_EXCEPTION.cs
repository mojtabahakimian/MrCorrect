using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// کالای مشمول استثنای تخفیف
    /// </summary>
    public class PRICE_ELAMIETF_EXCEPTION : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int _exception_id;
        public int EXCEPTION_ID { get => _exception_id; set { if (_exception_id == value) return; _exception_id = value; OnPropertyChanged("EXCEPTION_ID"); } }
        private int _petid;
        public int PETID { get => _petid; set { if (_petid == value) return; _petid = value; OnPropertyChanged("PETID"); } }
        private string _code;
        public string CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }
        private Single _exception_tf1;
        public Single EXCEPTION_TF1 { get => _exception_tf1; set { if (_exception_tf1 == value) return; _exception_tf1 = value; OnPropertyChanged("EXCEPTION_TF1"); } }
        private Single _exception_tf2;
        public Single EXCEPTION_TF2 { get => _exception_tf2; set { if (_exception_tf2 == value) return; _exception_tf2 = value; OnPropertyChanged("EXCEPTION_TF2"); } }
        private DateTime _tr_date;
        public DateTime TR_DATE { get => _tr_date; set { if (_tr_date == value) return; _tr_date = value; OnPropertyChanged("TR_DATE"); } }
        private string _username;
        public string USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private PRICE_ELAMIETF_EXCEPTION _backupCopy;
        private bool _inEdit;
        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (PRICE_ELAMIETF_EXCEPTION)this.Clone();
            _inEdit = true;
        }
        void IEditableObject.EndEdit()
        {
            _backupCopy = null;
            _inEdit = false;
        }
        void IEditableObject.CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            EXCEPTION_ID = _backupCopy.EXCEPTION_ID;
            PETID = _backupCopy.PETID;
            CODE = _backupCopy.CODE;
            EXCEPTION_TF1 = _backupCopy.EXCEPTION_TF1;
            EXCEPTION_TF2 = _backupCopy.EXCEPTION_TF2;
            TR_DATE = _backupCopy.TR_DATE;
            USERNAME = _backupCopy.USERNAME;
            CRT = _backupCopy.CRT;

            _inEdit = false;
        }
    }
}
