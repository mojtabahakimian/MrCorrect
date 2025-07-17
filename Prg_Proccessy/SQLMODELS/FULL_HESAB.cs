using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class FULL_HESAB
    {
        public void DoClear()
        {
            FULL_HES = null;
            NAME = null;
            N_KOL = null;
            N_MOIN = null;
            N_TAF = null;
            N_TAF2 = null;
            N_TAF3 = null;
            N_TAF4 = null;
        }
        public string? FULL_HES { get; set; }
        public string? NAME { get; set; }

        public string? N_KOL { get; set; }
        public string? N_MOIN { get; set; }
        public string? N_TAF { get; set; }
        public string? N_TAF2 { get; set; }
        public string? N_TAF3 { get; set; }
        public string? N_TAF4 { get; set; }
    }
}
