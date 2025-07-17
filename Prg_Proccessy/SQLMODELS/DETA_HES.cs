using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class DETA_HES : INotifyPropertyChanged, ICloneable
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

        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }

        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }

        private string? _tozih;
        public string? TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }

        private double? _bed_bes;
        public double? BED_BES { get => _bed_bes; set { if (_bed_bes == value) return; _bed_bes = value; OnPropertyChanged("BED_BES"); } }

        private string? _address;
        public string? ADDRESS { get => _address; set { if (_address == value) return; _address = value; OnPropertyChanged("ADDRESS"); } }

        private string? _tel;
        public string? TEL { get => _tel; set { if (_tel == value) return; _tel = value; OnPropertyChanged("TEL"); } }

        private string? _code_e;
        public string? CODE_E { get => _code_e; set { if (_code_e == value) return; _code_e = value; OnPropertyChanged("CODE_E"); } }

        private int? _userco;
        public int? USERCO { get => _userco; set { if (_userco == value) return; _userco = value; OnPropertyChanged("USERCO"); } }

        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
