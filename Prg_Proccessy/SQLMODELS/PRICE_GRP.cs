using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PRICE_GRP : INotifyPropertyChanged, IEditableObject, ICloneable
    {
        public object Clone() => this.MemberwiseClone();

        private bool _isInEdit;
        private PRICE_GRP _backup;

        public void BeginEdit()
        {
            if (_isInEdit) return;
            _backup = (PRICE_GRP)this.Clone();
            _isInEdit = true;
        }

        public void CancelEdit()
        {
            if (!_isInEdit) return;
            PGID = _backup.PGID;
            PGNAME = _backup.PGNAME;
            TR_DATE = _backup.TR_DATE;
            USERNAME = _backup.USERNAME;
            CRT = _backup.CRT;
            UID = _backup.UID;
            _isInEdit = false;
            _backup = null;
        }

        public void EndEdit()
        {
            if (!_isInEdit) return;
            _isInEdit = false;
            _backup = null;
        }

        private int _pgid;
        public int PGID
        {
            get => _pgid;
            set
            {
                if (Equals(_pgid, value)) return;
                _pgid = value;
                OnPropertyChanged(nameof(PGID));
            }
        }

        private string _pgname;
        public string PGNAME
        {
            get => _pgname;
            set
            {
                if (Equals(_pgname, value)) return;
                _pgname = value;
                OnPropertyChanged(nameof(PGNAME));
            }
        }

        private DateTime _tr_date;
        public DateTime TR_DATE
        {
            get => _tr_date;
            set
            {
                if (Equals(_tr_date, value)) return;
                _tr_date = value;
                OnPropertyChanged(nameof(TR_DATE));
            }
        }

        private string _username;
        public string USERNAME
        {
            get => _username;
            set
            {
                if (Equals(_username, value)) return;
                _username = value;
                OnPropertyChanged(nameof(USERNAME));
            }
        }

        private DateTime? _crt;
        public DateTime? CRT
        {
            get => _crt;
            set
            {
                if (Equals(_crt, value)) return;
                _crt = value;
                OnPropertyChanged(nameof(CRT));
            }
        }

        private int? _uid;
        public int? UID
        {
            get => _uid;
            set
            {
                if (Equals(_uid, value)) return;
                _uid = value;
                OnPropertyChanged(nameof(UID));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
