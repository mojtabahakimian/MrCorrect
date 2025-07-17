using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class TARAZ4_TAFZ2_DIRECT_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private int? _tnumber;
        public int? TNUMBER { get => _tnumber; set { if (_tnumber == value) return; _tnumber = value; OnPropertyChanged("TNUMBER"); } }
        private int? _hes_t2;
        public int? HES_T2 { get => _hes_t2; set { if (_hes_t2 == value) return; _hes_t2 = value; OnPropertyChanged("HES_T2"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private double? _sumofbed;
        public double? SumOfBED { get => _sumofbed; set { if (_sumofbed == value) return; _sumofbed = value; OnPropertyChanged("SumOfBED"); } }
        private double? _sumofbes;
        public double? SumOfBES { get => _sumofbes; set { if (_sumofbes == value) return; _sumofbes = value; OnPropertyChanged("SumOfBES"); } }
        private double? _bed;
        public double? bed { get => _bed; set { if (_bed == value) return; _bed = value; OnPropertyChanged("bed"); } }
        private double? _bes;
        public double? bes { get => _bes; set { if (_bes == value) return; _bes = value; OnPropertyChanged("bes"); } }
    }
}
