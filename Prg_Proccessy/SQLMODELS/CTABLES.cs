using Prg_Proccessy.MODELS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class CTABLES
    {
        /// <summary>
        /// دقیقا همون جدول اس کیو ال منتها در حالت سی شارپ
        /// </summary>
        public class HEAD_LST_CSHARP
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public double? NUMBER1 { get; set; }
            public long? DATE_N { get; set; }
            public string TAH { get; set; }
            public double? MAS { get; set; }
            public double? VAS { get; set; }
            public double? N_S { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public double? M_NAGHD { get; set; }
            public double? MABL_VAR { get; set; }
            public string MOIN_VAR { get; set; }
            public double? MABL_HAV { get; set; }
            public string MOIN_HAV { get; set; }
            public double? MABL_HAZ { get; set; }
            public string MOIN_HAZ { get; set; }
            public double? TAKHFIF { get; set; }
            public string MOIN_KHF { get; set; }
            public int? ANBARF { get; set; }
            public double? FNUMCO { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public string SHARAYET { get; set; }
            public bool? SGN1 { get; set; }
            public bool? SGN2 { get; set; }
            public bool? SGN3 { get; set; }
            public bool? SGN4 { get; set; }
            public double? MBAA { get; set; }
            public string HMBAA { get; set; }
            public double? TAMIR { get; set; }
            public bool? TICMBAA { get; set; }
            public bool? TKHF { get; set; }
            public bool? OKF { get; set; }
            public byte SADER { get; set; }
            public double? ARZD { get; set; }
            public byte ARZKIND { get; set; }
            public long? CDDATE { get; set; }
            public int? CDTIME { get; set; }
            public long? OKDATE { get; set; }
            public int? OKTIME { get; set; }
            public bool? JAY { get; set; }
            public int? MODAT_PPID { get; set; }
            public int? PEPID { get; set; }
            public int? PEID { get; set; }
            public int? sgn1usid { get; set; }
            public int? sgn2usid { get; set; }
            public int? sgn3usid { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class DEPART_CSHARP
        {
            public int? DEPATMAN { get; set; }
            public string DEPNAME { get; set; }
            public int? IDD { get; set; }
            public string DEPART { get; set; }
        }
        public class S_USER_SALADTL : INotifyPropertyChanged, ICloneable
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
            private string? _sal_name;
            public string? SAL_NAME { get => _sal_name; set { if (_sal_name == value) return; _sal_name = value; OnPropertyChanged("SAL_NAME"); } }
            private string? _psal_name;
            public string? PSAL_NAME { get => _psal_name; set { if (_psal_name == value) return; _psal_name = value; OnPropertyChanged("PSAL_NAME"); } }
            private int? _grsal;
            public int? GRSAL { get => _grsal; set { if (_grsal == value) return; _grsal = value; OnPropertyChanged("GRSAL"); } }
            private byte _enabl;
            public byte ENABL { get => _enabl; set { if (_enabl == value) return; _enabl = value; OnPropertyChanged("ENABL"); } }
            private int? _idd;
            public int? IDD { get => _idd; set { if (_idd == value) return; _idd = value; OnPropertyChanged("IDD"); } }
            private string? _hes;
            public string? HES { get => _hes; set { if (_hes == value) return; _hes = value; OnPropertyChanged("HES"); } }
            private int? _porid;
            public int? PORID { get => _porid; set { if (_porid == value) return; _porid = value; OnPropertyChanged("PORID"); } }
            private byte[] _emza;
            public byte[] EMZA { get => _emza; set { if (_emza == value) return; _emza = value; OnPropertyChanged("EMZA"); } }
            private int? _menup;
            public int? menup { get => _menup; set { if (_menup == value) return; _menup = value; OnPropertyChanged("menup"); } }
            private DateTime? _crt;
            public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
            private int? _uid;
            public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
            private int? _erjabe;
            public int? erjabe { get => _erjabe; set { if (_erjabe == value) return; _erjabe = value; OnPropertyChanged("erjabe"); } }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged("IsSelected"); }
            }

        }

        public class HEAD_LST_LOG1
        {
            public DateTime? UP_DATE { get; set; }
            public double? NUMBER { get; set; }
            public double? TAGG { get; set; }
            public double? RESERVED { get; set; }
            public string UP_USER_NAME { get; set; }
            public int? IDD { get; set; }
            public string fieldname { get; set; }
            public long? UDATEF { get; set; }
        }
        public class PRICE_PAYNO_MODATP
        {
            public int PPID { get; set; }
            public string PPAME { get; set; }
            public int MODAT { get; set; }
            public bool IsTempyDisplay { get; set; } = false;
        }
        public class Custom_TCODANBAR
        {
            public int CODE { get; set; }
            public string NAMES { get; set; }
            public int USERCO { get; set; }
        }
        public class Custom_NameCode
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
        }
        public class Custom_DEPART
        {
            public Int32 DEPATMAN { get; set; }
            public string DEPNAME { get; set; }
        }
        public class Custom_CUST_HESAB
        {
            public string hes { get; set; }
            public string NAME { get; set; }
        }
        //INVO_LST
        /// <summary>
        /// CODE = انبار
        /// | CODEKALA = کد کالا
        /// | CODEVAHED = کد واحد کالا
        /// </summary>
        public class INVOMonitor
        {
            public double? NUMBER { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public string MANDAH { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public double? IMBAA { get; set; }
            public double? N_MOIN { get; set; }
            public double? N_KOL { get; set; }
            public Nullable<double> TKHN { get; set; }
            public int? VAHED_K { get; set; }
            public int? CODE { get; set; }
            public string NAMES { get; set; }
            public int? ANBAR { get; set; }
            public string CODEKALA { get; set; }
            public string NAME { get; set; }
            public int? CODEVAHED { get; set; }
            public string NAMEVAHED { get; set; }
            public double? RADIF { get; set; }
            public double? TAG { get; set; }
            public long? id { get; set; }
        }
        //For Search KALA + Enter
        public class SERCHK_STUFDEF
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
            public double? MABL_F { get; set; }
            public double? MOG { get; set; }
            public int? ANBAR { get; set; }
            public int? vahed { get; set; }
            public string N_FANI { get; set; }
            public string TOZIH { get; set; }
        }
        //ارسلا به کارتابل 
        public class NumTG
        {
            public long? num { get; set; }
            public long? tg { get; set; }
        }
        //تایید امضا
        public class Signer_20
        {
            public int? USERCO { get; set; }
            public bool FFRP_FROOSH { get; set; }
            public bool FFRP_ANB { get; set; }
            public bool FFRP_HESAB { get; set; }
            public bool FFRP_MODIR { get; set; }
        }
        public class Signer_1
        {
            public int? USERCO { get; set; }
            public bool ANB_RASID_ANB { get; set; }
            public bool ANB_RASID_QC { get; set; }
        }
        public class Signer_6
        {
            public int? USERCO { get; set; }
            public bool ENTGH_AZANB { get; set; }
            public bool ENTGH_BEANB { get; set; }
            public bool ENTGH_MODIR { get; set; }
        }
        public class Signer_13
        {
            public int? USERCO { get; set; }
            public bool FFR_FROOSH { get; set; }
            public bool FFR_HESAB { get; set; }
            public bool FFR_MODIR { get; set; }
        }
        public class Signer_12
        {
            public int? USERCO { get; set; }
            public bool KFR_BAZAR { get; set; }
            public bool KFR_HESAB { get; set; }
            public bool KFR_MODIR { get; set; }
        }
        public class Signer_2
        {
            public int? USERCO { get; set; }
            public bool ANB_HAVL_FROSH { get; set; }
            public bool ANB_HAVL_ANB { get; set; }
            public bool ANB_HAVL_MODIR { get; set; }
        }
        public class Signer_100
        {
            public int? USERCO { get; set; }
            public bool PAY_MVAHED { get; set; }
            public bool PAY_HESAB { get; set; }
            public bool PAY_MAMEL { get; set; }
        }
        //برای چاپ عادی پیش فاکتور
        public class jst_JAMTAF
        {
            public double? JAMF { get; set; }
            public double? TF { get; set; }
            public double? BAA { get; set; }
        }
        //برای نمایش تمام محتوایش HEAD LST برای مشاده پیش فاکتور ها
        public class ALLFACOTRHAVLE
        {
            public string CUSTKNAME { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public bool OKF { get; set; }
            public string MOLAH { get; set; }
            public double? MAS { get; set; }
            public string NAME { get; set; }
            public double? TAMIR { get; set; }

        }
        // برای چارت سازمانی در پیش فاکتور آن اوپن
        public class USERCHART_QRE
        {
            public string SAL_NAME { get; set; }
            public int? SUBUSERCO { get; set; }
            public int? USERCO { get; set; }

        }
        //برای نمایش سربرگ پیش فاکتور
        public class HEADPISHFACTOR20
        {
            public double? NUMBER { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public long? DATE_N { get; set; }
            public int? CUST_KIND { get; set; }
            public bool TICMBAA { get; set; }
            public double? TAMIR { get; set; }
            public bool? OKF { get; set; }
            public bool? SGN1 { get; set; }
            public bool? SGN2 { get; set; }
            public bool? SGN3 { get; set; }
            public double? MAS { get; set; }
            public int? MODAT_PPID { get; set; }
        }
        public class CHECKBLOCKCUST
        {
            public int? USERCO { get; set; }
            public string HES { get; set; }
        }
        public class ETEBAR_AZAE
        {
            public string HES { get; set; }
            public long? TOPETEB { get; set; }
            public bool? TOPASN { get; set; }
            public bool? ENDIS { get; set; }
            public long? TOPMEGH { get; set; }
            public float? JAMZARIB { get; set; }
            public float? EMTIAZ { get; set; }
            public int? gradeid { get; set; }
            public DateTime? DTTIM { get; set; }
        }
        public class AutomasionEVNT : INotifyPropertyChanged
        {
            //EVENT
            private int? _idnum;
            private int? _idd;
            private string _events;
            private int? _stdate;
            private int? _sttime;
            private string _username;
            private int? _company;
            private int? _sumtime;
            private byte[] _pic;
            private int? _skid;
            private long? _num;
            private long? _tg;
            private DateTime? _crt;
            private int? _uid;
            private string _fxtype;

            public int? IDNUM
            {
                get => _idnum;
                set
                {
                    if (_idnum == value) return;
                    _idnum = value;
                    OnPropertyChanged();
                }
            }

            public int? IDD
            {
                get => _idd;
                set
                {
                    if (_idd == value) return;
                    _idd = value;
                    OnPropertyChanged();
                }
            }

            public string EVENTS
            {
                get => _events;
                set
                {
                    if (_events == value) return;
                    _events = value;
                    OnPropertyChanged();
                }
            }

            public int? STDATE
            {
                get => _stdate;
                set
                {
                    if (_stdate == value) return;
                    _stdate = value;
                    OnPropertyChanged();
                }
            }

            public int? STTIME
            {
                get => _sttime;
                set
                {
                    if (_sttime == value) return;
                    _sttime = value;
                    OnPropertyChanged();
                }
            }

            public string USERNAME
            {
                get => _username;
                set
                {
                    if (_username == value) return;
                    _username = value;
                    OnPropertyChanged();
                }
            }

            public int? COMPANY
            {
                get => _company;
                set
                {
                    if (_company == value) return;
                    _company = value;
                    OnPropertyChanged();
                }
            }

            public int? SUMTIME
            {
                get => _sumtime;
                set
                {
                    if (_sumtime == value) return;
                    _sumtime = value;
                    OnPropertyChanged();
                }
            }

            public byte[] pic
            {
                get => _pic;
                set
                {
                    if (_pic == value) return;
                    _pic = value;
                    OnPropertyChanged();
                }
            }

            public int? skid
            {
                get => _skid;
                set
                {
                    if (_skid == value) return;
                    _skid = value;
                    OnPropertyChanged();
                }
            }

            public long? num
            {
                get => _num;
                set
                {
                    if (_num == value) return;
                    _num = value;
                    OnPropertyChanged();
                }
            }

            public string? FXTYPE
            {
                get => _fxtype;
                set
                {
                    if (_fxtype == value) return;
                    _fxtype = value;
                    OnPropertyChanged();
                }
            }

            public long? tg
            {
                get => _tg;
                set
                {
                    if (_tg == value) return;
                    _tg = value;
                    OnPropertyChanged();
                }
            }

            public DateTime? CRT
            {
                get => _crt;
                set
                {
                    if (_crt == value) return;
                    _crt = value;
                    OnPropertyChanged();
                }
            }

            public int? UID
            {
                get => _uid;
                set
                {
                    if (_uid == value) return;
                    _uid = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //SETSECURITY
        public class SECURITYQuery
        {
            public string CAPTION { get; set; }
            public Int16? kind { get; set; }
            public int? USERCO { get; set; }
            public bool RUN { get; set; }
            public bool SEE { get; set; }
            public bool INP { get; set; }
            public bool UPD { get; set; }
            public bool DEL { get; set; }
        }
        //بررسی وضعیت {
        public class LOGETEB_JAMinpish
        {
            public double? MAB { get; set; }
            public double? takh { get; set; }
            public double? arzesh { get; set; }
            public double MABL_HAZ { get; set; }
            public bool SGN1 { get; set; }
        }
        public class CHKENHES
        {
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string CODE { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public int? OSTANID { get; set; }
            public int? SHAHRID { get; set; }
            public string N_FANI { get; set; }
            public string col1 { get; set; }
            public string col2 { get; set; }
            public string col3 { get; set; }
            public string col4 { get; set; }
            public string col5 { get; set; }
            public string col6 { get; set; }
            public string col7 { get; set; }
            public string col8 { get; set; }
            public string col9 { get; set; }
            public string colN1 { get; set; }
            public string colN2 { get; set; }
            public string colN3 { get; set; }
            public string colN4 { get; set; }
            public string colN5 { get; set; }
            public string colN6 { get; set; }
            public string colN7 { get; set; }
            public string colN8 { get; set; }
            public string colN9 { get; set; }

        }
        public class ENHESAR_A1
        {
            public string EN_CUST_CO { get; set; }
            public int? EN_CITY { get; set; }
            public int? EN_IYALAT { get; set; }
            public int? EN_COUNTRY { get; set; }
            public int? EN_STDATE { get; set; }
            public int? EN_ENDEN { get; set; }
            public string EN_KALA_COD { get; set; }
            public int? EN_KALA_GR { get; set; }
            public string EN_GRCODE1 { get; set; }
            public string EN_GRCODE2 { get; set; }
            public string EN_GRCODE3 { get; set; }
            public string EN_GRCODE4 { get; set; }
            public string EN_GRCODE5 { get; set; }
            public string EN_GRCODE6 { get; set; }
            public string EN_GRCODE7 { get; set; }
            public string EN_GRCODE8 { get; set; }
            public string EN_GRCODE9 { get; set; }
            public string EN_GRCODE10 { get; set; }
            public int? USERID { get; set; }
            public DateTime EN_CDATE { get; set; }
        }
        public class ENSHESARMOSHTARI
        {
            public string EN_CUST_CO { get; set; }
            public int? EN_CITY { get; set; }
            public int? EN_IYALAT { get; set; }
            public int? EN_COUNTRY { get; set; }
        }
        public class GRADE_1
        {
            public int? GID { get; set; }
            public int? GSHARTID { get; set; }
            public string GSHNAME { get; set; }
            public string GSHFUNC { get; set; }
            public float? GSVALUE { get; set; }
            public float? GSVALUE2 { get; set; }
            public string TOZI { get; set; }
            public string TOZIH { get; set; }
        }
        public class FACT_1
        {
            public double? MBES { get; set; }
            public double? MBED { get; set; }
        }
        public class FACT_2
        {
            public double? BED { get; set; }
            public double? NO_S { get; set; }
        }
        public class GRADEQRE2
        {
            public int? GID { get; set; }
            public int? GSHARTID { get; set; }
            public string GSHNAME { get; set; }
            public string GSHFUNC { get; set; }
            public float? GSVALUE { get; set; }
            public float? GSVALUE2 { get; set; }
            public string TOZI { get; set; }
            public string TOZIH { get; set; }
        }
        public class GETFLD_1
        {
            public int? USERID { get; set; }
            public string FLD { get; set; }
            public string FRM { get; set; }
            public string VLU { get; set; }
        }
        public class GTGRDNAME
        {
            public int? GID { get; set; }
            public string GNAME { get; set; }
        }
        public class JAYMD
        {
            public int? VAHED_K { get; set; }
            public long? idd { get; set; }
            public string CODE { get; set; }
            public int? VAHED { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public long? JAY { get; set; }
            public int? JAYO { get; set; }
            public long? id { get; set; }
        }

        public class ThePart1
        {
            public int? ANBAR { get; set; }
            public string CODE { get; set; }
            public double? MEGHk { get; set; }
        }

        public class grade_pishfactor
        {
            public double? GMAB { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public double? TAMIR { get; set; }
        }
        public class tabdilhav_1
        {
            public int? VAHED_K { get; set; }
            public long? idd { get; set; }
            public string CODE { get; set; }
            public int? VAHED { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public long? JAY { get; set; }
            public int? JAYO { get; set; }
            public long? id { get; set; }
        }
        public class tabdilhav_2
        {
            public string CUST_NO { get; set; }
            public double? DARSAD { get; set; }
            public double? PURSANT { get; set; }
            public string TOZIH { get; set; }
            public int? PORID { get; set; }
        }
        public class MAINTASK
        {
            public int? IDNUM { get; set; }
            public string NAME { get; set; }
            public int? GR { get; set; }
            //public int? PERSONEL { get; set; } //↓
            public int? IDD { get; set; }
            public string TASK { get; set; }
            public int? PERIORITY { get; set; }
            public int? STATUS { get; set; }
            public int? STDATE { get; set; }
            public int? STTIME { get; set; }
            public int? ENDATE { get; set; }
            public int? ENTIME { get; set; }
            public string USERNAME { get; set; }
            //public string COMP_COD { get; set; } ↓
            public string hes { get; set; }
            public int? SUMTIME { get; set; }
            public byte[] pic { get; set; }
            public double? ss { get; set; }
            public int? skid { get; set; }
            public long? num { get; set; }
            public long? tg { get; set; }
            public DateTime? CTIM { get; set; }
            public int? USERCO { get; set; }
            public int? SEE { get; set; }
        }
        public class MAINTASK2 : INotifyPropertyChanged
        {
            private int? _IDNUM;
            private int? _GR;
            //private // int? _PERSONEL  //↓
            private int? _IDD;
            private string _TASK;
            private int? _PERIORITY;
            private int? _STATUS;
            private int? _STDATE;
            private int? _STTIME;
            private int? _ENDATE;
            private int? _ENTIME;
            private string _USERNAME;
            //private // string _COMP_COD  ↓
            private string _hes;
            private int? _SUMTIME;
            private byte[] _pic;
            private double? _ss;
            private int? _skid;
            private long? _num;
            private long? _tg;
            private DateTime? _CTIM;
            private int? _USERCO;
            private int? _SEE;
            public int? IDNUM
            {
                get { return _IDNUM; }
                set
                {
                    _IDNUM = value;
                    OnPropertyChanged("IDNUM");
                }
            }
            public int? GR
            {
                get { return _GR; }
                set
                {
                    _GR = value;
                    OnPropertyChanged("GR");
                }
            }
            //public int? PERSONEL { get; set; } //↓
            public int? IDD
            {
                get { return _IDD; }
                set
                {
                    _IDD = value;
                    OnPropertyChanged("IDD");
                }
            }
            public string TASK
            {
                get { return _TASK; }
                set
                {
                    _TASK = value;
                    OnPropertyChanged("TASK");
                }
            }
            public int? PERIORITY
            {
                get { return _PERIORITY; }
                set
                {
                    _PERIORITY = value;
                    OnPropertyChanged("PERIORITY");
                }
            }
            public int? STATUS
            {
                get { return _STATUS; }
                set
                {
                    _STATUS = value;
                    OnPropertyChanged("STATUS");
                }
            }
            public int? STDATE
            {
                get { return _STDATE; }
                set
                {
                    _STDATE = value;
                    OnPropertyChanged("STDATE");
                }
            }
            public int? STTIME
            {
                get { return _STTIME; }
                set
                {
                    _STTIME = value;
                    OnPropertyChanged("STTIME");
                }
            }
            public int? ENDATE
            {
                get { return _ENDATE; }
                set
                {
                    _ENDATE = value;
                    OnPropertyChanged("ENDATE");
                }
            }
            public int? ENTIME
            {
                get { return _ENTIME; }
                set
                {
                    _ENTIME = value;
                    OnPropertyChanged("ENTIME");
                }
            }
            public string USERNAME
            {
                get { return _USERNAME; }
                set
                {
                    _USERNAME = value;
                    OnPropertyChanged("USERNAME");
                }
            }
            //public string COMP_COD { get; set; } ↓
            public string hes
            {
                get { return _hes; }
                set
                {
                    _hes = value;
                    OnPropertyChanged("hes");
                }
            }
            public int? SUMTIME
            {
                get { return _SUMTIME; }
                set
                {
                    _SUMTIME = value;
                    OnPropertyChanged("SUMTIME");
                }
            }
            public byte[] pic
            {
                get { return _pic; }
                set
                {
                    _pic = value;
                    OnPropertyChanged("pic");
                }
            }
            public double? ss
            {
                get { return _ss; }
                set
                {
                    _ss = value;
                    OnPropertyChanged("ss");
                }
            }
            public int? skid
            {
                get { return _skid; }
                set
                {
                    _skid = value;
                    OnPropertyChanged("skid");
                }
            }
            public long? num
            {
                get { return _num; }
                set
                {
                    _num = value;
                    OnPropertyChanged("num");
                }
            }
            public long? tg
            {
                get { return _tg; }
                set
                {
                    _tg = value;
                    OnPropertyChanged("tg");
                }
            }
            public DateTime? CTIM
            {
                get { return _CTIM; }
                set
                {
                    _CTIM = value;
                    OnPropertyChanged("CTIM");
                }
            }
            public int? USERCO
            {
                get { return _USERCO; }
                set
                {
                    _USERCO = value;
                    OnPropertyChanged("USERCO");
                }
            }
            public int? SEE
            {
                get { return _SEE; }
                set
                {
                    _SEE = value;
                    OnPropertyChanged("SEE");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(string strCaller = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
            }
        }

        public class COMBOPERSONEL
        {
            public string? SAL_NAME { get; set; }
            public string? PSAL_NAME { get; set; }
            public int? GRSAL { get; set; }
            public byte ENABL { get; set; }
            public int? IDD { get; set; }
            public int? SUBUSERCO { get; set; }
            public int? USERCO { get; set; }

            // برای نمایش در کومبوباکس
            public override string ToString()
            {
                return SAL_NAME ?? string.Empty;
            }
        }

        // پیش فاکتور های مشتری :
        public class grade_pish_model
        {
            public double? GMAB { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public double? TAMIR { get; set; }
        }

        //حواله های مشتری:
        public class grade_havala_model
        {
            public double? GMAB { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public double? TAMIR { get; set; }
        }
        //پیش فاکتور های کارشناس :
        public class grade_pish_karsh_model
        {
            public double? GMAB { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public double? TAMIR { get; set; }
            public string NAME { get; set; }
            public string HES { get; set; }
        }

        //حواله های کارشناس :
        public class grade_havala_arsh_model
        {
            public double? GMAB { get; set; }
            public double? NUMBER { get; set; }
            public long? DATE_N { get; set; }
            public string CUST_NO { get; set; }
            public string MOLAH { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public double? TAMIR { get; set; }
            public string NAME { get; set; }
            public string ROUTE_NAME { get; set; }
        }

        //چکهای برگشتی :
        public class grade_chek_bargashti_model
        {
            public double? N_SERI { get; set; }
            public double? vaz { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public string SHOBEH { get; set; }
            public double? MABL { get; set; }
            public string NAME_TAH { get; set; }
            public string N_HESAB { get; set; }
            public string HES1 { get; set; }
            public string HES2 { get; set; }
            public string HES3 { get; set; }
            public double? VAZ { get; set; }
        }
        //کمبوباکس های پنجره بررسی وضعیت
        public class DEPATMAN_MODEL
        {
            public int? DEPATMAN { get; set; }
            public string DEPNAME { get; set; }
        }
        public class SHIFT_MODEL
        {
            public int? SHIFT_ID { get; set; }
            public string SHNAME { get; set; }
        }
        public class CUST_KIND_MODEL
        {
            public int? CUST_COD { get; set; }
            public string CUSTKNAME { get; set; }
        }
        public class BANK_MODEL
        {
            public int? CODE { get; set; }
            public string NAMES { get; set; }
        }
        public class HES1_MODEL
        {
            public string hes { get; set; }
            public string Expr1 { get; set; }
        }
        public class Q1
        {
            public int? IDNUM { get; set; }
            public long? num { get; set; }
            public long? tg { get; set; }
        }
        public class Q2
        {
            public int? USERCO { get; set; }
            public bool? FFRP_FROOSH { get; set; }
            public bool? FFRP_ANB { get; set; }
            public bool? FFRP_HESAB { get; set; }
            public bool? FFRP_MODIR { get; set; }
        }

        public class Q3
        {
            public int? USERCO { get; set; }
            public bool? ANB_RASID_ANB { get; set; }
            public bool? ANB_RASID_QC { get; set; }
            public int? F3 { get; set; }
        }
        public class Q4
        {
            public int? USERCO { get; set; }
            public bool? ENTGH_AZANB { get; set; }
            public bool? ENTGH_BEANB { get; set; }
            public bool? ENTGH_MODIR { get; set; }
        }
        public class Q5
        {
            public int? USERCO { get; set; }
            public bool? FFRB_FROOSH { get; set; }
            public bool? FFRB_ANB { get; set; }
            public bool? FFRB_HESAB { get; set; }
            public bool? FFRB_MODIR { get; set; }
        }
        public class Q6
        {
            public int? USERCO { get; set; }
            public bool? KFRB_BAZAR { get; set; }
            public bool? KFRB_ANB { get; set; }
            public bool? KFRB_HESAB { get; set; }
            public bool? KFRB_MODIR { get; set; }
        }
        public class Q7
        {
            public int? USERCO { get; set; }
            public bool? KFRB_BAZAR { get; set; }
            public bool? KFRB_ANB { get; set; }
            public bool? KFRB_HESAB { get; set; }
            public bool? KFRB_MODIR { get; set; }
        }
        public class Q8
        {
            public string CODE { get; set; }
            public double? MAND { get; set; }
            public double? ANBAR { get; set; }
            public string ANBARN { get; set; }
            public string NAME { get; set; }
            public string NAMES { get; set; }
            public long? VCOD { get; set; }
            public double? GRCOD { get; set; }
            public string GRNAME { get; set; }
            public double? MANDF { get; set; }
            public string N_FANI { get; set; }
            public double? NESBAT { get; set; }
            public double? MEGHBAR { get; set; }
            public double? bsef { get; set; }
            public double? nsef { get; set; }
            public double? maxm { get; set; }
            public double? minm { get; set; }
            public double? VAZN { get; set; }
            public double? VAZNK { get; set; }
            public string menuit { get; set; }
            public double? MABL_F { get; set; }
            public double? B_SEF { get; set; }
            public double? fisiclymand { get; set; }
            public double? MAX_M { get; set; }
            public double? MEGHRES { get; set; }
        }

        public class Q9
        {
            public double? NUMBER { get; set; }
            public double? Expr2 { get; set; }
        }
        public class Q10
        {
            public int? USERCO { get; set; }
            public bool? SGN0137 { get; set; }
            public bool? SGN0237 { get; set; }
            public bool? SGN0337 { get; set; }
        }
        public class QRE10
        {
            public int? BASE { get; set; }
            public double? N_S { get; set; }
            public long? DATE_S { get; set; }
            public double? NO_S { get; set; }
        }

        public class QRE11
        {
            public double? N_S { get; set; }
            public long? DATE_S { get; set; }
            public double? NO_S { get; set; }
        }
        public class QRE12
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
            public double AVRAGE { get; set; }
        }
        public class QRE13
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
        }
        public class _QRE14_
        {
            public int? USERCO { get; set; }
            public bool? KFR_BAZAR { get; set; }
            public bool? KFR_HESAB { get; set; }
            public bool? KFR_MODIR { get; set; }
        }
        public class _QRE15_
        {
            public int? USERCO { get; set; }
            public bool? SGN0136 { get; set; }
            public bool? SGN0236 { get; set; }
            public bool? SGN0336 { get; set; }
        }
        public class DETA_HES_MODEL1
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public string NAME { get; set; }
            public string TOZIH { get; set; }
            public double? BED_BES { get; set; }
            public string ADDRESS { get; set; }
            public string TEL { get; set; }
            public string CODE_E { get; set; }
            public int? USERCO { get; set; }
            public string USER_NAME { get; set; }
        }

        public class TDETA_HES_MODEL1
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public string NAME { get; set; }
            public string TOZIH { get; set; }
            public double? BED_BES { get; set; }
            public string ADDRESS { get; set; }
            public string TEL { get; set; }
            public string CODE_E { get; set; }
            public int? IDD { get; set; }
            public string ECODE { get; set; }
            public string PCODE { get; set; }
            public string IYALAT { get; set; }
            public string CITY { get; set; }
            public string MCODEM { get; set; }
            public int? CUST_COD { get; set; }
            public string MOBILE { get; set; }
            public string ROUTE_NAME { get; set; }
            public double? Longitude { get; set; }
            public double? Latitude { get; set; }
            public int? OSTANID { get; set; }
            public int? SHAHRID { get; set; }
            public int? USERCO { get; set; }
            public string USER_NAME { get; set; }
        }
        public class QRE14
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
            public double? AVRAGE { get; set; }
        }
        public class QRE15
        {
            public string CODE { get; set; }
            public int? FNUMB { get; set; }
        }
        public class HEAD_MANF_1
        {
            public double? SumOfMABLK { get; set; }
            public double? SumOfIMBIBE_MANF { get; set; }
            public double? SumOfIMBIBE_SAR { get; set; }
            public int? FNUMB { get; set; }
        }
        public class QRE16
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? CUST_CO { get; set; }
            public string TAKH_COD { get; set; }
            public short? TAFPER { get; set; }
            public double? MABL_K { get; set; }
        }
        public class QRE17
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? MABL_K { get; set; }
            public double? N_MOIN { get; set; }
            public string CODE { get; set; }
            public int? CUST_KIND { get; set; }
        }
        public class VISITOR_DTL_1
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string CUST_NO { get; set; }
            public double? DARSAD { get; set; }
            public double? PURSANT { get; set; }
            public string TOZIH { get; set; }
            public bool? STAT { get; set; }
            public int? PORID { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class QRE18
        {
            public string code { get; set; }
            public double? mablk { get; set; }
        }
        public class QRE19
        {
            public double? NUMBER { get; set; }
            public double? Expr2 { get; set; }
        }
        public class QRE20
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
            public double? RADAH { get; set; }
        }
        public class PAY_GETP_1
        {
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public string SHOBEH { get; set; }
            public double? MABL { get; set; }
            public string NAME_TAH { get; set; }
            public string N_HESAB { get; set; }
            public int? N_S { get; set; }
            public int? N_KOL { get; set; }
            public int? N_MOIN { get; set; }
            public int? N_TAF { get; set; }
            public int? N_KOL2 { get; set; }
            public int? N_MOIN2 { get; set; }
            public int? N_TAF2 { get; set; }
            public int? N_KOL3 { get; set; }
            public int? N_MOIN3 { get; set; }
            public int? N_TAF3 { get; set; }
            public int? NUMBER { get; set; }
            public int? TAG { get; set; }
            public double? ANBAR { get; set; }
            public int? RADIF { get; set; }
            public double? CUST_NO { get; set; }
            public int? KIND { get; set; }
            public double? VAZ { get; set; }
            public string HES1 { get; set; }
            public string HES2 { get; set; }
            public string HES3 { get; set; }
        }
        public partial class MOIN_CUSTOM
        {
            public double N_S { get; set; }
            public int @base { get; set; }
            public Nullable<long> DATE_S { get; set; }
            public int HES_K { get; set; }
            public int HES_M { get; set; }
            public int HES_T { get; set; }
            public Nullable<int> HES_T2 { get; set; }
            public string SHARH { get; set; }
            public Nullable<double> BED { get; set; }
            public Nullable<double> BES { get; set; }
            public Nullable<double> MAND { get; set; }
            public long id { get; set; }
            public double NO_S { get; set; }
            public Nullable<double> N_SERI { get; set; }
            public Nullable<int> BANK { get; set; }
            public Nullable<double> NUMBER { get; set; }
            public Nullable<double> TAG { get; set; }
            public Nullable<double> ARZD { get; set; }
            public Nullable<int> HES_T3 { get; set; }
            public Nullable<int> HES_T4 { get; set; }
            public string TAFZILN { get; set; }
            public string HES { get; set; }
            public string TASH { get; set; }
            public string TSH { get; set; }

            public int? MONTH_S { get; set; }
            public string MONTH_S_DISPLAY
            {
                get
                {
                    if (!MONTH_S.HasValue || MONTH_S < 1 || MONTH_S > 12)
                    {
                        return string.Empty;
                    }

                    var monthName = MONTH_S.Value switch
                    {
                        1 => "فروردین",
                        2 => "اردیبهشت",
                        3 => "خرداد",
                        4 => "تیر",
                        5 => "مرداد",
                        6 => "شهریور",
                        7 => "مهر",
                        8 => "آبان",
                        9 => "آذر",
                        10 => "دی",
                        11 => "بهمن",
                        12 => "اسفند",
                        _ => string.Empty
                    };

                    return $"{monthName} - {MONTH_S.Value:00}";
                }
            }
        }
        /// <summary>
        /// پیش فاکتور 90 30
        /// </summary>
        public class QRE_PISH_90_30
        {
            public double? MEGHT { get; set; }
            public double? MABL_K { get; set; }
            public int? DD { get; set; }
            public int? mm { get; set; }
            public long? DATE_N { get; set; }
        }
        /// <summary>
        /// پیش فاکتور 7
        /// </summary>
        public class QRE_PISH_7
        {
            public double? MEGHT { get; set; }
            public double? MABL_K { get; set; }
            public int? DD { get; set; }
            public long? DATE_N { get; set; }
        }

        //--------------------------------------------------------

        /// <summary>
        /// فاکتور فروش 90 و 30 روزه
        /// </summary>
        public class QRE_FR_90_30
        {
            public double? MEGHT { get; set; }
            public int? dd { get; set; }
            public int? mm { get; set; }
            public double? MABL_K { get; set; }
            public long? DATE_N { get; set; }
        }
        /// <summary>
        /// فاکتور فروش 7 روزه
        /// </summary>
        public class QRE_FR_7
        {
            public double? MEGHT { get; set; }
            public int? dd { get; set; }
            public double? MABL_K { get; set; }
            public long? DATE_N { get; set; }
        }
        public class TheSHIFT1
        {
            public int? SHIFT_ID { get; set; }
            public string SHNAME { get; set; }
        }
        public class TheMAGHSAD1
        {
            public int? MPCODE { get; set; }
            public string MPNAME { get; set; }
        }
        public class TheMAGHSAD1_1
        {
            public int? CITYCODE { get; set; }
            public string? Expr1 { get; set; }
        }
        public class CUSTOM_HEADLST2
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? tamir { get; set; }
        }
        public class OTHER_DTL_SUB_MONITOR : INotifyPropertyChanged, ICloneable , IEditableObject
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public OTHER_DTL_SUB_MONITOR()
            {
                NUMBER = 0;
                TAGG = 1;
                CODE = Baseknow.NAME_CODE.ToString();
                NAME_CODE = "";
                CAM_KHALY = 0;
                CAM_POOR = 0;
                MEGHk = 0;
                TOZIH = "";
                VAZNH = 0;
                RADIF = 0;
            }
            private double? _number;
            private double? _tagg;
            private string _code;
            private string _name_code;
            private double? _cam_khaly;
            private double? _cam_poor;
            private double? _meghk;
            private string _tozih;
            private double? _vaznh;
            private double? _radif;

            public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }
            public double? TAGG { get => _tagg; set { if (_tagg == value) return; _tagg = value; OnPropertyChanged("TAGG"); } }
            public string CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }
            public string NAME_CODE { get => _name_code; set { if (_name_code == value) return; _name_code = value; OnPropertyChanged("NAME_CODE"); } }
            public double? CAM_KHALY { get => _cam_khaly; set { if (_cam_khaly == value) return; _cam_khaly = value; OnPropertyChanged("CAM_KHALY"); } }
            public double? CAM_POOR { get => _cam_poor; set { if (_cam_poor == value) return; _cam_poor = value; OnPropertyChanged("CAM_POOR"); } }
            public double? MEGHk { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged("MEGHk"); } }
            public string TOZIH { get => _tozih; set { if (_tozih == value) return; _tozih = value; OnPropertyChanged("TOZIH"); } }
            public double? VAZNH { get => _vaznh; set { if (_vaznh == value) return; _vaznh = value; OnPropertyChanged("VAZNH"); } }
            public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

            private OTHER_DTL_SUB_MONITOR _backupCopy;
            private bool _inEdit = false;
            public void BeginEdit()
            {
                if (_inEdit) return;
                _backupCopy = (OTHER_DTL_SUB_MONITOR)this.Clone();
                _inEdit = true;
            }
            public void EndEdit()
            {
                _backupCopy = null;
                _inEdit = false;
            }
            public void CancelEdit()
            {
                if (!_inEdit || _backupCopy == null) return;

                NUMBER = _backupCopy.NUMBER;
                TAGG = _backupCopy.TAGG;
                CODE = _backupCopy.CODE;
                NAME_CODE = _backupCopy.NAME_CODE;
                CAM_KHALY = _backupCopy.CAM_KHALY;
                CAM_POOR = _backupCopy.CAM_POOR;
                MEGHk = _backupCopy.MEGHk;
                TOZIH = _backupCopy.TOZIH;
                VAZNH = _backupCopy.VAZNH;
                RADIF = _backupCopy.RADIF;

                _inEdit = false;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //INVO_LST FOR HAVALE TAG = 2
        /// <summary>
        /// CODE = انبار
        /// | CODEKALA = کد کالا
        /// | CODEVAHED = کد واحد کالا
        /// </summary>
            //INVO_LST FOR HAVALE TAG = 2
        /// <summary>
        /// CODE = انبار
        /// | CODEKALA = کد کالا
        /// | CODEVAHED = کد واحد کالا
        /// </summary>
        private class INVO_HAVAL_MONITOR : INotifyPropertyChanged, ICloneable
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public INVO_HAVAL_MONITOR()
            {
                CODE = Baseknow.anbardef;
                MEGH = 0;
                MEGHk = 0;
                MEGH_MAR = 0;
                MEGH_R = 0;
                MABL = 0;
                MABL_K = 0;
                IMBAA = 0;
                N_KOL = 0;
                N_MOIN = 0;
                TKHN = 0;
                AVRAGE = 0;
                AVRAGE2 = 0;
                SANAD_NO = 0;
                ANBARF = 0;
                N_TAF = 0;
                TAG = 2;
                TOTALARZ = 0;
                JAY = 0;
                JAYO = 0;
                FROM_A = false;
            }

            private double? _number;
            public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

            private double? _megh;
            public double? MEGH { get => _megh; set { if (_megh == value) return; _megh = value; OnPropertyChanged("MEGH"); } }

            private double? _meghk;
            public double? MEGHk { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged("MEGHk"); } }

            private string _mandah;
            public string MANDAH { get => _mandah; set { _mandah = value; OnPropertyChanged("MANDAH"); } }

            private double? _mabl;
            public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }

            private double? _mabl_k;
            public double? MABL_K { get => _mabl_k; set { if (_mabl_k == value) return; _mabl_k = value; OnPropertyChanged("MABL_K"); } }

            private double? _imbaa;
            public double? IMBAA { get => _imbaa; set { if (_imbaa == value) return; _imbaa = value; OnPropertyChanged("IMBAA"); } }

            private double? _n_moin;
            public double? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }

            private double? _n_kol;
            public double? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }

            private int? _vahed_k;
            public int? VAHED_K { get => _vahed_k; set { if (_vahed_k == value) return; _vahed_k = value; OnPropertyChanged("VAHED_K"); } }

            private int? _code;
            public int? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

            private string _names;
            public string NAMES { get => _names; set { _names = value; OnPropertyChanged("NAMES"); } }

            private int? _anbar;
            public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }

            private string _codekala;
            public string CODEKALA { get => _codekala; set { _codekala = value; OnPropertyChanged("CODEKALA"); } }

            private string _name;
            public string NAME { get => _name; set { _name = value; OnPropertyChanged("NAME"); } }

            private int? _codevahed;
            public int? CODEVAHED { get => _codevahed; set { if (_codevahed == value) return; _codevahed = value; OnPropertyChanged("CODEVAHED"); } }

            private string _namevahed;
            public string NAMEVAHED { get => _namevahed; set { _namevahed = value; OnPropertyChanged("NAMEVAHED"); } }

            private double? _radif;
            public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

            private double? _tag;
            public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }

            private double? _tkhn;
            public double? TKHN { get => _tkhn; set { if (_tkhn == value) return; _tkhn = value; OnPropertyChanged("TKHN"); } }

            private long? _id;
            public long? id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("id"); } }

            private double? _megh_mar;
            public double? MEGH_MAR { get => _megh_mar; set { if (_megh_mar == value) return; _megh_mar = value; OnPropertyChanged("MEGH_MAR"); } }

            private bool? _from_a;
            public bool? FROM_A { get => _from_a; set { if (_from_a == value) return; _from_a = value; OnPropertyChanged("FROM_A"); } }

            private double? _megh_r;
            public double? MEGH_R { get => _megh_r; set { if (_megh_r == value) return; _megh_r = value; OnPropertyChanged("MEGH_R"); } }

            private string _n_rasid;
            public string N_RASID { get => _n_rasid; set { _n_rasid = value; OnPropertyChanged("N_RASID"); } }

            private double? _sanad_no;
            public double? SANAD_NO { get => _sanad_no; set { if (_sanad_no == value) return; _sanad_no = value; OnPropertyChanged("SANAD_NO"); } }

            private double? _radah;
            public double? RADAH { get => _radah; set { if (_radah == value) return; _radah = value; OnPropertyChanged("RADAH"); } }

            private double? _cust_no;
            public double? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }

            private double? _anbarf;
            public double? ANBARF { get => _anbarf; set { if (_anbarf == value) return; _anbarf = value; OnPropertyChanged("ANBARF"); } }

            private double? _n_taf;
            public double? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }

            private double? _avrage;
            public double? AVRAGE { get => _avrage; set { if (_avrage == value) return; _avrage = value; OnPropertyChanged("AVRAGE"); } }

            private double? _avrage2;
            public double? AVRAGE2 { get => _avrage2; set { if (_avrage2 == value) return; _avrage2 = value; OnPropertyChanged("AVRAGE2"); } }

            private long? _jay;
            public long? JAY { get => _jay; set { if (_jay == value) return; _jay = value; OnPropertyChanged("JAY"); } }

            private int? _jayo;
            public int? JAYO { get => _jayo; set { if (_jayo == value) return; _jayo = value; OnPropertyChanged("JAYO"); } }

            private double? _totalarz;
            public double? TOTALARZ { get => _totalarz; set { if (_totalarz == value) return; _totalarz = value; OnPropertyChanged("TOTALARZ"); } }

            private string _visitor;
            public string VISITOR { get => _visitor; set { _visitor = value; OnPropertyChanged("VISITOR"); } }





            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class QRE_18
        {
            public double? SumOfMABLK { get; set; }
            public double? SumOfIMBIBE_MANF { get; set; }
            public double? SumOfIMBIBE_SAR { get; set; }
            public int? FNUMB { get; set; }
        }



        public class QRE_19
        {
            public double? ANBAR { get; set; }
            public string CODE { get; set; }
            public long? DATE_N { get; set; }
            public double? avrage { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
        }
        public class QRE_20
        {
            public double? AVRAGE2 { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public double? MNN { get; set; }
        }
        public class QRE_21
        {
            public int? USERCO { get; set; }
            public bool? ANB_HAVL_FROSH { get; set; }
            public bool? ANB_HAVL_ANB { get; set; }
            public bool? ANB_HAVL_MODIR { get; set; }
        }
        public class QRE_22
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public byte NUM { get; set; }
            public string USER_NAME { get; set; }
            public int? IDD { get; set; }
        }

        public class CUSTOM_STUF_DEF_2
        {
            public bool? CMBAA { get; set; }
            public string code { get; set; }
        }
        public class VAHED_K_NESBAT_2
        {
            public string CODE { get; set; }
            public int? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }
        public class NST_DISKET_BIMEH_QRE
        {
            public int? CODE { get; set; }
            public string PNAME { get; set; }
            public string PFAMILY { get; set; }
            public string PNPF { get; set; }
            public string KHNOWNUM { get; set; }
            public string FATHER { get; set; }
            public double? BIMEH_NUM { get; set; }
            public string JOB { get; set; }
            public bool? SEX { get; set; }
            public double? WSDAT { get; set; }
            public long? WEDATE { get; set; }
            public long? WDATE { get; set; }
            public int? DAYS { get; set; }
            public double? SALARY_DAYLY { get; set; }
            public double? mosalary { get; set; }
            public int? hom { get; set; }
            public double? zereshk { get; set; }
            public double? jmazsalmash { get; set; }
            public double? jamazsalmash { get; set; }
            public double? bimper { get; set; }
            public double? ghabel { get; set; }
            public int? chid { get; set; }
            public int? CONDITIONS { get; set; }
            public int? JAZB { get; set; }
            public int? SAYER { get; set; }
            public int? EZAFAH { get; set; }
            public int? PADASH { get; set; }
            public int? KASR_VAM { get; set; }
            public int? MM { get; set; }
            public int? bimper2 { get; set; }
            public double? JAMMAHS20 { get; set; }
            public string DSW_IDPLC { get; set; }
            public string DSW_NAT { get; set; }
            public long? DSW_BDATE { get; set; }
            public long? MaxOfHDATE { get; set; }
            public double? MAZMASH { get; set; }
            public int? mali { get; set; }
            public bool? TAB56 { get; set; }
        }
        public class NST_DISKET_BIMEH_QRE2
        {
            public int? CODE { get; set; }
            public string PNAME { get; set; }
            public string PFAMILY { get; set; }
            public string PNPF { get; set; }
            public string KHNOWNUM { get; set; }
            public string FATHER { get; set; }
            public double? BIMEH_NUM { get; set; }
            public string JOB { get; set; }
            public bool? SEX { get; set; }
            public double? WSDAT { get; set; }
            public long? WEDATE { get; set; }
            public long? WDATE { get; set; }
            public int? DAYS { get; set; }
            public double? SALARY_DAYLY { get; set; }
            public double? mosalary { get; set; }
            public int? hom { get; set; }
            public double? jmazsalmash { get; set; }
            public double? jamazsalmash { get; set; }
            public double? bimper { get; set; }
            public double? ghabel { get; set; }
            public int? chid { get; set; }
            public int? CONDITIONS { get; set; }
            public int? JAZB { get; set; }
            public int? SAYER { get; set; }
            public int? EZAFAH { get; set; }
            public int? PADASH { get; set; }
            public int? KASR_VAM { get; set; }
            public int? MM { get; set; }
            public int? bimper2 { get; set; }
            public double? JAMMAHS20 { get; set; }
            public string DSW_IDPLC { get; set; }
            public string DSW_NAT { get; set; }
            public long? DSW_BDATE { get; set; }
            public long? MaxOfHDATE { get; set; }
            public double? MAZMASH { get; set; }
            public int? mali { get; set; }
            public bool? TAB56 { get; set; }
        }
        public class LIST_SALARY22_10_CHSARP
        {
            public int? CODE { get; set; }
            public string PNAME { get; set; }
            public string PFAMILY { get; set; }
            public string PNPF { get; set; }
            public string KHNOWNUM { get; set; }
            public string FATHER { get; set; }
            public double? BIMEH_NUM { get; set; }
            public string JOB { get; set; }
            public bool? SEX { get; set; }
            public double? WSDAT { get; set; }
            public long? WEDATE { get; set; }
            public long? WDATE { get; set; }
            public int? DAYS { get; set; }
            public double? SALARY_DAYLY { get; set; }
            public double? mosalary { get; set; }
            public int? hom { get; set; }
            public double? jmazsalmash { get; set; }
            public double? jamazsalmash { get; set; }
            public double? bimper { get; set; }
            public double? ghabel { get; set; }
            public int? chid { get; set; }
            public int? CONDITIONS { get; set; }
            public int? JAZB { get; set; }
            public int? SAYER { get; set; }
            public int? EZAFAH { get; set; }
            public int? PADASH { get; set; }
            public int? KASR_VAM { get; set; }
            public int? MM { get; set; }
            public int? bimper2 { get; set; }
            public double? JAMMAHS20 { get; set; }
            public string DSW_IDPLC { get; set; }
            public string DSW_NAT { get; set; }
            public long? DSW_BDATE { get; set; }
            public long? MaxOfHDATE { get; set; }
            public double? MAZMASH { get; set; }
            public int? mali { get; set; }
            public bool? TAB56 { get; set; }
            public string MELLICOD { get; set; }
        }
        public class LIST_SALARY22TOFILE_10_CSHARP
        {
            public int? DSK_NUM { get; set; }
            public int? DSK_TDD { get; set; }
            public double? DSK_TROOZ { get; set; }
            public double? DSK_TMAH { get; set; }
            public double? DSK_TMAZ { get; set; }
            public double? DSK_TTOTL { get; set; }
            public double? DSK_TBIME { get; set; }
            public double? DSK_TKOSO { get; set; }
            public double? DSK_BIC { get; set; }
            public long? WDATE { get; set; }
            public double? DSK_TMASH { get; set; }
        }

        public class LIST_SALARY22_CHSARP1_1
        {
            public int? CODE { get; set; }
            public string PNAME { get; set; }
            public string PFAMILY { get; set; }
            public string PNPF { get; set; }
            public string KHNOWNUM { get; set; }
            public string FATHER { get; set; }
            public double? BIMEH_NUM { get; set; }
            public string JOB { get; set; }
            public bool? SEX { get; set; }
            public double? WSDAT { get; set; }
            public long? WEDATE { get; set; }
            public long? WDATE { get; set; }
            public int? DAYS { get; set; }
            public double? SALARY_DAYLY { get; set; }
            public double? mosalary { get; set; }
            public int? hom { get; set; }
            public double? jmazsalmash { get; set; }
            public double? jamazsalmash { get; set; }
            public double? bimper { get; set; }
            public double? ghabel { get; set; }
            public int? chid { get; set; }
            public int? CONDITIONS { get; set; }
            public int? JAZB { get; set; }
            public int? SAYER { get; set; }
            public int? EZAFAH { get; set; }
            public int? PADASH { get; set; }
            public int? KASR_VAM { get; set; }
            public int? MM { get; set; }
            public int? bimper2 { get; set; }
            public double? JAMMAHS20 { get; set; }
            public string DSW_IDPLC { get; set; }
            public string DSW_NAT { get; set; }
            public long? DSW_BDATE { get; set; }
            public long? MaxOfHDATE { get; set; }
            public double? MAZMASH { get; set; }
            public int? mali { get; set; }
            public bool? TAB56 { get; set; }
            public int? MALIAT { get; set; }
            public string MELLICOD { get; set; }
        }

        public class DEED_HED_CSHARP
        {
            public double? N_S { get; set; }
            public long? DATE_S { get; set; }
            public string SHARH_S { get; set; }
            public double? NO_S { get; set; }
            public double? ANBAR { get; set; }
            public double? N_FACTOR { get; set; }
            public bool? GHATEI { get; set; }
            public string USER_NAME { get; set; }
            public int @base { get; set; }
            public bool? SGN1 { get; set; }
            public bool? SGN2 { get; set; }
            public bool? SGN3 { get; set; }
            public bool? SGN4 { get; set; }
            public bool? OKF { get; set; }
            public int? sgn1usid { get; set; }
            public int? sgn2usid { get; set; }
            public int? sgn3usid { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class SETSECURITYSUB_CSHARP
        {
            public string CAPTION { get; set; }
            public short? kind { get; set; }
            public int? USERCO { get; set; }
            public bool? RUN { get; set; }
            public bool? SEE { get; set; }
            public bool? INP { get; set; }
            public bool? UPD { get; set; }
            public bool? DEL { get; set; }
        }
        public class TAKHFIF_APLAY_CSHARP
        {
            public int? TID { get; set; }
            public double? NUMBER { get; set; }
            public double? KIND { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class SARST_CSHARP
        {
            public int @BASE { get; set; }
            public double? N_S { get; set; }
            public long? DATE_S { get; set; }
            public double? NO_S { get; set; }
        }
        public class PAY_GETD_CSHARP
        {
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public string SHOBEH { get; set; }
            public double? MABL { get; set; }
            public string NAME_TAH { get; set; }
            public string N_HESAB { get; set; }
            public double? N_S { get; set; }
            public int? N_KOL { get; set; }
            public int? N_MOIN { get; set; }
            public int? N_TAF { get; set; }
            public int? N_KOL2 { get; set; }
            public int? N_MOIN2 { get; set; }
            public int? N_TAF2 { get; set; }
            public int? N_KOL3 { get; set; }
            public int? N_MOIN3 { get; set; }
            public int? N_TAF3 { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public string CUST_NO { get; set; }
            public double? VAZ { get; set; }
            public int? LIST_NO { get; set; }
            public int? KIND { get; set; }
            public int? SANDUGH { get; set; }
            public string HES1 { get; set; }
            public string HES2 { get; set; }
            public string HES3 { get; set; }
            public string ESTELAM { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class INVO_LST_CSHARP
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public string CODE { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public string MANDAH { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public bool? FROM_A { get; set; }
            public string N_RASID { get; set; }
            public double? MEGH_R { get; set; }
            public double? RADAH { get; set; }
            public double? SANAD_NO { get; set; }
            public double? CUST_NO { get; set; }
            public double? ANBARF { get; set; }
            public int? VAHED_K { get; set; }
            public double? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
            public double? N_TAF { get; set; }
            public double? AVRAGE { get; set; }
            public long? id { get; set; }
            public double? AVRAGE2 { get; set; }
            public double? IMBAA { get; set; }
            public double? TOTALARZ { get; set; }
            public string VISITOR { get; set; }
            public double? TKHN { get; set; }
            public long? JAY { get; set; }
            public int? JAYO { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class STUF_DEF_CSHARP
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
            public string N_FANI { get; set; }
            public string TOZIH { get; set; }
            public int? VAHED { get; set; }
            public double? B_SEF { get; set; }
            public double? N_SEF { get; set; }
            public double? MIN_M { get; set; }
            public double? MAX_M { get; set; }
            public double? RADAH { get; set; }
            public short? KINDK { get; set; }
            public double? MABL_F { get; set; }
            public int? DEPART { get; set; }
            public int? IDD { get; set; }
            public bool? CMBAA { get; set; }
            public double? VAZN { get; set; }
            public bool? OKF { get; set; }
            public double? MENUIT { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public int? PGID { get; set; }
            public string BARCODE { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class TAKHPERS_CSHARP
        {
            public int? CUST_CO { get; set; }
            public string TAKH_COD { get; set; }
            public short? TAFPER { get; set; }
            public int? PRICE_M { get; set; }
        }
        public class PRICELIST_CSHARP
        {
            public int PEPID { get; set; }
            public string PEPNAME { get; set; }
            public int PEPDATE { get; set; }
            public Nullable<int> PEPDEPART { get; set; }
        }
        public class PRICELIST_ETF_TAKHFIF__CSHARP
        {
            public int PEID { get; set; }
            public string PENAME { get; set; }
            public int PEDATE { get; set; }
            public Nullable<int> PEPDEPART { get; set; }
        }

        /// <summary>
        /// برای دیتاگرید فاکتور فروش مستقیم شماره 22
        /// </summary>
        public class INVO_LST_FACTOR22 : INotifyPropertyChanged, ICloneable, IEditableObject
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public INVO_LST_FACTOR22()
            {
                ANBAR = Baseknow.anbardef;
                MEGH = 0;
                MEGHk = 0;
                MEGH_MAR = 0;
                MEGH_R = 0;
                MABL = 0;
                MABL_K = 0;
                IMBAA = 0;
                N_KOL = 0;
                N_MOIN = 0;
                TKHN = 0;
                AVRAGE = 0;
                AVRAGE2 = 0;
                SANAD_NO = 0;
                ANBARF = 0;
                N_TAF = 0;
                //TAG = 2;
                TOTALARZ = 0;
                JAY = 0; //#Check
                //JAYO = 0;
                FROM_A = false;
            }

            private INVO_LST_FACTOR22 _backupCopy;
            private bool _inEdit = false;
            public void BeginEdit()
            {
                if (_inEdit) return;
                _backupCopy = (INVO_LST_FACTOR22)this.Clone();
                _inEdit = true;
            }
            public void EndEdit()
            {
                _backupCopy = null;
                _inEdit = false;
            }
            public void CancelEdit()
            {
                if (!_inEdit || _backupCopy == null) return;

                NUMBER = _backupCopy.NUMBER;
                TAG = _backupCopy.TAG;
                ANBAR = _backupCopy.ANBAR;
                RADIF = _backupCopy.RADIF;
                CODE = _backupCopy.CODE;
                NAME_CODE = _backupCopy.NAME_CODE;
                MEGH = _backupCopy.MEGH;
                MEGHk = _backupCopy.MEGHk;
                MEGH_MAR = _backupCopy.MEGH_MAR;
                MANDAH = _backupCopy.MANDAH;
                MABL = _backupCopy.MABL;
                MABL_K = _backupCopy.MABL_K;
                MABMAR = _backupCopy.MABMAR;
                FROM_A = _backupCopy.FROM_A;
                N_RASID = _backupCopy.N_RASID;
                MEGH_R = _backupCopy.MEGH_R;
                RADAH = _backupCopy.RADAH;
                SANAD_NO = _backupCopy.SANAD_NO;
                CUST_NO = _backupCopy.CUST_NO;
                ANBARF = _backupCopy.ANBARF;
                VAHED_K = _backupCopy.VAHED_K;
                NAME_VAHED_K = _backupCopy.NAME_VAHED_K;
                N_KOL = _backupCopy.N_KOL;
                N_MOIN = _backupCopy.N_MOIN;
                N_TAF = _backupCopy.N_TAF;
                AVRAGE = _backupCopy.AVRAGE;
                id = _backupCopy.id;
                AVRAGE2 = _backupCopy.AVRAGE2;
                IMBAA = _backupCopy.IMBAA;
                TOTALARZ = _backupCopy.TOTALARZ;
                VISITOR = _backupCopy.VISITOR;
                TKHN = _backupCopy.TKHN;
                JAY = _backupCopy.JAY;
                JAYO = _backupCopy.JAYO;
                CRT = _backupCopy.CRT;
                UID = _backupCopy.UID;
                CODEO = _backupCopy.CODEO;

                _inEdit = false;
            }


            private double? _number;
            public double? NUMBER { get => _number; set { if (_number == value) return; _number = value; OnPropertyChanged("NUMBER"); } }

            private double? _tag;
            public double? TAG { get => _tag; set { if (_tag == value) return; _tag = value; OnPropertyChanged("TAG"); } }

            private int? _anbar;
            public int? ANBAR { get => _anbar; set { if (_anbar == value) return; _anbar = value; OnPropertyChanged("ANBAR"); } }

            private double? _radif;
            public double? RADIF { get => _radif; set { if (_radif == value) return; _radif = value; OnPropertyChanged("RADIF"); } }

            private string _code;
            public string CODE { get => _code; set { _code = value; OnPropertyChanged("CODE"); } }

            private string _name_code;
            public string NAME_CODE { get => _name_code; set { _name_code = value; OnPropertyChanged("NAME_CODE"); } }

            private double? _megh;
            public double? MEGH { get => _megh; set { if (_megh == value) return; _megh = value; OnPropertyChanged("MEGH"); } }

            private double? _meghk;
            public double? MEGHk { get => _meghk; set { if (_meghk == value) return; _meghk = value; OnPropertyChanged("MEGHk"); } }

            private double? _megh_mar;
            public double? MEGH_MAR { get => _megh_mar; set { if (_megh_mar == value) return; _megh_mar = value; OnPropertyChanged("MEGH_MAR"); } }

            private string _mandah;
            public string MANDAH { get => _mandah; set { _mandah = value; OnPropertyChanged("MANDAH"); } }

            private double? _mabl;
            public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }

            private double? _mabl_k;
            public double? MABL_K { get => _mabl_k; set { if (_mabl_k == value) return; _mabl_k = value; OnPropertyChanged("MABL_K"); } }

            private double? _mabmar;
            public double? MABMAR { get => _mabmar; set { if (_mabmar == value) return; _mabmar = value; OnPropertyChanged("MABMAR"); } }

            private bool? _from_a;
            public bool? FROM_A { get => _from_a; set { if (_from_a == value) return; _from_a = value; OnPropertyChanged("FROM_A"); } }

            private string _n_rasid;
            public string N_RASID { get => _n_rasid; set { _n_rasid = value; OnPropertyChanged("N_RASID"); } }

            private double? _megh_r;
            public double? MEGH_R { get => _megh_r; set { if (_megh_r == value) return; _megh_r = value; OnPropertyChanged("MEGH_R"); } }

            private double? _radah;
            public double? RADAH { get => _radah; set { if (_radah == value) return; _radah = value; OnPropertyChanged("RADAH"); } }

            private double? _sanad_no;
            public double? SANAD_NO { get => _sanad_no; set { if (_sanad_no == value) return; _sanad_no = value; OnPropertyChanged("SANAD_NO"); } }

            private double? _cust_no;
            public double? CUST_NO { get => _cust_no; set { if (_cust_no == value) return; _cust_no = value; OnPropertyChanged("CUST_NO"); } }

            private double? _anbarf;
            public double? ANBARF { get => _anbarf; set { if (_anbarf == value) return; _anbarf = value; OnPropertyChanged("ANBARF"); } }

            private int? _vahed_k;
            public int? VAHED_K { get => _vahed_k; set { if (_vahed_k == value) return; _vahed_k = value; OnPropertyChanged("VAHED_K"); } }

            private string _name_vahed_k;
            public string NAME_VAHED_K { get => _name_vahed_k; set { _name_vahed_k = value; OnPropertyChanged("NAME_VAHED_K"); } }

            private double? _n_kol;
            public double? N_KOL { get => _n_kol; set { if (_n_kol == value) return; _n_kol = value; OnPropertyChanged("N_KOL"); } }

            private double? _n_moin;
            public double? N_MOIN { get => _n_moin; set { if (_n_moin == value) return; _n_moin = value; OnPropertyChanged("N_MOIN"); } }

            private double? _n_taf;
            public double? N_TAF { get => _n_taf; set { if (_n_taf == value) return; _n_taf = value; OnPropertyChanged("N_TAF"); } }

            private double? _avrage;
            public double? AVRAGE { get => _avrage; set { if (_avrage == value) return; _avrage = value; OnPropertyChanged("AVRAGE"); } }

            private long? _id;
            public long? id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged("id"); } }

            private double? _avrage2;
            public double? AVRAGE2 { get => _avrage2; set { if (_avrage2 == value) return; _avrage2 = value; OnPropertyChanged("AVRAGE2"); } }

            private double? _imbaa;
            public double? IMBAA { get => _imbaa; set { if (_imbaa == value) return; _imbaa = value; OnPropertyChanged("IMBAA"); } }

            private double? _totalarz;
            public double? TOTALARZ { get => _totalarz; set { if (_totalarz == value) return; _totalarz = value; OnPropertyChanged("TOTALARZ"); } }

            private string _visitor;
            public string VISITOR { get => _visitor; set { _visitor = value; OnPropertyChanged("VISITOR"); } }

            private double? _tkhn;
            public double? TKHN { get => _tkhn; set { if (_tkhn == value) return; _tkhn = value; OnPropertyChanged("TKHN"); } }

            private long? _jay;
            public long? JAY { get => _jay; set { if (_jay == value) return; _jay = value; OnPropertyChanged("JAY"); } }

            private int? _jayo;
            public int? JAYO { get => _jayo; set { if (_jayo == value) return; _jayo = value; OnPropertyChanged("JAYO"); } }

            private DateTime? _crt;
            public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

            private int? _uid;
            public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

            #region CUSTOM_UnBOUND_FIELDS
            private string _codeo;
            public string CODEO { get => _codeo; set { _codeo = value; OnPropertyChanged("CODEO"); } }
            #endregion

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

        }



        public class STUF_STK_CSHARP
        {
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public double? MOGODI_A { get; set; }
            public double? MOGODI { get; set; }
            public double? MABL_M { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class DTLMANF_QRE1
        {
            public double? SumOfMABLK { get; set; }
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
        }
        public class QRE_FAC_01
        {
            public double? RADAH { get; set; }
            public string CODE { get; set; }
        }
        public class QRE_FAC_02
        {
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public double? MOGODI { get; set; }
            public double? MOGODI_A { get; set; }
        }
        public class INVO_EDAM_CSHARP
        {
            public long? idd { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public int? VAHED { get; set; }
            public double? NUMBER { get; set; }
            public double? TAGH { get; set; }
            public int? keyu { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }

        public class QRE_KH_0
        {
            public double? NUMBER { get; set; }
            public long? ID { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public string CODE { get; set; }
            public int? VAHED_K { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
        }
        public class QRE_KH_01
        {
            public double? MEGHS { get; set; }
            public double? MEGHkS { get; set; }
        }
        public class QRE_KH_02
        {
            public int? VAHED { get; set; }
            public double? MIN_M { get; set; }
        }
        public class QRE_KH_03
        {
            public int? ANBCO { get; set; }
        }
        public class tbl_BarCodes_CSHARP
        {
            public string Barcode { get; set; }
            public int? ID { get; set; }
            public string NAM { get; set; }
            public string CODE { get; set; }
            public double? mab { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class IVO_EXTENDED_CSHARP
        {
            public long? id { get; set; }
            public double? FLD1 { get; set; }
            public double? FLD2 { get; set; }
            public double? FLD3 { get; set; }
            public double? FLD4 { get; set; }
            public double? FLD5 { get; set; }
            public double? FLD6 { get; set; }
            public double? FLD7 { get; set; }
            public double? FLD8 { get; set; }
            public double? FLD9 { get; set; }
            public double? FLD10 { get; set; }
            public double? FLD11 { get; set; }
            public double? FLD12 { get; set; }
            public double? FLD13 { get; set; }
            public double? FLD14 { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class QRE_KH_04
        {
            public int? BASE { get; set; }
            public double? n_s { get; set; }
            public long? date_s { get; set; }
            public double? no_s { get; set; }
        }
        public class QRE_KH_05
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string REQUEST_NO { get; set; }
            public string BARNAMEH { get; set; }
            public string DRIVER { get; set; }
            public string DRIVER_MOB { get; set; }
            public string CAMIUN_NUM { get; set; }
            public double? MAGHSAD { get; set; }
            public double? CAM_KHALY { get; set; }
            public double? CAM_POOR { get; set; }
            public string TOZIH { get; set; }
            public string CAMIUN { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class QRE_KH_06
        {
            public double? NUMBER { get; set; }
            public double? TAGG { get; set; }
            public string CODE { get; set; }
            public double? CAM_KHALY { get; set; }
            public double? CAM_POOR { get; set; }
            public double? MEGHk { get; set; }
            public string TOZIH { get; set; }
            public double? RADIF { get; set; }
            public double? VAZNH { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class QRE_KH_07
        {
            public string CODE { get; set; }
            public double? RADIF { get; set; }
        }
        public class CL_HEAD_LST_RASID_US
        {
            public string DATE_N { get; set; }
            public string USER_NAME { get; set; }
            public string NUMBER1 { get; set; }
            public string SADER { get; set; }
            public string TAH { get; set; }
            public string MOLAH { get; set; }
            public string FNUMCO { get; set; }
            public string CUST_NO { get; set; }
            public string CUST_NO2 { get; set; }
            public bool OKF { get; set; }
        }
        public class TCOD_MAP_Combo
        {
            public int MPCODE { get; set; }
            public string MPNAME { get; set; }
        }
        public class Driver_Combo
        {
            public string DRIVER { get; set; }
        }
        public class Camiun_Num_Combo
        {
            public string CAMIUN_NUM { get; set; }
        }
        public class Camiun_Combo
        {
            public string CAMIUN { get; set; }
        }
        public class OTHER_DTL_CSHARP
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string REQUEST_NO { get; set; }
            public string BARNAMEH { get; set; }
            public string DRIVER { get; set; }
            public string DRIVER_MOB { get; set; }
            public string CAMIUN_NUM { get; set; }
            public double? MAGHSAD { get; set; }
            public double? CAM_KHALY { get; set; }
            public double? CAM_POOR { get; set; }
            public string TOZIH { get; set; }
            public string CAMIUN { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class THE_ONE1
        {
            public string name { get; set; }
            public string TYP { get; set; }
            public byte xtype { get; set; }
            public short? length { get; set; }
        }
        public class TAKHFIF_APLAY_1
        {
            public int? TID { get; set; }
            public double? NUMBER { get; set; }
            public double? KIND { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public int? TKIND { get; set; }
        }
        public class _QRE_1
        {
            public int? TID { get; set; }
            public double? AZMAB { get; set; }
            public double? TAMAB { get; set; }
            public double? TPERS { get; set; }
            public double? TMAB { get; set; }
        }
        public class _QRE_2
        {
            public double? TKHN { get; set; }
            public string code { get; set; }
            public double? megh { get; set; }
            public double? meghk { get; set; }
            public double? mabl { get; set; }
            public double? mabl_k { get; set; }
            public double? n_kol { get; set; }
            public double? n_moin { get; set; }
            public double? imbaa { get; set; }
            public double? id { get; set; }
        }
        public class _QRE_3
        {
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public double? MOGODI_A { get; set; }
            public double? FI_A { get; set; }
            public double? MABL_A { get; set; }
        }
        public class GENERSANAD_QRE1
        {
            public double? SumOfMABLK { get; set; }
            public double? SumOfIMBIBE_MANF { get; set; }
            public double? SumOfIMBIBE_SAR { get; set; }
            public int? FNUMB { get; set; }
        }
        public class GETFIRSTPRICE_QRE2
        {
            public string? CODE { get; set; }
            public double? FI_A { get; set; }
        }
        public class _QRE_4
        {
            public double? MOIN { get; set; }
            public double? MBAA { get; set; }
        }
        public class _QRE_5
        {
            public double? TAKHFIF { get; set; }
            public double? MBAA { get; set; }
        }
        public class _QRE_6
        {
            public int? USERCO { get; set; }
            public bool? FFR_FROOSH { get; set; }
            public bool? FFR_HESAB { get; set; }
            public bool? FFR_MODIR { get; set; }
        }
        public class _QRE_7
        {
            public int? USERCO { get; set; }
            public bool? SGN0134 { get; set; }
            public bool? SGN0234 { get; set; }
            public bool? SGN0334 { get; set; }
        }
    }
}
