using DocumentFormat.OpenXml.Drawing;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
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

namespace Wins.WinMenus.HESABDARI
{
    public partial class F_MENU_ASNAD : Window
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
        public F_MENU_ASNAD()
        {
            InitializeComponent();
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //
                string Formname = "TAEED";
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

            SNDNUM1.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!BTN_GO.IsFocused)
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private bool IsReallyNull(string _TEXT_)
        {
            if (string.IsNullOrEmpty(_TEXT_) || string.IsNullOrWhiteSpace(_TEXT_) || _TEXT_ == "0")
            {
                return true;
            }
            return false;
        }
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            if (IsReallyNull(SNDNUM2.Text) || IsReallyNull(SNDNUM1.Text))
            {
                new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                return;
            }
            else
            {
                Msgwin msgwin = new Msgwin(true, "آيا براي تائيد اسناد اطمينان داريد. در صورت تائيد اسناد ديگر امكان تغيير در سند وجود نخواهد داشت!");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    var AFFECTED = dbms.DoExecuteSQL("UPDATE DEED_HED Set DEED_HED.GHATEI = -1 WHERE (((DEED_HED.N_S) BETWEEN " + SNDNUM1.Text + " AND " + SNDNUM2.Text + "))");
                    new Msgwin(false, $"تعداد {AFFECTED} اسناد با موفقيت تائيد گرديد");
                }
            }
        }
    }
}
