using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Wins.WinMenus.HESABDARI;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class F_MENU_MOGHAYERAT : Window
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
        public F_MENU_MOGHAYERAT()
        {
            InitializeComponent();
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //
                string Formname = "MOGHA";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            DT2.Text = Tarikh.FullCurrentDate;
            FILL_ALL_COMBOBOXES();
            XTAF.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            XTAF.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
            HHTAF.ItemsSource = XTAF.ItemsSource;

            XTAF.SelectedIndex = -1; XTAF.Items.Refresh();
            HHTAF.SelectedIndex = -1; HHTAF.Items.Refresh();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!BTN_GO.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }
        UniversControl universControl = new UniversControl();
        private bool HeaderIsValid()
        {
            if (!Tarikh.IsValidedDate(DT1.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("مقدار از تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }
            if (!Tarikh.IsValidedDate(DT2.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("مقدار تا تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }
            if (XTAF.SelectedValue is null)
            {
                universControl.PopNotifyShow("حسابی انتخاب نشده!.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }

            return true;
        }
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid())
            {
                return;
            }

            var KOL = CL_HESABDARI.GETKOL(XTAF.SelectedValue.ToString());
            var MOIN = CL_HESABDARI.GETMOIN(XTAF.SelectedValue.ToString());
            var TAF = CL_HESABDARI.GETTAF(XTAF.SelectedValue.ToString());

            string PATH = null;
            int i = 0;

            if (string.IsNullOrEmpty(TAF.ToStringNullSafe()))
            {
                new Msgwin(false, "پارامتر ها کافی نیست").ShowDialog();
                return;
            }

            var RST = dbms.DoGetDataSQL<int?>("SELECT Max(MOGHHEAD.MONUM) AS MaxOfMONUM FROM MOGHHEAD").FirstOrDefault();
            if (RST is null)
            {
                i = 1;
            }
            else
            {
                i = (int)(RST + 1);
            }
            var MON = i;

            dbms.DoExecuteSQL($@"INSERT INTO dbo.MOGHHEAD(MONUM, MODATE, ONVAN, HES, FDATE, TDATE)
                                 VALUES({i},
                                 {Tarikh.FullCurrentDate},
                                 N'مغايرت حساب' ,
                                 N'{XTAF.SelectedValue}' ,
                                 {DT1.Text.ToRawTarikh()} ,
                                 {DT2.Text.ToRawTarikh()}    -- TDATE - bigint
                                     )");
            //RST.Open "MOGHHEAD", CurrentProject.Connection, adOpenKeyset, adLockOptimistic;
            //RST.AddNew;
            //RST.Fields["MONUM"] = i;
            //RST.Fields["MODATE"] = FARSIDATE(DateTime);
            //RST.Fields["ONVAN"] = "مغايرت حساب";
            //RST.Fields["HES"] = XTAF;
            //RST.Fields["FDATE"] = this.DT1;
            //RST.Fields["TDATE"] = this.DT2;
            //RST.update;
            dbms.DoExecuteSQL("INSERT INTO dbo.MO_DTL (MONUM, N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, NUMBER, TAG,ID) SELECT " + MON + " AS MMM, dbo.DEED_DTL.N_S, dbo.DEED_DTL.HES_K, dbo.DEED_DTL.HES_M, dbo.DEED_DTL.HES_T, dbo.DEED_DTL.SHARH, dbo.DEED_DTL.BED , dbo.DEED_DTL.BES, dbo.DEED_DTL.N_SERI, dbo.DEED_DTL.BANK, dbo.DEED_DTL.NUMBER, dbo.DEED_DTL.TAG , dbo.DEED_DTL.ID FROM dbo.DEED_HED INNER JOIN  dbo.DEED_DTL ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S AND dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S AND dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S And dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S WHERE (dbo.DEED_DTL.HES_K = " + KOL + ") AND (dbo.DEED_DTL.HES_M = " + MOIN + ") AND (dbo.DEED_DTL.HES_T = " + TAF + ") AND (dbo.DEED_HED.DATE_S BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + ")");
            new MOGHAYERAT(i).Show();
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
