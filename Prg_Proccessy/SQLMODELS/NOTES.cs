using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Prg_Proccessy.SQLMODELS
{
    public class NOTES : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        public NOTES()
        {
            Ndone = false;
        }
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _idd;
        public int? idd { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("idd"); } }
        private string? _note;
        public string? Note { get => _note; set { if (_note == value) return; _note = value; OnPropertyChanged("Note"); } }
        private string? _ndate;
        public string? Ndate { get => _ndate; set { if (_ndate == value) return; _ndate = value; OnPropertyChanged("Ndate"); } }
        private DateTime? _ntime;
        public DateTime? Ntime { get => _ntime; set { if (_ntime == value) return; _ntime = value; OnPropertyChanged("Ntime"); } }
        private int? _userid;
        public int? userid { get => _userid; set { if (_userid == value) return; _userid = value; OnPropertyChanged("userid"); } }
        private bool? _ndone;
        public bool? Ndone { get => _ndone; set { if (_ndone == value) return; _ndone = value; OnPropertyChanged("Ndone"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private NOTES _backupCopy;
        private bool _inEdit;
        void IEditableObject.BeginEdit()
        {
            if (_inEdit) return;
            _backupCopy = (NOTES)this.Clone();
            _inEdit = true;
        }
        void IEditableObject.EndEdit()
        {
            // commit: just forget the backup
            _backupCopy = null;
            _inEdit = false;
        }
        void IEditableObject.CancelEdit()
        {
            if (!_inEdit) return;
            // restore all of your properties from the backup
            idd = _backupCopy.idd;
            Note = _backupCopy.Note;
            Ndate = _backupCopy.Ndate;
            CRT = _backupCopy.CRT;
            UID = _backupCopy.UID;
            Ntime = _backupCopy.Ntime;

            _inEdit = false;
        }
    }
}
