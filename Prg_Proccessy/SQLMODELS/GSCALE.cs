using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class GSCALE : INotifyPropertyChanged, IEditableObject, ICloneable
    {
        public object Clone() => this.MemberwiseClone();

        private bool _isInEdit;
        private GSCALE _backup;

        public void BeginEdit()
        {
            if (_isInEdit) return;
            _backup = (GSCALE)this.Clone();
            _isInEdit = true;
        }

        public void CancelEdit()
        {
            if (!_isInEdit) return;
            GSCACOD = _backup.GSCACOD;
            GSCANAME = _backup.GSCANAME;
            GSCAKIND = _backup.GSCAKIND;
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

        private float _gscakind;
        public float GSCAKIND
        {
            get => _gscakind;
            set
            {
                if (Equals(_gscakind, value)) return;
                _gscakind = value;
                OnPropertyChanged(nameof(GSCAKIND));
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
