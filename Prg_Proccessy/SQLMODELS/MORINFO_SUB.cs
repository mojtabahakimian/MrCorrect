using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class MORINFO_SUB : INotifyPropertyChanged
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }

        private string? _codemelli;
        public string? CODEMELLI { get => _codemelli; set { if (_codemelli == value) return; _codemelli = value; OnPropertyChanged("CODEMELLI"); } }

        private string? _namp;
        public string? NAMP { get => _namp; set { if (_namp == value) return; _namp = value; OnPropertyChanged("NAMP"); } }

        private long? _tavalod_date;
        public long? TAVALOD_DATE { get => _tavalod_date; set { if (_tavalod_date == value) return; _tavalod_date = value; OnPropertyChanged("TAVALOD_DATE"); } }

        private long? _marrid_date;
        public long? MARRID_DATE { get => _marrid_date; set { if (_marrid_date == value) return; _marrid_date = value; OnPropertyChanged("MARRID_DATE"); } }

        private string? _emailm;
        public string? EMAILM { get => _emailm; set { if (_emailm == value) return; _emailm = value; OnPropertyChanged("EMAILM"); } }

        private string? _sitem;
        public string? SITEM { get => _sitem; set { if (_sitem == value) return; _sitem = value; OnPropertyChanged("SITEM"); } }

        private string? _watsap;
        public string? WATSAP { get => _watsap; set { if (_watsap == value) return; _watsap = value; OnPropertyChanged("WATSAP"); } }

        private string? _telegram;
        public string? TELEGRAM { get => _telegram; set { if (_telegram == value) return; _telegram = value; OnPropertyChanged("TELEGRAM"); } }

        private string? _instam;
        public string? INSTAM { get => _instam; set { if (_instam == value) return; _instam = value; OnPropertyChanged("INSTAM"); } }

        private string? _linkedinm;
        public string? LINKEDINM { get => _linkedinm; set { if (_linkedinm == value) return; _linkedinm = value; OnPropertyChanged("LINKEDINM"); } }

        private int? _motahel;
        public int? MOTAHEL { get => _motahel; set { if (_motahel == value) return; _motahel = value; OnPropertyChanged("MOTAHEL"); } }

        private string? _addressmanzel;
        public string? ADDRESSMANZEL { get => _addressmanzel; set { if (_addressmanzel == value) return; _addressmanzel = value; OnPropertyChanged("ADDRESSMANZEL"); } }

        private byte[] _aks;
        public byte[] AKS { get => _aks; set { if (_aks == value) return; _aks = value; OnPropertyChanged("AKS"); } }

        private int? _nesbat;
        public int? NESBAT { get => _nesbat; set { if (_nesbat == value) return; _nesbat = value; OnPropertyChanged("NESBAT"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }
    }
}
