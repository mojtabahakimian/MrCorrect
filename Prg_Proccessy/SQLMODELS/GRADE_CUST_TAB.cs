using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_CUST_TAB : INotifyPropertyChanged, ICloneable
    {
        public GRADE_CUST_TAB()
        {
            GCDATE = Convert.ToInt64(Tarikh.FullCurrentDate);

            USERNAME = Baseknow.UUSER;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int? _gctabid;
        public int? GCTABID { get => _gctabid; set { if (_gctabid == value) return; _gctabid = value; OnPropertyChanged("GCTABID"); } }

        private string? _gcname;
        public string? GCNAME { get => _gcname; set { if (_gcname == value) return; _gcname = value; OnPropertyChanged("GCNAME"); } }

        private Single? _gczarib;
        public Single? GCZARIB { get => _gczarib; set { if (_gczarib == value) return; _gczarib = value; OnPropertyChanged("GCZARIB"); } }

        private string? _gccust_hes;
        public string? GCCUST_HES { get => _gccust_hes; set { if (_gccust_hes == value) return; _gccust_hes = value; OnPropertyChanged("GCCUST_HES"); } }

        private long? _gcdate;
        public long? GCDATE { get => _gcdate; set { if (_gcdate == value) return; _gcdate = value; OnPropertyChanged("GCDATE"); } }

        private string? _username;
        public string? USERNAME { get => _username; set { if (_username == value) return; _username = value; OnPropertyChanged("USERNAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        //Detail as Nested:
        private ObservableCollection<GRADE_CUST_GRP> _detailRowData;
        public ObservableCollection<GRADE_CUST_GRP> DETAILROWDATA
        {
            get => _detailRowData;
            set
            {
                if (_detailRowData == value) return;
                _detailRowData = value;
                OnPropertyChanged(nameof(DETAILROWDATA));
            }
        }

    }
}
