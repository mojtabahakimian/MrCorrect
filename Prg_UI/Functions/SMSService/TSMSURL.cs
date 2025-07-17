using Interfaces;
using IPE.SmsIrClient.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using static Functions.SMSService.MSGMODEL;
using System.Threading.Tasks;
using System.Web;

namespace Functions.SMSService
{
    public class TSMSURL : ISmsService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _from;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://tsms.ir/url/tsmshttp.php";

        public TSMSURL(string username, string password, string from_linenumber, HttpClient httpClient)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _from = from_linenumber ?? throw new ArgumentNullException(nameof(from_linenumber));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new Exception("No internet connection available. Some features may not work");
            }
        }

        // Implement the interface methods
        public async Task<BulkSendResult> SendBulkSmsAsync(string message, string[] recipients, DateTime? sendDateTime = null)
        {
            var bulkSendResult = new BulkSendResult
            {
                IsAllSentSuccess = true,
                TotalCost = 0
            };

            foreach (var batch in BatchRecipients(recipients))
            {
                var batchResult = await SendSMSToMultipleRecipientsAsync(batch, message);
                var messageIds = batchResult.Split(',');

                for (int i = 0; i < batch.Length; i++)
                {
                    var recipient = batch[i];
                    var messageId = i < messageIds.Length ? messageIds[i] : null;

                    var statusMessage = await HandleSendSMSResponseAsync(messageId ?? "Unknown error");
                    var isSentSuccess = !string.IsNullOrEmpty(messageId) && !statusMessage.Contains(SMSPINFO.ERRORKEY);

                    var recipientResult = new RecipientResult
                    {
                        IsSentSuccess = isSentSuccess,
                        StatusMessage = statusMessage,
                        MessageId = isSentSuccess ? messageId : null,
                        // Assuming each successful message costs 1 credit. Adjust as needed.
                        Cost = isSentSuccess ? 1 : 0
                    };

                    if (recipientResult.IsSentSuccess)
                    {
                        bulkSendResult.TotalCost += recipientResult.Cost;
                    }
                    else
                    {
                        bulkSendResult.IsAllSentSuccess = false;
                    }

                    bulkSendResult.RecipientResults[recipient] = recipientResult;
                }
            }

            return bulkSendResult;
        }
        public async Task<string> SendSMSToMultipleRecipientsAsync(IEnumerable<string> recipients, string message)
        {
            if (recipients.Count() > 20)
            {
                throw new ArgumentException("Maximum 20 recipients allowed.");
            }

            var to = string.Join(",", recipients);
            return await SendSMSAsync(to, message);
        }

        public async Task<VerificationSendResult> SendVerificationSmsAsync(string phoneNumber, int templateId, VerifySendParameter[] parameters)
        {
            throw new Exception("TSMS Does not have Veryfi SMS Service");

            //// TSMSURL doesn't have a direct equivalent, so we'll create a simple implementation
            //var message = $"Verification code: {parameters[0].Value}"; // Assuming the first parameter is the verification code
            //var result = await SendSMSAsync(phoneNumber, message);
            //return new VerificationSendResult
            //{
            //    StatusMessage = result,
            //    IsSentSuccess = !result.StartsWith("Error"),
            //    // Other properties can be set as needed
            //};
        }
        public async Task<string> SendSMSAsync(string to, string message)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("Recipient number cannot be null or whitespace.", nameof(to));
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));

            //if (message.Length > 70)
            //    throw new ArgumentException("Message Length cannot be more that 70 1.", nameof(message));

            var TOLST = to.Split(',');
            var modifiedTOLST = new List<string>();
            foreach (string item in TOLST)
            {
                if (!item.Trim().StartsWith('0'))
                {
                    modifiedTOLST.Add("0" + item);
                }
                else
                {
                    modifiedTOLST.Add(item);
                }
            }

            to = string.Join(",", modifiedTOLST);

            var parameters = new Dictionary<string, string>
            {
                { "to", to },
                { "message", message }
            };
            var response = await SendRequestAsync(parameters);
            //return await HandleSendSMSResponseAsync(response);
            return response;
        }

        /// <summary>
        /// Only 20 recipients allowed at a call 
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>

        public async Task<string> GetCreditAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                { "credit", "what" }
            };
            var response = await SendRequestAsync(parameters);
            return await HandleCreditResponseAsync(response);
        }

        public async Task<string> GetDeliveryStatusAsync(string smsId)
        {
            if (string.IsNullOrWhiteSpace(smsId)) throw new ArgumentException("SMS ID cannot be null or whitespace.", nameof(smsId));

            var parameters = new Dictionary<string, string>
            {
                { "deliver20", smsId }
            };
            var response = await SendRequestAsync(parameters);
            return await HandleDeliveryStatusResponseAsync(response);
        }

        public async Task<string> GetMultipleDeliveryStatusAsync(IEnumerable<string> smsIds)
        {
            if (smsIds.Count() > 20)
            {
                throw new ArgumentException("Maximum 20 SMS IDs allowed.");
            }

            var deliverParameter = string.Join(",", smsIds.Select(id => $"deliver20={id}"));
            var parameters = new Dictionary<string, string>
            {
                { "deliver20", deliverParameter }
            };
            var response = await SendRequestAsync(parameters);

            return await HandleDeliveryStatusResponseAsync(response);

            //var deliveryTasks = smsIds.Select(GetDeliveryStatusAsync);
            //var deliveryResults = await Task.WhenAll(deliveryTasks);
            //return string.Join(Environment.NewLine, deliveryResults);
        }

        private async Task<string> SendRequestAsync(Dictionary<string, string> parameters)
        {
            var url = BuildUrl(parameters);
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to communicate with TSMS service.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("unexpected error on SendRequestAsync.", ex);
            }
        }

        private IEnumerable<string[]> BatchRecipients(string[] recipients, int batchSize = 20)
        {
            for (int i = 0; i < recipients.Length; i += batchSize)
            {
                yield return recipients.Skip(i).Take(batchSize).ToArray();
            }
        }
        private string BuildUrl(Dictionary<string, string> parameters)
        {
            var builder = new StringBuilder(BaseUrl);
            builder.Append($"?from={HttpUtility.UrlEncode(_from)}");
            builder.Append($"&username={HttpUtility.UrlEncode(_username)}");
            builder.Append($"&password={HttpUtility.UrlEncode(_password)}");
            foreach (var parameter in parameters)
            {
                builder.Append($"&{parameter.Key}={HttpUtility.UrlEncode(parameter.Value)}");
            }
            string url = builder.ToString();
            Console.WriteLine($"Debug: API URL = {url}");  // Log the URL
            return url;
        }

        private async Task<string> HandleSendSMSResponseAsync(string response)
        {
            switch (response)
            {
                case "1":
                    return "خطا در سرور توبا" + SMSPINFO.ERRORKEY;
                case "2":
                    return "UDH has more than 6 characters" + SMSPINFO.ERRORKEY; ;
                case "3":
                    return "شماره موبایل صحیح نیست" + SMSPINFO.ERRORKEY; ;
                case "4":
                    return "Error in sending variables" + SMSPINFO.ERRORKEY; ;
                case "5":
                    return "متن ارسال شده خالی است" + SMSPINFO.ERRORKEY; ;
                case "6":
                    return "شماره موبایل خالی است" + SMSPINFO.ERRORKEY; ;
                case "7":
                    return "اطلاعات درست نیست , لطفا شماره خط , نام کاربری و رمز عبور خود را بررسی کنید" + SMSPINFO.ERRORKEY; ;
                case "8":
                    return "خطا در سرور توبا (ترافیک شدید) لطفا مجددا تلاش کنید" + SMSPINFO.ERRORKEY; ;
                case "9":
                    return "سرویس ارسال اس ام اس متوقف شد !" + SMSPINFO.ERRORKEY; ;
                case "10":
                    return "برای ارسال ییام اعتبار کافی نیست!" + SMSPINFO.ERRORKEY; ;
                default:
                    return $"{response}"; //Success Response: 
            }
        }

        private async Task<string> HandleCreditResponseAsync(string response)
        {
            if (int.TryParse(response, out int credit))
            {
                return $"Remaining credit: {credit}";
            }
            else
            {
                return $"Failed to retrieve credit. Response: {response}";
            }
        }

        private async Task<string> HandleDeliveryStatusResponseAsync(string response)
        {
            switch (response)
            {
                case "1":
                    return "Delivered to phone";
                case "2":
                    return "Not delivered to phone";
                case "5":
                    return "Delivery not received yet";
                case "6":
                    return "Date expired (maximum 3 days)";
                case "9":
                    return "Delivery not received yet or code is incorrect";
                default:
                    return $"Unknown delivery status. Response: {response}";
            }
        }
    }
}
