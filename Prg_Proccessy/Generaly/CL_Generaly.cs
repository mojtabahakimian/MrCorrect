using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System.Diagnostics;
using System.Reflection;
using static Dapper.SqlMapper;

namespace Prg_Proccessy.Generaly
{
    public static class CL_Generaly
    {

        private static string _myexefile;
        public static string MyExeFileName
        {
            get
            {
                try
                {
                    _myexefile = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                }
                catch (Exception)
                {
                    _myexefile = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
                }
                return _myexefile;
                //return AppDomain.CurrentDomain.FriendlyName + ".exe"; //Wrong for .NET Core
            }
            set { _myexefile = value; }
        }

        //public static string StiLicKey { get; } = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHn0s4gy0Fr5YoUZ9V00Y0igCSFQzwEqYBh/N77k4f0fWXTHW5rqeBNLkaurJDenJ9o97TyqHs9HfvINK18Uwzsc/bG01Rq+x3H3Rf+g7AY92gvWmp7VA2Uxa30Q97f61siWz2dE5kdBVcCnSFzC6awE74JzDcJMj8OuxplqB1CYcpoPcOjKy1PiATlC3UsBaLEXsok1xxtRMQ283r282tkh8XQitsxtTczAJBxijuJNfziYhci2jResWXK51ygOOEbVAxmpflujkJ8oEVHkOA/CjX6bGx05pNZ6oSIu9H8deF94MyqIwcdeirCe60GbIQByQtLimfxbIZnO35X3fs/94av0ODfELqrQEpLrpU6FNeHttvlMc5UVrT4K+8lPbqR8Hq0PFWmFrbVIYSi7tAVFMMe2D1C59NWyLu3AkrD3No7YhLVh7LV0Tttr/8FrcZ8xirBPcMZCIGrRIesrHxOsZH2V8t/t0GXCnLLAWX+TNvdNXkB8cF2y9ZXf1enI064yE5dwMs2fQ0yOUG/xornE";
        public static string StiLicKey { get; set; } = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";
        public static string General_Servername { get; set; }
        public static string General_DBname { get; set; }
        public static string General_Username { get; set; }
        public static string General_Password { get; set; }

        private static bool iamserver = false;
        public static bool I_AM_SERVER
        {
            get
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    try
                    {
                        db.Open();

                        string ServerMachineName = db.Query<string>("SELECT SERVERPROPERTY('MachineName')").First();

                        bool isServerDevice = Environment.MachineName.Equals(ServerMachineName, StringComparison.OrdinalIgnoreCase);

                        iamserver = isServerDevice;
                    }
                    catch (Exception)
                    {
                        throw; // Re-throw the exception to handle it further up the call stack
                    }
                    finally
                    {
                        db?.Close(); db?.Dispose();
                    }
                }

                return iamserver;
            }
        }


        public static int SHIFT_OF_USER { get; set; }

        /// <summary>
        /// Forms["Default"] ["TFSAZMAN"] 
        /// </summary>
        public static int VAHED_OF_USER { get; set; }

        public static bool IsCalledExternally { get; set; } = false;
        public static string SectionName { get; set; }
        public static string REMIND_TITLE { get; } = @".....................:::::::::::::::|||||||||||||||||||||(         يادآوري          )|||||||||||||||||||||:::::::::::::::.....................";


        private static bool _ismrcorrectlocky;
        /// <summary>
        ///Baseknow.GHAYM?.ToString() == "7" | نمایش قیمت پیش فرض رو اعلامیه قیمت است
        /// </summary>
        public static bool IsGHAYM_7
        {
            get
            {
                //if (Strings.Mid(Baseknow.tindata.ToUpper(), 20, 7) == "CORRECT" && Baseknow.GHAYM == 7)
                if (Baseknow.GHAYM == 7)
                {
                    _ismrcorrectlocky = true;
                }
                else
                {
                    _ismrcorrectlocky = false;
                }
                return _ismrcorrectlocky;
            }
            set { _ismrcorrectlocky = value; }
        }



        /// <summary>
        /// C:\getaccess.txt
        /// </summary>
        public static string FILEACCESSPATH { get; set; } = @"C:\getaccess.txt";


        private static bool _IsMrCorrectLoadedWinBase = false;
        public static bool IsMrCorrectLoadedWinBase
        {
            get
            {
                return _IsMrCorrectLoadedWinBase;
            }
            set { _IsMrCorrectLoadedWinBase = value; }
        }



        //private static int _flag; // 0 for false, 1 for true
        //public static bool Flag
        //{
        //    get => Interlocked.CompareExchange(ref _flag, 1, 1) == 1; // Atomic read
        //    set => Interlocked.Exchange(ref _flag, value ? 1 : 0); // Atomic write
        //}

        public static class ThreadSafeProperties
        {
            private static volatile bool _myBoolean;
            private static readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(true);
            public static bool IsStillWorking
            {
                get
                {
                    return _myBoolean;
                }
                set
                {
                    if (_myBoolean != value)
                    {
                        _myBoolean = value;
                        if (_myBoolean)
                        {
                            // If set to true, reset the event (block waiting threads)
                            _resetEvent.Reset();
                        }
                        else
                        {
                            // If set to false, set the event (unblock waiting threads)
                            _resetEvent.Set();
                        }
                    }
                }
            }
            public static void WaitForFalse()
            {
                _resetEvent.Wait();  // This will block the thread until the boolean is set to false
            }
            public static bool WaitForFalse(TimeSpan timeout)
            {
                try
                {
                    return _resetEvent.Wait(timeout);  // Returns false if timeout is reached
                }
                catch (ObjectDisposedException)
                {
                    // Handle edge case where ManualResetEventSlim might be disposed
                    return false;
                }
                catch (InvalidOperationException)
                {
                    // Handle any other invalid operations
                    return false;
                }
            }
        }
    }
}
