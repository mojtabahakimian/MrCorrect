using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Wins.WinSetting;

namespace Prg_UI.Wins.WinSetting
{
    public partial class lockok : Window
    {
        #region COMMENT
        //private void TLCK()
        //{
        //    AxTINYLib.AxTiny axTiny1 = new AxTINYLib.AxTiny();
        //    axTiny1.CreateControl();

        //    axTiny1.ServerIP = SERVERNAM.Text;
        //    axTiny1.Enabled = true;

        //    axTiny1.Initialize = true;

        //    if (axTiny1.TinyErrCode == 0)
        //    {
        //        axTiny1.UserPassWord = RWK;
        //        axTiny1.ShowTinyInfo = true;

        //        if (axTiny1.TinyErrCode == 0)
        //        {
        //            SpecialIDNumber.Text = axTiny1.SpecialID;
        //            SerialNumber.Text = axTiny1.SerialNumber;
        //            DataPartion.Text = axTiny1.DataPartition;
        //            MaxNTUser.Text = axTiny1.NTUserMax.ToString();
        //            NTUser.Text = axTiny1.NTUser.ToString();
        //            Timer.Text = axTiny1.Timer.ToString();
        //            Counter.Text = axTiny1.Counter.ToString();
        //        }
        //        else
        //        {
        //            lblstatus.Content = ReasonError(axTiny1.TinyErrCode.ToString());
        //        }
        //    }
        //    else
        //    {
        //        lblstatus.Content = ReasonError(axTiny1.TinyErrCode.ToString());
        //    }
        //}
        #endregion

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

        //public AxTINYLib.AxTiny axTiny1;
        public lockok()
        {
            InitializeComponent();
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        CL_LOCKWATCH Lockwatch = new CL_LOCKWATCH();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var RST = dbms.DoGetDataSQL<string>("SELECT SERVERNAM FROM SAZMAN").FirstOrDefault();
            SERVERNAM.Text = RST;

            //AxTINYLib.AxTiny axTiny1 = new AxTINYLib.AxTiny();
            //axTiny1.CreateControl();
            //axTiny1.ServerIP = SERVERNAM.Text;
            //axTiny1.Enabled = true;
            //axTiny1.Initialize = true;

            //if (axTiny1.TinyErrCode != 0)
            //{
            //    Msgwin msgwin = new Msgwin(false, CL_LMethods.LockReasonError(axTiny1.TinyErrCode.ToString())); msgwin.ShowDialog();
            //    Close();
            //}
        }
        private void Command3_Click(object sender, RoutedEventArgs e)
        {
            dbms.DoExecuteSQL("UPDATE dbo.SAZMAN SET SERVERNAM = '" + this.SERVERNAM.Text + "'");
            var RST = dbms.DoGetDataSQL<int?>("SELECT COUNT(N_S) AS CN_S FROM DEED_HED").FirstOrDefault();
            //var RST = dbms.DoGetDataSQL<long?>("SELECT COUNT(N_S) AS CN_S FROM DEED_HED WITH (INDEX ([N_SI]))").FirstOrDefault(); ////this is faster
            if (RST > 31)
            {
                AxTINYLib.AxTiny axTiny1 = new AxTINYLib.AxTiny();
                axTiny1.CreateControl();
                axTiny1.ServerIP = SERVERNAM.Text;
                axTiny1.Enabled = true;
                axTiny1.Initialize = true;

                if (axTiny1.TinyErrCode != 0)
                {
                    Msgwin msgwin = new Msgwin(false, CL_LMethods.LockReasonError(axTiny1.TinyErrCode.ToString())); msgwin.ShowDialog();
                    Close();
                }
                else
                {
                    foreach (var password in Lockwatch.TheKeys)
                    {
                        if (Lockwatch.TryMatchValidLock(axTiny1, password))
                            break;
                    }

                    if (axTiny1.TinyErrCode != 0)
                    {
                        Msgwin msgwin = new Msgwin(false, CL_LMethods.LockReasonError(axTiny1.TinyErrCode.ToString())); msgwin.ShowDialog();
                        Close();
                        CL_LMethods.GoExitTheApplication();
                    }

                }
            }
            this.Close();
            CL_LMethods.GoExitTheApplication();
        }
   
    }
}
