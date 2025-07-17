using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SALA_DTL : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string SAL_NAME { get; set; }
        public string PSAL_NAME { get; set; }
        public int GRSAL { get; set; }
        public byte ENABL { get; set; }
        public int IDD { get; set; }
        public string HES { get; set; }
        public Nullable<int> PORID { get; set; }
        public byte[] EMZA { get; set; }
        public Nullable<int> menup { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }



        //// Deep clone implementation
        //public object Clone()
        //{
        //    return new SALA_DTL
        //    {
        //        SAL_NAME = this.SAL_NAME,
        //        PSAL_NAME = this.PSAL_NAME,
        //        GRSAL = this.GRSAL,
        //        ENABL = this.ENABL,
        //        IDD = this.IDD,
        //        HES = this.HES,
        //        PORID = this.PORID,
        //        EMZA = (byte[])this.EMZA?.Clone(), // Clone arrays or complex types to avoid reference issues
        //        menup = this.menup,
        //        IsSelected = this.IsSelected
        //    };
        //}
    }
}