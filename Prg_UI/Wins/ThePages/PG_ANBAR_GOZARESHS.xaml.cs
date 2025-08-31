using Functions;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.ANBAR.ANBAR_REPORTS;

namespace Wins.ThePages
{
    public partial class PG_ANBAR_GOZARESHS : Page
    {
        public PG_ANBAR_GOZARESHS()
        {
            InitializeComponent();
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //جستجو گر موجودی
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGUDI_SEARCH_MAIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //کارت انبار کالا
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, null /*DEFAULT OWNER MAIN*/, "R");

            //کارت انبار کالا
            //new F_MENU_KART("R").Show();
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            //لیست موجودی کالا ها به تفکیک انبار
            //new F_MENU_ANBAR("F").Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR, null, "F");
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST, null);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            //لیست موجودی کالای انبار های تلفیق شده
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_MERG, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            //کارت انبار کالا
            //new F_MENU_KART("KARTR2").Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, null /*DEFAULT OWNER MAIN*/, "KARTR2");
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            //گزارش حواله انبار گروهی
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_GRP, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewKeyDown_2(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST, null);
        }

        private void Image_PreviewKeyDown_3(object sender, KeyEventArgs e)
        {

        }

        private void Image_PreviewKeyDown_4(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_MERG, null);
        }

        private void Image_PreviewKeyDown_5(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, null, "KARTR2");
        }

        private void Image_PreviewKeyDown_6(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_GRP, null);
        }

        private void Image_PreviewKeyDown_7(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.C_TARAZ_ANBAR_KHAS, null);
        }

        private void Image_PreviewKeyDown_8(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_F, null);
        }

        private void Image_PreviewKeyDown_9(object sender, KeyEventArgs e)
        {
        }

        private void Image_PreviewKeyDown_10(object sender, KeyEventArgs e)
        {
        }

        private void Image_PreviewKeyDown_11(object sender, KeyEventArgs e)
        {
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_MOGUDI_ANBARHA.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            //report.Render();
            //report.Show();
            new Rpts.WINRPT(report, "گزارش موجودی کل انبار ها").Show();
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGUDI_ANBARHA_LIST, null);
        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_R, null, "R");
        }

        private void Image_PreviewMouseDown_9(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_PreviewMouseDown_10(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TARAZ_ANBAR_KOL, null);
        }

        private void Image_PreviewMouseDown_11(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_F, null);
        }

        private void Image_PreviewMouseDown_12(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_R, null);
        }

        private void Image_PreviewMouseDown_13(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_TANBGRP, null);
        }

        private void Image_PreviewMouseDown_14(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_FD, null);
        }

        private void Image_PreviewMouseDown_15(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.R_TARAZ_ANBARHA, null);
        }
    }
}
