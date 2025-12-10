using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Wins.WinMenus.HESABDARI.GOZARESHAT.F_MENU_KOL_MOIN_DATE_AI;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for F_MENU_DATE_visit.xaml
    /// </summary>
    public partial class F_MENU_DATE_visit : Window
    {

        #region Header Window Begin
        //Header Window Begin
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
                    //🗖,🗗
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
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion

        public F_MENU_DATE_visit(string _oPEN_ARG)
        {
            OpenArgs = _oPEN_ARG;
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        public string OpenArgs { get; set; } = "VISITDLV";

        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        UniversControl universControl = new UniversControl();

        public Visual I_AM_DATE_VISIT { get; set; }

        public string Condition { get; private set; } = "";

        public class Q1
        {
            public string CUST_NO { get; set; }
            public string NAME { get; set; }
            public string hes { get; set; }
        }

        public class Q2
        {
            public string CUST_NO { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var WINDOW_ID = new WindowInteropHelper(this).Handle;
            I_AM_DATE_VISIT = CL_LMethods.GetTheWindow(WINDOW_ID);

            #region SecuritCheck
            try
            {
                string Formname = "VISITDLV";

                if (OpenArgs == "VISITDLV") //"VISITDLV" //گزارش عملکرد ویزیتور ها
                {
                    // 2. Run Security:
                    bool HasAccess = CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, WINDOW_ID, this.GetType().Name);
                    if (!HasAccess)
                    {
                        this?.Close();
                    }
                    // 3. Final State Check:
                    if (!this.IsLoaded) { this?.Close(); return; }
                }
            }
            catch { try { this?.Close(); } catch { } }
            if (!this.IsLoaded) { this?.Close(); return; }
            #endregion

            FILL_ALL_COMBOBOXES();

            if (CL_HESABDARI.LETSGO("DEPEMAL"))
            {
                if (Condition == "")
                {
                    Condition = " WHERE (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
                else
                {
                    Condition = Condition + " AND  (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
            }

            if (OpenArgs == "VISITONE")
            {
                CUST_NO2.SelectedValue = CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD));
                CUST_NO.SelectedValue = CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD));
                CUST_NO.IsEnabled = false;
                CUST_NO2.IsEnabled = false;
            }

            DT1.Text = Tarikh.FirstDayOfCurrentMonth;
            DT2.Text = Tarikh.LastDayOfCurrentMonth;
            DT1.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Command5.IsFocused)
                {
                    //Enter Key Continue
                }
                else
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        public void FILL_ALL_COMBOBOXES()
        {
            CUST_NO.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT VISITOR_DTL.CUST_NO, CUST_HESAB.NAME, CUST_HESAB.hes FROM VISITOR_DTL INNER JOIN CUST_HESAB ON VISITOR_DTL.CUST_NO = CUST_HESAB.hes GROUP BY VISITOR_DTL.CUST_NO, CUST_HESAB.NAME, CUST_HESAB.hes").ToList();
            CUST_NO.SelectedValuePath = "CUST_NO";
            CUST_NO.DisplayMemberPath = "NAME";

            CUST_NO2.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT CUST_NO FROM VISITOR_DTL GROUP BY CUST_NO").ToList();
            CUST_NO2.SelectedValuePath = "CUST_NO";
            CUST_NO2.DisplayMemberPath = "CUST_NO";
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (IsNull(CUST_NO.SelectedValue))
            {
                return;
            }
            else
            {
                CUST_NO2.SelectedValue = CUST_NO.SelectedValue;
            }
        }

        private void CUST_NO2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (IsNull(CUST_NO2.SelectedValue))
            {
                return;
            }
            else
            {
                CUST_NO.SelectedValue = CUST_NO2.SelectedValue;
            }
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(DT1.Text.ToRawTarikh()))
                    DT1.Text = Convert.ToString(Baseknow.YEA + "0101");

                if (IsNull(CUST_NO.SelectedValue) && OpenArgs != "VISIT_MAS")
                    CUST_NO.SelectedValue = "%";

                if (string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
                    DT2.Text = Convert.ToString(CL_HESABDARI.FARSIDATE());

                if (OpenArgs == "VISIT_MAS" && IsNull(CUST_NO.SelectedValue))
                {
                    Msgwin msgwin = new Msgwin(false, "لطفاً ابتدا مشتری را انتخاب کنید");
                    msgwin.ShowDialog();
                    return;
                }

                switch (OpenArgs)
                {
                    case "VISITDLV":
                        new VISITORS_KOL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), Convert.ToString(CUST_NO.SelectedValue is null ? "%" : CUST_NO.SelectedValue), Condition.ToString()).Show();
                        break;

                    case "VISITKAL":

                        string sql = $"SELECT * FROM VISITOR_DTL_KALA({DT1.Text.ToRawTarikh()},{DT2.Text.ToRawTarikh()},N'{Convert.ToString(CUST_NO.SelectedValue is null ? "%" : CUST_NO.SelectedValue)}') {Condition}";
                        new VISITORS_DTL_KALA(sql).Show();
                        break;

                    case "VISITKALMA":
                        sql = $"SELECT * FROM VISITOR_DTL_KALA_mara({DT1.Text.ToRawTarikh()},{DT2.Text.ToRawTarikh()},N'{Convert.ToString(CUST_NO.SelectedValue is null ? "%" : CUST_NO.SelectedValue)}') {Condition}";

                        new VISITOR_DTL_KALA_mara(sql).Show();
                        break;

                    case "VISIT_MAS":
                        // Build SQL for GPSLOCATION
                        //string sql =
                        //    "SELECT VISITOR, FDATE, EDATE, LATITUDE, LONGITUDE, SPEED, IDD " +
                        //    "FROM dbo.GPSLOCATION " +
                        //    "WHERE VISITOR = @Visitor " +
                        //    "  AND FDATE BETWEEN @DT1 AND @DT2 " +
                        //    "ORDER BY IDD";

                        //// Ensure C:\CORRECT\ and cnr.udl exist
                        //const string folder = @"C:\CORRECT\";
                        //if (!Directory.Exists(folder))
                        //{
                        //    Directory.CreateDirectory(folder);
                        //    File.WriteAllText(
                        //        Path.Combine(folder, "cnr.udl"),
                        //        CL_CCNNMANAGER.CONNECTION_STR
                        //    );
                        //}

                        //// Launch external exe with SQL as argument
                        //Process.Start(new ProcessStartInfo
                        //{
                        //    FileName = Path.Combine(folder, "GPSH.EXE"),
                        //    Arguments = $"\"{sql.Replace("@Visitor", CUST_NO.Text).Replace("@DT1", DT1.Text).Replace("@DT2", DT2.Text)}\"",
                        //    UseShellExecute = true
                        //});

                        break;

                    case "VISITONE":
                        sql = $"WHERE CUST_NO = N'{CUST_NO2.SelectedValue}' AND DATE_N BETWEEN {DT1.Text.ToRawTarikh()} AND {DT2.Text.ToRawTarikh()}";
                        new flist_porsant_factors(sql).Show();
                        break;

                    default:
                        // no-op
                        break;
                }
            }
            catch (Exception ex)
            {
                // swallow errors as in VBA's On Error Resume Next
                Debug.WriteLine("Error in Command5_Click: " + ex);
            }
        }

        private void DT1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT1.SelectAll();
        }
        private void DT2_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT2.SelectAll();
        }
    }
}

