using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SALS_PERMIS : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                OnPropertyChanged("IsModified");
            }
        }

        private bool? _del;
        public bool? DEL { get => _del; set { if (_del == value) return; _del = value; IsModified = true; OnPropertyChanged("DEL"); } }
        private bool? _upd;
        public bool? UPD { get => _upd; set { if (_upd == value) return; _upd = value; IsModified = true; OnPropertyChanged("UPD"); } }
        private bool? _inp;
        public bool? INP { get => _inp; set { if (_inp == value) return; _inp = value; IsModified = true; OnPropertyChanged("INP"); } }
        private bool? _see;
        public bool? SEE { get => _see; set { if (_see == value) return; _see = value; IsModified = true; OnPropertyChanged("SEE"); } }
        private bool? _run;
        public bool? RUN { get => _run; set { if (_run == value) return; _run = value; IsModified = true; OnPropertyChanged("RUN"); } }
        private int? _object;
        public int? OBJECT { get => _object; set { if (_object == value) return; _object = value; OnPropertyChanged("OBJECT"); } }
        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }
        private string? _caption;
        public string? CAPTION { get => _caption; set { if (_caption == value) return; _caption = value; OnPropertyChanged("CAPTION"); } }
        private int? _idh;
        public int? IDH { get => _idh; set { if (_idh == value) return; _idh = value; OnPropertyChanged("IDH"); } }
        private short? _grp;
        public short? GRP { get => _grp; set { if (_grp == value) return; _grp = value; OnPropertyChanged("GRP"); } }
        private short? _kind;
        public short? kind { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("kind"); } }
        private string? _formname;
        public string? FORMNAME { get => _formname; set { if (_formname == value) return; _formname = value; OnPropertyChanged("FORMNAME"); } }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }  
        
        private SALS_PERMIS _selectedrow;
        public SALS_PERMIS SelectedRow
        {
            get => _selectedrow;
            set
            {
                _selectedrow = value;
                OnPropertyChanged("SelectedRow");
            }
        }
    }
}
