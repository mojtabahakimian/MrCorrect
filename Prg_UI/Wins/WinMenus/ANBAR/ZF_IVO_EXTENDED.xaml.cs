using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class ZF_IVO_EXTENDED : Window
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
        UniversControl universControl = new UniversControl();
        public int Id { get; set; }
        public Visual WIN_COME { get; set; }
        public ZF_IVO_EXTENDED(int _Id, Visual _YOUR_VL_WIN)
        {
            InitializeComponent();
            Id = _Id;
            WIN_COME = _YOUR_VL_WIN;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (Id > 0)
            {
                var query = dbms.DoGetDataSQL<IVO_EXTENDED_CSHARP>("SELECT * FROM IVO_EXTENDED WHERE id=" + Id).SingleOrDefault();
                FLD1.Text = query.FLD1.ToString();
                FLD2.Text = query.FLD2.ToString();
                FLD3.Text = query.FLD3.ToString();
                FLD4.Text = query.FLD4.ToString();
                FLD5.Text = query.FLD5.ToString();
                FLD6.Text = query.FLD6.ToString();
                FLD7.Text = query.FLD7.ToString();
                FLD8.Text = query.FLD8.ToString();
                FLD9.Text = query.FLD9.ToString();
                FLD10.Text = query.FLD10.ToString();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            dbms.DoExecuteSQL($@"UPDATE dbo.IVO_EXTENDED SET
                                               FLD1 = {(FLD1.Text != "" ? FLD1.Text : "0")},
                                               FLD2 = {(FLD2.Text != "" ? FLD2.Text : "0")},
                                               FLD3 = {(FLD3.Text != "" ? FLD3.Text : "0")},
                                               FLD4 = {(FLD4.Text != "" ? FLD4.Text : "0")},
                                               FLD5 = {(FLD5.Text != "" ? FLD5.Text : "0")},
                                               FLD6 = {(FLD6.Text != "" ? FLD6.Text : "0")},
                                               FLD7 = {(FLD7.Text != "" ? FLD7.Text : "0")},
                                               FLD8 = {(FLD8.Text != "" ? FLD8.Text : "0")},
                                               FLD9 = {(FLD9.Text != "" ? FLD9.Text : "0")},
                                               FLD10 = {(FLD10.Text != "" ? FLD10.Text : "0")}
                                               WHERE id =" + Id);
            universControl.PopNotifyShow(".مقادیر ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            try
            {
                // Concatenate the values from the FLD1 to FLD10 fields into a single string
                string paramsString = "چربي:" + FLD1.Text +
                                      "- ماده خشک:" + FLD2.Text +
                                      "- رطوبت:" + FLD3.Text +
                                      "- پي اچ:" + FLD4.Text +
                                      "- نمک:" + FLD5.Text +
                                      "- دانسيته:" + FLD6.Text +
                                      "- پروتئين:" + FLD7.Text +
                                      "- انجماد:" + FLD8.Text +
                                      "- اسيد:" + FLD9.Text +
                                      "- الکل:" + FLD10.Text;

                switch (WIN_COME)
                {
                    //  ,   
                    case HEAD_LST_KHAREED1:
                        (WIN_COME as HEAD_LST_KHAREED1).MOLAH.Text = paramsString;

                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET MOLAH = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_KHAREED1).NUMBER.Text} AND TAG IN (12) ");
                        break;

                    case HEAD_LST_RASID:
                        (WIN_COME as HEAD_LST_RASID).TAH.Text = paramsString; //فیلد دتحویل گیرنده

                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET TAH = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_RASID).NUMBER.Text} AND TAG IN (1) ");
                        break;

                    case HEAD_LST_KHADAMAT:
                        (WIN_COME as HEAD_LST_KHADAMAT).MOLAH.Text = paramsString;

                        dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET MOLAH = N'{paramsString}'
                                             WHERE NUMBER = {(WIN_COME as HEAD_LST_KHADAMAT).NUMBER.Text} AND TAG IN (14) ");
                        break;

                    default: break;
                }
                // Assuming MOLAH fields are TextBlocks or Labels in a form, set their text values
                //head_lst_khareed1.MOLAH.Text = paramsString;
                //head_lst_rasid.MOLAH.Text = paramsString;
            }
            catch (Exception ex)
            {
                // Handle the error if needed
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
            }

            Close();
        }
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!Save.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

    }
}
