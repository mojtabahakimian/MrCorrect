using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class PGET_HED
    {
        public int? ID { get; set; }
        public long? DATE { get; set; }
        public string? MOLAH { get; set; }
        public double? N_S { get; set; }
        public int? DEPATMAN { get; set; }
        public int? SHIFT { get; set; }
        public int? CUST_KIND { get; set; }
        public string? USER_NAME { get; set; }
        public short? KIND { get; set; }
        public int? IDK { get; set; }
        public bool? OKF { get; set; }
        public int? RPLICA { get; set; }
        public bool? SGN1 { get; set; }
        public bool? SGN2 { get; set; }
        public bool? SGN3 { get; set; }
        public int? sgn1usid { get; set; }
        public int? sgn2usid { get; set; }
        public int? sgn3usid { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
    }
}
