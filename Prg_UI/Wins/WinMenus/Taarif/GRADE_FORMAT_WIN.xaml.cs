using AUTO_BAZ.Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Tarikh = Prg_Proccessy.FUNCTIONS.Tarikh;

namespace Wins.WinMenus.Taarif
{
    public partial class GRADE_FORMAT_WIN : Window
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();

        public Visual WINARG { get; set; }
        public ObservableCollection<GRADE_FORMAT> MASTER_GRADE_DATA { get; set; } = new ObservableCollection<GRADE_FORMAT>();
        public GRADE_FORMAT_WIN(Visual _YOUR_VL_WIN = null)
        {
            InitializeComponent();

            this.DataContext = this;

            WINARG = _YOUR_VL_WIN;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            ArezeshComboColumn.ItemsSource = dbms.DoGetDataSQL<GSCALE>("SELECT GSCACOD, GSCANAME FROM dbo.GSCALE").ToList();

            // Fetch GRADE_FORMAT data
            var gradeFormats = dbms.DoGetDataSQL<GRADE_FORMAT>("SELECT IDD, GFNAME, GFDATE, TOZIH, USERNAME, JAMZARIB, EMTIAZ, CRT, UID FROM GRADE_FORMAT").ToList();
            foreach (var format in gradeFormats)
            {
                // For each GRADE_FORMAT, fetch related GRADE_TAB_FT data
                format.GRADETABFT = dbms.DoGetDataSQL<GRADE_TAB_FT>($"SELECT GFID, GFTID, GFNAMEFT, GFGZARIB, CRT, UID FROM GRADE_TAB_FT WHERE GFID = {format.IDD}").ToList();

                foreach (var tabFt in format.GRADETABFT)
                {
                    // For each GRADE_TAB_FT, fetch related GRADE_GRP_FT data
                    tabFt.GRADEGRPFT = dbms.DoGetDataSQL<GRADE_GRP_FT>($"SELECT GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE, CRT, UID FROM GRADE_GRP_FT WHERE GFTID = {tabFt.GFTID}").ToList();
                }
            }
            // Populate the MASTER_GRADE_TOP collection for binding
            MASTER_GRADE_DATA = new ObservableCollection<GRADE_FORMAT>(gradeFormats);
            GRADE_SUB.ItemsSource = MASTER_GRADE_DATA;
        }

        private void GRADE_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GRADE_SUB.SelectedItem != null && GRADE_SUB.SelectedIndex > -1 && WINARG != null)
            {
                var RowChoosed = GRADE_SUB.SelectedItem as GRADE_FORMAT;

                //(WINARG as AZAE_WIN).MasterData

                string hes = (WINARG as AZAE_WIN).HES.SelectedValue.ToStringNullSafe().Trim();
                var existingRecord = dbms.DoGetDataSQL<GRADE_CUST_TAB>($"SELECT * FROM GRADE_CUST_TAB WHERE GCCUST_HES = '{hes}'").FirstOrDefault();
                if (existingRecord != null)
                {
                    new Msgwin(false, "گريد بندي قبلا انجام شده").ShowDialog();
                }
                else
                {
                    var RSTS = dbms.DoGetDataSQL<GRADE_TAB_FT>("SELECT * FROM GRADE_TAB_FT WHERE GFID = " + RowChoosed.IDD).ToList();
                    for (int i = 0; i < RSTS.Count; i++) //while (!RSTS.EOF)
                    {
                        //rst.AddNew();
                        var GCTAB = (int?)CL_HESABDARI.GetLGradetab();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.GRADE_CUST_TAB(GCTABID, GCNAME, GCZARIB, GCCUST_HES, GCDATE, USERNAME)
                                             VALUES({GCTAB},
                                             N'{RSTS[i].GFNAMEFT}' ,
                                             {RSTS[i].GFGZARIB} ,
                                             N'{Strings.Trim(hes)}' ,
                                             {Tarikh.FullCurrentDate}  ,
                                             N'{(string)CL_HESABDARI.UCurrentUser()}' )");

                        CL_HESABDARI.INSERTGRPGRADE((long)GCTAB, (long)RSTS[i].GFTID);
                        //RSTS.MoveNext();
                    }

                    (WINARG as AZAE_WIN).ReGetData();      //AZAEForm.GRADE_TAB_FORM.Requery();
                    CL_HESABDARI.UpAZAE(hes);
                    this.Close();
                }
            }
        }
    }
}
