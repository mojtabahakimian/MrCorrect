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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    /// <summary>
    /// Interaction logic for WIN_GSCALE.xaml
    /// </summary>
    public partial class WIN_GSCALE : Window, ISearchableWindow
    {
        public WIN_GSCALE(int? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (int)number_to_open;
                GSCACOD.Text = NUMBER_TO_OPEN?.ToString();
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;

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

        public bool NowIsReady { get; private set; }
        public int? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {
                _bl = value;

                // Get the window handle
                IntPtr handle = WINDOW_ID;

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

                GSCACOD.IsReadOnly = !ican; //کد اِسکِیل
                GSCANAME.IsReadOnly = !ican; //عنوان
                DG_SUB.IsReadOnly = !ican; //دیتاگرید

                GSCAKIND.IsEnabled = ican; //نوع


            }
        }

        public nint WINDOW_ID { get; private set; }

        private NavigationManager<GSCALE> _navigationManager; //Head/Master

        public ObservableCollection<GSCADTL> DG_DATA { get; set; } = new ObservableCollection<GSCADTL>(); //Detail
        public GSCADTL? WAS_ROW_ITEM { get; private set; } = new();
        public string? ENTERED_VALUE_ROW { get; private set; }
        public GSCADTL? CURRENT_ROW_ITEMS { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WINDOW_ID = new WindowInteropHelper(this).Handle;

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "GSCALE", WINDOW_ID, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            string WhereCondition = "";
            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE GSCACOD = {GSCACOD.Text} ";
            }

            _navigationManager = new NavigationManager<GSCALE>(
                dbms,
                x => x?.GSCACOD.ToString(),
                $"SELECT * FROM dbo.GSCALE {WhereCondition} ORDER BY CRT",
                x => $"SELECT * FROM dbo.GSCALE WHERE GSCACOD = {x?.GSCACOD} ",
                Convert.ToDouble(GSCACOD.Text));

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            CL_LMethods.SetTabIndexes(
                GSCACOD,
                GSCANAME,
                GSCAKIND,
                BTN_SAVE,
                DG_SUB
                );

            MakeDefaultFocuseReady();
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
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 2;
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[1]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                    var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                    if (focusedWindow != null)
                                    {
                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            focusedWindow.Activate();
                                            focusedWindow.Focus();
                                        }), DispatcherPriority.Background);
                                    }

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    catch { /*ignore*/ }

                }
                else if (BTN_SAVE.IsFocused)
                {
                    BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (!DG_SUB.IsKeyboardFocusWithin && !DG_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }
            else
            {
                if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    DataGridExtension.HandleKeyPress(sender, e, DG_SUB);
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
        private void FILL_ALL_COMBOBOXES()
        {
            GSCAKIND.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "کمی" },
                new COMBOYMODEL { ID = 1, NAME = "کیفی" },
            };
        }
        private void MakeDefaultFocuseReady()
        {
            GSCANAME.Focus();
            GSCANAME.SelectAll();
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

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

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is GSCALE item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.GSCACOD.Equals(item.GSCACOD));
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;

                        // 1) Find its index in the master list
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx < 0)
                        {
                            // not found (perhaps filtered out?), bail out
                            new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                            return;
                        }

                        // 2) Tell the navigation manager to move to that position
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);

                        //OnCurrentRecordChanged(itemfound);
                    }
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "کد اِسکِیل", PropertyPath = "GSCACOD", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "نام", PropertyPath = "GSCANAME", PropertyType = typeof(string) },
            };
        }
        #endregion

        private void DataGridActivation()
        {
            if (_navigationManager.IsNewRecord)
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                DG_SUB.IsReadOnly = false;
            }
        }
        private void ClearFreshAll()
        {
            GSCACOD.Text = "0";
            GSCANAME.Text = null;
            GSCAKIND.Text = null; GSCAKIND.SelectedIndex = -1; ;

            ESLAH.IsEnabled = false;

            DG_DATA?.Clear();

            AllowEdits = true;

            DG_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(GSCALE record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<GSCALE>($"SELECT * FROM GSCALE WHERE GSCACOD = {GSCACOD.Text}").FirstOrDefault();
                record = itemtoadd;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(GSCALE HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, PRICE_ELAMIE_DTL_DATA);
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین آیتمی ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                GSCACOD.Text = HEADER_FAC.GSCACOD.ToString(); //کد اِسکِیل
                GSCANAME.Text = HEADER_FAC.GSCANAME; //عنوان
                GSCAKIND.SelectedValue = HEADER_FAC.GSCAKIND; GSCAKIND.Items.Refresh(); //نوع

                ESLAH.IsEnabled = true;

                AllowEdits = false;

                DG_SUB_ReGetData();
            }
        }
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<GSCALE>($"SELECT * FROM GSCALE WHERE GSCACOD = {GSCACOD.Text}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                DG_SUB_CANCEL_EDIT(); ApplyDataGridItems(); DG_SUB.Items.Refresh();
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            try
            {
                if (!DoCmdHeaderSave())
                {
                    return;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"این عنوان قبلا تعریف شده نمیتوان عنوان تکراری تعریف کرد").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }

            this.DG_SUB.IsReadOnly = false;
            ESLAH.IsEnabled = true;

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (DG_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private bool DoCmdHeaderSave(bool displayMsg = true)
        {
            int gscacod = Convert.ToInt32(string.IsNullOrWhiteSpace(GSCACOD.Text) ? "0" : GSCACOD.Text);

            if (_navigationManager.IsNewRecord)
            {
                gscacod = (int)CL_HESABDARI.GetLIDD("GSCALE", "GSCACOD");
            }

            string gscaname = GSCANAME.Text?.Trim();
            if (string.IsNullOrWhiteSpace(gscaname))
            {
                if (displayMsg)
                {
                    Msgwin msg = new Msgwin(true, "نام نمی‌تواند خالی باشد");
                    _ = msg.ShowDialog();
                }
                return false;
            }

            float gscakind;
            if (!float.TryParse(GSCAKIND.Text, out gscakind))
                gscakind = 0;

            var model = new GSCALE
            {
                GSCACOD = gscacod,
                GSCANAME = gscaname,
                GSCAKIND = gscakind,
                CRT = DateTime.Now,
                UID = Baseknow.USERCOD
            };

            var exists = dbms.DoGetDataSQL<int?>(
                @"SELECT 1 FROM GSCALE 
                  WHERE GSCANAME = @GSCANAME 
                  AND GSCACOD <> @GSCACOD",
                new { GSCANAME = gscaname, GSCACOD = gscacod }
            ).FirstOrDefault();

            if (_navigationManager.IsNewRecord && exists != null)
            {
                Msgwin msg = new Msgwin(true, $"نام '{gscaname}' از قبل وجود دارد");
                _ = msg.ShowDialog();
                return false;
            }

            if (_navigationManager.IsNewRecord)
            {
                string insertSql = @"
            INSERT INTO GSCALE
                (GSCACOD, GSCANAME, GSCAKIND, CRT, UID)
            VALUES
                (@GSCACOD, @GSCANAME, @GSCAKIND, @CRT, @UID)";

                dbms.DoExecuteSQL(insertSql, model);
                GSCACOD.Text = gscacod.ToString();
                RefreshAfterUpdate();
            }
            else
            {
                string updateSql = @"
                UPDATE GSCALE
                SET
                    GSCANAME = @GSCANAME,
                    GSCAKIND = @GSCAKIND,
                WHERE
                GSCACOD = @GSCACOD";

                dbms.DoExecuteSQL(updateSql, model);
            }

            return true;
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            AllowEdits = true;
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var isVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (_navigationManager.IsNewRecord || !DG_SUB.IsEnabled || !BTN_DELETE.IsEnabled || !isVisible)
                return;

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

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true) return;

            bool isDeletedSomething = false;
            List<MsgModel> errors = new List<MsgModel>();

            // ---------- حذف جزئیات ----------
            if (DG_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                {
                    var item = DG_SUB.SelectedItems[i];

                    if (item is not GSCADTL dtl)
                        continue;

                    // Placeholder جدید
                    if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                    {
                        DG_DATA.Remove(dtl);
                        continue;
                    }

                    try
                    {
                        dbms.DoExecuteSQL(
                            @"DELETE FROM dbo.GSCADTL WHERE GSCADTCOD = @GSCADTCOD",
                            new { dtl.GSCADTCOD });

                        isDeletedSomething = true;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                            errors.Add(new MsgModel { MessageText_U = "این آیتم دارای وابستگی است و قابل حذف نیست" });
                        else
                            errors.Add(new MsgModel { MessageText_U = "خطای پایگاه داده در حذف آیتم" });
                    }
                    catch
                    {
                        errors.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف آیتم" });
                    }
                }

                if (errors.Any())
                {
                    new MsgListwin(false, errors).ShowDialog();
                }
                else if (isDeletedSomething)
                {
                    DG_SUB_ReGetData();
                }

                return;
            }

            // ---------- حذف هدر (GSCALE) ----------
            if (!_navigationManager.IsNewRecord)
            {
                try
                {
                    int gscacod = Convert.ToInt32(GSCACOD.Text);

                    dbms.DoExecuteSQL(
                        @"DELETE FROM dbo.GSCALE WHERE GSCACOD = @GSCACOD",
                        new { GSCACOD = gscacod });

                    _navigationManager?.DeleteCurrentRecord();
                }
                catch (SqlException ex)
                {
                    if (e != null)
                        e.Handled = true;

                    if (ex.Number == 547)
                    {
                        new Msgwin(false, "این دارای آیتم‌های وابسته است، ابتدا جزئیات آن را حذف کنید").ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "حذف به دلیل خطای پایگاه داده انجام نشد").ShowDialog();
                    }
                }
                catch
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
                }
            }
        }

        public void DG_SUB_ReGetData()
        {
            if (!_navigationManager.IsNewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<GSCADTL>(@$"SELECT * FROM dbo.GSCADTL WHERE GSCACOD = @GSCACOD ORDER BY CRT", new { GSCACOD = GSCACOD.Text }).ToList();

                DG_DATA?.Clear();
                foreach (var item in QRE_LST)
                    DG_DATA?.Add(item);
            }
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.Invoke(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_SUB.CancelEdit();
                    //DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                }
                else
                {
                    DG_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                    //DG_SUB.CommitEdit((DataGridEditingUnit)_RC_, true);
                }
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }
        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;
            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "GSCANAME", DEFINDX, true);
        }
        private bool BodyIsValid(GSCADTL theRow)
        {
            var hasGridErrors =
                (from object i in DG_SUB.ItemsSource
                 let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                 where c != null && Validation.GetHasError(c)
                 select c).Any();

            if (hasGridErrors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> errors = new List<MsgModel>();

            if (theRow == null)
                errors.Add(new MsgModel { MessageText_U = "رکورد نامعتبر است" });

            if (string.IsNullOrWhiteSpace(theRow?.GSCANAME))
                errors.Add(new MsgModel { MessageText_U = "عنوان نمی‌تواند خالی باشد" });

            if (theRow?.GSCAGRADE < 0 || theRow?.GSCAGRADE > 100)
                errors.Add(new MsgModel { MessageText_U = "امتیاز باید بین 0 تا 100 باشد" });

            if (theRow?.GSCAFROM < 0 || theRow?.GSCATO < 0)
                errors.Add(new MsgModel { MessageText_U = "محدوده مقادیر نمی‌تواند منفی باشد" });

            if (theRow?.GSCAFROM > theRow?.GSCATO)
                errors.Add(new MsgModel { MessageText_U = "مقدار شروع نمی‌تواند بزرگتر از مقدار پایان باشد" });

            //if (theRow?.GSCACOD <= 0)
            //    errors.Add(new MsgModel { MessageText_U = "کد مقیاس اصلی مشخص نیست" });

            if (errors.Count > 0)
            {
                errors = errors
                    .Select(x => x.MessageText_U)
                    .Distinct()
                    .Select(m => new MsgModel { MessageText_U = m })
                    .ToList();

                new MsgListwin(false, errors).ShowDialog();
                return false;
            }

            return true;
        }

        private void ApplyDataGridItems()
        {
            if (DG_SUB.Items is IEditableCollectionView editableCollectionView)
            {
                if (editableCollectionView.IsAddingNew)
                {
                    editableCollectionView.CancelNew(); // discard the new item
                }
                if (editableCollectionView.IsEditingItem)
                {
                    editableCollectionView.CommitEdit(); // commit the edit transaction
                }
            }
        }
        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (DG_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = DG_SUB.CurrentCell.Column?.SortMemberPath;
            }
            else
            {
                return;
            }


            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                DataGridExtension.HandleKeyPress(sender, e, DG_SUB);
            }

            string ColumnTarget = "";
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME.Contains("PRICE1", StringComparison.OrdinalIgnoreCase))
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
                if (CURRENT_COLUMN_NAME.Contains("PRICE1", StringComparison.OrdinalIgnoreCase))
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

            if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            {
                try
                {
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
                BTN_DELETE_Click(null, null);
            }
        }
        private bool IsSubDataNull()
        {
            if (DG_SUB != null && DG_SUB?.Items?.Count > 0 && DG_DATA?.Count > 0)
            {
                return false;
            }
            return true;
        }
        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is GSCADTL rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = DG_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as GSCADTL;
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.EditingElement == null || e.Column == null)
            {
                return;
            }

            #region REFILL_CURRENTS
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

            CURRENT_ROW_ITEMS = e.Row.Item as GSCADTL;
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

            #endregion
        }
        bool IsSaveSuccess = true;
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) return;
            if (Keyboard.IsKeyDown(Key.Escape)) return;

            if (!HeaderIsValid())
            {
                IsSaveSuccess = false;
                DG_SUB_CANCEL_EDIT();
                return;
            }

            var ROW = e.Row.Item as GSCADTL;
            if (ROW == null) return;

            // اگر سطر دست نخورده
            if (ConstructorRowDetector.IsPristine(ROW))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            IsSaveSuccess = false;

            if (!BodyIsValid(ROW))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            // اتصال به هدر
            ROW.GSCACOD = Convert.ToInt32(GSCACOD.Text);
            ROW.UID = Baseknow.USERCOD;
            ROW.CRT = DateTime.Now;

            int? newId = null;

            try
            {
                if (ROW?.GSCADTCOD == null || ROW.GSCADTCOD == 0) // INSERT
                {
                    // بررسی تکراری در حافظه (بر اساس نام)
                    bool duplicateInMemory =
                        DG_DATA.Count(x => x.GSCANAME == ROW.GSCANAME) > 1;

                    if (duplicateInMemory)
                    {
                        DG_SUB_CANCEL_EDIT();
                        universControl.PopNotifyShow("این عنوان قبلاً در لیست اضافه شده است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    // بررسی تکراری در دیتابیس
                    var duplicateInDb = dbms.DoGetDataSQL<GSCADTL>(
                        @"SELECT TOP 1 * FROM dbo.GSCADTL
                        WHERE GSCACOD = @GSCACOD
                          AND GSCANAME = @GSCANAME",
                              new { ROW.GSCACOD, ROW.GSCANAME }
                          ).FirstOrDefault();

                    if (duplicateInDb != null)
                    {
                        DG_SUB_CANCEL_EDIT();
                        universControl.PopNotifyShow(
                            "این عنوان قبلاً ثبت شده است",
                            Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    newId = (int)CL_HESABDARI.GetLIDD("GSCADTL", "GSCADTCOD");

                    var insertModel = new GSCADTL
                    {
                        GSCADTCOD = (int)newId,
                        GSCANAME = ROW.GSCANAME,
                        GSCAGRADE = ROW.GSCAGRADE,
                        GSCAFROM = ROW.GSCAFROM,
                        GSCATO = ROW.GSCATO,
                        GSCACOD = ROW.GSCACOD,
                        CRT = ROW.CRT,
                        UID = ROW.UID
                    };

                    dbms.DoExecuteSQL(@"
                INSERT INTO GSCADTL
                    (GSCADTCOD, GSCANAME, GSCAGRADE, GSCAFROM, GSCATO, GSCACOD, CRT, UID)
                VALUES
                    (@GSCADTCOD, @GSCANAME, @GSCAGRADE, @GSCAFROM, @GSCATO, @GSCACOD, @CRT, @UID)",
                        insertModel);
                }
                else // UPDATE
                {
                    bool duplicateInMemory =
                        DG_DATA.Count(x =>
                            x.GSCANAME == ROW.GSCANAME &&
                            x.GSCADTCOD != ROW.GSCADTCOD) > 0;

                    if (duplicateInMemory)
                    {
                        DG_SUB_CANCEL_EDIT();
                        universControl.PopNotifyShow(
                            "عنوان تکراری است",
                            Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    var updateModel = new GSCADTL
                    {
                        GSCADTCOD = ROW.GSCADTCOD,
                        GSCANAME = ROW.GSCANAME,
                        GSCAGRADE = ROW.GSCAGRADE,
                        GSCAFROM = ROW.GSCAFROM,
                        GSCATO = ROW.GSCATO,
                        UID = ROW.UID
                    };

                    dbms.DoExecuteSQL(@"
                        UPDATE GSCADTL
                        SET
                            GSCANAME = @GSCANAME,
                            GSCAGRADE = @GSCAGRADE,
                            GSCAFROM = @GSCAFROM,
                            GSCATO = @GSCATO,
                            UID = @UID
                        WHERE
                            GSCADTCOD = @GSCADTCOD",
                                updateModel);
                }
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                    new Msgwin(false, "آیتم تکراری وارد شده است").ShowDialog();
                else
                    new Msgwin(false, "خطا در ذخیره اطلاعات").ShowDialog();

                return;
            }
            catch (Exception)
            {
                DG_SUB_CANCEL_EDIT();
                new Msgwin(false, "خطای غیرمنتظره در ذخیره").ShowDialog();
                return;
            }

            if (newId != null) // بسیار مهم
                ROW.GSCADTCOD = (int)newId;

            IsSaveSuccess = true;
        }


        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //MouseRightButtonUp="DG_SUB_MouseRightButtonUp"
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
            catch (Exception)
            {
            }
        }
        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!NowIsReady || (e is null))
                return;

            var selectedItem = DG_SUB.SelectedItem;

            if (selectedItem == null || Equals(selectedItem, CollectionView.NewItemPlaceholder))
                return;

            if (selectedItem is GSCADTL detailRow)
            {
                WAS_ROW_ITEM = detailRow.Clone() as GSCADTL;
            }
        }
        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
