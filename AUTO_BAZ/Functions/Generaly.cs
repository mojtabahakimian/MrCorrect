using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace AUTO_BAZ.Functions
{
    public static class Generaly
    {
        public static SolidColorBrush PutThisColor(string _YUOR_COLOR_ = "#FF2ECC71")
        {
            SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_YUOR_COLOR_));

            return brush;
        }
        public static void DoResetCountersDisplay()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                //CHK.Foreground = PutThisColor("#FF000000");
                //winy.MyProgressBar1.Maximum = 100;
                //winy.MyProgressBar1.Value = 0;
                //winy.LblCountery.Text = "0";
                winy.Text23.Text = "0";
                winy.co.Text = "0";
                winy.Text19.Text = "0";

                winy.PRGR_C00.Value = 0;
                winy.PRGR_C0.Value = 0;
                winy.PRGR_C1.Value = 0;
                winy.PRGR_C2.Value = 0;
                winy.PRGR_C3.Value = 0;
                winy.PRGR_C4.Value = 0;
                winy.PRGR_C5.Value = 0;
                winy.PRGR_C6.Value = 0;
                winy.PRGR_C7.Value = 0;
                winy.PRGR_C8.Value = 0;
                winy.PRGR_C9.Value = 0;
                winy.PRGR_C10.Value = 0;
                winy.PRGR_C11.Value = 0;
                winy.TOGHER_PROGRESS.Value = 0;
                winy.COUNTER_TXBL.Content = $"0%";
            });
        }

        private static bool _defac = true;
        /// <summary>
        /// تیک حساب هایی که نیست بساز
        /// </summary>
        public static bool defacc
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    if (winy != null)
                    {
                        _defac = Convert.ToBoolean(winy?.defacc?.IsChecked);
                    }
                }));
                return _defac;
            }
        }

        private static bool _c0;
        private static bool _c00;
        private static bool _c1;
        private static bool _c2;
        private static bool _c3;
        private static bool _c4;
        private static bool _c5;
        private static bool _c6;
        private static bool _c7;
        private static bool _c8;
        private static bool _c9;
        private static bool _c10;
        private static bool _c11;

        public static bool C00
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c00 = Convert.ToBoolean(winy.C00.IsChecked);
                }));
                return _c00;
            }
        }
        public static bool C0
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c0 = Convert.ToBoolean(winy.C0.IsChecked);
                }));
                return _c0;
            }
        }
        public static bool C1
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c1 = Convert.ToBoolean(winy.c1.IsChecked);
                }));
                return _c1;
            }
        }
        public static bool C2
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c2 = Convert.ToBoolean(winy.c2.IsChecked);
                }));
                return _c2;
            }
        }
        public static bool C3
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c3 = Convert.ToBoolean(winy.c3.IsChecked);
                }));
                return _c3;
            }
        }
        public static bool C4
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c4 = Convert.ToBoolean(winy.c4.IsChecked);
                }));
                return _c4;
            }
        }
        public static bool C5
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c5 = Convert.ToBoolean(winy.c5.IsChecked);
                }));
                return _c5;
            }
        }
        public static bool C6
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c6 = Convert.ToBoolean(winy.c6.IsChecked);
                }));
                return _c6;
            }
        }
        public static bool C7
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c7 = Convert.ToBoolean(winy.c7.IsChecked);
                }));
                return _c7;
            }
        }
        public static bool C8
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c8 = Convert.ToBoolean(winy.c8.IsChecked);
                }));
                return _c8;
            }
        }
        public static bool C9
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c9 = Convert.ToBoolean(winy.c9.IsChecked);
                }));
                return _c9;
            }
        }
        public static bool C10
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c10 = Convert.ToBoolean(winy.c10.IsChecked);
                }));
                return _c10;
            }
        }
        public static bool C11
        {
            get
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                    _c11 = Convert.ToBoolean(winy.c11.IsChecked);
                }));
                return _c11;
            }
        }


        //private static MainWindow mainWindow;
        //public static MainWindow Auto_Baz_Win
        //{
        //    get
        //    {
        //        Application.Current.Dispatcher.Invoke(new Action(() =>
        //        {
        //            var winy = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
        //            defacc = Convert.ToBoolean(winy.defacc.IsChecked);
        //        }));
        //        return mainWindow;
        //    }
        //    set { mainWindow = value; }
        //}
    }
}
