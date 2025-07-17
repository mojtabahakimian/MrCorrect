using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class TCOD_BANKS : INotifyPropertyChanged , ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private int? _code;
        public int? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
