using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_CUST_GRP : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int? _gcgrpid;
        public int? GCGRPID { get => _gcgrpid; set { if (_gcgrpid == value) return; _gcgrpid = value; OnPropertyChanged("GCGRPID"); } }

        private string? _gcgrpname;
        public string? GCGRPNAME { get => _gcgrpname; set { if (_gcgrpname == value) return; _gcgrpname = value; OnPropertyChanged("GCGRPNAME"); } }

        private Single? _gcgrpzarib;
        public Single? GCGRPZARIB { get => _gcgrpzarib; set { if (_gcgrpzarib == value) return; _gcgrpzarib = value; OnPropertyChanged("GCGRPZARIB"); } }

        private Single? _gcgrpgrade;
        public Single? GCGRPGRADE { get => _gcgrpgrade; set { if (_gcgrpgrade == value) return; _gcgrpgrade = value; OnPropertyChanged("GCGRPGRADE"); } }

        private int? _gctabid;
        public int? GCTABID { get => _gctabid; set { if (_gctabid == value) return; _gctabid = value; OnPropertyChanged("GCTABID"); } }

        private int? _gcvalue;
        public int? GCVALUE { get => _gcvalue; set { if (_gcvalue == value) return; _gcvalue = value; OnPropertyChanged("GCVALUE"); } }

        private int? _gcvaluescal;
        public int? GCVALUESCAL { get => _gcvaluescal; set { if (_gcvaluescal == value) return; _gcvaluescal = value; OnPropertyChanged("GCVALUESCAL"); } }

        private byte[] _gcps;
        public byte[] GCPS { get => _gcps; set { if (_gcps == value) return; _gcps = value; OnPropertyChanged("GCPS"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
