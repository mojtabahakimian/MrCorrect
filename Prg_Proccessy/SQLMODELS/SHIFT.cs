using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SHIFT : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private int? _shift_id;
        public int? SHIFT_ID { get => _shift_id; set { if (_shift_id == value) return; _shift_id = value; OnPropertyChanged("SHIFT_ID"); } }

        private string? _shname;
        public string? SHNAME { get => _shname; set { if (_shname == value) return; _shname = value; OnPropertyChanged("SHNAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
