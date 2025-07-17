using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Syncfusion.Data.Extensions;
using Wins.WinMenus.SALARY;

namespace Wins.WinOther
{
    public partial class SERCHP : Window
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
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private Window TheWindow { get; set; }
        public string OpenArgs { get; }
        public List<TDETA_HES> RecordSource { get; private set; }

        public SERCHP(string _arg, IntPtr windowHandle)
        {
            TheWindow = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            InitializeComponent();
            OpenArgs = _arg;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double? PSK1 = null;
            double? PSM1 = null;
            double? PST1 = null;
            double? PST2 = null;
            double? PST3 = null;
            double? PST4 = null;
            CL_HESABDARI.GETTAF3(Baseknow.PERSONEL, ref PSK1, ref PSM1, ref PST1, ref PST2, ref PST3, ref PST4);
            if (string.IsNullOrEmpty(PST2?.ToStringNullSafe()))
            {
                RecordSource = dbms.DoGetDataSQL<TDETA_HES>("SELECT     TNUMBER, NAME  AS PERNAME, NUMBER FROM TDETA_HES WHERE (NUMBER = " + PSM1.ToString() + ") And (N_KOL =" + PSK1.ToString() + ") And (" + OpenArgs + ")  ORDER BY NAME + N' ' + RTRIM(CAST(TNUMBER AS NVARCHAR))").ToList();
            }
            else
            {
                RecordSource = dbms.DoGetDataSQL<TDETA_HES>("SELECT     TNUMBER2, NAME AS PERNAME FROM TDETA_HES2 WHERE (TNUMBER = " + PST1.ToString() + ") And (NUMBER = " + PSM1.ToString() + ") And (N_KOL =" + PSK1.ToString() + ")  And (" + OpenArgs + ")  ORDER BY NAME + N' ' + RTRIM(CAST(TNUMBER AS NVARCHAR))").ToList();
            }

            DG_PERSONELS.ItemsSource = RecordSource;
        }
        private void SetSelectedRow()
        {
            if (DG_PERSONELS.SelectedIndex > -1 && !(DG_PERSONELS.SelectedItem is null))
            {
                //ثبت و ویرایش مرخصی پرسنل
                if (TheWindow is PMORAKH)
                {
                    (TheWindow as PMORAKH).CODE.SelectedValue = null;
                    (TheWindow as PMORAKH).CODE.SelectedValue = (DG_PERSONELS.SelectedItem as TDETA_HES)?.TNUMBER;
                    (TheWindow as PMORAKH).CODE.Items.Refresh();
                }

                Close();
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key is Key.Enter)
            {
                SetSelectedRow();
            }
            else if (e.Key is Key.Escape)
            {
                Close();
            }
        }
        private void DG_PERSONELS_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            SetSelectedRow();
        }
    }
}
