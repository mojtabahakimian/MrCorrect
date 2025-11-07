using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class LIST_KALA_MODEL : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public object Clone()
        {
            return MemberwiseClone();
        }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(NAME)); } }
        private string? _kala;
        public string? kala { get => _kala; set { if (_kala == value) return; _kala = value; OnPropertyChanged(nameof(kala)); } }
        private int? _tag;
        public int? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged(nameof(TAG)); } }
        private string? _bargah; public string? BARGAH
        {
            get => _bargah; set
            {
                if (_bargah == value) return; _bargah = value;
                OnPropertyChanged(nameof(BARGAH));
            }
        }
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged(nameof(CODE)); } }
        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged(nameof(CUST_NO)); } }
        private decimal? _mar;
        public decimal? mar { get => _mar; set { if (_mar == value) return; _mar = value; OnPropertyChanged(nameof(mar)); } }
        private decimal? _mrgh;
        public decimal? mrgh { get => _mrgh; set { if (_mrgh == value) return; _mrgh = value; OnPropertyChanged(nameof(mrgh)); } }
    }
}