using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_BRAND_CONTRACT_EDIT : Window
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private int? contractId;
        private ObservableCollection<BrandContractItemModel> patternsList = new ObservableCollection<BrandContractItemModel>();

        public WIN_BRAND_CONTRACT_EDIT(int? id = null)
        {
            InitializeComponent();
            contractId = id;
        }

        #region Standard Header Bar Handling
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (contractId.HasValue)
            {
                LblTitle.Content = "ویرایش قرارداد و تعهدات برند";
                LoadContractData(contractId.Value);
            }
            else
            {
                // Generate and set the current Persian date: yyyyMMdd
                var pc = new System.Globalization.PersianCalendar();
                var now = DateTime.Now;
                string persianDateStr = $"{pc.GetYear(now)}{pc.GetMonth(now):D2}{pc.GetDayOfMonth(now):D2}";
                TxtContractDate.Text = persianDateStr;

                // Add initial empty pattern row to start with
                patternsList.Add(new BrandContractItemModel());
                DG_Patterns.ItemsSource = patternsList;
            }
        }

        private void LoadContractData(int id)
        {
            try
            {
                string headerQuery = "SELECT * FROM dbo.BrandContracts WHERE ContractID = @Id";
                var header = dbms.DoGetDataSQL<BrandContractModel>(headerQuery, new { Id = id }).FirstOrDefault();

                if (header != null)
                {
                    TxtContractNumber.Text = header.ContractNumber;
                    TxtCustomerCode.Text = header.CustomerCode;
                    TxtCustomerName.Text = header.CustomerName;
                    TxtBrandName.Text = header.BrandName;
                    TxtContractDate.Text = header.ContractDate.ToString();
                    TxtTotalQuantity.Text = header.TotalQuantity.ToString("G");
                    TxtDescription.Text = header.Description;

                    string itemsQuery = "SELECT * FROM dbo.BrandContractItems WHERE ContractID = @Id ORDER BY ItemID";
                    var items = dbms.DoGetDataSQL<BrandContractItemModel>(itemsQuery, new { Id = id }).ToList();
                    patternsList = new ObservableCollection<BrandContractItemModel>(items);
                    DG_Patterns.ItemsSource = patternsList;
                }
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری اطلاعات قرارداد: " + ex.Message, true);
            }
        }

        private void TxtCustomerCode_LostFocus(object sender, RoutedEventArgs e)
        {
            ResolveCustomerName();
        }

        private bool ResolveCustomerName()
        {
            string code = TxtCustomerCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
                return false;

            try
            {
                // Query TDETA_HES supporting both absolute match and the composite format N_KOL-NUMBER-TNUMBER
                string query = @"
                    SELECT TOP 1
                        NAME,
                        REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR), ' ', '') AS FullCode
                    FROM dbo.TDETA_HES
                    WHERE CAST(TNUMBER AS NVARCHAR(50)) = @Code
                       OR REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR), ' ', '') = @Code";

                var resolved = dbms.DoGetDataSQL<dynamic>(query, new { Code = code }).FirstOrDefault();
                if (resolved != null)
                {
                    TxtCustomerName.Text = resolved.NAME;
                    TxtCustomerCode.Text = resolved.FullCode; // Fill the textbox with the full standard code
                    return true;
                }
                else
                {
                    TxtCustomerName.Text = "مشتری یافت نشد!";
                    return false;
                }
            }
            catch
            {
                TxtCustomerName.Text = "خطا در استعلام مشتری";
                return false;
            }
        }

        private void DG_Patterns_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is BrandContractItemModel rowItem)
            {
                // Real-time product name resolving upon code modifications
                if (e.Column.Header.ToString() == "کد کالا / طرح")
                {
                    var textbox = e.EditingElement as TextBox;
                    string code = textbox?.Text?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(code))
                    {
                        try
                        {
                            string query = "SELECT TOP 1 NAME FROM dbo.STUF_DEF WHERE CODE = @Code";
                            string name = dbms.DoGetDataSQL<string>(query, new { Code = code }).FirstOrDefault() ?? "کالا یافت نشد!";
                            rowItem.ProductName = name;
                        }
                        catch
                        {
                            rowItem.ProductName = "خطا در استعلام کالا";
                        }
                        // Refresh the DataGrid so the name populates immediately
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                            DG_Patterns.Items.Refresh();
                            RecalculateTotalQuantity();
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
                else if (e.Column.Header.ToString() == "متراژ تعهد شده (متر)")
                {
                    var textbox = e.EditingElement as TextBox;
                    if (double.TryParse(textbox?.Text, out double qty))
                    {
                        rowItem.Quantity = qty;
                    }
                    // Recalculate contract total automatically on row quantity updates
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                        RecalculateTotalQuantity();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        private void RecalculateTotalQuantity()
        {
            double sum = patternsList.Sum(p => p.Quantity);
            TxtTotalQuantity.Text = sum.ToString("G");
        }

        private void Btn_AddItem_Click(object sender, RoutedEventArgs e)
        {
            patternsList.Add(new BrandContractItemModel());
        }

        private void Btn_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Patterns.SelectedItem is BrandContractItemModel selected)
            {
                patternsList.Remove(selected);
                RecalculateTotalQuantity();
            }
            else
            {
                ShowNotification("لطفاً ابتدا ردیف الگوی مورد نظر را انتخاب کنید.", true);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter key emulates Tab key navigation
            if (e.Key == Key.Enter)
            {
                var focusedElement = Keyboard.FocusedElement as UIElement;
                if (focusedElement != null)
                {
                    // Check if focus is inside the Patterns DataGrid and on the last column (Quantity) of the last row
                    if (DG_Patterns.IsKeyboardFocusWithin)
                    {
                        var cell = focusedElement as DataGridCell ?? CL_LMethods.FindVisualParent<DataGridCell>(focusedElement);
                        if (cell != null && cell.Column.Header.ToString() == "متراژ تعهد شده (متر)")
                        {
                            var lastRowItem = patternsList.LastOrDefault();
                            if (DG_Patterns.SelectedItem == lastRowItem)
                            {
                                // Append a new row, focus its ProductCode cell, and trigger edit mode automatically
                                e.Handled = true;
                                var newRow = new BrandContractItemModel();
                                patternsList.Add(newRow);

                                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                                    DG_Patterns.SelectedItem = newRow;
                                    CL_LMethods.FocusCellReadyToEdit(DG_Patterns, "ProductCode", DG_Patterns.Items.Count - 1, true);
                                }), System.Windows.Threading.DispatcherPriority.Background);
                                return;
                            }
                        }
                    }

                    // Standard Enter-to-Tab key movement
                    e.Handled = true;
                    focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            string contractNum = TxtContractNumber.Text.Trim();
            string custCode = TxtCustomerCode.Text.Trim();
            string custName = TxtCustomerName.Text.Trim();
            string brandName = TxtBrandName.Text.Trim();
            string dateStr = TxtContractDate.Text.Trim();
            string totalQtyStr = TxtTotalQuantity.Text.Trim();
            string desc = TxtDescription.Text.Trim();

            if (string.IsNullOrEmpty(contractNum))
            {
                ShowNotification("وارد کردن شماره قرارداد الزامی است.", true);
                return;
            }

            if (string.IsNullOrEmpty(custCode) || custName.Contains("یافت نشد"))
            {
                ShowNotification("کد مشتری نامعتبر است.", true);
                return;
            }

            if (!long.TryParse(dateStr, out long dateVal))
            {
                ShowNotification("تاریخ قرارداد نامعتبر است.", true);
                return;
            }

            if (!double.TryParse(totalQtyStr, out double totalQty))
            {
                totalQty = 0;
            }

            // Clean list from blank rows before validation
            var validItems = patternsList.Where(p => !string.IsNullOrEmpty(p.ProductCode) && p.Quantity > 0).ToList();
            if (validItems.Count == 0)
            {
                ShowNotification("حداقل یک الگوی کالا با مقدار بزرگتر از صفر الزامی است.", true);
                return;
            }

            try
            {
                // Unique check for Contract Number
                string checkQuery = "SELECT COUNT(*) FROM dbo.BrandContracts WHERE ContractNumber = @Num" + (contractId.HasValue ? " AND ContractID != @Id" : "");
                int count = dbms.DoGetDataSQL<int>(checkQuery, new { Num = contractNum, Id = contractId ?? 0 }).FirstOrDefault();
                if (count > 0)
                {
                    ShowNotification("قراردادی با این شماره از قبل در سیستم ثبت شده است.", true);
                    return;
                }

                // Build the single atomic parameterized transaction batch
                var sbBatch = new System.Text.StringBuilder();
                sbBatch.AppendLine("BEGIN TRANSACTION;");
                sbBatch.AppendLine("BEGIN TRY");

                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@ContractNumber", contractNum);
                parameters.Add("@CustomerCode", custCode);
                parameters.Add("@CustomerName", custName);
                parameters.Add("@BrandName", brandName);
                parameters.Add("@ContractDate", dateVal);
                parameters.Add("@TotalQuantity", totalQty);
                parameters.Add("@Description", desc);

                if (contractId.HasValue)
                {
                    parameters.Add("@ContractID", contractId.Value);
                    sbBatch.AppendLine(@"
                        UPDATE dbo.BrandContracts
                        SET ContractNumber = @ContractNumber,
                            CustomerCode = @CustomerCode,
                            CustomerName = @CustomerName,
                            BrandName = @BrandName,
                            ContractDate = @ContractDate,
                            TotalQuantity = @TotalQuantity,
                            Description = @Description
                        WHERE ContractID = @ContractID;

                        DELETE FROM dbo.BrandContractItems WHERE ContractID = @ContractID;");
                }
                else
                {
                    sbBatch.AppendLine(@"
                        INSERT INTO dbo.BrandContracts (ContractNumber, CustomerCode, CustomerName, BrandName, ContractDate, TotalQuantity, Description)
                        VALUES (@ContractNumber, @CustomerCode, @CustomerName, @BrandName, @ContractDate, @TotalQuantity, @Description);

                        DECLARE @ContractID INT = SCOPE_IDENTITY();");
                }

                // Append items inserts to execute atomically inside SQL Server transaction
                for (int i = 0; i < validItems.Count; i++)
                {
                    var item = validItems[i];
                    string pCodeKey = $"@PCode_{i}";
                    string pNameKey = $"@PName_{i}";
                    string pQtyKey = $"@PQty_{i}";

                    parameters.Add(pCodeKey, item.ProductCode);
                    parameters.Add(pNameKey, item.ProductName);
                    parameters.Add(pQtyKey, item.Quantity);

                    sbBatch.AppendLine($@"
                        INSERT INTO dbo.BrandContractItems (ContractID, ProductCode, ProductName, Quantity)
                        VALUES (@ContractID, {pCodeKey}, {pNameKey}, {pQtyKey});");
                }

                sbBatch.AppendLine("COMMIT TRANSACTION;");
                sbBatch.AppendLine("END TRY");
                sbBatch.AppendLine("BEGIN CATCH");
                sbBatch.AppendLine("IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
                sbBatch.AppendLine("THROW;");
                sbBatch.AppendLine("END CATCH;");

                dbms.DoExecuteSQL(sbBatch.ToString(), parameters);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در ذخیره سازی اطلاعات قرارداد: " + ex.Message, true);
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
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
}