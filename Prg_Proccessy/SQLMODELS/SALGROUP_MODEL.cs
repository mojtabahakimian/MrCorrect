using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SALGROUP_MODEL : INotifyPropertyChanged, ICloneable
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
        private string? _sal_name;
        public string? SAL_NAME { get => _sal_name; set { if (_sal_name == value) return; _sal_name = value; OnPropertyChanged("SAL_NAME"); } }
        private string? _psal_name;
        public string? PSAL_NAME { get => _psal_name; set { if (_psal_name == value) return; _psal_name = value; OnPropertyChanged("PSAL_NAME"); } }
        private int? _grsal;
        public int? GRSAL { get => _grsal; set { if (_grsal == value) return; _grsal = value; OnPropertyChanged("GRSAL"); } }
        private byte _enabl;
        public byte ENABL { get => _enabl; set { if (_enabl == value) return; _enabl = value; OnPropertyChanged("ENABL"); } }
        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }
        private string? _name_hes;
        public string? NAME_HES { get => _name_hes; set { if (_name_hes == value) return; _name_hes = value; OnPropertyChanged("NAME_HES"); } }
        private int? _porid;
        public int? PORID { get => _porid; set { if (_porid == value) return; _porid = value; OnPropertyChanged("PORID"); } }
        private byte[] _emza;
        public byte[] EMZA { get => _emza; set { if (_emza == value) return; _emza = value; OnPropertyChanged("EMZA"); } }
        private int? _menup;
        public int? menup { get => _menup; set { if (_menup == value) return; _menup = value; OnPropertyChanged("menup"); } }
        private int? _erjabe;
        public int? erjabe { get => _erjabe; set { if (_erjabe == value) return; _erjabe = value; OnPropertyChanged("erjabe"); } }
        
        private int? _DEFAULT_NAHVA;
        public int? DEFAULT_NAHVA { get => _DEFAULT_NAHVA; set { if (_DEFAULT_NAHVA == value) return; _DEFAULT_NAHVA = value; OnPropertyChanged("DEFAULT_NAHVA"); } }


        private int? _DEFAULT_SHIFT;
        public int? DEFAULT_SHIFT { get => _DEFAULT_SHIFT; set { if (_DEFAULT_SHIFT == value) return; _DEFAULT_SHIFT = value; OnPropertyChanged("DEFAULT_SHIFT"); } }

        private int? _DEFAULT_TFSAZMAN;
        public int? DEFAULT_TFSAZMAN { get => _DEFAULT_TFSAZMAN; set { if (_DEFAULT_TFSAZMAN == value) return; _DEFAULT_TFSAZMAN = value; OnPropertyChanged("DEFAULT_TFSAZMAN"); } }

    }
}
