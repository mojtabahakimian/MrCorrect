using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace Prg_Proccessy.SQLMODELS
{
    public class COMBMODEL_INT : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int _ID;
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value; OnPropertyChanged("ID");
            }
        }
        private string _NAME;
        public string NAME
        {
            get { return _NAME; }
            set { _NAME = value; OnPropertyChanged("NAME"); }
        }
    }
}
