using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using MaterialDesignThemes.Wpf;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_DATE_KOL_MOIN_TAFKIK.xaml
    /// </summary>
    public partial class F_MENU_DATE_KOL_MOIN_TAFKIK : Window
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

        public F_MENU_DATE_KOL_MOIN_TAFKIK()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

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

        public Visual I_AM_KOL_MOIN_TAFKIK { get; set; }

        public class Q1
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
        }

        public string _where_q { get; set; }
        public string _where { get; set; }
        public string _sql_query { get; set; }

        public bool First_Open = true;

        public class Q2
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
            public int? Expr1 { get; set; }
        }

        public class Q3
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_KOL_MOIN_TAFKIK = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            HKOL.PreviewLostKeyboardFocus -= HKOL_PreviewLostKeyboardFocus;

            HKOL.Focus();

            HKOL.PreviewLostKeyboardFocus += HKOL_PreviewLostKeyboardFocus;
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

        public void Fill_ComboBoxes()
        {
            HKOL.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME, TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HKOL.DisplayMemberPath = "NAME";
            HKOL.SelectedValuePath = "NUMBER";
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql, COND;
            string PATH;
            int i;
            if (IsNull(this.DT1.Text.ToRawTarikh()))
            {
                DT1.Text = Convert.ToString(Baseknow.YEA + "0101");
            }

            if (IsNull(this.HKOL.SelectedValue) || IsNull(this.HMOIN1.SelectedValue) || IsNull(this.TAFZ1.SelectedValue) || IsNull(this.DT1.Text.ToRawTarikh()) || IsNull(this.DT2.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (RD1.IsChecked == true)
            {
                COND = "(BEDBES <> 0 )";
                _where_q = COND;
            }
            else if (RD2.IsChecked == true)
            {
                COND = "(BEDBES = 0 )";
                _where_q = COND;
            }
            else
            {
                COND = "";
                _where_q = COND;
            }
            if (RD1.IsChecked == true || RD3.IsChecked == true)
            {
                if (RDS1.IsChecked == true)
                {
                    if (Strings.Len(COND) > 0)
                    {
                        COND = COND + " AND (BEDBES > 0)";
                        _where_q = COND;
                    }
                    else
                    {
                        COND = "(BEDBES > 0)";
                        _where_q = COND;
                    }
                }
                else if (RDS2.IsChecked == true)
                {
                    if (Strings.Len(COND) > 0)
                    {
                        COND = COND + " AND  (BEDBES < 0)";
                        _where_q = COND;
                    }
                    else
                    {
                        COND = "(BEDBES < 0)";
                        _where_q = COND;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_where_q))
            {
                _where = "WHERE";
            }
            else
            {
                _where = "";
            }

            _sql_query = $"SELECT hname, NAMET, SumOfBED, SumOfBES, BEDBES, HES_T, HES_K FROM dbo.Q_TAFKIK_MOIN('{HKOL.SelectedValue}', '{HMOIN1.SelectedValue}', '{TAFZ1.SelectedValue}', '{TAFZ2.SelectedValue}', '{DT1.Text.ToRawTarikh()}', '{DT2.Text.ToRawTarikh()}'){_where}{_where_q}";
            OpenReport();

            this.Close();
        }

        private void HKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر


            if (First_Open == true)
            {
                First_Open = false;
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);

            if (textBox != null)
            {
                string inputCode = textBox.Text;


                if (comboBox.SelectedValue == inputCode)
                {
                    return;
                }
                else
                {
                    if (int.TryParse(inputCode, out _))
                    {
                        // Cast the ItemsSource back to the correct type, and search for matching bank code
                        var kollist = comboBox.ItemsSource as List<Q1>;

                        if (kollist == null) return; // Ensure that the list is properly cast

                        var matchingkol = kollist.FirstOrDefault(b => b.NUMBER == Convert.ToInt32(inputCode));

                        if (matchingkol != null)
                        {
                            HKOL.SelectedValue = matchingkol.NUMBER;
                            comboBox.SelectedValue = matchingkol.NUMBER;
                            //return;
                        }
                        else
                        {
                            comboBox.SelectedValue = null;
                        }
                    }
                }
            }


            if (HKOL.SelectedValue is null || HKOL.SelectedValue.ToString() == "")
            {
                universControl.PopNotifyShow("حساب کل نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                return;
            }



            HMOIN1.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT  NUMBER, NAME, NUMBER AS Expr1 FROM DETA_HES WHERE (N_KOL = " + this.HKOL.SelectedValue + ") ORDER BY NAME").ToList();
            HMOIN1.DisplayMemberPath = "NAME";
            HMOIN1.SelectedValuePath = "NUMBER";
        }

        private void HMOIN1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            if (HMOIN1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ComboBox comboBox = sender as ComboBox;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                string inputCode = textBox.Text;

                if (comboBox.SelectedValue == inputCode)
                {
                    return;
                }
                else
                {
                    if (int.TryParse(inputCode, out _))
                    {
                        // Cast the ItemsSource back to the correct type, and search for matching bank code
                        var moinlist = comboBox.ItemsSource as List<Q2>;

                        if (moinlist == null) return; // Ensure that the list is properly cast

                        var matchingmoin = moinlist.FirstOrDefault(b => b.NUMBER == Convert.ToInt32(inputCode));

                        if (matchingmoin != null)
                        {
                            HMOIN1.SelectedValue = matchingmoin.NUMBER;
                            comboBox.SelectedValue = matchingmoin.NUMBER;
                            //return;
                        }
                        else
                        {
                            comboBox.SelectedValue = null;
                        }
                    }
                }
            }


            if ((HKOL.SelectedValue is null || HKOL.SelectedValue.ToString() == "") || (HMOIN1.SelectedValue is null || HMOIN1.SelectedValue.ToString() == ""))
            {
                universControl.PopNotifyShow("حساب کل و معین نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (Baseknow.PERSON == 2)
            {
                TAFZ1.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT     TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TNUMBER FROM TDETA_HES WHERE     (((TDETA_HES.NUMBER) = " + this.HMOIN1 + " ) AND ((TDETA_HES.N_KOL)= " + this.HKOL + "AND (CODE_E = N'0') OR (CODE_E IS NULL))) ORDER BY TDETA_HES.TNUMBER, TDETA_HES.NAME ").ToList();
                TAFZ1.DisplayMemberPath = "NAME";
                TAFZ1.SelectedValuePath = "TNUMBER";

                TAFZ2.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT     TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TNUMBER FROM TDETA_HES WHERE     (((TDETA_HES.NUMBER) = " + this.HMOIN1.SelectedValue + " ) AND ((TDETA_HES.N_KOL)= " + this.HKOL.SelectedValue + "AND (CODE_E = N'0') OR (CODE_E IS NULL))) ORDER BY TDETA_HES.TNUMBER DESC, TDETA_HES.NAME ").ToList();
                TAFZ1.DisplayMemberPath = "NAME";
                TAFZ1.SelectedValuePath = "TNUMBER";
            }
            else
            {
                TAFZ1.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT     TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TNUMBER FROM TDETA_HES WHERE     (((TDETA_HES.NUMBER) = " + this.HMOIN1.SelectedValue + " ) AND ((TDETA_HES.N_KOL)= " + this.HKOL.SelectedValue + " )) ORDER BY TDETA_HES.TNUMBER, TDETA_HES.NAME ").ToList();
                TAFZ1.DisplayMemberPath = "NAME";
                TAFZ1.SelectedValuePath = "TNUMBER";

                TAFZ2.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT     TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TNUMBER FROM TDETA_HES WHERE     (((TDETA_HES.NUMBER) = " + this.HMOIN1.SelectedValue + " ) AND ((TDETA_HES.N_KOL)= " + this.HKOL.SelectedValue + " )) ORDER BY TDETA_HES.TNUMBER DESC, TDETA_HES.NAME ").ToList();
                TAFZ2.DisplayMemberPath = "NAME";
                TAFZ2.SelectedValuePath = "TNUMBER";
            }
        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DAFTAR_MOIN_TAFKIK.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            //report.Render();
            //report.Show();
            new Rpts.WINRPT(report, "تفکیک معین").Show();
        }

        private void TAFZ1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TAFZ1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ComboBox comboBox = sender as ComboBox;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                string inputCode = textBox.Text;

                if (comboBox.SelectedValue == inputCode)
                {
                    return;
                }
                else
                {
                    if (int.TryParse(inputCode, out _))
                    {
                        // Cast the ItemsSource back to the correct type, and search for matching bank code
                        var moinlist = comboBox.ItemsSource as List<Q3>;

                        if (moinlist == null) return; // Ensure that the list is properly cast

                        var matchingmoin = moinlist.FirstOrDefault(b => b.TNUMBER == Convert.ToInt32(inputCode));

                        if (matchingmoin != null)
                        {
                            TAFZ1.SelectedValue = matchingmoin.TNUMBER;
                            comboBox.SelectedValue = matchingmoin.TNUMBER;
                            //return;
                        }
                        else
                        {
                            comboBox.SelectedValue = null;
                        }
                    }
                }
            }
        }

        private void TAFZ2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TAFZ2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ComboBox comboBox = sender as ComboBox;
            var textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                string inputCode = textBox.Text;

                if (comboBox.SelectedValue == inputCode)
                {
                    return;
                }
                else
                {
                    if (int.TryParse(inputCode, out _))
                    {
                        // Cast the ItemsSource back to the correct type, and search for matching bank code
                        var moinlist = comboBox.ItemsSource as List<Q3>;

                        if (moinlist == null) return; // Ensure that the list is properly cast

                        var matchingmoin = moinlist.FirstOrDefault(b => b.TNUMBER == Convert.ToInt32(inputCode));

                        if (matchingmoin != null)
                        {
                            TAFZ1.SelectedValue = matchingmoin.TNUMBER;
                            comboBox.SelectedValue = matchingmoin.TNUMBER;
                            //return;
                        }
                        else
                        {
                            comboBox.SelectedValue = null;
                        }
                    }
                }
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
