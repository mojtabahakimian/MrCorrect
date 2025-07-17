using System;
using System.Collections.Generic;
using System.Text;

namespace Functions.SMSService
{
    public class MSGMODEL
    {
        public class BulkSendResult
        {
            public Guid PackId { get; set; }
            public decimal TotalCost { get; set; }
            public bool IsAllSentSuccess { get; set; }
            public Dictionary<string, RecipientResult> RecipientResults { get; set; } = new Dictionary<string, RecipientResult>();
        }

        public class RecipientResult
        {
            public string MessageId { get; set; }
            public bool IsSentSuccess { get; set; }
            public string StatusMessage { get; set; }
            public decimal Cost { get; set; }
        }
        public class VerificationSendResult
        {
            public int MessageId { get; set; }
            public decimal Cost { get; set; }
            public string StatusMessage { get; set; }
            public bool IsSentSuccess { get; set; }
        }

        public class SmsResultRecord
        {
            public string Recipient { get; set; }
            public bool IsSentSuccess { get; set; }
            public string StatusMessage { get; set; }
            public string MessageId { get; set; }
            public decimal Cost { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
