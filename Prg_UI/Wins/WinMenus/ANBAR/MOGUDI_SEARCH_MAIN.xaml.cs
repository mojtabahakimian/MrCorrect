using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Wins.WinMenus.ANBAR;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class MOGUDI_SEARCH_MAIN : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        //List<Q8> ItemsDGRDefalt = new List<Q8>();
        public string DefaultValue { get; set; }
        public MOGUDI_SEARCH_MAIN()
        {
            InitializeComponent();
            this.Owner = PublicVRB.WINBASE;
        }
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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
        public class A111
        {
            public int? CODE { get; set; }
            public string NAMES { get; set; }
        }
        public class A222
        {
            public int? CODE { get; set; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (!CL_HESABDARI.LETSGO("SEARCHMO"))
            {
                this.Close();
                return;
            }
            //Forms["baseknow"]["hhwin"] = this.hwnd;
            this.mogudi_search.Visibility = Visibility.Hidden;

            ANBAR.ItemsSource = dbms.DoGetDataSQL<A111>("SELECT     CODE, NAMES FROM         TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = " + Baseknow.USERCOD + ")))").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";


            //CANBAR.ItemsSource = dbms.DoGetDataSQL<A222>("SELECT CODE FROM TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = " + Baseknow.USERCOD + ")))").ToList();
            CANBAR.ItemsSource = ANBAR.ItemsSource;
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";


            if (Strings.Mid(Baseknow.OPTIONSS, 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").FirstOrDefault();
                if (!ReferenceEquals(rst, null))
                {
                    //rst.Fields(0);
                    this.ANBAR.SelectedValue = rst;
                    this.CANBAR.SelectedValue = rst;
                }
            }
            //DoCmd.Maximize();
            //Forms["baseknow"]["hhwin"] = this.hwnd;
            //if (Strings.Mid(Forms["baseknow"]["OPTIONSS"], 44, 1) == "5")
            //{
            //    if (this.WindowWidth > 15480)
            //    {
            //        this.Width = this.WindowWidth - 100;
            //        this.mogudi_search.Left = 50;
            //        this.mogudi_search.Width = this.Width - 200;
            //        this.Detail.Height = this.WindowHeight - 200;
            //        this.mogudi_search.Height = this.Detail.Height - 2000;
            //        // Me.ANBAR.Left = 0.5
            //        // Me.CANBAR.Left = Me.ANBAR.Left + Me.ANBAR.Width + 0.3
            //        // Me.Label4.Left = Me.CANBAR.Left + Me.CANBAR.Width + 0.2
            //    }
            //    this.Repaint();
            //}

            //this.InvalidateVisual();
            //this.UpdateLayout();
            ANBAR.SelectedIndex = 0;
            mogudi_search.ItemsSource = dbms.DoGetDataSQL<Q8>(" SELECT     CODE, MAND, ANBAR, ANBARN,  NAME, NAMES, VCOD, GRCOD, GRNAME, MANDF, N_FANI, NESBAT, MEGHBAR, bsef, nsef, maxm, minm, VAZN, VAZNK,menuit,MABL_F,B_SEF,fisiclymand,MAX_M,MEGHRES FROM         dbo.AKMOGUDI_KOL_ANBAR(99999999," + ANBAR.SelectedValue + ") AKMOGUDI_KOL_ANBAR ").ToList();
            this.mogudi_search.Visibility = Visibility.Visible;

            set_box_josteju.Focus();
        }

        private void set_box_josteju_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((bool)ONS.IsChecked)
            {
                string SHAR;
                if (string.IsNullOrEmpty(set_box_josteju.Text))
                {
                    SHAR = "";

                }
                else
                {
                    SHAR = CL_HESABDARI.SHARTCRATOR(set_box_josteju.Text);
                }
                mogudi_search.ItemsSource = dbms.DoGetDataSQL<Q8>(" SELECT     CODE, MAND, ANBAR, ANBARN,  NAME, NAMES, VCOD, GRCOD, GRNAME, MANDF, N_FANI, NESBAT, MEGHBAR, bsef, nsef, maxm, minm, VAZN, VAZNK,menuit,MABL_F,B_SEF,fisiclymand,MAX_M,MEGHRES FROM         dbo.AKMOGUDI_KOL_ANBAR(99999999," + this.ANBAR.SelectedValue + ") AKMOGUDI_KOL_ANBAR where " + Interaction.IIf(string.IsNullOrEmpty(SHAR), " NAME LIKE '%' ", SHAR)).ToList();
            }
        }

        private void set_box_josteju_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string SHAR;
            if (string.IsNullOrEmpty(set_box_josteju.Text))
            {
                SHAR = "";
            }
            else
            {
                SHAR = CL_HESABDARI.SHARTCRATOR(set_box_josteju.Text);
            }
            mogudi_search.ItemsSource = dbms.DoGetDataSQL<Q8>(" SELECT     CODE, MAND, ANBAR, ANBARN,  NAME, NAMES, VCOD, GRCOD, GRNAME, MANDF, N_FANI, NESBAT, MEGHBAR, bsef, nsef, maxm, minm, VAZN, VAZNK,menuit,MABL_F,B_SEF,fisiclymand,MAX_M,MEGHRES FROM         dbo.AKMOGUDI_KOL_ANBAR(99999999," + this.ANBAR.SelectedValue + ") AKMOGUDI_KOL_ANBAR where " + Interaction.IIf(string.IsNullOrEmpty(SHAR), " NAME LIKE '%' ", SHAR)).ToList();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var focusedControl = FocusManager.GetFocusedElement(this);
                if (focusedControl is DataGridCell || ((FrameworkElement)focusedControl).Parent is DataGridCell)
                {
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void mogudi_search_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mogudi_search.Items.Count > 0)
            {
                if (mogudi_search.SelectedItem is not null)
                {
                    var row = mogudi_search.SelectedItem as Q8;
                    if (row?.ANBAR != null && !string.IsNullOrEmpty(row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", row.ANBAR.ToString(), row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void mogudi_search_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null) return;

            try
            {
                // Find the row under the mouse
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;
                if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
                {
                    if (dataGrid?.SelectedItem != null)
                    {
                        // Select the row under the mouse
                        dataGrid.SelectedItem = row.Item;
                    }
                    if (dataGrid?.ContextMenu != null)
                    {
                        // Show the context menu
                        dataGrid.ContextMenu.IsOpen = true;
                    }

                    // Mark the event as handled to prevent the default context menu behavior
                    e.Handled = true;
                }
                else
                {
                    // No valid row, don't show context menu
                    var isEditing = ((IEditableCollectionView)mogudi_search.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }
    }
}
