using AUTO_BAZ.HelperWins;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR_MERG.xaml
    /// </summary>
    public partial class F_MENU_ANBAR_MERG : Window
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

        public F_MENU_ANBAR_MERG()
        {
            InitializeComponent();
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

        public string SQL_DATA { get; set; }

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

        UniversControl universControl = new UniversControl();

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

        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR GROUP BY TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES ORDER BY TCOD_ANBAR.NAMES;").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            CANBAR.ItemsSource = dbms.DoGetDataSQL<Q2>($"SELECT TCOD_ANBAR.CODE FROM TCOD_ANBAR GROUP BY TCOD_ANBAR.CODE;").ToList();
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string SHART;
            string PATH;
            int i;
            if (string.IsNullOrEmpty(this.F2.Text) || string.IsNullOrEmpty(this.F1.Text) || IsNull(this.ANBAR.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "پارامترها کافی نیست!");
                msgwin.ShowDialog();
                return;
            }
            SHART = "(ANBAR = " + this.ANBAR.SelectedValue + " OR ANBAR BETWEEN " + this.F1.Text.ToRawTarikh() + " AND " + this.F2.Text.ToRawTarikh() + ")";
            SQL_DATA = $"SELECT CODE, GRCOD, GRNAME, KK, SUM(MABLK) AS MABLK, SUM(MAND) AS MAND, SUM(MANDF) AS MANDF, N_FANI, NAME, NAMES, NESBAT, TOZIH, VAZN, SUM(VAZNK) AS VAZNK, VCOD, AVG(FII) AS FII FROM AKMOGUDI_KOL_ANBAR(99999999, '%') AKMOGUDI_KOL_ANBAR WHERE {SHART} GROUP BY CODE, GRCOD, GRNAME, KK, N_FANI, NAME, NAMES, NESBAT, TOZIH, VCOD, VAZN ";
            // Additional conditions based on VF.Text
            if (!string.IsNullOrEmpty(this.VF.Text))
            {
                for (i = 0; i < this.VF.Text.Length; i++)
                {
                    // Check the last 11 characters to see if "or ANBAR =" is not already there
                    if (SHART.Trim().Length < 11 || SHART.Trim().Substring(SHART.Trim().Length - 11) != "or ANBAR =")
                    {
                        SHART += " or ANBAR =";
                    }

                    // Append numeric characters from VF.Text to SHART
                    while (i < this.VF.Text.Length && char.IsDigit(this.VF.Text[i]))
                    {
                        SHART += this.VF.Text[i];
                        i++;
                    }
                }

                // Remove the last "or ANBAR =" if it wasn't followed by a number
                if (SHART.Trim().Length >= 11 && SHART.Trim().Substring(SHART.Trim().Length - 11) == "or ANBAR =")
                {
                    SHART = SHART.Substring(0, SHART.Length - 11);
                }


            }
            new AK_MOGUDI_ANBAR_LIST_MERG(SQL_DATA).Show();
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
    }
}
