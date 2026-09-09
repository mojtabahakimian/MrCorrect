using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;

namespace Prg_UI.Wins.WinSetting
{
    public partial class WIN_SYS_AUDIT_EXPLORER : Window
    {
        private readonly CL_CCNNMANAGER _dbms = new CL_CCNNMANAGER();

        public WIN_SYS_AUDIT_EXPLORER()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var pc = new PersianCalendar();
                var now = DateTime.Now;
                long pDate = (long)pc.GetYear(now) * 10000 + pc.GetMonth(now) * 100 + pc.GetDayOfMonth(now);
                TXT_PERSIAN_DATE.Text = pDate.ToString();

                var users = _dbms.DoGetDataSQL<SALA_DTL>("SELECT IDD, SAL_NAME FROM SALA_DTL ORDER BY SAL_NAME").ToList();
                CMB_USERS.ItemsSource = users;
                if (Baseknow.USERCOD.HasValue)
                {
                    CMB_USERS.SelectedValue = Baseknow.USERCOD.Value;
                }

                ExecuteSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اولیه: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterMode_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            bool isUserMode = RAD_USER_MODE.IsChecked ?? true;
            CMB_USERS.IsEnabled = isUserMode;
            TXT_PERSIAN_DATE.IsEnabled = isUserMode;

            TXT_DOC_NUMBER.IsEnabled = !isUserMode;
            CMB_DOC_TAG.IsEnabled = !isUserMode;
        }

        private void BTN_SEARCH_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSearch();
        }

        private void ExecuteSearch()
        {
            try
            {
                using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();

                    string sql = "";
                    DynamicParameters parameters = new DynamicParameters();

                    string selectedActionType = (CMB_ACTION_TYPE.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

                    if (RAD_USER_MODE.IsChecked == true)
                    {
                        sql = @"
                            SELECT TOP 1000
                                LogId, PersianDate, LogTime, ServerDateTime, UserId, UserName,
                                ClientIP, MachineName, SessionId, ModuleTitle, FormClass,
                                ActionType, ActionCaption, EntityName, DocNumber, DocTag,
                                DetailsJson, Severity
                            FROM dbo.SYS_AUDIT_TRAIL WITH (NOLOCK)
                            WHERE 1=1 ";

                        if (CMB_USERS.SelectedValue != null && Convert.ToInt32(CMB_USERS.SelectedValue) > 0)
                        {
                            sql += " AND UserId = @UserId ";
                            parameters.Add("UserId", Convert.ToInt32(CMB_USERS.SelectedValue));
                        }

                        if (long.TryParse(TXT_PERSIAN_DATE.Text?.Trim(), out long pDate) && pDate > 0)
                        {
                            sql += " AND PersianDate = @PersianDate ";
                            parameters.Add("PersianDate", pDate);
                        }

                        if (!string.IsNullOrEmpty(selectedActionType))
                        {
                            sql += " AND ActionType LIKE @ActionType ";
                            parameters.Add("ActionType", $"%{selectedActionType}%");
                        }

                        sql += " ORDER BY LogId DESC";
                    }
                    else
                    {
                        sql = @"
                            SELECT TOP 1000
                                LogId, PersianDate, LogTime, ServerDateTime, UserId, UserName,
                                ClientIP, MachineName, SessionId, ModuleTitle, FormClass,
                                ActionType, ActionCaption, EntityName, DocNumber, DocTag,
                                DetailsJson, Severity
                            FROM dbo.SYS_AUDIT_TRAIL WITH (NOLOCK)
                            WHERE 1=1 ";

                        if (double.TryParse(TXT_DOC_NUMBER.Text?.Trim(), out double docNum) && docNum > 0)
                        {
                            sql += " AND DocNumber = @DocNumber ";
                            parameters.Add("DocNumber", docNum);
                        }

                        string selectedTag = (CMB_DOC_TAG.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
                        if (double.TryParse(selectedTag, out double docTag) && docTag > 0)
                        {
                            sql += " AND DocTag = @DocTag ";
                            parameters.Add("DocTag", docTag);
                        }

                        if (!string.IsNullOrEmpty(selectedActionType))
                        {
                            sql += " AND ActionType LIKE @ActionType ";
                            parameters.Add("ActionType", $"%{selectedActionType}%");
                        }

                        sql += " ORDER BY LogId DESC";
                    }

                    var results = db.Query<AuditEntryModel>(sql, parameters).ToList();
                    DG_AUDIT.ItemsSource = results;
                    TXT_RECORD_COUNT.Text = $"تعداد رکوردها: {results.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در دریافت اطلاعات سوابق: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTN_CLEAR_Click(object sender, RoutedEventArgs e)
        {
            TXT_DOC_NUMBER.Text = "";
            CMB_DOC_TAG.SelectedIndex = 0;
            CMB_ACTION_TYPE.SelectedIndex = 0;
            TXT_DETAILS_JSON.Text = "";
            ExecuteSearch();
        }

        private void DG_AUDIT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_AUDIT.SelectedItem is AuditEntryModel selected)
            {
                TXT_DETAILS_JSON.Text = selected.DetailsJson ?? "";
            }
            else
            {
                TXT_DETAILS_JSON.Text = "";
            }
        }

        private void BTN_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
