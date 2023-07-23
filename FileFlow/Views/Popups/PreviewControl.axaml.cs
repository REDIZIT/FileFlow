using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FileFlow.ViewModels;
using NAudio.Wave;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace FileFlow.Views.Popups
{
    public partial class PreviewControl : Window
    {
        public Bitmap FileImage { get; private set; }

        private AudioFileReader audioReader;
        private WaveOutEvent audioFile;
        private Timer audioTimer;

        private StorageElement element;

        private static HashSet<string> imageExtensions = new()
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".tif"
        };
        private static HashSet<string> audioExtensions = new()
        {
            ".mp3", ".wav"
        };

        public PreviewControl()
        {
            InitializeComponent();
            DataContext = this;

            audioTimer = new(TimeSpan.FromMilliseconds(20));
            audioTimer.Elapsed += OnAudioTimerElapsed;

            audioFile = new WaveOutEvent();

            audioFile.PlaybackStopped += (s, args) =>
            {
                audioTimer.Stop();
            };
        }

        public void Show(StorageElement element)
        {
            if (element.IsFolder) return;
            this.element = element;

            if (TryShowImage())
            {
                image.IsVisible = true;
                audioGroup.IsVisible = false;
                UpdateFileInfo();
            }
            else if (TryShowAudio())
            {
                image.IsVisible = false;
                audioGroup.IsVisible = true;
                UpdateFileInfo();
            }
            else
            {
                Hide();
            }
        }

        protected override void OnHidden()
        {
            base.OnHidden();

            audioTimer.Stop();
            audioFile?.Stop();
            audioFile?.Dispose();
            audioReader?.Close();
        }

        private bool TryShowImage()
        {
            string ext = Path.GetExtension(element.Path).ToLower();
            if (imageExtensions.Contains(ext) == false) return false;

            try
            {
                FileImage = new Bitmap(element.Path);

                double preferredWidth = FileImage.Size.Width * 4;
                double preferredHeight = FileImage.Size.Height * 4;

                image.MaxWidth = preferredWidth;
                image.MaxHeight = preferredHeight;

                resolutionText.Text = FileImage.Size.Width + "x" + FileImage.Size.Height;

                this.RaisePropertyChanged(nameof(FileImage));

                Show();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryShowAudio()
        {
            string ext = Path.GetExtension(element.Path).ToLower();
            if (audioExtensions.Contains(ext) == false) return false;

            audioReader = new AudioFileReader(element.Path)
            {
                Volume = 0.5f
            };
           
            audioFile.Init(audioReader);
            audioFile.Play();

            audioTimer.Start();

            audioProgressBar.Maximum = audioReader.TotalTime.TotalMilliseconds;
            UpdateAudioInfo();

            Show();
            return true;
        }

        private void OnAudioTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(UpdateAudioInfo);            
        }
        private void UpdateAudioInfo()
        {
            audioProgressBar.Value = audioReader.CurrentTime.TotalMilliseconds;
            resolutionText.Text = audioReader.CurrentTime.ToString(@"m\:ss") + " / " + audioReader.TotalTime.ToString(@"m\:ss");
        }


        private void UpdateFileInfo()
        {
            nameText.Text = element.Name;
            sizeText.Text = element.SizeString;
        }
    }
}
