using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using System.Collections.ObjectModel;
using Prg_Proccessy.SQLMODELS;
using DocumentFormat.OpenXml.Bibliography;
using Prg_Proccessy.Generaly;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for GOZARESH_FROOSH_MAHSUL.xaml
    /// </summary>
    public partial class GOZARESH_FROOSH_MAHSUL : Window
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

        public GOZARESH_FROOSH_MAHSUL(string dT1, string dT2)
        {
            InitializeComponent();

            this.DataContext = this;

            DT1 = dT1;
            DT2 = dT2;
        }

        string DT1;
        string DT2;

        public Visual I_AM_GOZARESH_FROOSH_MAHSUL { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        public ObservableCollection<DEPART_MODEL> DEPART_DATA { get; set; } = new ObservableCollection<DEPART_MODEL>();
        public ObservableCollection<FROOSH_COUNT> FROOSH_COUNT_DATA { get; set; } = new ObservableCollection<FROOSH_COUNT>();

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                if (_ican is true) // Is Enable and ReadOnly = False
                {

                }
                else
                {

                }
            }
        }

        private double _sum_of_mabl_k = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _sum_of_mabl_k = (double)FROOSH_COUNT_DATA.Sum(r => r.SMABL);
                if (_sum_of_mabl_k == 0) _sum_of_mabl_k = 0;
                return _sum_of_mabl_k;
            }
            set { _sum_of_mabl_k = value; }
        }

        private double _sum_of_tedad = 0;
        public double SUM_OF_TEDAD
        {
            get
            {
                _sum_of_tedad = (double)FROOSH_COUNT_DATA.Sum(r => r.TEDAD);
                if (_sum_of_tedad == 0) _sum_of_tedad = 0;
                return _sum_of_tedad;
            }
            set { _sum_of_tedad = value; }
        }

        private double _sum_of_smeghk = 0;
        public double SUM_OF_SMEGHk
        {
            get
            {
                _sum_of_smeghk = (double)FROOSH_COUNT_DATA.Sum(r => r.SMEGHk);
                if (_sum_of_smeghk == 0) _sum_of_smeghk = 0;
                return _sum_of_smeghk;
            }
            set { _sum_of_smeghk = value; }
        }

        public class Q1
        {
            public int? DEPATMAN { get; set; }
            public string? DEPNAME { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_GOZARESH_FROOSH_MAHSUL = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            var RST = dbms.DoGetDataSQL<DEPART_MODEL>(@$"SELECT * FROM DEPART").ToList();
            DEPART_DATA?.Clear();
            foreach (var item in RST)
            {
                DEPART_DATA.Add(item);
            }


            Text42.Text = SUM_OF_MABL_K.ToString();
            Text41.Text = SUM_OF_TEDAD.ToString();
        }

        private void Fill_ComboBoxes()
        {
            //DEPATMAN.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT DEPATMAN, DEPNAME FROM DEPART").ToList();
            //DEPATMAN.DisplayMemberPath = "DEPNAME";
            //DEPATMAN.SelectedValuePath = "DEPATMAN";
        }

        private void Command46_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Command47_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = DEPART_SUB.SelectedItem as DEPART_MODEL;

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Kharid_Froosh.froosh_vaheds.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.ToString();
            report["EDATE_PARM"] = DT2.ToString();
            report["DEPART_PARM"] = CurrentRow.DEPATMAN.ToString();

            (report.GetComponentByName("DT1_N") as StiText).Text = DT1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = DT2.ToString();

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "گزارش فروش روزانه محصولات").Show();
        }

        private void DEPART_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var CurrentRow = DEPART_SUB.SelectedItem as DEPART_MODEL;
            if (CurrentRow != null && CurrentRow?.DEPATMAN != null)
            {
                var RST = dbms.DoGetDataSQL<FROOSH_COUNT>(@$"SELECT * FROM FROOSH_COUNT({DT1},{DT2}) WHERE DEPATMAN = {CurrentRow.DEPATMAN}").ToList();
                FROOSH_COUNT_DATA?.Clear();
                foreach (var item in RST)
                {
                    FROOSH_COUNT_DATA.Add(item);
                }
            }
            Text41.Text = SUM_OF_SMEGHk.ToString();
            Text42.Text = SUM_OF_TEDAD.ToString();
        }
    }
}
