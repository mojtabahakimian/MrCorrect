using Dapper;
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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Wins.WinMenus.ANBAR;
using Syncfusion.Data.Extensions;
using System.Windows.Threading;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using System.Windows.Data;
using System.ComponentModel;
using static Prg_UI.Functions.CL_LMethods;
using System.Windows.Controls.Primitives;
using Prg_Proccessy.CNNMANAGER;

namespace Prg_UI.Wins.WinMenus.SANATI
{
    public partial class WIN_HEAD_MANF_DAY : Window
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

        public WIN_HEAD_MANF_DAY(double? number_to_open = null, double mAVADR = 0, double dASTR = 0, double sARR = 0, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = number_to_open;
                PRGID.Text = number_to_open.ToString();
                PRGID.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;

                MAVADR = mAVADR; //جمع مواد
                DASTR = dASTR; //جمع دستمزد
                SARR = sARR; //جمع سربار
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
        public double MAVADR { get; } //جمع مواد
        public double DASTR { get; }  //جمع دستمزد
        public double SARR { get; } //جمع سربار

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        public ObservableCollection<DTL_MANF_SUB_DAY> SUB_DATA { get; } = new ObservableCollection<DTL_MANF_SUB_DAY>();
        public bool NowIsReady { get; private set; }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int DG_SUB_DEF_INDEX_COL
        {
            get
            {
                if (DG_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = DG_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODE")?.DisplayIndex;
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
        public DTL_MANF_SUB_DAY? CURRENT_ITEMS_ROW { get; private set; }
        public DTL_MANF_SUB_DAY? WAS_ROW_ITEM { get; private set; } = new DTL_MANF_SUB_DAY();
        public DTL_MANF_SUB_DAY FROM_SEARCH_KAL { get; set; } = new DTL_MANF_SUB_DAY();

        #region LOCAL_MODEL
        public class CODE_MODEL
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
        }
        #endregion

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;

        private decimal sum_of_megh_k = 0;
        public decimal SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (decimal)SUB_DATA.Sum(r => r.MEGHK ?? 0);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        private double _SUM_OF_MABL_K = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _SUM_OF_MABL_K = (double)SUB_DATA.Sum(r => r.SumOfMABLK ?? 0);
                if (_SUM_OF_MABL_K == 0) _SUM_OF_MABL_K = 0;
                return _SUM_OF_MABL_K;
            }
            set { _SUM_OF_MABL_K = value; }
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
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
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

        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                // --- Header Input Controls ---
                PRG_DATE.IsReadOnly = !ican;
                TOZIH.IsReadOnly = !ican;
                SA_HOUR.IsReadOnly = !ican;
                SA_NHOU.IsReadOnly = !ican;
                IMBIBE_MANF.IsReadOnly = !ican;
                IMBIBE_SAR.IsReadOnly = !ican;

                PCODE.IsEnabled = ican;

                // --- DataGrid Control ---
                DG_SUB.IsReadOnly = !ican;
            }
        }

        public int ANBARDefaultValue { get; private set; }
        public double Meidnum { get; private set; }
        public Visual IAM_HEAD_MANF { get; private set; }

        private NavigationManager<PRGHEAD> _navigationManager;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            IAM_HEAD_MANF = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            const string REPLACEMENT_VALUE = "dbo.HEAD_LST.";

            string WhereCondition = $" dbo.PRGHEAD.PRGID = {NUMBER_TO_OPEN}"; //= CL_LMethods.GetRestrictedSqlQuery(0).Replace(REPLACEMENT_VALUE, null);

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" dbo.PRGHEAD.PRGID = {NUMBER_TO_OPEN}";
            }

            _navigationManager = new NavigationManager<PRGHEAD>(
                dbms,
                x => x.PRGID.ToString(), // property selector (used to find a record by its CODE)
                @$"SELECT dbo.PRGHEAD.PRGID, dbo.PRGHEAD.PCODE, dbo.PRGHEAD.PRG_DATE, dbo.STUF_DEF.NAME FROM dbo.PRGHEAD
                         INNER JOIN dbo.STUF_DEF ON dbo.PRGHEAD.PCODE=dbo.STUF_DEF.CODE
                    WHERE {WhereCondition}", //All Record of The Table
              /*on navigation get ever record where*/ x => @$"SELECT dbo.PRGHEAD.PRGID, dbo.PRGHEAD.PCODE, dbo.PRGHEAD.PRG_DATE, dbo.STUF_DEF.NAME FROM dbo.PRGHEAD
                         INNER JOIN dbo.STUF_DEF ON dbo.PRGHEAD.PCODE=dbo.STUF_DEF.CODE
                    WHERE {WhereCondition}", //On Change for One Record
            Convert.ToDouble(PRGID.Text)
            );

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            Form_Current();
            AllowEdits = false;

            CL_LMethods.SetTabIndexes(
             PCODE, PRG_DATE, SA_HOUR, SA_NHOU, IMBIBE_MANF, IMBIBE_SAR, TOZIH,
             DG_SUB
             );

            MakeDefaultFocuseReady();
        }
        private void Form_Current()
        {
            //if (string.IsNullOrEmpty(PCODE.SelectedValue?.ToStringNullSafe()))
            //{
            //    this.DG_SUB.IsReadOnly = true;
            //}
            //else
            //{
            //    this.DG_SUB.IsReadOnly = false;
            //}
            //this.AllowDeletions = false;
            //this.AllowEdits = false;
            //this.DG_SUB.IsReadOnly = true;
        }

        private bool OnInsertRecord(PRGHEAD record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<PRGHEAD>($"SELECT TOP 1 * FROM HEAD_MANF WHERE FNUMB = {PRGID.Text} ").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(PRGHEAD HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                PRGID.Text = HEADER_FAC.PRGID.ToString();
                PRGID.Tag = HEADER_FAC.PRGID.ToString();

                PCODE.SelectedValue = HEADER_FAC.PCODE;

                PRG_DATE.Text = HEADER_FAC.PRG_DATE.ToStringNullSafe();

                ItwasNewFirstTime = false; //Reset for Sanad Concurrency at first insert

                DG_SUB_ReGetData();

                Form_Current();
            }
        }
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<PRGHEAD>($"SELECT TOP 1 * FROM HEAD_MANF WHERE FNUMB = {PRGID.Text} ").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void MakeDefaultFocuseReady()
        {
            PCODE.Focus();
        }
        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(PRGID.Text) || PRGID.Text == "0")
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                DG_SUB.IsReadOnly = false;
            }

            //SecurityAllCheck();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = DG_SUB;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (DG_SUB.IsKeyboardFocusWithin)
                {
                    try
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DG_SUB_DEF_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    catch { /*ignore*/ }

                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.S || e.SystemKey == Key.S))
                {
                    e.Handled = true;
                }
            }


            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }

        private void CODE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PCODE.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CODE_TEX = (TextBox)PCODE.Template.FindName("PART_EditableTextBox", PCODE);
            if (CODE_TEX is null)
            {
                return;
            }
            if (PCODE.SelectedValue is not null)
            {
                if ((PCODE.SelectedItem as CODE_MODEL)?.NAME == CODE_TEX.Text)
                {
                    return;
                }
            }

            //var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, CODE_TEX.Text);
            //if (RST_KALA != null)
            //{
            //    CODE.SelectedValue = RST_KALA.CODE; CODE.Items.Refresh();
            //}
            //else
            //{
            //    universControl.PopNotifyShowUp("چنین کالایی وجود ندارد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
            //    return;
            //}

            //string currentCode = CODE.SelectedValue?.ToString();
            //int FnumbCode = Convert.ToInt32(PRGID.Text);


            //if (string.IsNullOrWhiteSpace(currentCode))
            //{
            //    DG_SUB.IsReadOnly = true;
            //    return;
            //}
            //else
            //{
            //    DG_SUB.IsReadOnly = false;
            //}

            //string sql = "SELECT FNUMB FROM HEAD_MANF WHERE CODE = @CODE";
            //var parameters = new { CODE = currentCode };
            //var existingFormula = dbms.DoGetDataSQL<PRGHEAD>(sql, parameters).FirstOrDefault();
            //if (existingFormula != null)
            //{
            //    if (FnumbCode != existingFormula.FNUMB)
            //    {
            //        string message = "کاربر گرامی برای این کالا قبلا فرمول تعریف شده است. دقت کنید که عملیات را بصورت صحیح انجام داده باشید.";
            //        new Msgwin(false, message).ShowDialog();
            //    }
            //}
        }
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (!DATE_IS_VALID())
            {
                e.Handled = true;
            }
        }


        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "FORMOL", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(DG_SUB, "HEAD_MANF");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
        }
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT ANBCO FROM dbo.OPANBACCESS WHERE (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
                if (rst.Count > 0)
                {
                    ANBARDefaultValue = (int)rst.FirstOrDefault();

                    Baseknow.anbardef = ANBARDefaultValue;
                }
                else
                {
                    Baseknow.anbardef = Baseknow.DEFANB;
                }
            }
            else
            {
                Baseknow.anbardef = Baseknow.DEFANB;
            }
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            //ANBAR_COLUMN.ItemsSource = ARST;
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //کالا
            PCODE.ItemsSource = dbms.DoGetDataSQL<CODE_MODEL>($"SELECT CODE, NAME + N' ' + CODE AS NAME FROM STUF_DEF WHERE (RADAH > 1) ORDER BY NAME + N' ' + CODE").ToList();

            ////انبار کالا
            //ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //string date_n_val = PRG_DATE.Text.ToRawTarikh();
            //if (!string.IsNullOrEmpty(date_n_val))
            //{
            //    if (!Tarikh.IsValidedDate(date_n_val))
            //    {
            //        PRG_DATE.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
            //        ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
            //    }
            //    else
            //    {
            //        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
            //        {
            //            PRG_DATE.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
            //            ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
            //        }
            //    }
            //}
            //else
            //{
            //    PRG_DATE.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            //}

            if (PCODE.SelectedValue is null) //کالا
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا نمیتواند خالی باشد." });
            }
            if (!CL_LMethods.IsNumeric(SA_HOUR.Text)) //جذب ساعت کار
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب ساعت کار معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(SA_NHOU.Text)) //نرخ ساعت کار استاندارد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار نرخ ساعت کار استاندارد معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(IMBIBE_MANF.Text)) //جذب هزینه دستمزد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب هزینه دستمزد معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(IMBIBE_SAR.Text)) //جذب هزینه سربار
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب هزینه سربار معتبر نیست" });
            }

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }

            return true;
        }
        private bool BodyIsValid(DTL_MANF_SUB_DAY TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        public bool ItwasNewFirstTime { get; set; } = false;
        public object NUMBER_TO_OPEN { get; private set; }

        public bool DATE_IS_VALID(bool DisplayMsg = false)
        {
            bool Date_Is_Valid = true;

            var DATE = PRG_DATE.Text.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    }
                    Date_Is_Valid = false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        if (DisplayMsg)
                        {
                            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        }
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }
        public void DG_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(PRGID.Text) && PRGID.Text != "0")
            {
                // The SQL query is updated to target DTL_MANF_SUB_DAY and join STUF_DEF for the name.
                const string SQL_QUERY = @"
                    SELECT dbo.QPROGPAS2.PRGID,
                           dbo.QPROGPAS2.CODB,
                           dbo.QPROGPAS2.VAHED,
                           dbo.QPROGPAS2.SumOfMEGH AS MEGH,
                           dbo.QPROGPAS2.SumOfMEGHK AS MEGHK,
                           dbo.QPROGPAS2.SumOfPERT AS PERT,
                           dbo.QPROGPAS2.SumOfKOLMAV AS KOLMAV,
                           dbo.QPROGPAS2.PASED,
                           dbo.QPROGPAS2.RADAH,
                           dbo.STUF_DEF.NAME,
                           dbo.QPROGPAS2.SumOfMABL,
                           dbo.QPROGPAS2.SumOfMABLK
                    FROM dbo.QPROGPAS2
                        INNER JOIN dbo.STUF_DEF
                            ON dbo.QPROGPAS2.CODB = dbo.STUF_DEF.CODE
                    WHERE (dbo.QPROGPAS2.PRGID = @PRGID)";
                var parameters = new Dictionary<string, object>
                {
                    { "@PRGID", int.Parse(PRGID.Text) } // Use 'Fumb' to match the query and parse to int
                };
                var QRE_LST = dbms.DoGetDataSQL<DTL_MANF_SUB_DAY>(SQL_QUERY, parameters).ToList();
                SUB_DATA?.Clear();
                foreach (var item in QRE_LST)
                {
                    SUB_DATA.Add(item);
                }

                Summer();
            }
            else
            {
                SUB_DATA?.Clear();
            }
        }
        private void Summer()
        {
            SUM_AVALIEH.Text = MAVADR.ToString(); //جمع مواد اولیه مصرفی

            IMBIBE_MANF.Text = DASTR.ToString(); //جذب هزينه دستمزد 

            IMBIBE_SAR.Text = SARR.ToString(); //جذب هزینه سربار 

            //جمع مواد + جمع دستمزد + جمع سربار
            var TotalCost = MAVADR + DASTR + SARR;

            SUM_TAMAMSHODEH.Text = TotalCost.ToString(); //قیمت تمام شده استاندارد
        }

        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || DG_SUB == null || DG_SUB.CurrentCell == null)
                return;

            string CURRENT_COLUMN_NAME = "";
            if (DG_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = DG_SUB.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                try
                {
                    var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;
                    if (isEditing) { return; }
                    // 1) اگر داخل یک TextBox در حالت ویرایش هستیم، کاری نکنیم
                    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
                    {
                        // اجازه بدهید Delete عادی متن کارش رو بکنه
                        return;
                    }
                    //else
                    //{
                    //    // اگر داخل حالت ویرایش سلول هستیم، از رفتار پیش‌فرض Delete (حذف کاراکتر) استفاده کن
                    //    var cell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                    //    if (cell != null && cell.IsEditing)
                    //        return;
                    //}
                }
                catch { }

                e.Handled = true;
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME == "SMABL" || CURRENT_COLUMN_NAME == "MABLK")
                {
                    e.Handled = true;
                    var text = "000";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
            if (e.Key == Key.Subtract)
            {
                if (CURRENT_COLUMN_NAME == "SMABL" || CURRENT_COLUMN_NAME == "MABLK")
                {
                    e.Handled = true;
                    var text = "00";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
        }
        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(DG_SUB.Items.Count < 1) && !(DG_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = DG_SUB.SelectedIndex;
                }
            }
        }
        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && DG_SUB.SelectedItem != null)
            {
                if (DG_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = DG_SUB.SelectedIndex;
                }

                if (!(e is null) && DG_SUB.SelectedItem is not null)
                {
                    var view = DG_SUB.Items as IEditableCollectionView;
                    if (view.IsAddingNew) { return; }

                    if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((DTL_MANF_SUB_DAY)DG_SUB.SelectedItem).Clone() as DTL_MANF_SUB_DAY;
                    }
                }
            }
        }
        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is TOZIE_SUB rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = DG_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as DTL_MANF_SUB_DAY;
        }
        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.InvokeAsync(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_SUB.CancelEdit();
                }
                else
                {
                    DG_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as DTL_MANF_SUB_DAY;
        }

        private void ValidateUpdateCurrentMeghk()
        {
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.EditingElement == null || e.Column == null) { return; }

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

            CURRENT_ITEMS_ROW = e.Row.Item as DTL_MANF_SUB_DAY;
            #endregion     
        }

        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var TheRow = e.Row.Item as DTL_MANF_SUB_DAY;
            if (ConstructorRowDetector.IsPristine(TheRow)) { DG_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(TheRow))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            Summer();
        }

        private void ClearFreshAll()
        {
            PRGID.Text = "0";
            PRGID.Tag = null;

            PCODE.SelectedIndex = -1;

            PRG_DATE.Text = Tarikh.FullCurrentDate;
            SA_HOUR.Text = "0";
            SA_NHOU.Text = "0";
            IMBIBE_MANF.Text = "0";
            IMBIBE_SAR.Text = "0";
            TOZIH.Text = null;

            SUB_DATA?.Clear();

            SUM_AVALIEH.Text = "0";
            SUM_TAMAMSHODEH.Text = "0";

            //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.Text = null;
            //PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            //SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            //SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            //SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            Form_Current();

            AllowEdits = true;

            DG_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

        private void CHAP_BTN_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
