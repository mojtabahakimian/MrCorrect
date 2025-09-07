using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.FUNCTIONS
{
    public static class CL_FUNCTIONS
    {
        public static string GetBetweenStr(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                var nstrsorce = strSource.Replace(" ", "");
                var nstrstart = strStart.Replace(" ", "");
                var nstrend = strEnd.Replace(" ", "");
                int Start, End;
                Start = nstrsorce.IndexOf(nstrstart, 0) + nstrstart.Length;
                End = nstrsorce.IndexOf(nstrend, Start);
                if (End == -1)
                {
                    End = nstrsorce.Length;
                }
                return nstrsorce.Substring(Start, End - Start);
            }
            return "";
        }
    }
}
