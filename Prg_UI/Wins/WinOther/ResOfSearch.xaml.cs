using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.Checkha;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH.GOZARESHAT;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.Checkha.NAZDESANDOGH;
using Wins.WinMenus.HESABDARI;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.KHARID_FORUSH.GOZARESHAT;
using Wins.WinMenus.SANATI;
using Wins.WinMenus.Taarif;
using Wins.WinMenus.WinAutomasion;
using Wins.WinSetting;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using PGET_HED = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED;

namespace Prg_UI.Wins.WinOther
{
    public partial class ResOfSearch : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
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

        string DefaultSelectSearch = "";
        bool IsSelectedItmFromResult = false;
        string shart = "";
        string WhoFinalyCallMe = "";
        public Visual Win_US { get; set; }
        string anbars;

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public Custom_CUST_HESAB SELECTED_HESAB { get; set; }

        public ResOfSearch(string sharty = "", string FormNAM = "", string _defaultselect = "", Visual _YOUR_VL_WIN = default)
        {
            shart = sharty;
            WhoFinalyCallMe = FormNAM;
            DefaultSelectSearch = _defaultselect;
            Win_US = _YOUR_VL_WIN;
            InitializeComponent();
        }
        public static ComboSearch COMBSRCH
        {
            get
            {
                return Application.Current.Windows
                    .OfType<ComboSearch>()
                    .FirstOrDefault();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (IsSelectedItmFromResult == true)
            {
                IsSelectedItmFromResult = false;
                if (!(COMBSRCH is null))
                {
                    COMBSRCH.Close();
                }
            }
        }

        private void ResDGR1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ResDGR1.Items.Count > 0 && ResDGR1.SelectedItem != null)
            {
                e.Handled = true;

                var selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;

                if (selectedVal == null)
                {
                    e.Handled = true;
                    return;
                }

                switch (WhoFinalyCallMe)
                {
                    case "HEAD_LST_PISHFROOSH2":

                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            //Safety
                            if ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.ItemsSource == null)
                            {
                                (Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_PISHFROOSH2).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "F_MENU_KOL_MOIN_TAFZIL":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevaluehes = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevaluehes = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            try
                            {
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.SelectedValue = null;
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.SelectedValue = thevaluehes;

                            }
                            catch (Exception)
                            {
                                //NAME
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT hes,NAME FROM CUST_HESAB").ToList();
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.DisplayMemberPath = "NAME";
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.SelectedValuePath = "hes";

                                //CODE (HES)
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo34.ItemsSource = (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.ItemsSource;
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo34.DisplayMemberPath = "hes";
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo34.SelectedValuePath = "hes";

                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.SelectedValue = null;
                                (Win_US as F_MENU_KOL_MOIN_TAFZIL).Combo36.SelectedValue = thevaluehes;
                            }

                        }
                        break;
                    case "MAIN":
                        if (!(selectedVal is null))
                        {
                            e.Handled = true;
                            MAIN.MAIN_INST.COMP_COD_DATA?.Clear();


                            string hesvla = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesvla = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }
                            CUST_HESAB _nh_ = new CUST_HESAB()
                            {
                                hes = hesvla,
                                NAME = selectedVal.NAME
                            };

                            //(Win_US as MAIN).COMP_COD_DATA.Add(_nh_);
                            MAIN.MAIN_INST.COMP_COD_DATA.Add(_nh_);
                            MAIN.MAIN_INST.COMP_COD.SelectedValue = hesvla;
                            MAIN.MAIN_INST.COMP_COD.SelectedValue = selectedVal.NAME;
                            MAIN.MAIN_INST.COMP_COD.SelectedIndex = 0;
                        }
                        break;
                    case "HEAD_LST_HAVL":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_HAVL).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_HAVL).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_HAVL).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_HAVL).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_HAVL).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "HEAD_LST_RASID":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_RASID).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_RASID).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_RASID).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_RASID).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_RASID).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "HEAD_LST_FROOSH22":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_FROOSH22).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_FROOSH22).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_FROOSH22).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_FROOSH22).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_FROOSH22).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "POSHTE_FACTOR": //پشت فاکتور
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as HEAD_LST_FROOSH22).HESAB_POSHTEF_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as HEAD_LST_FROOSH22).HESAB_POSHTEF_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as HEAD_LST_FROOSH22).HESAB_POSHTEF_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as HEAD_LST_FROOSH22).HESAB_POSHTEF_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as HEAD_LST_FROOSH22).HESAB_POSHTEF_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;
                    case "CREATE_CHEKDP":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as CREATE_CHEKDP).HESAB_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as CREATE_CHEKDP).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as CREATE_CHEKDP).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as CREATE_CHEKDP).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as CREATE_CHEKDP).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;
                    case "CREATE_CHEKPDP":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as CREATE_CHEKPDP).HESAB_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as CREATE_CHEKPDP).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as CREATE_CHEKPDP).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as CREATE_CHEKPDP).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as CREATE_CHEKPDP).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;
                    case "PGET_HED":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as PGET_HED).FROM_SEARCH.HES = hesval;
                            (Win_US as PGET_HED).FROM_SEARCH.NAME = selectedVal.NAME;

                            //if ((Win_US as PGET_HED).FOCUSED_COLUMN_NAME is "FHES")
                            //{
                            //if (!((Win_US as PGET_HED).KHAZANEH_DATA).Any(item => item?.FHES == thevalue))
                            //{
                            //    ((Win_US as PGET_HED).KHAZANEH_DATA).Add(new PGET_LST { FHES = thevalue, NAME_FHES = selectedVal.NAME });
                            //}

                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.FHES = null;
                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.FHES = thevalue;
                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.NAME_FHES = selectedVal.NAME;
                            //}
                        }
                        break;
                    case "DEED_HEAD":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as WinMenus.HESABDARI.DEED_HEAD).FROM_SEARCH.HES = hesval;
                            (Win_US as WinMenus.HESABDARI.DEED_HEAD).FROM_SEARCH.NAME = selectedVal.NAME;

                            //if ((Win_US as PGET_HED).FOCUSED_COLUMN_NAME is "FHES")
                            //{
                            //if (!((Win_US as PGET_HED).KHAZANEH_DATA).Any(item => item?.FHES == thevalue))
                            //{
                            //    ((Win_US as PGET_HED).KHAZANEH_DATA).Add(new PGET_LST { FHES = thevalue, NAME_FHES = selectedVal.NAME });
                            //}

                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.FHES = null;
                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.FHES = thevalue;
                            //(Win_US as PGET_HED).CURRENT_ITMES_ROW.NAME_FHES = selectedVal.NAME;
                            //}
                        }
                        break;
                    case "AZAE":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as AZAE_WIN).FROM_SEARCH.HES = hesval;
                            (Win_US as AZAE_WIN).FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "paymentformorder_CUST_NO":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as paymentformorder).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as paymentformorder).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as paymentformorder).CUST_NO.SelectedValue = null;
                            (Win_US as paymentformorder).CUST_NO.SelectedValue = thevalue;
                            (Win_US as paymentformorder).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "paymentformorder_ORDERER":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as paymentformorder).ORDERER.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as paymentformorder).ORDERER.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as paymentformorder).ORDERER.SelectedValue = null;
                            (Win_US as paymentformorder).ORDERER.SelectedValue = thevalue;
                            (Win_US as paymentformorder).ORDERER.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "HEAD_LST_KHAREED1":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KHAREED1).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KHAREED1).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_KHAREED1).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_KHAREED1).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_KHAREED1).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;

                    case "HEAD_LST_KHADAMAT":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KHADAMAT).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KHADAMAT).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_KHADAMAT).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_KHADAMAT).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_KHADAMAT).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;

                    case "HEAD_LST_KH_BACK_AZAD":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.SelectedValue = null;
                            (Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;

                    case "POSHTE_FACTOR_AZADF": //پشت فاکتور
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as HEAD_LST_BRFR).HESAB_POSHTEF_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as HEAD_LST_BRFR).HESAB_POSHTEF_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as HEAD_LST_BRFR).HESAB_POSHTEF_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as HEAD_LST_BRFR).HESAB_POSHTEF_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as HEAD_LST_BRFR).HESAB_POSHTEF_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;

                    case "WIN_MESSAGEPANEL":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!(Win_US as WIN_MESSAGEPANEL).COMP_COD_DATA.Any(item => item?.hes == thevalue))
                            {
                                (Win_US as WIN_MESSAGEPANEL).COMP_COD_DATA.Add(new CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as WIN_MESSAGEPANEL).COMP_COD.SelectedValue = thevalue; (Win_US as WIN_MESSAGEPANEL).COMP_COD.Items.Refresh();

                        }
                        break;

                    case "MESSAGEPANEL_CUSTOMER":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!(Win_US as MESSAGEPANEL_CUSTOMER).COMP_COD_DATA.Any(item => item?.hes == thevalue))
                            {
                                (Win_US as MESSAGEPANEL_CUSTOMER).COMP_COD_DATA.Add(new CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as MESSAGEPANEL_CUSTOMER).COMP_COD.SelectedValue = thevalue; (Win_US as MESSAGEPANEL_CUSTOMER).COMP_COD.Items.Refresh();

                        }
                        break;

                    case "WIN_F_MENU_KHFR":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HEAD_LST_KH_BACK_AZAD).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as WIN_F_MENU_KHFR).HMOIN.SelectedValue = null;
                            (Win_US as WIN_F_MENU_KHFR).HMOIN.SelectedValue = thevalue;
                            (Win_US as WIN_F_MENU_KHFR).HMOIN.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "HEAD_LST_HAV_OTHER_WIN":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as HEAD_LST_HAV_OTHER_WIN).HESAB_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as HEAD_LST_HAV_OTHER_WIN).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as HEAD_LST_HAV_OTHER_WIN).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as HEAD_LST_HAV_OTHER_WIN).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as HEAD_LST_HAV_OTHER_WIN).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;

                    case "F_USER_PERMITION_FORMS_BLOCKED_SUB":
                    case "F_USER_PERMITION_FORMS_UNBLOCKED_SUB":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as F_USER_PERMITION_FORMS).HESAB_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as F_USER_PERMITION_FORMS).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as F_USER_PERMITION_FORMS).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as F_USER_PERMITION_FORMS).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as F_USER_PERMITION_FORMS).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "VOSULDIALOG":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as VOSULDIALOG).HESAB_FROM_SEARCH.FULL_HES = hesval;
                            (Win_US as VOSULDIALOG).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as VOSULDIALOG).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as VOSULDIALOG).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();
                            (Win_US as VOSULDIALOG).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "VOSULDIALOG2":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as VOSULDIALOG2).HESAB_FROM_SEARCH.FULL_HES = hesval;
                            (Win_US as VOSULDIALOG2).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as VOSULDIALOG2).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as VOSULDIALOG2).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();
                            (Win_US as VOSULDIALOG2).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "BEHESABDIALOG":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as BEHESABDIALOG).HESAB_FROM_SEARCH.FULL_HES = hesval;
                            (Win_US as BEHESABDIALOG).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as BEHESABDIALOG).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as BEHESABDIALOG).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();
                            (Win_US as BEHESABDIALOG).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "ZASESABBEESAB":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as ZASESABBEESAB).HESAB_FROM_SEARCH.FULL_HES = hesval;
                            (Win_US as ZASESABBEESAB).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as ZASESABBEESAB).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as ZASESABBEESAB).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();
                            (Win_US as ZASESABBEESAB).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;

                        }
                        break;

                    case "HAVALAH_ENTER":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            if (!((List<Custom_CUST_HESAB>)(Win_US as HAVALAH_ENTER).CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                            {
                                ((List<Custom_CUST_HESAB>)(Win_US as HAVALAH_ENTER).CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME });
                            }

                            (Win_US as HAVALAH_ENTER).CUST_NO.SelectedValue = null;
                            (Win_US as HAVALAH_ENTER).CUST_NO.SelectedValue = thevalue;
                            (Win_US as HAVALAH_ENTER).CUST_NO.Items.Refresh(); //Just to select and display new selectedValue
                        }
                        break;
                    case "F_MENU_DATE_HES":
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string hesval = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                hesval = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            (Win_US as F_MENU_DATE_HES).HESAB_FROM_SEARCH.FULL_HES = hesval;

                            (Win_US as F_MENU_DATE_HES).HESAB_FROM_SEARCH.N_KOL = selectedVal.N_KOL.ToString();
                            (Win_US as F_MENU_DATE_HES).HESAB_FROM_SEARCH.N_MOIN = selectedVal.NUMBER.ToString();
                            (Win_US as F_MENU_DATE_HES).HESAB_FROM_SEARCH.N_TAF = selectedVal.TNUMBER.ToString();

                            (Win_US as F_MENU_DATE_HES).HESAB_FROM_SEARCH.NAME = selectedVal.NAME;
                        }
                        break;

                    default:
                        selectedVal = ResDGR1.SelectedItem as CUST_HESAB_DTL;
                        if (!(selectedVal is null))
                        {
                            IsSelectedItmFromResult = true;
                            e.Handled = true;

                            string thevalue = $"{selectedVal.N_KOL}-{selectedVal.NUMBER}-{selectedVal.TNUMBER}";
                            if (!string.IsNullOrEmpty(selectedVal?.tnumber2.ToStringNullSafe()) && selectedVal?.tnumber2.ToStringNullSafe() != "NULL")
                            {
                                thevalue = selectedVal?.tnumber2.ToString();
                                selectedVal.NAME = selectedVal.TNAME;
                            }

                            SELECTED_HESAB = new Custom_CUST_HESAB { hes = thevalue, NAME = selectedVal.NAME };
                        }
                        break;
                }
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //was : cust_hesab_dtl

            var QreResult = "SELECT  TNUMBER, NAME, NUMBER, N_KOL, NMOIN, NKOL, ADDRESS, tNUMBER2, TNAME, CODE_E FROM dbo.CUST_HESAB_DTL_EXTENDED ORDER BY NAME, TNAME";

            if (!string.IsNullOrEmpty(shart))
            {
                if (Baseknow.PERSON.ToString() == "2")
                {
                    if (shart.Length > 0)
                    {
                        QreResult = "SELECT TNUMBER, NAME, NUMBER, N_KOL, NMOIN, NKOL, ADDRESS, tNUMBER2, TNAME, CODE_E FROM CUST_HESAB_DTL_EXTENDED WHERE     (" + shart + ") and ((CODE_E = N'0') OR  (CODE_E IS NULL) )  ORDER BY NAME, TNAME ";
                    }
                }
                else if (shart.Length > 0)
                {
                    QreResult = "SELECT TNUMBER, NAME, NUMBER, N_KOL, NMOIN, NKOL, ADDRESS, tNUMBER2, TNAME, CODE_E FROM CUST_HESAB_DTL_EXTENDED WHERE     (" + shart + ")  ORDER BY NAME, TNAME ";
                }
            }
            ResDGR1.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB_DTL>(QreResult).ToList();
            if (ResDGR1.Items.Count > 0)
            {
                //ResDGR1.CurrentCell = new DataGridCellInfo(ResDGR1.Items[0], ResDGR1.Columns[1]);
                //ResDGR1.BeginEdit();


                var LAST_ROW = 0;
                var col_index = ResDGR1.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME").DisplayIndex;

                ResDGR1.ScrollIntoView(this.ResDGR1.Items[LAST_ROW]);
                ResDGR1.SelectedIndex = LAST_ROW;
                ResDGR1.Focus();
                ResDGR1.CurrentCell = new DataGridCellInfo(ResDGR1.Items[LAST_ROW], ResDGR1.Columns[col_index]);
                DataGridRow row = ResDGR1.ItemContainerGenerator.ContainerFromIndex(LAST_ROW) as DataGridRow;
                if (row is null)
                {
                    object item = ResDGR1.Items[LAST_ROW];
                    ResDGR1.ScrollIntoView(ResDGR1.Items[LAST_ROW]);
                    row = (DataGridRow)ResDGR1.ItemContainerGenerator.ContainerFromIndex(LAST_ROW);
                    ResDGR1.SelectedItem = item;
                    //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    col_index = ResDGR1.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME").DisplayIndex;
                    DataGridCell cell = CL_LMethods.GetCell(ResDGR1, row, Convert.ToInt32(col_index));
                    if (cell != null)
                        cell.Focus();
                }
                else
                {
                    object item = ResDGR1.Items[LAST_ROW];
                    ResDGR1.SelectedItem = item;
                    ResDGR1.ScrollIntoView(item);
                    //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    DataGridCell cell = CL_LMethods.GetCell(ResDGR1, row, Convert.ToInt32(col_index));
                    if (cell != null)
                        cell.Focus();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                Btn_Close_Click(null, null);
            }
        }
    }
}
