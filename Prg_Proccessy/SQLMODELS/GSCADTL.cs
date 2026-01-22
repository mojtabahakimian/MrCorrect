using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class GSCADTL : INotifyPropertyChanged, IEditableObject, ICloneable
    {
        public object Clone() => this.MemberwiseClone();

        private bool _isInEdit;
        private GSCADTL _backup;

        public void BeginEdit()
        {
            if (_isInEdit) return;
            _backup = (GSCADTL)this.Clone();
            _isInEdit = true;
        }

        public void CancelEdit()
        {
            if (!_isInEdit) return;
            GSCADTCOD = _backup.GSCADTCOD;
            GSCANAME = _backup.GSCANAME;
            GSCAGRADE = _backup.GSCAGRADE;
            GSCAFROM = _backup.GSCAFROM;
            GSCATO = _backup.GSCATO;
            GSCACOD = _backup.GSCACOD;
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

        private int _gscadtcod;
        public int GSCADTCOD
        {
            get => _gscadtcod;
            set
            {
                if (Equals(_gscadtcod, value)) return;
                _gscadtcod = value;
                OnPropertyChanged(nameof(GSCADTCOD));
            }
        }

        private string _gscaname;
        public string GSCANAME
        {
            get => _gscaname;
            set
            {
                if (Equals(_gscaname, value)) return;
                _gscaname = value;
                OnPropertyChanged(nameof(GSCANAME));
            }
        }

        private float _gscagrade;
        public float GSCAGRADE
        {
            get => _gscagrade;
            set
            {
                if (Equals(_gscagrade, value)) return;
                _gscagrade = value;
                OnPropertyChanged(nameof(GSCAGRADE));
            }
        }

        private float _gscafrom;
        public float GSCAFROM
        {
            get => _gscafrom;
            set
            {
                if (Equals(_gscafrom, value)) return;
                _gscafrom = value;
                OnPropertyChanged(nameof(GSCAFROM));
            }
        }

        private float _gscato;
        public float GSCATO
        {
            get => _gscato;
            set
            {
                if (Equals(_gscato, value)) return;
                _gscato = value;
                OnPropertyChanged(nameof(GSCATO));
            }
        }

        private int _gscacod;
        public int GSCACOD
        {
            get => _gscacod;
            set
            {
                if (Equals(_gscacod, value)) return;
                _gscacod = value;
                OnPropertyChanged(nameof(GSCACOD));
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
