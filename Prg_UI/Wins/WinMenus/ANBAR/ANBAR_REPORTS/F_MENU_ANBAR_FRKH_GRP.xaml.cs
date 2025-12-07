using AUTO_BAZ.HelperWins;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_UI.Functions.CL_LMethods;
using System.Diagnostics;
using AUTO_BAZ.Functions;
using CL_LMethods = Prg_UI.Functions.CL_LMethods;
using Rpts;

namespace Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR_FRKH_GRP.xaml
    /// </summary>
    public partial class F_MENU_ANBAR_FRKH_GRP : Window
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

        public F_MENU_ANBAR_FRKH_GRP()
        {
            InitializeComponent();
        }

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

        public class Q2
        {
            public int? CODE { get; set; }
        }

        public class Q3
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        UniversControl universControl = new UniversControl();

        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR GROUP BY TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES ORDER BY TCOD_ANBAR.NAMES;").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            CANBAR.ItemsSource = dbms.DoGetDataSQL<Q2>($"SELECT TCOD_ANBAR.CODE FROM TCOD_ANBAR GROUP BY TCOD_ANBAR.CODE;").ToList();
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Fill_ComboBoxes();

            CANBAR.Focus();
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

        private void F1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (F1.Text is not null)
            {
                try
                {
                    Convert.ToInt32(F1.Text);
                }
                catch (Exception)
                {

                    universControl.PopNotifyShow("لطفا در (از انبار) فقط مقدار عددی وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                }
            }
        }

        private void F2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (F2.Text is not null)
            {
                try
                {
                    Convert.ToInt32(F2.Text);
                }
                catch (Exception)
                {

                    universControl.PopNotifyShow("لطفا در (به انبار) فقط مقدار عددی وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                }
            }
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

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string SHART;
            string PATH;
            int i;
            if (IsNull(this.F2.Text) || IsNull(this.F1.Text) || IsNull(this.ANBAR.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "پارامتر ها کافی نیست!");
                msgwin.ShowDialog();
                return;
            }
            SHART = "INVO_LST.NUMBER = ";

            if (!string.IsNullOrEmpty(this.VF.Text))
            {
                for (i = 0; i < this.VF.Text.Length; i++)
                {
                    // Append numeric characters from VF.Text to SHART
                    while (i < this.VF.Text.Length && char.IsDigit(this.VF.Text[i]))
                    {
                        SHART += this.VF.Text[i];
                        i++;
                    }

                    // Check the last 22 characters to see if "or INVO_LST.NUMBER =" is not already there
                    if (SHART.Trim().Length < 22 || SHART.Trim().Substring(SHART.Trim().Length - 22) != "or INVO_LST.NUMBER =")
                    {
                        SHART += " or INVO_LST.NUMBER = ";
                    }
                }

                // Remove the last "or INVO_LST.NUMBER =" if it wasn't followed by a number
                if (SHART.Trim().Length >= 22 && SHART.Trim().Substring(SHART.Trim().Length - 22) == "or INVO_LST.NUMBER =")
                {
                    SHART = SHART.Substring(0, SHART.Length - 22);
                }
            }
            else
            {
                SHART = "";
            }
            Open_Report();
        }

        public void Open_Report()
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.LIST_FROOSH_ANBARS_HAVALA.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));
            //Parameters

            var Saman_Name = dbms.DoGetDataSQL<string>("SELECT NAME FROM SAZMAN").FirstOrDefault();
            (report.GetComponentByName("TRNAME") as StiText).Text = Saman_Name.ToString();
            (report.GetComponentByName("DDATE") as StiText).Text = Tarikh.FullCurrentDate;


            report["ANBAR_TAG_PARM"] = 2;
            if (!String.IsNullOrEmpty(F1.Text) && !String.IsNullOrEmpty(F2.Text))
            {
                report["FNUMBER_PARM"] = Convert.ToInt32(F1.Text);
                report["SNUMBER_PARM"] = Convert.ToInt32(F2.Text);
            }

            report["TNUMBER_PARM"] = Convert.ToInt32(ANBAR.SelectedValue);

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            new WINRPT(report, "گزارش حواله انبار گروهی").Show();
            //report.Show();
        }
    }
}
