using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TAKHPERS : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public TAKHPERS()
        {
            TAFPER = 0;
            PRICE_M = 0;
            PERS = 0;
            BLNS = 0;
        }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private string? _takh_cod;
        public string? TAKH_COD { get => _takh_cod; set { if (_takh_cod == value) return; _takh_cod = value; OnPropertyChanged("TAKH_COD"); } }

        private int? _cust_co;
        public int? CUST_CO { get => _cust_co; set { if (_cust_co == value) return; _cust_co = value; OnPropertyChanged("CUST_CO"); } }

        private short? _tafper;
        public short? TAFPER { get => _tafper; set { if (_tafper == value) return; _tafper = value; OnPropertyChanged("TAFPER"); } }

        private int? _price_m;
        public int? PRICE_M { get => _price_m; set { if (_price_m == value) return; _price_m = value; OnPropertyChanged("PRICE_M"); } }

        private double? _pers;
        public double? PERS { get => _pers; set { if (_pers == value) return; _pers = value; OnPropertyChanged("PERS"); } }

        private int? _blns;
        public int? BLNS { get => _blns; set { if (_blns == value) return; _blns = value; OnPropertyChanged("BLNS"); } }

        private bool? _put;
        public bool? PUT { get => _put; set { if (_put == value) return; _put = value; OnPropertyChanged("PUT"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
