using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class ORDR_LST : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ORDR_LST()
        {
            MEGH = 0;
            MEGHK = 0;
            MEGH_MAR = 0; // Assuming this might be needed or 0
        }

        private ORDR_LST _backupCopy;
        private bool _inEdit = false;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (ORDR_LST)this.Clone();
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

            ID = _backupCopy.ID;
            CODE = _backupCopy.CODE;
            NAME_CODE = _backupCopy.NAME_CODE;
            DATE = _backupCopy.DATE;
            RADIF = _backupCopy.RADIF;
            MEGH = _backupCopy.MEGH;
            MEGHK = _backupCopy.MEGHK;
            VAHED_K = _backupCopy.VAHED_K;
            CUST_NO = _backupCopy.CUST_NO;
            idd = _backupCopy.idd;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;
            NAME_VAHED_K = _backupCopy.NAME_VAHED_K;
            MEGH_MAR = _backupCopy.MEGH_MAR;
            ContractID = _backupCopy.ContractID;

            _inEdit = false;
        }

        private int _id_fk;
        public int ID // FK
        {
            get => _id_fk;
            set
            {
                if (_id_fk == value) return;
                _id_fk = value;
                OnPropertyChanged();
            }
        }

        private string _code;
        public string CODE
        {
            get => _code;
            set
            {
                if (_code == value) return;
                _code = value;
                OnPropertyChanged();
            }
        }

        private string _name_code;
        public string NAME_CODE
        {
            get => _name_code;
            set
            {
                if (_name_code == value) return;
                _name_code = value;
                OnPropertyChanged();
            }
        }

        private long? _date_lst;
        public long? DATE
        {
            get => _date_lst;
            set
            {
                if (_date_lst == value) return;
                _date_lst = value;
                OnPropertyChanged();
            }
        }

        private int? _radif;
        public int? RADIF
        {
            get => _radif;
            set
            {
                if (_radif == value) return;
                _radif = value;
                OnPropertyChanged();
            }
        }

        private double _megh;
        public double MEGH
        {
            get => _megh;
            set
            {
                if (_megh == value) return;
                _megh = value;
                OnPropertyChanged();
            }
        }

        private double _meghk;
        public double MEGHK
        {
            get => _meghk;
            set
            {
                if (_meghk == value) return;
                _meghk = value;
                OnPropertyChanged();
            }
        }

        private double? _megh_mar;
        public double? MEGH_MAR // Added for consistency with HAVALAH logic if needed, otherwise ignore.
        {
            get => _megh_mar;
            set
            {
                if (_megh_mar == value) return;
                _megh_mar = value;
                OnPropertyChanged();
            }
        }

        private double? _vahed_k;
        public double? VAHED_K
        {
            get => _vahed_k;
            set
            {
                if (_vahed_k == value) return;
                _vahed_k = value;
                OnPropertyChanged();
            }
        }

        private string _name_vahed_k;
        public string NAME_VAHED_K
        {
            get => _name_vahed_k;
            set
            {
                if (_name_vahed_k == value) return;
                _name_vahed_k = value;
                OnPropertyChanged();
            }
        }


        private string _cust_no_lst; // Description/Remarks
        public string CUST_NO
        {
            get => _cust_no_lst;
            set
            {
                if (_cust_no_lst == value) return;
                _cust_no_lst = value;
                OnPropertyChanged();
            }
        }

        private int _idd; // PK
        public int idd
        {
            get => _idd;
            set
            {
                if (_idd == value) return;
                _idd = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _crt;
        public DateTime? CRT
        {
            get => _crt;
            set
            {
                if (_crt == value) return;
                _crt = value;
                OnPropertyChanged();
            }
        }

        private int? _uid;
        public int? UID
        {
            get => _uid;
            set
            {
                if (_uid == value) return;
                _uid = value;
                OnPropertyChanged();
            }
        }

        private int? _contractID;
        public int? ContractID
        {
            get => _contractID;
            set
            {
                if (_contractID == value) return;
                _contractID = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
