using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class SMS_SENDS
    {
        public int? IDS { get; set; }
        public long? SM_DT { get; set; }
        public long? SM_TT { get; set; }
        public long? SM_DTQ { get; set; }
        public long? SM_TTQ { get; set; }
        public long? SM_DTT { get; set; }
        public long? SM_TTT { get; set; }
        public string? SM_AMobiles { get; set; }
        public string? AMSG { get; set; }
        public double? NUMBER { get; set; }
        public double? TAGS { get; set; }
        public string? id_sms { get; set; }
        public string? CUST_NO { get; set; }
        public string? USERNAME { get; set; }
        public string? PC_NAME { get; set; }
        public string? IPADD { get; set; }
        public int? STATUSSMS { get; set; }
        public DateTime? CRT { get; set; }
        public int? UID { get; set; }
    }
}
