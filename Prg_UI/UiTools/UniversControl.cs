using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Prg_UI.UiTools
{
    public class UniversControl
    {
        System.Windows.Threading.DispatcherTimer MyTimer = new System.Windows.Threading.DispatcherTimer();
        Popup My_Pouop = null;
        public enum RangPop
        {
            Red,
            Green,
            Blue,
            Yellow
        }

        /// <summary>
        /// Blue  : #FF007ACC
        /// 
        /// Red   : #E5EC2B2B
        /// 
        /// Green : #FF1AAA2C
        /// 
        /// Yellow: #FFDC9E18
        /// </summary>
        /// <param name="Msgtext"></param>
        /// <param name="Secound_Wait"></param>
        /// <param name="Rang_Back"></param>
        /// <param name="StayShowinger">با کلیک روی یک قسمتی پاپ آپ مخفی شود یا همینطور باقی بماند , بدون تایمر اگر تنظیم شده باشد</param>
        public void PopNotifyShow(string Msgtext, Popup _thePopup = null, TextBox textOfPop = null, Border bordOfPop = null, string Rang_Back = "#E5EC2B2B", int Secound_Wait = 2)
        {
            My_Pouop = _thePopup;
            if (My_Pouop.IsOpen == false)
            {
                if (!string.IsNullOrEmpty(Rang_Back))
                {
                    var bc = new BrushConverter();
                    bordOfPop.Background = (Brush)bc.ConvertFrom(Rang_Back);
                }
                else
                {
                    var bc = new BrushConverter();
                    bordOfPop.Background = (Brush)bc.ConvertFrom("#E5EC2B2B");
                }
                textOfPop.Text = Msgtext; My_Pouop.IsOpen = true;

                MyTimer.Tick += MyTimer_Tick;
                MyTimer.Interval = new TimeSpan(0, 0, 0, Secound_Wait, 0);
                MyTimer.Start();

            }
        }
        public void PopNotifyShowUp(string Msgtext, Popup _thePopup = null, TextBox textOfPop = null, Border bordOfPop = null, RangPop rangPop = RangPop.Red, int Secound_Wait = 2)
        {
            My_Pouop = _thePopup;
            if (My_Pouop.IsOpen == false)
            {
                string RangStr = "";

                switch (rangPop)
                {
                    case RangPop.Blue: RangStr = "#FF007ACC"; break;
                    case RangPop.Red: RangStr = "#E5EC2B2B"; break;
                    case RangPop.Green: RangStr = "#FF1AAA2C"; break;
                    case RangPop.Yellow: RangStr = "#FFDC9E18"; break;
                    default: break;
                }

                var bc = new BrushConverter();
                bordOfPop.Background = (Brush)bc.ConvertFrom(RangStr);

                textOfPop.Text = Msgtext; My_Pouop.IsOpen = true;

                MyTimer.Tick += MyTimer_Tick;
                MyTimer.Interval = new TimeSpan(0, 0, 0, Secound_Wait, 0);
                MyTimer.Start();

            }
        }
        public void MyTimer_Tick(object sender, EventArgs e)
        {
            MyTimer.Stop();
            MyTimer.IsEnabled = false;
            My_Pouop.IsOpen = false;
        }
    }
}
