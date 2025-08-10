using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class ANBARGRD_SUB3_MODEL : INotifyPropertyChanged, ICloneable
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
        private double? _ekh;
        public double? EKH { get => _ekh; set { if (_ekh == value) return; _ekh = value; OnPropertyChanged("EKH"); } }
        private int? _grd_num;
        public int? GRD_NUM { get => _grd_num; set { if (_grd_num == value) return; _grd_num = value; OnPropertyChanged("GRD_NUM"); } }
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }
        private string? _nam;
        public string? nam { get => _nam; set { if (_nam == value) return; _nam = value; OnPropertyChanged("nam"); } }
        private decimal? _mog;
        public decimal? MOG { get => _mog; set { if (_mog == value) return; _mog = value; OnPropertyChanged("MOG"); } }
        private double? _num1;
        public double? NUM1 { get => _num1; set { if (_num1 == value) return; _num1 = value; OnPropertyChanged("NUM1"); } }
        private double? _num2;
        public double? NUM2 { get => _num2; set { if (_num2 == value) return; _num2 = value; OnPropertyChanged("NUM2"); } }
        private double? _num3;
        public double? NUM3 { get => _num3; set { if (_num3 == value) return; _num3 = value; OnPropertyChanged("NUM3"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }
        private string? _n_fani;
        public string? N_FANI { get => _n_fani; set { if (_n_fani == value) return; _n_fani = value; OnPropertyChanged("N_FANI"); } }
        private string? _grp;
        public string? grp { get => _grp; set { if (_grp == value) return; _grp = value; OnPropertyChanged("grp"); } }

    }
}
