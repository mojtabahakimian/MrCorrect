using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Stimulsoft.Blockly.Blocks.Maths;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class TAKHFIF : Window
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
        public TAKHFIF(Visual _the_win)
        {
            THEWIN = _the_win;
            InitializeComponent();

            if (Baseknow.TKHF == 2)
            {
                new Msgwin(false, "تخفيفات مصوب بصورت دستي قابل تغيير نيست").ShowDialog();
                Close();
            }

            bool _cango = true;
            if ((THEWIN as HEAD_LST_FROOSH22).AllowEdits is true)
            { }
            else
            {
                _cango = false;
                new Msgwin(false, "كليد اصلاح را بزنيد تا فاكتور قابل اصلاح باشد").ShowDialog();
            }

            if (CL_HESABDARI.LETSGO("TFTMLOCK"))
            {
                _cango = false;
                new Msgwin(false, "ستون تخفيفات قفل است و شما نمي توانيد تخفيف بزنيد").ShowDialog();
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) == "5")
            {
                _cango = false;
                new Msgwin(false, "باتوجه  به فعال بودن ستون تخفيفات نقدي نميتوانيد تخفيف کلي بزنيد").ShowDialog();
            }
            if (_cango == false)
            {
                Close();
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
        }
        private void OK_BTN_Click(object sender, RoutedEventArgs e)
        {
            bool IsFactorNonStraight = false;
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            double jamf;
            int i;
            double jammoin;
            double jammointkh;
            i = 0;
            jammoin = 0d;
            jammointkh = 0d;
            if (THEWIN is HEAD_LST_FROOSH22)
            {
                var H22 = (THEWIN as HEAD_LST_FROOSH22);
                var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + H22.NUMBER.Text + " ) AND ((INVO_LST.TAG)=2))").ToList();
                if (JST.Count > 0 && JST.FirstOrDefault() is not null)
                {
                    jamf = (double)JST.FirstOrDefault();
                    if (Strings.Right(Text0.Text, 1) == "%")
                    {
                        //this.Text0 = Round(jamf * Val(Strings.Left(this.Text0, Len(this.Text0) - 1)) / 100);
                        Text0.Text = Convert.ToString(Math.Round(jamf * Convert.ToDouble(Text0.Text.TrimEnd('%')) / 100));
                    }
                    (THEWIN as HEAD_LST_FROOSH22).TAKHFIF.Text = Text0.Text;
                    var rst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + H22.NUMBER.Text).ToList();
                    //while (!rst.EOF)
                    for (int y = 0; y < rst.Count; y++)
                    {
                        i = i + 1;
                        if (i == rst.Count)
                        {
                            rst[y].N_MOIN = Convert.ToDouble(Text0.Text) - jammoin;
                            if (rst[y].MABL_K != 0)
                            {
                                rst[y].N_KOL = (Convert.ToDouble(Text0.Text) - jammoin) / rst[y].MABL_K * 100;
                            }
                            else
                            {
                                rst[y].N_KOL = 0;
                            }
                        }
                        else
                        {
                            rst[y].N_MOIN = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text));
                            if (rst[y].MABL_K != 0)
                            {
                                rst[y].N_KOL = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text)) / rst[y].MABL_K * 100;
                            }
                            else
                            {
                                rst[y].N_KOL = 0;
                            }
                        }
                        jammoin = jammoin + Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text));
                        jammointkh = jammointkh + Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text)) + Math.Round((double)((rst[y].MABL_K - Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text))) * rst[y].TKHN / 100));
                        if ((bool)H22.TICMBAA.IsChecked)
                        {
                            var RST2_ONE = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF where code = '" + rst[y].CODE + "'").ToList();
                            if (RST2_ONE.Count > 0)
                            {
                                if ((bool)RST2_ONE.FirstOrDefault().CMBAA)
                                {
                                    if (rst[y].IMBAA != Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2_ONE.FirstOrDefault().CODE) / 100)))
                                    {
                                        rst[y].IMBAA = Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2_ONE.FirstOrDefault().CODE) / 100));
                                    }
                                }
                                else if (rst[y].IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();

                                    if (msgwin.DialogResult is true)
                                    {
                                        rst[y].IMBAA = 0;

                                    }
                                }
                            }
                        }
                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET N_KOL = {rst[y].N_KOL}, N_MOIN = {rst[y].N_MOIN}, IMBAA = {rst[y].IMBAA}
                                                 WHERE id = {rst[y].id}");
                        //rst.update();
                        //rst.MoveNext();
                    }
                }
                else
                {
                    jamf = 0d;
                }
                var rst2 = dbms.DoGetDataSQL<_QRE_4>("SELECT SUM(N_MOIN) AS MOIN,SUM(IMBAA)  AS MBAA  FROM INVO_LST  WHERE TAG=2 AND NUMBER  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text).ToList();
                var RST2 = dbms.DoGetDataSQL<_QRE_5>("SELECT TAKHFIF,MBAA FROM HEAD_LST  WHERE TAG=13 AND NUMBER  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text).ToList();
                if (RST2.Count == 1)
                {
                    if (rst2.FirstOrDefault().MOIN is null)
                    {
                        RST2.FirstOrDefault().TAKHFIF = 0;
                        RST2.FirstOrDefault().MBAA = 0;
                    }
                    else
                    {
                        RST2.FirstOrDefault().TAKHFIF = rst2.FirstOrDefault().MOIN;
                        RST2.FirstOrDefault().MBAA = rst2.FirstOrDefault().MBAA;
                    }
                    string _where = "  WHERE TAG=13 AND NUMBER  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text;
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET TAKHFIF = {RST2.FirstOrDefault().TAKHFIF} , MBAA = {RST2.FirstOrDefault().MBAA} {_where} ");
                    //RST2.update();
                }
              (THEWIN as HEAD_LST_FROOSH22).BUTTON_SAVE_HAVALE_Click(null, null);
                //Forms["head_lst_froosh22"]["INVO_LST_sub"].Requery();
            }
            else if (IsFactorNonStraight /*فاکتور فروش غیر مستقیم*/)
            {
                var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text + " ) AND ((INVO_LST.TAG)=2))").ToList();
                if (JST.Count > 0 && JST.FirstOrDefault() is not null)
                {
                    jamf = (double)JST.FirstOrDefault();
                    if (Strings.Right(Text0.Text, 1) == "%")
                    {
                        //Text0.Text = Round(jamf * Val(Strings.Left(Text0.Text, Len(Text0.Text) - 1)) / 100);
                        Text0.Text = Convert.ToString(Math.Round(jamf * Convert.ToDouble(Text0.Text.TrimEnd('%')) / 100));
                    }
                    (THEWIN as HEAD_LST_FROOSH22).TAKHFIF.Text = Text0.Text;
                    var rst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text).ToList();
                    for (int y = 0; y < rst.Count; y++) // while (!rst.EOF)
                    {
                        rst[y].N_MOIN = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text));
                        if (rst[y].MABL_K != 0)
                        {
                            rst[y].N_KOL = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text)) / rst[y].MABL_K * 100;
                        }
                        else
                        {
                            rst[y].N_KOL = 0;
                        }
                        if ((bool)(THEWIN as HEAD_LST_FROOSH22).TICMBAA.IsChecked)
                        {
                            var RST2 = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + rst[y].CODE + "'").ToList();
                            if (RST2.Count > 0)
                            {
                                if ((bool)RST2.FirstOrDefault().CMBAA)
                                {
                                    if (rst[y].IMBAA != Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100)))
                                    {
                                        rst[y].IMBAA = Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                    }
                                }
                                else if (rst[y].IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();
                                    if (msgwin.DialogResult is true)
                                    {
                                        rst[y].IMBAA = 0;
                                    }
                                }
                            }
                        }
                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET N_KOL = {rst[y].N_KOL}, N_MOIN = {rst[y].N_MOIN}, IMBAA = {rst[y].IMBAA}
                                                 WHERE id = {rst[y].id}");
                        //rst.update();
                        //rst.MoveNext();
                    }
                }
                else
                {
                    jamf = 0d;
                }
                var rst1 = dbms.DoGetDataSQL<_QRE_4>("SELECT SUM(N_MOIN) AS MOIN,SUM(IMBAA)  AS MBAA  FROM INVO_LST  WHERE TAG=2 AND NUMBER  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text).ToList();
                var RST2_TWO = dbms.DoGetDataSQL<_QRE_5>("select TAKHFIF,MBAA from head_lst  where tag=13 and number  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text).ToList();
                if (RST2_TWO.Count == 1)
                {
                    if (rst1.FirstOrDefault().MOIN is null)
                    {
                        RST2_TWO.FirstOrDefault().TAKHFIF = 0;
                        RST2_TWO.FirstOrDefault().MBAA = 0;
                    }
                    else
                    {
                        RST2_TWO.FirstOrDefault().TAKHFIF = rst1.FirstOrDefault().MOIN;
                        RST2_TWO.FirstOrDefault().MBAA = rst1.FirstOrDefault().MBAA;
                    }
                    var _where = "  where tag=13 and number  = " + (THEWIN as HEAD_LST_FROOSH22).NUMBER.Text;
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET TAKHFIF = {RST2_TWO.FirstOrDefault().TAKHFIF} , MBAA = {RST2_TWO.FirstOrDefault().MBAA} {_where} ");
                    //RST2_TWO.update();
                    //RST2_TWO.Close();
                }

                (THEWIN as HEAD_LST_FROOSH22).BUTTON_SAVE_HAVALE_Click(null, null);
                //Forms["HEAD_LST_FROOSH2"]["INVO_LST_sub"].Requery();
            }
            else if (THEWIN is HEAD_LST_PISHFROOSH2)
            {
                var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + (THEWIN as HEAD_LST_PISHFROOSH2).NUMBER.Text + " ) AND ((INVO_LST.TAG)=20))").ToList();
                if (JST.Count > 0 && JST.FirstOrDefault() is not null)
                {
                    jamf = (double)JST.FirstOrDefault();
                    if (Strings.Right(Text0.Text, 1) == "%")
                    {
                        //Text0.Text = Round(jamf * Val(Strings.Left(Text0.Text, Len(Text0.Text) - 1)) / 100);
                        Text0.Text = Convert.ToString(Math.Round(jamf * Convert.ToDouble(Text0.Text.TrimEnd('%')) / 100));
                    }
                    (THEWIN as HEAD_LST_PISHFROOSH2).TAKHFIF.Text = Text0.Text;
                    var rst = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE TAG = 20 AND NUMBER = " + (THEWIN as HEAD_LST_PISHFROOSH2).NUMBER.Text).ToList();
                    for (int y = 0; y < rst.Count; y++) // while (!rst.EOF)
                    {
                        rst[y].N_MOIN = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text));
                        if (rst[y].MABL_K != 0)
                        {
                            rst[y].N_KOL = Math.Round(rst[y].MABL_K / jamf * Convert.ToDouble(Text0.Text)) / rst[y].MABL_K * 100;
                        }
                        else
                        {
                            rst[y].N_KOL = 0;
                        }
                        if ((bool)(THEWIN as HEAD_LST_PISHFROOSH2).TICMBAA.IsChecked)
                        {
                            var RST2 = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + rst[y].CODE + "'").ToList();
                            if (RST2.Count > 0)
                            {
                                if ((bool)RST2.FirstOrDefault().CMBAA)
                                {
                                    if (rst[y].IMBAA != Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100)))
                                    {
                                        rst[y].IMBAA = Math.Floor((double)((rst[y].MABL_K - rst[y].N_MOIN) * CL_HESABDARI.GetArzesh(RST2.FirstOrDefault().CODE) / 100));
                                    }
                                }
                                else if (rst[y].IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();
                                    if (msgwin.DialogResult is true)
                                    {
                                        rst[y].IMBAA = 0;
                                    }
                                }
                            }
                        }
                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET N_KOL = {rst[y].N_KOL}, N_MOIN = {rst[y].N_MOIN}, IMBAA = {rst[y].IMBAA}
                                                 WHERE id = {rst[y].id}");
                        //rst.update();
                        //rst.MoveNext();
                    }
                }
                else
                {
                    jamf = 0d;
                }
                var rst_p = dbms.DoGetDataSQL<_QRE_4>("SELECT SUM(N_MOIN) AS MOIN,SUM(IMBAA)  AS MBAA  FROM INVO_LST  WHERE TAG=20 AND NUMBER  = " + (THEWIN as HEAD_LST_PISHFROOSH2).NUMBER.Text).ToList();
                var RST2__ = dbms.DoGetDataSQL<_QRE_5>("SELECT TAKHFIF,MBAA FROM HEAD_LST  WHERE TAG=20 AND NUMBER  = " + (THEWIN as HEAD_LST_PISHFROOSH2).NUMBER.Text).ToList();
                if (RST2__.Count == 1)
                {
                    if (rst_p.FirstOrDefault().MOIN is null)
                    {
                        RST2__.FirstOrDefault().TAKHFIF = 0;
                        RST2__.FirstOrDefault().MBAA = 0;
                    }
                    else
                    {
                        RST2__.FirstOrDefault().TAKHFIF = rst_p.FirstOrDefault().MOIN;
                        RST2__.FirstOrDefault().MBAA = rst_p.FirstOrDefault().MBAA;
                    }
                    var _where = "  WHERE TAG=20 AND NUMBER  = " + (THEWIN as HEAD_LST_PISHFROOSH2).NUMBER.Text;
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET TAKHFIF = {RST2__.FirstOrDefault().TAKHFIF} , MBAA = {RST2__.FirstOrDefault().MBAA} {_where} ");
                    //RST2__.update();
                    //RST2__.Close();
                }
                (THEWIN as HEAD_LST_PISHFROOSH2).INVO_LST_SUB_ReGetData();
            }
            this.Close();
        }
    }
}
