using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.FUNCTIONS
{
    public static class CL_Tools
    {
        public static void DoGetwriteLog(string _YOUR_TEXT_)
        {
            Random ts = new Random();

            string pathfile = @$"C:\CORRECT\ERLOGS_INQUIRY\PRGLOG{DateTime.Now.ToString("YYYYMMDDHHMM")}_{ts.Next(0, 500)}.txt";

            //if (!Directory.Exists("C:\\CORRECT\\ERLOGS_INQUIRY")) Directory.CreateDirectory("C:\\CORRECT\\ERLOGS_INQUIRY");
            File.AppendAllText(pathfile, "______________________________________________\n");
            File.AppendAllText(pathfile, _YOUR_TEXT_ + " Till Completed} ");
            File.AppendAllText(pathfile, "______________________________________________\n");
        }
  
    }
}
