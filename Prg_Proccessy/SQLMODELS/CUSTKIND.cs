using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CUSTKIND : INotifyPropertyChanged, ICloneable
    {
        private int? _cust_cod;
        public int? CUST_COD { get => _cust_cod; set { if (_cust_cod == value) return; _cust_cod = value; OnPropertyChanged("CUST_COD"); } }

        private string? _custkname;
        public string? CUSTKNAME { get => _custkname; set { if (_custkname == value) return; _custkname = value; OnPropertyChanged("CUSTKNAME"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
   
}
