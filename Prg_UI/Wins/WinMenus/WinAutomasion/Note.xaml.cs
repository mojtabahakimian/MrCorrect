using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY.VISITORS_DTL_KALA;

namespace Prg_UI.Wins.WinMenus.WinAutomasion
{
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class Note : Window
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
        public Note()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }


        public bool NOTE_SUB_IsFocused { get; private set; }

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                return _newrecord;
            }
            set { _newrecord = value; }
        }
        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int NOTE_SUB_DEF_INDEX_COL
        {
            get
            {
                if (NOTE_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = NOTE_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "Ndone")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        datagridname_tbox_def_index_col = 0;
                    }
                    else
                    {
                        datagridname_tbox_def_index_col = (int)defaultcolumnindex;
                    }
                }
                return datagridname_tbox_def_index_col;
            }
        }
        public string? ENTERED_VALUE_ROW { get; private set; }

        public Visual I_AM_NOTE { get; private set; }
        public NOTES? CURRENT_ROW_ITEMS { get; private set; }
        public NOTES? WAS_ROW_ITEM { get; private set; } = new NOTES();

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

        public ObservableCollection<NOTES> NOTE_DATA { get; set; } = new ObservableCollection<NOTES>();


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NOTE_SUB_ReGetData();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {

                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);

            }
        }

        public void NOTE_SUB_ReGetData()
        {
            if (!NewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<NOTES>(@$"SELECT Notes.* FROM Notes WHERE (Ndone = 0)").ToList();

                NOTE_DATA?.Clear();
                foreach (var item in QRE_LST)
                    NOTE_DATA?.Add(item);
            }
        }

        private void NOTE_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.EditingElement == null || e.Column == null)
            {
                return;
            }

            ComboBox Comboval = null; TextBox TexboVal = null; CheckBox? CheckVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
            {
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text?.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as NOTES;
            if (CURRENT_ROW_ITEMS == null)
            {
                return;
            }

            if (!(e.EditingElement is null))
            {
                CheckVal = e.EditingElement as CheckBox;
            }

            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                ENTERED_VALUE_ROW = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();
        }

        private void NOTE_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void NOTE_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void NOTE_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                return;
            }

            // گرفتن آیتم (رکورد) که ویرایش شده است
            NOTES editedItem = e.Row.Item as NOTES;
            if (editedItem == null)
            {
                return;
            }

            NOTES NTM = new NOTES
            {
                Note = editedItem.Note,
                Ndate = Tarikh.FullCurrentDate,
                Ntime = DateTime.Now,
                userid = Baseknow.USERCOD,
                Ndone = editedItem.Ndone,
                idd = editedItem?.idd
            };

            try
            {
                if (editedItem.idd == null || editedItem.idd == 0)
                {
                    // عملیات INSERT برای رکورد جدید
                    string DATE = Tarikh.FullCurrentDate;

                    var insertedrowid = dbms.DoGetDataSQL<int?>($@"SET NOCOUNT OFF;
                        INSERT INTO Notes (
                            Note,
                            Ndate,
                            Ntime,
                            userid,
                            Ndone
                        )
                        VALUES (
                            @Note,
                            @Ndate,
                            @Ntime,
                            @userid,
                            @Ndone
                        );                        
                        SELECT SCOPE_IDENTITY() AS SCOPE_ID_COLUMN;", NTM).FirstOrDefault();

                    if (insertedrowid != null)
                    {
                        editedItem.idd = insertedrowid;
                    }

                }
                else
                {
                    // عملیات UPDATE برای رکورد موجود
                    dbms.DoExecuteSQL(@$"UPDATE Notes
                                                SET Ndone = @Ndone , Note = @Note
                                                WHERE idd = @idd AND userid = @userid ;", NTM);

                }
            }
            catch (Exception ex)
            {
                Msgwin msgwin = new Msgwin(false, "خطا در ذخیره سازی!");
                msgwin.ShowDialog();
            }
            finally
            {
                NOTE_SUB_ReGetData();
            }
        }

        private void NOTE_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NOTE_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is TOZIE_SUB rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = NOTE_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as NOTES;
        }

        private void NOTE_SUB_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
            }
            // Find the row under the mouse
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            DataGridRow row = dep as DataGridRow;
            if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
            {
                // Select the row under the mouse
                dataGrid.SelectedItem = row.Item;

                // Show the context menu
                dataGrid.ContextMenu.IsOpen = true;

                // Mark the event as handled to prevent the default context menu behavior
                e.Handled = true;
            }
            else
            {
                // No valid row, don't show context menu
                e.Handled = true;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            NOTE_SUB.BeginEdit();
        }
    }
}
