using Functions;
using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
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
        private int? contractId = null;
        private ObservableCollection<BrandContractItemModel> patternsList = new ObservableCollection<BrandContractItemModel>();

        public WIN_BRAND_CONTRACT_EDIT(int? id = null)
        {
            InitializeComponent();
            contractId = id;
        }

        #region Standard Header Handling
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (Btn_Save.IsFocused)
                    {
                        Btn_Save_Click(null, null);
                        return;
                    }

                    if (DG_Patterns.IsKeyboardFocusWithin)
                    {
                        e.Handled = true;

                        var currentCell = DG_Patterns.CurrentCell;
                        if (currentCell.Column != null)
                        {
                            int currentColumnIndex = DG_Patterns.Columns.IndexOf(currentCell.Column);
                            bool isLastColumn = currentColumnIndex == DG_Patterns.Columns.Count - 1;
                            int selectedIndex = DG_Patterns.SelectedIndex;
                            bool isLastRow = selectedIndex == DG_Patterns.Items.Count - 1;

                            if (isLastColumn)
                            {
                                if (isLastRow)
                                {
                                    // Automatically append a new empty row when pressing Enter on the last cell of the last row!
                                    patternsList.Add(new BrandContractItemModel
                                    {
                                        ProductCode = string.Empty,
                                        ProductName = "در انتظار وارد کردن کد...",
                                        Quantity = 0
                                    });

                                    Dispatcher.BeginInvoke(new Action(() => {
                                        DG_Patterns.SelectedIndex = DG_Patterns.Items.Count - 1;
                                        DG_Patterns.CurrentCell = new DataGridCellInfo(DG_Patterns.SelectedItem, DG_Patterns.Columns[0]);
                                        DG_Patterns.BeginEdit();
                                    }), System.Windows.Threading.DispatcherPriority.Background);

                                    return;
                                }
                                else
                                {
                                    // Move to the first column of the next row
                                    Dispatcher.BeginInvoke(new Action(() => {
                                        DG_Patterns.SelectedIndex = selectedIndex + 1;
                                        DG_Patterns.CurrentCell = new DataGridCellInfo(DG_Patterns.SelectedItem, DG_Patterns.Columns[0]);
                                        DG_Patterns.BeginEdit();
                                    }), System.Windows.Threading.DispatcherPriority.Background);

                                    return;
                                }
                            }
                        }

                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                    else
                    {
                        e.Handled = true;
                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (contractId.HasValue)
            {
                LoadContractForEdit(contractId.Value);
            }
            else
            {
                // New contract setup
                patternsList = new ObservableCollection<BrandContractItemModel>();
                DG_Patterns.ItemsSource = patternsList;

                // Set default Persian date
                try
                {
                    // Query current Persian date if function is available or use numeric format of today
                    long today = dbms.DoGetDataSQL<long>("SELECT CAST(CONVERT(NVARCHAR(8), GETDATE(), 112) AS BIGINT)").FirstOrDefault();
                    // Let's format it in standard Persian date format roughly if we don't have conversion function, or just default to 14030101
                    TxtContractDate.Text = "14030101";
                }
                catch
                {
                    TxtContractDate.Text = "14030101";
                }
            }
            UpdateTotalQuantity();
        }

        private void LoadContractForEdit(int id)
        {
            try
            {
                var contract = dbms.DoGetDataSQL<BrandContractModel>("SELECT * FROM dbo.BrandContracts WHERE ContractID = @Id", new { Id = id }).FirstOrDefault();
                if (contract != null)
                {
                    TxtContractNumber.Text = contract.ContractNumber;
                    TxtBrandName.Text = contract.BrandName;
                    TxtContractDate.Text = contract.ContractDate.ToString();
                    TxtCustomerCode.Text = contract.CustomerCode;
                    TxtCustomerName.Text = contract.CustomerName;
                    TxtDescription.Text = contract.Description;
                    TxtTotalQuantity.Text = contract.TotalQuantity.ToString("N0");

                    // Load items
                    var items = dbms.DoGetDataSQL<BrandContractItemModel>("SELECT * FROM dbo.BrandContractItems WHERE ContractID = @Id ORDER BY ItemID", new { Id = id }).ToList();
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
            {
                TxtCustomerName.Text = string.Empty;
                return false;
            }

            try
            {
                // Supports both single TNUMBER (e.g. 25) and composite account code (e.g. 115-1-25)
                string query = @"
                    SELECT TOP 1 NAME
                    FROM dbo.TDETA_HES
                    WHERE CAST(TNUMBER AS NVARCHAR(50)) = @Code
                       OR REPLACE(CAST(N_KOL AS NVARCHAR) + '-' + CAST(NUMBER AS NVARCHAR) + '-' + CAST(TNUMBER AS NVARCHAR), ' ', '') = @Code";

                var name = dbms.DoGetDataSQL<string>(query, new { Code = code }).FirstOrDefault();
                if (!string.IsNullOrEmpty(name))
                {
                    TxtCustomerName.Text = name;
                    return true;
                }
                else
                {
                    TxtCustomerName.Text = "مشتری یافت نشد!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                TxtCustomerName.Text = "خطا در استعلام مشتری";
                return false;
            }
        }

        private void Btn_AddItem_Click(object sender, RoutedEventArgs e)
        {
            patternsList.Add(new BrandContractItemModel
            {
                ProductCode = string.Empty,
                ProductName = "در انتظار وارد کردن کد...",
                Quantity = 0
            });
        }

        private void Btn_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Patterns.SelectedItem is BrandContractItemModel selected)
            {
                patternsList.Remove(selected);
                UpdateTotalQuantity();
            }
            else
            {
                ShowNotification("لطفاً یک ردیف از جدول طرح‌ها جهت حذف انتخاب کنید.", true);
            }
        }

        private void DG_Patterns_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Triggered when editing of a cell is complete
            if (e.Column.Header.ToString().Contains("کد کالا"))
            {
                var textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    string productCode = textBox.Text.Trim();
                    var editedItem = e.Row.Item as BrandContractItemModel;
                    if (editedItem != null)
                    {
                        ResolveProductName(editedItem, productCode);
                    }
                }
            }

            // Wait slightly and update total quantity
            Dispatcher.BeginInvoke(new Action(() => {
                UpdateTotalQuantity();
                DG_Patterns.Items.Refresh(); // Forces immediate redraw of the resolved product name on the grid
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ResolveProductName(BrandContractItemModel item, string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return;

            try
            {
                var name = dbms.DoGetDataSQL<string>("SELECT NAME FROM dbo.STUF_DEF WHERE CODE = @Code", new { Code = productCode }).FirstOrDefault();
                if (!string.IsNullOrEmpty(name))
                {
                    item.ProductCode = productCode;
                    item.ProductName = name;
                }
                else
                {
                    item.ProductName = "کالا یافت نشد!";
                }
            }
            catch
            {
                item.ProductName = "خطا در استعلام کالا";
            }
        }

        private void UpdateTotalQuantity()
        {
            double total = patternsList.Sum(p => p.Quantity);
            TxtTotalQuantity.Text = total.ToString("N0");
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation
            string contractNum = TxtContractNumber.Text.Trim();
            string brandName = TxtBrandName.Text.Trim();
            string dateStr = TxtContractDate.Text.Trim();
            string custCode = TxtCustomerCode.Text.Trim();
            string custName = TxtCustomerName.Text.Trim();
            string desc = TxtDescription.Text.Trim();

            if (string.IsNullOrEmpty(contractNum))
            {
                ShowNotification("لطفاً شماره قرارداد را وارد کنید.", true);
                return;
            }

            if (string.IsNullOrEmpty(brandName))
            {
                ShowNotification("لطفاً نام برند را وارد کنید.", true);
                return;
            }

            if (!long.TryParse(dateStr, out long dateVal) || dateStr.Length != 8)
            {
                ShowNotification("لطفاً تاریخ قرارداد را با فرمت صحیح وارد کنید (مانند 14030101).", true);
                return;
            }

            if (!ResolveCustomerName())
            {
                ShowNotification("لطفاً کد حسابداری مشتری معتبر را وارد کنید.", true);
                return;
            }

            if (patternsList.Count == 0)
            {
                ShowNotification("لطفاً حداقل یک طرح/کالا برای این قرارداد تعریف کنید.", true);
                return;
            }

            if (patternsList.Any(p => string.IsNullOrEmpty(p.ProductCode) || p.ProductName == "کالا یافت نشد!"))
            {
                ShowNotification("لطفاً تمامی کدهای کالاها را اصلاح و تایید کنید.", true);
                return;
            }

            if (patternsList.Any(p => p.Quantity <= 0))
            {
                ShowNotification("متراژ تعهد شده برای تمامی طرح‌ها باید بزرگتر از صفر باشد.", true);
                return;
            }

            double totalQty = patternsList.Sum(p => p.Quantity);

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

                // Save or Update Contract Header
                if (contractId.HasValue)
                {
                    string updateQuery = @"
                        UPDATE dbo.BrandContracts
                        SET ContractNumber = @ContractNumber,
                            CustomerCode = @CustomerCode,
                            CustomerName = @CustomerName,
                            BrandName = @BrandName,
                            ContractDate = @ContractDate,
                            TotalQuantity = @TotalQuantity,
                            Description = @Description
                        WHERE ContractID = @ContractID";

                    dbms.DoExecuteSQL(updateQuery, new {
                        ContractNumber = contractNum,
                        CustomerCode = custCode,
                        CustomerName = custName,
                        BrandName = brandName,
                        ContractDate = dateVal,
                        TotalQuantity = totalQty,
                        Description = desc,
                        ContractID = contractId.Value
                    });

                    // Overwrite items
                    dbms.DoExecuteSQL("DELETE FROM dbo.BrandContractItems WHERE ContractID = @ContractID", new { ContractID = contractId.Value });

                    foreach (var item in patternsList)
                    {
                        dbms.DoExecuteSQL(@"
                            INSERT INTO dbo.BrandContractItems (ContractID, ProductCode, ProductName, Quantity)
                            VALUES (@ContractID, @ProductCode, @ProductName, @Quantity)",
                            new { ContractID = contractId.Value, ProductCode = item.ProductCode, ProductName = item.ProductName, Quantity = item.Quantity });
                    }
                }
                else
                {
                    // Create Contract
                    string insertQuery = @"
                        INSERT INTO dbo.BrandContracts (ContractNumber, CustomerCode, CustomerName, BrandName, ContractDate, TotalQuantity, Description)
                        VALUES (@ContractNumber, @CustomerCode, @CustomerName, @BrandName, @ContractDate, @TotalQuantity, @Description)";

                    dbms.DoExecuteSQL(insertQuery, new {
                        ContractNumber = contractNum,
                        CustomerCode = custCode,
                        CustomerName = custName,
                        BrandName = brandName,
                        ContractDate = dateVal,
                        TotalQuantity = totalQty,
                        Description = desc
                    });

                    // Get Identity of new Contract
                    int newId = dbms.DoGetDataSQL<int>("SELECT TOP 1 ContractID FROM dbo.BrandContracts WHERE ContractNumber = @Num", new { Num = contractNum }).FirstOrDefault();

                    foreach (var item in patternsList)
                    {
                        dbms.DoExecuteSQL(@"
                            INSERT INTO dbo.BrandContractItems (ContractID, ProductCode, ProductName, Quantity)
                            VALUES (@ContractID, @ProductCode, @ProductName, @Quantity)",
                            new { ContractID = newId, ProductCode = item.ProductCode, ProductName = item.ProductName, Quantity = item.Quantity });
                    }
                }

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

    #region Inner Model
    public class BrandContractItemModel
    {
        public int ItemID { get; set; }
        public int ContractID { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public double Quantity { get; set; }
    }
    #endregion
}