using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Syncfusion.Data.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Prg_Proccessy.MODELS;
using Prg_UI.Wins.WinOther;
using Prg_UI.HelperWins;
using Prg_UI.Functions;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Functions.InventoryManager;
using Prg_Proccessy.Generaly;
using Stimulsoft.Base;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using Stimulsoft.Report.Components;
using Prg_Proccessy.FUNCTIONS;
using Rpts;

namespace Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR.xaml
    /// </summary>
    public partial class F_MENU_ANBAR : Window
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

        public F_MENU_ANBAR(string _OpenArg_)
        {
            OpenArgs = _OpenArg_;
            InitializeComponent();
        }

        /// <summary>
        /// ////////////////////////////////////////////// FORM IN NOT COMPLETE !!!!!!!!!
        /// </summary>
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; }
        public Visual I_AM_MENU_ANBAR { get; set; }
        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();
        public class Q1
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
        }

        public class Q2
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public class Q3
        {
            public int? CODE { get; set; }
        }

        public class Q4
        {
            public string? CODE { get; set; }
            public double? ANBAR { get; set; }
            public double? MAND { get; set; }
            public double? FII { get; set; }
            public double? MABLK { get; set; }
            public long? VCOD { get; set; }
            public double? GRCOD { get; set; }
            public long? EGDATE { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class Q5
        {
            public string? CODE { get; set; }
            public double? ANBAR { get; set; }
            public string? BARGAH { get; set; }
            public long? DATE_N { get; set; }
            public double? MEGK { get; set; }
            public double? MEGKM { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
        }

        public string MANBAR { get; set; }
        public string DTT { get; set; }
        public string MKALA { get; set; }
        public string OpenArgs { get; private set; } = "R";

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


        private void CANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ANBAR.SelectedValue = CANBAR.SelectedValue;

            if (CANBAR.SelectedValue is null)
            {
                return;
            }

            KALA.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF INNER JOIN STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (((STUF_FSK.ANBAR)= {ANBAR.SelectedValue})) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            KALA.SelectedValuePath = "CODE";
            KALA.DisplayMemberPath = "NAME";
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (ANBAR.SelectedValue is null)
            {
                return;
            }

            CANBAR.SelectedValue = ANBAR.SelectedValue;
            KALA.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF INNER JOIN STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (((STUF_FSK.ANBAR)= {ANBAR.SelectedValue})) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            KALA.SelectedValuePath = "CODE";
            KALA.DisplayMemberPath = "NAME";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_MENU_ANBAR = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DT2.Text = Tarikh.FullCurrentDate;

            Fill_ComboBoxes();
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            if (ANBAR.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "انبار نمیتواند خالی باشد");
                msgwin.ShowDialog();
                return;
            }

            string sql;
            string PATH;
            int i;
            if (IsNull(this.DT2.Text.ToRawTarikh()))
            {
                Msgwin msgwin = new Msgwin(false, "تاریخ نباید خالی باشد.");
                msgwin.ShowDialog();
                return;
            }
            if (IsNull(this.ANBAR.SelectedValue))
            {
                MANBAR = "%";
            }
            else
            {
                MANBAR = this.ANBAR.SelectedValue.ToString();
            }
            if (IsNull(this.KALA.SelectedValue))
            {
                MKALA = "%";
            }
            else
            {
                MKALA = this.KALA.SelectedValue.ToString();
            }
            switch (this.OpenArgs)
            {
                case "R":
                    {
                        if (IsNull(this.KALA.SelectedValue))
                        {
                            OpenReport2();
                        }
                        else
                        {
                            OpenReport();
                        }
                        break;
                    }
                //case "R2":
                //    {
                //        if (IsNull(this.KALA))
                //        {
                //            DoCmd.OpenReport("R_AK_MOGUDI_ANBAR2", acViewPreview);
                //        }
                //        else
                //        {
                //            DoCmd.OpenReport("R_AK_MOGUDI_ANBAR2", acViewPreview, default, "CODE = '" + this.KALA + "'");
                //        }
                //        DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
                //        break;
                //    }
                case "F":
                    {
                        if (this.KALA.SelectedValue is null && ANBAR.SelectedValue is null && CANBAR.SelectedValue is null)
                        {
                            string Query = $"SELECT * FROM mogudi_tafkik({DT2.Text.ToRawTarikh()},N'%')";
                            new AK_MOGUDI_ANBAR_LIST(Query).Show();
                        }
                        else if (ANBAR.SelectedValue is not null && CANBAR.SelectedValue is not null && KALA.SelectedValue is null)
                        {
                            string Query = $"SELECT * FROM mogudi_tafkik({DT2.Text.ToRawTarikh()},N'{ANBAR.SelectedValue}')";
                            new AK_MOGUDI_ANBAR_LIST(Query).Show();
                        }
                        else if (ANBAR.SelectedValue is not null && CANBAR.SelectedValue is not null && KALA.SelectedValue is not null)
                        {
                            string Query = $"SELECT * FROM mogudi_tafkik({DT2.Text.ToRawTarikh()},N'{ANBAR.SelectedValue}') WHERE CODE = {KALA.SelectedValue}";
                            new AK_MOGUDI_ANBAR_LIST(Query).Show();
                        }
                        break;
                    }
                //case "F2":
                //    {
                //        if (IsNull(this.KALA))
                //        {
                //            DoCmd.OpenForm("AK_MOGUDI_ANBAR_LIST2", acFormDS);
                //        }
                //        else
                //        {
                //            DoCmd.OpenForm("AK_MOGUDI_ANBAR_LIST2", acFormDS, default, "CODE = '" + this.KALA + "'");
                //        }
                //        DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
                //        break;
                //    }
                case "MOGDT":
                    {
                        // محاسبه تاريخ ايجاد مانده کالاهادر جدولmogdt+usercod
                        //DoCmd.OpenForm("BUN");
                        double meg;
                        string CODE;
                        dbms.DoExecuteSQL("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[mogdt" + Baseknow.USERCOD + "]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[mogdt" + Baseknow.USERCOD + "](    [CODE] [nvarchar](15)  NOT NULL,    [ANBAR] [float] NOT NULL,    [MAND] [float] NOT NULL,    [FII] [float] NOT NULL,    [MABLK] [float] NOT NULL,    [NAMES] [NVARCHAR](50) NOT NULL,    [GRCOD] [float] NOT NULL,    [EGDATE] [bigint] NOT NULL, CONSTRAINT [PK_mogdt" + Baseknow.USERCOD + "] PRIMARY KEY CLUSTERED(    [CODE] ASC,    [ANBAR] Asc)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] End");
                        dbms.DoExecuteSQL("IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_mogdt_EGDATE" + Baseknow.USERCOD + "]') AND type = 'D') BEGIN ALTER TABLE [dbo].[mogdt" + Baseknow.USERCOD + "] ADD  CONSTRAINT [DF_mogdt_EGDATE" + Baseknow.USERCOD + "]  DEFAULT ((0)) FOR [EGDATE] End");
                        dbms.DoExecuteSQL("DELETE FROM dbo.mogdt" + Baseknow.USERCOD);
                        if (IsNull(this.KALA.SelectedValue))
                        {
                            dbms.DoExecuteSQL("INSERT INTO dbo.mogdt" + Baseknow.USERCOD + " ([CODE],[ANBAR],[MAND], [FII] , [MABLK] , [NAMES] ,[GRCOD], [EGDATE]) SELECT  [CODE],[ANBAR],[MAND], [FII] , [MABLK] , [NAMES] ,[GRCOD], " + Baseknow.YEA + "0101 as EGDATE  FROM dbo.mogudi_tafkik(" + this.DT2.Text.ToRawTarikh() + ",'" + MANBAR + "') WHERE     (dbo.mogudi_tafkik.mand <> 0 )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL("INSERT INTO dbo.mogdt" + Baseknow.USERCOD + " ([CODE],[ANBAR],[MAND], [FII] , [MABLK] , [NAMES] ,[GRCOD], [EGDATE]) SELECT  [CODE],[ANBAR],[MAND], [FII] , [MABLK] , [NAMES] ,[GRCOD], " + Baseknow.YEA + "0101  as EGDATE  FROM dbo.mogudi_tafkik(" + this.DT2.Text.ToRawTarikh() + ",'" + MANBAR + "') WHERE     (dbo.mogudi_tafkik.mand <> 0  and CODE = '" + this.KALA.SelectedValue + "')");
                        }
                        var RST = dbms.DoGetDataSQL<Q4>("SELECT * FROM mogdt" + Baseknow.USERCOD).ToList();
                        //while (!RST.EOF)
                        foreach (var item in RST)
                        {
                            if (item.CODE == "2542")
                            {

                            }
                            meg = Convert.ToDouble(item.MAND);

                            var RST2 = dbms.DoGetDataSQL<Q5>("SELECT RTRIM(LTRIM(CODE)) AS CODE,ANBAR,  BARGAH, DATE_N,  MEGK, MEGKM,  NUMBER, TAG FROM   dbo.KART_KALA(" + (Convert.ToInt64(Baseknow.YEA) + "0101") + ", " + item.ANBAR + ", " + this.DT2.Text.ToRawTarikh() + ") KART_KALA where (tag = 1 or tag= 0 or tag = 6 or tag = 7 or tag = 4 or tag = 9 or tag = 17 or tag = 24 or tag = 22) and  code = '" + item.CODE + "' ORDER BY  DATE_N DESC, BARGAH DESC, NUMBER DESC").ToList();

                            foreach (var item2 in RST2)
                            {
                                meg = meg - Convert.ToDouble(item2.MEGK);
                                if (meg <= 0d)
                                {
                                    // If meg = 0 And Not RST2.BOF Then
                                    // RST2.MovePrevious
                                    // rst.Fields("EGDATE") = RST2.Fields("DATE_N")
                                    // Else
                                    item.EGDATE = item2.DATE_N;
                                    // End If
                                    //RST.update();
                                }
                                //RST2.MoveNext();
                                //System.Windows.Forms["BUN"].Form.Refresh();
                            }
                            //if (RST2.EOF)
                            //{
                            //    RST.Fields("EGDATE") = (long)(System.Windows.Forms["BASEKNOW"]["YEA"] + "0101");
                            //    RST.update();
                            //}
                        }
                        break;
                    }
            }
        }

        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<Q2>($"SELECT CODE, NAMES FROM TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = {Baseknow.USERCOD})))").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            CANBAR.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT CODE FROM TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = {Baseknow.USERCOD})))").ToList();
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";
        }

        private void KALA_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (KALA.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox CUTSNO_TEX = (TextBox)KALA.Template.FindName("PART_EditableTextBox", KALA);
            if (CUTSNO_TEX.Text is null || CUTSNO_TEX.Text == "")
            {
                Msgwin msgwin = new Msgwin(false, "کالا نباید خالی باشد");
                msgwin.ShowDialog();
                return;
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                SERCHK sERCHK = new SERCHK(I_AM_MENU_ANBAR, ANBAR.SelectedValue.ToString());
                sERCHK.ShowDialog();

                if (FROM_SAERCH_KAL.CODE is null)
                {
                    return;
                }
                else
                {
                    ANBAR.SelectedValue = Convert.ToInt32(FROM_SAERCH_KAL.CODE);
                    //Cleaning
                    FROM_SAERCH_KAL.CODE = null;
                    FROM_SAERCH_KAL.NAME_CODE = null;
                }
            }
        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_AK_MOGUDI_ANBAR.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["DATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report["ANBAR_PARM"] = ANBAR.SelectedValue.ToString();
            report["CODE_PARM"] = KALA.SelectedValue.ToString();

            (report.GetComponentByName("DATE_S") as StiText).Text = DT2.Text.ToString();
            (report.GetComponentByName("DATE_F") as StiText).Text = DT2.Text.ToString();
            (report.GetComponentByName("ANBAR_F") as StiText).Text = ANBAR.Text.ToString();

            //report["DATE_S"] = DT2.Text.ToString();
            //report["DATE_F"] = DT2.Text.ToString();
            //report["ANBAR_F"] = ANBAR.Text.ToString();

            //report["AZDATE"] = Baseknow.YEA + "0101";
            //report["ANBAR"] = ANBAR.SelectedValue.ToString();

            //string TaTarikh = "99999999";
            //if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            //report["TADATE"] = TaTarikh;

            //report["KALACODE"] = KALA.SelectedValue.ToString();
            //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;

            //Report_Open:
            //(report.GetComponentByName("Table1_Cell17") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEG
            //(report.GetComponentByName("Table1_Cell14") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK

            //report.Render();
            //report.Show();

            new WINRPT(report, "گزارش موجودی انبار").Show();
        }

        private void OpenReport2()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_AK_MOGUDI_ANBAR_NO_WHERE.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["DATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report["ANBAR_PARM"] = ANBAR.SelectedValue.ToString();

            (report.GetComponentByName("DATE_S") as StiText).Text = DT2.Text.ToString();
            (report.GetComponentByName("DATE_F") as StiText).Text = DT2.Text.ToString();
            (report.GetComponentByName("ANBAR_F") as StiText).Text = ANBAR.Text.ToString();

            //report["AZDATE"] = Baseknow.YEA + "0101";
            //report["ANBAR"] = ANBAR.SelectedValue.ToString();

            //string TaTarikh = "99999999";
            //if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            //report["TADATE"] = TaTarikh;

            //report["KALACODE"] = KALA.SelectedValue.ToString();
            //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;

            //Report_Open:
            //(report.GetComponentByName("Table1_Cell17") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEG
            //(report.GetComponentByName("Table1_Cell14") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK

            new WINRPT(report, "گزارش موجودی انبار").Show();
            //report.Render();
            //report.Show();
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
    }
}

