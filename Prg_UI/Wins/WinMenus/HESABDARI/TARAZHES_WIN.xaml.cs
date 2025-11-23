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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for TARAZHES_WIN.xaml
    /// </summary>
    public partial class TARAZHES_WIN : Window
    {
        #region Header Window Begin
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
        #endregion

        public TARAZHES_WIN()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<TARAZHES> TARAZHES_DATA { get; set; } = new ObservableCollection<TARAZHES>();
        public ObservableCollection<TOTA_HES> HESAB_DATA { get; set; } = new ObservableCollection<TOTA_HES>();
        public TARAZHES CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public TARAZHES WAS_ROW_ITEM { get; private set; }
        public bool NowIsReady { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        UniversControl universControl = new UniversControl();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                string Formname = "TNAMH";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion


            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            ReGetData();
            LoadHesabData();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void ReGetData()
        {
            TARAZHES_DATA?.Clear();
            var data = dbms.DoGetDataSQL<TARAZHES>("SELECT NUMBER, CRT, UID FROM dbo.TARAZHES").ToList();
            foreach (var item in data)
            {
                TARAZHES_DATA.Add(item);
            }
        }

        private void LoadHesabData()
        {
            HESAB_DATA?.Clear();
            var hesabData = dbms.DoGetDataSQL<TOTA_HES>("SELECT NUMBER, NAME FROM TOTA_HES").ToList();
            foreach (var item in hesabData)
            {
                HESAB_DATA.Add(item);
            }
        }

        private void TARAZHES_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TARAZHES_SUB.Dispatcher.InvokeAsync(() =>
            {
                TARAZHES_SUB.CellEditEnding -= TARAZHES_SUB_CellEditEnding;
                TARAZHES_SUB.RowEditEnding -= TARAZHES_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    TARAZHES_SUB.CancelEdit();
                }
                else
                {
                    TARAZHES_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TARAZHES_SUB.RowEditEnding += TARAZHES_SUB_RowEditEnding;
                TARAZHES_SUB.CellEditEnding += TARAZHES_SUB_CellEditEnding;
            });
        }

        private void TARAZHES_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as TARAZHES;
        }

        private bool BodyIsValid(TARAZHES _row)
        {
            var ROW = _row;

            if (ROW?.NUMBER is null)
            {
                universControl.PopNotifyShow("شماره حساب نمی تواند خالی باشد!", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            var isDuplicate = TARAZHES_DATA.Any(x => x.NUMBER == ROW.NUMBER && !ReferenceEquals(x, ROW));
            if (isDuplicate)
            {
                universControl.PopNotifyShow("این شماره حساب قبلا انتخاب شده است.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            return true;
        }

        private void TARAZHES_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as TARAZHES;
            if (!BodyIsValid(ROW))
            {
                TARAZHES_SUB_CANCEL_EDIT();
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            try
            {
                var existingRecord = dbms.DoGetDataSQL<int?>($"SELECT 1 FROM dbo.TARAZHES WHERE NUMBER = {ROW.NUMBER}").FirstOrDefault();

                if (existingRecord is null)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TARAZHES(NUMBER, CRT, UID)
                                     VALUES({ROW.NUMBER}, GETDATE(), {Baseknow.USERCOD} ) ");
                }
                else
                {
                    // Update is not logical for this form as only NUMBER is editable and it's the key.
                    // If needed, an update logic can be added here.
                    // For now, we prevent duplicates in BodyIsValid.
                }
            }
            catch (SqlException ex)
            {
                TARAZHES_SUB_CANCEL_EDIT();
                if (ex.Number == 2627) // Unique constraint violation
                {
                    new Msgwin(false, "این شماره حساب قبلا ثبت شده است.").ShowDialog();
                    return;
                }
                new Msgwin(false, "خطا در بروزرسانی پایگاه داده!").ShowDialog();
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات!").ShowDialog(); return;
            }
            ReGetData();

            CL_LMethods.MovingDG(TARAZHES_SUB, null, CURRENT_ROW_INDEX);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void TARAZHES_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TARAZHES_SUB.Items.Count > 0 && TARAZHES_SUB.SelectedItem != null)
                {
                    if (!(TARAZHES_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            for (int i = 0; i < TARAZHES_SUB.SelectedItems.Count; i++)
                            {
                                var item = TARAZHES_SUB.SelectedItems[i] as TARAZHES;

                                if (item != null)
                                {
                                    try
                                    {
                                        IsDeletedSomething = true;
                                        dbms.DoExecuteSQL($@"DELETE FROM dbo.TARAZHES WHERE NUMBER = {item.NUMBER}");
                                    }
                                    catch (Exception)
                                    {
                                        new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog();
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (IsDeletedSomething)
                        {
                            ReGetData();
                        }
                    }
                }
            }
        }

        private void TARAZHES_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && TARAZHES_SUB.SelectedItem is not null)
            {
                if (TARAZHES_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((TARAZHES)TARAZHES_SUB.SelectedItem).Clone() as TARAZHES;
                }
            }
        }

        private void TARAZHES_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (!(TARAZHES_SUB.Items.Count < 1) && !(TARAZHES_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = TARAZHES_SUB.SelectedIndex;
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {

                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);

            }
        }
    }
}
