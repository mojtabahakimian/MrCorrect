using Prg_Proccessy.FUNCTIONS;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_FORMAT : INotifyPropertyChanged
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }

        public GRADE_FORMAT()
        {
            GFDATE = Convert.ToInt64(Tarikh.FullCurrentDate);

            JAMZARIB = 0;
            EMTIAZ = 0;

            GRADETABFT ??= new ObservableCollection<GRADE_TAB_FT?>();
        }
        private ObservableCollection<GRADE_TAB_FT?> _GRADETABFT;
        public ObservableCollection<GRADE_TAB_FT?> GRADETABFT
        {
            get => _GRADETABFT;
            set { if (_GRADETABFT == value) return; _GRADETABFT = value; OnPropertyChanged(nameof(GRADETABFT)); }
        }

        private int? _idd;
        public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }

        private string? _gfname;
        public string? GFNAME { get => _gfname; set { if (_gfname == value) return; _gfname = value; OnPropertyChanged("GFNAME"); } }

        private long? _gfdate;
        public long? GFDATE { get => _gfdate; set { if (_gfdate == value) return; _gfdate = value; OnPropertyChanged("GFDATE"); } }

        private string? _tozih;
        public string? TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }

        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }

        private Single? _jamzarib;
        public Single? JAMZARIB { get => _jamzarib; set { if (_jamzarib == value) return; _jamzarib = value; OnPropertyChanged("JAMZARIB"); } }

        private Single? _emtiaz;
        public Single? EMTIAZ { get => _emtiaz; set { if (_emtiaz == value) return; _emtiaz = value; OnPropertyChanged("EMTIAZ"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
