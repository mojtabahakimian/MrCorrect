using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class TFORMS : INotifyPropertyChanged, ICloneable
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
        public string? FORMNAME { get; set; }
        public string? CAPTION { get; set; }
        public short? kind { get; set; }
        public short? GRP { get; set; }
        public int? IDH { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }

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
    }
}
