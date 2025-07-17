using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CUSTKIND_TF : INotifyPropertyChanged, ICloneable
    {
        public CUSTKIND_TF()
        {
            ACTIV = false;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private long? _ID;
        public long? ID { get => _ID; set { if (_ID == value) return; _ID = value; OnPropertyChanged("ID"); } }

        private int? _cust_cod;
        public int? CUST_COD { get => _cust_cod; set { if (_cust_cod == value) return; _cust_cod = value; OnPropertyChanged("CUST_COD"); } }

        private int? _tid;
        public int? TID { get => _tid; set { if (_tid == value) return; _tid = value; OnPropertyChanged("TID"); } }

        private DateTime? _crea;
        public DateTime? CREA { get => _crea; set { if (_crea == value) return; _crea = value; OnPropertyChanged("CREA"); } }

        private bool? _activ;
        public bool? ACTIV { get => _activ; set { if (_activ == value) return; _activ = value; OnPropertyChanged("ACTIV"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
