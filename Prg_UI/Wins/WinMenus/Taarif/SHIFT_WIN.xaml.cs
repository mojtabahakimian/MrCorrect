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

namespace Wins.WinMenus.Taarif
{
    public partial class SHIFT_WIN : Window
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
        public SHIFT_WIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<SHIFT> SHIFT_DATA { get; set; } = new ObservableCollection<SHIFT>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public SHIFT? CURRENT_ROW_ITEMS { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public SHIFT? WAS_ROW_ITEM { get; private set; }
        public bool NowIsReady { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; } = 0;

        UniversControl universControl = new UniversControl();
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SHIFT", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }


            ReGetData();
        }
        private void ReGetData()
        {
            SHIFT_DATA?.Clear();
            var _DATA_ = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID, SHNAME FROM dbo.SHIFT").ToList();
            foreach (var item in _DATA_)
            {
                SHIFT_DATA.Add(item);
            }
        }
        private void SHIFT_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            SHIFT_SUB.Dispatcher.InvokeAsync(() =>
            {
                SHIFT_SUB.CellEditEnding -= SHIFT_SUB_CellEditEnding;
                SHIFT_SUB.RowEditEnding -= SHIFT_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    SHIFT_SUB.CancelEdit();
                }
                else
                {
                    SHIFT_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                SHIFT_SUB.RowEditEnding += SHIFT_SUB_RowEditEnding;
                SHIFT_SUB.CellEditEnding += SHIFT_SUB_CellEditEnding;
            });
        }
        private void SHIFT_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && SHIFT_SUB.SelectedItem is not null)
            {
                if (SHIFT_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((SHIFT)SHIFT_SUB.SelectedItem).Clone() as SHIFT;
                }
            }
        }
        private void SHIFT_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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

            CURRENT_ROW_ITEMS = e.Row.Item as SHIFT;
            #endregion

            if (e.Column.SortMemberPath == "SHNAME")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("شیفت نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    SHIFT_SUB_CANCEL_EDIT();
                    CURRENT_ROW_ITEMS.SHNAME = WAS_ROW_ITEM?.SHNAME;
                }
            }
        }
        private bool BodyIsValid(SHIFT _row)
        {
            var ROW = _row;

            var errors = (from object i in SHIFT_SUB.ItemsSource
                          let c = SHIFT_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(ROW?.SHNAME))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام گروه نمی تواند خالی باشد" });
            }

            if (ErrosMessages.Count > 0)
            {
                //ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                //      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                //new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void SHIFT_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as SHIFT;

            if (!BodyIsValid(ROW))
            {
                SHIFT_SUB_CANCEL_EDIT();
                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            double? THECODE = null;
            if (string.IsNullOrEmpty(ROW?.SHIFT_ID.ToStringNullSafe()))
            {
                THECODE = (double)dbms.DoGetDataSQL<double?>("SELECT MAX(SHIFT_ID + 1) FROM dbo.SHIFT").FirstOrDefault();
                if (THECODE is null || THECODE.ToStringNullSafe() == "NULL")
                {
                    THECODE = 1;
                }
            }
            else
            {
                THECODE = ROW.SHIFT_ID;
            }

            try
            {
                if (ROW?.SHIFT_ID is null)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.SHIFT(SHIFT_ID, SHNAME)
                                         VALUES({THECODE}, N'{ROW.SHNAME}' ) ");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.SHIFT
                                         SET SHNAME = N'{ROW.SHNAME}'
                                         WHERE SHIFT_ID = {ROW.SHIFT_ID}");
                }

                #region Form_AfterUpdate
                var RST2 = dbms.DoGetDataSQL<DEPART_MODEL>("SELECT DEPATMAN, DEPNAME, IDD, DEPART FROM dbo.DEPART").ToList();
                foreach (var item in RST2) //while (!RST2.EOF())
                {
                    var _N_KOL_ = Baseknow.SANDOGH;
                    var _NUMBER_ = item.DEPATMAN;
                    var _TNUMBER_ = ROW.SHIFT_ID;
                    var _NAME_ = ROW.SHNAME;
                    var _BED_BES_ = -1;

                    var rst = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TOP 1 N_KOL, NUMBER,TNUMBER, NAME FROM dbo.TDETA_HES WHERE N_KOL={Baseknow.SANDOGH} AND NUMBER={item.DEPATMAN} AND TNUMBER = {ROW.SHIFT_ID}").FirstOrDefault();
                    if (rst is null)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_},
                                                 {_TNUMBER_},
                                                 N'{_NAME_}',
                                                 {_BED_BES_} )");
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_},
                                                 TNUMBER = {_TNUMBER_}, NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                 WHERE N_KOL = {rst.N_KOL} AND NUMBER = {rst.NUMBER} AND TNUMBER = {rst.TNUMBER}");
                    }

                }
                #endregion
            }
            catch (SqlException ex)
            {
                SHIFT_SUB_CANCEL_EDIT();
                if (ex.Number == 547)
                {
                    new Msgwin(false, "این شیفت دارای گردش است و نمی توان آنرا حذف کرد").ShowDialog();
                    return;
                }
                if (ex.Number == 2627)
                {
                    new Msgwin(false, "نام شیفت تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            ReGetData();

            CL_LMethods.MovingDG(SHIFT_SUB, null, CURRENT_ROW_INDEX);

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private void SHIFT_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL برای بروز رسانی کارنت رو و کارنت کالمن
                if (!(SHIFT_SUB.Items.Count < 1) && !(SHIFT_SUB.SelectedItem is null))
                {
                    if (SHIFT_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        CURRENT_ROW_INDEX = SHIFT_SUB.SelectedIndex;
                    }
                }
            }
        }
        private void SHIFT_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (SHIFT_SUB.Items.Count > 0 && SHIFT_SUB.SelectedItem != null)
                {
                    if (!(SHIFT_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "تعریف شیفت",
                                recordId: SHIFT_SUB.SelectedItem.ToStringNullSafe(),
                                oldValue: null,
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < SHIFT_SUB.SelectedItems.Count; i++)
                            {
                                var item = SHIFT_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("SHIFT_ID").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _shift_id = item.GetType().GetProperty("SHIFT_ID").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.SHIFT WHERE SHIFT_ID = {_shift_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                var _SHIFTNAME = item.GetType().GetProperty("SHNAME").GetValue(item);
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این شیفت ({_SHIFTNAME}) دارای گردش است و نمی توان آنرا حذف کرد" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
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
