using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_MAP : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged(nameof(ID)); } }

        private int? _mpp;
        public int? MPP { get => _mpp; set { if (_mpp == value) return; _mpp = value; OnPropertyChanged(nameof(MPP)); } }

        private int? _mpcode;
        public int? MPCODE { get => _mpcode; set { if (_mpcode == value) return; _mpcode = value; OnPropertyChanged(nameof(MPCODE)); } }

        private string? _mpname;
        public string? MPNAME { get => _mpname; set { if (_mpname == value) return; _mpname = value; OnPropertyChanged(nameof(MPNAME)); } }

        private int? _osco;
        public int? OSCO { get => _osco; set { if (_osco == value) return; _osco = value; OnPropertyChanged(nameof(OSCO)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        // --- Clone ---
        public object Clone() => this.MemberwiseClone();

        // --- IEditableObject backup handling ---
        private TCOD_MAP _backupCopy;
        private bool _inEdit;

        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (TCOD_MAP)this.Clone();
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
            this.MPCODE = _backupCopy.MPCODE;
            this.MPNAME = _backupCopy.MPNAME;
            this.OSCO = _backupCopy.OSCO;
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
