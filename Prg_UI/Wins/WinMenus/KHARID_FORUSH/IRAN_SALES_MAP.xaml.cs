using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class IRAN_SALES_MAP : Window, INotifyPropertyChanged
    {
        public IRAN_SALES_MAP()
        {
            InitializeComponent();
            DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        private bool NowIsReady { get; set; } = false;

        #region LocalModels
        public class OSTAN_RPT
        {
            public int ID { get; set; }
            public double WeightInKilograms { get; set; }
            public double TotalAmount { get; set; }
            public string Province { get; set; }
            public int ProvinceCode { get; set; }
        }

        public class CityInfo : INotifyPropertyChanged
        {
            private int id;
            public int ID
            {
                get { return id; }
                set
                {
                    if (id != value)
                    {
                        id = value;
                        OnPropertyChanged(nameof(ID));
                    }
                }
            }
            private string cityName;
            public string CityName
            {
                get { return cityName; }
                set
                {
                    if (cityName != value)
                    {
                        cityName = value;
                        OnPropertyChanged(nameof(CityName));
                    }
                }
            }
            private string col1;
            public string? COL1
            {
                get { return col1; }
                set
                {
                    if (col1 != value)
                    {
                        col1 = value;
                        OnPropertyChanged(nameof(COL1));
                    }
                }
            }

            private double? col2;
            public double? COL2
            {
                get { return col2; }
                set
                {
                    if (col2 != value)
                    {
                        col2 = value;
                        OnPropertyChanged(nameof(COL2));
                    }
                }
            }
            private string col3;
            public string? COL3
            {
                get { return col3; }
                set
                {
                    if (col3 != value)
                    {
                        col3 = value;
                        OnPropertyChanged(nameof(COL3));
                    }
                }
            }
            public CityInfo(int id, string cityname, string? cOL1 = null, double? cOL2 = null, string? cOL3 = null)
            {
                ID = id;
                CityName = cityname;
                COL1 = cOL1;
                COL2 = cOL2;
                COL3 = cOL3;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        //public ObservableCollection<CityInfo> IranCityMapData { get; set; } = new ObservableCollection<CityInfo>();

        private CityInfo? _currentCityClicked;
        public CityInfo? CurrentCityClicked
        {
            get { return _currentCityClicked; }
            set
            {
                _currentCityClicked = value; OnPropertyChanged(nameof(CurrentCityClicked));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_PRC_LOADER.ShowPreloader();

            ReGetData();

            CL_PRC_LOADER.HidePreloader();
        }
        private void ReGetData()
        {
            var Checkeses = GetAllCheckBoxStates(Mahha_SPanel);

            dbms.DoExecuteSQL("DELETE FROM OSTAN_RPT");

            if (Checkeses.Length < 1)
            {
            }
            else
            {
                dbms.DoExecuteSQL(@$"INSERT INTO dbo.OSTAN_RPT(WeightInKilograms, TotalAmount, Province, ProvinceCode)
                                                            SELECT SUM(CAST(coln6 AS FLOAT)* MEGHk) AS VAZN, SUM(MABL_K) AS MABL_K, OSNAME, OSTANID
                                                            FROM KALAS
                                                            WHERE((TAGCODE=2)AND col1=01 AND (MM IN ({Checkeses})))
                                                            GROUP BY OSTANID, OSNAME
                                                            OPTION(FORCE ORDER, LOOP JOIN, HASH JOIN, ORDER GROUP);");
            }


            var QreData = dbms.DoGetDataSQL<OSTAN_RPT>(@"SELECT * FROM OSTAN_RPT").ToList();

            //IF DATA IS NULL:
            //Reset Data (COL1,COL2,COL3)
            Parallel.ForEach(CityData.Values, item =>
            {
                item.COL1 = null;
                item.COL2 = null;
                item.COL3 = null;
            });
            foreach (var path in IRAN_MAP_GRID.Children.OfType<Path>())
            {
                if (path.Name is not "PersianGulf" && path.Name is not "SeaKhazar")
                {
                    path.Tag = 0;
                }
            }



            foreach (var row in QreData)
            {
                var cityInfoToUpdate = CityData.Values.FirstOrDefault(x => x.ID == row.ProvinceCode);
                if (cityInfoToUpdate != null)
                {
                    //cityInfoToUpdate.CityName = row.Province; // Update CityName with the Province value
                    cityInfoToUpdate.COL1 = Convert.ToString(row.WeightInKilograms);
                    cityInfoToUpdate.COL2 = row.TotalAmount;
                    cityInfoToUpdate.COL3 = Convert.ToString(row.ProvinceCode);
                }
            }
            foreach (var path in IRAN_MAP_GRID.Children.OfType<Path>())
            {
                if (CityData.TryGetValue(path.Name, out CityInfo cityInfo))
                {
                    double weight = 0;
                    if (double.TryParse(cityInfo.COL1, out _))
                    {
                        path.Tag = string.Format("{0:#,##0}", double.Parse(cityInfo.COL1));
                    }
                }
            }
        }

        private Path highlightedCity; // Track the currently highlighted city
        private void OnMapClick(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(IRAN_MAP_GRID);
            Path clickedCity = FindCityUnderMouse(mousePosition);

            if (clickedCity != null)
            {
                //ChangeCityColor(clickedCity, CityColorBrush("#FF0389C7").Color);
                string cityName = GetPersianCityName(clickedCity.Name);

                if (!string.IsNullOrEmpty(cityName))
                {
                    var Loc = e.GetPosition(this);
                    cityMessage.Content = cityName;
                    //cityMessage.Margin = new Thickness(Loc.X, Loc.Y, 0, 0); //To Show On Mouse Location On Map
                    cityMessage.Visibility = Visibility.Visible;


                    if (CityData.ContainsKey(clickedCity.Name))
                    {
                        CurrentCityClicked = null;
                        var thecity = CityData[clickedCity.Name];
                        CurrentCityClicked = new CityInfo(thecity.ID, thecity.CityName, thecity.COL1, thecity.COL2, thecity.COL3);
                    }
                }
            }
        }
        private void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(IRAN_MAP_GRID);
            Path hoveredCity = FindCityUnderMouse(mousePosition);

            if (hoveredCity != highlightedCity)
            {
                if (highlightedCity != null)
                {
                    if (highlightedCity.Tag?.ToString() is "0")
                    {
                        ChangeCityColor(highlightedCity, CityColorBrush("#FFF47174").Color); //Set Red
                    }

                    //ChangeCityColor(highlightedCity, CityColorBrush("#FF03A9F4").Color); // Revert the previous city's color
                }

                if (hoveredCity != null)
                {
                    //ChangeCityColor(hoveredCity, CityColorBrush("#FF0075A9").Color); // Change the hovered city's color
                }

                highlightedCity = hoveredCity;
            }
        }
        private void OnMapLeave(object sender, MouseEventArgs e)
        {
            if (highlightedCity != null)
            {
                if (highlightedCity.Tag?.ToString() is "0")
                {
                    ChangeCityColor(highlightedCity, CityColorBrush("#FFF47174").Color); //Set Red
                }
                //SolidColorBrush brush = CityColorBrush("#FF03A9F4").Color;
                //ChangeCityColor(highlightedCity, CityColorBrush("#FF03A9F4").Color); // Revert the highlighted city's color
                highlightedCity = null;
            }
        }
        private void OnMapMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(IRAN_MAP_GRID);
            Path clickedCity = FindCityUnderMouse(mousePosition);

            if (clickedCity != null)
            {
                //ChangeCityColor(clickedCity, CityColorBrush("#FF0075A9").Color);
                string cityName = GetPersianCityName(clickedCity.Name);

                if (!string.IsNullOrEmpty(cityName))
                {
                    var Loc = e.GetPosition(this);
                    cityMessage.Content = cityName;
                    //cityMessage.Margin = new Thickness(Loc.X, Loc.Y, 0, 0);
                    cityMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private SolidColorBrush CityColorBrush(string Rang)
        {
            //#FF03A9F4
            return (SolidColorBrush)new BrushConverter().ConvertFrom(Rang);
        }
        private Path FindCityUnderMouse(Point point)
        {
            foreach (UIElement element in IRAN_MAP_GRID.Children)
            {
                if (element is Path path)
                {
                    if (IsPointInPath(point, path))
                    {
                        return path;
                    }
                }
            }
            return null;
        }
        private void ChangeCityColor(Path cityPath, System.Windows.Media.Color color)
        {
            cityPath.Fill = new SolidColorBrush(color);
        }
        private bool IsPointInPath(Point point, Path path)
        {
            Geometry geometry = path.Data;
            if (geometry != null)
            {
                return geometry.FillContains(point);
            }
            return false;
        }

        private Dictionary<string, CityInfo> CityData = new Dictionary<string, CityInfo>
        {
             { "AzarbayejaneSharghi"               , new CityInfo(1 , "آذربایجان شرقی"         , null) } ,
            { "AzarbayejaneGharbi"                , new CityInfo(2 , "آذربایجان غربی"         , null) } ,
            { "Ardabil"                           , new CityInfo(3 , "اردبیل"                 , null) } ,
            { "Esfahan"                           , new CityInfo(4 , "اصفهان"                 , null) } ,
            { "Ilam"                              , new CityInfo(5 , "ایلام"                   , null) } ,
            { "Booshehr"                          , new CityInfo(6 , "بوشهر"                  , null) } ,
            { "Tehran"                            , new CityInfo(7 , "تهران"                  , null) } ,
            { "ChaharmahaleVaBakhtiari"           , new CityInfo(8 , "چهارمحال و بختیاری"     , null) } ,
            { "KhorasanJonubi"                    , new CityInfo(9 , "خراسان جنوبی"           , null) } ,
            { "KhorasanRazavi"                    , new CityInfo(10, "خراسان رضوی"            , null) } ,
            { "KhorasaneShomali"                  , new CityInfo(11, "خراسان شمالی"           , null) } ,
            { "Khuzestan"                         , new CityInfo(12, "خوزستان"                , null) } ,
            { "Zanjan"                            , new CityInfo(13, "زنجان"                  , null) } ,
            { "Semnan"                            , new CityInfo(14, "سمنان"                  , null) } ,
            { "SistanVaBaluchestan"               , new CityInfo(15, "سیستان و بلوچستان"      , null) } ,
            { "Fars"                              , new CityInfo(16, "فارس"                   , null) } ,
            { "Ghazvin"                           , new CityInfo(17, "قزوین"                  , null) } ,
            { "Ghom"                              , new CityInfo(18, "قم"                     , null) } ,
            { "Kordestan"                         , new CityInfo(19, "کردستان"                , null) } ,
            { "Kerman"                            , new CityInfo(20, "کرمان"                  , null) } ,
            { "Kermanshah"                        , new CityInfo(21, "کرمانشاه"               , null) } ,
            { "KohkiluyehVaBoyerahmad"            , new CityInfo(22, "کهگیلویه و بویر احمد"   , null) } ,
            { "discrepancies "                    , new CityInfo(23, "گلستان"                 , null) } ,
            { "Gilan"                             , new CityInfo(24, "گیلان"                   , null) } ,
            { "Lorestan"                          , new CityInfo(25, "لرستان"                 , null) } ,
            { "Mazandaran"                        , new CityInfo(26, "مازندران"               , null) } ,
            { "Arak"                              , new CityInfo(27, "مرکزی"                  , null) } , //اراک
            { "Hormozgan"                         , new CityInfo(28, "هرمزگان"                , null) } ,
            { "Hamedan"                           , new CityInfo(29, "همدان"                  , null) } ,
            { "Yazd"                              , new CityInfo(30, "یزد"                    , null) } ,
            { "Alborz"                            , new CityInfo(31, "البرز"                  , null) }
        };

        private string GetPersianCityName(string englishCityName)
        {
            if (CityData.ContainsKey(englishCityName))
            {
                //Key Name to find that
                return CityData[englishCityName].CityName;
            }
            return "نام نامعتبر";
        }


        // Define a dictionary to map Arabic letters to their Persian equivalents
        public static readonly Dictionary<char, char> arabicToPersian = new Dictionary<char, char>
        {
          //Input → Output
            { 'ي', 'ی' },
            { 'ة', 'ه' },
            { 'ك', 'ک' }
            // Add more mappings as needed
        };
        public string ReplaceArabicToPersian(string input)
        {
            // Convert the input string to a character array
            char[] characters = input.ToCharArray();
            // Iterate over each character in the array
            for (int i = 0; i < characters.Length; i++)
            {
                // Check if the character is in the dictionary
                if (arabicToPersian.ContainsKey(characters[i]))
                {
                    // Replace the Arabic letter with its Persian equivalent
                    characters[i] = arabicToPersian[characters[i]];
                }
            }
            // Convert the character array back to a string
            string result = new string(characters);
            return result;
        }

        private string GetAllCheckBoxStates(StackPanel panel)
        {
            string MHA = "";
            foreach (var child in panel.Children)
            {
                // Check if the child is a CheckBox
                if (child is CheckBox checkBox)
                {
                    if (checkBox.IsChecked == true)
                    {
                        if (MHA.Length <= 0)
                        {
                            MHA = checkBox.Tag.ToString();
                        }
                        else
                        {
                            MHA += "," + checkBox.Tag.ToString();
                        }
                    }
                }
            }
            return MHA;
        }
        private void ManageRadioButtons(object sender, RoutedEventArgs e)
        {
            var Checkeses = GetAllCheckBoxStates(Mahha_SPanel);
            if (Checkeses.Length < 26)
            {
                Some_RDB.IsChecked = true;
            }
            else
            {
                Hameh_RDB.IsChecked = true;
            }
            ReGetData();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Some_RDB_Click(object sender, RoutedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (Some_RDB.IsChecked is true)
            {
                var DOCHECK = false;
                Farvarding_CHB.IsChecked = DOCHECK;
                Ordibehesht_CHB.IsChecked = DOCHECK;
                Khordad_CHB.IsChecked = DOCHECK;
                Tir_CHB.IsChecked = DOCHECK;
                Mordad_CHB.IsChecked = DOCHECK;
                Shahrivar_CHB.IsChecked = DOCHECK;
                Mehr_CHB.IsChecked = DOCHECK;
                Aban_CHB.IsChecked = DOCHECK;
                Azar_CHB.IsChecked = DOCHECK;
                Dey_CHB.IsChecked = DOCHECK;
                Bahman_CHB.IsChecked = DOCHECK;
                Esfand_CHB.IsChecked = DOCHECK;

                var Emrooz = Tarikh.ToPersianDateTime(DateTime.Now);
                foreach (var child in Mahha_SPanel.Children)
                {
                    // Check if the child is a CheckBox
                    if (child is CheckBox checkBox)
                    {
                        if (Convert.ToInt32(checkBox.Tag) == Emrooz.Month)
                        {
                            checkBox.IsChecked = true;
                        }
                    }
                }
                ReGetData();
            }
        }

        private void Hameh_RDB_Click(object sender, RoutedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (Hameh_RDB.IsChecked is true)
            {
                var DOCHECK = (bool)Hameh_RDB.IsChecked;
                Farvarding_CHB.IsChecked = DOCHECK;
                Ordibehesht_CHB.IsChecked = DOCHECK;
                Khordad_CHB.IsChecked = DOCHECK;
                Tir_CHB.IsChecked = DOCHECK;
                Mordad_CHB.IsChecked = DOCHECK;
                Shahrivar_CHB.IsChecked = DOCHECK;
                Mehr_CHB.IsChecked = DOCHECK;
                Aban_CHB.IsChecked = DOCHECK;
                Azar_CHB.IsChecked = DOCHECK;
                Dey_CHB.IsChecked = DOCHECK;
                Bahman_CHB.IsChecked = DOCHECK;
                Esfand_CHB.IsChecked = DOCHECK;
                ReGetData();
            }
        }

    }
}
