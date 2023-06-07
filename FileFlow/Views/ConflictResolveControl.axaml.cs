using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.ViewModels;
using Ninject;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileFlow.Views
{
    public partial class ConflictResolveControl : UserControl, INotifyPropertyChanged
    {
        public bool IsShowed { get; set; }
        public ObservableCollection<Record> Files { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private List<string> newNames = new();
        private FileConflict conflict;

        private IIconExtractorService iconExtractor;
        private IFileSystemService fileSystem;

        public class Record : INotifyPropertyChanged
        {
            public string OldName { get; set; }
            public string NewName { get; set; }
            public Bitmap FileIcon { get; set; }
            public bool IsInvalid { get; set; }
            public string SourcePath { get; set; }
            public string TargetFolder { get; set; }

            public Record(string sourcePath, string targetFolder, IIconExtractorService iconExtractor)
            {
                SourcePath = sourcePath;
                TargetFolder = targetFolder;

                OldName = Path.GetFileName(SourcePath);
                NewName = OldName;

                FileIcon = iconExtractor.GetFileIcon(sourcePath);

                UpdateValidity();
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void ChangeName(List<string> newNames)
            {
                string name = Path.GetFileNameWithoutExtension(OldName);
                string postFix;
                string ext = Path.GetExtension(OldName);
                int index = 0;

                while (true)
                {
                    index++;
                    postFix = " (" + index + ")";

                    string newName = name + postFix + ext;
                    if (File.Exists(TargetFolder + "/" + newName) == false)
                    {
                        if (newNames.Contains(newName) == false)
                        {
                            NewName = newName;
                            newNames.Add(newName);
                            this.RaisePropertyChanged(nameof(NewName));
                            break;
                        }
                    }
                }
            }
            public void ResetName()
            {
                NewName = OldName;
                this.RaisePropertyChanged(nameof(NewName));
            }
            public void UpdateValidity()
            {
                IsInvalid = File.Exists(TargetFolder + "/" + NewName);

                this.RaisePropertyChanged(nameof(IsInvalid));
            }
        }

        public ConflictResolveControl()
        {
            InitializeComponent();
        }
        [Inject]
        public ConflictResolveControl(IIconExtractorService iconExtractor, IFileSystemService fileSystem)
        {
            this.iconExtractor = iconExtractor;
            this.fileSystem = fileSystem;
            DataContext = this;
            InitializeComponent();

            rewriteButton.Click += RewriteButton_Click;
            renameButton.Click += RenameButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        public void Show(FileConflict conflict)
        {
            this.conflict = conflict;

            IsHitTestVisible = true;
            IsShowed = true;

            Files.Clear();
            foreach (string path in conflict.conflictedPathes)
            {
                Files.Add(new Record(path, conflict.targetFolder, iconExtractor));
            }

            this.RaisePropertyChanged(nameof(IsShowed));
            this.RaisePropertyChanged(nameof(Files));
        }
        public void Hide()
        {
            IsHitTestVisible = false;
            IsShowed = false;

            this.RaisePropertyChanged(nameof(IsShowed));
        }
        private void OnRenamePointerEnter(object sender, PointerEventArgs e)
        {
            newNames.Clear();
            foreach (Record file in Files)
            {
                file.ChangeName(newNames);
                file.UpdateValidity();
            }
        }
        private void OnRenamePointerLeave(object sender, PointerEventArgs e)
        {
            foreach (Record file in Files)
            {
                file.ResetName();
                file.UpdateValidity();
            }
        }

        private void RenameButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (Record record in Files)
            {
                conflict.Resolve(record.SourcePath, record.NewName);
            }

            if (fileSystem.Move(conflict, out var newConflict) == false)
            {
                Show(newConflict);
            }
            else
            {
                Hide();
            }
        }

        private void RewriteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            fileSystem.Move(conflict.sourcePathes, conflict.targetFolder, out _, overwrite: true);
            Hide();
        }
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
