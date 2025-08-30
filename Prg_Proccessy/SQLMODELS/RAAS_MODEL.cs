using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class RAAS_MODEL : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public object Clone()
        {
            return MemberwiseClone();
        }

        private int? _id;
        public int? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(ID)); } }

        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged(nameof(DATE_S)); } }

        private long? _mabl;
        public long? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged(nameof(MABL)); } }

        private int? _roos;
        public int? ROOS { get => _roos; set { if (_roos == value) return; _roos = value; OnPropertyChanged(nameof(ROOS)); } }

        private double? _mabk;
        public double? MABK { get => _mabk; set { if (_mabk == value) return; _mabk = value; OnPropertyChanged(nameof(MABK)); } }

        private int? _userid;
        public int? USERID { get => _userid; set { if (_userid == value) return; _userid = value; OnPropertyChanged(nameof(USERID)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }
    }
}
