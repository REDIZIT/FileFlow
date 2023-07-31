using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FileFlow.ViewModels;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Factories;
using SharpCompress.Writers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            infoText.Text = "������� " + elementsToPack.Count() + " ���������";

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
            await Task.Run(BeginPacking);
            Hide();
        }
        private void BeginPacking()
        {
            string archivePath = archiveTargetFolder + "/" + nameField.Text + ".zip";

            using (var archive = ZipArchive.Create())
            {
                List<string> filesToPack = new List<string>();

                SetProgressText("������� ������");

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

                SetProgressText("��������");
                archive.onWriteProgress += SetProgress;
                archive.SaveTo(archivePath, CompressionType.Deflate);
            }
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
                infoText.Text = "�������� " + progress.ToString("0") + "% (" + value + "/" + max + ")";
                progressBar.Maximum = max;
                progressBar.Value = value;
            });
        }
    }
}
