using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using static Functions.InventoryManager;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Functions
{
    public class InventoryManager
    {
        #region LOCALMODEL
        public class KALA
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
            public int ANBAR_CODE { get; set; }
            public string ANBAR_NAME { get; set; }
            public double CURRENT_MOGUDI { get; set; }
        }
        #endregion

        public TransactionManagement TM;
        private bool autoManageTransaction;


        public void ShowErrorMessages(List<MsgModel> ErrosMessages)
        {
            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                .Select(message => new MsgModel { MessageText_U = message }).ToList();
            new MsgListwin(false, ErrosMessages).ShowDialog();
        }

        // Method to start a new transaction
        public void StartTransaction()
        {
            TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);
            autoManageTransaction = false; // Disable auto management when manually starting a transaction
        }

        // Method to commit the transaction
        public void CommitTransaction()
        {
            TM?.DoCommit();
            TM = null;
        }

        // Method to rollback the transaction
        public void RollbackTransaction()
        {
            TM?.DoRollback();
            TM = null;
        }

        // Method for checking inventory only
        public (List<MsgModel> errorMessages, List<MsgModel> infoMessages, List<KALA> inventoryDetails) GetKalaMogudi(CL_CCNNMANAGER dbms, IEnumerable<object> items)
        {
            List<MsgModel> errorMessages = new List<MsgModel>();
            List<MsgModel> infoMessages = new List<MsgModel>();
            List<KALA> inventoryDetails = new List<KALA>();

            try
            {
                foreach (var item in items)
                {
                    var meghMarProperty = item.GetType().GetProperty("MEGH_MAR");
                    double MEGH_MAR = meghMarProperty != null ? (double)(meghMarProperty.GetValue(item) ?? 0) : 0;


                    string CODE = Convert.ToString(item.GetType().GetProperty("CODE").GetValue(item)); // Commodity CODE
                    int ANBAR = Convert.ToInt32(item.GetType().GetProperty("ANBAR").GetValue(item)); // Warehouse (Stock) Amount CODE

                    var MEGHkProperty = item.GetType().GetProperty("MEGHk");
                    double MEGHk = MEGHkProperty != null ? (double)(MEGHkProperty.GetValue(item) ?? 0) : 0;

                    bool HaveNotMarguee = MEGH_MAR == 0; // Has some of this invoice been returned? (return value)
                    string ANBAR_NAME = dbms.DoGetDataSQL<string>("SELECT TOP 1 NAMES FROM dbo.TCOD_ANBAR WHERE CODE = @ANBAR", new { ANBAR }).FirstOrDefault(); // Warehouse (Stock) Name
                    string NAME_CODE = dbms.DoGetDataSQL<string>("SELECT NAME FROM dbo.STUF_DEF WHERE CODE = @CODE", new { CODE }).FirstOrDefault(); // Commodity Name

                    var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT MOGODI FROM dbo.STUF_STK WHERE CODE = @CODE AND ANBAR = @ANBAR",
                                 new { CODE, ANBAR }).FirstOrDefault();

                    if (RST == null)
                    {
                        errorMessages.Add(new MsgModel
                        {
                            MessageText_U = $"اطلاعات این کالا با کد {CODE} از انبار {ANBAR_NAME} ناقص مي باشد , با پشتیبانی در ارتباط باشید."
                        });
                        continue;
                    }

                    //محاسبه موجودی واقعی این کالا
                    double min = CL_HESABDARI.Getmin(ANBAR, CODE);
                    double? MAND = dbms.DoGetDataSQL<double?>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0), 2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999, @ANBAR) AK_MOGO_AVL_KOL " +
                        "RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR " +
                        "LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, @ANBAR) AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = @ANBAR " +
                        "WHERE dbo.STUF_FSK.CODE = @CODE AND dbo.STUF_FSK.ANBAR = @ANBAR",
                        new { ANBAR, CODE }).FirstOrDefault();


                    if ((Baseknow.RMOG ?? true) || Baseknow.MOJU)
                    {
                        inventoryDetails.Add(new KALA
                        {
                            CODE = CODE,
                            NAME = NAME_CODE,
                            ANBAR_CODE = ANBAR,
                            ANBAR_NAME = ANBAR_NAME,
                            CURRENT_MOGUDI = MAND ?? 0
                        });

                        infoMessages.Add(new MsgModel
                        {
                            MessageText_U = $"کالا با کد {CODE} در انبار {ANBAR_NAME} دارای موجودی {MAND ?? 0} می‌باشد."
                        });

                        if (MAND.HasValue)
                        {
                            double remainingQty = MAND.Value - (MEGHk - (MEGHk - MEGH_MAR));
                            if (Math.Round(remainingQty, Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, (int)Baseknow.DIG) && ANBAR != 0) //انبار خدمات نباشه
                            {
                                errorMessages.Add(new MsgModel
                                {
                                    MessageText_U = $"کالا با کد {CODE} از انبار  \"{ANBAR_NAME}\" با مقدار {MEGHk} ، خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return (errorMessages, infoMessages, inventoryDetails);
        }

        // Method for checking inventory and executing a custom query with output
        public (List<MsgModel> errorMessages, List<MsgModel> infoMessages, List<KALA> inventoryDetails, List<T> queryOutputs) CheckInventoryAndExecuteQuery<T>(IEnumerable<object> items, string query, object queryParams = null, bool autoManageTransaction = true, bool isBarGashti = false)
        {
            this.autoManageTransaction = autoManageTransaction;
            return MogudiCheckInternal<T>(items, true, query, queryParams, false, isBarGashti);
        }

        // Internal method to handle the logic for both cases
        private (List<MsgModel> errorMessages, List<MsgModel> infoMessages, List<KALA> inventoryDetails, List<T> queryOutputs) MogudiCheckInternal<T>(
            IEnumerable<object> items,
            bool performQuery,
            string query,
            object queryParams,
            bool checkOnly, bool isBarGashti)
        {
            List<MsgModel> errorMessages = new List<MsgModel>();
            List<MsgModel> infoMessages = new List<MsgModel>();
            List<KALA> inventoryDetails = new List<KALA>();
            List<T> queryOutputs = new List<T>();

            bool internalTransactionStarted = false;
            int maxRetries = autoManageTransaction ? 3 : 0;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                errorMessages.Clear();
                infoMessages.Clear();
                inventoryDetails.Clear();
                queryOutputs.Clear();

                try
                {
                    if (autoManageTransaction)
                    {
                        TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);
                        internalTransactionStarted = true;
                    }
                    else if (TM == null)
                    {
                        throw new InvalidOperationException("Transaction not started. Call StartTransaction() before performing operations.");
                    }

                    foreach (var item in items)
                    {
                        var meghMarProperty = item.GetType().GetProperty("MEGH_MAR");
                        double MEGH_MAR = meghMarProperty != null ? (double)(meghMarProperty.GetValue(item) ?? 0) : 0;


                        string CODE = Convert.ToString(item.GetType().GetProperty("CODE").GetValue(item)); // Commodity CODE
                        int ANBAR = Convert.ToInt32(item.GetType().GetProperty("ANBAR").GetValue(item)); // Warehouse (Stock) Amount CODE
                        double MEGHk = Convert.ToDouble(item.GetType().GetProperty("MEGHk").GetValue(item));
                        bool HaveNotMarguee = MEGH_MAR == 0; // Has some of this invoice been returned? (return value)
                        string ANBAR_NAME = TM.SqlQueryCtc<string>("SELECT TOP 1 NAMES FROM dbo.TCOD_ANBAR WITH (NOLOCK) WHERE CODE = @ANBAR", new { ANBAR }).FirstOrDefault(); // Warehouse (Stock) Name
                        string NAME_CODE = TM.SqlQueryCtc<string>("SELECT NAME FROM dbo.STUF_DEF WITH (NOLOCK) WHERE CODE = @CODE", new { CODE }).FirstOrDefault(); // Commodity Name


                        if (isBarGashti) //فاکتور/انبار برگشت فروش
                        {
                            var RST = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM dbo.STUF_STK WHERE CODE = @CODE AND ANBAR = @ANBAR",
                               new { CODE, ANBAR }).FirstOrDefault();

                            if (RST == null)
                            {
                                errorMessages.Add(new MsgModel
                                {
                                    MessageText_U = $"اطلاعات این کالا با کد {CODE} از انبار {ANBAR_NAME} ناقص مي باشد , با پشتیبانی در ارتباط باشید."
                                });
                                continue;
                            }

                            // Execute query if provided and not in check-only mode
                            if (performQuery && !string.IsNullOrEmpty(query))
                            {
                                queryOutputs = TM.SqlQueryCtc<T>(query, queryParams).ToList(); //Main INVO_LST Query
                            }

                            //محاسبه موجودی واقعی این کالا
                            double min = CL_HESABDARI.Getmin(ANBAR, CODE);
                            double? MAND = TM.SqlQueryCtc<double?>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0), 2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999, @ANBAR) AK_MOGO_AVL_KOL " +
                                "RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR " +
                                "LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, @ANBAR) AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = @ANBAR " +
                                "WHERE dbo.STUF_FSK.CODE = @CODE AND dbo.STUF_FSK.ANBAR = @ANBAR",
                                new { ANBAR, CODE }).FirstOrDefault();

                            // Update inventory quantity in STUF_STK table if not in check-only mode
                            if (!checkOnly && ANBAR != 0) //انبار خدمات نباشه
                            {
                                if (MAND.HasValue)
                                {
                                    //بروز رسانی جدول برای موجودی موقت
                                    TM.ExecuteSqlCommandCtc("UPDATE dbo.STUF_STK SET MOGODI = @MOGODI WHERE CODE = @CODE AND ANBAR = @ANBAR",
                                        new { MOGODI = MAND.HasValue, CODE, ANBAR });

                                }
                            }

                            if ((Baseknow.RMOG ?? false) || Baseknow.MOJU)
                            {
                                inventoryDetails.Add(new KALA
                                {
                                    CODE = CODE,
                                    NAME = NAME_CODE,
                                    ANBAR_CODE = ANBAR,
                                    ANBAR_NAME = ANBAR_NAME,
                                    CURRENT_MOGUDI = MAND ?? 0
                                });

                                infoMessages.Add(new MsgModel
                                {
                                    MessageText_U = $"کالا با کد {CODE} در انبار {ANBAR_NAME} دارای موجودی {MAND ?? 0} می‌باشد."
                                });

                                //مقدار مرجوعی بیش از فروش نباشد
                                if (Math.Round(MEGH_MAR - MEGHk, 5) > 0)
                                {
                                    errorMessages.Add(new MsgModel { MessageText_U = $"کالا با کد {CODE} از انبار  \"{ANBAR_NAME}\" با مقدار {MEGHk} ، مقدار مرجوعی از مقدار فروش بیشتر است" });
                                }

                                if (MAND.HasValue)
                                {
                                    // اصلاحیه: موجودی فعلی (MAND) چون بعد از آپدیت دیتابیس خوانده شده، خودش مقدار نهایی است
                                    // بنابراین کسر کردن دوباره (MEGHk - MEGH_MAR) اشتباه است.
                                    double remainingQty = MAND.Value; // تغییر از: MAND.Value - (MEGHk - MEGH_MAR);

                                    if (Math.Round(remainingQty, (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && ANBAR != 0)
                                    {
                                        errorMessages.Add(new MsgModel
                                        {
                                            MessageText_U = $"کالا با کد {CODE} از انبار \"{ANBAR_NAME}\" با مقدار {MEGHk}، خروج کالا از انبار موجودی را به مقدار غیر مجاز کاهش میدهد"
                                        });
                                    }
                                }

                                //if (MAND.HasValue)
                                //{
                                //    double remainingQty = MAND.Value - (MEGHk - MEGH_MAR); //این خط غلطه چون ما توی حالت ایزوله اجازه ثبت و مانده رو میگیریم این منطقی نیست
                                //    if (Math.Round(remainingQty, (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && ANBAR != 0) //انبار خدمات نباشه
                                //    {
                                //        errorMessages.Add(new MsgModel
                                //        {
                                //            MessageText_U = $"کالا با کد {CODE} از انبار  \"{ANBAR_NAME}\" با مقدار {MEGHk} ، خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد"
                                //        });
                                //    }
                                //}
                            }
                        }
                        else //فاکتور/انبار معمولی
                        {
                            if (HaveNotMarguee) // If no returns
                            {
                                var RST = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT MOGODI FROM dbo.STUF_STK WHERE CODE = @CODE AND ANBAR = @ANBAR",
                                    new { CODE, ANBAR }).FirstOrDefault();

                                if (RST == null)
                                {
                                    errorMessages.Add(new MsgModel
                                    {
                                        MessageText_U = $"اطلاعات این کالا با کد {CODE} از انبار {ANBAR_NAME} ناقص مي باشد , با پشتیبانی در ارتباط باشید."
                                    });
                                    continue;
                                }

                                // Execute query if provided and not in check-only mode
                                if (performQuery && !string.IsNullOrEmpty(query))
                                {
                                    queryOutputs = TM.SqlQueryCtc<T>(query, queryParams).ToList(); //Main INVO_LST Query
                                }

                                //محاسبه موجودی واقعی این کالا
                                double min = CL_HESABDARI.Getmin(ANBAR, CODE);
                                double? MAND = TM.SqlQueryCtc<double?>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0), 2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999, @ANBAR) AK_MOGO_AVL_KOL " +
                                    "RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR " +
                                    "LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, @ANBAR) AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = @ANBAR " +
                                    "WHERE dbo.STUF_FSK.CODE = @CODE AND dbo.STUF_FSK.ANBAR = @ANBAR",
                                    new { ANBAR, CODE }).FirstOrDefault();


                                // Update inventory quantity in STUF_STK table if not in check-only mode
                                if (!checkOnly && ANBAR != 0) //انبار خدمات نباشه
                                {
                                    if (MAND.HasValue)
                                    {
                                        //بروز رسانی جدول برای موجودی موقت
                                        TM.ExecuteSqlCommandCtc("UPDATE dbo.STUF_STK SET MOGODI = @MOGODI WHERE CODE = @CODE AND ANBAR = @ANBAR",
                                            new { MOGODI = MAND.HasValue, CODE, ANBAR });

                                    }
                                }

                                if ((bool)Baseknow.RMOG || Baseknow.MOJU)
                                {
                                    inventoryDetails.Add(new KALA
                                    {
                                        CODE = CODE,
                                        NAME = NAME_CODE,
                                        ANBAR_CODE = ANBAR,
                                        ANBAR_NAME = ANBAR_NAME,
                                        CURRENT_MOGUDI = MAND ?? 0
                                    });

                                    infoMessages.Add(new MsgModel
                                    {
                                        MessageText_U = $"کالا با کد {CODE} در انبار {ANBAR_NAME} دارای موجودی {MAND ?? 0} می‌باشد."
                                    });

                                    if (MAND.HasValue)
                                    {
                                        double remainingQty = MAND.Value - (MEGHk - (MEGHk - MEGH_MAR));
                                        if (Math.Round(remainingQty, (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && ANBAR != 0) //انبار خدمات نباشه
                                        {
                                            errorMessages.Add(new MsgModel
                                            {
                                                MessageText_U = $"کالا با کد {CODE} از انبار  \"{ANBAR_NAME}\" با مقدار {MEGHk} ، خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد"
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                errorMessages.Add(new MsgModel
                                {
                                    MessageText_U = "این فاکتور دارای مرجوعی است و نمیتوانید آنرا حذف کنید"
                                });
                                break; // Get out regardless of whole loop items
                            }
                        }

                    }

                    if (internalTransactionStarted)
                    {
                        if (errorMessages.Count == 0)
                        {
                            CommitTransaction();
                        }
                        else
                        {
                            RollbackTransaction();
                        }
                    }

                    return (errorMessages, infoMessages, inventoryDetails, queryOutputs);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (internalTransactionStarted && ex.Number == 1205 && attempt < maxRetries)
                {
                    if (internalTransactionStarted)
                    {
                        RollbackTransaction();
                    }
                    System.Threading.Thread.Sleep(200 * (attempt + 1));
                    // loop continues
                }
                catch (Exception)
                {
                    if (internalTransactionStarted)
                    {
                        RollbackTransaction();
                    }
                    throw;
                }
            }

            return (errorMessages, infoMessages, inventoryDetails, queryOutputs);
        }

    }


}