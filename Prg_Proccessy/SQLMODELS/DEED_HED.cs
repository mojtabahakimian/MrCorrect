using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class DEED_HED
    {
        public double N_S { get; set; }
        public long DATE_S { get; set; }
        public string SHARH_S { get; set; }
        public double NO_S { get; set; }
        public Nullable<double> ANBAR { get; set; }
        public Nullable<double> N_FACTOR { get; set; }
        public bool GHATEI { get; set; }
        public string USER_NAME { get; set; }
        public int @base { get; set; }
        public int BAYEG { get; set; }
        public Nullable<bool> SGN1 { get; set; }
        public Nullable<bool> SGN2 { get; set; }
        public Nullable<bool> SGN3 { get; set; }
        public Nullable<bool> SGN4 { get; set; }
        public Nullable<bool> OKF { get; set; }
        public int? sgn1usid { get; set; }
        public int? sgn2usid { get; set; }
        public int? sgn3usid { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
    }
}
