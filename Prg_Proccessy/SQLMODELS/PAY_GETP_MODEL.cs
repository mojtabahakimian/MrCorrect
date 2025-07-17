using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PAY_GETP_MODEL : INotifyPropertyChanged, ICloneable
    {
        public PAY_GETP_MODEL()
        {
            N_KOL = (int?)Baseknow.BANKHA;
            KIND = 1;
            DATE = Convert.ToInt64(Tarikh.FullCurrentDate);

        }
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
        private int? _n_s;
        public int? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
        private int? _n_kol;
        public int? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }
        private int? _n_moin;
        public int? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }
        private int? _n_taf;
        public int? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }
        private int? _n_kol2;
        public int? N_KOL2 { get => _n_kol2; set { if (_n_kol2 == value) return; _n_kol2 = value; OnPropertyChanged("N_KOL2"); } }
        private int? _n_moin2;
        public int? N_MOIN2 { get => _n_moin2; set { if (_n_moin2 == value) return; _n_moin2 = value; OnPropertyChanged("N_MOIN2"); } }
        private int? _n_taf2;
        public int? N_TAF2 { get => _n_taf2; set { if (_n_taf2 == value) return; _n_taf2 = value; OnPropertyChanged("N_TAF2"); } }
        private int? _n_kol3;
        public int? N_KOL3 { get => _n_kol3; set { if (_n_kol3 == value) return; _n_kol3 = value; OnPropertyChanged("N_KOL3"); } }
        private int? _n_moin3;
        public int? N_MOIN3 { get => _n_moin3; set { if (_n_moin3 == value) return; _n_moin3 = value; OnPropertyChanged("N_MOIN3"); } }
        private int? _n_taf3;
        public int? N_TAF3 { get => _n_taf3; set { if (_n_taf3 == value) return; _n_taf3 = value; OnPropertyChanged("N_TAF3"); } }
        private int? _number;
        public int? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
        private int? _tag;
        public int? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }
        private double? _anbar;
        public double? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }
        private int? _radif;
        public int? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }
        private double? _cust_no;
        public double? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }
        private int? _kind;
        public int? KIND { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("KIND"); } }
        private double? _vaz;
        public double? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged("VAZ"); } }
        private string? _hes1;
        public string? HES1 { get => _hes1; set { if (_hes1 == value) return; _hes1 = value; OnPropertyChanged("HES1"); } }
        private string? _hes2;
        public string? HES2 { get => _hes2; set { if (_hes2 == value) return; _hes2 = value; OnPropertyChanged("HES2"); } }
        private string? _hes3;
        public string? HES3 { get => _hes3; set { if (_hes3 == value) return; _hes3 = value; OnPropertyChanged("HES3"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
        private string? _sayadi;
        public string? SAYADI { get => _sayadi; set { if (_sayadi == value) return; _sayadi = value; OnPropertyChanged("SAYADI"); } }



        private string? _n_kol_name;
        public string? N_KOL_NAME { get => _n_kol_name; set { if (_n_kol_name == value) return; _n_kol_name = value; OnPropertyChanged("N_KOL_NAME"); } }
        private string? _n_moin_name;
        public string? N_MOIN_NAME { get => _n_moin_name; set { if (_n_moin_name == value) return; _n_moin_name = value; OnPropertyChanged("N_MOIN_NAME"); } }
        private string? _n_taf_name;
        public string? N_TAF_NAME { get => _n_taf_name; set { if (_n_taf_name == value) return; _n_taf_name = value; OnPropertyChanged("N_TAF_NAME"); } }
        private string? _n_kol2_name;
        public string? N_KOL2_NAME { get => _n_kol2_name; set { if (_n_kol2_name == value) return; _n_kol2_name = value; OnPropertyChanged("N_KOL2_NAME"); } }
        private string? _n_moin2_name;
        public string? N_MOIN2_NAME { get => _n_moin2_name; set { if (_n_moin2_name == value) return; _n_moin2_name = value; OnPropertyChanged("N_MOIN2_NAME"); } }
        private string? _n_taf2_name;
        public string? N_TAF2_NAME { get => _n_taf2_name; set { if (_n_taf2_name == value) return; _n_taf2_name = value; OnPropertyChanged("N_TAF2_NAME"); } }
        private string? _n_kol3_name;
        public string? N_KOL3_NAME { get => _n_kol3_name; set { if (_n_kol3_name == value) return; _n_kol3_name = value; OnPropertyChanged("N_KOL3_NAME"); } }
        private string? _n_moin3_name;
        public string? N_MOIN3_NAME { get => _n_moin3_name; set { if (_n_moin3_name == value) return; _n_moin3_name = value; OnPropertyChanged("N_MOIN3_NAME"); } }
        private string? _n_taf3_name;
        public string? N_TAF3_NAME { get => _n_taf3_name; set { if (_n_taf3_name == value) return; _n_taf3_name = value; OnPropertyChanged("N_TAF3_NAME"); } }
        private string? _bank_name;
        public string? BANK_NAME { get => _bank_name; set { if (_bank_name == value) return; _bank_name = value; OnPropertyChanged("BANK_NAME"); } }


        private string? _hes1_name;
        public string? HES1_NAME { get => _hes1_name; set { if (_hes1_name == value) return; _hes1_name = value; OnPropertyChanged("HES1_NAME"); } }

        private string? _hes2_name;
        public string? HES2_NAME { get => _hes2_name; set { if (_hes2_name == value) return; _hes2_name = value; OnPropertyChanged("HES2_NAME"); } }

        private string? _hes3_name;
        public string? HES3_NAME { get => _hes3_name; set { if (_hes3_name == value) return; _hes3_name = value; OnPropertyChanged("HES3_NAME"); } }

    }
}
