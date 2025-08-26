using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.OleDb;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    /// <summary>
    /// Interaction logic for KARTBANK.xaml
    /// </summary>
    public partial class KARTBANK : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
        }
        //Header Window End;
        #endregion
        public Visual THEWIN { get; set; }
        public KARTBANK(Visual _the_win)
        {
            THEWIN = _the_win;
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                OK_BTN_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void OK_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (THEWIN is HEAD_LST_FROOSH22)
            {
                if (string.IsNullOrEmpty(PORTP.SelectedValue?.ToString()))
                {
                    SerialPort serialPort = new SerialPort(PORTP.SelectedValue?.ToString(), 19200, Parity.None, 8, StopBits.One);
                    try
                    {
                        serialPort.Open();
                        string ST = "RQ075PR006000000AM010" + Text0.Text.PadLeft(10, '0') + "CU003364R1000R2000T1000T2000SV000SG000AD000PD0011";
                        byte[] BB = new byte[84];

                        BB[0] = 48;
                        BB[1] = 48;
                        BB[2] = 56;
                        BB[3] = 48;

                        for (int i = 0; i < ST.Length; i++)
                        {
                            BB[i + 4] = (byte)ST[i];
                        }

                        serialPort.Write(BB, 0, BB.Length);
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "پورت مورد نظر برای کارت خوان پیدا نشد").ShowDialog();
                    }
                    finally
                    {
                        serialPort?.Close();
                    }
                }
                CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
                var HLFW = (THEWIN as HEAD_LST_FROOSH22);

                var MOIN_VAR_ROW = dbms.DoGetDataSQL<string?>($"SELECT TOP 100 PERCENT MOIN_VAR FROM dbo.HEAD_LST WHERE (TAG = 13) AND (MABL_VAR > 0) AND (DEPATMAN = {HLFW.DEPATMAN.SelectedValue}) AND (NUMBER <> {HLFW.NUMBER.Text}) AND (Not (MOIN_VAR Is Null)) ORDER BY NUMBER DESC").FirstOrDefault();
                if (MOIN_VAR_ROW is not null)
                {
                    (THEWIN as HEAD_LST_FROOSH22).MOIN_VAR.Text = MOIN_VAR_ROW;
                }

                (THEWIN as HEAD_LST_FROOSH22).MABL_VAR.Text = Text0.Text;
                (THEWIN as HEAD_LST_FROOSH22).CUST_NO.Focus();
                CL_HESABDARI.APLAYTAKH(Convert.ToInt64(HLFW.NUMBER.Text), 2, Convert.ToDouble(HLFW.M_NAGHD.Text), Convert.ToDouble(HLFW.MABL_VAR.Text), Convert.ToDouble(HLFW.MABL_HAV.Text), (bool)HLFW.TICMBAA.IsChecked);
                HLFW.BUTTON_SAVE_HAVALE_Click(null, null);
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //On Open
            if ((THEWIN as HEAD_LST_FROOSH22).AllowEdits is true)
            {
                string incmd, batch, Q;
                string configFilePath = @"C:\POSCONF.CFG";
                if (File.Exists(configFilePath))
                {
                    using (StreamReader reader = new StreamReader(configFilePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            incmd = reader.ReadLine();
                            if (incmd == CL_HESABDARI.UCurrentUser()?.ToString()?.FixPersianChars())
                            {
                                incmd = reader.ReadLine();
                                PORTP.SelectedValue = incmd;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                new Msgwin(false, "كليد اصلاح را بزنيد تا فاكتور قابل اصلاح باشد").ShowDialog();
                Close();
            }

            Text0.Focus();
        }
    }
}
