using Interfaces;
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Wins.WinMenus.HESABDARI
{
    public partial class DEED_SEARCH_MAIN : Window
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

        #region LOCAL
        public class COMBOITEM
        {
            public string NAMES { get; set; }
        }
        #endregion

        public DEED_SEARCH_MAIN()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<COMBOITEM> KOLY_ITEMS { get; set; } = new ObservableCollection<COMBOITEM>
        {
            new COMBOITEM {NAMES = "="  },
            new COMBOITEM {NAMES = ">"  },
            new COMBOITEM {NAMES = "<"  },
            new COMBOITEM {NAMES = "<>" },
            new COMBOITEM {NAMES = "<=" },
            new COMBOITEM {NAMES = ">=" },
            new COMBOITEM {NAMES = "بين"},
        };
        public ObservableCollection<COMBOITEM> ADADY_ITEMS { get; set; } = new ObservableCollection<COMBOITEM>
        {
            new COMBOITEM {NAMES = "="  },
            new COMBOITEM {NAMES = ">"  },
            new COMBOITEM {NAMES = "<"  },
            new COMBOITEM {NAMES = "<>" },
            new COMBOITEM {NAMES = "<=" },
            new COMBOITEM {NAMES = ">=" },
        };
        public ObservableCollection<COMBOITEM> MATNY_ITEMS { get; set; } = new ObservableCollection<COMBOITEM>
        {
            new COMBOITEM {NAMES = "شامل"},
            new COMBOITEM {NAMES = "بدون"},
            new COMBOITEM {NAMES = "="},
            new COMBOITEM {NAMES = "<>"},
        };

        public ObservableCollection<COMBOITEM> TPVY { get; set; } = new ObservableCollection<COMBOITEM>
        {
            new COMBOITEM {NAMES = "یا"},
            new COMBOITEM {NAMES = "و"},
        };

        private const string BEYN = "بین";
        public bool NowIsReady { get; private set; }
        public bool NewRecord { get; set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private string SHART;

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
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //TextBox.IsReadOnly = !ican;

                //ComboBox.IsEnabled = ican;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            DATE_S.SelectAll();
            DATE_S.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
        }
        public void Form_Current()
        {
        }
        public void ReGetMasterData()
        {
        }

        private void GO_RPT_Click(object sender, RoutedEventArgs e)
        {
            Creatshart();

            //var replacedShart = SHART.Replace("ی", "ي").Replace("ک", "ك");
            var replacedShart = CL_HESABDARI.Fixp(SHART);
            new DEED_SERCH_CREATE(replacedShart).ShowDialog();
            //OpenForm("DEED_SERCH_CREATE", false, $"({SHART}) or ({replacedShart})"); ???????????????????????????

            this.Close();
        }

        private void ReAddSharts(string CMB_TEXT)
        {
            if (NowIsReady)
            {
                Creatshart();
                ResetFields();
                if (!string.IsNullOrEmpty(SHART))
                {
                    if (CMB_TEXT.Equals("و"))
                    {
                        SHART += " AND ";
                    }
                    else
                    {
                        SHART += " OR ";
                    }
                }
            }
        }

        private void YA_BTN_Click(object sender, RoutedEventArgs e)
        {
            ReAddSharts("یا");
        }
        private void VA_BTN_Click(object sender, RoutedEventArgs e)
        {
            ReAddSharts("و");
        }

        private void DATE_SB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    DATE_ST.Text = string.Empty;
                }
                DATE_ST.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                DATE_ST_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void N_SB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    N_ST.Text = string.Empty;
                }
                N_ST.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                N_ST_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void HES_KB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    HES_KT.Text = string.Empty;
                }
                HES_KT.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                HES_KT_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void HES_MB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    HES_MT.Text = string.Empty;
                }
                HES_MT.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                HES_MT_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void TNUMBERB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    TNUMBERT.Text = string.Empty;
                }
                TNUMBERT.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                TNUMBERT_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void BEDB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    BEDT.Text = string.Empty;
                }
                BEDT.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                BEDT_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void BESB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    BEST.Text = string.Empty;
                }
                BEST.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                BEST_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void NUMBERB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CMB)
            {
                var CMB_TEXT = ((COMBOITEM)CMB.SelectedItem).NAMES.FixPersianChars();
                var _VICO_ = CMB_TEXT.Equals(BEYN.FixPersianChars());
                if (_VICO_)
                {
                    NUMBERT.Text = string.Empty;
                }
                NUMBERT.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
                NUMBERT_LBL.Visibility = _VICO_ ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Creatshart()
        {
            // Reset SHART
            SHART = string.Empty;

            if (!string.IsNullOrEmpty(DATE_S.Text.ToRawTarikh()))
            {
                if (DATE_SB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(DATE_ST.Text.ToRawTarikh()))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(DATE_S BETWEEN '{DATE_S.Text.ToRawTarikh()}' AND '{DATE_ST.Text.ToRawTarikh()}')";
                }
                else
                {
                    SHART += $"(DATE_S = {DATE_SB.Text.ToRawTarikh()} '{DATE_S.Text.ToRawTarikh()}')";
                }
            }

            if (!string.IsNullOrEmpty(N_S.Text))
            {
                Chshart();
                if (N_SB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(N_ST.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(N_S BETWEEN {N_S.Text} AND {N_ST.Text})";
                }
                else
                {
                    SHART += $"(N_S {N_SB.Text} {N_S.Text})";
                }
            }

            if (!string.IsNullOrEmpty(HES_K.Text))
            {
                Chshart();
                if (HES_KB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(HES_KT.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(HES_K BETWEEN {HES_K.Text} AND {HES_KT.Text})";
                }
                else
                {
                    SHART += $"(HES_K {HES_KB.Text} {HES_K.Text})";
                }
            }

            if (!string.IsNullOrEmpty(HES_M.Text))
            {
                Chshart();
                if (HES_MB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(HES_MT.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(HES_M BETWEEN {HES_M.Text} AND {HES_MT.Text})";
                }
                else
                {
                    SHART += $"(HES_M {HES_MB.Text} {HES_M.Text})";
                }
            }

            if (!string.IsNullOrEmpty(TNUMBER.Text))
            {
                Chshart();
                if (TNUMBERB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(TNUMBERT.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(TNUMBER BETWEEN {TNUMBER.Text} AND {TNUMBERT.Text})";
                }
                else
                {
                    SHART += $"(TNUMBER {TNUMBERB.Text} {TNUMBER.Text})";
                }
            }

            if (!string.IsNullOrEmpty(SHARH.Text))
            {
                Chshart();
                switch (SHARHB.Text)
                {
                    case "=":
                        SHART += $"(SHARH = N'{SHARH.Text}')";
                        break;
                    case "<>":
                        SHART += $"(SHARH <> N'{SHARH.Text}')";
                        break;
                    case "شامل":
                        SHART += $"(SHARH LIKE N'%{SHARH.Text}%')";
                        break;
                    case "بدون":
                        SHART += $"(SHARH NOT LIKE N'%{SHARH.Text}%')";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(BED.Text))
            {
                Chshart();
                if (BEDB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(BEDT.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(BED BETWEEN {BED.Text} AND {BEDT.Text})";
                }
                else
                {
                    SHART += $"(BED {BEDB.Text} {BED.Text})";
                }
            }

            if (!string.IsNullOrEmpty(BES.Text))
            {
                Chshart();
                if (BESB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(BEST.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(BES BETWEEN {BES.Text} AND {BEST.Text})";
                }
                else
                {
                    SHART += $"(BES {BESB.Text} {BES.Text})";
                }
            }

            if (!string.IsNullOrEmpty(N_SERI.Text))
            {
                Chshart();
                SHART += $"(N_SERI {N_SERIB.Text} {N_SERI.Text})";
            }

            if (!string.IsNullOrEmpty(BANK.Text))
            {
                Chshart();
                SHART += $"(BANK {BANKB.Text} {BANK.Text})";
            }

            if (!string.IsNullOrEmpty(NUMBER.Text))
            {
                Chshart();
                if (NUMBERB.Text.Equals(BEYN.FixPersianChars()))
                {
                    if (string.IsNullOrEmpty(NUMBERT.Text))
                    {
                        new Msgwin(false, "پارامترها كافي نيست!").ShowDialog();
                        return;
                    }
                    SHART += $"(NUMBER BETWEEN {NUMBER.Text} AND {NUMBERT.Text})";
                }
                else
                {
                    SHART += $"(NUMBER {NUMBERB.Text} {NUMBER.Text})";
                }
            }

            if (!string.IsNullOrEmpty(HTAG.Text))
            {
                Chshart();
                SHART += $"(TAG {TAGB.Text} {HTAG.Text})";
            }

            if (!string.IsNullOrEmpty(hes.Text))
            {
                Chshart();
                switch (hesB.Text)
                {
                    case "=":
                        SHART += $"(HES = N'{hes.Text}')";
                        break;
                    case "<>":
                        SHART += $"(HES <> N'{hes.Text}')";
                        break;
                    case "شامل":
                        SHART += $"(HES LIKE N'%{hes.Text}%')";
                        break;
                    case "بدون":
                        SHART += $"(HES NOT LIKE N'%{hes.Text}%')";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(SHART))
            {
                SHART = $"({SHART})";
            }
        }
        private void Chshart()
        {
            if (!string.IsNullOrEmpty(SHART) && !SHART.Trim().EndsWith("AND") && !SHART.Trim().EndsWith("OR"))
            {
                SHART += " AND ";
            }
        }
        private void ResetFields()
        {
            DATE_S.Text = null;
            N_S.Text = null;
            HES_K.Text = null;
            HES_M.Text = null;
            TNUMBER.Text = null;
            SHARH.Text = null;
            BED.Text = null;
            BES.Text = null;
            N_SERI.Text = null;
            BANK.Text = null;
            NUMBER.Text = null;
            HTAG.Text = null;
            hes.Text = null;
            DATE_ST.Text = null;
            N_ST.Text = null;
            HES_KT.Text = null;
            HES_MT.Text = null;
            TNUMBERT.Text = null;
            BEDT.Text = null;
            BEST.Text = null;
            NUMBERT.Text = null;
        }

        private void CLEAR_BTN_Click(object sender, RoutedEventArgs e)
        {
            SHART = null;
            ResetFields();
        }
    }
}
