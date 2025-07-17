using Prg_UI.Wins;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;

namespace Prg_UI.Functions
{
    public static class PublicVRB
    {
        public static string MAHNAME(string mon)
        {
            switch (mon)
            {
                case "1":
                case "01":
                    mon = "فروردین";
                    break;
                case "2":
                case "02":
                    mon = "اردیبهشت";
                    break;
                case "3":
                case "03":
                    mon = "خرداد";
                    break;
                case "4":
                case "04":
                    mon = "تیر";
                    break;
                case "5":
                case "05":
                    mon = "مرداد";
                    break;
                case "6":
                case "06":
                    mon = "شهریور";
                    break;
                case "7":
                case "07":
                    mon = "مهر";
                    break;
                case "8":
                case "08":
                    mon = "آبان";
                    break;
                case "9":
                case "09":
                    mon = "آذر";
                    break;
                case "10":
                    mon = "دی";
                    break;
                case "11":
                    mon = "بهمن";
                    break;
                case "12":
                    mon = "اسفند";
                    break;
                default:
                    break;
            }
            return mon;
        }

        public static void DisableManagerTextSearch(ComboBox CMB, TextCompositionEventArgs e)
        {
            if (e.Text == "+" || e.Text == "++" || e.Text == "+++")
            {
                CMB.IsTextSearchEnabled = false;
            }
            else
            {
                CMB.IsTextSearchEnabled = true;
            }
        }
        /// <summary>
        /// Just put elements by the order you want left to righ →
        /// </summary>
        /// <param name="FRELM"></param>
        public static void TanzimTabindexha(params FrameworkElement[] FRELM)
        {
            bool onTimedone = false;
            for (int i = -2147483647; i <= 2147483647; i++)
            {
                if (!onTimedone)
                {
                    foreach (var ElementITM in FRELM)
                    {
                        if (ElementITM is TextBox) ((TextBox)ElementITM).TabIndex = i;
                        if (ElementITM is DataGrid) ((DataGrid)ElementITM).TabIndex = i;
                        if (ElementITM is ComboBox) ((ComboBox)ElementITM).TabIndex = i;
                        if (ElementITM is CheckBox) ((CheckBox)ElementITM).TabIndex = i;
                        if (ElementITM is Button) ((Button)ElementITM).TabIndex = i;
                        if (ElementITM is RadioButton) ((RadioButton)ElementITM).TabIndex = i;
                        i++;
                    }
                    onTimedone = true;
                }
                else { break; }
            }
        }
        public static bool DoingDeSeprate = false;
        public class TextBoxFormat
        {
            public static void FormatStringTexBox(Window win)
            {
                foreach (var item in FindVisualChildren<TextBox>(win))
                {
                    if (item.Tag != null)
                    {
                        if (item.Tag.ToString() == "123") //// نماد شناسایی تکست باکس که میتونه به دلخواه شما تغییر کنه
                        {
                            item.TextChanged += new TextChangedEventHandler(FormatTextBoxNumber);
                        }
                    }
                }
            }
            public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
            {
                if (depObj != null)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                    {
                        var child = VisualTreeHelper.GetChild(depObj, i);

                        if (child != null && child is T)
                            yield return (T)child;

                        foreach (T childOfChild in FindVisualChildren<T>(child))
                            yield return childOfChild;
                    }
                }
            }
            public static void FormatTextBoxNumber(object sender, RoutedEventArgs e)
            {
                if (PublicVRB.DoingDeSeprate == false)
                {
                    try
                    {
                        //(sender as TextBox).TextChanged -= (sender as TextBox).Name + TextChange;
                        //object.Event -= new EventHandlerType(your_Method);
                        // N_MOIN_TextChanged(object sender, TextChangedEventArgs e)
                        //(sender as TextBox).TextChanged -= TextChange
                        // (sender as TextBox).TextChanged += TextChangedEventHandler(sender,e);

                        //////////////////جدا کردن سه رقم اعداد 
                        var text = (TextBox)sender;

                        ////e.Handled = true;
                        if (text.Text.Length == 0) { return; }
                        double range;
                        if (!Double.TryParse(text.Text, out range))
                        {
                            text.Text = text.Text.Replace(text.Text.Substring(text.Text.Length - 1, 1), "");
                        }
                        if (text.Text != string.Empty)
                        {
                            if (text.Text.Substring(text.Text.Length - 1, 1) == ".") { return; }
                            text.Text = string.Format("{0:#,##0.#}", double.Parse(text.Text.Trim()));
                            if (text.Text.Length != 0)
                            {
                                text.SelectionStart = text.Text.Length;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }

            }
        }
        public static bool PlusingSearching = false;
        public static object SelectedResFromSearch { get; set; }
        public static bool IsSelected_SERCHKAL_item_ = false;
        public static HEAD_LST_PISHFROOSH2 Head_LstPish2
        {
            get
            {
                return Application.Current.Windows
                    .OfType<HEAD_LST_PISHFROOSH2>()
                    .FirstOrDefault();
            }
        }

        public static BUDGET_DEFAULT BUDG_Deflt
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET_DEFAULT>()
                    .FirstOrDefault();
            }
        }
        public static BUDGET0 BUDG_0
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET0>()
                    .FirstOrDefault();
            }
        }
        public static BUDGET1 BUDG_1
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET1>()
                    .FirstOrDefault();
            }
        }
        public static BUDGET2 BUDG_2
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET2>()
                    .FirstOrDefault();
            }
        }
        public static BUDGET3 BUDG_3
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET3>()
                    .FirstOrDefault();
            }
        }
        public static BUDGET5 BUDG_5
        {
            get
            {
                return Application.Current.Windows
                    .OfType<BUDGET5>()
                    .FirstOrDefault();
            }
        }
        /// <summary>
        /// پنجره اصلی برنامه
        /// </summary>
        public static WinBase WINBASE
        {
            get
            {
                return Application.Current.Windows
                    .OfType<WinBase>()
                    .FirstOrDefault();
            }
        }
        public static string HesabCombobText = "";
        //شماره بودجه تعریف شده
        public static Int64 GenetalBGID = 0;
        public static string General_bgdate = "";
        //public static string SalmahNow = "";
        //Week Name as Persian
        public static string SalDt = "";
        public static string MahDt = "";
        public static string RoozDt = "";
        //تاریخ این شماره بودجه ی جی آیدی
        //تاریخی که ثبت شده BGCDATE ↓
        public static Int64 UDayOfBGDATEofBGID = 0;
        public static Int64 UMonthOfBGDATEofBGID = 0;
        public static Int64 UYearOfBGDATEofBGID = 0;
        public static Int64 BGDATE_OF_BGID = UYearOfBGDATEofBGID + UMonthOfBGDATEofBGID + UDayOfBGDATEofBGID;
        //تاریخی که ثبت شده BGCDATE 

        //بودجه بندی ماه و سال یعنی بودجه ای که داشتند  میزدند 
        public static Int64 BGMAH = 0;
        public static Int64 BGSAL = 0;

    }
}
