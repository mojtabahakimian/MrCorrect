using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TOTA_HES : INotifyPropertyChanged, ICloneable
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

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }

        private double? _no_hes;
        public double? NO_HES { get => _no_hes; set { if (_no_hes == value) return; _no_hes = value; OnPropertyChanged("NO_HES"); } }

        private double? _m_d;
        public double? M_D { get => _m_d; set { if (_m_d == value) return; _m_d = value; OnPropertyChanged("M_D"); } }

        private double? _group;
        public double? GROUP { get => _group; set { if (_group == value) return; _group = value; OnPropertyChanged("GROUP"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
