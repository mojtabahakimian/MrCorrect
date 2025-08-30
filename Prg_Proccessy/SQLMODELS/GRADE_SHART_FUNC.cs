using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_SHART_FUNC : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private int _gshartid;
        public int GSHARTID
        {
            get => _gshartid;
            set
            {
                if (_gshartid == value) return;
                _gshartid = value;
                OnPropertyChanged(nameof(GSHARTID));
            }
        }

        private string? _gshname;
        public string? GSHNAME
        {
            get => _gshname;
            set
            {
                if (_gshname == value) return;
                _gshname = value;
                OnPropertyChanged(nameof(GSHNAME));
            }
        }

        private string? _gshfunc;
        public string? GSHFUNC
        {
            get => _gshfunc;
            set
            {
                if (_gshfunc == value) return;
                _gshfunc = value;
                OnPropertyChanged(nameof(GSHFUNC));
            }
        }

        private string? _tozih;
        public string? TOZIH
        {
            get => _tozih;
            set
            {
                if (_tozih == value) return;
                _tozih = value;
                OnPropertyChanged(nameof(TOZIH));
            }
        }

        // CRT and UID are handled by the database or system
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }

        // ------------------- IEditableObject -------------------
        private GRADE_SHART_FUNC? _backupCopy;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (GRADE_SHART_FUNC)this.Clone();
            _inEdit = true;
        }

        public void CancelEdit()
        {
            if (!_inEdit) return;

            if (_backupCopy != null)
            {
                this.GSHARTID = _backupCopy.GSHARTID;
                this.GSHNAME = _backupCopy.GSHNAME;
                this.GSHFUNC = _backupCopy.GSHFUNC;
                this.TOZIH = _backupCopy.TOZIH;
                this.CRT = _backupCopy.CRT;
                this.UID = _backupCopy.UID;
            }
            _inEdit = false;
            _backupCopy = null;
        }

        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null; // discard backup (commit changes)
        }
    }

}
