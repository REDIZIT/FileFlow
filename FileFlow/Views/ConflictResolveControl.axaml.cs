using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using FileFlow.Extensions;
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
        private MoveAction conflict;

        private IIconExtractorService iconExtractor;
        private IFileSystemService fileSystem;

        public class Record : INotifyPropertyChanged
        {
            public string OldName { get; set; }
            public string NewLocalPath { get; set; }
            public Bitmap FileIcon { get; set; }
            public bool IsInvalid { get; set; }
            public string SourceFolder { get; set; }
            public string TargetFolder { get; set; }
            public string LocalPath { get; set; }

            public Record(IIconExtractorService iconExtractor, string sourceFolder, string targetFolder, string localPath)
            {
                SourceFolder = sourceFolder;
                TargetFolder = targetFolder;
                LocalPath = localPath;

                OldName = localPath;
                NewLocalPath = OldName;

                FileIcon = iconExtractor.GetFileIcon(sourceFolder + "/" + localPath);

                UpdateValidity();
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void ChangeName(List<string> resolvedLocalPathes)
            {
                string targetFilePath = TargetFolder + "/" + LocalPath;
                string resolvedFilePath = FileSystemExtensions.GetRenamedPath(targetFilePath, n => resolvedLocalPathes.Contains(n) == false);
                resolvedLocalPathes.Add(resolvedFilePath);
                NewLocalPath = resolvedFilePath.Replace(TargetFolder.CleanUp() + "/", "");
                this.RaisePropertyChanged(nameof(NewLocalPath));
            }
            public void ResetName()
            {
                NewLocalPath = OldName;
                this.RaisePropertyChanged(nameof(NewLocalPath));
            }
            public void UpdateValidity()
            {
                IsInvalid = File.Exists(TargetFolder + "/" + NewLocalPath);

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
            skipButton.Click += SkipButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        

        public void Show(MoveAction action)
        {
            this.conflict = action;

            IsHitTestVisible = true;
            IsShowed = true;

            Files.Clear();
            foreach (string localPath in conflict.conflictedLocalPathes)
            {
                Files.Add(new Record(iconExtractor, action.sourceFolder, action.targetFolder, localPath));
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
            conflict.Perform(ActionType.Rename);
            Hide();
        }
        private void RewriteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            conflict.Perform(ActionType.Overwrite);
            Hide();
        }
        private void SkipButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            conflict.Perform(ActionType.Skip);
            Hide();
        }
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
