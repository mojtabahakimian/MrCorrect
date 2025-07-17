namespace Prg_Proccessy.SQLMODELS
{
    public class COMBOYMODEL //: INotifyPropertyChanged, ICloneable
    {
        //public object Clone()
        //{
        //    return this.MemberwiseClone();
        //}

        //public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        ////private int _ID;
        ////public int ID
        ////{
        ////    get { return _ID; }
        ////    set
        ////    {
        ////        if (_ID == value) return;
        ////        _ID = value; OnPropertyChanged(nameof(ID));
        ////    }
        ////}  OnPropertyChanged("CODE");}

        //private int _ID;
        //public int ID
        //{
        //    get { return _ID; }
        //    set
        //    {
        //        _ID = value; OnPropertyChanged("ID");
        //    }
        //}

        //private string _NAME;
        //public string NAME
        //{
        //    get { return _NAME; }
        //    set { _NAME = value; OnPropertyChanged("NAME"); }
        //}

        public string NAME { get; set; }
        public int ID { get; set; }
    }
}