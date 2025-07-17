using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Rpts;
using Prg_UI.Wins.WinSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    public partial class BUGET_MAIN_list2 : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public static int BGID_tahlil_list = 0;

        #region Header Window Begin
        //Header Window Begin
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

        private static void SelectRowByIndex(DataGrid dataGrid, int rowIndex)
        {
            if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.FullRow))
                throw new ArgumentException("The SelectionUnit of the DataGrid must be set to FullRow.");

            if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
                throw new ArgumentException(string.Format("{0} is an invalid row index.", rowIndex));

            dataGrid.SelectedItems.Clear();
            object item = dataGrid.Items[rowIndex];
            dataGrid.SelectedItem = item;

            DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row == null)
            {
                /* bring the data item (Product object) into view
                 * in case it has been virtualized away */
                dataGrid.ScrollIntoView(item);
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }
            if (row != null)
            {
                DataGridCell cell = GetCell(dataGrid, row, 0);
                if (cell != null)
                    cell.Focus();
            }
        }
        private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow rowContainer, int column)
        {
            if (rowContainer != null)
            {
                System.Windows.Controls.Primitives.DataGridCellsPresenter presenter
                    = FindVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    /* if the row has been virtualized away, call its ApplyTemplate() method 
                     * to build its visual tree in order for the DataGridCellsPresenter
                     * and the DataGridCells to be created */
                    rowContainer.ApplyTemplate();
                    presenter = FindVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);
                }
                if (presenter != null)
                {
                    DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell == null)
                    {
                        /* bring the column into view
                         * in case it has been virtualized away */
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }
            return null;
        }
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        private List<BUGET_MAIN> GetAllDataBDG()
        {
            List<BUGET_MAIN> Dt = new List<BUGET_MAIN>();
            Dt = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT * FROM BUGET_MAIN ORDER BY BGID").ToList();
            return Dt;
        }

        public BUGET_MAIN_list2()
        {
            InitializeComponent();
            preloader.Visibility = Visibility.Visible;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            preloader.Visibility = Visibility.Collapsed;
        }
        private void cnnparamlbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinConnectionChoose chos3cnn = new WinConnectionChoose();
            chos3cnn.ShowDialog();
        }
        private void DGR_ProcessBudget_Loaded(object sender, RoutedEventArgs e)
        {
            DGR_ProcessBudget.ItemsSource = GetAllDataBDG();
        }
        private void DGR_ProcessBudget_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DGR_ProcessBudget.SelectedItem != null)
            {
                //BUGET_MAIN member = new BUGET_MAIN();
                //foreach (var obj in DGR_ProcessBudget.SelectedItems)
                //{
                //    member = obj as BUGET_MAIN;
                //}
                if (e.Key == Key.Enter)
                {
                    BUGET_MAIN BDGMAIN = (BUGET_MAIN)DGR_ProcessBudget.SelectedItem;
                    BGID_tahlil_list = (int)BDGMAIN.BGID;

                    WinReportBudgettahlil winReportBudgettahlil = new WinReportBudgettahlil();
                    winReportBudgettahlil.ShowDialog();
                    //SelectRowByIndex(DGR_ProcessBudget, DGR_ProcessBudget.Items.Count - 1);//Go End Of Row

                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var Test = btn.Tag.ToString();
                BGID_tahlil_list = Convert.ToInt32(btn.Tag);

                WinReportBudgettahlil winReportBudgettahlil = new WinReportBudgettahlil();
                winReportBudgettahlil.ShowDialog();
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
