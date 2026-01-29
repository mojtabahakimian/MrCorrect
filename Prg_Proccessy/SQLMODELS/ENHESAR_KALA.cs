using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prg_Proccessy.SQLMODELS
{
    public class ENHESAR_KALA : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        private int _en_idd;
        private string _en_cust_co;
        private string _en_kala_cod;
        private int _en_kala_gr;
        private string _en_grcode1;
        private string _en_grcode2;
        private string _en_grcode3;
        private string _en_grcode4;
        private string _en_grcode5;
        private string _en_grcode6;
        private string _en_grcode7;
        private string _en_grcode8;
        private string _en_grcode9;
        private string _en_grcode10;
        private int _userid;
        private DateTime _en_cdate;
        private DateTime? _crt;
        private int? _uid;

        // Non-DB properties for UI binding (names)
        private string _kala_name;
        public string KALA_NAME
        {
            get => _kala_name;
            set { if (_kala_name == value) return; _kala_name = value; OnPropertyChanged(); }
        }

        public int EN_IDD
        {
            get => _en_idd;
            set { if (_en_idd == value) return; _en_idd = value; OnPropertyChanged(); }
        }

        public string EN_CUST_CO
        {
            get => _en_cust_co;
            set { if (_en_cust_co == value) return; _en_cust_co = value; OnPropertyChanged(); }
        }

        public string EN_KALA_COD
        {
            get => _en_kala_cod;
            set { if (_en_kala_cod == value) return; _en_kala_cod = value; OnPropertyChanged(); }
        }

        public int EN_KALA_GR
        {
            get => _en_kala_gr;
            set { if (_en_kala_gr == value) return; _en_kala_gr = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE1
        {
            get => _en_grcode1;
            set { if (_en_grcode1 == value) return; _en_grcode1 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE2
        {
            get => _en_grcode2;
            set { if (_en_grcode2 == value) return; _en_grcode2 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE3
        {
            get => _en_grcode3;
            set { if (_en_grcode3 == value) return; _en_grcode3 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE4
        {
            get => _en_grcode4;
            set { if (_en_grcode4 == value) return; _en_grcode4 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE5
        {
            get => _en_grcode5;
            set { if (_en_grcode5 == value) return; _en_grcode5 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE6
        {
            get => _en_grcode6;
            set { if (_en_grcode6 == value) return; _en_grcode6 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE7
        {
            get => _en_grcode7;
            set { if (_en_grcode7 == value) return; _en_grcode7 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE8
        {
            get => _en_grcode8;
            set { if (_en_grcode8 == value) return; _en_grcode8 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE9
        {
            get => _en_grcode9;
            set { if (_en_grcode9 == value) return; _en_grcode9 = value; OnPropertyChanged(); }
        }

        public string EN_GRCODE10
        {
            get => _en_grcode10;
            set { if (_en_grcode10 == value) return; _en_grcode10 = value; OnPropertyChanged(); }
        }

        public int USERID
        {
            get => _userid;
            set { if (_userid == value) return; _userid = value; OnPropertyChanged(); }
        }

        public DateTime EN_CDATE
        {
            get => _en_cdate;
            set { if (_en_cdate == value) return; _en_cdate = value; OnPropertyChanged(); }
        }

        public DateTime? CRT
        {
            get => _crt;
            set { if (_crt == value) return; _crt = value; OnPropertyChanged(); }
        }

        public int? UID
        {
            get => _uid;
            set { if (_uid == value) return; _uid = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private ENHESAR_KALA _backupCopy;
        private bool _inEdit = false;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (ENHESAR_KALA)this.Clone();
            _inEdit = true;
        }

        public void EndEdit()
        {
            _backupCopy = null;
            _inEdit = false;
        }

        public void CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            EN_IDD = _backupCopy.EN_IDD;
            EN_CUST_CO = _backupCopy.EN_CUST_CO;
            EN_KALA_COD = _backupCopy.EN_KALA_COD;
            EN_KALA_GR = _backupCopy.EN_KALA_GR;
            EN_GRCODE1 = _backupCopy.EN_GRCODE1;
            EN_GRCODE2 = _backupCopy.EN_GRCODE2;
            EN_GRCODE3 = _backupCopy.EN_GRCODE3;
            EN_GRCODE4 = _backupCopy.EN_GRCODE4;
            EN_GRCODE5 = _backupCopy.EN_GRCODE5;
            EN_GRCODE6 = _backupCopy.EN_GRCODE6;
            EN_GRCODE7 = _backupCopy.EN_GRCODE7;
            EN_GRCODE8 = _backupCopy.EN_GRCODE8;
            EN_GRCODE9 = _backupCopy.EN_GRCODE9;
            EN_GRCODE10 = _backupCopy.EN_GRCODE10;
            USERID = _backupCopy.USERID;
            EN_CDATE = _backupCopy.EN_CDATE;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;

            _inEdit = false;
        }
    }
}
