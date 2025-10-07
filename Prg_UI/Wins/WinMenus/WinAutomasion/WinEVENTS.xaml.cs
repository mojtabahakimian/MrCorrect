using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using HeyRed.Mime;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.KHARID_FORUSH;
using static MimeDetective.Definitions.Default;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.WinAutomasion
{
    public partial class WinEVENTS : Window
    {
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public ObservableCollection<COMBOYMODEL> SKID_ITEMS { get; set; }
        public ObservableCollection<AutomasionEVNT> EVENTS_DATA { get; set; } = new ObservableCollection<AutomasionEVNT>();
        public long IDNUMTASK { get; set; }

        /// <summary>
        /// Give me the IDNUM of TASK for events
        /// </summary>
        /// <param name="giveTheIDNUMTASK"></param>
        public WinEVENTS(long giveTheIDNUMTASK)
        {
            IDNUMTASK = giveTheIDNUMTASK;
            InitializeComponent();

            this.DataContext = this;
        }

        private int _name_code_index;
        private bool eVENTSDataGrid_IsFocused;

        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (eVENTSDataGrid.Columns.Count > 0)
                {
                    int? defaultcolumnindex = eVENTSDataGrid.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "EVENTS")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _name_code_index = 0;
                    }
                    else
                    {
                        _name_code_index = (int)defaultcolumnindex;
                    }
                }
                return _name_code_index;
            }
        }

        public int CURRENT_ROW_INDEX { get; set; }
        public string? ENTERED_VALUE_ROW { get; private set; }
        public AutomasionEVNT? CURRENT_ITEMS_ROW { get; private set; }
        public bool NowIsReady { get; private set; }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = eVENTSDataGrid;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                try
                {
                    if (uie is DataGridCell || eVENTSDataGrid_IsFocused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            //bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            int? thatcolumnindex = eVENTSDataGrid.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "skid")?.DisplayIndex;
                            if (thatcolumnindex > -1)
                            {
                                bool isThatColumn = currentColumnIndex == thatcolumnindex;
                                bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                                if (isThatColumn)
                                {
                                    if (isLastRow)
                                    {
                                        DG.SelectedIndex++;

                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[NAME_CODE_INDEX_COL]);

                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            DG.BeginEdit();
                                        }), DispatcherPriority.Background);

                                        return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                    }
                                }
                            }

                        }
                    }
                }
                catch { /*ignore*/ }

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Command8_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            ReGetData(true);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            SKID_ITEMS = new ObservableCollection<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "سند حسابداری" },
                new COMBOYMODEL { ID = 1, NAME = "رسید خرید" },
                new COMBOYMODEL { ID = 2, NAME = "حواله فروش" },
                new COMBOYMODEL { ID = 3, NAME = "برگشت خرید" },
                new COMBOYMODEL { ID = 4, NAME = "برگشت فروش" },
                new COMBOYMODEL { ID = 20, NAME = "پیش فاکتور" },
                new COMBOYMODEL { ID = 100, NAME = "درخواست پرداخت" },
                new COMBOYMODEL { ID = 13, NAME = "فاکتور فروش" },
                new COMBOYMODEL { ID = 12, NAME = "فاکتور خرید" },
                new COMBOYMODEL { ID = 32, NAME = "فاکتور خرید صادرات" },
                new COMBOYMODEL { ID = 6, NAME = "انتقال کالا از انبار به انبار" },
                new COMBOYMODEL { ID = 90, NAME = "پذیرش تعمیرگاه" },
                new COMBOYMODEL { ID = 25, NAME = "فاکتور برگشت فروش آزاد" },
                new COMBOYMODEL { ID = 24, NAME = "سایر رسید انبارها" },
                new COMBOYMODEL { ID = 26, NAME = "سایر حواله انبار ها" },
                new COMBOYMODEL { ID = 27, NAME = "برگشت خريد ازاد" },
                new COMBOYMODEL { ID = 33, NAME = "فروش صادراتی" },
                new COMBOYMODEL { ID = 34, NAME = "خزانه داری" },
                new COMBOYMODEL { ID = 35, NAME = "سفارش" },
                new COMBOYMODEL { ID = 36, NAME = "درخواست خريد" },
                new COMBOYMODEL { ID = 37, NAME = "مرخصی" },
                new COMBOYMODEL { ID = 38, NAME = "حواله خروج سایر" },
                new COMBOYMODEL { ID = 39, NAME = "حواله خروج مواد" },
                new COMBOYMODEL { ID = 40, NAME = "تجهیزات PM" },
                new COMBOYMODEL { ID = 41, NAME = "درخواست کالا از انبار" },
                new COMBOYMODEL { ID = 42, NAME = "اعلام خرابی" },
                new COMBOYMODEL { ID = 101, NAME = "اتوماسیون" }
            };

            skidColumn.ItemsSource = SKID_ITEMS;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void ReGetData(bool GOTOLAST = false)
        {
            EVENTS_DATA?.Clear();
            var RST = dbms.DoGetDataSQL<AutomasionEVNT>($"SELECT IDNUM,IDD,EVENTS,STDATE,STTIME,USERNAME,COMPANY,SUMTIME,pic,skid,num,tg,FXTYPE FROM dbo.EVENTS WHERE IDNUM = {IDNUMTASK} ORDER BY IDD").ToList();
            foreach (var item in RST)
            {
                EVENTS_DATA.Add(item);
            }

            if (GOTOLAST)
            {
                CL_LMethods.FocusCellReadyToEdit(eVENTSDataGrid, "EVENTS", eVENTSDataGrid.Items.Count - 1, false);
            }
            else
            {
                int lastindexrow = 0;
                lastindexrow = eVENTSDataGrid.Items.IndexOf(eVENTSDataGrid?.CurrentItem);

                lastindexrow = CURRENT_ROW_INDEX;
                if (lastindexrow > 0)
                {
                    CL_LMethods.FocusCellReadyToEdit(eVENTSDataGrid, "EVENTS", lastindexrow, false);
                }
                else
                {
                    CL_LMethods.FocusCellReadyToEdit(eVENTSDataGrid, "EVENTS", eVENTSDataGrid.Items.Count - 1, false);
                }
            }
        }

        private void eVENTSDataGrid_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            eVENTSDataGrid.Dispatcher.InvokeAsync(() =>
            {
                eVENTSDataGrid.CellEditEnding -= eVENTSDataGrid_CellEditEnding;
                eVENTSDataGrid.RowEditEnding -= eVENTSDataGrid_RowEditEnding;

                if (_RC_ is null)
                {
                    eVENTSDataGrid.CancelEdit();
                }
                else
                {
                    eVENTSDataGrid.CancelEdit((DataGridEditingUnit)_RC_);
                }
                eVENTSDataGrid.RowEditEnding += eVENTSDataGrid_RowEditEnding;
                eVENTSDataGrid.CellEditEnding += eVENTSDataGrid_CellEditEnding;
            });
        }

        private void eVENTSDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS
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
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ITEMS_ROW = e.Row.Item as AutomasionEVNT;
            #endregion

        }

        private void eVENTSDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var errors = (from object i in eVENTSDataGrid.ItemsSource
                          let c = eVENTSDataGrid.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                Msgwin msgwin = new Msgwin(false, "داده های وارد شده صحیح نیست!"); msgwin.ShowDialog();
                eVENTSDataGrid_CANCEL_EDIT();
                return;
            }

            if (!(eVENTSDataGrid is null) && eVENTSDataGrid.Items.Count > 0)
            {
                // if Is Not Null
                var CURRENT_ITM = e.Row.Item as AutomasionEVNT;
                if (!(CURRENT_ITM is null))
                {
                    //بررسی اینکه چیزی خالی نباشد
                    if (
                        string.IsNullOrEmpty(CURRENT_ITM.EVENTS.ToStringNullSafe()) ||
                        CURRENT_ITM.STDATE is null ||
                        CURRENT_ITM.STTIME is null ||
                        CURRENT_ITM.SUMTIME is null)
                    {
                        Msgwin msgwin = new Msgwin(false, "بعضی از آیتم ها رو خالی گذاشتید لطفا اصلاح کنید");
                        msgwin.ShowDialog();
                        eVENTSDataGrid_CANCEL_EDIT();
                        return;
                    }
                    else
                    {
                        string _FXTYPE_ = (string.IsNullOrEmpty(CURRENT_ITM.FXTYPE) ? "NULL" : $"N'{CURRENT_ITM.FXTYPE}'");

                        //Insert
                        if (CURRENT_ITM.IDD is null)
                        {
                            string QRE_INSERT = "";

                            if (CURRENT_ITM?.pic is null)
                            {
                                QRE_INSERT = $@"INSERT INTO dbo.EVENTS(IDNUM, EVENTS, STDATE, STTIME, USERNAME, SUMTIME, skid, num)
                                              VALUES({IDNUMTASK},
                                              N'{CURRENT_ITM.EVENTS.Trim()}' ,
                                              {CURRENT_ITM.STDATE},
                                              {CURRENT_ITM.STTIME},
                                              N'{CL_HESABDARI.UCurrentUser()}',
                                              {CURRENT_ITM.SUMTIME}, 
                                              {(string.IsNullOrEmpty(CURRENT_ITM.skid.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.skid.ToString())} ,
                                              {(string.IsNullOrEmpty(CURRENT_ITM.num.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.num.ToString())}
                                              )";
                                dbms.DoExecuteSQL(QRE_INSERT);
                            }
                            else
                            {
                                QRE_INSERT = $@"INSERT INTO dbo.EVENTS(IDNUM, EVENTS, STDATE, STTIME, USERNAME, SUMTIME,skid, num, FXTYPE , pic)
                                              VALUES({IDNUMTASK},
                                              N'{CURRENT_ITM.EVENTS.Trim()}' ,
                                              {CURRENT_ITM.STDATE},
                                              {CURRENT_ITM.STTIME},
                                              N'{CL_HESABDARI.UCurrentUser()}',
                                              {CURRENT_ITM.SUMTIME},
                                              {(string.IsNullOrEmpty(CURRENT_ITM.skid.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.skid.ToString())} ,
                                              {(string.IsNullOrEmpty(CURRENT_ITM.num.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.num.ToString())} ,
                                              {_FXTYPE_} ,
                                              @img)";

                                dbms.DoExecuteSQL(QRE_INSERT, new { img = CURRENT_ITM.pic });
                            }
                            ReGetDataAndPreserveState();
                        }
                        //Update
                        else
                        {
                            string QRE_UPDATE = "";

                            if (CURRENT_ITM?.pic is null)
                            {
                                QRE_UPDATE = $@"UPDATE dbo.EVENTS SET
                                              EVENTS = N'{CURRENT_ITM.EVENTS}' , 
                                              SUMTIME = {CURRENT_ITM.SUMTIME} ,
                                              skid = {(string.IsNullOrEmpty(CURRENT_ITM.skid.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.skid.ToString())},
                                              num = {(string.IsNullOrEmpty(CURRENT_ITM.num.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.num.ToString())} ,
                                              FXTYPE = {_FXTYPE_} , 
                                              pic = @img
                                              WHERE IDNUM = {IDNUMTASK} AND IDD = {CURRENT_ITM.IDD}";

                                dbms.DoExecuteSQL(QRE_UPDATE, new { img = (byte[])null });
                            }
                            else
                            {
                                QRE_UPDATE = $@"UPDATE dbo.EVENTS SET
                                                EVENTS = N'{CURRENT_ITM.EVENTS}' , 
                                                SUMTIME = {CURRENT_ITM.SUMTIME},  
                                                skid = {(string.IsNullOrEmpty(CURRENT_ITM.skid.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.skid.ToString())},
                                                num = {(string.IsNullOrEmpty(CURRENT_ITM.num.ToStringNullSafe()) ? "NULL" : CURRENT_ITM.num.ToString())} ,
                                                FXTYPE = {_FXTYPE_} , 
                                                pic = @img
                                                WHERE IDNUM = {IDNUMTASK} AND IDD = {CURRENT_ITM.IDD}";

                                dbms.DoExecuteSQL(QRE_UPDATE, new { img = CURRENT_ITM.pic });
                            }
                            ReGetDataAndPreserveState();
                        }
                    }
                }
            }

            //eVENTSDataGrid_COMMIT_EDIT();
        }

        private void ReGetDataAndPreserveState()
        {
            // Save the current row and column index
            var selectedItem = eVENTSDataGrid.SelectedItem;
            var selectedIndex = eVENTSDataGrid.SelectedIndex;
            var currentColumn = eVENTSDataGrid.CurrentColumn;

            // Refresh data
            ReGetData();

            // Restore the selection and focus
            if (selectedItem != null)
            {
                try
                {
                    eVENTSDataGrid.SelectedIndex = selectedIndex;
                    eVENTSDataGrid.SelectedItem = selectedItem;
                    eVENTSDataGrid.CurrentCell = new DataGridCellInfo(selectedItem, currentColumn);
                    eVENTSDataGrid.ScrollIntoView(selectedItem);
                    eVENTSDataGrid.Focus();
                }
                catch { }
            }
        }


        private void eVENTSDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            //##:##
            var NI = new AutomasionEVNT
            {
                STDATE = Convert.ToInt32(Tarikh.GoGetPersianDate(true)),
                STTIME = Convert.ToInt32(DateTime.Now.ToString("HHmm")),
                USERNAME = Baseknow.UUSER,
                SUMTIME = 0000
            };
            e.NewItem = NI;
        }

        bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fs.Close();
                }
            }
            catch (IOException)
            {
                return true; // File is locked
            }
            return false; // File is not locked
        }


        List<string> GotPaths = new List<string>();
        private async void DeleteDownloadZamimeh()
        {
            foreach (var item in GotPaths)
            {
                _ = CL_FileSecretHandler.DeleteFileSilentlyAsync(item);
            }
        }

        public async Task<string> GetExtensionFormat(byte[] fileBytes, string fileName)
        {
            var result = await FileSignatureDetector.DetectFileTypeAsync(fileBytes, fileName);

            if (result.IsValid)
            {
                return result.Extension;
            }
            else
            {
                return $"bin";
            }
        }
        private async void ZamimehBtnColum_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag != null)) return;

            //IDD
            long _IDD_ = Convert.ToInt64((btn.Tag as AutomasionEVNT)?.IDD);

            var CURRENT_AutomasionEVNT = (btn.Tag as AutomasionEVNT);
            //اگر آیدی دارد یعنی رکرود قبلا ثبت شده
            if (_IDD_ > 0)
            {
                ////ببین آیا اصلا ضمیمه ای داره ؟
                if (!(CURRENT_AutomasionEVNT?.pic is null))
                {
                    try
                    {
                        //ساخت مسیر یکتا در تمپ برای دریافت فایل از سرور
                        string THE_PATH_FILE = CL_LMethods.GetTempFilePathWithExtension("");
                        //string THE_PATH_FILE = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //On Desktop's User

                        string? THE_MIMTYP = null;
                        if (!string.IsNullOrEmpty(CURRENT_AutomasionEVNT?.FXTYPE))
                        {
                            THE_MIMTYP = CURRENT_AutomasionEVNT?.FXTYPE;
                        }
                        else
                        {
                            //ببینم فایل مون چه فرمتی داره
                            THE_MIMTYP = GetExtensionFormat(CURRENT_AutomasionEVNT.pic, default).Result;
                            if (THE_MIMTYP == "bin")
                            {
                                //var THE_MIMTYP2 = MimeTypeImagy.GetMimeType(CURRENT_AutomasionEVNT.pic, THE_PATH_FILE);
                                THE_MIMTYP = FileTypeDetector.GetFileExtension(CURRENT_AutomasionEVNT.pic);
                            }
                        }

                        string THE_TYPEFILE = "";

                        Process? MY_PROCESS1 = null;

                        bool FileWroteSuccesfully = false;

                        if (true) //Just Format
                        {
                            if ((!THE_MIMTYP.StartsWith('.') && THE_MIMTYP is "application/octet-stream") || THE_MIMTYP is "bin") //اگر فرمت ناشناخته بود From MS Access Attached (OLE object)
                            {
                                THE_TYPEFILE = ExtensionTypeManage.GetExtensionFileType(CURRENT_AutomasionEVNT.pic);
                                var OLE_Removed_Bytes = ExtensionTypeManage.MimeTypeManagement.GetNormalizeBytes(CURRENT_AutomasionEVNT.pic); //Becuase of OLE object has special signature byte from MS Access

                                THE_PATH_FILE = THE_PATH_FILE + THE_TYPEFILE;  ////C:\Users\ADMINI~1\AppData\Local\Temp\csharp.png
                                                                               //نوشتن فایل و باز کردن
                                FileWroteSuccesfully = TryToWriteFile(THE_PATH_FILE, OLE_Removed_Bytes);
                                if (!FileWroteSuccesfully)
                                {
                                    return;
                                }
                            }
                            else // WPF Attached
                            {
                                if (THE_MIMTYP.StartsWith('.')) //Found extension filetype already
                                {
                                    THE_TYPEFILE = THE_MIMTYP;
                                }
                                else
                                {
                                    THE_TYPEFILE = MimeTypesMap.GetExtension(THE_MIMTYP);
                                }

                                THE_PATH_FILE = THE_PATH_FILE + THE_TYPEFILE;   ////C:\Users\ADMINI~1\AppData\Local\Temp\csharp.png
                                                                                //نوشتن فایل و باز کردن
                                                                                //File.WriteAllBytes(THE_PATH_FILE, CURRENT_AutomasionEVNT.pic);

                                FileWroteSuccesfully = TryToWriteFile(THE_PATH_FILE, CURRENT_AutomasionEVNT);
                                if (!FileWroteSuccesfully)
                                {
                                    return;
                                }
                            }


                            if (CL_LMethods.IsImageTYPE(THE_TYPEFILE)) // اگر عکس , عکس باز کن
                            {
                                try
                                {
                                    var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                                    MY_PROCESS1 = Process.Start(new ProcessStartInfo()
                                    {
                                        FileName = "rundll32.exe",
                                        Arguments = string.Format(
                                            "\"{0}{1}\", ImageView_Fullscreen {2}",
                                            Environment.Is64BitOperatingSystem ?
                                                path.Replace(" (x86)", "") : path,
                                            @"\Windows Photo Viewer\PhotoViewer.dll", THE_PATH_FILE),
                                        WindowStyle = ProcessWindowStyle.Maximized,
                                        UseShellExecute = false
                                    });
                                }
                                catch (Exception) { MY_PROCESS1 = Process.Start("explorer.exe", THE_PATH_FILE); }
                            }
                            else
                            {
                                MY_PROCESS1 = Process.Start("explorer.exe", THE_PATH_FILE);
                            }

                            CURRENT_AutomasionEVNT.FXTYPE = THE_TYPEFILE;

                            GotPaths.Add(THE_PATH_FILE);

                            if (false) //Remove File After Open
                            {
                                //if (!(MY_PROCESS1 is null)) //Finnaly After file has Open
                                //{
                                //    // Wait for the file to be unlocked
                                //    //Thread.Sleep(4000);
                                //    //while (true)
                                //    //{
                                //    //    if (!IsFileLocked(THE_PATH_FILE))
                                //    //    {
                                //    //        break;
                                //    //    }
                                //    //    Thread.Sleep(500); // Wait for 500 milliseconds before rechecking
                                //    //}

                                //    MY_PROCESS1?.WaitForExit();

                                //    if (MY_PROCESS1 != null && MY_PROCESS1.HasExited)
                                //    {
                                //        //حالا میخوایم فایلی که بسته توی دیتامون آپدیتش کنیم که اگر تغیری داده اوکی بشه
                                //        // چون ممکنه فرمت ورد یا اکسل رو باز کرده بشه !
                                //        //IDEVNT
                                //        if (File.Exists(THE_PATH_FILE)) //ناگهانی حذف نشده باشه
                                //        {
                                //            try
                                //            {
                                //                var TheNewUpdatedBianaryFile = CL_LMethods.ConvertFileToByte(THE_PATH_FILE);
                                //                if (!(TheNewUpdatedBianaryFile is null))
                                //                {
                                //                    try
                                //                    {
                                //                        string QRE_UPDATE = "";
                                //                        QRE_UPDATE = $@"UPDATE dbo.EVENTS SET pic = @img WHERE IDNUM = {IDNUMTASK} AND IDD = {_IDD_}";

                                //                        if (TheNewUpdatedBianaryFile is null)
                                //                        {
                                //                            dbms.DoExecuteSQL(QRE_UPDATE, new { img = "NULL" });
                                //                        }
                                //                        else
                                //                        {
                                //                            dbms.DoExecuteSQL(QRE_UPDATE, new { img = CURRENT_AutomasionEVNT.pic });
                                //                        }

                                //                        try
                                //                        {
                                //                            //و در نهایت حذف فایل داخل تمپ
                                //                            File.Delete(THE_PATH_FILE);
                                //                        }
                                //                        catch { }
                                //                    }
                                //                    catch (Exception) { }
                                //                }
                                //            }
                                //            catch (IOException ex)
                                //            {
                                //                if ((ex.HResult & 0xFFFF) == 32)
                                //                {
                                //                    this.Dispatcher.Invoke(() =>
                                //                    {
                                //                        new Msgwin(false, "این فایل در حال استفاده است و نمیتوان آنرا ضمیمه کرد").ShowDialog();
                                //                    });
                                //                    return;
                                //                }
                                //                else
                                //                {
                                //                    this.Dispatcher.Invoke(() =>
                                //                    {
                                //                        new Msgwin(false, "دسترسی به فایل مورد نظر مقدور نیست").ShowDialog();
                                //                    });
                                //                    return;
                                //                }
                                //            }
                                //            catch (Exception)
                                //            {
                                //                new Msgwin(false, "فایل مورد نظر باز شد , لطفا اگر تغیراتی در فایل مد نظر ایجاد کردید , مجددا آنرا ضمیمه کنید تا فایل جدید در اتوماسیون ضمیمه شود.").ShowDialog();
                                //            }
                                //        }
                                //    }
                                //}
                            }

                        }
                    }
                    catch (IOException ex)
                    {
                        if ((ex.HResult & 0xFFFF) == 32)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                new Msgwin(false, "این فایل در حال استفاده است و نمیتوان آنرا ضمیمه کرد").ShowDialog();
                            });
                            return;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                new Msgwin(false, "دسترسی به فایل مورد نظر مقدور نیست").ShowDialog();
                            });
                            return;
                        }
                    }
                    catch (Exception exr)
                    {
                        Msgwin msgwin = new Msgwin(false, "خطا در انجام عملیات !");
                        msgwin.ShowDialog();
                        return;
                    }
                }
                else
                {
                    var dialog = new OpenFileDialog
                    {
                        CheckFileExists = true,
                        Multiselect = false,
                        Filter = "Allowed Files (*.jpg, *.png, *.rar, *.zip, *.pdf, *.docx, *.xlsx)|*.jpg;*.png;*.rar;*.zip;*.pdf;*.docx;*.xlsx"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        string[] allowedExtensions = { ".jpg", ".png", ".rar", ".zip", ".pdf", ".docx", ".xlsx" };
                        string fileExtension = Path.GetExtension(dialog.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            new Msgwin(false, "فایلی که انتخاب کرده اید مجاز نیست!").Show();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    try
                    {
                        CURRENT_AutomasionEVNT.pic = CL_LMethods.ConvertFileToByte(dialog.FileName);
                        CURRENT_AutomasionEVNT.FXTYPE = Path.GetExtension(dialog.FileName).ToLower();
                    }
                    catch (IOException ex)
                    {
                        if ((ex.HResult & 0xFFFF) == 32)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                new Msgwin(false, "این فایل در حال استفاده است و نمیتوان آنرا ضمیمه کرد").ShowDialog();
                            });
                            return;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                new Msgwin(false, "دسترسی به فایل مورد نظر مقدور نیست").ShowDialog();
                            });
                            return;
                        }
                    }
                    catch (Exception exr)
                    {
                        Msgwin msgwin = new Msgwin(false, "خطا در انجام عملیات !");
                        msgwin.ShowDialog();
                        return;
                    }

                }
            }
            else // صرفا آماده کردن فایل و گذاشتن در حافظه برای ذخیره در صورت پایین اومدن از سطر
            {
                var dialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Multiselect = false,
                };
                if (dialog.ShowDialog() != true) { return; }

                CURRENT_AutomasionEVNT.pic = CL_LMethods.ConvertFileToByte(dialog.FileName);
                CURRENT_AutomasionEVNT.FXTYPE = Path.GetExtension(dialog.FileName).ToLower();
            }
        }

        private bool TryToWriteFile(string THE_PATH_FILE, object CURRENT_AutomasionEVNT)
        {
            if (CURRENT_AutomasionEVNT is AutomasionEVNT AUTOEVNT)
            {
                return CL_LMethods.WriteTheFile(THE_PATH_FILE, AUTOEVNT.pic);
            }
            else if (CURRENT_AutomasionEVNT is byte[] PIC)
            {
                return CL_LMethods.WriteTheFile(THE_PATH_FILE, PIC);
            }

            return false;
        }


        private void eVENTSDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady)
            {
                if (eVENTSDataGrid.SelectedIndex > -1)
                {
                    CURRENT_ROW_INDEX = eVENTSDataGrid.SelectedIndex;
                }
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void eVENTSDataGrid_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                eVENTSDataGrid_IsFocused = false;
            }
            else //Is Focus inside of eVENTSDataGrid_IsFocused
            {
                eVENTSDataGrid_IsFocused = true;
            }
        }

        private void BTN_OPN_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag != null)) return;

            var SelectedRow = (btn.Tag as AutomasionEVNT);
            if (SelectedRow != null && SelectedRow?.skid != null && SelectedRow?.num != null)
            {
                if (SelectedRow.num == 0)
                {
                    new Msgwin(false, "شماره درج شده صفر است !").Show(); return;
                }
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, (int)SelectedRow.skid, SelectedRow.num, true, _isCalledFromAutomasion_: true);
            }

        }

        private void DeleteCurrentRowIMG_Click(object sender, RoutedEventArgs e)
        {
            var Row = eVENTSDataGrid.SelectedItem as AutomasionEVNT;
            if (Row != null)
            {
                if (Row?.pic is not null)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از حذف ضمیمه این سطر اطمینان دارید ؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        Row.pic = null;
                        eVENTSDataGrid.BeginEdit();
                        eVENTSDataGrid.EndInit();
                    }
                }
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DeleteDownloadZamimeh();
        }

        private void eVENTSDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (eVENTSDataGrid.Items.Count > 0)
            {
                if (eVENTSDataGrid?.CurrentCell != null)
                {
                    DataGridHelper.CopyValueFromAboveCell(eVENTSDataGrid, e);
                }
            }
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid? dg = sender as DataGrid;
            if (dg == null) return;

            if (dg.CurrentItem == null || dg.CurrentItem == CollectionView.NewItemPlaceholder)
            {
                e.Handled = true; // Cancel opening the menu. Avoids the crash.
                return;
            }
            if (dg?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            else if (dg?.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            base.OnContextMenuOpening(e);
        }
        private void eVENTSDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid? dg = sender as DataGrid;
            if (dg == null) return;

            if (dg.CurrentItem == null || dg.CurrentItem == CollectionView.NewItemPlaceholder)
            {
                e.Handled = true; // Cancel opening the menu. Avoids the crash.
                return;
            }
            if (dg?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            else if (dg?.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            base.OnContextMenuOpening(e);
        }
        private void eVENTSDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
                    var isEditing = ((IEditableCollectionView)eVENTSDataGrid.Items).IsEditingItem;
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
