using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PRICE_ELAMIETF_DTL_MODEL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        public PRICE_ELAMIETF_DTL_MODEL()
        {
            TF1 = 0;
            TF2 = 0;
        }

        private int _peid;
        public int PEID { get => _peid; set { if (_peid == value) return; _peid = value; OnPropertyChanged("PEID"); } }
        private int _custcode;
        public int CUSTCODE { get => _custcode; set { if (_custcode == value) return; _custcode = value; OnPropertyChanged("CUSTCODE"); } }
        private int _ppid;
        public int PPID { get => _ppid; set { if (_ppid == value) return; _ppid = value; OnPropertyChanged("PPID"); } }
        private Single _tf1;
        public Single TF1 { get => _tf1; set { if (_tf1 == value) return; _tf1 = value; OnPropertyChanged("TF1"); } }
        private Single _tf2;
        public Single TF2 { get => _tf2; set { if (_tf2 == value) return; _tf2 = value; OnPropertyChanged("TF2"); } }
        private int _petid;
        public int PETID { get => _petid; set { if (_petid == value) return; _petid = value; OnPropertyChanged("PETID"); } }
        private DateTime _tr_date;
        public DateTime TR_DATE { get => _tr_date; set { if (_tr_date == value) return; _tr_date = value; OnPropertyChanged("TR_DATE"); } }
        private string _username;
        public string USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }

        private ObservableCollection<PRICE_ELAMIETF_EXCEPTION?> _SUB_DETAIL_EXP;
        public ObservableCollection<PRICE_ELAMIETF_EXCEPTION?> SUB_DETAIL_EXP
        {
            get => _SUB_DETAIL_EXP;
            set
            {
                if (_SUB_DETAIL_EXP == value) return;
                _SUB_DETAIL_EXP = value;
                OnPropertyChanged(nameof(SUB_DETAIL_EXP));
            }
        }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private PRICE_ELAMIETF_DTL_MODEL _backupCopy;
        private bool _inEdit;
        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (PRICE_ELAMIETF_DTL_MODEL)this.Clone();
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

            PEID = _backupCopy.PEID;
            SUB_DETAIL_EXP = _backupCopy.SUB_DETAIL_EXP;
            CUSTCODE = _backupCopy.CUSTCODE;
            PPID = _backupCopy.PPID;
            TF1 = _backupCopy.TF1;
            TF2 = _backupCopy.TF2;
            PETID = _backupCopy.PETID;
            TR_DATE = _backupCopy.TR_DATE;
            USERNAME = _backupCopy.USERNAME;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;

            _inEdit = false;
        }
    }
}
