using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Wins.WinMenus.Taarif.TCOD_ANBAR_WIN;

namespace Wins.WinMenus.Taarif
{
    public partial class CUSTKIND_WIN : Window
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
        public CUSTKIND_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<CUSTKIND> CUSTKIND_DATA { get; set; } = new ObservableCollection<CUSTKIND>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public CUSTKIND? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public CUSTKIND? WAS_ROW_ITEM { get; private set; }

        UniversControl universControl = new UniversControl();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "CUST", new WindowInteropHelper(this).Handle, this.GetType().Name); //دسترسی
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            ReGetData();
        }
        private void ReGetData()
        {
            CUSTKIND_DATA?.Clear();
            var _DATA_ = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND ORDER BY CUST_COD").ToList();
            foreach (var item in _DATA_)
            {
                CUSTKIND_DATA.Add(item);
            }
        }
        private void CUSTKIND_SUB_CANCEL_EDIT()
        {
            CUSTKIND_SUB.Dispatcher.InvokeAsync(() =>
            {
                CUSTKIND_SUB.CellEditEnding -= CUSTKIND_SUB_CellEditEnding;
                CUSTKIND_SUB.RowEditEnding -= CUSTKIND_SUB_RowEditEnding;

                CUSTKIND_SUB.CancelEdit();
                CUSTKIND_SUB.RowEditEnding += CUSTKIND_SUB_RowEditEnding;
                CUSTKIND_SUB.CellEditEnding += CUSTKIND_SUB_CellEditEnding;
            });
        }
        private bool BodyIsValid(CUSTKIND _row)
        {
            var ROW = _row;

            var errors = (from object i in CUSTKIND_SUB.ItemsSource
                          let c = CUSTKIND_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.CUSTKNAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام نوع مشتری نمی تواند خالی باشد" });
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void CUSTKIND_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && CUSTKIND_SUB.SelectedItem is not null)
            {
                if (CUSTKIND_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((CUSTKIND)CUSTKIND_SUB.SelectedItem).Clone() as CUSTKIND;
                }
            }
        }
        private void CUSTKIND_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_
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
            {
            }
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();

            CURRENT_ROW_ITEMS = e.Row.Item as CUSTKIND;
            #endregion

            if (e.Column.SortMemberPath == "CUSTKNAME")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("نام نوع مشتری نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CUSTKIND_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.CUSTKNAME = WAS_ROW_ITEM?.CUSTKNAME;
                }
            }
        }
        private void CUSTKIND_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as CUSTKIND;

            if (!BodyIsValid(ROW))
            {
                CUSTKIND_SUB_CANCEL_EDIT();
                return;
            }
         

            if (e.Row.Item == null)
            {
                return;
            }

            double? THECODE = null;
            if (string.IsNullOrEmpty(ROW?.CUST_COD.ToStringNullSafe()))
            {
                THECODE = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(CUST_COD + 1) FROM dbo.CUSTKIND").FirstOrDefault();
                if (THECODE is null || THECODE.ToStringNullSafe() == "NULL")
                {
                    THECODE = 1;
                }
            }
            else
            {
                THECODE = ROW.CUST_COD;
            }

            try
            {
                if (ROW?.CUST_COD is null)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.CUSTKIND(CUST_COD, CUSTKNAME)
                                         VALUES({THECODE},
                                         N'{ROW.CUSTKNAME}')");
                    
                    ROW.CUST_COD = (int?)THECODE;
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.CUSTKIND
                                         SET CUSTKNAME = N'{ROW.CUSTKNAME}'
                                         WHERE CUST_COD = {ROW.CUST_COD}");
                }


                #region Form_AfterUpdate
                if (!(Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5"))
                {
                    var rst = dbms.DoGetDataSQL<ANBAR_MODEL_QRE1>($"SELECT TOP 1 N_KOL, NUMBER, NAME FROM dbo.DETA_HES WHERE N_KOL={Baseknow.TFROSH} AND NUMBER={ROW.CUST_COD}").FirstOrDefault();

                    var _N_KOL = Baseknow.TFROSH;
                    var _NUMBER = ROW.CUST_COD;
                    var _NAME = ROW.CUSTKNAME;
                    var _BED_BES_ = -1;

                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME ,BED_BES)
                                         VALUES({_N_KOL}, {_NUMBER}, N'{_NAME}',{_BED_BES_})");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES SET N_KOL = {_N_KOL}, NUMBER = {_NUMBER}, NAME = N'{_NAME}' , BED_BES = {_BED_BES_}
                                         WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER}");
                    }
                }
                #endregion

            }
            catch (SqlException ex)
            {
                CUSTKIND_SUB_CANCEL_EDIT();

                if (ex.Number == 547)
                {
                    new Msgwin(false, "این نوع مشتری دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    new Msgwin(false, "نام یا کد تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            ReGetData();

            //int lastindexrow = 0;
            //lastindexrow = CUSTKIND_SUB.Items.IndexOf(CUSTKIND_SUB?.CurrentItem);
            CL_LMethods.MovingDG(CUSTKIND_SUB, null, CUSTKIND_SUB.Items.Count - 1);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

        }

        private void CUSTKIND_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (CUSTKIND_SUB.Items.Count > 0 && CUSTKIND_SUB.SelectedItem != null)
                {
                    if (!(CUSTKIND_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف نوع مشتری",
                                    recordId: CUSTKIND_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < CUSTKIND_SUB.SelectedItems.Count; i++)
                            {
                                var item = CUSTKIND_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("CUST_COD").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _cust_cod = item.GetType().GetProperty("CUST_COD").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.CUSTKIND WHERE CUST_COD = {_cust_cod}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var CNAME = item.GetType().GetProperty("CUSTKNAME").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این نوع مشتری ({CNAME}) دارای گردش است و نمی توان آنرا حذف کرد" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }

                        //After Opration:
                        if (IsDeletedSomething)
                        {
                            ReGetData();
                        }

                    }
                }
            }
        }
    }
}
