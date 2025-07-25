using System;
using System.Collections.Generic;
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
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using System.Windows.Interop;
using Prg_UI.Wins.WinOther;
using Rpts;
using System.Reflection;
using Stimulsoft.Svg.ExCSS;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_DATE_HES.xaml
    /// </summary>
    public partial class F_MENU_DATE_HES : Window
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

        public F_MENU_DATE_HES()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public Visual I_AM_WIN_F_MENU_DATE_HES { get; set; }

        UniversControl universControl = new UniversControl();
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
        public string _sql_query { get; set; }
        public string OpenArgs { get; set; } 


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
            public string? hes { get; set; }
        }

        public class Q2
        {
            public string? hes { get; set; }
            public string? nam { get; set; }
            public string? Expr1 { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_WIN_F_MENU_DATE_HES = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();

            DT2.Text = Tarikh.FullCurrentDate.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_GO.IsFocused) //Not Focused on this button
                {
                    e.Handled = true;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            HES1.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT hes FROM CUST_HESAB").ToList();
            HES1.SelectedValuePath = "hes";
            HES1.DisplayMemberPath = "hes";

            HES11.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT hes, NAME AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            HES11.SelectedValuePath = "hes";
            HES11.DisplayMemberPath = "nam";

            HES2.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT hes FROM CUST_HESAB").ToList();
            HES2.SelectedValuePath = "hes";
            HES2.DisplayMemberPath = "hes";

            HES22.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT hes, NAME AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            HES22.SelectedValuePath = "hes";
            HES22.DisplayMemberPath = "nam";
        }

        private void HES1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            #region Not_In_List
            TextBox CUTSNO_TEX = (TextBox)HES1.Template.FindName("PART_EditableTextBox", HES1);
            if (CUTSNO_TEX.Text is null || CUTSNO_TEX.Text == "")
            {
                Msgwin msgwin = new Msgwin(false, "مشتری نباید خالی باشد");
                msgwin.ShowDialog();
                return;
            }
            if (HES1.SelectedIndex == -1)
            {
                if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("HEAD_LST_HAV_OTHER_WIN", I_AM_WIN_F_MENU_DATE_HES);//Search Plusy Form Specialy for Customers
                    CMBSearch.ShowDialog();

                    if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                    {
                        HES1.SelectedValue = HESAB_FROM_SEARCH.FULL_HES;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                    HESAB_FROM_SEARCH.DoClear();


                }
                else
                {
                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes FROM dbo.CUST_HESAB WHERE (hes = N'" + CUTSNO_TEX.Text + "')").FirstOrDefault();
                    if (data.hes is null || !string.IsNullOrEmpty(data.hes))
                    {
                        HES1.SelectedValue = data.hes;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                }
            }
            #endregion

            #region After_Update
            if (HES1.SelectedValue is not null)
            {
                HES11.SelectedValue = HES1.SelectedValue;
            }
            #endregion

            #region On_Exit
            if (HES1.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HES1.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر دارای تفصیلی می‌باشد باید تفصیلی آن را انتخاب کنید!");
                    msgwin.ShowDialog();
                    return;
                }
            }
            #endregion


        }

        private void HES11_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            #region After_Update
            if (HES11.SelectedValue is not null)
            {
                HES1.SelectedValue = HES11.SelectedValue;
            }
            #endregion

            #region On_Exit
            if (HES11.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HES11.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر دارای تفصیلی می‌باشد باید تفصیلی آن را انتخاب کنید!");
                    msgwin.ShowDialog();
                    return;
                }
            }
            #endregion
        }

        private void HES2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            #region Not_In_List
            TextBox CUTSNO_TEX = (TextBox)HES2.Template.FindName("PART_EditableTextBox", HES2);
            if (CUTSNO_TEX.Text is null || CUTSNO_TEX.Text == "")
            {
                Msgwin msgwin = new Msgwin(false, "مشتری نباید خالی باشد");
                msgwin.ShowDialog();
                return;
            }
            if (HES2.SelectedIndex == -1)
            {
                if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("HEAD_LST_HAV_OTHER_WIN", I_AM_WIN_F_MENU_DATE_HES);//Search Plusy Form Specialy for Customers
                    CMBSearch.ShowDialog();

                    if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                    {
                        HES2.SelectedValue = HESAB_FROM_SEARCH.FULL_HES;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                    HESAB_FROM_SEARCH.DoClear();


                }
                else
                {
                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes FROM dbo.CUST_HESAB WHERE (hes = N'" + CUTSNO_TEX.Text + "')").FirstOrDefault();
                    if (data.hes is null || !string.IsNullOrEmpty(data.hes))
                    {
                        HES2.SelectedValue = data.hes;
                    }
                    else
                    {
                        new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                        return;
                    }
                }
            }
            #endregion

            #region After_Update
            if (HES2.SelectedValue is not null)
            {
                HES22.SelectedValue = HES2.SelectedValue;
            }
            #endregion

            #region On_Exit
            if (HES2.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HES2.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر دارای تفصیلی می‌باشد باید تفصیلی آن را انتخاب کنید!");
                    msgwin.ShowDialog();
                    return;
                }
            }
            #endregion
        }

        private void HES22_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES22.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            #region After_Update
            if (HES22.SelectedValue is not null)
            {
                HES2.SelectedValue = HES22.SelectedValue;
            }
            #endregion

            #region On_Exit
            if (HES22.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HES22.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر دارای تفصیلی می‌باشد باید تفصیلی آن را انتخاب کنید!");
                    msgwin.ShowDialog();
                    return;
                }
            }
            #endregion
        }

        private void OpenReport()
        {
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.LIST_FROOSH_KALA_BASKOL.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["HES1_PARM"] = HES1.SelectedValue.ToString();
            report["HES2_PARM"] = HES2.SelectedValue.ToString();
            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = DT1.Text.ToRawTarikh().ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = DT2.Text.ToRawTarikh().ToString();

            //report["DATE_S"] = DT2.Text.ToString();
            //report["DATE_F"] = DT2.Text.ToString();
            //report["ANBAR_F"] = ANBAR.Text.ToString();

            //report["AZDATE"] = Baseknow.YEA + "0101";
            //report["ANBAR"] = ANBAR.SelectedValue.ToString();

            //string TaTarikh = "99999999";
            //if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            //report["TADATE"] = TaTarikh;

            //report["KALACODE"] = KALA.SelectedValue.ToString();
            //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;

            //Report_Open:
            //(report.GetComponentByName("Table1_Cell17") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEG
            //(report.GetComponentByName("Table1_Cell14") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK

            //report.Render();
            //report.Show();

            new WINRPT(report, "لیست فروش کالا ها به تفکیک فاکتور").Show();
        }

        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            if (IsNull(this.HES1.SelectedValue) || IsNull(this.HES2.SelectedValue))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (String.IsNullOrEmpty(this.DT1.Text.ToRawTarikh()))
            {
                if (this.OpenArgs == "HBGHB")
                {
                    DT1.Text = Tarikh.FullCurrentDate.ToString();
                }
                else
                {
                    DT1.Text = Convert.ToString(Baseknow.YEA + "0101");
                }
            }

            if (String.IsNullOrEmpty(this.DT2.Text.ToRawTarikh()))
            {
                DT2.Text = Tarikh.FullCurrentDate.ToString();
            }

            OpenReport();
        }
    }
}
