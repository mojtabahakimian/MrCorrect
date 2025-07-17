using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CUST_HESAB : INotifyPropertyChanged
    {
        private string _hes;
        private string _name;
        private int? _custCod;


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string hes
        {
            get { return _hes; }
            set
            {
                if (_hes != value)
                {
                    _hes = value;
                    OnPropertyChanged(nameof(hes));
                }
            }
        }
        public string NAME
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(NAME));
                }
            }
        }
        public int? CUST_COD
        {
            get { return _custCod; }
            set
            {
                if (_custCod != value)
                {
                    _custCod = value;
                    OnPropertyChanged(nameof(CUST_COD));
                }
            }
        }

        public string? ADDRESS { get; set; }
        public string? TEL { get; set; }
        public string? CODE_E { get; set; }
        public string? ECODE { get; set; }
        public string? PCODE { get; set; }
        public string? IYALAT { get; set; }
        public string? CITY { get; set; }
        public string? MCODEM { get; set; }
        public string? TOZIH { get; set; }
        public string? MOBILE { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? ROUTE_NAME { get; set; }
        public int? OSTANID { get; set; }
        public int? SHAHRID { get; set; }
        public int? tob { get; set; }
    }
}