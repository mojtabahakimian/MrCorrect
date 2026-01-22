using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Maps;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinMenus.Taarif;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    public partial class WIN_PRICE_GRP : Window
    {
        public WIN_PRICE_GRP()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<PRICE_GRP> DG_DATA { get; set; } = new ObservableCollection<PRICE_GRP>();

        // State Variables
        public PRICE_GRP? WAS_ROW_ITEM { get; private set; } = new();
        public bool IsSelectorMode { get; private set; } = false;
        public int? SelectedPriceGroupID { get; private set; } = null;
        public bool NowIsReady { get; private set; }

        public WIN_PRICE_GRP(bool isSelector = false)
        {
            InitializeComponent();
            this.DataContext = this;
            this.IsSelectorMode = isSelector;

            if (IsSelectorMode)
            {
                LABEL_HEADER.Content = "انتخاب گروه قیمت گذاری";
                DG_SUB.IsReadOnly = true; // Lock editing in selector mode
                BTN_DELETE.Visibility = Visibility.Collapsed;
            }
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var windowId = new WindowInteropHelper(this).Handle;

            // Security Check
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "CUST", windowId, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            DG_SUB_ReGetData();

            navigationControl.ReGetDataAction = () => //Realod Data
            {
                DG_SUB_ReGetData();
            };
            navigationControl.NewRecordAction = () => //Go Focus New Row Cell
            {
                navigationControl.GoNewPlaceholder(DG_SUB, PGNAME_COLUMN);
            };

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Selector Mode Logic (Enter to select)
            if (e.Key == Key.Enter && IsSelectorMode && DG_SUB.SelectedItem is PRICE_GRP selectedItem)
            {
                e.Handled = true;
            }

            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                DataGridExtension.HandleKeyPress(sender, e, DG_SUB);
            }
        }

        public void DG_SUB_ReGetData()
        {
            try
            {
                var list = dbms.DoGetDataSQL<PRICE_GRP>("SELECT * FROM PRICE_GRP ORDER BY PGID");
                DG_DATA.Clear();
                foreach (var item in list)
                {
                    DG_DATA.Add(item);
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری اطلاعات").Show();
            }
        }

        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e)
        {
            DG_SUB_ReGetData();
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelectorMode) return;

            if (DG_SUB.SelectedItem is not PRICE_GRP item || item.PGID == 0) return;

            if (DG_DATA.Count > 0)
            {
                if (DG_SUB.IsReadOnly) return;

                var hasErrors =
                    (from object i in DG_SUB.ItemsSource
                     let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                     where c != null && Validation.GetHasError(c)
                     select c).Any();

                if (hasErrors) return;

                try
                {
                    var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(DG_SUB.ItemsSource);
                    if (view.IsAddingNew && view.CanCancelEdit)
                    {
                        view.CancelNew();
                        return;
                    }
                    else if (view.IsEditingItem && view.CanCancelEdit)
                    {
                        view.CancelEdit();
                        return;
                    }
                    else
                    {
                        DG_SUB.CommitEdit(DataGridEditingUnit.Cell, true);
                        DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                    }
                }
                catch { }
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف این گروه قیمت اطمینان دارید؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true) return;

            _ = AuditLogger.LogActionAsync(
                actionType: "DELETE",
                tableName: "تعریف گروه قیمت گذاری",
                recordId: DG_SUB.SelectedItem.ToStringNullSafe(),
                oldValue: null,
                newValue: null,
                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


            try
            {
                dbms.DoExecuteSQL("DELETE FROM PRICE_GRP WHERE PGID = @PGID", new { PGID = item.PGID });
                DG_DATA.Remove(item);
                universControl.PopNotifyShow("آیتم با موفقیت حذف شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // FK Constraint
                    new Msgwin(false, "این گروه در سیستم استفاده شده و قابل حذف نیست").Show();
                else
                    new Msgwin(false, "خطا در حذف اطلاعات").Show();
            }
        }

        #region DataGrid Edit Logic

        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (IsSelectorMode) { e.Cancel = true; return; }
            if (e.Row.Item is PRICE_GRP rowItem && rowItem != null)
            {
                if (!(e is null) && DG_SUB.SelectedItem is not null)
                {
                    if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((PRICE_GRP)DG_SUB.SelectedItem).Clone() as PRICE_GRP;
                    }
                }
            }
        }

        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Standard cleanup if needed
        }

        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) return;
            if (IsSelectorMode) return;

            var row = e.Row.Item as PRICE_GRP;
            if (row == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(row.PGNAME))
            {
                universControl.PopNotifyShow("عنوان نمی‌تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                e.Cancel = true;
                return;
            }

            // Fill Audit Fields
            row.TR_DATE = DateTime.Now;
            // Assuming Baseknow.USERNAME exists as per project context
            row.USERNAME = Baseknow.UUSER ?? "Unknown";
            row.UID = Baseknow.USERCOD;

            try
            {
                if (row?.PGID == null || row?.PGID == 0) // INSERT
                {
                    var maxId = dbms.DoGetDataSQL<int?>("SELECT MAX(PGID) FROM PRICE_GRP").FirstOrDefault();
                    row.PGID = (maxId ?? 0) + 1;
                    row.CRT = DateTime.Now;

                    string insertSql = @"
                        INSERT INTO PRICE_GRP (PGID, PGNAME, TR_DATE, USERNAME, CRT, UID)
                        VALUES (@PGID, @PGNAME, @TR_DATE, @USERNAME, @CRT, @UID)";

                    dbms.DoExecuteSQL(insertSql, row);
                }
                else // UPDATE
                {
                    // Check if actually changed
                    if (WAS_ROW_ITEM != null && WAS_ROW_ITEM.PGNAME == row?.PGNAME)
                        return;

                    string updateSql = @"
                        UPDATE PRICE_GRP 
                        SET PGNAME = @PGNAME, TR_DATE = @TR_DATE, USERNAME = @USERNAME 
                        WHERE PGID = @PGID";

                    dbms.DoExecuteSQL(updateSql, row);
                }

                universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (Exception ex)
            {
                e.Cancel = true; // Keep row in edit mode
                new Msgwin(false, "خطا در ذخیره اطلاعات").Show();
            }
        }

        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && !IsSelectorMode)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
        }

        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Helper to handle click selection if needed
        }

        #endregion

        private void DG_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectorMode && DG_SUB.SelectedItem is PRICE_GRP selectedItem)
            {
                SelectedPriceGroupID = selectedItem.PGID;

                if (selectedItem.PGID > 0)
                {
                    var ExistingCodeKala = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 CODE FROM dbo.STUF_DEF WHERE PGID = {selectedItem.PGID}").FirstOrDefault();
                    if (ExistingCodeKala != null)
                    {
                        new STUF_DEF_WIN(ExistingCodeKala).Show();
                    }
                }
                e.Handled = true;
            }
        }
    }
}
