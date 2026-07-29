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

        public WIN_BRAND_CONTRACTS()
        {
            InitializeComponent();
        }

        #region Standard Header Handling
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
        }

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadContracts();
        }

        public void LoadContracts(string searchFilter = "")
        {
            try
            {
                string query = "SELECT * FROM dbo.BrandContracts ORDER BY ContractID DESC";
                var list = dbms.DoGetDataSQL<BrandContractModel>(query).ToList();

                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    list = list.Where(c =>
                        (c.ContractNumber != null && c.ContractNumber.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (c.BrandName != null && c.BrandName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (c.CustomerName != null && c.CustomerName.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (c.CustomerCode != null && c.CustomerCode.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                contractsList = new ObservableCollection<BrandContractModel>(list);
                DG_Contracts.ItemsSource = contractsList;

                if (contractsList.Count > 0)
                {
                    DG_Contracts.SelectedIndex = 0;
                }
                else
                {
                    ClearDashboard();
                }
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری لیست قراردادها: " + ex.Message, true);
            }
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
                ClearDashboard();
            }
        }

        private void ClearDashboard()
        {
            LblContractNumber.Text = "---";
            LblBrandName.Text = "---";
            LblCustomer.Text = "---";
            LblContractDate.Text = "---";
            LblDescription.Text = "---";

            KpiTotalContracted.Text = "0";
            KpiTotalProduced.Text = "0";
            KpiTotalRemaining.Text = "0";
            KpiTotalSold.Text = "0";
            KpiTotalInWarehouse.Text = "0";

            DG_ItemStatus.ItemsSource = null;
            DG_LinkedDocs.ItemsSource = null;
        }

        private void LoadContractDetails(BrandContractModel contract)
        {
            if (contract == null) return;

            LblContractNumber.Text = contract.ContractNumber;
            LblBrandName.Text = contract.BrandName;
            LblCustomer.Text = $"{contract.CustomerName} ({contract.CustomerCode})";
            LblContractDate.Text = contract.ContractDate.ToString();
            LblDescription.Text = contract.Description ?? "---";

            try
            {
                // 1. Calculate and Load KPIs (from line-level INVO_LST.ContractID)
                string kpiQuery = @"
                    SELECT
                        (SELECT COALESCE(SUM(Quantity), 0) FROM dbo.BrandContractItems WHERE ContractID = @ContractID) AS TotalContracted,
                        (SELECT COALESCE(SUM(il.MEGHk), 0) FROM dbo.INVO_LST il WHERE il.ContractID = @ContractID AND il.TAG = 9) AS TotalProduced,
                        (SELECT COALESCE(SUM(il.MEGHk), 0) FROM dbo.INVO_LST il WHERE il.ContractID = @ContractID AND il.TAG = 2) AS TotalSold";

                var kpi = dbms.DoGetDataSQL<ContractKpis>(kpiQuery, new { ContractID = contract.ContractID }).FirstOrDefault();

                if (kpi != null)
                {
                    double remaining = Math.Max(0, kpi.TotalContracted - kpi.TotalProduced);
                    double inWarehouse = Math.Max(0, kpi.TotalProduced - kpi.TotalSold);

                    KpiTotalContracted.Text = kpi.TotalContracted.ToString("N0") + " متر";
                    KpiTotalProduced.Text = kpi.TotalProduced.ToString("N0") + " متر";
                    KpiTotalRemaining.Text = remaining.ToString("N0") + " متر";
                    KpiTotalSold.Text = kpi.TotalSold.ToString("N0") + " متر";
                    KpiTotalInWarehouse.Text = inWarehouse.ToString("N0") + " متر";
                }

                // 2. Load Itemized Progress Report Grid (from line-level INVO_LST.ContractID)
                string progressQuery = @"
                    WITH Contracted AS (
                        SELECT
                            ci.ProductCode,
                            COALESCE(p.NAME, ci.ProductName) AS ProductName,
                            ci.Quantity AS ContractedQty
                        FROM dbo.BrandContractItems ci
                        LEFT JOIN dbo.STUF_DEF p ON ci.ProductCode = p.CODE
                        WHERE ci.ContractID = @ContractID
                    ),
                    Produced AS (
                        SELECT il.CODE AS ProductCode, SUM(il.MEGHk) AS ProducedQty
                        FROM dbo.INVO_LST il
                        WHERE il.ContractID = @ContractID AND il.TAG = 9
                        GROUP BY il.CODE
                    ),
                    Sold AS (
                        SELECT il.CODE AS ProductCode, SUM(il.MEGHk) AS SoldQty
                        FROM dbo.INVO_LST il
                        WHERE il.ContractID = @ContractID AND il.TAG = 2
                        GROUP BY il.CODE
                    ),
                    AllKeys AS (
                        SELECT ProductCode FROM Contracted
                        UNION
                        SELECT ProductCode FROM Produced
                        UNION
                        SELECT ProductCode FROM Sold
                    )
                    SELECT
                        ak.ProductCode,
                        COALESCE(c.ProductName, p.NAME, ak.ProductCode) AS ProductName,
                        COALESCE(c.ContractedQty, 0) AS ContractedQty,
                        COALESCE(pr.ProducedQty, 0) AS ProducedQty,
                        CASE
                            WHEN COALESCE(c.ContractedQty, 0) - COALESCE(pr.ProducedQty, 0) < 0 THEN 0
                            ELSE COALESCE(c.ContractedQty, 0) - COALESCE(pr.ProducedQty, 0)
                        END AS NotProducedQty,
                        COALESCE(s.SoldQty, 0) AS SoldQty,
                        CASE
                            WHEN COALESCE(pr.ProducedQty, 0) - COALESCE(s.SoldQty, 0) < 0 THEN 0
                            ELSE COALESCE(pr.ProducedQty, 0) - COALESCE(s.SoldQty, 0)
                        END AS InWarehouseQty
                    FROM AllKeys ak
                    LEFT JOIN Contracted c ON ak.ProductCode = c.ProductCode
                    LEFT JOIN Produced pr ON ak.ProductCode = pr.ProductCode
                    LEFT JOIN Sold s ON ak.ProductCode = s.ProductCode
                    LEFT JOIN dbo.STUF_DEF p ON ak.ProductCode = p.CODE";

                var progressItems = dbms.DoGetDataSQL<ContractItemProgressModel>(progressQuery, new { ContractID = contract.ContractID }).ToList();
                DG_ItemStatus.ItemsSource = progressItems;

                // 3. Load Linked Documents (distinct list of documents containing lines linked to this contract)
                string docsQuery = @"
                    SELECT DISTINCT
                        hl.NUMBER,
                        hl.TAG,
                        hl.DATE_N,
                        hl.TAH,
                        hl.MAS,
                        CASE hl.TAG
                            WHEN 9 THEN N'رسید تولید (ورود)'
                            WHEN 2 THEN N'حواله فروش (خروج)'
                            ELSE N'سایر سند متصل'
                        END AS DocumentTypeName
                    FROM dbo.HEAD_LST hl
                    INNER JOIN dbo.INVO_LST il ON hl.NUMBER = il.NUMBER AND hl.TAG = il.TAG
                    WHERE il.ContractID = @ContractID
                    ORDER BY hl.DATE_N DESC, hl.NUMBER DESC";

                var linkedDocs = dbms.DoGetDataSQL<LinkedDocumentModel>(docsQuery, new { ContractID = contract.ContractID }).ToList();
                DG_LinkedDocs.ItemsSource = linkedDocs;
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری جزئیات قرارداد: " + ex.Message, true);
            }
        }

        private void Btn_NewContract_Click(object sender, RoutedEventArgs e)
        {
            var editWin = new WIN_BRAND_CONTRACT_EDIT();
            editWin.Owner = this;
            if (editWin.ShowDialog() == true)
            {
                LoadContracts();
                ShowNotification("قرارداد جدید با موفقیت ثبت گردید.", false);
            }
        }

        private void Btn_EditContract_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                var editWin = new WIN_BRAND_CONTRACT_EDIT(selected.ContractID);
                editWin.Owner = this;
                if (editWin.ShowDialog() == true)
                {
                    LoadContracts();
                    // Reselect the edited contract
                    var reselected = contractsList.FirstOrDefault(c => c.ContractID == selected.ContractID);
                    if (reselected != null)
                    {
                        DG_Contracts.SelectedItem = reselected;
                    }
                    ShowNotification("قرارداد با موفقیت ویرایش گردید.", false);
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا یک قرارداد جهت ویرایش انتخاب کنید.", true);
            }
        }

        private void Btn_DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                var result = MessageBox.Show($"آیا از حذف قرارداد '{selected.ContractNumber}' (برند {selected.BrandName}) اطمینان کامل دارید؟ با حذف قرارداد تمامی اقلام و جزئیات متناظر حذف و اسناد متصل آزاد خواهند شد.", "تایید حذف قرارداد", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Clear line-level ContractID links and delete contract
                        dbms.DoExecuteSQL("UPDATE dbo.INVO_LST SET ContractID = NULL WHERE ContractID = @ContractID", new { ContractID = selected.ContractID });
                        dbms.DoExecuteSQL("DELETE FROM dbo.BrandContracts WHERE ContractID = @ContractID", new { ContractID = selected.ContractID });

                        LoadContracts();
                        ShowNotification("قرارداد انتخاب شده با موفقیت حذف گردید.", false);
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("خطا در حذف قرارداد: " + ex.Message, true);
                    }
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا یک قرارداد جهت حذف انتخاب کنید.", true);
            }
        }

        private void Btn_LinkProduction_Click(object sender, RoutedEventArgs e)
        {
            LinkDocumentToSelectedContract(9); // 9 = Production Receipt
        }

        private void Btn_LinkSales_Click(object sender, RoutedEventArgs e)
        {
            LinkDocumentToSelectedContract(2); // 2 = Sales Invoice/Delivery
        }

        private void LinkDocumentToSelectedContract(int tag)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                var selectWin = new WIN_SELECT_DOCUMENT(selected.CustomerCode, tag);
                selectWin.Owner = this;
                if (selectWin.ShowDialog() == true && selectWin.SelectedLineIds.Count > 0)
                {
                    try
                    {
                        foreach (var id in selectWin.SelectedLineIds)
                        {
                            dbms.DoExecuteSQL(@"
                                UPDATE dbo.INVO_LST
                                SET ContractID = @ContractID
                                WHERE id = @Id",
                                new { ContractID = selected.ContractID, Id = id });
                        }

                        LoadContractDetails(selected);
                        ShowNotification("اقلام انتخاب شده با موفقیت به قرارداد متصل شدند.", false);
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("خطا در اتصال اقلام به قرارداد: " + ex.Message, true);
                    }
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا یک قرارداد انتخاب کنید.", true);
            }
        }

        private void Btn_UnlinkDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selectedContract && DG_LinkedDocs.SelectedItem is LinkedDocumentModel selectedDoc)
            {
                var result = MessageBox.Show($"آیا از قطع اتصال تمامی اقلام سند شماره '{selectedDoc.NUMBER}' از این قرارداد اطمینان دارید؟", "قطع اتصال سند", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        dbms.DoExecuteSQL(@"
                            UPDATE dbo.INVO_LST
                            SET ContractID = NULL
                            WHERE NUMBER = @Number AND TAG = @Tag AND ContractID = @ContractID",
                            new { Number = selectedDoc.NUMBER, Tag = selectedDoc.TAG, ContractID = selectedContract.ContractID });

                        LoadContractDetails(selectedContract);
                        ShowNotification("اتصال اقلام سند به قرارداد قطع گردید.", false);
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("خطا در قطع اتصال سند: " + ex.Message, true);
                    }
                }
            }
            else
            {
                ShowNotification("لطفاً ابتدا یک سند از جدول اسناد متصل انتخاب کنید.", true);
            }
        }

        private async void Btn_PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Contracts.SelectedItem is BrandContractModel selected)
            {
                try
                {
                    ShowNotification("در حال آماده‌سازی فایل خلاصه گزارش قرارداد...", false);
                    await UniversalExcelExporter.ExportToExcelAsync(DG_ItemStatus, $"وضعیت_قرارداد_{selected.ContractNumber}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در تولید خروجی اکسل گزارش: " + ex.Message);
                }
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

    #region Models
    public class BrandContractModel
    {
        public int ContractID { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public long ContractDate { get; set; }
        public double TotalQuantity { get; set; }
        public string? Description { get; set; }
    }

    public class ContractKpis
    {
        public double TotalContracted { get; set; }
        public double TotalProduced { get; set; }
        public double TotalSold { get; set; }
    }

    public class ContractItemProgressModel
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double ContractedQty { get; set; }
        public double ProducedQty { get; set; }
        public double NotProducedQty { get; set; }
        public double SoldQty { get; set; }
        public double InWarehouseQty { get; set; }
    }

    public class LinkedDocumentModel
    {
        public double NUMBER { get; set; }
        public double TAG { get; set; }
        public long DATE_N { get; set; }
        public string? TAH { get; set; }
        public double MAS { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
    }
    #endregion
}