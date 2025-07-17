using AUTO_BAZ.HelperWins;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
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
    /// Interaction logic for BACK_CHK_SERCH.xaml
    /// </summary>
    public partial class BACK_CHK_SERCH : Window
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
        public BACK_CHK_SERCH(string _n_seri , Visual _vl_win)
        {
            N_SERI_FILTER = _n_seri;
            VL_WIN = _vl_win;
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            #region KEY_Press
            if (e.Key is Key.Enter)
            {
                if (SUB_BACK_CHK_SERCH.Items.Count > 0)
                {
                    if (SUB_BACK_CHK_SERCH.SelectedItem is not null)
                    {
                        var ITMSELECTED = (SUB_BACK_CHK_SERCH.SelectedItem as BACK_CHK_SERCH_Model);
                        if (true/*this.OpenArgs == 1*/)
                        {
                            (VL_WIN as BAKCHEK).RADIF.Text = ITMSELECTED.RADIF.ToString();
                            //Forms["BAKCHEK"]["RADIF"] = ITMSELECTED.RADIF;
                            (VL_WIN as BAKCHEK).N_SERI.SelectedValue = ITMSELECTED.N_SERI;
                            //Forms["BAKCHEK"]["N_SERI"] = this.N_SERI;
                            (VL_WIN as BAKCHEK).BANK.SelectedValue = ITMSELECTED.BANK;
                            //Forms["BAKCHEK"]["BANK"] = this.BANK;
                            (VL_WIN as BAKCHEK).SHOBEH.Text = ITMSELECTED.SHOBEH;
                            //Forms["BAKCHEK"]["SHOBEH"] = this.SHOBEH;
                            (VL_WIN as BAKCHEK).DATE_S.Text = ITMSELECTED.DATE_S.ToString();
                            //Forms["BAKCHEK"]["DATE_S"] = this.DATE_S;
                            (VL_WIN as BAKCHEK).DATE.Text = ITMSELECTED.DATE.ToString();
                            //Forms["BAKCHEK"]["DATE"] = this.DATE;
                            (VL_WIN as BAKCHEK).N_HESAB.Text = ITMSELECTED.N_HESAB;
                            //Forms["BAKCHEK"]["N_HESAB"] = this.N_HESAB;
                            (VL_WIN as BAKCHEK).NAME_TAH.Text = ITMSELECTED.NAME_TAH;
                            //Forms["BAKCHEK"]["NAME_TAH"] = this.NAME_TAH;
                            (VL_WIN as BAKCHEK).MABL.Text = ITMSELECTED.MABL.ToString();
                            //Forms["BAKCHEK"]["MABL"] = this.MABL;
                            (VL_WIN as BAKCHEK).KOL.Text = ITMSELECTED.N_KOL.ToString();
                            //Forms["BAKCHEK"]["KOL"] = this.N_KOL;
                            (VL_WIN as BAKCHEK).MOIN.Text = ITMSELECTED.N_MOIN.ToString();
                            //Forms["BAKCHEK"]["MOIN"] = this.N_MOIN;
                            (VL_WIN as BAKCHEK).TAF.Text = ITMSELECTED.N_TAF.ToString();
                            //Forms["BAKCHEK"]["TAF"] = this.N_TAF;
                            (VL_WIN as BAKCHEK).HES1.SelectedValue = ITMSELECTED.HES1;
                            //Forms["BAKCHEK"]["HES1"] = this.HES1;
                            this.Close();
                        }
                        //DoCmd.Close();
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (N_SERI_FILTER is not null)
            {
            SUB_BACK_CHK_SERCH.ItemsSource = dbms.DoGetDataSQL<BACK_CHK_SERCH_Model>($@"SELECT dbo.PAY_GETD.N_SERI, dbo.PAY_GETD.BANK, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.DATE_S, dbo.PAY_GETD.DATE, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.MABL, dbo.PAY_GETD.NAME_TAH, 
                                                               dbo.PAY_GETD.N_HESAB, dbo.PAY_GETD.RADIF, dbo.PAY_GETD.N_KOL, dbo.PAY_GETD.N_MOIN, dbo.PAY_GETD.N_TAF, dbo.PAY_GETD.HES1
                                                               FROM            dbo.PAY_GETD LEFT OUTER JOIN
                                                                                        dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE
                                                               WHERE        (dbo.PAY_GETD.N_KOL3 IS NULL) AND (dbo.PAY_GETD.N_KOL2 IS NULL)  AND (dbo.PAY_GETD.{N_SERI_FILTER})").ToList();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "چکی با این شماره سریال وجود ندارد ، لطفا از صحت شماره سریال چک اطمینان حاصل فرمایید");
                msgwin.ShowDialog();
            }

        }
    }
}
