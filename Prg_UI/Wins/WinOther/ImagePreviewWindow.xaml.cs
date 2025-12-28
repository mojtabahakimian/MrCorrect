using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Prg_UI.Wins.WinOther
{
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow(byte[] imageData)
        {
            InitializeComponent();
            LoadImage(imageData);
        }

        private void LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return;

            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                imgPreview.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در نمایش تصویر: " + ex.Message);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
