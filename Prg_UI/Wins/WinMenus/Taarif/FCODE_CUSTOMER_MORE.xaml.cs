using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wins.WinOther;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Stimulsoft.Base.Drawing.StiFontReader;

namespace Wins.WinMenus.Taarif
{
    public partial class FCODE_CUSTOMER_MORE : Window
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
        public FCODE_CUSTOMER_MORE(string _hes_)
        {
            InitializeComponent();

            this.DataContext = this;

            HES = _hes_;
        }
        #region LOCAL_MODEL
     
        #endregion
        public List<COMBOYMODEL> NESBAT_MODEL { get; set; } = new List<COMBOYMODEL>()
        {
            //1;"همسر";2;"دختر";3;"پسر";4;"مادر";5;"پدر";6;"خواهر";7;"برادر";7;"دوست";9;"سایر"
            new COMBOYMODEL { ID = 1, NAME = "همسر" },
            new COMBOYMODEL { ID = 2, NAME = "دختر" },
            new COMBOYMODEL { ID = 3, NAME = "پسر" },
            new COMBOYMODEL { ID = 4, NAME = "مادر" },
            new COMBOYMODEL { ID = 5, NAME = "پدر" },
            new COMBOYMODEL { ID = 6, NAME = "خواهر" },
            new COMBOYMODEL { ID = 7, NAME = "برادر" },
            new COMBOYMODEL { ID = 8, NAME = "دوست" },
            new COMBOYMODEL { ID = 9, NAME = "سایر" }
        };

        public string HES { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<MORINFO_SUB> MORINFO_SUB_DATA { get; set; } = new ObservableCollection<MORINFO_SUB>();

        UniversControl universControl = new UniversControl();
        public byte[]? BINY_FILE { get; set; } = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            var DataHead = dbms.DoGetDataSQL<MORINFO>($"SELECT * FROM dbo.MORINFO WHERE HES = N'{HES}'").FirstOrDefault();
            if (DataHead is not null)
            {
                EMAILM.Text = DataHead.EMAILM;
                SITEM.Text = DataHead.SITEM;
                WATSAP.Text = DataHead.WATSAP;
                TELEGRAM.Text = DataHead.TELEGRAM;
                INSTAM.Text = DataHead.INSTAM;
                LINKEDINM.Text = DataHead.LINKEDINM;
                ADDRESSMANZEL.Text = DataHead.ADDRESSMANZEL;
                TAVALOD_DATE.Text = DataHead?.TAVALOD_DATE.ToStringNullSafe();
                MARRID_DATE.Text = DataHead?.MARRID_DATE.ToStringNullSafe();

                MOTAHEL.SelectedValue = DataHead.MOTAHEL;
                TAVALOD_CITY.SelectedValue = DataHead.TAVALOD_CITY;

                if (DataHead?.AKS != null && DataHead?.AKS.Length > 0)
                {
                    BINY_FILE = DataHead.AKS;
                    BitmapImage bitmapImage = CL_LMethods.ByteArrayToBitmapImage(DataHead.AKS);
                    AKS.Source = bitmapImage;
                }
            }

            ReGetData();

            if (MORINFO_SUB_DATA.Count > 0 && DataHead is not null)
            {
                AllowEdits = false;
                AllowDeletions = false;
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            var Cities = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYNAME FROM dbo.TCOD_CITY").ToList();
            foreach (var rw in Cities)
            {
                rw.CITYNAME = rw.CITYNAME.FixPersianChars();
            }

            TAVALOD_CITY.ItemsSource = Cities;

            List<COMBOYMODEL> item = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "متاهل" },
                new COMBOYMODEL { ID = 2, NAME = "مجرد" },
                new COMBOYMODEL { ID = 3, NAME = "مطلقه" },
                new COMBOYMODEL { ID = 4, NAME = "نامشخص" }
            };

            MOTAHEL.ItemsSource = item;
            COLUMN_TAHOL.ItemsSource = item;

            COLUMN_NESBAT.ItemsSource = NESBAT_MODEL;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }

            //if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    if (SAVER_BTN.IsFocused || ESLAH.IsFocused || DELETER_BTN.IsFocused)
            //    {
            //    }
            //    else
            //    {
            //        e.Handled = true;
            //        CL_LMethods.SendKey_US(Key.Tab);
            //    }
            //}
        }
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                AllowAdditionEdits(_ican);
            }
        }
        private void AllowAdditionEdits(bool ican)
        {
            if (ican is true)
            {
                EMAILM.IsReadOnly = false;
                SITEM.IsReadOnly = false;
                WATSAP.IsReadOnly = false;
                TELEGRAM.IsReadOnly = false;
                INSTAM.IsReadOnly = false;
                LINKEDINM.IsReadOnly = false;
                ADDRESSMANZEL.IsReadOnly = false;
                TAVALOD_DATE.IsReadOnly = false;
                MARRID_DATE.IsReadOnly = false;

                MOTAHEL.IsEnabled = true;
                TAVALOD_CITY.IsEnabled = true;
                StorePicBtn.IsEnabled = true;
                IMGREMOVE.IsEnabled = true;

                SAVER_BTN.IsEnabled = true;

                FCODE_CUSTOMER_MORE_SUB.IsEnabled = true;
            }
            else
            {
                EMAILM.IsReadOnly = true;
                SITEM.IsReadOnly = true;
                WATSAP.IsReadOnly = true;
                TELEGRAM.IsReadOnly = true;
                INSTAM.IsReadOnly = true;
                LINKEDINM.IsReadOnly = true;
                ADDRESSMANZEL.IsReadOnly = true;
                TAVALOD_DATE.IsReadOnly = true;
                MARRID_DATE.IsReadOnly = true;

                MOTAHEL.IsEnabled = false;
                TAVALOD_CITY.IsEnabled = false;
                StorePicBtn.IsEnabled = false;
                IMGREMOVE.IsEnabled = false;

                SAVER_BTN.IsEnabled = false;

                FCODE_CUSTOMER_MORE_SUB.IsEnabled = false;
            }
        }
        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }

        public int CURRENT_ROW_INDEX { get; set; }
        public MORINFO_SUB CURRENT_ROW_ITEMS { get; set; } = new MORINFO_SUB();

        private int _codemeli_index;
        public int CODEMELI_INDEX_COL
        {
            get
            {
                if (FCODE_CUSTOMER_MORE_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = FCODE_CUSTOMER_MORE_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODEMELLI")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _codemeli_index = 0;
                    }
                    else
                    {
                        _codemeli_index = (int)defaultcolumnindex;
                    }
                }
                return _codemeli_index;
            }
        }

        public bool NowIsReady { get; set; }
        public object ENTERED_VALUE_ROW { get; set; }

        private void StorePicBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                Title = "Select an image"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var imagePath = openFileDialog.FileName;

                // Display the selected image
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                AKS.Source = bitmap;
                BINY_FILE = CL_LMethods.CompressImage(bitmap);
            }
        }
        private void TAVALOD_DATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = TAVALOD_DATE.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    TAVALOD_DATE.Text = null;
                    return;
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                TAVALOD_DATE.Text = null;
                return;
            }
        }
        private void MARRID_DATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = MARRID_DATE.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    MARRID_DATE.Text = null;
                    return;
                }
            }
            else
            {
                MARRID_DATE.Text = null;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
        }
        private void EMAILM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!CL_LMethods.EmailIsValid(EMAILM.Text.Trim()))
            {
                EMAILM.Text = null;
                universControl.PopNotifyShow("ایمیل وارد شده صحیح نیست !", Pop1, Pop1Text1, Pop_Border1);
            }
        }
        private void IMGREMOVE_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا مطمئن هستید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult is true)
            {
                AKS.Source = null;
                BINY_FILE = null;
            }
        }
        public bool MasterIsValid()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!Tarikh.IsValidedDate(TAVALOD_DATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ تولد درست نیست" });
            }
            if (!Tarikh.IsValidedDate(MARRID_DATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ ازدواج درست نیست" });
            }
            if (!CL_LMethods.EmailIsValid(EMAILM.Text.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ایمیل وارد شده صحیح نیست !" });
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
        private void SAVER_BTN_Click(object sender, RoutedEventArgs e) //-------------------ذخیره--------------------
        {
            if (!MasterIsValid()) { return; }

            if (MORINFO_SUB_DATA.Count > 0)
            {
                if (!BodyIsValid()) { return; }
            }

            var _TAVALOD_DATE = string.IsNullOrEmpty(TAVALOD_DATE.Text.ToRawTarikh()) ? "NULL" : TAVALOD_DATE.Text.ToRawTarikh();
            var _MARRID_DATE = string.IsNullOrEmpty(MARRID_DATE.Text.ToRawTarikh()) ? "NULL" : MARRID_DATE.Text.ToRawTarikh();
            var _MOTAHEL = MOTAHEL.SelectedValue is null ? "NULL" : MOTAHEL.SelectedValue.ToStringNullSafe();

            //HEAD {
            string Qry = "";
            var CustomyRow = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 HES FROM MORINFO WHERE HES = N'{HES}'").ToList();
            if (CustomyRow.Count <= 0)
            {
                Qry = $@"INSERT INTO dbo.MORINFO(HES, TAVALOD_CITY, TAVALOD_DATE, MARRID_DATE, EMAILM, SITEM, WATSAP, TELEGRAM, INSTAM, LINKEDINM, MOTAHEL, ADDRESSMANZEL, AKS)
                            VALUES(N'{HES}',
                            N'{TAVALOD_CITY.SelectedValue}' ,
                            {_TAVALOD_DATE}   ,
                            {_MARRID_DATE}   ,
                            N'{EMAILM.Text.Trim()}' ,
                            N'{SITEM.Text.Trim()}' ,
                            N'{WATSAP.Text.Trim()}' ,
                            N'{TELEGRAM.Text.Trim()}' ,
                            N'{INSTAM.Text.Trim()}' ,
                            N'{LINKEDINM.Text.Trim()}' ,
                            {_MOTAHEL}   ,
                            N'{ADDRESSMANZEL.Text.Trim()}' ,
                            @img
                            )";
            }
            else
            {
                Qry = $@"UPDATE dbo.MORINFO
                     SET TAVALOD_CITY = N'{TAVALOD_CITY.SelectedValue}',
                     TAVALOD_DATE = {_TAVALOD_DATE},
                     MARRID_DATE = {_MARRID_DATE}, 
                     EMAILM = N'{EMAILM.Text.Trim()}', SITEM = N'{SITEM.Text.Trim()}',
                     WATSAP = N'{WATSAP.Text.Trim()}', TELEGRAM = N'{TELEGRAM.Text.Trim()}',
                     INSTAM = N'{INSTAM.Text.Trim()}',
                     LINKEDINM = N'{LINKEDINM.Text.Trim()}', MOTAHEL = {_MOTAHEL}, ADDRESSMANZEL = N'{ADDRESSMANZEL.Text.Trim()}',
                     AKS = @img
                     WHERE HES = N'{HES}' ";
            }


            try
            {
                if (BINY_FILE is null)
                {
                    dbms.DoExecuteSQL(Qry, new { img = "NULL" });
                }
                else
                {
                    dbms.DoExecuteSQL(Qry, new { img = BINY_FILE });
                }
            }
            catch (SqlException ex)
            {
                FCODE_CUSTOMER_MORE_SUB_Cancel_Edit();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری وارد شده !").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }


            //HEAD }

            //Body {

            dbms.DoExecuteSQL($"DELETE FROM dbo.MORINFO_SUB WHERE HES = N'{HES}'");

            foreach (var item in MORINFO_SUB_DATA)
            {
                var Sub_TAVALOD_DATE = string.IsNullOrEmpty(item.TAVALOD_DATE.ToStringNullSafe().ToRawTarikh()) ? "NULL" : item.TAVALOD_DATE.ToStringNullSafe().ToRawTarikh();
                var Sub_MARRID_DATE = string.IsNullOrEmpty(item.MARRID_DATE.ToStringNullSafe().ToRawTarikh()) ? "NULL" : item.MARRID_DATE.ToStringNullSafe().ToRawTarikh();
                var Sub_MOTAHEL = item.MOTAHEL is null ? "NULL" : item.MOTAHEL.ToStringNullSafe();

                string QrySub = "";
                QrySub = $@"INSERT INTO dbo.MORINFO_SUB (HES, CODEMELLI,NAMP, TAVALOD_DATE, MARRID_DATE, EMAILM, SITEM, WATSAP, TELEGRAM, INSTAM, LINKEDINM, MOTAHEL, ADDRESSMANZEL, AKS)
                                VALUES(N'{HES}',
                                N'{item.CODEMELLI}' ,
                                N'{item.NAMP}' ,
                                {Sub_TAVALOD_DATE}   ,
                                {Sub_MARRID_DATE}   ,
                                N'{item.EMAILM}' ,
                                N'{item.SITEM}' ,
                                N'{item.WATSAP}' ,
                                N'{item.TELEGRAM}' ,
                                N'{item.INSTAM}' ,
                                N'{item.LINKEDINM}' ,
                                {Sub_MOTAHEL}   ,
                                N'{item.ADDRESSMANZEL}' ,
                                @img
                                )";

                //var CustomyRowSub = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 CODEMELLI FROM dbo.MORINFO_SUB WHERE HES = N'{HES}' AND CODEMELLI = N'{item.CODEMELLI}'").ToList();
                //if (CustomyRowSub.Count <= 0)
                //{
                //}
                //else
                //{
                //    QrySub = $@"UPDATE dbo.MORINFO_SUB
                //                SET TAVALOD_DATE = {Sub_TAVALOD_DATE}, CODEMELLI = N'{item.CODEMELLI}', NAMP = N'{item.NAMP}',
                //                MARRID_DATE = {Sub_MARRID_DATE}, 
                //                EMAILM = N'{item.EMAILM}', SITEM = N'{item.SITEM}',
                //                WATSAP = N'{item.WATSAP}', TELEGRAM = N'{item.TELEGRAM}',
                //                INSTAM = N'{item.INSTAM}',
                //                LINKEDINM = N'{item.LINKEDINM}', MOTAHEL = {Sub_MOTAHEL}, ADDRESSMANZEL = N'{item.ADDRESSMANZEL}',
                //                AKS = @img
                //                WHERE HES = N'{HES}' ";
                //}
                var _aks = (item.AKS is null ? null : item.AKS);

                try
                {
                    dbms.DoExecuteSQL(QrySub, new { img = _aks });
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 || ex.Number == 2627)
                    {
                        new Msgwin(false, "داده تکراری وارد شده").ShowDialog();
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                }


                //if (item.AKS is null) { }
                //else { dbms.DoExecuteSQL(QrySub, new { img = item.AKS }); }
            }
            //Body }

            universControl.PopNotifyShow("اطلاعات ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }
        private bool BodyIsValid()
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var hasError = false;
            DataGridRow row = (DataGridRow)FCODE_CUSTOMER_MORE_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                FCODE_CUSTOMER_MORE_SUB.UpdateLayout();
                FCODE_CUSTOMER_MORE_SUB.ScrollIntoView(FCODE_CUSTOMER_MORE_SUB.Items[CURRENT_ROW_INDEX]);
                row = (DataGridRow)FCODE_CUSTOMER_MORE_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
            }
            if (row != null && Validation.GetHasError(row))
            {
                hasError = true;
            }

            hasError = (from object i in FCODE_CUSTOMER_MORE_SUB.ItemsSource
                        let c = row
                        where c != null && Validation.GetHasError(c)
                        select c).Any();

            foreach (var ROW in MORINFO_SUB_DATA)
            {
                if (string.IsNullOrEmpty(ROW?.CODEMELLI))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "کد ملی سطر نمی تواند خالی باشد" });
                }
                if (string.IsNullOrEmpty(ROW?.NAMP))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نام سطر نمی تواند خالی باشد" });
                }

                if (ROW.CODEMELLI?.Length > 50)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "طول کد ملی سطر بیش از حد مجاز است" });
                }
                if (ROW.NAMP?.Length > 100)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "طول نام سطر بیش از اندازه است" });
                }

                if (!string.IsNullOrEmpty(ROW?.TAVALOD_DATE.ToStringNullSafe().ToRawTarikh()))
                {
                    if (!Tarikh.IsValidedDate(ROW?.TAVALOD_DATE.ToStringNullSafe().ToRawTarikh()))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ تولد سطر صحیح نیست" });
                    }
                }
                if (!string.IsNullOrEmpty(ROW?.MARRID_DATE.ToStringNullSafe().ToRawTarikh()))
                {
                    if (!Tarikh.IsValidedDate(ROW?.MARRID_DATE.ToStringNullSafe().ToRawTarikh()))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ ازدواج سطر صحیح نیست" });
                    }
                }

            }

            if (ErrosMessages.Count > 0 || hasError)
            {
                FCODE_CUSTOMER_MORE_SUB_Cancel_Edit();

                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                      .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void FCODE_CUSTOMER_MORE_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (NowIsReady)
            {
                #region REFILL_CURRENTS_
                DataGridRow row1 = e.Row;
                int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
                CURRENT_ROW_INDEX = row_index;
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

                CURRENT_ROW_ITEMS = e.Row.Item as MORINFO_SUB;
                #endregion

                if (e.Column.SortMemberPath == "TAVALOD_DATE")
                {
                    if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                    {
                        ENTERED_VALUE_ROW = ENTERED_VALUE_ROW.ToStringNullSafe().Replace("/", "");
                        if (!Tarikh.IsValidedDate(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            CURRENT_ROW_ITEMS.TAVALOD_DATE = null;
                            //FCODE_CUSTOMER_MORE_SUB_Cancel_Edit(DataGridEditingUnit.Cell);
                            universControl.PopNotifyShow("تاریخ تولد سطر صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        }
                    }
                }
                if (e.Column.SortMemberPath == "MARRID_DATE")
                {
                    if (!string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                    {
                        ENTERED_VALUE_ROW = ENTERED_VALUE_ROW.ToStringNullSafe().Replace("/", "");
                        if (!Tarikh.IsValidedDate(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            CURRENT_ROW_ITEMS.MARRID_DATE = null;
                            //FCODE_CUSTOMER_MORE_SUB_Cancel_Edit(DataGridEditingUnit.Cell);
                            universControl.PopNotifyShow("تاریخ ازدواج سطر صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        }
                    }
                }
            }
        }
        private void FCODE_CUSTOMER_MORE_SUB_Cancel_Edit(DataGridEditingUnit? _RC_ = null)
        {
            FCODE_CUSTOMER_MORE_SUB.Dispatcher.InvokeAsync(() =>
            {
                FCODE_CUSTOMER_MORE_SUB.CellEditEnding -= FCODE_CUSTOMER_MORE_SUB_CellEditEnding;
                FCODE_CUSTOMER_MORE_SUB.RowEditEnding -= FCODE_CUSTOMER_MORE_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    FCODE_CUSTOMER_MORE_SUB.CancelEdit();
                }
                else
                {
                    FCODE_CUSTOMER_MORE_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                FCODE_CUSTOMER_MORE_SUB.RowEditEnding += FCODE_CUSTOMER_MORE_SUB_RowEditEnding;
                FCODE_CUSTOMER_MORE_SUB.CellEditEnding += FCODE_CUSTOMER_MORE_SUB_CellEditEnding;
            });
        }
        private void FCODE_CUSTOMER_MORE_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            BodyIsValid();
        }
        private void FCODE_CUSTOMER_MORE_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null) && FCODE_CUSTOMER_MORE_SUB.Items.Count > 0 && FCODE_CUSTOMER_MORE_SUB.SelectedIndex > -1 && !(FCODE_CUSTOMER_MORE_SUB.SelectedItem is null))
            {
                if (FCODE_CUSTOMER_MORE_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    CURRENT_ROW_INDEX = FCODE_CUSTOMER_MORE_SUB.SelectedIndex;
                }
            }
        }
        private void FCODE_CUSTOMER_MORE_SUB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && FCODE_CUSTOMER_MORE_SUB.SelectedItem != null && FCODE_CUSTOMER_MORE_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (FCODE_CUSTOMER_MORE_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = FCODE_CUSTOMER_MORE_SUB.SelectedIndex;
                }
            }
        }
        private void ReGetData()
        {
            MORINFO_SUB_DATA?.Clear();
            var DataSub = dbms.DoGetDataSQL<MORINFO_SUB>($"SELECT * FROM dbo.MORINFO_SUB WHERE HES = N'{HES}'").ToList();
            foreach (var item in DataSub)
            {
                MORINFO_SUB_DATA.Add(item);
            }
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.TR("MORINFO", $"(hes = N'{HES}' )", DateTime.Now, 1);

            AllowEdits = true;
            AllowDeletions = true;

            if (CURRENT_ROW_ITEMS is not null)
            {
                if (!string.IsNullOrEmpty(HES) && !string.IsNullOrEmpty(CURRENT_ROW_ITEMS?.CODEMELLI))
                {
                    CL_HESABDARI.TR("MORINFO_SUB", $"(hes = N'{HES}') AND (CODEMELLI = " + CURRENT_ROW_ITEMS?.CODEMELLI + ")", DateTime.Now, 1);
                }
            }
        }
        private void DELETER_BTN_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETER_BTN.Visibility == Visibility.Visible;
            if (!DELETER_BTN.IsEnabled || !IsVisible) { return; }

            if (MORINFO_SUB_DATA.Count < 0)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از حذف اصطلاعات بیشتر این مشتری اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    CL_HESABDARI.TR("MORINFO", $"(hes = N'{HES}' )", DateTime.Now, 1);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "تعریف مشتری - اطلاعات بیشتر",
                            recordId: HES,
                            oldValue: null,
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

             

                    try
                    {
                        dbms.DoExecuteSQL($"DELETE FROM dbo.MORINFO WHERE HES = N'{HES}'");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                        }
                        else
                        {
                            new Msgwin(false, "خطا پایگاه داده در انجام عملیات حذف").ShowDialog();
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
                        return;
                    }

                    this.Close();
                }
            }
            else
            {
                new Msgwin(false, "نمی توان تا زمانی که سط های مربوط به اقوام دارد این ها را پاک کرد").ShowDialog();
            }
        }
        private void AksSubBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as MORINFO_SUB)?.AKS is not null)
                    {
                        new IMAGEY_WIN((btn.Tag as MORINFO_SUB)?.AKS).ShowDialog();
                    }
                    else
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog
                        {
                            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                            Title = "Select an image"
                        };
                        if (openFileDialog.ShowDialog() == true)
                        {
                            var imagePath = openFileDialog.FileName;

                            // Validate image file size (maximum 10 MB)
                            if (!CL_LMethods.ValidateImageFile(openFileDialog.FileName, out string errorMessage, 3))
                            {
                                new Msgwin(false, errorMessage).ShowDialog();
                                return;
                            }

                            // Display the selected image
                            BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                            CURRENT_ROW_ITEMS.AKS = CL_LMethods.CompressImage(bitmap);
                        }
                    }
                }
            }
        }
        private void EditCurrentRowIMG_Click(object sender, RoutedEventArgs e)
        {
            if (CURRENT_ROW_ITEMS is not null)
            {
                if (CURRENT_ROW_ITEMS?.AKS is not null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                        Title = "Select an image"
                    };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        var imagePath = openFileDialog.FileName;

                        // Display the selected image
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                        CURRENT_ROW_ITEMS.AKS = CL_LMethods.CompressImage(bitmap);
                    }
                }
            }
        }
        private void DeleteCurrentRowIMG_Click(object sender, RoutedEventArgs e)
        {
            if (CURRENT_ROW_ITEMS is not null)
            {
                if (CURRENT_ROW_ITEMS?.AKS is not null)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از حذف عکس این سطر اطمینان دارید ؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        CURRENT_ROW_ITEMS.AKS = null;
                    }
                }
            }
        }
        private void FCODE_CUSTOMER_MORE_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از حذف سطر مطمئن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void AKS_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new IMAGEY_WIN(BINY_FILE).ShowDialog();
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }
    }
}
