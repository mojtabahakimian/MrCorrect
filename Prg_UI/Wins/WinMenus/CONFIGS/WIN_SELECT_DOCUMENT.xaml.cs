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
    public partial class WIN_SELECT_DOCUMENT : Window
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private string customerCode = string.Empty;
        private int documentTag = 2;
        private ObservableCollection<UnlinkedDocModel> docsList = new ObservableCollection<UnlinkedDocModel>();
        private ObservableCollection<UnlinkedDocLineModel> linesList = new ObservableCollection<UnlinkedDocLineModel>();

        public List<long> SelectedLineIds { get; private set; } = new List<long>();

        public WIN_SELECT_DOCUMENT(string custCode, int tag)
        {
            InitializeComponent();
            customerCode = custCode;
            documentTag = tag;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (documentTag == 9)
            {
                LblTitle.Content = "اتصال اقلام رسید تولید (TAG = 9) به قرارداد";
            }
            else
            {
                LblTitle.Content = "اتصال اقلام حواله فروش (TAG = 2) به قرارداد";
            }

            LoadDocuments();
        }

        private void LoadDocuments(string searchFilter = "")
        {
            try
            {
                // Retrieve all documents of the specified TAG that contain at least one unlinked line for the EXACT resolved composite customer code
                string query = @"
                    SELECT DISTINCT hl.NUMBER, hl.DATE_N, hl.TAH, hl.MAS
                    FROM dbo.HEAD_LST hl
                    INNER JOIN dbo.INVO_LST il ON hl.NUMBER = il.NUMBER AND hl.TAG = il.TAG
                    WHERE hl.TAG = @Tag
                      AND hl.CUST_NO = @CustCode
                      AND (il.ContractID IS NULL)
                    ORDER BY hl.DATE_N DESC, hl.NUMBER DESC";

                var list = dbms.DoGetDataSQL<UnlinkedDocModel>(query, new { Tag = documentTag, CustCode = customerCode }).ToList();

                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    list = list.Where(d =>
                        d.NUMBER.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        d.DATE_N.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        (d.TAH != null && d.TAH.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                docsList = new ObservableCollection<UnlinkedDocModel>(list);
                DG_Docs.ItemsSource = docsList;
                linesList.Clear();
                DG_Lines.ItemsSource = null;
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری لیست اسناد: " + ex.Message, true);
            }
        }

        private void TxtSearchDoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDocuments(TxtSearchDoc.Text);
        }

        private void DG_Docs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Docs.SelectedItem is UnlinkedDocModel selected)
            {
                LoadDocumentLines(selected.NUMBER);
            }
            else
            {
                linesList.Clear();
                DG_Lines.ItemsSource = null;
            }
        }

        private void LoadDocumentLines(double number)
        {
            try
            {
                // Load unlinked lines of the selected document
                string query = @"
                    SELECT il.id, il.CODE, p.NAME AS ProductName, il.MEGHk, il.MABL, il.MABL_K
                    FROM dbo.INVO_LST il
                    LEFT JOIN dbo.STUF_DEF p ON il.CODE = p.CODE
                    WHERE il.NUMBER = @Number AND il.TAG = @Tag AND (il.ContractID IS NULL)
                    ORDER BY il.id";

                var list = dbms.DoGetDataSQL<UnlinkedDocLineModel>(query, new { Number = number, Tag = documentTag }).ToList();
                linesList = new ObservableCollection<UnlinkedDocLineModel>(list);
                DG_Lines.ItemsSource = linesList;
            }
            catch (Exception ex)
            {
                ShowNotification("خطا در بارگذاری اقلام سند: " + ex.Message, true);
            }
        }

        private void Btn_Select_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSelection();
        }

        private void DG_Docs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Handled via DG_Docs_SelectionChanged
        }

        private void ExecuteSelection()
        {
            var selectedItems = DG_Lines.SelectedItems.OfType<UnlinkedDocLineModel>().ToList();

            if (selectedItems.Count > 0)
            {
                SelectedLineIds = selectedItems.Select(line => line.id).ToList();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ShowNotification("لطفاً ابتدا حداقل یک قلم کالا از جدول پایین جهت اتصال انتخاب کنید.", true);
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

    #region Inner Models
    public class UnlinkedDocModel
    {
        public double NUMBER { get; set; }
        public long DATE_N { get; set; }
        public string? TAH { get; set; }
        public double MAS { get; set; }
    }

    public class UnlinkedDocLineModel
    {
        public long id { get; set; }
        public string CODE { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public double MEGHk { get; set; }
        public double MABL { get; set; }
        public double MABL_K { get; set; }
    }
    #endregion
}