using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public partial class HEAD_LST
    {
        public double NUMBER { get; set; }
        public double TAG { get; set; }
        public Nullable<int> ANBAR { get; set; }
        public Nullable<double> NUMBER1 { get; set; }
        public long DATE_N { get; set; }
        public string TAH { get; set; }
        public double MAS { get; set; }
        public double VAS { get; set; }
        public Nullable<double> N_S { get; set; }
        public string CUST_NO { get; set; }
        public string MOLAH { get; set; }
        public double M_NAGHD { get; set; }
        public double MABL_VAR { get; set; }
        public string MOIN_VAR { get; set; }
        public double MABL_HAV { get; set; }
        public string MOIN_HAV { get; set; }
        public double MABL_HAZ { get; set; }
        public string MOIN_HAZ { get; set; }
        public double TAKHFIF { get; set; }
        public string MOIN_KHF { get; set; }
        public Nullable<int> ANBARF { get; set; }
        public Nullable<double> FNUMCO { get; set; }
        public Nullable<int> DEPATMAN { get; set; }
        public Nullable<int> SHIFT { get; set; }
        public Nullable<int> CUST_KIND { get; set; }
        public string USER_NAME { get; set; }
        public string SHARAYET { get; set; }
        public Nullable<bool> SGN1 { get; set; }
        public Nullable<bool> SGN2 { get; set; }
        public Nullable<bool> SGN3 { get; set; }
        public Nullable<bool> SGN4 { get; set; }
        public int? sgn1usid { get; set; }
        public int? sgn2usid { get; set; }
        public int? sgn3usid { get; set; }
        public Nullable<double> MBAA { get; set; }
        public string HMBAA { get; set; }
        public double TAMIR { get; set; }
        public Nullable<bool> TICMBAA { get; set; }
        public Nullable<bool> TKHF { get; set; }
        public Nullable<bool> OKF { get; set; }
        public Nullable<byte> SADER { get; set; }
        public Nullable<double> ARZD { get; set; }
        public Nullable<byte> ARZKIND { get; set; }
        public string? ARZKIND2 { get; set; }
        public Nullable<long> CDDATE { get; set; }
        public Nullable<int> CDTIME { get; set; }
        public Nullable<long> OKDATE { get; set; }
        public Nullable<int> OKTIME { get; set; }
        public Nullable<bool> JAY { get; set; }
        public Nullable<int> MODAT_PPID { get; set; }
        public Nullable<int> PEPID { get; set; }
        public Nullable<int> PEID { get; set; }

        public int? UID { get; set; }
    }
}
