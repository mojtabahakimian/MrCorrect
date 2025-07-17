using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.MODELS
{
    public static class Baseknow
    {
        #region ITMS
        public static bool Text44 = false;

        public static int STHFR = 1;
        public static string HESMBAA = "";
        public static short SANAD = 0;
        /// <summary>
        /// انبار پیش فرض که توسط هر فرم باتوجه به دسترسی کاربر مقدار دهی میشود
        /// </summary>
        public static int anbardef { get; set; } = 0;
        public static short DEFANB { get; set; }
        public static string hesnaghd { get; set; }
        public static long dt { get; set; }
        public static string UUSER { get; set; }
        public static bool MOJU { get; set; }
        public static double? FROSH { get; set; }
        public static double? MFROSH { get; set; }
        public static double? BANKHA { get; set; }
        public static short? GHAYM { get; set; }
        public static byte? ARSESH { get; set; }
        public static short? YEA { get; set; }
        public static bool? CTL_DT { get; set; }
        public static bool? mrcorrect { get; set; } = false;
        public static int? TKHF { get; set; }
        public static string OPTIONSS { get; set; }
        public static string UGRP { get; set; }
        public static double? DIG { get; set; }
        public static short? PERSON { get; set; }
        public static short? KALA { get; set; }
        public static double? CPI { get; set; }
        public static bool? SIGN { get; set; }
        public static string SERVERNAM { get; set; }

        private static int? _usercod;
        public static int? USERCOD
        {
            get
            {
                if (_usercod is null)
                {
                    _usercod = 0;
                }
                return _usercod;
            }
            set { _usercod = value; }
        }

        private static string _tindata;
        public static string tindata
        {
            get
            {
                if (string.IsNullOrEmpty(_tindata))
                {
                    _tindata = "0000000000000000000000000000";
                }
                return _tindata;
            }
            set { _tindata = value; }
        }
        public static bool? SAGHF { get; set; }
        public static bool? SAGHF2 { get; set; }
        public static bool? TRANSF { get; set; }
        public static bool? SNDKH { get; set; }
        public static double? DARAM { get; set; }
        public static bool? SANAT { get; set; }
        public static double? MOGODIA { get; set; }
        public static double? GHEYMAT { get; set; }
        public static string ADA { get; set; }
        public static double? TFROSH { get; set; }
        public static string HPOR { get; set; }
        public static double? SANDOGH { get; set; }
        public static string APA { get; set; }
        public static double? TKHARID { get; set; }
        public static int? PKHARID { get; set; }
        public static double? KHARID { get; set; }
        /// <summary>
        /// نام شرکت
        /// </summary>
        public static string NAME { get; set; }
        public static bool? UPDDATE { get; set; }
        public static bool? RMOG { get; set; }
        public static bool? LOCKFAP { get; set; }
     

        public static string ISO_FROOSH { get; set; }
        public static bool? HOLA { get; set; }
        public static int? SAGHFH { get; set; }
        public static bool? HSAY { get; set; }
        public static bool? HJAZ { get; set; }
        public static bool? HNAH { get; set; }
        public static bool? HCON { get; set; }
        public static bool? HKHA { get; set; }
        public static bool? HSHI { get; set; }
        public static bool? HEZA { get; set; }
        public static bool? HBON { get; set; }
        public static int? SANAVP { get; set; }
        public static double? UNIVERSITY_CO { get; set; }
        public static string MANAGER { get; set; }
        public static string TFADDRESS { get; set; }
        /// <summary>
        ///  SANAVP حق سنوات پایهHSANP
        /// </summary>
        public static int? HSANP { get; set; } = 0;
        public static bool MAND { get; set; }
        public static int? STFR { get; set; }
        public static double? BEDEHKAR { get; set; }
        public static bool? BARCOD { get; set; }
        public static double? WAR { get; set; }
        public static bool? FRUP { get; set; }
        public static int? STHKH { get; set; }
        //
        public static int NAME_CODE { get; set; } = 0;
        public static string TFTEL { get; set; }
        public static string MCODEM { get; set; }
        public static string ECODE { get; set; }
        public static string ADV { get; set; }
        public static string APV { get; set; }

        public static bool? FINALS { get; set; }
        public static double? PHAZ_TOL { get; set; }
        public static double? HAZ_TOL { get; set; }
        public static double? CONKAL { get; set; }
        public static double? AMALKARD { get; set; }
        public static double? HDARAM { get; set; }
        public static string TFSAZMAN { get; set; }
        public static string TFCODE_E { get; set; }

       

        public static string SMS_OWNER { get; set; }
        public static string WIDTH_D { get; set; }
        public static string BACKPATH { get; set; }
        public static int? STENT { get; set; }
        public static int? STFRB { get; set; }
        public static double? HKHARID { get; set; }
        public static double? HAVALAH { get; set; }
        public static string HIGH_D { get; set; }

        /// <summary>
        /// ارسال پيامك فعال باشد
        /// </summary>
        public static bool? SMSACT { get; set; }

        /// <summary>
        /// ارسال پيامك با تاييد كاربر باشد
        /// </summary>
        public static bool? PRMFR { get; set; }

        /// <summary>
        /// ارسال پيامك فقط از طريق سرور انجام  شود
        /// </summary>
        public static bool? DSMS { get; set; }

        public static double? BESTANKAR { get; set; }
        public static string MEMORYID { get; set; }
        public static string PERSONEL { get; private set; }
        public static int? STTOL { get; set; }
        public static bool? ECONM { get; set; }
        #endregion
        public static void GetInitTheApp()
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            if (!CL_Generaly.IsCalledExternally) //اگر اکسس اون رو صدا نزده بود بیا چک کن
            {
                //0- بررسی اتصال به اس کیو ال
                _ = dbms.CheckIsConnectedToSQLDB();
            }

            //1- بررسی نهایی و مقدار دهی
            //StiLicense.Key = StiLicKey; //#Left
            if (CL_CCNNMANAGER.ConnectedToSQLDB == true)
            {
                string connetionstring = "";
                if (!CL_Generaly.IsCalledExternally)
                {
                    connetionstring = CL_CryptionAlgorithem.DecryptTextUsingUTF8(dbms.GetConnectionSpecifyNameApp());
                }
                else
                {
                    connetionstring = CL_CCNNMANAGER.CONNECTION_STR;
                }

                int DataSrcIndx = connetionstring.IndexOf("Data Source=", StringComparison.CurrentCultureIgnoreCase);//++11
                int InitlCtalg = connetionstring.IndexOf(";Initial Catalog", StringComparison.CurrentCultureIgnoreCase);//++15
                int Integrated_Secrt = connetionstring.IndexOf(";Integrated Security", StringComparison.CurrentCultureIgnoreCase);
                int intiti2 = InitlCtalg;
                string servnm = connetionstring.Substring(DataSrcIndx + 12, ((InitlCtalg - DataSrcIndx) - 12));
                string dbnm = connetionstring.Substring(InitlCtalg + 17, ((Integrated_Secrt - intiti2) - 17)); // چون در ساب استرینگ فاصله میخوام یعنی میگه چندتا برم جلو نه مختصات
                string qt = "\"";
                //char nothing = '';
                if (dbnm.Contains(qt))
                {
                    dbnm = dbnm.Replace(qt, "");
                }
                CL_Generaly.General_Servername = servnm;
                CL_Generaly.General_DBname = dbnm;

                #region GetTarikh_Times_OfToday
                DateTime thisDate = DateTime.Now;
                PersianCalendar perscal = new PersianCalendar();
                string mah = (perscal.GetMonth(thisDate)).ToString();
                string rooz = (perscal.GetDayOfMonth(thisDate)).ToString();
                //Make 2 digit if is one without zero at first of that
                if (mah.Length < 2)
                {
                    mah = ("0" + mah).ToString();
                }
                if (rooz.Length < 2)
                {
                    rooz = ("0" + rooz).ToString();
                }

                Tarikh.Current_Sal = perscal.GetYear(thisDate).ToString();
                Tarikh.Current_Mah = mah.ToString();
                Tarikh.Current_Rooz = rooz.ToString();

                Tarikh.FullCurrentDate = $"{Tarikh.Current_Sal}{Tarikh.Current_Mah}{Tarikh.Current_Rooz}"; //تاریخ بدون اسلش

                Tarikh.SlashyFullDate = $"{Tarikh.Current_Sal}/{Tarikh.Current_Mah}/{Tarikh.Current_Rooz}";//تاریخ با اسلش 
                #endregion

                //2- مقدار دهی بیس نو
                var quer_loadbase = dbms.DoGetDataSQL<SAZMAN>("SELECT * FROM SAZMAN").ToList();
                foreach (var item in quer_loadbase)
                {
                    FROSH = item.FROSH;
                    MFROSH = item.MFROSH;
                    BANKHA = item.BANKHA;
                    if (item.hesnaghd != null)
                    {
                        hesnaghd = item.hesnaghd;
                    }
                    if (item.GHAYM != null)
                    {
                        GHAYM = item.GHAYM;
                    }
                    if (item.ARSESH != null)
                    {
                        ARSESH = item.ARSESH;
                    }
                    if (item.YEA != null)
                    {
                        YEA = item.YEA;
                    }
                    if (item.CTL_DT != null)
                    {
                        CTL_DT = item.CTL_DT;
                    }
                    if (item.TKHF != null)
                    {
                        TKHF = item.TKHF;
                    }
                    if (item.OPTIONSS != null)
                    {
                        OPTIONSS = item.OPTIONSS;
                    }
                    if (item.DIG != null)
                    {
                        DIG = item.DIG;
                    }
                    if (item.PERSON != null)
                    {
                        PERSON = item.PERSON;
                    }
                    if (item.KALA != null)
                    {
                        KALA = item.KALA;
                    }
                    if (item.CPI != null)
                    {
                        CPI = item.CPI;
                    }
                    if (item.SIGN != null)
                    {
                        SIGN = item.SIGN;
                    }
                    if (item.SERVERNAM != null)
                    {
                        SERVERNAM = item.SERVERNAM;
                    }
                    if (item.SAGHF != null)
                    {
                        SAGHF = item.SAGHF;
                    }
                    if (item.SAGHF2 != null)
                    {
                        SAGHF2 = item.SAGHF2;
                    }
                    if (item.TRANSF != null)
                    {
                        TRANSF = item.TRANSF;
                    }
                    if (item.SNDKH != null)
                    {
                        SNDKH = item.SNDKH;
                    }
                    if (item.DARAM != null)
                    {
                        DARAM = item.DARAM;
                    }
                    if (item.SANAT != null)
                    {
                        SANAT = item.SANAT;
                    }
                    if (item.MOGODIA != null)
                    {
                        MOGODIA = item.MOGODIA;
                    }
                    if (item.GHEYMAT != null)
                    {
                        GHEYMAT = item.GHEYMAT;
                    }
                    if (item.ADA != null)
                    {
                        ADA = item.ADA;
                    }
                    if (item.TFROSH != null)
                    {
                        TFROSH = item.TFROSH;
                    }
                    if (item.HPOR != null)
                    {
                        HPOR = item.HPOR;
                    }
                    if (item.SANDOGH != null)
                    {
                        SANDOGH = item.SANDOGH;
                    }
                    if (item.APA != null)
                    {
                        APA = item.APA;
                    }
                    if (item.TKHARID != null)
                    {
                        TKHARID = item.TKHARID;
                    }
                    if (item.PKHARID != null)
                    {
                        PKHARID = item.PKHARID;
                    }
                    if (item.KHARID != null)
                    {
                        KHARID = item.KHARID;
                    }
                    if (item.NAME != null)
                    {
                        NAME = item.NAME;
                    }
                    if (item.UPDDATE != null)
                    {
                        UPDDATE = item.UPDDATE;
                    }
                    if (item.RMOG != null)
                        RMOG = item.RMOG;

                    if (item.DEFANB != null)
                        DEFANB = (short)item.DEFANB;
                    if (item.LOCKFAP != null)
                        LOCKFAP = item.LOCKFAP;
                    if (item.SMSACT != null)
                        SMSACT = item.SMSACT;
                    if (item.ISO_FROOSH != null)
                        ISO_FROOSH = item.ISO_FROOSH;
                    if (item.HOLA != null)
                        HOLA = item.HOLA;
                    if (item.SAGHFH != null)
                        SAGHFH = item.SAGHFH;
                    if (item.HSAY != null)
                        HSAY = item.HSAY;
                    if (item.HJAZ != null)
                        HJAZ = item.HJAZ;
                    if (item.HNAH != null)
                        HNAH = item.HNAH;
                    if (item.HCON != null)
                        HCON = item.HCON;
                    if (item.SANAVP != null)
                        SANAVP = item.SANAVP;
                    if (item.UNIVERSITY_CO != null)
                        UNIVERSITY_CO = item.UNIVERSITY_CO;
                    if (item.MANAGER != null)
                        MANAGER = item.MANAGER;
                    if (item.TFADDRESS != null)
                        TFADDRESS = item.TFADDRESS;

                    if (item.SANAVP != null)
                        HSANP = item.SANAVP;
                    if (item.MAND != null) MAND = (bool)item.MAND;
                    if (item.STFR != null) STFR = item.STFR;
                    if (item.BEDEHKAR != null) BEDEHKAR = item.BEDEHKAR;
                    if (item.BARCOD != null) BARCOD = item.BARCOD;
                    if (item.WAR != null) WAR = item.WAR;
                    if (item.FRUP != null) FRUP = item.FRUP;
                    if (item.STHKH != null) STHKH = item.STHKH;
                    if (item.TFTEL != null) TFTEL = item.TFTEL;
                    if (item.MCODEM != null) MCODEM = item.MCODEM;
                    if (item.ECODE != null) ECODE = item.ECODE;

                    if (item.ADV != null) ADV = item.ADV;
                    if (item.APV != null) APV = item.APV;

                    if (item.FINALS != null) FINALS = item.FINALS;
                    if (item.PHAZ_TOL != null) PHAZ_TOL = item.PHAZ_TOL;
                    if (item.HAZ_TOL != null) HAZ_TOL = item.HAZ_TOL;
                    if (item.CONKAL != null) CONKAL = item.CONKAL;
                    if (item.AMALKARD != null) AMALKARD = item.AMALKARD;
                    if (item.HDARAM != null) HDARAM = item.HDARAM;
                    if (item.TFSAZMAN != null) TFSAZMAN = item.TFSAZMAN;
                    if (item.TFCODE_E != null) TFCODE_E = item.TFCODE_E;
                    if (item.PRMFR != null) PRMFR = item.PRMFR;
                    if (item.SMS_OWNER != null) SMS_OWNER = item.SMS_OWNER;
                    if (item.WIDTH_D != null) WIDTH_D = item.WIDTH_D;
                    if (item.BACKPATH != null) BACKPATH = item.BACKPATH;
                    if (item.STENT != null) STENT = item.STENT;
                    if (item.STFRB != null) STFRB = item.STFRB;
                    if (item.HKHARID != null) HKHARID = item.HKHARID;
                    if (item.HAVALAH != null) HAVALAH = item.HAVALAH;
                    if (item.HIGH_D != null) HIGH_D = item.HIGH_D;
                    if (item.DSMS != null) DSMS = item.DSMS;
                    if (item.BESTANKAR != null) BESTANKAR = item.BESTANKAR;
                    if (item.MEMORYID != null) MEMORYID = item.MEMORYID;
                    if (item.PERSONEL != null) PERSONEL = item.PERSONEL;

                    if (item.STTOL != null) STTOL = item.STTOL;
                    if (item.ECONM != null) ECONM = item.ECONM;



                    HKHA = item.HKHA;
                    HSHI = item.HSHI;
                    HEZA = item.HEZA;
                    HBON = item.HBON;
                    //WholeFunction wholeFunction = new WholeFunction();
                    //dt = wholeFunction.FARSIDATE2(); #Left
                    MOJU = (bool)item.MOJU;
                    STHFR = (int)item.STHFR;
                    HESMBAA = item.HESMBAA;
                    SANAD = (short)item.SANAD;
                }

                try
                {
                    //مشکل رفع عدم اجرای صحیح بعضی موارد که به خاطر تداخل تقسیم با / نقطه اتفاق می افتد تنظیمات ویدنوز به ممیزر نقطه ایا تغییر میکند
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sDecimal", ".");
                }
                catch (Exception) { }
            }
        }
    }
}
