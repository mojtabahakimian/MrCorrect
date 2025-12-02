using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
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
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Prg_UI.UiTools;
using static Stimulsoft.Base.StiDbType;
using Prg_UI.Wins.WinOther;
using Prg_UI.HelperWins;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using DocumentFormat.OpenXml.Bibliography;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR_FRKH.xaml
    /// </summary>
    public partial class F_MENU_ANBAR_FRKH : Window
    {
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

        public Visual I_AM_F_MENU_ANBAR_FRKH { get; set; }
        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();
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
        public string OpenArgs { get; private set; } = "RF";
        public class Q3
        {
            public int? CODE { get; set; }
        }

        public class Q2
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public class Q1
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
        }

        bool First_Open = true;

        public string MANBAR { get; set; }
        public string DTT { get; set; }
        public string MKALA { get; set; }

        public F_MENU_ANBAR_FRKH(string _OpenArg_)
        {
            OpenArgs = _OpenArg_;
            InitializeComponent();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_F_MENU_ANBAR_FRKH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            DT2.Text = Tarikh.FullCurrentDate;
            DT1.Text = "11111111";
            CANBAR.Focus();
        }



        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<Q2>($"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR GROUP BY TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES HAVING (((TCOD_ANBAR.CODE)<>0)) ORDER BY TCOD_ANBAR.NAMES;").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            CANBAR.ItemsSource = ANBAR.ItemsSource;
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";
        }

        private void CANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (First_Open == true)
            {
                First_Open = false;

                return;
            }

            ANBAR.SelectedValue = CANBAR.SelectedValue;
            //KALA.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF INNER JOIN STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (((STUF_FSK.ANBAR)= {ANBAR.SelectedValue})) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            //KALA.SelectedValuePath = "CODE";
            //KALA.DisplayMemberPath = "NAME";
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر


            if (ANBAR.SelectedValue is null)
            {
                return;
            }

            CANBAR.SelectedValue = ANBAR.SelectedValue;
            KALA.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF INNER JOIN STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (((STUF_FSK.ANBAR)= {ANBAR.SelectedValue})) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            KALA.SelectedValuePath = "CODE";
            KALA.DisplayMemberPath = "NAME";
        }

        private void KALA_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (KALA.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox CUTSNO_TEX = (TextBox)KALA.Template.FindName("PART_EditableTextBox", KALA);
            if (CUTSNO_TEX.Text is null || CUTSNO_TEX.Text == "")
            {
                Msgwin msgwin = new Msgwin(false, "کالا نباید خالی باشد");
                msgwin.ShowDialog();
                return;
            }

            if (CUTSNO_TEX is null)
            {
                return;
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                if (ANBAR.SelectedValue is null)
                {
                    new Msgwin(false, "ابتدا انبار را انتخاب کنید").ShowDialog();
                    return;
                }

                var selectedAnbar = ANBAR.SelectedValue.ToString();
                SERCHK sERCHK = new SERCHK(I_AM_F_MENU_ANBAR_FRKH, selectedAnbar);
                sERCHK.ShowDialog();

                if (FROM_SAERCH_KAL.CODE is null)
                {
                    return;
                }
                else
                {
                    KALA.SelectedValue = Convert.ToInt32(FROM_SAERCH_KAL.CODE);
                    //Cleaning
                    FROM_SAERCH_KAL.CODE = null;
                    FROM_SAERCH_KAL.NAME_CODE = null;
                }
            }
        }

        private void OpenReport()
        {

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.LIST_FROOSH_ANBARS.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();

            report["ANBAR_PARM"] = MANBAR;
            report["KALA_PARM"] = MKALA;

            var salN = report.GetComponentByName("SAL_N") as StiText;
            if (salN != null)
            {
                salN.Text = Baseknow.WIDTH_D ?? "";
            }




            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "گزارش خرید فروش از انبار ها").Show();
        }

        private void OpenReport2()
        {

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.LIST_KHAREED_ANBARS.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();

            if (ANBAR.SelectedValue != null)
            {
                report["ANBAR_PARM"] = ANBAR.SelectedValue.ToString();
            }
            else
            {
                report["ANBAR_PARM"] = "%";
            }

            if (KALA.SelectedValue != null)
            {
                report["KALA_PARM"] = KALA.SelectedValue.ToString();
            }
            var _sql_query = $"SELECT ANBAR, NAME, MEGHk, MABL, MABL_K, DATE_N, NUMBER, CODE FROM dbo.Q_FR_KH_ANBAR(N'{DT1.Text.ToRawTarikh()}', N'{DT2.Text.ToRawTarikh()}', N'{(ANBAR.SelectedValue != null ? ANBAR.SelectedValue.ToString() : "1")}', N'{(KALA.SelectedValue != null ? KALA.SelectedValue.ToString() : "%")}')";
            report.Dictionary.Variables.Add("Q_PARM", _sql_query);
            var salN = report.GetComponentByName("SAL_N") as StiText;
            if (salN != null)
            {
                salN.Text = Baseknow.WIDTH_D ?? "";
            }
            new Rpts.WINRPT(report, "گزارش خرید فروش از انبار ها").Show();
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string PATH;
            int i;
            if (IsNull(this.DT1.Text.ToRawTarikh()))
            {
                DT1.Text = Convert.ToString(Baseknow.YEA + "0101");
            }
            if (IsNull(this.DT2.Text.ToRawTarikh()) || IsNull(this.DT1.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            if (IsNull(this.ANBAR.SelectedValue))
            {
                MANBAR = "%";
            }
            else
            {
                MANBAR = this.ANBAR.SelectedValue.ToString();
            }
            if (IsNull(this.KALA.SelectedValue))
            {
                MKALA = "%";
                //universControl.PopNotifyShow("کالا نمیتواند خالی باشد!", Pop1, Pop1Text1, Pop_Border1);
                //return;
            }
            else
            {
                MKALA = this.KALA.SelectedValue.ToString();
            }
            switch (this.OpenArgs)
            {
                case "RF":
                    {
                        OpenReport();

                        break;
                    }
                case "RK":
                    {
                        OpenReport2();
                        break;
                    }
                case "F":
                    {

                        break;
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
