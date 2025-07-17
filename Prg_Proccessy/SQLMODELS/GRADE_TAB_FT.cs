using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_TAB_FT : INotifyPropertyChanged
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
        public List<GRADE_GRP_FT> GRADEGRPFT { get; set; } = new List<GRADE_GRP_FT>();

        private int? _gfid;
        public int? GFID { get => _gfid; set { if (_gfid == value) return; _gfid = value; OnPropertyChanged("GFID"); } }

        private int? _gftid;
        public int? GFTID { get => _gftid; set { if (_gftid == value) return; _gftid = value; OnPropertyChanged("GFTID"); } }

        private string? _gfnameft;
        public string? GFNAMEFT { get => _gfnameft; set { if (_gfnameft == value) return; _gfnameft = value; OnPropertyChanged("GFNAMEFT"); } }

        private Single? _gfgzarib;
        public Single? GFGZARIB { get => _gfgzarib; set { if (_gfgzarib == value) return; _gfgzarib = value; OnPropertyChanged("GFGZARIB"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
