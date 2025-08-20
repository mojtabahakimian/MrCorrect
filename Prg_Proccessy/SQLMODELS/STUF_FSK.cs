using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class STUF_FSK : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public STUF_FSK()
        {
            MANDAH_A = 0;
            FI_A = 0;
            MABL_A = 0;
            MOGODI_A = 0;
            MIN_M = 0;
        }

        // --- Clone ---
        public object Clone() => this.MemberwiseClone();

        // --- IEditableObject backup handling ---
        private STUF_FSK _backupCopy;
        private bool _inEdit;

        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (STUF_FSK)this.Clone();
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

            this.CODE = _backupCopy.CODE;
            this.ANBAR = _backupCopy.ANBAR;
            this.MOGODI_A = _backupCopy.MOGODI_A;
            this.FI_A = _backupCopy.FI_A;
            this.MABL_A = _backupCopy.MABL_A;
            this.MANDAH_A = _backupCopy.MANDAH_A;
            this.VAZ = _backupCopy.VAZ;
            this.IDD = _backupCopy.IDD;
            this.POSITION = _backupCopy.POSITION;
            this.B_SEF = _backupCopy.B_SEF;
            this.N_SEF = _backupCopy.N_SEF;
            this.MIN_M = _backupCopy.MIN_M;
            this.MAX_M = _backupCopy.MAX_M;
            this.CRT = _backupCopy.CRT;
            this.UID = _backupCopy.UID;

            _inEdit = false;
        }

        // --- Properties ---
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged(nameof(CODE)); } }

        private int? _anbar;
        public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged(nameof(ANBAR)); } }

        private double? _mogodi_a;
        public double? MOGODI_A { get => _mogodi_a; set { if (_mogodi_a == value) return; _mogodi_a = value; OnPropertyChanged(nameof(MOGODI_A)); } }

        private double? _fi_a;
        public double? FI_A { get => _fi_a; set { if (_fi_a == value) return; _fi_a = value; OnPropertyChanged(nameof(FI_A)); } }

        private double? _mabl_a;
        public double? MABL_A { get => _mabl_a; set { if (_mabl_a == value) return; _mabl_a = value; OnPropertyChanged(nameof(MABL_A)); } }

        private double? _mandah_a;
        public double? MANDAH_A { get => _mandah_a; set { if (_mandah_a == value) return; _mandah_a = value; OnPropertyChanged(nameof(MANDAH_A)); } }

        private double? _vaz;
        public double? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged(nameof(VAZ)); } }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged(nameof(IDD)); } }

        private string? _position;
        public string? POSITION { get => _position; set { if (_position == value) return; _position = value; OnPropertyChanged(nameof(POSITION)); } }

        private double? _b_sef;
        public double? B_SEF { get => _b_sef; set { if (_b_sef == value) return; _b_sef = value; OnPropertyChanged(nameof(B_SEF)); } }

        private double? _n_sef;
        public double? N_SEF { get => _n_sef; set { if (_n_sef == value) return; _n_sef = value; OnPropertyChanged(nameof(N_SEF)); } }

        private double? _min_m;
        public double? MIN_M { get => _min_m; set { if (_min_m == value) return; _min_m = value; OnPropertyChanged(nameof(MIN_M)); } }

        private double? _max_m;
        public double? MAX_M { get => _max_m; set { if (_max_m == value) return; _max_m = value; OnPropertyChanged(nameof(MAX_M)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}