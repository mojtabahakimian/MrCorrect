using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    public partial class WIN_VISITSELECT : Window
    {
        public bool NowIsReady { get; private set; }

        public ObservableCollection<VISISELECT_MODEL> VISISELECT_DATA { get; } = new ObservableCollection<VISISELECT_MODEL>();
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

        public VISISELECT_MODEL SelectedVisit { get; set; }
        public WIN_VISITSELECT(string HES_PARAM)
        {
            InitializeComponent();

            OpenArgs = HES_PARAM;

            this.DataContext = this;
        }
        public string OpenArgs { get; set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);


            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };

            FocusOnFirstCellRow();
        }

        private void ReGetData()
        {
            VISISELECT_DATA?.Clear();

            var RST = dbms.DoGetDataSQL<VISISELECT_MODEL>($@"SELECT ROUTE_NAME,
                                                                   HES,
                                                                   IYALAT,
                                                                   CITY,
                                                                   District,
                                                                   RACTIVE,
                                                                   NAME
                                                            FROM
                                                            (
                                                                SELECT dbo.Visit_route.ROUTE_NAME,
                                                                       dbo.Visit_route.HES,
                                                                       dbo.Visit_route.IYALAT,
                                                                       dbo.Visit_route.CITY,
                                                                       dbo.Visit_route.District,
                                                                       dbo.Visit_route.RACTIVE,
                                                                       dbo.CUST_HESAB.NAME
                                                                FROM dbo.Visit_route
                                                                    INNER JOIN dbo.CUST_HESAB
                                                                        ON dbo.Visit_route.HES = dbo.CUST_HESAB.hes
                                                            ) AS DRVD_TBL
                                                            WHERE (HES = @HES) ", new { HES = OpenArgs }).ToList(); //AND (RACTIVE = 1);
            foreach (var item in RST)
            {
                VISISELECT_DATA?.Add(item);
            }
        }
        private void FocusOnFirstCellRow()
        {
            try
            {
                //برای انتخاب سطر اول که وقتی توی پایین میان با کلید های جهتی نره ستون بعد بره ردیف بعدی
                if (DG_SUB.Items.Count > 0)
                {
                    var LAST_ROW = 0;
                    var col_index = DG_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "ROUTE_NAME").DisplayIndex;

                    DG_SUB.ScrollIntoView(DG_SUB.Items[LAST_ROW]);
                    DG_SUB.SelectedIndex = LAST_ROW;
                    DG_SUB.Focus();

                    // Use the Dispatcher to wait for the UI to render
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        DataGridRow row = (DataGridRow)DG_SUB.ItemContainerGenerator.ContainerFromIndex(LAST_ROW);
                        if (row != null)
                        {
                            DataGridCell cell = CL_LMethods.GetCell(DG_SUB, row, Convert.ToInt32(col_index));
                            if (cell != null)
                                cell.Focus();
                        }
                    }));
                }
            }
            catch { }

        }
        private void DG_SUB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DG_SUB.Items.Count > 0 && DG_SUB.SelectedItem != null)
            {
                e.Handled = true;
                SelectAndCloseWindow();
            }
        }

        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DG_SUB.Items.Count > 0 && DG_SUB.SelectedItem != null)
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

        private void SelectAndCloseWindow()
        {
            if (DG_SUB.SelectedItem != null && DG_SUB.SelectedItem is VISISELECT_MODEL SM)
            {
                SelectedVisit = SM;

                Close();
            }
        }
    }
}
