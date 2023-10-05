using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FileFlow.ViewModels;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace FileFlow.Views.Popups
{
    public partial class CreateArchiveControl : Window, INotifyPropertyChanged
    {
        public bool IsValid { get; private set; }
        public bool IsInvalid => !IsValid;
        public float Progress { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private string archiveTargetFolder;
        private IEnumerable<StorageElement> elementsToPack;

        [Inject] private ExplorerViewModel explorer;

        public CreateArchiveControl()
        {
            InitializeComponent();
            DataContext = this;

            nameField.GetObservable(TextBox.TextProperty).Subscribe(OnTextChanged);
        }

        public void Show(string archiveTargetFolder, string archiveDefaultName, IEnumerable<StorageElement> elementsToPack)
        {
            this.archiveTargetFolder = archiveTargetFolder;
            this.elementsToPack = elementsToPack;

            nameField.Text = archiveDefaultName;
            nameField.Focus();

            infoText.Text = "Выбрано " + elementsToPack.Count() + " элементов";
            progressBar.Value = 0;

            Show();
        }

        private void OnTextChanged(string name)
        {
            if (name == null) return;

            IsValid = File.Exists(archiveTargetFolder + "/" + name + ".zip") == false;

            this.RaisePropertyChanged(nameof(IsValid));
            this.RaisePropertyChanged(nameof(IsInvalid));
        }
        private async void OnCreateClicked()
        {
            CanBeHidden = false;
            await Task.Run(BeginPacking);
            CanBeHidden = true;
            Hide();
        }
        private void BeginPacking()
        {
            string archivePath = archiveTargetFolder + "/" + nameField.Text + ".zip";

            using (var archive = ZipArchive.Create())
            {
                List<string> filesToPack = new List<string>();

                SetProgressText("Подсчет файлов");

                foreach (StorageElement element in elementsToPack)
                {
                    if (element.IsFolder)
                    {
                        foreach (string filepath in Directory.EnumerateFiles(element.Path, "*", SearchOption.AllDirectories))
                        {
                            string key = filepath.Replace(archiveTargetFolder + "/", "").CleanUp();
                            archive.AddEntry(key, new FileInfo(filepath.CleanUp()));
                        }
                    }
                    else
                    {
                        string key = element.Path.Replace(archiveTargetFolder + "/", "").CleanUp();
                        archive.AddEntry(key, new FileInfo(element.Path.CleanUp()));
                    }
                }

                SetProgressText("Упаковка");
                archive.onWriteProgress += SetProgress;
                archive.SaveTo(archivePath, CompressionType.Deflate);
            }

            Dispatcher.UIThread.Post(() =>
            {
                explorer.SelectElement(archivePath);
            });
        }
        private void SetProgressText(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                infoText.Text = message;
            });
        }
        private void SetProgress(int value, int max)
        {
            Dispatcher.UIThread.Post(() =>
            {
                float progress = value / (float)max * 100;
                infoText.Text = "Упаковка " + progress.ToString("0") + "% (" + value + "/" + max + ")";
                progressBar.Maximum = max;
                progressBar.Value = value;
            });
        }
    }
}
