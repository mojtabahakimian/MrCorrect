using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_ZARMALIAT_MODEL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        // Event for property change notifications
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Creates a shallow copy of the object
        public object Clone() => this.MemberwiseClone();

        #region Fields
        private int _mabl_h;
        private double _zarib;
        private int _maliat;
        private DateTime? _crt;
        private int? _uid;
        #endregion

        #region Properties
        public int MABL_H
        {
            get => _mabl_h;
            set { if (_mabl_h == value) return; _mabl_h = value; OnPropertyChanged(nameof(MABL_H)); }
        }

        public double ZARIB
        {
            get => _zarib;
            set { if (_zarib == value) return; _zarib = value; OnPropertyChanged(nameof(ZARIB)); }
        }

        public int MALIAT
        {
            get => _maliat;
            set { if (_maliat == value) return; _maliat = value; OnPropertyChanged(nameof(MALIAT)); }
        }

        public DateTime? CRT
        {
            get => _crt;
            set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); }
        }

        public int? UID
        {
            get => _uid;
            set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); }
        }
        #endregion

        #region IEditableObject Implementation
        private TCOD_ZARMALIAT_MODEL _backupCopy;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _inEdit = true;
            _backupCopy = this.Clone() as TCOD_ZARMALIAT_MODEL;
        }

        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            this.MABL_H = _backupCopy.MABL_H;
            this.ZARIB = _backupCopy.ZARIB;
            this.MALIAT = _backupCopy.MALIAT;
            this.CRT = _backupCopy.CRT;
            this.UID = _backupCopy.UID;

            _inEdit = false;
            _backupCopy = null;
        }

        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null; // Commit changes by discarding the backup
        }
        #endregion
    }
}
