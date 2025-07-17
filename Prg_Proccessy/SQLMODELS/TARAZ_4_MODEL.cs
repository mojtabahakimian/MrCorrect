using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TARAZ_4_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int? _number;
        public int? NUMBER
        {
            get => _number;
            set { if (_number == value) return; _number = value; OnPropertyChanged(nameof(NUMBER)); }
        }

        private string? _name;
        public string? NAME
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(NAME)); }
        }

        private double? _sumOfBED;
        public double? SumOfBED
        {
            get => _sumOfBED;
            set { if (_sumOfBED == value) return; _sumOfBED = value; OnPropertyChanged(nameof(SumOfBED)); }
        }

        private double? _sumOfBES;
        public double? SumOfBES
        {
            get => _sumOfBES;
            set { if (_sumOfBES == value) return; _sumOfBES = value; OnPropertyChanged(nameof(SumOfBES)); }
        }

        private double? _bed;
        public double? bed
        {
            get => _bed;
            set { if (_bed == value) return; _bed = value; OnPropertyChanged(nameof(bed)); }
        }

        private double? _bes;
        public double? bes
        {
            get => _bes;
            set { if (_bes == value) return; _bes = value; OnPropertyChanged(nameof(bes)); }
        }
    }
}
