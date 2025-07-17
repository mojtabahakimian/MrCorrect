using Prg_Proccessy.MODELS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.FUNCTIONS
{
    public static class Tarikh
    {
        public static string Current_Rooz = "";
        public static string Current_Mah = "";
        public static string Current_Sal = "";
        public struct PersianDateTime
        {
            public int Year { get; }
            public int Month { get; }
            public int Day { get; }
            public int Hour { get; }
            public int Minute { get; }
            public int Second { get; }
            public int Millisecond { get; }
            public PersianDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
            {
                Year = year;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
                Second = second;
                Millisecond = millisecond;
            }
            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}",
                    Year, Month, Day, Hour, Minute, Second, Millisecond);
            }
        }
        private static readonly PersianCalendar persianCalendar = new PersianCalendar();
        public static PersianDateTime ToPersianDateTime(DateTime date)
        {
            return new PersianDateTime(
                persianCalendar.GetYear(date),
                persianCalendar.GetMonth(date),
                persianCalendar.GetDayOfMonth(date),
                date.Hour,
                date.Minute,
                date.Second,
                date.Millisecond);
        }
        public static bool IsSyncedDateNow(string dt, bool flag)
        {
            string sal = dt.Substring(0, 4);
            bool res = true;
            if (flag)//آیا تیک کنترل شود تاریخ وارد شده با تاریخ سال مالی یکی است ؟
            {
                if (sal != Baseknow.YEA.ToString())
                {
                    res = false;
                }
                else
                {
                    res = true;//تاریخ صحیح است
                }
            }
            return res;
        }

        private static string _fullcurrentdate;
        /// <summary>
        /// تاریخ بدون اسلش
        /// </summary>
        public static string FullCurrentDate
        {
            get
            {
                _fullcurrentdate = GoGetPersianDate(true);
                return _fullcurrentdate;
            }
            set { _fullcurrentdate = value; }
        }
        private static string _slashy;
        public static string SlashyFullDate
        {
            get
            {
                _slashy = GoGetPersianDate(false);
                return _slashy;
            }
            set { _slashy = value; }
        }

        private static string _ADFullCurrentDate;
        /// <summary>
        /// تاریخ میلادی بدون اسلش
        /// </summary>
        public static string ADFullCurrentDate
        {
            get
            {
                //var GregorianDate2 = DateTime.Now.ToString(null, CultureInfo.GetCultureInfo("en-US")); ////UtcNow is Time of Europe so not using
                _ADFullCurrentDate = DateTime.Now.ToString("yyyymmdd", CultureInfo.InvariantCulture);
                return _ADFullCurrentDate;
            }

        }

        /// <summary>
        /// Item1: SAL | Item2: MAH | Item3: ROOZ | 
        /// </summary>
        /// <param name="_the_date_"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> GetSplitPersianDate(string _the_date_)
        {
            var SAL = _the_date_.Substring(0, 4); //Year
            var MAH = _the_date_.Substring(4, 2); //Month
            var ROOZ = _the_date_.Substring(6, 2); //Day

            return Tuple.Create(SAL, MAH, ROOZ);
        }
        /// <summary>
        /// Get Miladi Date | AD Date
        /// </summary>
        /// <param name="persianDate"></param>
        /// <returns></returns>
        public static DateTime GetRawGregorianDateTime(string persianDate)
        {
            int persianYear = int.Parse(persianDate.Substring(0, 4));
            int persianMonth = int.Parse(persianDate.Substring(4, 2));
            int persianDay = int.Parse(persianDate.Substring(6, 2));

            PersianCalendar persianCalendar = new PersianCalendar();
            DateTime dateTime = persianCalendar.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);
            //var now = new DateTimeOffset(TheFunctions.GetGregorianDateTime("14020224")).ToUnixTimeMilliseconds();
            return dateTime;
        }
        public static string GoGetPersianDate(bool IsRawdate)
        {
            string resulty = "";
            DateTime miladi = DateTime.Now;
            System.Globalization.PersianCalendar shamsi = new System.Globalization.PersianCalendar();

            if (IsRawdate)
                resulty = string.Format("{0}{1}{2}", shamsi.GetYear(miladi).ToString("00.##"), shamsi.GetMonth(miladi).ToString("00.##"), shamsi.GetDayOfMonth(miladi).ToString("00.##"));
            else
                resulty = string.Format("{0}/{1}/{2}", shamsi.GetYear(miladi).ToString("00.##"), shamsi.GetMonth(miladi).ToString("00.##"), shamsi.GetDayOfMonth(miladi).ToString("00.##"));
            string slashydate = string.Format("{0}/{1}/{2}", shamsi.GetYear(miladi).ToString("00.##"), shamsi.GetMonth(miladi).ToString("00.##"), shamsi.GetDayOfMonth(miladi).ToString("00.##"));

            return resulty;
        }

        public static bool IsValidedDate(string date)
        {
            var flag = false;
            if (date.Length != 8) // اگر طول تاریخ درست وارد نشده برگرد و بگو درست نیست11112233
            {
                return flag = false;
            }
            if (!date.Contains("/"))
            {
                string sal = date.Substring(0, 4);
                string mah = date.Substring(4, 2);
                string rooz = date.Substring(6, 2);
                date = $"{sal}/{mah}/{rooz}";

                //Maximum valid value : 9377 12 29
                if (date == "9999/12/30") //Just this exception
                {
                    return true;
                }
            }
            try
            {
                //// Validate with PersianCalendar
                //var persianCalendar = new System.Globalization.PersianCalendar();
                //var validDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);

                flag = DateTime.TryParseExact(date, "yyyy/MM/dd", new System.Globalization.CultureInfo("fa-IR"), System.Globalization.DateTimeStyles.None, out _);
            }
            catch { }

            return flag;
        }
        /// <summary>
        /// تاریخ عددی سیستمی
        /// </summary>
        /// <returns></returns>
        public static double GET_OADATE_DAO()
        {
            return Convert.ToDouble(DateTime.Now.ToOADate());
        }
        public static string GetMiladiDateTimeForSQL(bool isOnlyTime = false, bool IsGetASSqlcast = false)
        {
            if (isOnlyTime)
                return DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));

            if (IsGetASSqlcast)
                //return $"CAST('{DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"))}' AS DATETIME)"; ;
                return $"CAST('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"))}' AS DATETIME)";


            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
        }
    }
}
