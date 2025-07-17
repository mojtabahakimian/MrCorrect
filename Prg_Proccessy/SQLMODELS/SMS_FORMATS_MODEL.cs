using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class SMS_FORMATS_MODEL : INotifyPropertyChanged, ICloneable
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
        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private int? _smsty;
        public int? SMSTY { get => _smsty; set { if (_smsty == value) return; _smsty = value; OnPropertyChanged("SMSTY"); } }
        private byte _radif;
        public byte RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }
        private string? _sms_fiels;
        public string? SMS_FIELS { get => _sms_fiels; set { if (_sms_fiels == value) return; _sms_fiels = value; OnPropertyChanged("SMS_FIELS"); } }
        private string? _sms_caption;
        public string? SMS_CAPTION { get => _sms_caption; set { if (_sms_caption == value) return; _sms_caption = value; OnPropertyChanged("SMS_CAPTION"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
