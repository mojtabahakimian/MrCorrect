using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISITORS_PORSANT_KALA_DTO : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        // =================================================================================
        // 1. INotifyPropertyChanged Implementation
        // =================================================================================
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // =================================================================================
        // 2. ICloneable Implementation
        // =================================================================================
        public object Clone() => this.MemberwiseClone();

        // =================================================================================
        // 3. Backing Fields
        // =================================================================================
        private int? _porid;
        private string? _code;
        private double? _porsant;
        private string? _name;
        private string? _vahed;

        // =================================================================================
        // 4. Public Properties
        // =================================================================================

        public int? PORID
        {
            get => _porid;
            set
            {
                if (_porid == value) return;
                _porid = value;
                OnPropertyChanged(nameof(PORID));
            }
        }

        public string? CODE
        {
            get => _code;
            set
            {
                if (_code == value) return;
                _code = value;
                OnPropertyChanged(nameof(CODE));
            }
        }

        public double? PORSANT
        {
            get => _porsant;
            set
            {
                if (_porsant == value) return;
                _porsant = value;
                OnPropertyChanged(nameof(PORSANT));
            }
        }

        // Read-only logic usually applies to joined columns like Name, 
        // but setters are kept here in case you need to populate them from code.
        public string? NAME
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(NAME));
            }
        }

        public string? VAHED
        {
            get => _vahed;
            set
            {
                if (_vahed == value) return;
                _vahed = value;
                OnPropertyChanged(nameof(VAHED));
            }
        }

        // =================================================================================
        // 5. IEditableObject Implementation
        // Manages transactional edits for the DataGrid
        // =================================================================================
        private VISITORS_PORSANT_KALA_DTO _backupCopy;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (VISITORS_PORSANT_KALA_DTO)this.Clone();
            _inEdit = true;
        }

        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            // Restore all properties from the backup copy
            this.PORID = _backupCopy.PORID;
            this.CODE = _backupCopy.CODE;
            this.PORSANT = _backupCopy.PORSANT;
            this.NAME = _backupCopy.NAME;
            this.VAHED = _backupCopy.VAHED;

            _inEdit = false;
            _backupCopy = null;
        }

        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null; // Commit changes by discarding the backup
        }
    }
}
