using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SQLSTATE : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ICloneable
        public object Clone() => this.MemberwiseClone();

        // Fields
        private int? _idd;
        private string? _sqlst;
        private string? _titel;
        private string? _user_name;
        private byte _letother;
        private DateTime? _cr_date;
        private DateTime? _crt;
        private int? _uid;

        // Properties
        public int? IDD
        {
            get => _idd;
            set { if (_idd == value) return; _idd = value; OnPropertyChanged(nameof(IDD)); }
        }
        public string? SQLST
        {
            get => _sqlst;
            set { if (_sqlst == value) return; _sqlst = value; OnPropertyChanged(nameof(SQLST)); }
        }
        public string? TITEL
        {
            get => _titel;
            set { if (_titel == value) return; _titel = value; OnPropertyChanged(nameof(TITEL)); }
        }
        public string? USER_NAME
        {
            get => _user_name;
            set { if (_user_name == value) return; _user_name = value; OnPropertyChanged(nameof(USER_NAME)); }
        }
        public byte LETOTHER
        {
            get => _letother;
            set { if (_letother == value) return; _letother = value; OnPropertyChanged(nameof(LETOTHER)); }
        }
        public DateTime? CR_DATE
        {
            get => _cr_date;
            set { if (_cr_date == value) return; _cr_date = value; OnPropertyChanged(nameof(CR_DATE)); }
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

        // IEditableObject implementation
        private SQLSTATE? _backupCopy;
        private bool _inEdit;
        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (SQLSTATE)this.Clone();
            _inEdit = true;
        }
        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;
            // restore backup values
            IDD = _backupCopy.IDD;
            SQLST = _backupCopy.SQLST;
            TITEL = _backupCopy.TITEL;
            USER_NAME = _backupCopy.USER_NAME;
            LETOTHER = _backupCopy.LETOTHER;
            CR_DATE = _backupCopy.CR_DATE;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;
            _inEdit = false;
            _backupCopy = null;
        }
        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null;
        }
    }
}
