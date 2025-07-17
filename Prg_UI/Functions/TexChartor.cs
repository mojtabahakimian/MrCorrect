using Prg_UI.HelperWins;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Prg_UI.Functions
{
    public static class TexChartor
    {
        //For ComboBox Editable lost Focus
        public static bool CanRunEvnt = true;

        public static bool isFinishSeprate = false;
        //Make Zero Default when is Empty TextBox
        public static void Zero_ifisEmpt(TextBox TXB2)
        {
            try
            {
                if (string.IsNullOrEmpty(TXB2.Text))
                {
                    TXB2.Text = "0";
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        //1000 => 1,000 Put , between thrid of numbers in TextBox
        public static void Make_TSeprated(TextBox TXTB1)
        {
            try
            {
                if (!string.IsNullOrEmpty(TXTB1.Text))
                {
                    if (TXTB1.Text.Contains(",") || TXTB1.Text.Contains("ریال") || TXTB1.Text.Contains("ریال"))
                    {
                        try
                        {
                            string str1 = TXTB1.Text;
                            TXTB1.Text = str1.Replace(",", "");
                            TXTB1.Text = str1.Replace("ریال", "");
                            TXTB1.Text = str1.Replace("ریال", "");
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                    int n = TXTB1.SelectionStart;
                    double dectext = Convert.ToDouble(TXTB1.Text.Trim());
                    TXTB1.Text = String.Format("{0:#,###0}", dectext);
                    TXTB1.SelectionStart = n + 1;
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        //1,000 => Remove  , between thrid of numbers in TextBox
        public static void Del_TSeprated(TextBox TXTB1)
        {
            try
            {
                if (!string.IsNullOrEmpty(TXTB1.Text))
                {
                    string str1 = TXTB1.Text;
                    TXTB1.Text = str1.Replace(",", "");
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        //Get All Character or anything exept Number 0^9
        public static string GetAllWords(string input)
        {
            return new string(input.Where(c => !char.IsDigit(c)).ToArray());
        }
        //Get All Number From Whole Text Output is Digit Only
        public static string GetAllNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
        //Stop Type IF its anything else of number just can type number
        public static void Prevent_AnyWords(TextBox TXB2)
        {
            try
            {
                if (!string.IsNullOrEmpty(TXB2.Text.Trim()))
                {
                    TXB2.Text = GetAllNumbers(TXB2.Text);
                    TXB2.SelectionStart = TXB2.Text.Length;
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        public static void Prevent_AnyWords_Sepy(TextBox TXB2)
        {
            try
            {
                if (!string.IsNullOrEmpty(TXB2.Text.Trim()))
                {
                    bool sep = false;
                    if (TXB2.Text.Contains(","))
                    {
                        sep = true;
                    }
                    //1000,000H
                    bool isDigitPresent = TXB2.Text.Any(c => !char.IsDigit(c));
                    if (isDigitPresent == true)
                    {
                        TXB2.Text = GetAllNumbers(TXB2.Text);
                        TXB2.SelectionStart = TXB2.Text.Length;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        /// <summary>
        /// جلوگیری از ورود غیر مجاز همراه با جداکننده سه رقم
        /// </summary>
        /// <param name="TheTextBox"></param>
        public static void Prevent_AnyWords_DigitGrp(TextBox TheTextBox)
        {
            if (!string.IsNullOrEmpty(TheTextBox.Text.Trim()))
            {
                TheTextBox.Text = GetAllNumbers(TheTextBox.Text);

                double range;
                if (!double.TryParse(TheTextBox.Text, out range))
                {
                    TheTextBox.Text = TheTextBox.Text.Replace(TheTextBox.Text.Substring(TheTextBox.Text.Length - 1, 1), "");
                }

                TheTextBox.Text = string.Format("{0:#,##0.#}", double.Parse(TheTextBox.Text.Trim()));

                TheTextBox.SelectionStart = TheTextBox.Text.Length;
            }
        }
        //Remove Zero at Beginning of Text like  0123 => 123
        public static void Del_ZeroFirst(TextBox TXB2)
        {
            try
            {
                if (!string.IsNullOrEmpty(TXB2.Text.Trim()) && TXB2.Text.Trim().Length > 1)
                {
                    TXB2.Text = TXB2.Text.TrimStart('0').Trim();
                }
            }
            catch (Exception)
            {
                return;
            }

        }
        //Remove Zero at Beginning of Text like  0123 => 123 For morethan one  or Multiple TextBoxes
        public static void Del_Multi_SepratTexs(params TextBox[] textboxes)
        {
            foreach (TextBox item in textboxes)
            {
                item.Text = item.Text.TrimStart('0').Trim();
            }
        }
        //بررسی خالی بودن تکست باکس
        public static bool TexBisEmpt(TextBox TXB)
        {
            bool st;
            if (!string.IsNullOrEmpty(TXB.Text) && TXB.Text != "")
            {
                st = false;
                //TextBox is Not Empty is OK
            }
            else
            {
                st = true;
                //TextBox Null Or Empty
            }
            return st;
        }
        //بررسی خالی بودن تکست باکس
        public static bool TexBisEmptWith_Trimit(TextBox TXB)
        {
            bool st = true;
            TXB.Text = TXB.Text.Trim();
            if (!string.IsNullOrEmpty(TXB.Text) && TXB.Text != "" && !string.IsNullOrWhiteSpace(TXB.Text))
            {
                st = false;
                //TextBox is Not Empty is OK
            }
            else
            {
                st = true;
                //TextBox Null Or Empty
            }
            return st;
        }
        //بررسی خالی بودن کامبوباکس اگر کمبوباکس خالی نیست و یا آیتمی انتخاب شده
        public static bool ComboisNotEmpt(ComboBox CMB)
        {
            bool st = false;
            if (CMB.SelectedIndex > -1 && CMB.SelectedItem != null)
            {
                st = true;
                //CanRunEvnt = false;
                //ComboBox is Not Empty is Correct Item Selected
            }
            else
            {
                st = false;
                // CanRunEvnt = true;
                //ComboBox Is Empty Or Not Correct Item Selected
            }
            return st;
        }
        //بررسی میکنم حتما آتمی که وارد شده داخل لیست موجودی باشد
        public static bool ComboisChoosedItem_Valid(ComboBox CMB, bool CanSearchPlus = false)
        {
            bool st = false;
            TextBox TexBo = (TextBox)CMB.Template.FindName("PART_EditableTextBox", CMB);

            if (CanSearchPlus) //Check With "+"
            {
                if (TexBo.Text != "+" && TexBo.Text != "++" && TexBo.Text != "+++")
                {
                    if (!string.IsNullOrEmpty(CMB.Text.Trim()) && CMB.SelectedIndex > -1 && CMB.SelectedItem != null)
                    {
                        st = true; //OK
                    }
                    else st = false;
                }
                else st = false;
            }
            else //Check Without "+"
            {
                //<WpfApp5.CUSTKIND>
                if (!string.IsNullOrEmpty(CMB.Text.Trim()) && CMB.SelectedIndex > -1 && CMB.SelectedItem != null)
                {
                    st = true; //OK
                }
                else st = false;
            }
            return st;
            //bool st = true;
            ////!string.IsNullOrEmpty(CMB.Text.Trim()) &&  --- Removed Because some name maybe empty but they have code
            //if (CMB.SelectedIndex > -1 && CMB.SelectedItem != null)
            //{
            //    if (CMB.Items.OfType<ComboBoxItem>().Any(cbi => cbi.Content.Equals(CMB.Text)))
            //    {
            //        //ComboBox is Not Empty is Correct Item Selected
            //        st = true; //OK
            //    }
            //}
            //else
            //{
            //    //ComboBox Is Empty Or Not Correct Item Selected
            //    //CMB.SelectedItem = null;
            //    //CMB.SelectedIndex = -1;
            //    st = false; //Wrong
            //}
            //return st;
        }
        //اگر در کلیپ بورد یا حافظه چیزی غیر عددیست آنرا خالی کن مثل Ctrl + C
        public static void Clear_Clip(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                int inumb;
                bool Theresult = int.TryParse(Clipboard.GetText(), out inumb);
                if (Theresult == false)
                {
                    e.Handled = true;
                    return;
                }
            }
            // نحوه استفاده از آن در کد
            //private void texb_codekala_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
            //{
            //    Clear_Clip(sender, e);
            //}

            //در زمل
            //CommandManager.PreviewExecuted = "texb_codekala_PreviewExecuted"
        }
        // برای مشکل فوکوس شدن کمبوباکس از این متد برای بررسی انکه آیا روی کمبوباکس قابل ادیت فوکوس شده یعنی روی تکست باکس کمبوباکس فوکوس شده که اگر شده خود اون رو برگردون
        public static TextBox IsComboBoxReallyFocuesd(ComboBox comboBox)
        {
            TextBox ComboTexBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (ComboTexBox != null && !ComboTexBox.IsKeyboardFocused) //!ComboTexBox.IsKeyboardFocused Wich Means Lost Focus the TextBox of ComboBox Happened
            {
                return ComboTexBox;
            }
            else
            {
                return null;
            }
        }
        //چک میکنه اگر کمبوباکس چیزی درست انتخاب نشده فوکوس رو برمیگردونه توی اون
        public static void IFComboBoxIsNotValidShowEr_For_Hesab(string msg, ComboBox combo, RoutedEventArgs e, bool CanSearchPlus = false, string FormNameForPlusSearch = "")
        {
            if (combo.IsEditable)
            {
                if (!(e.OriginalSource is TextBox)) return;
            }
            if (ComboisChoosedItem_Valid(combo, CanSearchPlus: true) == false && CanRunEvnt)
            {
                //TextBox TexBo = IsComboBoxReallyFocuesd(combo);
                TextBox TexBo = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);

                if (TexBo != null && combo.IsDropDownOpen != true)
                {
                    if (CanSearchPlus == true)
                    {
                        if (TexBo.Text == "+" || TexBo.Text == "++" || TexBo.Text == "+++")
                        {
                            ComboSearch CMBSearch = new ComboSearch(FormNameForPlusSearch);//Search Plusy Form Specialy for Customers
                            CMBSearch.ShowDialog();
                            return;
                            //if (combo.SelectedItem != null && combo.SelectedValue != null)
                            //{
                            //    combo.SelectedItem = null; // For Prevent Stay + in Text Of ComboBox event Item Selected
                            //    //Continue;
                            //}
                            //else
                            //{
                            //    ComboSearch CMBSearch = new ComboSearch(FormNameForPlusSearch);//Search Plusy Form Specialy for Customers
                            //    CMBSearch.ShowDialog();
                            //    return;
                            //    //For + Enter To Open Search
                            //}
                        }
                    }
                    if (!string.IsNullOrEmpty(TexBo.Text))
                    {
                        try
                        {
                            combo.SelectedValue = Convert.ToInt32(TexBo.Text);
                            if (!ReferenceEquals(combo.SelectedValue, null))
                            {
                                return;
                            }
                        }
                        catch
                        {
                            //NO-OP
                        }
                    }
                    Msgwin msgwin = new Msgwin(false, msg, "", true); msgwin.ShowDialog();
                    //MessageBox.Show(msg, "Error Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    combo.Text = ""; combo.SelectedItem = null;
                    combo.IsDropDownOpen = true;
                    CanRunEvnt = false;
                    combo.Focus();
                    CanRunEvnt = true;
                    e.Handled = true;
                    return;
                    //var restoreFocus = (System.Threading.ThreadStart)delegate
                    //    {
                    //        MessageBox.Show(msg, "Error Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //        TexBo.Focus(); TexBo.SelectAll();
                    //        combo.IsDropDownOpen = true;
                    //    };
                    //Application.Current.Dispatcher.BeginInvoke(restoreFocus);
                    //System.Windows.Threading.Dispatcher.BeginInvoke(restoreFocus);
                }
            }
        }
        public static void IFComboBoxIsNotValidShowEr_For_Kala(string msg, ComboBox combo, RoutedEventArgs e, bool CanSearchPlus = false, object FormNameForPlusSearch = null, string AnbarForSearch = "")
        {
            if (combo.IsEditable)
            {
                if (!(e.OriginalSource is TextBox)) return;
            }

            if (ComboisChoosedItem_Valid(combo, CanSearchPlus: true) == false && CanRunEvnt)
            {
                //TextBox TexBo = IsComboBoxReallyFocuesd(combo);
                TextBox TexBo = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);

                if (TexBo != null && combo.IsDropDownOpen != true)
                {
                    if (CanSearchPlus == true)
                    {
                        if (TexBo.Text == "+" || TexBo.Text == "++" || TexBo.Text == "+++")
                        {
                            //if (combo.SelectedItem != null && combo.SelectedValue != null)
                            //{
                            //    combo.SelectedItem = null; // For Prevent Stay + in Text Of ComboBox event Item Selected
                            //    //Continue;
                            //}
                            //else
                            //{

                            //    //For + Enter To Open Search
                            //}
                            combo.SelectedItem = null; // برای جلو گیری از گیر افتدان در این رویداد به خاطر کد های داخلش که وضعیت رو تغییر میدهند
                            SERCHK sERCHK = new SERCHK((System.Windows.Media.Visual)FormNameForPlusSearch, AnbarForSearch);//Search Plusy Form Specialy for Kala
                            sERCHK.ShowDialog();
                            return;
                        }
                    }
                    if (!string.IsNullOrEmpty(TexBo.Text))
                    {
                        try
                        {
                            combo.SelectedValue = Convert.ToInt32(TexBo.Text);
                            if (!ReferenceEquals(combo.SelectedValue, null))
                            {
                                return;
                            }
                        }
                        catch
                        {
                            //NO-OP
                        }
                    }
                    MessageBox.Show(msg, "Error Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    combo.Text = ""; combo.SelectedItem = null;
                    combo.IsDropDownOpen = true;
                    CanRunEvnt = false;
                    combo.Focus();
                    CanRunEvnt = true;
                    e.Handled = true;
                    return;
                    //var restoreFocus = (System.Threading.ThreadStart)delegate
                    //    {
                    //        MessageBox.Show(msg, "Error Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //        TexBo.Focus(); TexBo.SelectAll();
                    //        combo.IsDropDownOpen = true;
                    //    };
                    //Application.Current.Dispatcher.BeginInvoke(restoreFocus);
                    //System.Windows.Threading.Dispatcher.BeginInvoke(restoreFocus);
                }
            }
        }
        public static void IFComboBoxIsNotValidShowEr(string msg, ComboBox combo, RoutedEventArgs e)
        {
            ///UniversControl universControl = new UniversControl();

            if (combo.IsEditable)
            {
                if (!(e.OriginalSource is TextBox)) return;
            }

            if (ComboisNotEmpt(combo) == false && CanRunEvnt)
            {
                TextBox TexBo = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);

                if (TexBo != null && combo.IsDropDownOpen != true)
                {
                    if (!string.IsNullOrEmpty(TexBo.Text))
                    {
                        try
                        {
                            combo.SelectedValue = Convert.ToInt32(TexBo.Text);
                            if (!ReferenceEquals(combo.SelectedValue, null))
                            {
                                return;
                            }
                        }
                        catch { }
                    }
                    //MessageBox.Show(msg, "Error Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ///universControl.PopNotifyShow(" واحد نميتواند خالي باشد ", Pop1, Pop1Text1, Pop_Border1);
                    //combo.Focus(); TexBo.SelectAll();
                    //combo.IsDropDownOpen = true;
                    //e.Handled = true;

                    combo.Text = ""; combo.SelectedItem = null;
                    //Thread For Prenevt Twice Focus at same time
                    //var restoreFocus = (System.Threading.ThreadStart)delegate { Keyboard.Focus(combo); }; Application.Current.Dispatcher.BeginInvoke(restoreFocus);
                    combo.IsDropDownOpen = true;

                    CanRunEvnt = false;
                    combo.Focus();
                    CanRunEvnt = true;
                    //TexBo.Focus();//
                    e.Handled = true;
                    return;
                }
            }
        }
        //همه تکست باکس های این گرید رو از حالت 3 رقمی خارج میکنه
        public static void RemovalQotTex(Grid mygrid)
        {
            PublicVRB.DoingDeSeprate = true;
            foreach (UIElement control in mygrid.Children)
            {
                if (control is TextBox)
                {
                    if (!string.IsNullOrEmpty(((TextBox)control).Text))
                    {
                        ((TextBox)control).Text = ((TextBox)control).Text.Replace(",", "");
                    }
                }
            }
            PublicVRB.DoingDeSeprate = false;
        }
        /// <summary>
        /// برای زمانی هست که کوئری هایی که با شارپ و اتساین اولش هست که ترکیب متن و کد سی شارپ برای اس کیو ال هست رو بشه به صورت کوئری قابل مشاهده بررسی کرد یعنی میاد تنظیمات مربوط به فاصله ای که سی شارپ انداخته رو برمیداره
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string GetNormalyTexQre(this string txt)
        {
            return txt.Replace("\r\n", " ").Replace("                        ", " ").Replace("\r\n                        ", " ").Replace("              ", "").Replace("       ", " ").Replace("       ", " ").Replace(@"\t", " ");
        }
        /// <summary>
        /// گرفتن یک متن بین دو کلمه
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string GetStrBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            return "";
        }
    }
}
