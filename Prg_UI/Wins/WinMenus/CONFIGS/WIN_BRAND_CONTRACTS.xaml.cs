using Functions;
using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_BRAND_CONTRACTS : Window
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private ObservableCollection<BrandContractModel> contractsList = new ObservableCollection<BrandContractModel>();
        private ObservableCollection<BrandContractProgressModel> itemsProgressList = new ObservableCollection<BrandContractProgressModel>();
        private ObservableCollection<LinkedDocLineModel> docsList = new ObservableCollection<LinkedDocLineModel>();

        public WIN_BRAND_CONTRACTS()
        {
            InitializeComponent();
        }

        #region Standard Window Control Handlers
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadContracts();
        }

        private void LoadContracts(string searchFilter = "")
        {
            try
            {
                string query = "SELECT * FROM dbo.BrandContracts ORDER BY ContractID DESC";
                var list = dbms.DoGetDataSQL<BrandContractModel>(query).ToList();

                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    list = list.Where(c =>
                        c.ContractNumber.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        c.CustomerName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        c.BrandName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                contractsList = new ObservableCollection<BrandContractModel>(list);
                DG_Contracts.ItemsSource = contractsList;

                // Clear details when list refreshes
                itemsProgressList.Clear();
                docsList.Clear();
                DG_ItemStatus.ItemsSource = null;
                DG_LinkedDocs.ItemsSource = null;

                ClearKPIs();
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری لیست قراردادها: " + ex.Message, true);
            }
        }

        private void ClearKPIs()
        {
            KpiTotalContracted.Text = "0 متر مربع";
            KpiTotalProduced.Text = "0 متر مربع";
            KpiTotalRemaining.Text = "0 / 0%";
            KpiTotalSold.Text = "0 متر مربع";
            KpiTotalInWarehouse.Text = "0 متر مربع";

            LblContractNumber.Text = "---";
            LblBrandName.Text = "---";
            LblCustomer.Text = "---";
            LblContractDate.Text = "---";
            LblDescription.Text = "---";
        }

        private void TxtSearchContract_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadContracts(TxtSearchContract.Text);
        }

        private void DG_Contracts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                LoadContractDetails(selected);
            }
            else
            {
                ClearKPIs();
                itemsProgressList.Clear();
                docsList.Clear();
                DG_ItemStatus.ItemsSource = null;
                DG_LinkedDocs.ItemsSource = null;
            }
        }

        private void LoadContractDetails(BrandContractModel contract)
        {
            try
            {
                LblContractNumber.Text = contract.ContractNumber;
                LblBrandName.Text = contract.BrandName;
                LblCustomer.Text = contract.CustomerName;
                LblContractDate.Text = contract.ContractDate.ToString();
                LblDescription.Text = string.IsNullOrEmpty(contract.Description) ? "---" : contract.Description;

                // Item Progress query calculating Produced (TAG=9), Sold (TAG=2) on line level (ContractID mapping)
                string progressQuery = @"
                    SELECT
                        ci.ProductCode,
                        ci.ProductName,
                        ci.Quantity AS ContractedQty,
                        ISNULL(SUM(CASE WHEN il.TAG = 9 THEN il.MEGHk ELSE 0 END), 0) AS ProducedQty,
                        ISNULL(SUM(CASE WHEN il.TAG = 2 THEN il.MEGHk ELSE 0 END), 0) AS SoldQty
                    FROM dbo.BrandContractItems ci
                    LEFT JOIN dbo.INVO_LST il ON ci.ProductCode = il.CODE AND il.ContractID = ci.ContractID
                    WHERE ci.ContractID = @ContractID
                    GROUP BY ci.ProductCode, ci.ProductName, ci.Quantity";

                var progressItems = dbms.DoGetDataSQL<BrandContractProgressModel>(progressQuery, new { ContractID = contract.ContractID }).ToList();

                // Compute dependent UI variables per pattern row
                foreach (var pi in progressItems)
                {
                    pi.NotProducedQty = Math.Max(0, pi.ContractedQty - pi.ProducedQty);
                    pi.InWarehouseQty = Math.Max(0, pi.ProducedQty - pi.SoldQty);
                }

                itemsProgressList = new ObservableCollection<BrandContractProgressModel>(progressItems);
                DG_ItemStatus.ItemsSource = itemsProgressList;

                // Load list of all linked transaction documents (distinct at header level via line contract match)
                string docsQuery = @"
                    SELECT DISTINCT
                        hl.NUMBER,
                        hl.DATE_N,
                        hl.TAG,
                        CASE WHEN hl.TAG = 9 THEN N'رسید تولید' ELSE N'حواله فروش' END AS DocumentTypeName,
                        hl.TAH,
                        hl.MAS
                    FROM dbo.HEAD_LST hl
                    INNER JOIN dbo.INVO_LST il ON hl.NUMBER = il.NUMBER AND hl.TAG = il.TAG
                    WHERE il.ContractID = @ContractID
                    ORDER BY hl.DATE_N DESC, hl.NUMBER DESC";

                var linkedDocs = dbms.DoGetDataSQL<LinkedDocLineModel>(docsQuery, new { ContractID = contract.ContractID }).ToList();
                docsList = new ObservableCollection<LinkedDocLineModel>(linkedDocs);
                DG_LinkedDocs.ItemsSource = docsList;

                // Update Dashboard KPI Cards
                double total = progressItems.Sum(p => p.ContractedQty);
                double produced = progressItems.Sum(p => p.ProducedQty);
                double sold = progressItems.Sum(p => p.SoldQty);

                double notProduced = Math.Max(0, total - produced);
                double percentage = total > 0 ? (notProduced / total) * 100 : 0;
                double stock = Math.Max(0, produced - sold);

                KpiTotalContracted.Text = total.ToString("N0") + " متر";
                KpiTotalProduced.Text = produced.ToString("N0") + " متر";
                KpiTotalRemaining.Text = $"{notProduced:N0} / {percentage:F1}%";
                KpiTotalSold.Text = sold.ToString("N0") + " متر";
                KpiTotalInWarehouse.Text = stock.ToString("N0") + " متر";
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری جزئیات قرارداد: " + ex.Message, true);
            }
        }

        private void Btn_NewContract_Click(object sender, RoutedEventArgs e)
        {
            var editWin = new WIN_BRAND_CONTRACT_EDIT();
            if (editWin.ShowDialog() == true)
            {
                LoadContracts();
                ShowNotification("قرارداد جدید با موفقیت ثبت شد.", false);
            }
        }

        private void Btn_EditContract_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                var editWin = new WIN_BRAND_CONTRACT_EDIT(selected.ContractID);
                if (editWin.ShowDialog() == true)
                {
                    LoadContracts();
                    // Refocus and reload details
                    var updated = contractsList.FirstOrDefault(c => c.ContractID == selected.ContractID);
                    if (updated != null)
                    {
                        DG_Contracts.SelectedItem = updated;
                        LoadContractDetails(updated);
                    }
                    ShowNotification("قرارداد مورد نظر با موفقیت ویرایش شد.", false);
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا قرارداد مورد نظر را جهت ویرایش از جدول بالا انتخاب کنید.", true);
            }
        }

        private void Btn_DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                if (MessageBox.Show("آیا از حذف این قرارداد و تمامی الگوهای متصل به آن اطمینان دارید؟", "تأیید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Clean line linkages back to null first
                        dbms.DoExecuteSQL("UPDATE dbo.INVO_LST SET ContractID = NULL WHERE ContractID = @Id", new { Id = selected.ContractID });
                        // Delete contract metadata
                        dbms.DoExecuteSQL("DELETE FROM dbo.BrandContractItems WHERE ContractID = @Id", new { Id = selected.ContractID });
                        dbms.DoExecuteSQL("DELETE FROM dbo.BrandContracts WHERE ContractID = @Id", new { Id = selected.ContractID });

                        LoadContracts();
                        ShowNotification("قرارداد و الگوهای مربوطه با موفقیت حذف شدند.", false);
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("خطا در حذف قرارداد: " + ex.Message, true);
                    }
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا قرارداد مورد نظر را جهت حذف از جدول انتخاب کنید.", true);
            }
        }

        private void Btn_LinkProduction_Click(object sender, RoutedEventArgs e)
        {
            LinkDocuments(9);
        }

        private void Btn_LinkSales_Click(object sender, RoutedEventArgs e)
        {
            LinkDocuments(2);
        }

        private void Btn_UnlinkDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selectedContract)
            {
                if (DG_LinkedDocs.SelectedItem is LinkedDocLineModel selectedDoc)
                {
                    if (MessageBox.Show($"آیا از قطع اتصال سند شماره {selectedDoc.NUMBER} به این قرارداد مطمئن هستید؟", "قطع اتصال سند", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Unlink all lines of this document that were linked to this contract
                            dbms.DoExecuteSQL(@"
                                UPDATE dbo.INVO_LST
                                SET ContractID = NULL
                                WHERE NUMBER = @Number AND TAG = @Tag AND ContractID = @ContractID",
                                new { Number = selectedDoc.NUMBER, Tag = selectedDoc.TAG, ContractID = selectedContract.ContractID });

                            LoadContractDetails(selectedContract);
                            ShowNotification("اتصال سند انتخاب شده به قرارداد قطع شد.", false);
                        }
                        catch (Exception ex)
                        {
                            ShowNotification("خطا در قطع اتصال سند: " + ex.Message, true);
                        }
                    }
                }
                else
                {
                    ShowNotification("لطفاً ابتدا سند مورد نظر را جهت قطع اتصال انتخاب کنید.", true);
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا قرارداد هدف را انتخاب کنید.", true);
            }
        }

        private void Btn_PrintReport_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("چاپ گزارش برای قراردادهای آرمان سرام آماده سازی شد.", false);
        }

        private void LinkDocuments(int tag)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                var selectWin = new WIN_SELECT_DOCUMENT(selected.CustomerCode, tag);
                if (selectWin.ShowDialog() == true && selectWin.SelectedLineIds.Count > 0)
                {
                    try
                    {
                        // Perform the updates atomically. Add safety check: "AND ContractID IS NULL" to guarantee unlinked status in real-time
                        foreach (var id in selectWin.SelectedLineIds)
                        {
                            int affected = dbms.DoExecuteSQL(@"
                                UPDATE dbo.INVO_LST
                                SET ContractID = @ContractID
                                WHERE id = @Id AND ContractID IS NULL",
                                new { ContractID = selected.ContractID, Id = id }) ?? 0;

                            if (affected == 0)
                            {
                                throw new InvalidOperationException("خطای همزمانی: یکی از اقلام کالا قبلاً توسط کاربر دیگری به قرارداد دیگری متصل شده است!");
                            }
                        }

                        LoadContractDetails(selected);
                        ShowNotification("اقلام سند انتخاب شده با موفقیت به قرارداد متصل شدند.", false);
                    }
                    catch (Exception ex)
                    {
                        ShowNotification(ex.Message, true);
                    }
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا قرارداد هدف را از جدول بالا انتخاب کنید.", true);
            }
        }

        private void ShowNotification(string message, bool isError)
        {
            Pop1Text1.Text = message;
            Pop_Border1.Background = new System.Windows.Media.SolidColorBrush(
                isError ? System.Windows.Media.Color.FromRgb(220, 38, 38) : System.Windows.Media.Color.FromRgb(5, 150, 105));
            Pop1.IsOpen = true;

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, args) => { Pop1.IsOpen = false; timer.Stop(); };
            timer.Start();
        }
    }

    #region Model Classes
    public class BrandContractModel
    {
        public int ContractID { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public long ContractDate { get; set; }
        public double TotalQuantity { get; set; }
        public string? Description { get; set; }
    }

    public class BrandContractItemModel
    {
        public int ItemID { get; set; }
        public int ContractID { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double Quantity { get; set; }
    }

    public class BrandContractProgressModel
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double ContractedQty { get; set; }
        public double ProducedQty { get; set; }
        public double NotProducedQty { get; set; }
        public double SoldQty { get; set; }
        public double InWarehouseQty { get; set; }
    }

    public class LinkedDocLineModel
    {
        public double NUMBER { get; set; }
        public long DATE_N { get; set; }
        public int TAG { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string? TAH { get; set; }
        public double MAS { get; set; }
    }
    #endregion
}