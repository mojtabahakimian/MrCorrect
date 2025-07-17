using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using static Functions.SMSService.MSGMODEL;
using AUTO_BAZ.HelperWins;
using System.Threading.Tasks;
using Prg_SendInvoice.CNNMANAGER;
using System.Windows;
using Prg_Proccessy.Generaly;

namespace Functions.SMSService
{
    public class CL_SMSAC
    {
        public async Task<BulkSendResult> ErselSmsAsync(string hes, string matn, long number, int tag, bool fl = false, bool IsGroupMessage = false)
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

            BulkSendResult bulkResult = null;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    if ((bool)Baseknow.SMSACT == false)
                    {
                        //ارسال پیامک فعال نیست!
                        return bulkResult;
                    }

                    if ((bool)Baseknow.PRMFR && !IsGroupMessage)
                    {
                        // Capture the result from the dialog
                        bool shouldSendSms = await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Msgwin msgwin = new Msgwin(true, "آیا پیامک ارسال شود ؟");
                            msgwin.ShowDialog();
                            return msgwin.DialogResult == true;
                        });
                        if (!shouldSendSms)
                        {
                            // Exit the method early if the user cancels
                            return bulkResult; // or return null as desired
                        }
                    }

                    SmsManager smsManager = new SmsManager(SMSPINFO.SERVICE_TYPE, SMSPINFO.API_KEY, SMSPINFO.USERNAME, SMSPINFO.PASSWORD, SMSPINFO.LINE_NUMBER, httpClient);

                    // Fetch customer details
                    var customerDetails = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT * FROM CUST_HESAB WHERE hes = '{hes}'").FirstOrDefault();

                    if (customerDetails != null)
                    {
                        if (!string.IsNullOrEmpty(customerDetails.MOBILE))
                        {
                            var smsSendRecord = new SMS_SENDS
                            {
                                SM_DTQ = CL_HESABDARI.FARSIDATE(),
                                SM_TTQ = CL_HESABDARI.GTFS(),
                                SM_AMobiles = customerDetails.MOBILE,
                                AMSG = matn,
                                NUMBER = number,
                                TAGS = tag,
                                USERNAME = (string)CL_HESABDARI.UCurrentUser(),
                                PC_NAME = CL_HESABDARI.CurrentMachineName(),
                                IPADD = CL_HESABDARI.GETIPADD(),
                                CUST_NO = hes
                            };

                            long idSms = 0;
                            List<SmsResultRecord> resultRecords = null;

                            if ((bool)Baseknow.DSMS)  //ارسال پیامک فقط از طریق سرور انجام شود
                            {
                                if (CL_Generaly.I_AM_SERVER)
                                {
                                    // Send SMS
                                    bulkResult = await smsManager.SendBulkSms(matn, new[] { customerDetails.MOBILE });
                                    resultRecords = SmsResultProcessor.ConvertToRecords(bulkResult);
                                    if ((bool)(resultRecords.FirstOrDefault()?.IsSentSuccess))
                                    {
                                        if (resultRecords.FirstOrDefault()?.MessageId != null)
                                        {
                                            idSms = Convert.ToInt32(resultRecords.FirstOrDefault()?.MessageId);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Send SMS
                                bulkResult = await smsManager.SendBulkSms(matn, new[] { customerDetails.MOBILE });
                                resultRecords = SmsResultProcessor.ConvertToRecords(bulkResult);
                                if ((bool)(resultRecords.FirstOrDefault()?.IsSentSuccess))
                                {
                                    if (resultRecords.FirstOrDefault()?.MessageId != null)
                                    {
                                        idSms = Convert.ToInt32(resultRecords.FirstOrDefault()?.MessageId);
                                    }
                                }
                            }

                            if (idSms < 0)
                            {
                                string resultSummary = SmsResultProcessor.GenerateResultSummary(bulkResult);
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    new Msgwin(false, resultSummary).Show();
                                });

                                smsSendRecord.STATUSSMS = (int)idSms;
                            }
                            else
                            {
                                smsSendRecord.STATUSSMS = Convert.ToByte(resultRecords?.FirstOrDefault()?.IsSentSuccess);
                                smsSendRecord.SM_DT = CL_HESABDARI.FARSIDATE();
                                smsSendRecord.SM_TT = CL_HESABDARI.GTFS();
                                smsSendRecord.id_sms = idSms.ToString();
                            }

                            // Insert SMS_SENDS record
                            await dbms.DoExecuteSQLAsync("INSERT INTO SMS_SENDS (SM_DTQ, SM_TTQ, SM_DT, SM_TT, SM_AMobiles, AMSG, NUMBER, TAGS, USERNAME, PC_NAME, IPADD, STATUSSMS, id_sms, CUST_NO) VALUES (@SM_DTQ, @SM_TTQ, @SM_DT, @SM_TT, @SM_AMobiles, @AMSG, @NUMBER, @TAGS, @USERNAME, @PC_NAME, @IPADD, @STATUSSMS, @id_sms, @CUST_NO)", smsSendRecord);
                        }
                        else
                        {
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                new Msgwin(false, "شماره  موبايل براي اين حساب تعريف نشده است جهت ارسال اس ام اس بايد شماره موبايل تعريف كنيد.").ShowDialog();
                            });

                            // Insert record for missing mobile number
                            var smsSendRecord = new SMS_SENDS
                            {
                                SM_DTQ = CL_HESABDARI.FARSIDATE(),
                                SM_TTQ = CL_HESABDARI.GTFS(),
                                SM_AMobiles = "0",
                                AMSG = matn,
                                NUMBER = number,
                                TAGS = tag,
                                USERNAME = (string)CL_HESABDARI.UCurrentUser(),
                                PC_NAME = CL_HESABDARI.CurrentMachineName(),
                                IPADD = CL_HESABDARI.GETIPADD(),
                                STATUSSMS = 0,
                                CUST_NO = hes
                            };

                            await dbms.DoExecuteSQLAsync("INSERT INTO SMS_SENDS (SM_DTQ, SM_TTQ, SM_AMobiles, AMSG, NUMBER, TAGS, USERNAME, PC_NAME, IPADD, STATUSSMS, CUST_NO) VALUES (@SM_DTQ, @SM_TTQ, @SM_AMobiles, @AMSG, @NUMBER, @TAGS, @USERNAME, @PC_NAME, @IPADD, @STATUSSMS, @CUST_NO)", smsSendRecord);
                        }
                    }

                    dbms = null;
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در انجام علملیات SMS").Show();
            }

            return bulkResult;
        }
    }
}