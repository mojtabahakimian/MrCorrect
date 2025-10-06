using System;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TARAZHES : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}