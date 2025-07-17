using static Functions.SMSService.SmsServiceFactory;

namespace Functions.SMSService
{
    public static class SMSPINFO
    {
        public static string? API_KEY { get; set; } = null;
        public static long LINE_NUMBER { get; set; }
        public static SmsServiceType SERVICE_TYPE { get; set; }
        public static string? USERNAME { get; set; } = null;
        public static string? PASSWORD { get; set; } = null;

        public static string ERRORKEY { get; } = "#error#"; //"#error#";

    }
}
