using Interfaces;
using IPE.SmsIrClient.Models.Requests;
using System;
using System.Net.Http;
using static Functions.SMSService.MSGMODEL;
using System.Threading.Tasks;
using static Functions.SMSService.SmsServiceFactory;

namespace Functions.SMSService
{
    public class SmsManager
    {
        private readonly ISmsService _smsService;
        public SmsManager(SmsServiceType serviceType, string apiKey, string username, string password, long lineNumber, HttpClient httpClient)
        {
            _smsService = SmsServiceFactory.CreateSmsService(serviceType, apiKey, username, password, lineNumber, httpClient);
        }

        /// <summary>
        /// Check Configuration on call for unexcpected TLs/SSL Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        /// <param name="sendDateTime"></param>
        /// <returns></returns>
        public async Task<BulkSendResult> SendBulkSms(string message, string[] recipients, DateTime? sendDateTime = null)
        {
            return await _smsService.SendBulkSmsAsync(message, recipients, sendDateTime);
        }

        #region JUST_IN_CASE
        //// On Call Startup:

        //  httpClient.Timeout = TimeSpan.FromSeconds(60); // Increase the timeout value as needed

        //HttpClientHandler handler = new HttpClientHandler();
        //handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
        //HttpClient httpClient = new HttpClient(handler);
        #endregion

        public async Task<VerificationSendResult> SendVerificationSms(string phoneNumber, int templateId, VerifySendParameter[] parameters)
        {
            return await _smsService.SendVerificationSmsAsync(phoneNumber, templateId, parameters);
        }
    }
}
