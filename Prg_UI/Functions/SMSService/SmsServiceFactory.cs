using Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Functions.SMSService
{
    public class SmsServiceFactory
    {
        public enum SmsServiceType
        {
            /// <summary>
            /// شرکت ایده پردازان SMS.ir
            /// </summary>
            SmsIr,

            /// <summary>
            /// شرکت توبا TSMS.ir
            /// </summary>
            TsmsUrl
        }
        public static ISmsService CreateSmsService(SmsServiceType serviceType, string apiKey, string username, string password, long lineNumber, HttpClient httpClient)
        {
            switch (serviceType)
            {
                case SmsServiceType.SmsIr:
                    return new SMSIR(apiKey, lineNumber);
                case SmsServiceType.TsmsUrl:
                    return new TSMSURL(username, password, lineNumber.ToString(), httpClient);
                default:
                    throw new ArgumentException("Invalid SMS service type", nameof(serviceType));
            }
        }
    }
}
