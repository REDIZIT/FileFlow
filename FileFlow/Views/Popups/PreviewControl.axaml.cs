using Avalonia.Media.Imaging;
using FileFlow.ViewModels;
using System;
using System.IO;
using System.Linq;

namespace FileFlow.Views.Popups
{
    public partial class PreviewControl : Window
    {
        public Bitmap FileImage { get; private set; }

        private static string[] extensions = new string[]
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".tif"
        };

        public PreviewControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Show(StorageElement element)
        {
            if (TryShow(element) == false)
            {
                Hide();
            }
        }
        private bool TryShow(StorageElement element)
        {
            if (element.IsFolder) return false;

            string ext = Path.GetExtension(element.Path).ToLower();
            if (extensions.Contains(ext) == false) return false;

            try
            {
                FileImage = new Bitmap(element.Path);

                double preferredWidth = FileImage.Size.Width * 4;
                double preferredHeight = FileImage.Size.Height * 4;

                image.MaxWidth = preferredWidth;
                image.MaxHeight = preferredHeight;

                resolutionText.Text = FileImage.Size.Width + "x" + FileImage.Size.Height;
                nameText.Text = element.Name;
                sizeText.Text = element.SizeString;

                this.RaisePropertyChanged(nameof(FileImage));

                Show();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
