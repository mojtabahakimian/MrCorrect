using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class BLOCK_CUSTOMER : INotifyPropertyChanged, ICloneable
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

        public BLOCK_CUSTOMER()
        {
            STBLKDT = Convert.ToInt32(Tarikh.FullCurrentDate);
            CUSER = Baseknow.UUSER;
            ENDBLK = false;
        }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        private string? _hes;
        public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }   
        
        private string? _NAME_HES;
        public string? NAME_HES { get => _NAME_HES; set { if (_NAME_HES == value) return; _NAME_HES = value; OnPropertyChanged("NAME_HES"); } }

        private int? _stblkdt;
        public int? STBLKDT { get => _stblkdt; set { if (_stblkdt == value) return; _stblkdt = value; OnPropertyChanged("STBLKDT"); } }
        private int? _endblkdt;
        public int? ENDBLKDT { get => _endblkdt; set { if (_endblkdt == value) return; _endblkdt = value; OnPropertyChanged("ENDBLKDT"); } }
        private bool? _endblk;
        public bool? ENDBLK { get => _endblk; set { if (_endblk == value) return; _endblk = value; OnPropertyChanged("ENDBLK"); } }
        private string? _comm;
        public string? COMM { get => _comm; set { if (_comm == value) return; _comm = value; OnPropertyChanged("COMM"); } }
        private string? _cuser;
        public string? CUSER { get => _cuser; set { if (_cuser == value) return; _cuser = value; OnPropertyChanged("CUSER"); } }
        private string? _euser;
        public string? EUSER { get => _euser; set { if (_euser == value) return; _euser = value; OnPropertyChanged("EUSER"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

    }
}
