using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_KOL_MOIN_DATE_AI.xaml
    /// </summary>
    public partial class F_MENU_KOL_MOIN_DATE_AI : Window
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

        public F_MENU_KOL_MOIN_DATE_AI()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public class Q1
        {
            public string? hes { get; set; }
        }

        public class Q2
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }
        }
        public class VISIT_MODEL0
        {
            public string? ROUTE_NAME { get; set; }
            public string? Expr1 { get; set; }
            public string? hes { get; set; }
            public override string ToString() => ROUTE_NAME;
        }
        public class Q3
        {
            public double? N_S { get; set; }
            public double? HES_T { get; set; }
            public double? HES_M { get; set; }
            public int? DATE_S { get; set; }
            public double? HES_K { get; set; }
            public string? SHARH { get; set; }
            public double? BED { get; set; }
            public double? BES { get; set; }
            public double? MAND { get; set; }
            public short? TS { get; set; }
            public string? HES { get; set; }
            public int? DIFF { get; set; }
            public int? ID { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }

        public class Q4
        {
            public int? HES_M { get; set; }
            public int? HES_T { get; set; }
            public long? DATE_S { get; set; }
            public double? N_S { get; set; }
            public double? BED { get; set; }
            public double? BES { get; set; }
            public string? HES { get; set; }
            public string? SHARH { get; set; }
            public double? MAND { get; set; }
            public int? HES_K { get; set; }
        }

        public class Q5
        {
            public double? SBES { get; set; }
        }

        UniversControl universControl = new UniversControl();

        public Visual I_AM_KOL_MOIN_AI { get; set; }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                //
                string Formname = "HESMAI";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

            I_AM_KOL_MOIN_AI = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOXES();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Command5.IsFocused)
                {
                    //Enter Key Continue
                }
                else
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        public void FILL_ALL_COMBOBOXES()
        {
            Combo34.ItemsSource = new List<Custom_CUST_HESAB>();
            Combo34.DisplayMemberPath = "NAME";
            Combo34.SelectedValuePath = "hes";

            Combo36.ItemsSource = Combo34.ItemsSource;
            Combo36.DisplayMemberPath = "hes";
            Combo36.SelectedValuePath = "hes";



            Combo39.ItemsSource = new List<Custom_CUST_HESAB>();
            Combo39.DisplayMemberPath = "NAME";
            Combo39.SelectedValuePath = "hes";

            Combo41.ItemsSource = Combo39.ItemsSource;
            Combo41.DisplayMemberPath = "hes";
            Combo41.SelectedValuePath = "hes";

            visit.ItemsSource = dbms.DoGetDataSQL<VISIT_MODEL0>("SELECT Visit_route.ROUTE_NAME,CUST_HESAB.hes, Visit_route.ROUTE_NAME + N' - ' + CUST_HESAB.NAME + N' - ' + CUST_HESAB.hes AS Expr1 FROM Visit_route INNER JOIN CUST_HESAB ON Visit_route.HES = CUST_HESAB.hes WHERE (Visit_route.RACTIVE = 1)").ToList();

            //Combo34.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT RTRIM(CAST(TDETA_HES.N_KOL AS nvarchar)) + '-' + RTRIM(CAST(TDETA_HES.NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes FROM TOTA_HES INNER JOIN DETA_HES INNER JOIN TDETA_HES ON DETA_HES.NUMBER = TDETA_HES.NUMBER AND DETA_HES.N_KOL = TDETA_HES.N_KOL ON TOTA_HES.NUMBER = DETA_HES.N_KOL").ToList();

            //Combo36.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT RTRIM(CAST(TDETA_HES.N_KOL AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.NUMBER AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes, TDETA_HES.NAME FROM TOTA_HES INNER JOIN DETA_HES INNER JOIN TDETA_HES ON DETA_HES.NUMBER = TDETA_HES.NUMBER AND DETA_HES.N_KOL = TDETA_HES.N_KOL ON TOTA_HES.NUMBER = DETA_HES.N_KOL").ToList();
            //Combo36.SelectedValuePath = "hes";
            //Combo36.DisplayMemberPath = "NAME";

            //Combo39.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT RTRIM(CAST(TDETA_HES.N_KOL AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.NUMBER AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes FROM TOTA_HES INNER JOIN DETA_HES INNER JOIN TDETA_HES ON DETA_HES.NUMBER = TDETA_HES.NUMBER AND DETA_HES.N_KOL = TDETA_HES.N_KOL ON TOTA_HES.NUMBER = DETA_HES.N_KOL ORDER BY RTRIM(CAST(TDETA_HES.N_KOL AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.NUMBER AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar))").ToList();
            //Combo39.SelectedValuePath = "hes";
            //Combo39.DisplayMemberPath = "hes";

            //Combo41.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT RTRIM(CAST(TDETA_HES.N_KOL AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.NUMBER AS nvarchar)) + ''-'' + RTRIM(CAST(TDETA_HES.TNUMBER AS nvarchar)) AS hes, TDETA_HES.NAME FROM TOTA_HES INNER JOIN DETA_HES INNER JOIN TDETA_HES ON DETA_HES.NUMBER = TDETA_HES.NUMBER AND DETA_HES.N_KOL = TDETA_HES.N_KOL ON TOTA_HES.NUMBER = DETA_HES.N_KOL").ToList();
            //Combo41.SelectedValuePath = "hes";
            //Combo41.DisplayMemberPath = "NAME";
        }

        private void Combo34_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo34.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (Combo34.SelectedValue is not null)
            {
                Combo36.SelectedValue = Combo34.SelectedValue;
            }

            TextBox CUTSNO_TEX = (TextBox)Combo34.Template.FindName("PART_EditableTextBox", Combo34);

            if (Combo34.SelectedValue is not null)
            {
                if ((Combo34.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(Combo34, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"از حساب نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }
        }

        private void Combo36_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo36.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر


            if (Combo36.SelectedValue is not null)
            {
                Combo34.SelectedValue = Combo36.SelectedValue;
            }
        }

        private void Combo39_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo39.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox CUTSNO_TEX = (TextBox)Combo39.Template.FindName("PART_EditableTextBox", Combo39);

            if (Combo39.SelectedValue is not null)
            {
                if ((Combo39.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(Combo39, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"از حساب نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }
        }

        private void Combo41_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo41.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (Combo41.SelectedValue is not null)
            {
                Combo39.SelectedValue = Combo41.SelectedValue;
            }
        }

        public class AI_DAFTAR
        {
            public double? N_S { get; set; }
            public string HES_T { get; set; }
            public string HES_M { get; set; }
            public long? DATE_S { get; set; }
            public string HES_K { get; set; }
            public string SHARH { get; set; }
            public double? BED { get; set; }
            public double? BES { get; set; }
            public string HES { get; set; }
            public double? MAND { get; set; }
            public int? TS { get; set; }
            public int? DIFF { get; set; }
        }

        public class AI
        {
            public double? N_S { get; set; }
            public string HES_T { get; set; }
            public string HES_M { get; set; }
            public long? DATE_S { get; set; }
            public string HES_K { get; set; }
            public string SHARH { get; set; }
            public double? BED { get; set; }
            public double? BES { get; set; }
            public string HES { get; set; }
            public double? MAND { get; set; }
        }

        private void Commnd5_Click000(object sender, RoutedEventArgs e)
        {
            //string sql;
            //double MAND;
            //var SBES = default(double);
            //string HESM;
            //bool CHK;
            //string PATH;
            ////int i;

            //if ((IsNull(Combo34.SelectedValue) || IsNull(Combo39.SelectedValue)) && IsNull(this.visit.SelectedValue))
            //{
            //    universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1); 
            //    return;
            //}
            //MAND = 0d;
            //HESM = (-1).ToString();
            //CHK = false;

            //if (this.RB1.IsChecked == true)
            //{
            //    dbms.DoExecuteSQL("DELETE FROM dbo.AI_DAFTAR");

            //    var RST2 = dbms.DoGetDataSQL<Q3>("SELECT * FROM AI_DAFTAR").ToList();

            //    List<Q3> rst = null;

            //    if (IsNull(this.visit.SelectedValue))
            //    {
            //        rst = dbms.DoGetDataSQL<Q3>("SELECT AI.* FROM dbo.AI( '" + this.Combo34.SelectedValue + "','" + this.Combo39.SelectedValue + "') AI").ToList();
            //    }
            //    else
            //    {
            //        rst = dbms.DoGetDataSQL<Q3>("SELECT AI2.* FROM dbo.AI2( N'" + this.visit.SelectedValue + "') AI2").ToList();
            //    }


            //    for (int i = 0; i < rst.Count; i++) // while (!rst.EOF)
            //    {
            //        if (rst[i].HES != HESM)
            //        {
            //            MAND = 0d;
            //            var rst3 = dbms.DoGetDataSQL<Q5>("SELECT SUM(BES) AS sbes FROM dbo.DEED_DTL WHERE     (HES = N'" + rst[i].HES + "')").ToList();
            //            SBES = Convert.ToDouble(rst3.FirstOrDefault().SBES);
            //            HESM = rst[i].HES;
            //        }

            //        SBES = SBES - Convert.ToDouble(rst[i].BED);
            //        if (SBES <= 0d)
            //        {
            //            //RST2.AddNew();

            //            RST2.FirstOrDefault().N_S = rst[i].N_S;
            //            RST2.FirstOrDefault().HES_T = rst[i].HES_T;
            //            RST2.FirstOrDefault().HES_M = rst[i].HES_M;
            //            RST2.FirstOrDefault().DATE_S = rst[i].DATE_S;
            //            RST2.FirstOrDefault().HES_K = rst[i].HES_K;
            //            RST2.FirstOrDefault().SHARH = Strings.Right("مانده تا اين تاريخ - " + rst[i].SHARH, 255);
            //            RST2.FirstOrDefault().BED = Math.Abs(SBES);
            //            RST2.FirstOrDefault().BES = rst[i].BES;
            //            RST2.FirstOrDefault().HES = rst[i].HES;
            //            MAND = Math.Abs(SBES) + MAND;
            //            RST2.FirstOrDefault().MAND = Math.Round(Math.Abs(MAND));
            //            RST2.FirstOrDefault().TS = Interaction.IIf(MAND >= 0d, 1, -1);
            //            RST2.FirstOrDefault().DIFF = DIFF(rst[i].DATE_S, CL_HESABDARI.FARSIDATE());

            //            //RST2.update();
            //            //rst.MoveNext();

            //            while (!rst.EOF & rst[i].("HES") == HESM)
            //            {
            //                if (rst[i].("BES") == 0)
            //                {
            //                    RST2.AddNew();
            //                    RST2.Fields("N_S") = rst[i].("N_S");
            //                    RST2.Fields("HES_T") = rst[i].("HES_T");
            //                    RST2.Fields("HES_M") = rst[i].("HES_M");
            //                    RST2.Fields("DATE_S") = rst[i].("DATE_S");
            //                    RST2.Fields("HES_K") = rst[i].("HES_K");
            //                    RST2.Fields("SHARH") = rst[i].("SHARH");
            //                    RST2.Fields("BED") = rst[i].("BEd");
            //                    RST2.Fields("BES") = rst[i].("BES");
            //                    RST2.Fields("HES") = rst[i].("HES");
            //                    MAND = rst[i].("BEd") + MAND;
            //                    RST2.Fields("MAND") = Round(Abs[MAND]);
            //                    RST2.Fields("TS") = Interaction.IIf(MAND >= 0d, 1, -1);
            //                    RST2.Fields("DIFF") = DIFF(rst[i].("DATE_S"), FARSIDATE<DateTime>);
            //                    RST2.update();
            //                }
            //                rst.MoveNext();
            //                if (rst.EOF)
            //                {
            //                    goto b1;
            //                }
            //            }

            //        b1:
            //            ;
            //            rst.MovePrevious();
            //        }
            //        rst.MoveNext();
            //    }
            //    DoCmd.RunSQL("DELETE FROM dbo.AI_DAFTAR WHERE     (BED = 0) AND (BES = 0)");
            //    DoCmd.OpenReport("R_DAFTAR_AI", acViewPreview);
            //    DoCmd.OpenForm(this.NAME, default, default, default, default, acHidden);
            //}
            //else
            //{
            //    RST2.Open("AI_DAFTARMOIN", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    DoCmd.RunSQL("DELETE FROM dbo.AI_DAFTARMOIN");
            //    if (IsNull(this.visit))
            //    {
            //        RST2.Open("AI_DAFTAR", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    }
            //    else
            //    {
            //        rst.Open("SELECT AI2.* FROM dbo.AI2( ,N'" + this.visit + "') AI2", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    }
            //    while (!rst.EOF)
            //    {
            //        if (rst.Fields("HES") != HESM)
            //        {
            //            MAND = 0d;
            //            HESM = rst.Fields("HES");
            //        }
            //        RST2.AddNew();
            //        RST2.Fields("N_S") = rst.Fields("N_S");
            //        RST2.Fields("HES_T") = rst.Fields("HES_T");
            //        RST2.Fields("HES_M") = rst.Fields("HES_M");
            //        RST2.Fields("DATE_S") = rst.Fields("DATE_S");
            //        RST2.Fields("HES_K") = rst.Fields("HES_K");
            //        RST2.Fields("SHARH") = rst.Fields("SHARH");
            //        RST2.Fields("BED") = rst.Fields("BED");
            //        RST2.Fields("BES") = rst.Fields("BES");
            //        RST2.Fields("HES") = rst.Fields("HES");
            //        MAND = rst.Fields("MAND") + MAND;
            //        RST2.Fields("MAND") = Round(Abs[MAND]);
            //        RST2.Fields("TS") = Interaction.IIf(MAND >= 0d, 1, -1);
            //        RST2.update();
            //        rst.MoveNext();
            //    }
            //    RST2.Open("AI_DAFTAR", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    rst.Open("AI_DAFTARMOIN", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    DoCmd.RunSQL("DELETE FROM dbo.AI_DAFTAR");
            //    rst.MoveLast();
            //    MAND = 0d;
            //    HESM = (-1).ToString();
            //    while (!rst.BOF)
            //    {
            //        if (rst.Fields("HES") != HESM)
            //        {
            //            MAND = 0d;
            //            HESM = rst.Fields("HES");
            //            CHK = false;
            //        }
            //        if (Round(rst.Fields("MAND") * rst.Fields("TS") * -1) >= Round(rst.Fields("BES")) & rst.Fields("MAND") != 0)
            //        {
            //            RST2.AddNew();
            //            RST2.Fields("N_S") = rst.Fields("N_S");
            //            RST2.Fields("HES_T") = rst.Fields("HES_T");
            //            RST2.Fields("HES_M") = rst.Fields("HES_M");
            //            RST2.Fields("DIFF") = DIFF(rst.Fields("DATE_S"), FARSIDATE<DateTime>);
            //            RST2.Fields("DATE_S") = rst.Fields("DATE_S");
            //            RST2.Fields("HES_K") = rst.Fields("HES_K");
            //            RST2.Fields("SHARH") = rst.Fields("SHARH");
            //            RST2.Fields("BED") = rst.Fields("BED");
            //            RST2.Fields("BES") = rst.Fields("BES");
            //            RST2.Fields("HES") = rst.Fields("HES");
            //            RST2.Fields("MAND") = Round(rst.Fields("MAND"));
            //            RST2.Fields("TS") = rst.Fields("TS");
            //            RST2.update();
            //            CHK = true;
            //        }
            //        else
            //        {
            //            if (CHK)
            //            {
            //                RST2.AddNew();
            //                RST2.Fields("N_S") = rst.Fields("N_S");
            //                RST2.Fields("HES_T") = rst.Fields("HES_T");
            //                RST2.Fields("DIFF") = DIFF(rst.Fields("DATE_S"), FARSIDATE<DateTime>);
            //                RST2.Fields("HES") = rst.Fields("HES");
            //                RST2.Fields("HES_M") = rst.Fields("HES_M");
            //                RST2.Fields("DATE_S") = rst.Fields("DATE_S");
            //                RST2.Fields("HES_K") = rst.Fields("HES_K");
            //                RST2.Fields("SHARH") = "مانده تا اين تاريخ";
            //                if (rst.Fields("TS") == 1)
            //                {
            //                    RST2.Fields("BED") = Round(rst.Fields("MAND"));
            //                    RST2.Fields("BES") = 0;
            //                }
            //                else
            //                {
            //                    RST2.Fields("BEs") = Round(rst.Fields("MAND"));
            //                    RST2.Fields("BEd") = 0;
            //                }
            //                RST2.Fields("MAND") = Round(rst.Fields("MAND"));
            //                RST2.Fields("TS") = rst.Fields("TS");
            //                RST2.update();
            //                CHK = false;
            //            }
            //            while (rst.Fields("HES") == HESM)
            //            {
            //                rst.MovePrevious();
            //                if (rst.BOF)
            //                {
            //                    goto A5;
            //                }
            //            }
            //            rst.MoveNext();
            //        }
            //        rst.MovePrevious();
            //    A5:
            //        ;
            //    }
            //    RST2.Close();
            //    rst.Close();
            //    DoCmd.OpenReport("R_DAFTAR_AIS", acViewPreview);
            //    DoCmd.OpenForm(this.NAME, default, default, default, default, acHidden);
            //}

        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            double MAND = 0;
            double SBES = 0;
            string HESM = "-1";
            bool CHK = false;

            var combo34Value = Combo34.SelectedValue?.ToString();
            var combo39Value = Combo39.SelectedValue?.ToString();

            if ((IsNull(Combo34.SelectedValue) || IsNull(Combo39.SelectedValue)) || IsNull(this.visit.SelectedValue))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (RB1.IsChecked == true) // بدهکار
            {
                dbms.DoExecuteSQL("DELETE FROM dbo.AI_DAFTAR");

                List<AI> rstData;
                if (string.IsNullOrEmpty(visit.SelectedValue.ToStringNullSafe()))
                {
                    rstData = dbms.DoGetDataSQL<AI>($"SELECT AI.* FROM dbo.AI('{combo34Value}', '{combo39Value}') AI").ToList();
                }
                else
                {
                    rstData = dbms.DoGetDataSQL<AI>($"SELECT AI2.* FROM dbo.AI2(N'{visit.SelectedValue}') AI2").ToList();
                }

                int index = 0;
                while (index < rstData.Count)
                {
                    var rst = rstData[index];
                    if (rst.HES != HESM)
                    {
                        MAND = 0;
                        SBES = dbms.DoGetDataSQL<double?>($"SELECT SUM(BES) AS sbes FROM dbo.DEED_DTL WHERE (HES = N'{rst.HES}')").FirstOrDefault() ?? 0;
                        HESM = rst.HES;
                    }

                    SBES -= rst.BED ?? 0;
                    if (SBES <= 0)
                    {
                        var rst2 = new AI_DAFTAR
                        {
                            N_S = rst.N_S,
                            HES_T = rst.HES_T,
                            HES_M = rst.HES_M,
                            DATE_S = rst.DATE_S,
                            HES_K = rst.HES_K,
                            SHARH = ("مانده تا اين تاريخ - " + rst.SHARH).Substring(0, Math.Min(255, ("مانده تا اين تاريخ - " + rst.SHARH).Length)),
                            BED = Math.Abs(SBES),
                            BES = rst.BES,
                            HES = rst.HES
                        };
                        MAND = Math.Abs(SBES) + MAND;
                        rst2.MAND = Math.Round(Math.Abs(MAND));
                        rst2.TS = MAND >= 0 ? 1 : -1;
                        rst2.DIFF = (int?)CL_HESABDARI.DIFF((long)rst.DATE_S, CL_HESABDARI.FARSIDATE()); // Replace with actual implementation

                        dbms.DoExecuteSQL($"INSERT INTO dbo.AI_DAFTAR (N_S, HES_T, HES_M, DATE_S, HES_K, SHARH, BED, BES, HES, MAND, TS, DIFF) " +
                                          $"VALUES ({rst2.N_S}, '{rst2.HES_T}', '{rst2.HES_M}', {rst2.DATE_S}, '{rst2.HES_K}', '{rst2.SHARH}', {rst2.BED}, {rst2.BES}, '{rst2.HES}', {rst2.MAND}, {rst2.TS}, {rst2.DIFF})");

                        index++;
                        while (index < rstData.Count && rstData[index].HES == HESM)
                        {
                            rst = rstData[index];
                            if (rst.BES == 0)
                            {
                                rst2 = new AI_DAFTAR
                                {
                                    N_S = rst.N_S,
                                    HES_T = rst.HES_T,
                                    HES_M = rst.HES_M,
                                    DATE_S = rst.DATE_S,
                                    HES_K = rst.HES_K,
                                    SHARH = rst.SHARH,
                                    BED = rst.BED,
                                    BES = rst.BES,
                                    HES = rst.HES
                                };
                                MAND = (rst.BED ?? 0) + MAND;
                                rst2.MAND = Math.Round(Math.Abs(MAND));
                                rst2.TS = MAND >= 0 ? 1 : -1;
                                rst2.DIFF = (int?)CL_HESABDARI.DIFF((long)rst.DATE_S, CL_HESABDARI.FARSIDATE());

                                dbms.DoExecuteSQL($"INSERT INTO dbo.AI_DAFTAR (N_S, HES_T, HES_M, DATE_S, HES_K, SHARH, BED, BES, HES, MAND, TS, DIFF) " +
                                                  $"VALUES ({rst2.N_S}, '{rst2.HES_T}', '{rst2.HES_M}', {rst2.DATE_S}, '{rst2.HES_K}', '{rst2.SHARH}', {rst2.BED}, {rst2.BES}, '{rst2.HES}', {rst2.MAND}, {rst2.TS}, {rst2.DIFF})");
                            }
                            index++;
                        }
                        if (index < rstData.Count) index--; // MovePrevious equivalent
                    }
                    index++;
                }

                dbms.DoExecuteSQL("DELETE FROM dbo.AI_DAFTAR WHERE (BED = 0) AND (BES = 0)");

                OpenReportAI();
            }
            else
            {
                dbms.DoExecuteSQL("DELETE FROM dbo.AI_DAFTARMOIN");
                List<AI> rstData;
                if (string.IsNullOrEmpty(visit.SelectedValue.ToStringNullSafe()))
                {
                    rstData = dbms.DoGetDataSQL<AI>($"SELECT AI.* FROM dbo.AI('{combo34Value}', '{combo39Value}') AI").ToList();
                }
                else
                {
                    rstData = dbms.DoGetDataSQL<AI>($"SELECT AI2.* FROM dbo.AI2(N'{visit.SelectedValue}') AI2").ToList();
                }

                foreach (var rst in rstData)
                {
                    if (rst.HES != HESM)
                    {
                        MAND = 0;
                        HESM = rst.HES;
                    }

                    var rst2 = new AI_DAFTAR
                    {
                        N_S = rst.N_S,
                        HES_T = rst.HES_T,
                        HES_M = rst.HES_M,
                        DATE_S = rst.DATE_S,
                        HES_K = rst.HES_K,
                        SHARH = rst.SHARH,
                        BED = rst.BED,
                        BES = rst.BES,
                        HES = rst.HES
                    };
                    MAND = (rst.MAND ?? 0) + MAND;
                    rst2.MAND = Math.Round(Math.Abs(MAND));
                    rst2.TS = MAND >= 0 ? 1 : -1;

                    dbms.DoExecuteSQL($"INSERT INTO dbo.AI_DAFTARMOIN (N_S, HES_T, HES_M, DATE_S, HES_K, SHARH, BED, BES, HES, MAND, TS) " +
                                      $"VALUES ({rst2.N_S}, '{rst2.HES_T}', '{rst2.HES_M}', {rst2.DATE_S}, '{rst2.HES_K}', '{rst2.SHARH}', {rst2.BED}, {rst2.BES}, '{rst2.HES}', {rst2.MAND}, {rst2.TS})");
                }

                var rstData2 = dbms.DoGetDataSQL<AI_DAFTAR>("SELECT * FROM dbo.AI_DAFTARMOIN").ToList();
                dbms.DoExecuteSQL("DELETE FROM dbo.AI_DAFTAR");

                rstData2.Reverse(); // MoveLast equivalent
                MAND = 0;
                HESM = "-1";
                CHK = false;

                for (int i = 0; i < rstData2.Count; i++)
                {
                    var rst = rstData2[i];
                    if (rst.HES != HESM)
                    {
                        MAND = 0;
                        HESM = rst.HES;
                        CHK = false;
                    }

                    if ((Math.Round((rst.MAND ?? 0) * (rst.TS ?? 1) * -1) >= Math.Round(rst.BES ?? 0)) && (rst.MAND != 0))
                    {
                        var rst2 = new AI_DAFTAR
                        {
                            N_S = rst.N_S,
                            HES_T = rst.HES_T,
                            HES_M = rst.HES_M,
                            DIFF = (int?)CL_HESABDARI.DIFF((long)rst.DATE_S, CL_HESABDARI.FARSIDATE()),
                            DATE_S = rst.DATE_S,
                            HES_K = rst.HES_K,
                            SHARH = rst.SHARH,
                            BED = rst.BED,
                            BES = rst.BES,
                            HES = rst.HES,
                            MAND = Math.Round(rst.MAND ?? 0),
                            TS = rst.TS
                        };

                        dbms.DoExecuteSQL($"INSERT INTO dbo.AI_DAFTAR (N_S, HES_T, HES_M, DIFF, DATE_S, HES_K, SHARH, BED, BES, HES, MAND, TS) " +
                                          $"VALUES ({rst2.N_S}, '{rst2.HES_T}', '{rst2.HES_M}', {rst2.DIFF}, {rst2.DATE_S}, '{rst2.HES_K}', '{rst2.SHARH}', {rst2.BED}, {rst2.BES}, '{rst2.HES}', {rst2.MAND}, {rst2.TS})");
                        CHK = true;
                    }
                    else if (CHK)
                    {
                        var rst2 = new AI_DAFTAR
                        {
                            N_S = rst.N_S,
                            HES_T = rst.HES_T,
                            HES = rst.HES,
                            HES_M = rst.HES_M,
                            DATE_S = rst.DATE_S,
                            HES_K = rst.HES_K,
                            SHARH = "مانده تا اين تاريخ",
                            MAND = Math.Round(rst.MAND ?? 0),
                            TS = rst.TS,
                            DIFF = (int?)CL_HESABDARI.DIFF((long)rst.DATE_S, CL_HESABDARI.FARSIDATE())
                        };

                        if (rst.TS == 1)
                        {
                            rst2.BED = Math.Round(rst.MAND ?? 0);
                            rst2.BES = 0;
                        }
                        else
                        {
                            rst2.BES = Math.Round(rst.MAND ?? 0);
                            rst2.BED = 0;
                        }

                        dbms.DoExecuteSQL($"INSERT INTO dbo.AI_DAFTAR (N_S, HES_T, HES, HES_M, DATE_S, HES_K, SHARH, BED, BES, MAND, TS, DIFF) " +
                                          $"VALUES ({rst2.N_S}, '{rst2.HES_T}', '{rst2.HES}', '{rst2.HES_M}', {rst2.DATE_S}, '{rst2.HES_K}', '{rst2.SHARH}', {rst2.BED}, {rst2.BES}, {rst2.MAND}, {rst2.TS}, {rst2.DIFF})");
                        CHK = false;

                        while (i > 0 && rstData2[i].HES == HESM)
                        {
                            i--;
                        }
                        if (i > 0) i++; // MoveNext equivalent
                    }
                    if (i > 0) i--; // MovePrevious equivalent
                }

                OpenReportAI();
            }
            
            this.Close();
        }

        private void OpenReportAI()
        {
            throw new NotImplementedException();
        }
    }
}
