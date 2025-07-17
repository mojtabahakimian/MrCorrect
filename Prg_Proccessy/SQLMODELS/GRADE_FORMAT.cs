using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<GRADE_TAB_FT> GRADETABFT { get; set; } = new List<GRADE_TAB_FT>();

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
