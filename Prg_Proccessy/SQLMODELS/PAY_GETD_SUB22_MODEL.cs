using Prg_Proccessy.FUNCTIONS;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class PAY_GETD_SUB22_MODEL : INotifyPropertyChanged, ICloneable, IDataErrorInfo
    {
        //فقط پس از تغییر کاربر، ولیدیشن اجرا شود
        private HashSet<string> touchedFields = new HashSet<string>();
        public PAY_GETD_SUB22_MODEL()
        {
            KIND = 1;
            DATE = Convert.ToInt64(Tarikh.FullCurrentDate);
            VAZ = 1;
            SANDUGH = 1;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string this[string columnName]
        {
            get
            {
                // فقط اگر کاربر این فیلد را لمس کرده ولیدیشن کن
                if (!touchedFields.Contains(columnName))
                    return string.Empty;

                switch (columnName)
                {
                    case nameof(N_SERI):
                        if (N_SERI == null || N_SERI <= 0)
                            return "شماره سریال الزامی است و باید عددی مثبت باشد.";
                        break;

                    case nameof(BANK):
                        if (BANK == null || BANK <= 0)
                            return "بانک الزامی است.";
                        break;

                    case nameof(SHOBEH):
                        if (!string.IsNullOrWhiteSpace(SHOBEH) && SHOBEH.Length > 20)
                            return "نام شعبه حداکثر باید ۲۰ کاراکتر باشد.";
                        break;

                    case nameof(MABL):
                        if (MABL == null || MABL <= 0)
                            return "مبلغ الزامی است و باید بیشتر از صفر باشد.";
                        break;

                    case nameof(NAME_TAH):
                        if (NAME_TAH?.Length > 200)
                            return "نام پرداخت کننده نباید بیش از ۲۰۰ کاراکتر باشد.";
                        break;

                    case nameof(RADIF):
                        if (RADIF != null && RADIF < 0)
                            return "ردیف دفتر باید مثبت باشد.";
                        break;

                    case nameof(VAZ):
                        if (VAZ == null || VAZ <= 0)
                            return "وضعیت چک الزامی است.";
                        break;

                    case nameof(SANDUGH):
                        if (SANDUGH == null || SANDUGH <= 0)
                            return "موقعیت چک الزامی است.";
                        break;

                    case nameof(SAYADI):
                        if (!string.IsNullOrWhiteSpace(SAYADI) && SAYADI.Length != 16)
                            return "شماره صیادی باید ۱۶ رقم باشد.";
                        break;


                }
                return string.Empty;
            }
        }
        public void SetTouched(string columnName)
        {
            if (!touchedFields.Contains(columnName))
                touchedFields.Add(columnName);
            OnPropertyChanged(columnName);
        }
        // Row-level validation
        public string Error
        {
            get
            {
                var errors = new List<string>();
                foreach (var prop in GetType().GetProperties())
                {
                    var error = this[prop.Name];
                    if (!string.IsNullOrEmpty(error))
                        errors.Add(error);
                }

                // Cross-field rules
                if (DATE != null && DATE_S != null && DATE_S < DATE)
                    errors.Add("تاریخ سررسید نمی‌تواند قبل از تاریخ دریافت باشد.");

                if (MABL == 0)
                    errors.Add("مبلغ نباید صفر باشد.");

                // Add more rules if necessary

                return string.Join(Environment.NewLine, errors);
            }
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

        private double? _vaz;
        public double? VAZ { get => _vaz; set { if (_vaz == value) return; _vaz = value; OnPropertyChanged("VAZ"); } }

        private int? _list_no;
        public int? LIST_NO { get => _list_no; set { if (_list_no == value) return; _list_no = value; OnPropertyChanged("LIST_NO"); } }

        private int? _kind;
        public int? KIND { get => _kind; set { if (_kind == value) return; _kind = value; OnPropertyChanged("KIND"); } }

        private int? _sandugh;
        public int? SANDUGH { get => _sandugh; set { if (_sandugh == value) return; _sandugh = value; OnPropertyChanged("SANDUGH"); } }

        private string? _hes1;
        public string? HES1 { get => _hes1; set { if (_hes1 == value) return; _hes1 = value; OnPropertyChanged("HES1"); } }

        private string? _hes2;
        public string? HES2 { get => _hes2; set { if (_hes2 == value) return; _hes2 = value; OnPropertyChanged("HES2"); } }

        private string? _hes3;
        public string? HES3 { get => _hes3; set { if (_hes3 == value) return; _hes3 = value; OnPropertyChanged("HES3"); } }

        private string? _estelam;
        public string? ESTELAM { get => _estelam; set { if (_estelam == value) return; _estelam = value; OnPropertyChanged("ESTELAM"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private string? _sayadi;
        public string? SAYADI { get => _sayadi; set { if (_sayadi == value) return; _sayadi = value; OnPropertyChanged("SAYADI"); } }

        private long? _id;
        public long? ID { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("ID"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }
    }
}
