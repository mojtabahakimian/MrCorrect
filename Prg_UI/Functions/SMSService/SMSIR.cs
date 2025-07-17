using Interfaces;
using IPE.SmsIrClient.Models.Requests;
using IPE.SmsIrClient.Models.Results;
using IPE.SmsIrClient;
using System;
using System.Linq;
using static Functions.SMSService.MSGMODEL;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Policy;

namespace Functions.SMSService
{
    /// <summary>
    /// از شرکت ایده پردازان
    /// </summary>
    public class SMSIR : ISmsService
    {
        private readonly string _apiKey;
        private readonly SmsIr _smsIr;
        private readonly long _lineNumber;

        public SMSIR(string apiKey, long lineNumber)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _lineNumber = lineNumber;
            _smsIr = new SmsIr(_apiKey);
        }

        public async Task<BulkSendResult> SendBulkSmsAsync(string message, string[] recipients, DateTime? sendDateTime = null)
        {
            if (recipients == null || recipients.Length == 0)
            {
                throw new ArgumentException("Recipients cannot be null or empty.", nameof(recipients));
            }

            try
            {
                int? unixTimestamp = sendDateTime.HasValue ? (int)(sendDateTime.Value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds : (int?)null;

                if (recipients.Count() > 100)
                {
                    throw new ArgumentException("Recipients cannot be more than 100.", nameof(recipients));
                }

                for (int i = 0; i < recipients.Count(); i++) //Ensure that format number are correct
                {
                    if (recipients[i].StartsWith('0')) //09353954639
                    {
                        recipients[i] = recipients[i].TrimStart('0'); //9353954639
                    }
                }

                //for (int i = 0; i < 3; i++)
                //{
                //    try
                //    {
                //        var bulkSendResult = await _smsIr.BulkSendAsync(_lineNumber, message, recipients, unixTimestamp);
                //    }
                //    catch (HttpRequestException ex)
                //    {
                //        await Task.Delay(1000 * (int)Math.Pow(2, i)); // Exponential backoff
                //    }
                //}


                var bulkSendResult = await _smsIr.BulkSendAsync(_lineNumber, message, recipients, unixTimestamp);

                var result = new BulkSendResult
                {
                    PackId = bulkSendResult.Data?.PackId ?? Guid.Empty,
                    TotalCost = bulkSendResult.Data?.Cost ?? 0,
                    IsAllSentSuccess = bulkSendResult.Status == 1
                };

                if (bulkSendResult.Status == 1)
                {
                    for (int i = 0; i < recipients.Length; i++)
                    {
                        if (bulkSendResult.Data?.MessageIds[i] == null)
                        {
                            result.IsAllSentSuccess = false;

                            result.RecipientResults[recipients[i]] = new RecipientResult
                            {
                                IsSentSuccess = false,
                                MessageId = bulkSendResult.Data.MessageIds[i]?.ToString(),
                                StatusMessage = "Failed",
                                Cost = bulkSendResult.Data.Cost / recipients.Length // Assuming equal cost distribution
                            };
                        }
                        else
                        {
                            result.RecipientResults[recipients[i]] = new RecipientResult
                            {
                                IsSentSuccess = true,
                                MessageId = bulkSendResult.Data.MessageIds[i]?.ToString(),
                                StatusMessage = "Success",
                                Cost = bulkSendResult.Data.Cost / recipients.Length // Assuming equal cost distribution
                            };
                        }

                    }
                }
                else
                {
                    foreach (var recipient in recipients)
                    {
                        result.RecipientResults[recipient] = new RecipientResult
                        {
                            IsSentSuccess = false,
                            StatusMessage = bulkSendResult.Message,
                            Cost = 0
                        };
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return new BulkSendResult
                {
                    IsAllSentSuccess = false,
                    RecipientResults = recipients.ToDictionary(
                        r => r,
                        r => new RecipientResult
                        {
                            IsSentSuccess = false,
                            StatusMessage = ex.Message,
                            Cost = 0
                        }
                    )
                };
            }
        }

        public async Task<VerificationSendResult> SendVerificationSmsAsync(string phoneNumber, int templateId, VerifySendParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            }

            try
            {
                var response = await _smsIr.VerifySendAsync(phoneNumber, templateId, parameters);

                if (response.Status == 1)
                {
                    VerifySendResult sendResult = response.Data;
                    return new VerificationSendResult
                    {
                        MessageId = sendResult.MessageId,
                        Cost = sendResult.Cost,
                        StatusMessage = "Success",
                        IsSentSuccess = true
                    };
                }
                else
                {
                    return new VerificationSendResult
                    {
                        StatusMessage = response.Message,
                        IsSentSuccess = true
                    };
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return new VerificationSendResult
                {
                    StatusMessage = ex.Message
                };
            }
        }

        private void HandleException(Exception ex)
        {
            string errorName = ex.GetType().Name;
            string errorNameDescription = errorName switch
            {
                "UnauthorizedException" => "The provided token is not valid or access is denied.",
                "LogicalException" => "Please check and correct the request parameters.",
                "TooManyRequestException" => "The request count has exceeded the allowed limit.",
                "UnexpectedException" or "InvalidOperationException" => "An unexpected error occurred on the remote server.",
                _ => "Unable to send the request due to an unspecified error.",
            };

            string errorMessage = ex.Message;
            string errorDescription = "There is a problem with the request." +
                $"\n - Error: {errorName} - {errorNameDescription} - {errorMessage}";

            // Log the error or handle it as necessary for your application
            Console.Error.WriteLine(errorDescription);
        }
    }
}
