using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class CHKE_DLIST : INotifyPropertyChanged, ICloneable
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
        private double? _n_seri;
        public double? N_SERI { get => _n_seri; set { if (_n_seri == value) return; _n_seri = value; OnPropertyChanged("N_SERI"); } }
        private int? _bank;
        public int? BANK { get => _bank; set { if (_bank == value) return; _bank = value; OnPropertyChanged("BANK"); } }
        private long? _date_s;
        public long? DATE_S { get => _date_s; set { if (_date_s == value) return; _date_s = value; OnPropertyChanged("DATE_S"); } }
        private long? _date;
        public long? DATE { get => _date; set { if (_date == value) return; _date = value; OnPropertyChanged("DATE"); } }
        private string? _shobeh;
        public string? SHOBEH { get => _shobeh; set { if (_shobeh == value) return; _shobeh = value; OnPropertyChanged("SHOBEH"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private string? _name_tah;
        public string? NAME_TAH { get => _name_tah; set { if (_name_tah == value) return; _name_tah = value; OnPropertyChanged("NAME_TAH"); } }
        private string? _n_hesab;
        public string? N_HESAB { get => _n_hesab; set { if (_n_hesab == value) return; _n_hesab = value; OnPropertyChanged("N_HESAB"); } }
        private double? _n_s;
        public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private int? _n_moin;
        public int? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }
        private int? _n_kol2;
        public int? N_KOL2 { get => _n_kol2; set { if (_n_kol2 == value) return; _n_kol2 = value; OnPropertyChanged("N_KOL2"); } }
        private int? _n_moin2;
        public int? N_MOIN2 { get => _n_moin2; set { if (_n_moin2 == value) return; _n_moin2 = value; OnPropertyChanged("N_MOIN2"); } }
        private int? _n_kol3;
        public int? N_KOL3 { get => _n_kol3; set { if (_n_kol3 == value) return; _n_kol3 = value; OnPropertyChanged("N_KOL3"); } }
        private int? _n_moin3;
        public int? N_MOIN3 { get => _n_moin3; set { if (_n_moin3 == value) return; _n_moin3 = value; OnPropertyChanged("N_MOIN3"); } }
        private double? _number;
        public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private double? _tag;
        public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }
        private double? _anbar;
        public double? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }
        private double? _radif;
        public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }
        private string? _cust_no;
        public string? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }
        private int? _vaz;
        public int? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged("VAZ"); } }
        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }
        private int? _n_taf;
        public int? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }
        private int? _n_taf2;
        public int? N_TAF2 { get => _n_taf2; set { if (_n_taf2 == value) return; _n_taf2 = value; OnPropertyChanged("N_TAF2"); } }
        private int? _n_taf3;
        public int? N_TAF3 { get => _n_taf3; set { if (_n_taf3 == value) return; _n_taf3 = value; OnPropertyChanged("N_TAF3"); } }
        private string? _name;
        public string? NAME { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged("NAME"); } }
        private int? _sandugh;
        public int? SANDUGH { get => _sandugh; set { if (_sandugh == value) return; _sandugh = value; OnPropertyChanged("SANDUGH"); } }
        private int? _shob_cod;
        public int? SHOB_COD { get => _shob_cod; set { if (_shob_cod == value) return; _shob_cod = value; OnPropertyChanged("SHOB_COD"); } }
        private int? _kind;
        public int? KIND { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("KIND"); } }
        private int? _listno;
        public int? LISTNO { get => _listno; set { if (_listno == value) return; _listno = value; OnPropertyChanged("LISTNO"); } }
        private string? _hes1;
        public string? HES1 { get => _hes1; set { if (_hes1 == value) return; _hes1 = value; OnPropertyChanged("HES1"); } }
        private string? _hes2;
        public string? HES2 { get => _hes2; set { if (_hes2 == value) return; _hes2 = value; OnPropertyChanged("HES2"); } }
        private string? _hes3;
        public string? HES3 { get => _hes3; set { if (_hes3 == value) return; _hes3 = value; OnPropertyChanged("HES3"); } }
        private long? _modat;
        public long? modat { get => _modat; set { if (_modat == value) return; _modat = value; OnPropertyChanged("modat"); } }
        private string? _estelam;
        public string? ESTELAM { get => _estelam; set { if (_estelam == value) return; _estelam = value; OnPropertyChanged("ESTELAM"); } }
        private int? _ds;
        public int? DS { get => _ds; set { if (_ds == value) return; _ds = value; OnPropertyChanged("DS"); } }
        private int? _ms;
        public int? MS { get => _ms; set { if (_ms == value) return; _ms = value; OnPropertyChanged("MS"); } }
        private int? _ys;
        public int? YS { get => _ys; set { if (_ys == value) return; _ys = value; OnPropertyChanged("YS"); } }
        private int? _dd;
        public int? DD { get => _dd; set { if (_dd == value) return; _dd = value; OnPropertyChanged("DD"); } }
        private int? _md;
        public int? MD { get => _md; set { if (_md == value) return; _md = value; OnPropertyChanged("MD"); } }
        private int? _yd;
        public int? YD { get => _yd; set { if (_yd == value) return; _yd = value; OnPropertyChanged("YD"); } }

        private string? _SAYADI;
        public string? SAYADI { get => _SAYADI; set { if (_SAYADI == value) return; _SAYADI = value; OnPropertyChanged("SAYADI"); } }
    }
}
