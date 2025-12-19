using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class ZCHRBI_MODEL : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        private string? _dateN;
        public string? DATE_N { get => _dateN; set { if (_dateN == value) return; _dateN = value; OnPropertyChanged(nameof(DATE_N)); } }

        private double? _fld1;
        public double? FLD1 { get => _fld1; set { if (_fld1 == value) return; _fld1 = value; OnPropertyChanged(nameof(FLD1)); } }

        private double? _fld2;
        public double? FLD2 { get => _fld2; set { if (_fld2 == value) return; _fld2 = value; OnPropertyChanged(nameof(FLD2)); } }

        private double? _fld3;
        public double? FLD3 { get => _fld3; set { if (_fld3 == value) return; _fld3 = value; OnPropertyChanged(nameof(FLD3)); } }

        private double? _fld4;
        public double? FLD4 { get => _fld4; set { if (_fld4 == value) return; _fld4 = value; OnPropertyChanged(nameof(FLD4)); } }

        private double? _fld5;
        public double? FLD5 { get => _fld5; set { if (_fld5 == value) return; _fld5 = value; OnPropertyChanged(nameof(FLD5)); } }

        private double? _fld6;
        public double? FLD6 { get => _fld6; set { if (_fld6 == value) return; _fld6 = value; OnPropertyChanged(nameof(FLD6)); } }

        private double? _fld7;
        public double? FLD7 { get => _fld7; set { if (_fld7 == value) return; _fld7 = value; OnPropertyChanged(nameof(FLD7)); } }

        private double? _fld8;
        public double? FLD8 { get => _fld8; set { if (_fld8 == value) return; _fld8 = value; OnPropertyChanged(nameof(FLD8)); } }

        private double? _fld9;
        public double? FLD9 { get => _fld9; set { if (_fld9 == value) return; _fld9 = value; OnPropertyChanged(nameof(FLD9)); } }

        private double? _fld10;
        public double? FLD10 { get => _fld10; set { if (_fld10 == value) return; _fld10 = value; OnPropertyChanged(nameof(FLD10)); } }

        private double? _meghk;
        public double? MEGHK { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged(nameof(MEGHK)); } }

        private string? _custNo;
        public string? CUST_NO { get => _custNo; set { if (_custNo == value) return; _custNo = value; OnPropertyChanged(nameof(CUST_NO)); } }

        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(NAME)); } }

        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged(nameof(NUMBER)); } }

        private int? _tag;
        public int? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged(nameof(TAG)); } }

        private int? _mm;
        public int? mm { get => _mm; set { if (_mm == value) return; _mm = value; OnPropertyChanged(nameof(mm)); } }

        private int? _depatman;
        public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged(nameof(DEPATMAN)); } }

        private string? _depname;
        public string? DEPNAME { get => _depname; set { if (_depname == value) return; _depname = value; OnPropertyChanged(nameof(DEPNAME)); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged(nameof(CRT)); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged(nameof(UID)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
