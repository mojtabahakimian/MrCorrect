using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.SANATI;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinOther
{
    public partial class SERCHKAL : Window
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

        #region LOCALMODEL
        public class SLQ1
        {
            public string? CODE { get; set; }
            public double? mog { get; set; }
            public double? MEGF { get; set; }
            public double? megk { get; set; }
            public string? name { get; set; }
            public string? NAMES { get; set; }
            public int? VAHED { get; set; }
            public string? KK { get; set; }
            public double? ANBAR { get; set; }
            public long? VCOD { get; set; }
            public string? N_FANI { get; set; }
            public string? TOZIH { get; set; }
            public double? NESBAT { get; set; }
            public double? MANDF { get; set; }
            public double? B_SEF { get; set; }
            public double? MABL_F { get; set; }
            public double? MAX_M { get; set; }
            public double? VAZN { get; set; }
            public double? VAZNK { get; set; }
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public INVO_LST_FACTOR22 SELECTED_KALA { get; set; }
        public SERCHKAL(string Shart = "", string ANBARCODE = "", Visual _YOUR_VL_WIN = default, dynamic GiveMeCODE_NAME = null)
        {
            VL_WIN = _YOUR_VL_WIN;
            shart = Shart;
            anbars = ANBARCODE;
            if (_YOUR_VL_WIN != null)
            {
                nam = (_YOUR_VL_WIN as Window).GetType().Name;
            }
            if (!(GiveMeCODE_NAME is null))
            {
                DefaultSelectSearch = GiveMeCODE_NAME;
            }
            InitializeComponent();
        }
        public Visual VL_WIN { get; set; }
        string shart = "";
        string anbars = "";
        object nam = ""; //نام ویندوز فرم
        public static SERCHK serchk
        {
            get
            {
                return Application.Current.Windows
                    .OfType<SERCHK>()
                    .FirstOrDefault();
            }
        }
        private dynamic DefaultSelectSearch { get; set; }

        //string DefaultQuery = @"SELECT * FROM (SELECT     STUF_DEF.CODE, STUF_DEF.NAME + N' ' + ISNULL(STUF_DEF.N_FANI, N' ') AS NAME, STUF_DEF.MABL_F, STUF_STK.MOGODI_A + STUF_STK.MOGODI AS MOG, STUF_STK.ANBAR , STUF_DEF.vahed, STUF_DEF.N_FANI, STUF_DEF.TOZIH FROM STUF_DEF INNER JOIN   VAHEDS ON STUF_DEF.CODE = VAHEDS.CODE INNER JOIN STUF_STK ON STUF_DEF.CODE = STUF_STK.CODE GROUP BY STUF_DEF.CODE, STUF_DEF.NAME + N' ' + ISNULL(STUF_DEF.N_FANI, N' '), STUF_DEF.MABL_F, STUF_STK.MOGODI_A + STUF_STK.MOGODI, STUF_STK.ANBAR,STUF_DEF.VAHED, STUF_DEF.N_FANI, STUF_DEF.TOZIH) AS DRVD_TBL ";
        string DefaultQuery = @"";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            // ورودی‌های شما: string anbars, string shart
            int? anb = null;
            string extracondtion = string.Empty;
            if (int.TryParse(anbars, out var parsed))
            {
                anb = parsed; // در غیر این صورت null باقی می‌ماند
            }
            else
            {
                mOGColumn.Visibility = Visibility.Hidden; //موجودی
                extracondtion = " DISTINCT ";
            }

            // کوئری پارامترایز (فقط یک‌بار تعریف کن)
            var sql = $@"
                SELECT  {extracondtion}
                    COALESCE(m1.CODE, SF.CODE) AS CODE,
                    ROUND(ISNULL(ISNULL(m1.SMEGH, 0) - ISNULL(m2.SMEGH, 0), 0), 2) AS mog,
                    ISNULL(m2.SMEGH, 0) AS MEGF,
                    ISNULL(m1.SMEGH, 0) AS megk,
                    SD.NAME + N' ' + ISNULL(SD.TOZIH, N' ') + N' ' + ISNULL(SD.N_FANI, N' ') AS name,
                    TV.NAMES,
                    TV.CODE AS VAHED,
                    N' ' AS KK,
                    m1.ANBAR,
                    CAST(COALESCE(m1.CODE, SF.CODE) AS BIGINT) AS VCOD,
                    SD.N_FANI,
                    SD.TOZIH,
                    ISNULL(FN.FNESBAT, 1) AS NESBAT,
                    ROUND(
                        ISNULL(ISNULL(m1.SMEGH, 0) - ISNULL(m2.SMEGH, 0), 0) / ISNULL(FN.FNESBAT, 1),
                        0
                    ) AS MANDF,
                    SD.B_SEF,
                    SD.MABL_F,
                    SD.MAX_M,
                    SD.VAZN,
                    ROUND(ISNULL(ISNULL(m1.SMEGH, 0) - ISNULL(m2.SMEGH, 0), 0), 2) * SD.VAZN AS VAZNK
                FROM dbo.TCOD_VAHEDS AS TV
                INNER JOIN dbo.STUF_DEF AS SD
                    INNER JOIN dbo.STUF_FSK AS SF ON SD.CODE = SF.CODE
                    ON TV.CODE = SD.VAHED
                LEFT JOIN dbo.mogudi_2_2(@ANB) AS m2
                    ON SF.ANBAR = m2.ANBAR AND SF.CODE = m2.CODE
                LEFT JOIN dbo.mogudi_1_1(@ANB) AS m1
                    ON SF.ANBAR = m1.ANBAR AND SF.CODE = m1.CODE
                LEFT JOIN dbo.FNESBAT AS FN
                    ON SD.CODE = FN.CODE
                WHERE
                    (@ANB IS NULL OR m1.CODE IS NOT NULL)
                ";

            //   -- اگر انبار مشخص است، فقط ردیف‌هایی که در TVF موجودند را نگه داریم
            //--اگر انبار خالی است(@ANB IS NULL)، این شرط True می‌شود و همه‌ی اقلام می‌آیند

            var parameters = new
            {
                ANB = anb // دپر/Dapper خودش Null را هندل می‌کنـد
            };

            if (!string.IsNullOrWhiteSpace(shart))
            {
                // چون قبلاً WHERE داریم، همین را اضافه می‌کنیم:
                sql += " AND " + shart;
            }

            IEnumerable<SLQ1> result = dbms.DoGetDataSQL<SLQ1>(sql, parameters);

            serchkalDGR.ItemsSource = result;

            if (serchk != null)
            {
                serchk.shart = null;
                serchk.shart2 = null;
            }

            FocusOnFirstNAMECellRow();

            ProcLoader.Stop(Prc);
        }

        private int ANBARfunc()
        {
            //این کد باید باشه چون میخوایم فرمت با اکسس بهم نخوره ولی من به شکل دیگه ای انبار رو گرفتم , این کد اینجا مونده تا فرم های بعدی تکمیل بشه که گیج نشم
            int ANBARfuncRet = default;
            //if (this.OpenArgs == "F_MENU_ANBAR_FRKH")
            //{
            //    ANBARfuncRet = Forms["F_MENU_ANBAR_FRKH"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "F_MENU_KART")
            //{
            //    ANBARfuncRet = Forms["F_MENU_KART"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "F_MENU_ANBAR")
            //{
            //    ANBARfuncRet = Forms["F_MENU_ANBAR"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_MOSTAGHIM")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_MOSTAGHIM"]["INVO_LST_MO_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_RASID")
            //{
            //    ANBARfuncRet = Forms["head_lst_rasid"]["INVO_LST_RASID_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_HAVL")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_HAVL"]["INVO_LST_HAVL_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "OHEAD_LST_FROOSH")
            //{
            //    ANBARfuncRet = Forms["OHEAD_LST_FROOSH"]["OINVOLST_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_PISHFROOSH")
            //{
            //    ANBARfuncRet = Forms["head_lst_pishfroosh"]["INVO_LST_PISHFROOSH_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_PISHFROOSH2")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_PISHFROOSH2"]["INVO_LST_PISHFROOSH_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_FROOSH")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_FROOSH"]["INVO_LST_sub"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_KHADAMAT")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_KHADAMAT"]["INVO_LST_KHAD"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_KHAREED")
            //{
            //    ANBARfuncRet = Forms["head_lst_khareed"]["INVO_LST_KH_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HAVALAH_EXIT")
            //{
            //    ANBARfuncRet = Forms["HAVALAH_EXIT"]["HAVALA_EXIT_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HAVALAH_ENTER")
            //{
            //    ANBARfuncRet = Forms["havalah_enter"]["HAVALAH_ENTER_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_ENTEGHAL")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_ENTEGHAL"]["INVO_LST_ENTEGHAL_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_MANF")
            //{
            //    ANBARfuncRet = Forms["HEAD_MANF"]["DTL_MANF_SUB1"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HAVALAH_EXIT_SAYER")
            //{
            //    ANBARfuncRet = Forms["havalah_exit_sayer"]["HAVALAH_EXIT_SAYER_SUB"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_RASID_OTHER")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_RASID_OTHER"]["INVO_LST_RASID_SUB_OTHER"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_FROOSH22")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_FROOSH22"]["INVO_LST_sub"].Form["ANBAR"];
            //}
            //if (this.OpenArgs == "HEAD_LST_hav_other")
            //{
            //    ANBARfuncRet = Forms["HEAD_LST_HAV_other"]["INVO_LST_HAV_SUB_OTHER"].Form["ANBAR"];
            //}
            //var RST = new ADODB.Recordset();
            //if (this.OpenArgs == "ORDR_HED")
            //{
            //    RST.Open("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Forms["BASEKNOW"]["USERCOD"] + " ) ORDER BY dbo.OPANBACCESS.RDF", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    if (RST.RecordCount > 0)
            //    {
            //        ANBARfuncRet = RST.Fields(0);
            //    }
            //    else
            //    {
            //        ANBARfuncRet = 1;
            //    }
            //}
            //if (this.OpenArgs == "PM_ASSETS_FR")
            //{
            //    RST.Open("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Forms["BASEKNOW"]["USERCOD"] + " ) ORDER BY dbo.OPANBACCESS.RDF", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    if (RST.RecordCount > 0)
            //    {
            //        ANBARfuncRet = RST.Fields(0);
            //    }
            //    else
            //    {
            //        ANBARfuncRet = 1;
            //    }
            //}
            return ANBARfuncRet;
        }
        private void FocusOnFirstNAMECellRow()
        {
            try
            {
                //برای انتخاب سطر اول که وقتی توی پایین میان با کلید های جهتی نره ستون بعد بره ردیف بعدی
                if (serchkalDGR.Items.Count > 0)
                {
                    var LAST_ROW = 0;
                    var col_index = serchkalDGR.Columns.FirstOrDefault(c => c.SortMemberPath == "name").DisplayIndex;

                    serchkalDGR.ScrollIntoView(serchkalDGR.Items[LAST_ROW]);
                    serchkalDGR.SelectedIndex = LAST_ROW;
                    serchkalDGR.Focus();

                    // Use the Dispatcher to wait for the UI to render
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        DataGridRow row = (DataGridRow)serchkalDGR.ItemContainerGenerator.ContainerFromIndex(LAST_ROW);
                        if (row != null)
                        {
                            DataGridCell cell = CL_LMethods.GetCell(serchkalDGR, row, Convert.ToInt32(col_index));
                            if (cell != null)
                                cell.Focus();
                        }
                    }));
                }
            }
            catch { }

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (PublicVRB.IsSelected_SERCHKAL_item_ == true)
            {
                if (!(serchk is null))
                {
                    serchk.Close();
                }
            }
            else
            {
                PublicVRB.IsSelected_SERCHKAL_item_ = false;
            }
        }
        private void serchkalDGR_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && serchkalDGR.Items.Count > 0 && serchkalDGR.SelectedItem != null)
            {
                e.Handled = true;
                SelectAndCloseWindow();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void serchkalDGR_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (serchkalDGR.Items.Count > 0 && serchkalDGR.SelectedItem != null)
            {
                e.Handled = true;
                SelectAndCloseWindow();
            }
            else
            {
                if (DefaultSelectSearch != null)
                {
                    PublicVRB.Head_LstPish2.FROM_SEARCH_KAL.CODE = DefaultSelectSearch.CODEKALA;
                    PublicVRB.Head_LstPish2.FROM_SEARCH_KAL.NAME_CODE = DefaultSelectSearch.NAME;
                }
            }
        }
        private void SelectAndCloseWindow()
        {
            if (serchkalDGR.SelectedItem is SLQ1 selectedVal)
            {
                PublicVRB.SelectedResFromSearch = selectedVal.CODE;
                PublicVRB.IsSelected_SERCHKAL_item_ = true;
                string CALA = selectedVal.CODE.ToString();

                switch (nam)
                {
                    case "HEAD_LST_PISHFROOSH2":
                        (VL_WIN as HEAD_LST_PISHFROOSH2).FROM_SEARCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_PISHFROOSH2).FROM_SEARCH_KAL.NAME_CODE = selectedVal.name;
                        break;
                    case "HEAD_LST_HAVL":
                        (VL_WIN as HEAD_LST_HAVL).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_HAVL).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;
                    case "HEAD_LST_RASID":
                        (VL_WIN as HEAD_LST_RASID).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_RASID).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;
                    case "HEAD_LST_FROOSH22":
                        (VL_WIN as HEAD_LST_FROOSH22).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_FROOSH22).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    case "HEAD_LST_ENTEGHAL_WIN":
                        (VL_WIN as HEAD_LST_ENTEGHAL_WIN).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_ENTEGHAL_WIN).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    case "HEAD_LST_KHAREED1":
                        (VL_WIN as HEAD_LST_KHAREED1).FROM_SEARCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_KHAREED1).FROM_SEARCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    case "HEAD_LST_KHADAMAT":
                        (VL_WIN as HEAD_LST_KHADAMAT).FROM_SEARCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_KHADAMAT).FROM_SEARCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    case "HEAD_LST_HAV_OTHER_WIN":
                        (VL_WIN as HEAD_LST_HAV_OTHER_WIN).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_HAV_OTHER_WIN).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;
                    case "HEAD_LST_REQUEST_WIN":
                        (VL_WIN as HEAD_LST_REQUEST_WIN).FROM_SAERCH_KAL.CODE = CALA;
                        (VL_WIN as HEAD_LST_REQUEST_WIN).FROM_SAERCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    case "I_AM_VK_SAKHTEH":
                        (VL_WIN as HAVALAH_ENTER).FROM_SEARCH_KAL.CODE = CALA;
                        (VL_WIN as HAVALAH_ENTER).FROM_SEARCH_KAL.NAME_CODE = selectedVal.name;
                        break;

                    default:
                        if (!string.IsNullOrEmpty(CALA))
                        {
                            SELECTED_KALA = new INVO_LST_FACTOR22 { CODE = CALA, NAME_CODE = selectedVal.name };
                        }
                        break;
                }
            }
            Close();
        }

    }
}
