using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Functions.SMSService.MSGMODEL;

namespace Functions.SMSService
{
    public static class SmsResultProcessor
    {
        public static string GenerateResultSummary(BulkSendResult bulkResult)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"ارسال اس ام اس:");
            //summary.AppendLine($"هزینه کامل: {bulkResult.TotalCost:C}");
            summary.AppendLine();

            foreach (var kvp in bulkResult.RecipientResults)
            {
                summary.Append(GenerateRecipientResultSummary(kvp.Key, kvp.Value));
            }

            return summary.ToString();
        }

        private static string GenerateRecipientResultSummary(string recipient, RecipientResult result)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"گیرندگان: {recipient}");
            summary.AppendLine($"وضعیت: {(result.IsSentSuccess ? "موفق" : "نا موفق")}");
            summary.AppendLine($"پیام: {result.StatusMessage}");

            if (result.IsSentSuccess)
            {
                summary.AppendLine($"شناسه ارسال: {result.MessageId}");
                //summary.AppendLine($"Cost: {result.Cost:C}");
            }
            else
            {
                summary.AppendLine($"پیغام خطا: {result.StatusMessage.Replace(SMSPINFO.ERRORKEY, string.Empty)}");
            }

            summary.AppendLine();
            return summary.ToString();
        }

        public static List<SmsResultRecord> ConvertToRecords(BulkSendResult bulkResult)
        {
            return bulkResult.RecipientResults.Select(kvp => new SmsResultRecord
            {
                Recipient = kvp.Key,
                IsSentSuccess = kvp.Value.IsSentSuccess,
                StatusMessage = kvp.Value.StatusMessage,
                MessageId = kvp.Value.MessageId,
                Cost = kvp.Value.Cost,
                ErrorMessage = kvp.Value.IsSentSuccess ? null : kvp.Value.StatusMessage.Replace(SMSPINFO.ERRORKEY, string.Empty)
            }).ToList();
        }
    }
}
