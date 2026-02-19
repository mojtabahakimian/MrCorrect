using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISITGOL_DTL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone() { return MemberwiseClone(); }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // --- Original Properties ---

        //public VISITGOL_DTL()
        //{
        //    MEGH = 0;
        //    MEGHk = 0;
        //}

        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged(nameof(HES)); } }

        private byte? _mah;
        public byte? MAH { get => _mah; set { if (_mah == value) return; _mah = value; OnPropertyChanged(nameof(MAH)); } }

        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged(nameof(CODE)); } }

        private DateTime? _cdate;
        public DateTime? CDATE { get => _cdate; set { if (_cdate == value) return; _cdate = value; OnPropertyChanged(nameof(CDATE)); } }

        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged(nameof(RADIF)); } }

        private double? _megh;
        public double? MEGH { get => _megh; set { if (_megh == value) return; _megh = value; OnPropertyChanged(nameof(MEGH)); } }

        private double? _meghk;
        public double? MEGHk { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged(nameof(MEGHk)); } }

        private int? _vahedK;
        public int? VAHED_K { get => _vahedK; set { if (_vahedK == value) return; _vahedK = value; OnPropertyChanged(nameof(VAHED_K)); } }

        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged(nameof(USERNAME)); } }

        private string _kala_name;
        public string NAME_CODE { get => _kala_name; set { if (_kala_name == value) return; _kala_name = value; OnPropertyChanged(nameof(NAME_CODE)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        // --- IEditableObject Implementation ---

        private VISITGOL_DTL? _backupData;
        private bool _inTxn = false;

        public void BeginEdit()
        {
            if (!_inTxn)
            {
                // Take a snapshot of the current state using your existing Clone method
                _backupData = (VISITGOL_DTL)this.Clone();
                _inTxn = true;
            }
        }

        public void CancelEdit()
        {
            if (_inTxn && _backupData != null)
            {
                // Restore all properties from the backup snapshot.
                // Reassigning through the public setters ensures OnPropertyChanged 
                // fires so the UI updates immediately to reflect the canceled state.
                this.HES = _backupData.HES;
                this.MAH = _backupData.MAH;
                this.CODE = _backupData.CODE;
                this.CDATE = _backupData.CDATE;
                this.RADIF = _backupData.RADIF;
                this.MEGH = _backupData.MEGH;
                this.MEGHk = _backupData.MEGHk;
                this.VAHED_K = _backupData.VAHED_K;
                this.USERNAME = _backupData.USERNAME;
                this.NAME_CODE = _backupData.NAME_CODE;
                this.CRT = _backupData.CRT;
                this.UID = _backupData.UID;

                _backupData = null;
                _inTxn = false;
            }
        }

        public void EndEdit()
        {
            if (_inTxn)
            {
                // Discard the snapshot to keep the new changes
                _backupData = null;
                _inTxn = false;
            }
        }
    }

}
