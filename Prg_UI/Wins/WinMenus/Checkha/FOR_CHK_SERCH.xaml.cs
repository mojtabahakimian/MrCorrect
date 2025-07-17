using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

namespace Prg_UI.Wins.WinMenus.Checkha
{
    /// <summary>
    /// Interaction logic for FOR_CHK_SERCH.xaml
    /// </summary>
    public partial class FOR_CHK_SERCH : Window
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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
        public string? N_SERI_FILTER { get; set; }
        private Visual VL_WIN { get; set; }
        public string OpenArgs { get; set; }
        public string RowSource { get; set; }
        public FOR_CHK_SERCH(string openargs, string _n_seri_filter, Visual vsl)
        {
            VL_WIN = vsl;
            N_SERI_FILTER = _n_seri_filter;
            OpenArgs = openargs;

            if (OpenArgs is "2")
            {
                RowSource = $@"SELECT dbo.PAY_GETD.N_SERI, dbo.PAY_GETD.BANK, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.DATE_S, dbo.PAY_GETD.DATE, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.MABL, dbo.PAY_GETD.NAME_TAH, 
                dbo.PAY_GETD.N_HESAB, dbo.PAY_GETD.RADIF, dbo.PAY_GETD.HES1, dbo.PAY_GETD.HES2, dbo.PAY_GETD.HES3
               FROM            dbo.PAY_GETD LEFT OUTER JOIN
                                        dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE
               WHERE        (dbo.PAY_GETD.N_KOL IS NULL) AND (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL3 IS NULL)";
            }
            if (OpenArgs is "3")
            {
                RowSource = $@"SELECT dbo.PAY_GETD.N_SERI, dbo.PAY_GETD.BANK, dbo.PAY_GETD.DATE_S, dbo.PAY_GETD.DATE, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.MABL, dbo.PAY_GETD.NAME_TAH, dbo.PAY_GETD.N_HESAB, dbo.PAY_GETD.RADIF, 
                         dbo.PAY_GETD.HES1, dbo.PAY_GETD.HES2, dbo.PAY_GETD.HES3, dbo.TCOD_BANKS.NAMES
                  FROM            dbo.PAY_GETD LEFT OUTER JOIN
                         dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE";
            }
            if (OpenArgs is "1")
            {
                RowSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, RADIF, HES1, HES2, HES3 FROM PAY_GETD WHERE (N_KOL IS NULL) AND (N_KOL2 IS NULL) AND (N_KOL3 IS NULL)";
            }

            InitializeComponent();
        }

        public FOR_CHK_MODEL Check_Item_selected { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var DataCHK = dbms.DoGetDataSQL<FOR_CHK_MODEL>($" {RowSource} AND (dbo.PAY_GETD.{N_SERI_FILTER}) ").ToList();

            var BanksData = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT CODE, NAMES FROM TCOD_BANKS").ToList();

            foreach (var row in DataCHK)
            {
                row.NAMES = BanksData.FirstOrDefault(x=>x.CODE == row.BANK).NAMES;
            }
            SUB_FOR_CHK_SERCH.ItemsSource = DataCHK;

            // AND (dbo.PAY_GETD.{N_SERI_FILTER})
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                if (SUB_FOR_CHK_SERCH.Items.Count > 0)
                {
                    if (SUB_FOR_CHK_SERCH.SelectedItem is not null)
                    {
                        var ITMSELECTED = (SUB_FOR_CHK_SERCH.SelectedItem as FOR_CHK_MODEL);
                        //(VL_WIN as FORCHEK). = (SUB_FOR_CHK_SERCH.SelectedItem as FOR_CHK_MODEL)
                        //On KeyPress:
                        if (true)
                        {

                            if (this.OpenArgs == "1")
                            {
                                //ERROR صدندوق نمیتواند مقدار دهی بشود
                                (VL_WIN as FORCHEK).RADIF.Text = ITMSELECTED.RADIF.ToString();
                                //Forms["FORCHEK"]["RADIF"] = ITMSELECTED.RADIF;
                                (VL_WIN as FORCHEK).N_SERI.SelectedValue = ITMSELECTED.N_SERI;
                                //Forms["FORCHEK"]["N_SERI"] = this.N_SERI;
                                (VL_WIN as FORCHEK).BANK.SelectedValue = ITMSELECTED.BANK;
                                (VL_WIN as FORCHEK).BANK.Items.Refresh();
                                //Forms["FORCHEK"]["BANK"] = this.BANK;
                                (VL_WIN as FORCHEK).SHOBEH.Text = ITMSELECTED.SHOBEH;
                                //Forms["FORCHEK"]["SHOBEH"] = this.SHOBEH;
                                (VL_WIN as FORCHEK).DATE_S.Text = ITMSELECTED.DATE_S.ToString();
                                //Forms["FORCHEK"]["DATE_S"] = this.DATE_S;
                                (VL_WIN as FORCHEK).DATE.Text = ITMSELECTED.DATE.ToString();
                                //Forms["FORCHEK"]["DATE"] = this.DATE;
                                (VL_WIN as FORCHEK).N_HESAB.Text = ITMSELECTED.N_HESAB;
                                //Forms["FORCHEK"]["N_HESAB"] = this.N_HESAB;
                                (VL_WIN as FORCHEK).NAME_TAH.Text = ITMSELECTED.NAME_TAH;
                                //Forms["FORCHEK"]["NAME_TAH"] = this.NAME_TAH;
                                (VL_WIN as FORCHEK).MABL.Text = ITMSELECTED.MABL.ToString();
                                //Forms["FORCHEK"]["MABL"] = this.MABL
                                //DoCmd.Close(acForm, this.NAME);
                            }
                            if (this.OpenArgs == "3")
                            {
                                (VL_WIN as FORCHEK).RADIF.Text = ITMSELECTED.RADIF.ToString();
                                //Forms["FORCHEKS"]["RADIF"] = this.RADIF;
                                (VL_WIN as FORCHEK).N_SERI.SelectedValue = ITMSELECTED.N_SERI;
                                //Forms["FORCHEKS"]["N_SERI"] = this.N_SERI;
                                (VL_WIN as FORCHEK).BANK.SelectedValue = ITMSELECTED.BANK;
                                //Forms["FORCHEKS"]["BANK"] = this.BANK;
                                (VL_WIN as FORCHEK).SHOBEH.Text = ITMSELECTED.SHOBEH;
                                //Forms["FORCHEKS"]["SHOBEH"] = this.SHOBEH;
                                (VL_WIN as FORCHEK).DATE_S.Text = ITMSELECTED.DATE_S.ToString();
                                //Forms["FORCHEKS"]["DATE_S"] = this.DATE_S;
                                (VL_WIN as FORCHEK).DATE.Text = ITMSELECTED.DATE.ToString();
                                //Forms["FORCHEKS"]["DATE"] = this.DATE;
                                (VL_WIN as FORCHEK).N_HESAB.Text = ITMSELECTED.N_HESAB;
                                //Forms["FORCHEKS"]["N_HESAB"] = this.N_HESAB;
                                (VL_WIN as FORCHEK).NAME_TAH.Text = ITMSELECTED.NAME_TAH;
                                //Forms["FORCHEKS"]["NAME_TAH"] = this.NAME_TAH;
                                (VL_WIN as FORCHEK).MABL.Text = ITMSELECTED.MABL.ToString();
                                //Forms["FORCHEKS"]["MABL"] = this.MABL;
                                //DoCmd.Close(acForm, this.NAME);
                            }
                            if (this.OpenArgs == "2")
                            {
                                //rst.Open("SELECT * FROM CHREC_HES WHERE [DATE] = " + Forms["VBP_CHECK"]["DTS"]);
                                //if (rst.RecordCount == 0)
                                //{
                                //    rst.AddNew();
                                //    rst.Fields("DATE") = Forms["VBP_CHECK"]["DTS"];
                                //    rst.Fields("MOLAH") = "به حساب گذاشته شده برگه";
                                //    rst.update();
                                //}
                                //rst.Close();
                                //rst.Open("SELECT * FROM CHRE_LSPH WHERE N_SERI = " + this.N_SERI + " AND BANK = " + this.BANK + " AND DATE_S = " + this.DATE_S, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                                //if (rst.RecordCount == 0)
                                //{
                                //    rst.AddNew();
                                //    rst.Fields("DATE") = Forms["VBP_CHECK"]["DTS"];
                                //    rst.Fields("DATE_S") = this.DATE_S;
                                //    rst.Fields("N_SERI") = this.N_SERI;
                                //    rst.Fields("BANK") = this.BANK;
                                //    rst.Fields("RADIF") = Forms["VBP_CHECK"]["LIST_NO"];
                                //    rst.update();
                                //    rst.Close();
                                //    rst.Open("SELECT * FROM PAY_GETD WHERE N_SERI = " + this.N_SERI + " AND BANK = " + this.BANK + " AND DATE_S = " + this.DATE_S, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                                //    if (rst.RecordCount > 0)
                                //    {
                                //        rst.Fields("N_KOL") = GETKOL(Forms["VBP_CHECK"]["HES"]);
                                //        rst.Fields("N_MOIN") = GETMOIN(Forms["VBP_CHECK"]["HES"]);
                                //        rst.Fields("N_TAF") = GETTAF(Forms["VBP_CHECK"]["HES"]);
                                //        rst.update();
                                //    }
                                //    rst.Close();
                                //    Forms["VBP_CHECK"]["VBP_CHECK_SUB"].Form.Refresh();
                                //    Forms["VBP_CHECK"]["VBP_CHECK_SUB"].Form.Requery();
                                //}
                                //else
                                //{
                                //    DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "اين چك قبلا در ليست شماره " + rst.Fields("radif") + "به حساب گذاشته شده است. ");
                                //}
                                //DoCmd.Close(acForm, this.NAME);
                            }
                        }
                        this.Close();
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void SUB_FOR_CHK_SERCH_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (SUB_FOR_CHK_SERCH.SelectedItem != null)
            //{
            //    if (e.Key is Key.Enter)
            //    {

            //    }
            //}
        }
    }
}
