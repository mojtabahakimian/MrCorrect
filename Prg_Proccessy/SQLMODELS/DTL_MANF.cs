using Prg_Proccessy.MODELS;
using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Prg_Proccessy.SQLMODELS
{
    public class DTL_MANF : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public object Clone() => this.MemberwiseClone();

        private int _fumb;
        private string _code;
        private int _anbar;
        private int _vahed_k;
        private double _megh;
        private double _meghk;
        private double _pert;
        private double? _smabl;
        private double? _mablk;
        private string _tozih;
        private DateTime? _crt;
        private int? _uid;
        private int? _ID;

        // An extra property that was in your refactoring request to hold the item's name from a JOIN
        private string _name_code;

        public DTL_MANF()
        {
            ANBAR = Baseknow.anbardef;
            MEGH = 0;
            MEGHk = 0;
            PERT = 0;
            SMABL = 0;
            MABLK = 0;
        }

        public int FNUMB
        {
            get => _fumb;
            set { if (_fumb == value) return; _fumb = value; OnPropertyChanged(nameof(FNUMB)); }
        }

        public string CODE
        {
            get => _code;
            set { if (_code == value) return; _code = value; OnPropertyChanged(nameof(CODE)); }
        }

        public int ANBAR
        {
            get => _anbar;
            set { if (_anbar == value) return; _anbar = value; OnPropertyChanged(nameof(ANBAR)); }
        }

        public int VAHED_K
        {
            get => _vahed_k;
            set { if (_vahed_k == value) return; _vahed_k = value; OnPropertyChanged(nameof(VAHED_K)); }
        }

        public double MEGH
        {
            get => _megh;
            set { if (_megh == value) return; _megh = value; OnPropertyChanged(nameof(MEGH)); }
        }

        public double MEGHk
        {
            get => _meghk;
            set { if (_meghk == value) return; _meghk = value; OnPropertyChanged(nameof(MEGHk)); }
        }

        public double PERT
        {
            get => _pert;
            set { if (_pert == value) return; _pert = value; OnPropertyChanged(nameof(PERT)); }
        }

        public double? SMABL
        {
            get => _smabl;
            set { if (_smabl == value) return; _smabl = value; OnPropertyChanged(nameof(SMABL)); }
        }

        public double? MABLK
        {
            get => _mablk;
            set { if (_mablk == value) return; _mablk = value; OnPropertyChanged(nameof(MABLK)); }
        }

        public string TOZIH
        {
            get => _tozih;
            set { if (_tozih == value) return; _tozih = value; OnPropertyChanged(nameof(TOZIH)); }
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


        public int? ID
        {
            get => _ID;
            set { if (_ID == value) return; _ID = value; OnPropertyChanged(nameof(ID)); }
        }


        public string NAME_CODE
        {
            get => _name_code;
            set { if (_name_code == value) return; _name_code = value; OnPropertyChanged(nameof(NAME_CODE)); }
        }


        // =================================================================================
        // 5. IEditableObject Implementation
        // Manages transactional edits (Begin, End, Cancel).
        // Essential for DataGrid row editing functionality.
        // =================================================================================
        private DTL_MANF _backupCopy;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (DTL_MANF)this.Clone();
            _inEdit = true;
        }
        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            // Restore all properties from the backup copy
            this.FNUMB = _backupCopy.FNUMB;
            this.CODE = _backupCopy.CODE;
            this.ANBAR = _backupCopy.ANBAR;
            this.VAHED_K = _backupCopy.VAHED_K;
            this.MEGH = _backupCopy.MEGH;
            this.MEGHk = _backupCopy.MEGHk;
            this.PERT = _backupCopy.PERT;
            this.SMABL = _backupCopy.SMABL;
            this.MABLK = _backupCopy.MABLK;
            this.TOZIH = _backupCopy.TOZIH;
            this.CRT = _backupCopy.CRT;
            this.UID = _backupCopy.UID;
            this._ID = _backupCopy._ID;
            this.NAME_CODE = _backupCopy.NAME_CODE; // Also restore non-table properties if they can be edited

            _inEdit = false;
            _backupCopy = null;
        }
        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null; // Discard the backup copy, committing the changes
        }
    }
}
