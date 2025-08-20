using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_MAP_GRP : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged(nameof(ID)); } }

        private int? _mpp;
        public int? MPP { get => _mpp; set { if (_mpp == value) return; _mpp = value; OnPropertyChanged(nameof(MPP)); } }

        private string? _mpname;
        public string? MPNAME { get => _mpname; set { if (_mpname == value) return; _mpname = value; OnPropertyChanged(nameof(MPNAME)); } }

        private int? _sizef;
        public int? SIZEF { get => _sizef; set { if (_sizef == value) return; _sizef = value; OnPropertyChanged(nameof(SIZEF)); } }

        private int? _startf;
        public int? STARTF { get => _startf; set { if (_startf == value) return; _startf = value; OnPropertyChanged(nameof(STARTF)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        // --- Clone ---
        public object Clone() => this.MemberwiseClone();

        // --- IEditableObject backup handling ---
        private TCOD_MAP_GRP _backupCopy;
        private bool _inEdit;

        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (TCOD_MAP_GRP)this.Clone();
            _inEdit = true;
        }

        void IEditableObject.EndEdit()
        {
            _backupCopy = null;
            _inEdit = false;
        }

        void IEditableObject.CancelEdit()
        {
            if (!_inEdit) return;

            this.ID = _backupCopy.ID;
            this.MPP = _backupCopy.MPP;
            this.MPNAME = _backupCopy.MPNAME;
            this.SIZEF = _backupCopy.SIZEF;
            this.STARTF = _backupCopy.STARTF;
            this.CRT = _backupCopy.CRT;
            this.UID = _backupCopy.UID;

            _inEdit = false;
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
