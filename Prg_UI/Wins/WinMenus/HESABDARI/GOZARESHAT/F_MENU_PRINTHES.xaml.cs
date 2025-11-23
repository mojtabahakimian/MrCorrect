using DocumentFormat.OpenXml.Wordprocessing;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
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
using static Stimulsoft.Base.StiDbType;
using Prg_Proccessy.FUNCTIONS;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_PRINTHES.xaml
    /// </summary>
    public partial class F_MENU_PRINTHES : Window
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
        public F_MENU_PRINTHES()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        public string _sql_query { get; set; }

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

        public Visual I_AM_PRINTHES { get; set; }

        public class Q1
        {
            public int? NUMBER { get; set; }
        }

        public class Q2
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_PRINTHES = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            HKOL.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //
                string Formname = "SPRIN";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

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
            HKOL.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NUMBER;").ToList();
            HKOL.DisplayMemberPath = "NUMBER";
            HKOL.SelectedValuePath = "NUMBER";

            HHKOL.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HHKOL.DisplayMemberPath = "NAME";
            HHKOL.SelectedValuePath = "NUMBER";

            H2K.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NUMBER;").ToList();
            H2K.DisplayMemberPath = "NUMBER";
            H2K.SelectedValuePath = "NUMBER";

            HH2K.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HH2K.DisplayMemberPath = "NAME";
            HH2K.SelectedValuePath = "NUMBER";
        }

        private void HKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HHKOL.SelectedValue = HKOL.SelectedValue;
        }

        private void HHKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HKOL.SelectedValue = HHKOL.SelectedValue;
        }

        private void H2K_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (H2K.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HH2K.SelectedValue = H2K.SelectedValue;
        }

        private void HH2K_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HH2K.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            H2K.SelectedValue = HH2K.SelectedValue;
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string PATH;
            int i;
            if (IsNull(this.H2K.SelectedValue) && IsNull(this.HKOL.SelectedValue))
            {
                this.HKOL.SelectedIndex = 0;
                this.H2K.SelectedIndex = H2K.Items.Count - 1;
            }
            if (IsNull(this.HKOL.SelectedValue))
            {
                this.HKOL.SelectedIndex = 0;
            }
            if (IsNull(this.H2K.SelectedValue))
            {
                this.H2K.SelectedIndex = H2K.Items.Count - 1;
            }
            //DoCmd.OpenReport("TOTA_HES", acViewPreview, default, "kol >= " + this.HKOL + " and kol <= " + this.H2K);
            OpenReport();

            this.Close();
        }

        private void OpenReport()
        {

            _sql_query = $"SELECT TOTA_HES.NUMBER AS kol, TOTA_HES.NAME, DETA_HES.N_KOL, DETA_HES.NUMBER, DETA_HES.NAME AS detahes_name, TDETA_HES.N_KOL AS tdetahes_nkol, TDETA_HES.NUMBER AS tdetahes_number, TDETA_HES.TNUMBER, TDETA_HES.NAME AS tdetahes_name FROM TOTA_HES INNER JOIN DETA_HES ON TOTA_HES.NUMBER = DETA_HES.N_KOL INNER JOIN TDETA_HES ON DETA_HES.N_KOL = TDETA_HES.N_KOL AND DETA_HES.NUMBER = TDETA_HES.NUMBER WHERE TOTA_HES.NUMBER >= {HKOL.SelectedValue} AND TOTA_HES.NUMBER <= {H2K.SelectedValue}";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TOTA_HES.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);


            (report.GetComponentByName("DT1_N") as StiText).Text = Tarikh.FullCurrentDate.ToString();

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "دفتر کل").Show();
        }
    }
}
