using MaterialDesignThemes.Wpf;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Stimulsoft.Base;
using Stimulsoft.Report.Components.Table;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prg_Proccessy.FUNCTIONS;

namespace Wins.WinMenus.ANBAR
{
    public partial class F_MENU_KART : Window
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
        public string FT { get; set; }
        public string MANBAR { get; set; }
        public string DTT { get; set; }
        public string MKALA { get; set; }

        public class _MODEL_F_MENU_KART
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
            public string? Expr1 { get; set; }
        }
        public class _MODEL_ANBAR
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public class FMM1
        {
            public int? CODE { get; set; }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public string OpenArgs { get; set; }
        public string ANBARCODE { get; set; }
        public string KALACODE { get; set; }
        public F_MENU_KART(string? openargs = null, string _ANBARCODE_ = null, string _KALACODE_ = null)
        {
            InitializeComponent();

            OpenArgs = openargs;


            if (_ANBARCODE_ is not null)
            {
                ANBARCODE = _ANBARCODE_;
            }
            if (_KALACODE_ is not null)
            {
                KALACODE = _KALACODE_;
            }
        }
        public string THE_RPT_NAME { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CheckPermistion();
            Fill_ComboBoxes();
            DT2.Text = Tarikh.FullCurrentDate;
            CANBAR.Focus();
        }

        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<_MODEL_ANBAR>("SELECT CODE, NAMES FROM TCOD_ANBAR GROUP BY CODE, NAMES HAVING (CODE <> 0) ORDER BY NAMES").ToList();
            ANBAR.DisplayMemberPath = "NAMES";
            ANBAR.SelectedValuePath = "CODE";

            CANBAR.ItemsSource = dbms.DoGetDataSQL<FMM1>("SELECT CODE FROM TCOD_ANBAR GROUP BY CODE HAVING (CODE <> 0)").ToList();
            CANBAR.DisplayMemberPath = "CODE";
            CANBAR.SelectedValuePath = "CODE";

            //var MasterAK = dbms.DoGetDataSQL<_MODEL_F_MENU_KART>("SELECT  STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE AS Expr1 FROM   STUF_DEF INNER JOIN  STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (STUF_FSK.ANBAR = " + ANBAR.SelectedValue + ") GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            var MasterAK = dbms.DoGetDataSQL<_MODEL_F_MENU_KART>("SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE AS Expr1 FROM STUF_DEF INNER JOIN STUF_FSK ON STUF_DEF.CODE = STUF_FSK.CODE WHERE (STUF_FSK.ANBAR = 1) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE ORDER BY STUF_DEF.NAME").ToList();
            KALA.DisplayMemberPath = "NAME";
            KALA.SelectedValuePath = "CODE";
            KALA.ItemsSource = MasterAK; KALA.SelectedIndex = -1; KALA.Items.Refresh();
        }

        private void CheckPermistion()
        {
            bool HasAccess = false;
            if (CL_HESABDARI.LETSGO("KARTR", this.GetType().Name))
            {
                HasAccess = true;
                //DoCmd.OpenForm "F_MENU_KART";
                //Forms["F_MENU_KART"]["ANBAR"] = this.ANBAR;
                //DoCmd.OpenReport "R_KA_KALA", acViewPreview,, "code = '" + this.CODE + "'";
                //DoCmd.OpenForm "F_MENU_KART",,,,, acHidden;
                THE_RPT_NAME = "R_KA_KALA";
            }
            else
            {
                if (CL_HESABDARI.LETSGO("KARTR2"))
                {
                    HasAccess = true;
                    //DoCmd.OpenForm "F_MENU_KART";
                    //Forms["F_MENU_KART"]["ANBAR"] = this.ANBAR;
                    //DoCmd.OpenReport "R_KA_KALA2", acViewPreview,, "code = '" + this.CODE + "'";
                    //DoCmd.OpenForm "F_MENU_KART";
                    THE_RPT_NAME = "R_KA_KALA2";
                }
            }

            if (!HasAccess)
            {
                new Msgwin(false, "شما به کارت انبار دسترسی ندارید!").ShowDialog();
                this.Close();
            }

        }

        public void ExternalCallShowReport()
        {
            CheckPermistion();

            if (ANBARCODE != null && KALACODE != null)
            {
                ANBAR.SelectedValue = ANBARCODE;
                KALA.SelectedValue = KALACODE;

                Command5_Click(null, null);
            }
        }
        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DT2.Text.ToRawTarikh()) || ANBAR.SelectedValue is null || KALA.SelectedValue is null)
            {
                new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                return;
            }
            if (ANBAR.SelectedValue is null)
            {
                MANBAR = "%";
            }
            else
            {
                MANBAR = ANBAR.SelectedValue.ToString();
            }

            if (KALA.SelectedValue is null)
            {
                MKALA = "%";
            }
            else
            {
                MKALA = KALA.SelectedValue.ToString();
            }

            switch (OpenArgs.ToString())
            {
                case "R": { THE_RPT_NAME = "R_KA_KALA"; OpenReport(); /*R_KA_KALA*/ break; }
                case "KARTR2": { THE_RPT_NAME = "R_KA_KALA2"; OpenReport(); /*R_KA_KALA2*/ break; }
                    //case "F": { /*AK_MOGUDI_ANBAR_LIST*/ break; }
            }

        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            //var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.{THE_RPT_NAME}.mrt");
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.{THE_RPT_NAME}.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["AZDATE"] = Baseknow.YEA + "0101";
            report["ANBAR"] = ANBAR.SelectedValue.ToString();

            string TaTarikh = "99999999";
            if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            report["TADATE"] = TaTarikh;

            report["KALACODE"] = KALA.SelectedValue.ToString();
            ((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;

            if (THE_RPT_NAME != "KARTR2")
            {
                //Report_Open:
                var _ONE_ = (report.GetComponentByName("Table1_Cell8") as StiTableCell); //موجودی
                if (_ONE_ != null)
                {
                    (report.GetComponentByName("Table1_Cell8") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK
                }  
                var _SECOND_ = (report.GetComponentByName("Table1_Cell9") as StiTableCell); //موجودی
                if (_SECOND_ != null)
                {
                    (report.GetComponentByName("Table1_Cell9") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK
                }
            }

            //report.Render();
            //report.Show();
            //
            //pathreport?.Dispose();

            new Rpts.WINRPT(report, "کارت انبار کالا").Show();
        }

        private void CANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ANBAR.SelectedValue = CANBAR.SelectedValue;
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            CANBAR.SelectedValue = ANBAR.SelectedValue;
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
