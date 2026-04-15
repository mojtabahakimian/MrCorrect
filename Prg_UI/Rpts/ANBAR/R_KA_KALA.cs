using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Stimulsoft.Controls;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Dialogs;
using Stimulsoft.Report.Components;

namespace Prg_UI.Rpts.ANBAR
{
    public class R_KA_KALA : Stimulsoft.Report.StiReport
    {
        public R_KA_KALA()        {
            this.InitializeComponent();
        }

        #region StiReport Designer generated code - do not modify
        public decimal MogRunning;
        public Stimulsoft.Report.Components.StiPage Page1;
        public Stimulsoft.Report.Components.StiPageHeaderBand PageHeaderBand1;
        public Stimulsoft.Report.Components.StiText Text10;
        public Stimulsoft.Report.Components.StiText Text2;
        public Stimulsoft.Report.Components.StiText Text14;
        public Stimulsoft.Report.Components.StiText Text15;
        public Stimulsoft.Report.Components.StiText Text16;
        public Stimulsoft.Report.Components.StiText Text1;
        public Stimulsoft.Report.Components.StiText Text4;
        public Stimulsoft.Report.Components.StiText Text3;
        public Stimulsoft.Report.Components.StiText Text5;
        public Stimulsoft.Report.Components.StiText Text6;
        public Stimulsoft.Report.Components.StiText Text7;
        public Stimulsoft.Report.Components.StiText Text8;
        public Stimulsoft.Report.Components.StiText Text11;
        public Stimulsoft.Report.Components.Table.StiTable Table1;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell1;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell2;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell3;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell4;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell5;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell6;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell7;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell8;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell9;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell10;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell11;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell12;
        public Stimulsoft.Report.Dictionary.StiSumDecimalFunctionService Table1_Cell12_Sum;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell13;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell14;
        public Stimulsoft.Report.Dictionary.StiSumDecimalFunctionService Table1_Cell14_Sum;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell15;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell16;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell17;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell18;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell19;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell20;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell21;
        public Stimulsoft.Report.Components.Table.StiTableCell Table1_Cell22;
        public Stimulsoft.Report.Components.StiFooterBand FooterBand1;
        public Stimulsoft.Report.Components.StiText Text9;
        public KART_KALADataSource KART_KALA;
        
        public void Text10__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text10
            e.Value = "کد کالا:";
        }
        
        public void Text2__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text2
            e.Value = "صفحه";
        }
        
        public void Text14__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text14
            e.StoreToPrinted = true;
            e.Value = "#%#{TotalPageCount}";
        }
        
        public string Text14_GetValue_End(Stimulsoft.Report.Components.StiComponent sender)
        {
            // CheckerInfo: Text Text14
            return ToString(sender, TotalPageCount, true);
        }
        
        public void Text15__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text15
            e.Value = "از";
        }
        
        public void Text16__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text16
            e.StoreToPrinted = true;
            e.Value = "#%#{PageNumber}";
        }
        
        public string Text16_GetValue_End(Stimulsoft.Report.Components.StiComponent sender)
        {
            // CheckerInfo: Text Text16
            return ToString(sender, PageNumber, true);
        }
        
        public void Text1__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text1
            e.Value = ":کارت کالا در انبار";
        }
        
        public void Text4__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text4
            e.Value = ToString(sender, KART_KALA.NAMES, true);
        }
        
        public void Text3__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text3
            e.Value = ToString(sender, KART_KALA.CODE, true);
        }
        
        public void Text5__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text5
            e.Value = "نام کالا:";
        }
        
        public void Text6__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text6
            e.Value = ToString(sender, KART_KALA.NAME, true);
        }
        
        public void Text7__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text7
            e.Value = "شماره فنی:";
        }
        
        public void Text8__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text8
            e.Value = ToString(sender, KART_KALA.N_FANI, true);
        }
        
        public void Text11__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text11
            e.Value = ToString(sender, MogRunning, true);
        }
        
        public void Table1_Cell1__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell1
            e.Value = "مبلغ موجودی";
        }
        
        public void Table1_Cell2__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell2
            e.Value = "مبلغ میانگین";
        }
        
        public void Table1_Cell3__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell3
            e.Value = "موجودی";
        }
        
        public void Table1_Cell4__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell4
            e.Value = "مبلغ کل";
        }
        
        public void Table1_Cell5__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell5
            e.Value = "بهای واحد";
        }
        
        public void Table1_Cell6__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell6
            e.Value = "مقدار";
        }
        
        public void Table1_Cell7__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell7
            e.Value = "فروشنده - خریدار";
        }
        
        public void Table1_Cell8__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell8
            e.Value = "تاریخ";
        }
        
        public void Table1_Cell9__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell9
            e.Value = "شماره برگه";
        }
        
        public void Table1_Cell10__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell10
            e.Value = "نوع برگه";
        }
        
        public void Table1_Cell11__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell11
            e.Value = "ردیف";
        }
        
        public void Table1_Cell12__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell12
            e.StoreToPrinted = true;
            e.Value = "#%#{ (KART_KALA.avrage) * (SumRunning(KART_KALA.MEG))} ";
        }
        
        public string Table1_Cell12_GetValue_End(Stimulsoft.Report.Components.StiComponent sender)
        {
            // CheckerInfo: Text Table1_Cell12
            return this.Table1_Cell12.TextFormat.Format(CheckExcelValue(sender,  (KART_KALA.avrage) * (((decimal)(StiReport.ChangeType(this.Table1_Cell12_Sum.GetValue(), typeof(decimal), true)))) + " "));
        }
        
        public void Table1_Cell13__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell13
            e.Value = this.Table1_Cell13.TextFormat.Format(CheckExcelValue(sender, KART_KALA.avrage));
        }
        
        private void Table1_Cell14_Conditions(object sender, System.EventArgs e)
        {
            // CheckerInfo: Conditions Table1_Cell14
            if ((MogRunning > 0))
            {
                ((Stimulsoft.Report.Components.IStiTextBrush)(sender)).TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0));
                ((Stimulsoft.Report.Components.IStiBrush)(sender)).Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 128, 230, 137));
                Stimulsoft.Report.Components.StiConditionHelper.ApplyFont(sender, new System.Drawing.Font("IRANYekanFN", 8F), Stimulsoft.Report.Components.StiConditionPermissions.All);
                ((Stimulsoft.Report.Components.IStiBorder)(sender)).Border = ((Stimulsoft.Base.Drawing.StiBorder)(((Stimulsoft.Report.Components.IStiBorder)(sender)).Border.Clone()));
                ((Stimulsoft.Report.Components.IStiBorder)(sender)).Border.Side = Stimulsoft.Base.Drawing.StiBorderSides.All;
                ((Stimulsoft.Report.Components.StiComponent)(sender)).Enabled = true;
            }
            if ((MogRunning < 0))
            {
                ((Stimulsoft.Report.Components.IStiTextBrush)(sender)).TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0));
                ((Stimulsoft.Report.Components.IStiBrush)(sender)).Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 192, 80, 77));
                Stimulsoft.Report.Components.StiConditionHelper.ApplyFont(sender, new System.Drawing.Font("IRANYekanFN", 8F), Stimulsoft.Report.Components.StiConditionPermissions.All);
                ((Stimulsoft.Report.Components.IStiBorder)(sender)).Border = ((Stimulsoft.Base.Drawing.StiBorder)(((Stimulsoft.Report.Components.IStiBorder)(sender)).Border.Clone()));
                ((Stimulsoft.Report.Components.IStiBorder)(sender)).Border.Side = Stimulsoft.Base.Drawing.StiBorderSides.All;
                ((Stimulsoft.Report.Components.StiComponent)(sender)).Enabled = true;
            }
        }
        
        public void Table1_Cell14__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell14
            e.StoreToPrinted = true;
            e.Value = "#%#{SumRunning(Table1,KART_KALA.MEG)}";
        }
        
        public string Table1_Cell14_GetValue_End(Stimulsoft.Report.Components.StiComponent sender)
        {
            // CheckerInfo: Text Table1_Cell14
            return this.Table1_Cell14.TextFormat.Format(CheckExcelValue(sender, ((decimal)(StiReport.ChangeType(this.Table1_Cell14_Sum.GetValue(), typeof(decimal), true)))));
        }
        
        public void Table1_Cell15__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell15
            e.Value = this.Table1_Cell15.TextFormat.Format(CheckExcelValue(sender, KART_KALA.MABLK));
        }
        
        public void Table1_Cell16__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell16
            e.Value = this.Table1_Cell16.TextFormat.Format(CheckExcelValue(sender, KART_KALA.mabl_a));
        }
        
        public void Table1_Cell17__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell17
            e.Value = this.Table1_Cell17.TextFormat.Format(CheckExcelValue(sender, KART_KALA.MEGK));
        }
        
        public void Table1_Cell18__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell18
            e.Value = ToString(sender, KART_KALA.BEDNAME, true);
        }
        
        public void Table1_Cell19__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell19
            e.Value = this.Table1_Cell19.TextFormat.Format(KART_KALA.DATE_N);
        }
        
        public void Table1_Cell20__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell20
            e.Value = ToString(sender, KART_KALA.NUMBER, true);
        }
        
        public void Table1_Cell21__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell21
            e.Value = ToString(sender, KART_KALA.BARGAH, true);
        }
        
        public void Table1_Cell22__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Table1_Cell22
            e.Value = ToString(sender, Line, true);
        }
        
        public void Table1_BeforePrint(object sender, System.EventArgs e)
        {
            // CheckerInfo: BeforePrintEvent Table1
            MogRunning = MogRunning + KART_KALA.MEG;;
        }
        
        public void Text9__GetValue(object sender, Stimulsoft.Report.Events.StiGetValueEventArgs e)
        {
            // CheckerInfo: Text Text9
            e.Value = ToString(sender, Today, true) + " " + ToString(sender, Time.ToString("HH:mm"), true);
        }
        
        public void Table1__BeginRender(object sender, System.EventArgs e)
        {
            this.Table1_Cell12_Sum.Init();
            this.Table1_Cell12.TextValue = "";
            this.Table1_Cell14_Sum.Init();
            this.Table1_Cell14.TextValue = "";
        }
        
        public void Table1__AfterPrint(object sender, System.EventArgs e)
        {
            this.Table1_Cell12.SetText(new Stimulsoft.Report.Components.StiGetValue(this.Table1_Cell12_GetValue_End), true);
            this.Table1_Cell14.SetText(new Stimulsoft.Report.Components.StiGetValue(this.Table1_Cell14_GetValue_End), true);
        }
        
        public void Table1__Rendering(object sender, System.EventArgs e)
        {
            // CheckerInfo: Text Table1_Cell12
            try
            {
                this.Table1_Cell12_Sum.CalcItem(KART_KALA.MEG);
            }
            catch (System.Exception ex)
            {
                StiLogService.Write(this.GetType(), "Table1 RenderingEvent Table1_Cell12_Sum ...ERROR");
                StiLogService.Write(this.GetType(), ex);
                this.WriteToReportRenderingMessages("Table1_Cell12_Sum " + ex.Message);
            }
            // CheckerInfo: Text Table1_Cell14
            try
            {
                this.Table1_Cell14_Sum.CalcItem(KART_KALA.MEG);
            }
            catch (System.Exception ex)
            {
                StiLogService.Write(this.GetType(), "Table1 RenderingEvent Table1_Cell14_Sum ...ERROR");
                StiLogService.Write(this.GetType(), ex);
                this.WriteToReportRenderingMessages("Table1_Cell14_Sum " + ex.Message);
            }
        }
        
        public void ReportWordsToEnd__EndRender(object sender, System.EventArgs e)
        {
            this.Text14.SetText(new Stimulsoft.Report.Components.StiGetValue(this.Text14_GetValue_End));
            this.Text16.SetText(new Stimulsoft.Report.Components.StiGetValue(this.Text16_GetValue_End));
        }
        
        public void CheckEndRenderRuntimes__EndRender(object sender, System.EventArgs e)
        {
            Stimulsoft.Report.Components.StiSimpleText.CheckEndRenderRuntimes(this);
            Stimulsoft.Report.Components.StiSimpleText.ProcessEndRenderSetText(this);
        }
        
        private void InitializeComponent()
        {
            this.KART_KALA = new KART_KALADataSource();
            this.Dictionary.Variables.Add(new Stimulsoft.Report.Dictionary.StiVariable("", "MogRunning", "MogRunning", "MogRunning", typeof(decimal), "0", false, Stimulsoft.Report.Dictionary.StiVariableInitBy.Value, false, new Stimulsoft.Report.Dictionary.StiDialogInfo(Stimulsoft.Report.Dictionary.StiDateTimeType.Date, "", true, Stimulsoft.Report.Dictionary.StiVariableSortDirection.Asc, new string[0], new string[0], null), null, false, Stimulsoft.Report.Dictionary.StiSelectionMode.FromVariable));
            this.NeedsCompiling = false;
            // 
            // Variables init
            // 
            // CheckerInfo: Value MogRunning
            this.MogRunning = 0m;
            this.EngineVersion = Stimulsoft.Report.Engine.StiEngineVersion.EngineV2;
            this.Key = "abfccdd0a0a04a0e908217eb3877da79";
            this.ReferencedAssemblies = new string[] {
                    "System.Dll",
                    "System.Drawing.Dll",
                    "System.Windows.Forms.Dll",
                    "System.Data.Dll",
                    "System.Xml.Dll",
                    "Stimulsoft.Controls.Dll",
                    "Stimulsoft.Base.Dll",
                    "Stimulsoft.Report.Dll"};
            this.ReportAlias = "Report";
            // 
            // ReportChanged
            // 
            this.ReportChanged = new DateTime(2025, 8, 18, 23, 40, 47, 0);
            // 
            // ReportCreated
            // 
            this.ReportCreated = new DateTime(2024, 1, 29, 9, 6, 45, 0);
            this.ReportFile = "E:\\prg\\MrCorrect\\Prg_UI\\Rpts\\ANBAR\\R_KA_KALA.mrt";
            this.ReportGuid = "2382a19598964910bea322b3d29c8861";
            this.ReportName = "Report";
            this.ReportUnit = Stimulsoft.Report.StiReportUnitType.Centimeters;
            this.ReportVersion = "2023.1.1.0";
            this.ScriptLanguage = Stimulsoft.Report.StiReportLanguageType.CSharp;
            // 
            // Page1
            // 
            this.Page1 = new Stimulsoft.Report.Components.StiPage();
            this.Page1.Guid = "8c9639f1e68e40fb8ae92b73d5be8429";
            this.Page1.Name = "Page1";
            this.Page1.Orientation = Stimulsoft.Report.Components.StiPageOrientation.Landscape;
            this.Page1.PageHeight = 21;
            this.Page1.PageWidth = 29.7;
            this.Page1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 2, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Page1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            // 
            // PageHeaderBand1
            // 
            this.PageHeaderBand1 = new Stimulsoft.Report.Components.StiPageHeaderBand();
            this.PageHeaderBand1.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 0.4, 27.7, 2.4);
            this.PageHeaderBand1.Name = "PageHeaderBand1";
            this.PageHeaderBand1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.PageHeaderBand1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            // 
            // Text10
            // 
            this.Text10 = new Stimulsoft.Report.Components.StiText();
            this.Text10.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(26.3, 1.6, 1.3, 0.6);
            this.Text10.Name = "Text10";
            this.Text10.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text10__GetValue);
            this.Text10.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text10.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text10.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text10.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text10.Indicator = null;
            this.Text10.Interaction = null;
            this.Text10.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text10.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text10.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text2
            // 
            this.Text2 = new Stimulsoft.Report.Components.StiText();
            this.Text2.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(3.4, 0, 1, 0.6);
            this.Text2.Name = "Text2";
            this.Text2.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text2__GetValue);
            this.Text2.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text2.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text2.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text2.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text2.Indicator = null;
            this.Text2.Interaction = null;
            this.Text2.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text2.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text2.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text14
            // 
            this.Text14 = new Stimulsoft.Report.Components.StiText();
            this.Text14.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 0, 1.4, 0.6);
            this.Text14.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Text14.Name = "Text14";
            this.Text14.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text14__GetValue);
            this.Text14.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text14.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Text14.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text14.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text14.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text14.Indicator = null;
            this.Text14.Interaction = null;
            this.Text14.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text14.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text14.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text15
            // 
            this.Text15 = new Stimulsoft.Report.Components.StiText();
            this.Text15.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(1.49, 0, 0.4, 0.6);
            this.Text15.Name = "Text15";
            this.Text15.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text15__GetValue);
            this.Text15.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text15.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text15.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text15.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text15.Indicator = null;
            this.Text15.Interaction = null;
            this.Text15.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text15.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text15.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text16
            // 
            this.Text16 = new Stimulsoft.Report.Components.StiText();
            this.Text16.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(2, 0, 1.4, 0.6);
            this.Text16.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Text16.Name = "Text16";
            this.Text16.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text16__GetValue);
            this.Text16.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text16.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Text16.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text16.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text16.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text16.Indicator = null;
            this.Text16.Interaction = null;
            this.Text16.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text16.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text16.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text1
            // 
            this.Text1 = new Stimulsoft.Report.Components.StiText();
            this.Text1.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(14.2, 0, 3.8, 1);
            this.Text1.Name = "Text1";
            this.Text1.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text1__GetValue);
            this.Text1.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text1.Font = new System.Drawing.Font("IRANYekanFN", 14F);
            this.Text1.Indicator = null;
            this.Text1.Interaction = null;
            this.Text1.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text1.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 192, 80, 77));
            this.Text1.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text4
            // 
            this.Text4 = new Stimulsoft.Report.Components.StiText();
            this.Text4.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(9.4, 0, 4.8, 1);
            this.Text4.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Right;
            this.Text4.Name = "Text4";
            this.Text4.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text4__GetValue);
            this.Text4.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text4.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text4.Font = new System.Drawing.Font("IRANYekanFN", 14F);
            this.Text4.Indicator = null;
            this.Text4.Interaction = null;
            this.Text4.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text4.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0));
            this.Text4.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text3
            // 
            this.Text3 = new Stimulsoft.Report.Components.StiText();
            this.Text3.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(24, 1.6, 2.2, 0.6);
            this.Text3.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Right;
            this.Text3.Name = "Text3";
            this.Text3.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text3__GetValue);
            this.Text3.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text3.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text3.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text3.Indicator = null;
            this.Text3.Interaction = null;
            this.Text3.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text3.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text3.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text5
            // 
            this.Text5 = new Stimulsoft.Report.Components.StiText();
            this.Text5.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(22.5, 1.6, 1.3, 0.6);
            this.Text5.Name = "Text5";
            this.Text5.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text5__GetValue);
            this.Text5.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text5.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text5.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text5.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text5.Indicator = null;
            this.Text5.Interaction = null;
            this.Text5.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text5.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text5.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text6
            // 
            this.Text6 = new Stimulsoft.Report.Components.StiText();
            this.Text6.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(8, 1.6, 14.4, 0.6);
            this.Text6.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Right;
            this.Text6.Name = "Text6";
            this.Text6.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text6__GetValue);
            this.Text6.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text6.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text6.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text6.Indicator = null;
            this.Text6.Interaction = null;
            this.Text6.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text6.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text6.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text7
            // 
            this.Text7 = new Stimulsoft.Report.Components.StiText();
            this.Text7.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(6.1, 1.6, 1.7, 0.6);
            this.Text7.Name = "Text7";
            this.Text7.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text7__GetValue);
            this.Text7.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text7.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text7.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text7.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text7.Indicator = null;
            this.Text7.Interaction = null;
            this.Text7.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text7.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text7.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text8
            // 
            this.Text8 = new Stimulsoft.Report.Components.StiText();
            this.Text8.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0.2, 1.6, 5.8, 0.6);
            this.Text8.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Right;
            this.Text8.Name = "Text8";
            this.Text8.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text8__GetValue);
            this.Text8.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text8.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text8.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text8.Indicator = null;
            this.Text8.Interaction = null;
            this.Text8.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text8.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text8.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Text11
            // 
            this.Text11 = new Stimulsoft.Report.Components.StiText();
            this.Text11.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(5.9, 0.8, 1.7, 0.6);
            this.Text11.Name = "Text11";
            this.Text11.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text11__GetValue);
            this.Text11.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text11.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text11.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text11.Font = new System.Drawing.Font("IRANYekanFN", 10F);
            this.Text11.Indicator = null;
            this.Text11.Interaction = null;
            this.Text11.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text11.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text11.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            this.PageHeaderBand1.Interaction = null;
            // 
            // Table1
            // 
            this.Table1 = new Stimulsoft.Report.Components.Table.StiTable();
            this.Table1.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 3.6, 27.7, 1.6);
            this.Table1.ColumnCount = 11;
            this.Table1.DataSourceName = "KART_KALA";
            this.Table1.HeaderRowsCount = 1;
            this.Table1.MinHeight = 0.4;
            this.Table1.Name = "Table1";
            this.Table1.NumberID = 77;
            this.Table1.RowCount = 2;
            this.Table1.Sort = new string[0];
            this.Table1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Table1.BusinessObjectGuid = null;
            // 
            // Table1_Cell1
            // 
            this.Table1_Cell1 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell1.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 0, 2.61, 0.8);
            this.Table1_Cell1.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell1.ID = 66;
            this.Table1_Cell1.JoinCells = new int[0];
            this.Table1_Cell1.Name = "Table1_Cell1";
            this.Table1_Cell1.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell1.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell1__GetValue);
            this.Table1_Cell1.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell1.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell1.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell1.Indicator = null;
            this.Table1_Cell1.Interaction = null;
            this.Table1_Cell1.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell1.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell1.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell2
            // 
            this.Table1_Cell2 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell2.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(2.61, 0, 2.61, 0.8);
            this.Table1_Cell2.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell2.ID = 67;
            this.Table1_Cell2.JoinCells = new int[0];
            this.Table1_Cell2.Name = "Table1_Cell2";
            this.Table1_Cell2.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell2.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell2__GetValue);
            this.Table1_Cell2.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell2.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell2.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell2.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell2.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell2.Indicator = null;
            this.Table1_Cell2.Interaction = null;
            this.Table1_Cell2.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell2.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell2.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell3
            // 
            this.Table1_Cell3 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell3.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(5.22, 0, 1.81, 0.8);
            this.Table1_Cell3.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell3.ID = 68;
            this.Table1_Cell3.JoinCells = new int[0];
            this.Table1_Cell3.Name = "Table1_Cell3";
            this.Table1_Cell3.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell3.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell3__GetValue);
            this.Table1_Cell3.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell3.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell3.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell3.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell3.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell3.Indicator = null;
            this.Table1_Cell3.Interaction = null;
            this.Table1_Cell3.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell3.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell3.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell4
            // 
            this.Table1_Cell4 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell4.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(7.03, 0, 2.61, 0.8);
            this.Table1_Cell4.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell4.ID = 69;
            this.Table1_Cell4.JoinCells = new int[0];
            this.Table1_Cell4.Name = "Table1_Cell4";
            this.Table1_Cell4.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell4.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell4__GetValue);
            this.Table1_Cell4.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell4.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell4.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell4.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell4.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell4.Indicator = null;
            this.Table1_Cell4.Interaction = null;
            this.Table1_Cell4.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell4.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell4.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell5
            // 
            this.Table1_Cell5 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell5.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(9.63, 0, 2.01, 0.8);
            this.Table1_Cell5.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell5.ID = 70;
            this.Table1_Cell5.JoinCells = new int[0];
            this.Table1_Cell5.Name = "Table1_Cell5";
            this.Table1_Cell5.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell5.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell5__GetValue);
            this.Table1_Cell5.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell5.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell5.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell5.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell5.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell5.Indicator = null;
            this.Table1_Cell5.Interaction = null;
            this.Table1_Cell5.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell5.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell5.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell6
            // 
            this.Table1_Cell6 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell6.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(11.64, 0, 2.01, 0.8);
            this.Table1_Cell6.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell6.ID = 71;
            this.Table1_Cell6.JoinCells = new int[0];
            this.Table1_Cell6.Name = "Table1_Cell6";
            this.Table1_Cell6.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell6.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell6__GetValue);
            this.Table1_Cell6.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell6.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell6.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell6.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell6.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell6.Indicator = null;
            this.Table1_Cell6.Interaction = null;
            this.Table1_Cell6.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell6.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell6.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell7
            // 
            this.Table1_Cell7 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell7.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(13.65, 0, 6.22, 0.8);
            this.Table1_Cell7.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell7.ID = 72;
            this.Table1_Cell7.JoinCells = new int[0];
            this.Table1_Cell7.Name = "Table1_Cell7";
            this.Table1_Cell7.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell7.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell7__GetValue);
            this.Table1_Cell7.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell7.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell7.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell7.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell7.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell7.Indicator = null;
            this.Table1_Cell7.Interaction = null;
            this.Table1_Cell7.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell7.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell7.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell8
            // 
            this.Table1_Cell8 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell8.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(19.87, 0, 2.21, 0.8);
            this.Table1_Cell8.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell8.ID = 73;
            this.Table1_Cell8.JoinCells = new int[0];
            this.Table1_Cell8.Name = "Table1_Cell8";
            this.Table1_Cell8.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell8.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell8__GetValue);
            this.Table1_Cell8.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell8.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell8.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell8.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell8.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell8.Indicator = null;
            this.Table1_Cell8.Interaction = null;
            this.Table1_Cell8.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell8.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell8.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell9
            // 
            this.Table1_Cell9 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell9.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(22.08, 0, 2.01, 0.8);
            this.Table1_Cell9.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell9.ID = 74;
            this.Table1_Cell9.JoinCells = new int[0];
            this.Table1_Cell9.Name = "Table1_Cell9";
            this.Table1_Cell9.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell9.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell9__GetValue);
            this.Table1_Cell9.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell9.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell9.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell9.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell9.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell9.Indicator = null;
            this.Table1_Cell9.Interaction = null;
            this.Table1_Cell9.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell9.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell9.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell10
            // 
            this.Table1_Cell10 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell10.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(24.09, 0, 2.81, 0.8);
            this.Table1_Cell10.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell10.ID = 75;
            this.Table1_Cell10.JoinCells = new int[0];
            this.Table1_Cell10.Name = "Table1_Cell10";
            this.Table1_Cell10.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell10.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell10__GetValue);
            this.Table1_Cell10.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell10.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell10.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell10.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell10.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell10.Indicator = null;
            this.Table1_Cell10.Interaction = null;
            this.Table1_Cell10.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell10.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell10.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell11
            // 
            this.Table1_Cell11 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell11.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(26.9, 0, 0.9, 0.8);
            this.Table1_Cell11.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell11.ID = 76;
            this.Table1_Cell11.JoinCells = new int[0];
            this.Table1_Cell11.Name = "Table1_Cell11";
            this.Table1_Cell11.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell11.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell11__GetValue);
            this.Table1_Cell11.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell11.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell11.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.FromArgb(255, 192, 0, 0), 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell11.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell11.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell11.Indicator = null;
            this.Table1_Cell11.Interaction = null;
            this.Table1_Cell11.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell11.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell11.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell12
            // 
            this.Table1_Cell12 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell12.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 0.8, 2.61, 0.8);
            this.Table1_Cell12.GrowToHeight = true;
            this.Table1_Cell12.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell12.ID = 0;
            this.Table1_Cell12.JoinCells = new int[0];
            this.Table1_Cell12.Name = "Table1_Cell12";
            this.Table1_Cell12.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            // Table1_Cell12_Sum
            this.Table1_Cell12_Sum = new Stimulsoft.Report.Dictionary.StiSumDecimalFunctionService(true);
            this.Table1_Cell12.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell12__GetValue);
            this.Table1_Cell12.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell12.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell12.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell12.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell12.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell12.Indicator = null;
            this.Table1_Cell12.Interaction = null;
            this.Table1_Cell12.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell12.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell12.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 0, ",", 3, true, false, " ");
            this.Table1_Cell12.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell13
            // 
            this.Table1_Cell13 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell13.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(2.61, 0.8, 2.61, 0.8);
            this.Table1_Cell13.GrowToHeight = true;
            this.Table1_Cell13.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell13.ID = 1;
            this.Table1_Cell13.JoinCells = new int[0];
            this.Table1_Cell13.Name = "Table1_Cell13";
            this.Table1_Cell13.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell13.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell13__GetValue);
            this.Table1_Cell13.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell13.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell13.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell13.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell13.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell13.Indicator = null;
            this.Table1_Cell13.Interaction = null;
            this.Table1_Cell13.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell13.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell13.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 2, ",", 3, true, false, " ");
            this.Table1_Cell13.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell14
            // 
            this.Table1_Cell14 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell14.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(5.22, 0.8, 1.81, 0.8);
            this.Table1_Cell14.BeforePrint += new System.EventHandler(this.Table1_Cell14_Conditions);
            this.Table1_Cell14.GrowToHeight = true;
            this.Table1_Cell14.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell14.ID = 2;
            this.Table1_Cell14.JoinCells = new int[0];
            this.Table1_Cell14.Name = "Table1_Cell14";
            this.Table1_Cell14.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            // Table1_Cell14_Sum
            this.Table1_Cell14_Sum = new Stimulsoft.Report.Dictionary.StiSumDecimalFunctionService(true);
            this.Table1_Cell14.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell14__GetValue);
            this.Table1_Cell14.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell14.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell14.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell14.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.FromArgb(255, 255, 255, 255));
            this.Table1_Cell14.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell14.Indicator = null;
            this.Table1_Cell14.Interaction = null;
            this.Table1_Cell14.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell14.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell14.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 2, ",", 3, true, false, " ");
            this.Table1_Cell14.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell15
            // 
            this.Table1_Cell15 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell15.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(7.03, 0.8, 2.61, 0.8);
            this.Table1_Cell15.GrowToHeight = true;
            this.Table1_Cell15.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell15.ID = 3;
            this.Table1_Cell15.JoinCells = new int[0];
            this.Table1_Cell15.Name = "Table1_Cell15";
            this.Table1_Cell15.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell15.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell15__GetValue);
            this.Table1_Cell15.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell15.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell15.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell15.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell15.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell15.Indicator = null;
            this.Table1_Cell15.Interaction = null;
            this.Table1_Cell15.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell15.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell15.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 0, ",", 3, true, false, " ");
            this.Table1_Cell15.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell16
            // 
            this.Table1_Cell16 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell16.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(9.63, 0.8, 2.01, 0.8);
            this.Table1_Cell16.GrowToHeight = true;
            this.Table1_Cell16.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell16.ID = 4;
            this.Table1_Cell16.JoinCells = new int[0];
            this.Table1_Cell16.Name = "Table1_Cell16";
            this.Table1_Cell16.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell16.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell16__GetValue);
            this.Table1_Cell16.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell16.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell16.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell16.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell16.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell16.Indicator = null;
            this.Table1_Cell16.Interaction = null;
            this.Table1_Cell16.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell16.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell16.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 0, ",", 3, true, false, " ");
            this.Table1_Cell16.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell17
            // 
            this.Table1_Cell17 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell17.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(11.64, 0.8, 2.01, 0.8);
            this.Table1_Cell17.GrowToHeight = true;
            this.Table1_Cell17.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell17.ID = 25;
            this.Table1_Cell17.JoinCells = new int[0];
            this.Table1_Cell17.Name = "Table1_Cell17";
            this.Table1_Cell17.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell17.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell17__GetValue);
            this.Table1_Cell17.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell17.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell17.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell17.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell17.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell17.Indicator = null;
            this.Table1_Cell17.Interaction = null;
            this.Table1_Cell17.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell17.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell17.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(3, ".", 0, ",", 3, true, false, " ");
            this.Table1_Cell17.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell18
            // 
            this.Table1_Cell18 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell18.CanBreak = true;
            this.Table1_Cell18.CanGrow = true;
            this.Table1_Cell18.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(13.65, 0.8, 6.22, 0.8);
            this.Table1_Cell18.GrowToHeight = true;
            this.Table1_Cell18.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Right;
            this.Table1_Cell18.ID = 26;
            this.Table1_Cell18.JoinCells = new int[0];
            this.Table1_Cell18.Name = "Table1_Cell18";
            this.Table1_Cell18.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell18.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell18__GetValue);
            this.Table1_Cell18.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell18.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell18.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell18.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell18.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell18.Indicator = null;
            this.Table1_Cell18.Interaction = null;
            this.Table1_Cell18.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell18.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell18.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, true, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell19
            // 
            this.Table1_Cell19 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell19.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(19.87, 0.8, 2.21, 0.8);
            this.Table1_Cell19.GrowToHeight = true;
            this.Table1_Cell19.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell19.ID = 27;
            this.Table1_Cell19.JoinCells = new int[0];
            this.Table1_Cell19.Name = "Table1_Cell19";
            this.Table1_Cell19.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell19.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell19__GetValue);
            this.Table1_Cell19.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell19.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell19.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell19.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell19.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell19.Indicator = null;
            this.Table1_Cell19.Interaction = null;
            this.Table1_Cell19.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell19.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell19.TextFormat = new Stimulsoft.Report.Components.TextFormats.StiCustomFormatService("####/##/##");
            this.Table1_Cell19.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell20
            // 
            this.Table1_Cell20 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell20.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(22.08, 0.8, 2.01, 0.8);
            this.Table1_Cell20.GrowToHeight = true;
            this.Table1_Cell20.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell20.ID = 28;
            this.Table1_Cell20.JoinCells = new int[0];
            this.Table1_Cell20.Name = "Table1_Cell20";
            this.Table1_Cell20.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell20.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell20__GetValue);
            this.Table1_Cell20.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell20.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell20.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell20.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell20.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell20.Indicator = null;
            this.Table1_Cell20.Interaction = null;
            this.Table1_Cell20.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell20.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell20.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell21
            // 
            this.Table1_Cell21 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell21.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(24.09, 0.8, 2.81, 0.8);
            this.Table1_Cell21.GrowToHeight = true;
            this.Table1_Cell21.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell21.ID = 29;
            this.Table1_Cell21.JoinCells = new int[0];
            this.Table1_Cell21.Name = "Table1_Cell21";
            this.Table1_Cell21.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell21.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell21__GetValue);
            this.Table1_Cell21.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Table1_Cell21.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell21.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell21.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell21.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell21.Indicator = null;
            this.Table1_Cell21.Interaction = null;
            this.Table1_Cell21.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell21.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell21.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            // 
            // Table1_Cell22
            // 
            this.Table1_Cell22 = new Stimulsoft.Report.Components.Table.StiTableCell();
            this.Table1_Cell22.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(26.9, 0.8, 0.9, 0.8);
            this.Table1_Cell22.GrowToHeight = true;
            this.Table1_Cell22.HorAlignment = Stimulsoft.Base.Drawing.StiTextHorAlignment.Center;
            this.Table1_Cell22.ID = 30;
            this.Table1_Cell22.JoinCells = new int[0];
            this.Table1_Cell22.Name = "Table1_Cell22";
            this.Table1_Cell22.Restrictions = ((Stimulsoft.Report.Components.StiRestrictions.AllowMove | Stimulsoft.Report.Components.StiRestrictions.AllowSelect) 
                        | Stimulsoft.Report.Components.StiRestrictions.AllowChange);
            this.Table1_Cell22.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Table1_Cell22__GetValue);
            this.Table1_Cell22.Type = Stimulsoft.Report.Components.StiSystemTextType.SystemVariables;
            this.Table1_Cell22.VertAlignment = Stimulsoft.Base.Drawing.StiVertAlignment.Center;
            this.Table1_Cell22.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.All, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Table1_Cell22.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.White);
            this.Table1_Cell22.Font = new System.Drawing.Font("IRANYekanFN", 9F);
            this.Table1_Cell22.Indicator = null;
            this.Table1_Cell22.Interaction = null;
            this.Table1_Cell22.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Table1_Cell22.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Table1_Cell22.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            this.Table1.DataRelationName = null;
            this.Table1.Interaction = null;
            this.Table1.TableStyleFX = null;
            this.Table1.BeforePrint += new System.EventHandler(this.Table1_BeforePrint);
            // 
            // FooterBand1
            // 
            this.FooterBand1 = new Stimulsoft.Report.Components.StiFooterBand();
            this.FooterBand1.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 6, 27.7, 0.6);
            this.FooterBand1.Name = "FooterBand1";
            this.FooterBand1.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.FooterBand1.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            // 
            // Text9
            // 
            this.Text9 = new Stimulsoft.Report.Components.StiText();
            this.Text9.ClientRectangle = new Stimulsoft.Base.Drawing.RectangleD(0, 0, 4.4, 0.6);
            this.Text9.Name = "Text9";
            this.Text9.GetValue += new Stimulsoft.Report.Events.StiGetValueEventHandler(this.Text9__GetValue);
            this.Text9.Type = Stimulsoft.Report.Components.StiSystemTextType.Expression;
            this.Text9.Border = new Stimulsoft.Base.Drawing.StiBorder(Stimulsoft.Base.Drawing.StiBorderSides.None, System.Drawing.Color.Black, 1, Stimulsoft.Base.Drawing.StiPenStyle.Solid, false, 4, new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black), false);
            this.Text9.Brush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Transparent);
            this.Text9.Font = new System.Drawing.Font("IRANYekanFN", 8F);
            this.Text9.Indicator = null;
            this.Text9.Interaction = null;
            this.Text9.Margins = new Stimulsoft.Report.Components.StiMargins(0, 0, 0, 0);
            this.Text9.TextBrush = new Stimulsoft.Base.Drawing.StiSolidBrush(System.Drawing.Color.Black);
            this.Text9.TextOptions = new Stimulsoft.Base.Drawing.StiTextOptions(false, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, System.Drawing.StringTrimming.None);
            this.FooterBand1.Interaction = null;
            this.Page1.Interaction = null;
            this.Page1.Margins = new Stimulsoft.Report.Components.StiMargins(1, 1, 1, 1);
            this.Page1.Report = this;
            this.PageHeaderBand1.Page = this.Page1;
            this.PageHeaderBand1.Parent = this.Page1;
            this.Text10.Page = this.Page1;
            this.Text10.Parent = this.PageHeaderBand1;
            this.Text2.Page = this.Page1;
            this.Text2.Parent = this.PageHeaderBand1;
            this.Text14.Page = this.Page1;
            this.Text14.Parent = this.PageHeaderBand1;
            this.Text15.Page = this.Page1;
            this.Text15.Parent = this.PageHeaderBand1;
            this.Text16.Page = this.Page1;
            this.Text16.Parent = this.PageHeaderBand1;
            this.Text1.Page = this.Page1;
            this.Text1.Parent = this.PageHeaderBand1;
            this.Text4.Page = this.Page1;
            this.Text4.Parent = this.PageHeaderBand1;
            this.Text3.Page = this.Page1;
            this.Text3.Parent = this.PageHeaderBand1;
            this.Text5.Page = this.Page1;
            this.Text5.Parent = this.PageHeaderBand1;
            this.Text6.Page = this.Page1;
            this.Text6.Parent = this.PageHeaderBand1;
            this.Text7.Page = this.Page1;
            this.Text7.Parent = this.PageHeaderBand1;
            this.Text8.Page = this.Page1;
            this.Text8.Parent = this.PageHeaderBand1;
            this.Text11.Page = this.Page1;
            this.Text11.Parent = this.PageHeaderBand1;
            this.Table1.Page = this.Page1;
            this.Table1.Parent = this.Page1;
            this.Table1_Cell1.Page = this.Page1;
            this.Table1_Cell1.Parent = this.Table1;
            this.Table1_Cell2.Page = this.Page1;
            this.Table1_Cell2.Parent = this.Table1;
            this.Table1_Cell3.Page = this.Page1;
            this.Table1_Cell3.Parent = this.Table1;
            this.Table1_Cell4.Page = this.Page1;
            this.Table1_Cell4.Parent = this.Table1;
            this.Table1_Cell5.Page = this.Page1;
            this.Table1_Cell5.Parent = this.Table1;
            this.Table1_Cell6.Page = this.Page1;
            this.Table1_Cell6.Parent = this.Table1;
            this.Table1_Cell7.Page = this.Page1;
            this.Table1_Cell7.Parent = this.Table1;
            this.Table1_Cell8.Page = this.Page1;
            this.Table1_Cell8.Parent = this.Table1;
            this.Table1_Cell9.Page = this.Page1;
            this.Table1_Cell9.Parent = this.Table1;
            this.Table1_Cell10.Page = this.Page1;
            this.Table1_Cell10.Parent = this.Table1;
            this.Table1_Cell11.Page = this.Page1;
            this.Table1_Cell11.Parent = this.Table1;
            this.Table1_Cell12.Page = this.Page1;
            this.Table1_Cell12.Parent = this.Table1;
            this.Table1_Cell13.Page = this.Page1;
            this.Table1_Cell13.Parent = this.Table1;
            this.Table1_Cell14.Page = this.Page1;
            this.Table1_Cell14.Parent = this.Table1;
            this.Table1_Cell15.Page = this.Page1;
            this.Table1_Cell15.Parent = this.Table1;
            this.Table1_Cell16.Page = this.Page1;
            this.Table1_Cell16.Parent = this.Table1;
            this.Table1_Cell17.Page = this.Page1;
            this.Table1_Cell17.Parent = this.Table1;
            this.Table1_Cell18.Page = this.Page1;
            this.Table1_Cell18.Parent = this.Table1;
            this.Table1_Cell19.Page = this.Page1;
            this.Table1_Cell19.Parent = this.Table1;
            this.Table1_Cell20.Page = this.Page1;
            this.Table1_Cell20.Parent = this.Table1;
            this.Table1_Cell21.Page = this.Page1;
            this.Table1_Cell21.Parent = this.Table1;
            this.Table1_Cell22.Page = this.Page1;
            this.Table1_Cell22.Parent = this.Table1;
            this.FooterBand1.Page = this.Page1;
            this.FooterBand1.Parent = this.Page1;
            this.Text9.Page = this.Page1;
            this.Text9.Parent = this.FooterBand1;
            this.Table1.BeginRender += new System.EventHandler(this.Table1__BeginRender);
            this.Table1.AfterPrint += new System.EventHandler(this.Table1__AfterPrint);
            this.Table1.Rendering += new System.EventHandler(this.Table1__Rendering);
            this.EndRender += new System.EventHandler(this.ReportWordsToEnd__EndRender);
            this.AggregateFunctions = new object[] {
                    this.Table1_Cell12_Sum,
                    this.Table1_Cell14_Sum};
            this.EndRender += new System.EventHandler(this.CheckEndRenderRuntimes__EndRender);
            // 
            // Add to PageHeaderBand1.Components
            // 
            this.PageHeaderBand1.Components.Clear();
            this.PageHeaderBand1.Components.AddRange(new Stimulsoft.Report.Components.StiComponent[] {
                        this.Text10,
                        this.Text2,
                        this.Text14,
                        this.Text15,
                        this.Text16,
                        this.Text1,
                        this.Text4,
                        this.Text3,
                        this.Text5,
                        this.Text6,
                        this.Text7,
                        this.Text8,
                        this.Text11});
            // 
            // Add to Table1.Components
            // 
            this.Table1.Components.Clear();
            this.Table1.Components.AddRange(new Stimulsoft.Report.Components.StiComponent[] {
                        this.Table1_Cell1,
                        this.Table1_Cell2,
                        this.Table1_Cell3,
                        this.Table1_Cell4,
                        this.Table1_Cell5,
                        this.Table1_Cell6,
                        this.Table1_Cell7,
                        this.Table1_Cell8,
                        this.Table1_Cell9,
                        this.Table1_Cell10,
                        this.Table1_Cell11,
                        this.Table1_Cell12,
                        this.Table1_Cell13,
                        this.Table1_Cell14,
                        this.Table1_Cell15,
                        this.Table1_Cell16,
                        this.Table1_Cell17,
                        this.Table1_Cell18,
                        this.Table1_Cell19,
                        this.Table1_Cell20,
                        this.Table1_Cell21,
                        this.Table1_Cell22});
            // 
            // Add to FooterBand1.Components
            // 
            this.FooterBand1.Components.Clear();
            this.FooterBand1.Components.AddRange(new Stimulsoft.Report.Components.StiComponent[] {
                        this.Text9});
            // 
            // Add to Page1.Components
            // 
            this.Page1.Components.Clear();
            this.Page1.Components.AddRange(new Stimulsoft.Report.Components.StiComponent[] {
                        this.PageHeaderBand1,
                        this.Table1,
                        this.FooterBand1});
            // 
            // Add to Pages
            // 
            this.Pages.Clear();
            this.Pages.AddRange(new Stimulsoft.Report.Components.StiPage[] {
                        this.Page1});
            this.KART_KALA.Columns.AddRange(new Stimulsoft.Report.Dictionary.StiDataColumn[] {
                        new Stimulsoft.Report.Dictionary.StiDataColumn("NAME", "NAME", "NAME", typeof(string), "4ec34206ea6d480aabb9c2c1f432067a"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("CODE", "CODE", "CODE", typeof(string), "58e294d50963443389d3479e4f3b3114"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("NAMES", "NAMES", "NAMES", typeof(string), "ea6299252f674aacbbd7bd49b94b48b3"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("N_FANI", "N_FANI", "N_FANI", typeof(string), "ef5f84660ec64f00918e14c60379f8d1"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("MEGK", "MEGK", "MEGK", typeof(double), "0a7ca8d9e51744e1b01442fd9f27d0a1"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("mabl_a", "mabl_a", "mabl_a", typeof(double), "ef7f325385dc4f3698d70ebe33bb52a8"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("MABLK", "MABLK", "MABLK", typeof(double), "7f5f1f5992734fdeb7c8c9853c8d405f"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("MEG", "MEG", "MEG", typeof(decimal), "5ec81a80cb714fd0b195c261b8c21fb7"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("BARGAH", "BARGAH", "BARGAH", typeof(string), "2729d3660785496e9f3fa02acae735bd"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("NUMBER", "NUMBER", "NUMBER", typeof(double), "c08280cbc5fe42f68acc1260b4f9c0aa"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("FNUMCO", "FNUMCO", "FNUMCO", typeof(double), "f58d0de87e7943d7b3d443eeaa684848"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("DATE_N", "DATE_N", "DATE_N", typeof(long), "2bef9cebcdd3436888a33ac7dbced1ac"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("BEDNAME", "BEDNAME", "BEDNAME", typeof(string), "9938e2f5ec844ed48834a97781fc9560"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("avrage", "avrage", "avrage", typeof(decimal), "1e6884f0141b404e8d845649d9bda920"),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("TAG", "TAG", "TAG", typeof(double), null),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("MABM", "MABM", "MABM", typeof(double), null),
                        new Stimulsoft.Report.Dictionary.StiDataColumn("MEGKM", "MEGKM", "MEGKM", typeof(double), "9fa7346c738e45f89559cb0f9bef059f")});
            this.KART_KALA.Parameters.AddRange(new Stimulsoft.Report.Dictionary.StiDataParameter[] {
                        new Stimulsoft.Report.Dictionary.StiDataParameter("AZDATE", 12, 10, null),
                        new Stimulsoft.Report.Dictionary.StiDataParameter("ANBAR", 12, 30, null),
                        new Stimulsoft.Report.Dictionary.StiDataParameter("TADATE", 12, 10, null),
                        new Stimulsoft.Report.Dictionary.StiDataParameter("KALACODE", 12, 30, null)});
            this.DataSources.Add(this.KART_KALA);
            this.Dictionary.Databases.Add(new Stimulsoft.Report.Dictionary.StiSqlDatabase("MS SQL", "MS SQL", "#%#", false, "6f2a847f8b23432eafaa10f7ed6e88ac"));
            ((Stimulsoft.Report.Dictionary.StiSqlDatabase)(this.Dictionary.Databases["MS SQL"])).ConnectionStringEncrypted = "9BrNoe9K+yFqe73M+lTjF9U0gS5FWVKk4k25gHM9InvVeHfP5wX6XX0IqNRjbCIPxHD7z2J9c91qbFm9eh3rG8lwdytTe0mIx1rZzOnc";
            this.KART_KALA.Connecting += new System.EventHandler(this.GetKART_KALA_SqlCommand);
        }
        
        public void GetKART_KALA_SqlCommand(object sender, System.EventArgs e)
        {
            this.KART_KALA.SqlCommand = "SELECT NAME, CODE, NAMES, N_FANI, MEGK, mabl_a, MABLK, MEG, BARGAH, NUMBER, FNUMCO, DATE_N, BEDNAME, avrage, TAG, MABM, MEGKM\r\n\tFROM dbo.KART_KALA(@AZDATE, @ANBAR, @TADATE)\r\nWHERE CODE=@KALACODE\r\nORDER BY CONVERT(BIGINT, CODE), DATE_N, BARGAH, NUMBER";
        }
        
        // CheckerInfo: *None* *DataSources*
        #region DataSource KART_KALA
        public class KART_KALADataSource : Stimulsoft.Report.Dictionary.StiSqlSource
        {
            
            public KART_KALADataSource() : 
                    base("MS SQL", "KART_KALA", "KART_KALA", "", true, false, 30, "d9ce18e660b343baa8fdf1388b9e58bc")
            {
            }
            
            public virtual string NAME
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["NAME"], typeof(string), true)));
                }
            }
            
            public virtual string CODE
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["CODE"], typeof(string), true)));
                }
            }
            
            public virtual string NAMES
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["NAMES"], typeof(string), true)));
                }
            }
            
            public virtual string N_FANI
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["N_FANI"], typeof(string), true)));
                }
            }
            
            public virtual double MEGK
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["MEGK"], typeof(double), true)));
                }
            }
            
            public virtual double mabl_a
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["mabl_a"], typeof(double), true)));
                }
            }
            
            public virtual double MABLK
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["MABLK"], typeof(double), true)));
                }
            }
            
            public virtual decimal MEG
            {
                get
                {
                    return ((decimal)(StiReport.ChangeType(this["MEG"], typeof(decimal), true)));
                }
            }
            
            public virtual string BARGAH
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["BARGAH"], typeof(string), true)));
                }
            }
            
            public virtual double NUMBER
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["NUMBER"], typeof(double), true)));
                }
            }
            
            public virtual double FNUMCO
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["FNUMCO"], typeof(double), true)));
                }
            }
            
            public virtual long DATE_N
            {
                get
                {
                    return ((long)(StiReport.ChangeType(this["DATE_N"], typeof(long), true)));
                }
            }
            
            public virtual string BEDNAME
            {
                get
                {
                    return ((string)(StiReport.ChangeType(this["BEDNAME"], typeof(string), true)));
                }
            }
            
            public virtual decimal avrage
            {
                get
                {
                    return ((decimal)(StiReport.ChangeType(this["avrage"], typeof(decimal), true)));
                }
            }
            
            public virtual double TAG
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["TAG"], typeof(double), true)));
                }
            }
            
            public virtual double MABM
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["MABM"], typeof(double), true)));
                }
            }
            
            public virtual double MEGKM
            {
                get
                {
                    return ((double)(StiReport.ChangeType(this["MEGKM"], typeof(double), true)));
                }
            }
        }
        #endregion DataSource KART_KALA
        #endregion StiReport Designer generated code - do not modify
    }
}
