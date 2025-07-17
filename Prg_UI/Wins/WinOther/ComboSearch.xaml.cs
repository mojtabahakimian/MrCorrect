using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinOther
{
    public partial class ComboSearch : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }

            if (e.ClickCount == 2)
            {
            }
        }
        //Header Window End;
        #endregion

        string shart = "";
        string shart2 = "";
        delegate void TestDelegate();
        string MyTextSearch = "";
        public bool searchStarted = false;
        public Visual NameWin_US { get; }

        string whocalme = "";
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ComboSearch(string wcalme = "", Visual _YOUR_VL_WIN = default)
        {
            NameWin_US = _YOUR_VL_WIN;
            whocalme = wcalme;
            InitializeComponent();
        }
        public Custom_CUST_HESAB SELECTED_HESAB { get; set; }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                GoSearchforCombo_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void GoSearchforCombo_Click(object sender, RoutedEventArgs e)
        {
            // Initialize search text from the TextBox (handles nulls safely)
            MyTextSearch = TexbSearchCombo?.Text?.Trim() ?? string.Empty;
            searchStarted = true;

            // Check if search text is empty or just whitespace
            if (string.IsNullOrEmpty(MyTextSearch) || string.IsNullOrWhiteSpace(MyTextSearch))
            {
                // Show dialog with no search condition (all customers)
                var resOfSearch = new ResOfSearch(null, whocalme, null, NameWin_US);
                resOfSearch.ShowDialog();

                if (resOfSearch?.SELECTED_HESAB != null)
                {
                    this.SELECTED_HESAB = resOfSearch.SELECTED_HESAB;
                }
                searchStarted = false;
                return; // Exit the function early if no search is needed
            }

            // Variables to store the search conditions
            string shart = string.Empty;
            string shart2 = string.Empty;

            // Split the search text into words by space
            string[] words = MyTextSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Process each word to build the search conditions
            foreach (string word in words)
            {
                string trimmedWord = word.Trim();
                if (!string.IsNullOrEmpty(trimmedWord))
                {
                    // Add the AND condition if this is not the first word
                    if (!string.IsNullOrEmpty(shart))
                    {
                        shart += " AND ";
                        shart2 += " AND ";
                    }

                    // Build search conditions for both NAME and TNAME fields, ensuring that quotes are properly handled
                    shart += BuildSearchCondition("NAME", trimmedWord);
                    shart2 += BuildSearchCondition("TNAME", trimmedWord);
                }
            }

            // Special case for single character input
            if (MyTextSearch.Length == 1)
            {
                // Simplify the search if it's a single character
                shart = BuildSearchCondition("NAME", MyTextSearch);
                shart2 = BuildSearchCondition("TNAME", MyTextSearch);
            }

            // Combine shart and shart2 into one condition
            if (!string.IsNullOrEmpty(shart) && !string.IsNullOrEmpty(shart2))
            {
                shart = $"(({shart}) OR ({shart2}))";
            }

            // Show the result of the search using the constructed condition
            var finalSearchResult = new ResOfSearch(shart, whocalme, null, NameWin_US);
            finalSearchResult.ShowDialog();

            if (finalSearchResult?.SELECTED_HESAB != null)
            {
                this.SELECTED_HESAB = finalSearchResult.SELECTED_HESAB;
            }

            // Reset flags after search
            searchStarted = false;
        }
        private string BuildSearchCondition(string columnName, string word)
        {
            // Handle potential dangerous characters like single quotes
            string sanitizedWord = SanitizeSearchInput(word);

            // Build the search condition for the given column
            return $"({columnName} LIKE N'%{sanitizedWord}%' OR {columnName} LIKE N'%{ReplacePerArab(sanitizedWord, false)}%' OR {columnName} LIKE N'%{ReplacePerArab(sanitizedWord, true)}%')";
        }
        private string SanitizeSearchInput(string input)
        {
            // Prevent breaking the query by handling single quotes and other characters
            if (input.Contains("'"))
            {
                input = input.Replace("'", "''"); // Escape single quotes by doubling them
            }

            // Remove any potential dangerous input such as extra spaces or empty input
            input = input.Trim();
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                input = string.Empty;
            }

            return input;
        }
        private string ReplacePerArab(string input, bool IsArabicy)
        {
            if (IsArabicy)
            {
                return input.Replace("ی", "ي").Replace("ک", "ك").Replace("ة", "ه").Trim();
            }
            else
            {
                return input.Replace("ي", "ی").Replace("ك", "ک").Replace("ه", "ة").Trim();
            }
        }





        private void Window_Closed(object sender, EventArgs e)
        {
            PublicVRB.PlusingSearching = false;
            if (whocalme == "HEAD_LST_PISHFROOSH2")
            {
                if ((string.IsNullOrEmpty(TexbSearchCombo.Text) || string.IsNullOrWhiteSpace(TexbSearchCombo.Text)) && searchStarted == false)
                {
                    PublicVRB.Head_LstPish2.CUST_NO.Text = "";

                    TestDelegate myDel = () => { PublicVRB.Head_LstPish2.CUST_NO.Focus(); };
                    Dispatcher.BeginInvoke(myDel, null);

                    PublicVRB.Head_LstPish2.CUST_NO.IsDropDownOpen = true;
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PublicVRB.PlusingSearching = true;
            TexbSearchCombo.Focus();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //this.Topmost = false;
        }
    }
}
