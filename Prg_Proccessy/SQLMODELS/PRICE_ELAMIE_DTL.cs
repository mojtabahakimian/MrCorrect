using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PRICE_ELAMIE_DTL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public PRICE_ELAMIE_DTL()
        {
            PRICE1 = 0;
        }

        // فیلدهای اصلی جدول
        private int _perid;
        public int PERID
        {
            get => _perid;
            set { if (_perid == value) return; _perid = value; OnPropertyChanged(nameof(PERID)); }
        }

        private int _pepid;
        public int PEPID
        {
            get => _pepid;
            set { if (_pepid == value) return; _pepid = value; OnPropertyChanged(nameof(PEPID)); }
        }

        private float _price1;
        public float PRICE1
        {
            get => _price1;
            set { if (_price1 == value) return; _price1 = value; OnPropertyChanged(nameof(PRICE1)); }
        }

        private DateTime _tr_date;
        public DateTime TR_DATE
        {
            get => _tr_date;
            set { if (_tr_date == value) return; _tr_date = value; OnPropertyChanged(nameof(TR_DATE)); }
        }

        private string _username;
        public string USERNAME
        {
            get => _username;
            set { if (_username == value) return; _username = value; OnPropertyChanged(nameof(USERNAME)); }
        }

        private int _pgid;
        public int PGID
        {
            get => _pgid;
            set { if (_pgid == value) return; _pgid = value; OnPropertyChanged(nameof(PGID)); }
        }

        private DateTime? _crt;
        public DateTime? CRT
        {
            get => _crt;
            set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); }
        }

        private int? _uid;
        public int? UID
        {
            get => _uid;
            set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); }
        }

        // --------------------------
        // ICloneable + IEditableObject برای Undo/Cancel
        private PRICE_ELAMIE_DTL _backupCopy;
        private bool _inEdit;

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (PRICE_ELAMIE_DTL)this.Clone();
            _inEdit = true;
        }

        void IEditableObject.EndEdit()
        {
            _backupCopy = null;
            _inEdit = false;
        }

        void IEditableObject.CancelEdit()
        {
            if (!_inEdit || _backupCopy == null) return;

            this.PERID = _backupCopy.PERID;
            this.PEPID = _backupCopy.PEPID;
            this.PRICE1 = _backupCopy.PRICE1;
            this.TR_DATE = _backupCopy.TR_DATE;
            this.USERNAME = _backupCopy.USERNAME;
            this.PGID = _backupCopy.PGID;
            this.CRT = _backupCopy.CRT;
            this.UID = _backupCopy.UID;

            _inEdit = false;
        }
    }
}
