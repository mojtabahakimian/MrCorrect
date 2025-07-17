using Functions.SMSService;
using IPE.SmsIrClient.Models.Requests;
using System;
using System.Threading.Tasks;
using static Functions.SMSService.MSGMODEL;

namespace Interfaces
{
    public interface ISmsService
    {
        Task<BulkSendResult> SendBulkSmsAsync(string message, string[] recipients, DateTime? sendDateTime = null);
        Task<VerificationSendResult> SendVerificationSmsAsync(string phoneNumber, int templateId, VerifySendParameter[] parameters);
    }
}
