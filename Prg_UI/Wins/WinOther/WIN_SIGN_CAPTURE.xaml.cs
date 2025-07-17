using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wins.WinOther
{
    public partial class WIN_SIGN_CAPTURE : Window
    {
        public bool NowIsReady { get; private set; }

        #region Header Window
        // Header window code kept the same
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
        #endregion

        public WIN_SIGN_CAPTURE(SALGROUP_MODEL sALGROUP_MODEL)
        {
            InitializeComponent();
            this.DataContext = this;
            CurrentRow = sALGROUP_MODEL;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public SALGROUP_MODEL CurrentRow { get; set; }

        // Track if image is being used versus drawing
        private bool _isImageUploaded = false;
        private byte[] _uploadedImageData = null;

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            // Set default editing mode for drawing
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;

            // Try to load existing signature
            if (CurrentRow?.EMZA != null && CurrentRow.EMZA.Length > 0)
            {
                try
                {
                    // First try to load the image directly
                    BitmapImage bitmapSource = CL_LMethods.ByteArrayToBitmapImage(CurrentRow.EMZA);

                    if (bitmapSource != null)
                    {
                        // We successfully loaded the image
                        ImageBrush brush = new ImageBrush(bitmapSource);
                        brush.Stretch = Stretch.Uniform;
                        inkCanvas.Background = brush;

                        // Disable drawing when showing an existing image
                        inkCanvas.EditingMode = InkCanvasEditingMode.None;
                        _isImageUploaded = true;
                        _uploadedImageData = CurrentRow.EMZA;
                    }
                    else
                    {
                        // If loading fails, try to load as InkCanvas strokes if applicable
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(CurrentRow.EMZA))
                            {
                                inkCanvas.Strokes.Clear();
                                inkCanvas.Strokes = new System.Windows.Ink.StrokeCollection(ms);
                                _isImageUploaded = false;
                            }
                        }
                        catch
                        {
                            // If all attempts fail, just show a white background
                            inkCanvas.Background = Brushes.White;
                            inkCanvas.Strokes.Clear();
                        }
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, "خطا در بارگذاری تصویر امضا از دیتابیس").ShowDialog();

                    // Reset to default state
                    inkCanvas.Background = Brushes.White;
                    inkCanvas.Strokes.Clear();
                }
            }
        }
        private void BTN_CLEAR_ALL_Click(object sender, RoutedEventArgs e)
        {
            // Reset to drawing mode
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;

            // Clear everything
            inkCanvas.Strokes.Clear();
            inkCanvas.Background = Brushes.White;
            _isImageUploaded = false;
            _uploadedImageData = null;
        }
        private void BTN_UPIMAGE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "انتخاب تصویر امضا",
                    Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                    FilterIndex = 1,
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {

                    // Validate image file size (maximum 10 MB)
                    if (!CL_LMethods.ValidateImageFile(openFileDialog.FileName, out string errorMessage, 3))
                    {
                        new Msgwin(false, errorMessage).ShowDialog();
                        return;
                    }


                    // Read the selected file
                    byte[] fileBytes = CL_LMethods.ConvertFileToByte(openFileDialog.FileName);

                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        new Msgwin(false, "خطا در خواندن فایل").ShowDialog();
                        return;
                    }



                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp")
                    {
                        try
                        {
                            // Store the uploaded image data
                            //_uploadedImageData = fileBytes;
                            _isImageUploaded = true;

                            // Display the uploaded image
                            BitmapImage bitmapSource = CL_LMethods.ByteArrayToBitmapImage(fileBytes);

                            // Optionally compress the image
                            byte[] compressedBytes = CL_LMethods.CompressImage(bitmapSource, 50);
                            _uploadedImageData = compressedBytes;

                            // Display the uploaded (or compressed) image
                            bitmapSource = CL_LMethods.ByteArrayToBitmapImage(compressedBytes);
                            inkCanvas.Strokes.Clear();
                            inkCanvas.Background = new ImageBrush(bitmapSource);

                            if (bitmapSource != null)
                            {
                                inkCanvas.Strokes.Clear();
                                ImageBrush brush = new ImageBrush(bitmapSource);
                                brush.Stretch = Stretch.Uniform;
                                inkCanvas.Background = brush;

                                // Disable drawing when showing an uploaded image
                                inkCanvas.EditingMode = InkCanvasEditingMode.None;

                                universControl.PopNotifyShowUp(
                                    "تصویر با موفقیت بارگذاری شد. برای ذخیره نهایی دکمه «ذخیره» را فشار دهید.",
                                    Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue);
                            }
                            else
                            {
                                new Msgwin(false, "فایل تصویر معتبر نیست یا قابل پردازش نمی‌باشد.").ShowDialog();
                            }
                        }
                        catch (Exception ex)
                        {
                            new Msgwin(false, $"خطا در پردازش تصویر: {ex.Message}").ShowDialog();
                        }
                    }
                    else
                    {
                        new Msgwin(false, "فایل انتخاب شده یک تصویر معتبر نیست. لطفاً یک تصویر با فرمت JPG، PNG یا BMP انتخاب کنید.").ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری تصویر: {ex.Message}").ShowDialog();
            }
        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            byte[] imageData = null;

            try
            {
                // If using an uploaded image
                if (_isImageUploaded && _uploadedImageData != null)
                {
                    imageData = _uploadedImageData;
                }
                // Otherwise capture what was drawn on the InkCanvas
                else if (inkCanvas.Strokes.Count > 0)
                {
                    // Convert the ink strokes to a PNG image
                    imageData = CL_LMethods.ConvertInkCanvasToPng(inkCanvas);

                    // If the conversion failed, try to save the strokes directly
                    if (imageData == null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            inkCanvas.Strokes.Save(ms);
                            imageData = ms.ToArray();
                        }
                    }
                }
                else
                {
                    new Msgwin(false, "لطفاً امضا را رسم کنید یا یک تصویر امضا را بارگذاری کنید.").ShowDialog();
                    return;
                }

                // Update our model
                CurrentRow.EMZA = imageData;

                // Update the database
                var parameters = new
                {
                    EMZA = imageData,
                    IDD = CurrentRow.IDD
                };

                string sql = @"
                        UPDATE [dbo].[SALA_DTL]
                        SET [EMZA] = @EMZA
                        WHERE [IDD] = @IDD";

                dbms.DoExecuteSQL(sql, parameters);

                universControl.PopNotifyShowUp("ذخیره با موفقیت انجام شد.",
                              Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا هنگام ذخیره‌سازی: {ex.Message}").ShowDialog();
            }
        }

    }
}