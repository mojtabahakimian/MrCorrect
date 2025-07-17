using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class ANBGRD_HEAD_LST_MODEL : INotifyPropertyChanged, ICloneable
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
        private int? _grd_num;
        public int? GRD_NUM { get => _grd_num; set { if (_grd_num == value) return; _grd_num = value; OnPropertyChanged("GRD_NUM"); } }
        private long? _grd_date;
        public long? GRD_DATE { get => _grd_date; set { if (_grd_date == value) return; _grd_date = value; OnPropertyChanged("GRD_DATE"); } }
        private int? _grd_anbar;
        public int? GRD_ANBAR { get => _grd_anbar; set { if (_grd_anbar == value) return; _grd_anbar = value; OnPropertyChanged("GRD_ANBAR"); } }
        private string? _grd_anbar_name;
        public string? GRD_ANBAR_NAME { get => _grd_anbar_name; set { if (_grd_anbar_name == value) return; _grd_anbar_name = value; OnPropertyChanged("GRD_ANBAR_NAME"); } }
        private string? _grd_hes;
        public string? GRD_HES { get => _grd_hes; set { if (_grd_hes == value) return; _grd_hes = value; OnPropertyChanged("GRD_HES"); } }
        private string? _grd_hes_name;
        public string? GRD_HES_NAME { get => _grd_hes_name; set { if (_grd_hes_name == value) return; _grd_hes_name = value; OnPropertyChanged("GRD_HES_NAME"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private string? _comment;
        public string? COMMENT { get => _comment; set { if (_comment == value) return; _comment = value; OnPropertyChanged("COMMENT"); } }
        private string? _user_name;
        public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
