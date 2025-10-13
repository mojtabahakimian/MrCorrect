using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_GRP_FT : INotifyPropertyChanged
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

        public GRADE_GRP_FT()
        {
            GFGRPZARIB = 0;
            GFGRPGRADE = 0;
        }

        private int? _gftid;
        public int? GFTID { get => _gftid; set { if (_gftid == value) return; _gftid = value; OnPropertyChanged("GFTID"); } }

        private int? _gftgrpid;
        public int? GFTGRPID { get => _gftgrpid; set { if (_gftgrpid == value) return; _gftgrpid = value; OnPropertyChanged("GFTGRPID"); } }

        private string? _gfgrpnameft;
        public string? GFGRPNAMEFT { get => _gfgrpnameft; set { if (_gfgrpnameft == value) return; _gfgrpnameft = value; OnPropertyChanged("GFGRPNAMEFT"); } }

        private Single? _gfgrpzarib;
        public Single? GFGRPZARIB { get => _gfgrpzarib; set { if (_gfgrpzarib == value) return; _gfgrpzarib = value; OnPropertyChanged("GFGRPZARIB"); } }

        private Single? _gfgrpgrade;
        public Single? GFGRPGRADE { get => _gfgrpgrade; set { if (_gfgrpgrade == value) return; _gfgrpgrade = value; OnPropertyChanged("GFGRPGRADE"); } }

        private int? _gvaluescale;
        public int? GVALUESCALE { get => _gvaluescale; set { if (_gvaluescale == value) return; _gvaluescale = value; OnPropertyChanged("GVALUESCALE"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
