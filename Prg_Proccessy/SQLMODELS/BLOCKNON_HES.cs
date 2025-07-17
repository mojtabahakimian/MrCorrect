using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class BLOCKNON_HES : INotifyPropertyChanged, ICloneable
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
        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }
        
        private string? name_hes;
        public string? NAME_HES { get => name_hes; set { if (name_hes == value) return; name_hes = value; OnPropertyChanged("NAME_HES"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
