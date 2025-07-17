using MaterialDesignThemes.Wpf;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Prg_Proccessy.MODELS;
using Prg_UI.Functions;
using System.Linq;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class F_MENU_FROOSH : Window
    {
        public F_MENU_FROOSH(string _OpenArg_)
        {
            OpenArgs = _OpenArg_;
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

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

        UniversControl universControl = new UniversControl();

        public string OpenArgs { get; private set; } = "R";

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

        public class Q1
        {
            public double? NUMBER { get; set; }
        }

        public string DTT { get; set; }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {

            if (IsNull(this.SNDNUM2.SelectedValue) || IsNull(this.SNDNUM1.SelectedValue))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }


            if (Option23.IsChecked == true)
            {
                OpenReport();
            }
        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Kharid_Froosh.INCOICE_FROOSH_GROUP.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARM"] = SNDNUM1.SelectedValue.ToString();
            report["NUMBER_PARM2"] = SNDNUM2.SelectedValue.ToString();

            (report.GetComponentByName("SELL_N") as StiText).Text = Baseknow.NAME.ToString();
            (report.GetComponentByName("TEL_N") as StiText).Text = Baseknow.TFTEL.ToString();
            (report.GetComponentByName("TAHVIL_N") as StiText).Text = Baseknow.TFADDRESS.ToString();
            (report.GetComponentByName("USER_N") as StiText).Text = Baseknow.UUSER.ToString();
            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();


            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "گزارش موجودی انبار").Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Fill_ComboBoxes();
            SNDNUM1.Focus();

            //if (this.mab > 0)
            //{
            //    COMM.CAPTION = "چكهاي دريافت شده " + this.CountOfMABL + " فقره جمعاًبه مبلغ :" + Strings.Format(this.mab, "#,### ريال") + "  ";
            //}

            //this.HR.CAPTION = ALPHANUM(JF + MABL_HAZ) + " " + "ريال";
        }

        public void Fill_ComboBoxes()
        {
            SNDNUM1.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT TOP 100 PERCENT NUMBER FROM HEAD_LST GROUP BY NUMBER, TAG HAVING (TAG = 2) ORDER BY NUMBER").ToList();
            SNDNUM1.SelectedValuePath = "NUMBER";
            SNDNUM1.DisplayMemberPath = "NUMBER";

            SNDNUM2.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT TOP 100 PERCENT NUMBER FROM HEAD_LST GROUP BY NUMBER, TAG HAVING (TAG = 2) ORDER BY NUMBER").ToList();
            SNDNUM2.SelectedValuePath = "NUMBER";
            SNDNUM2.DisplayMemberPath = "NUMBER";
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
    }
}

