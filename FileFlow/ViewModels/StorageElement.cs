using Avalonia.Media.Imaging;
using FileFlow.Services;
using System;
using System.ComponentModel;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class StorageElement : INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public DateTime LastModifyTime { get; private set; }
        public string LastModifyTimeString { get; private set; }
        public long Size { get; private set; }
        public string SizeString { get; private set; }
        public Bitmap Icon { get; private set; }
        public bool IsUnderCopyAction { get; private set; }
        public bool IsUnderCutAction { get; private set; }

        public bool IsAdded
        {
            get { return _isAdded; }
            set
            {
                _isAdded = value;
                this.RaisePropertyChanged(nameof(IsAdded));
            }
        }
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                this.RaisePropertyChanged(nameof(IsModified));
            }
        }

        private bool _isAdded, _isModified;

        public bool IsFolder { get; set; }

        private IFileSystemService fileSystem;
        private IIconExtractorService iconExtractor;

        public StorageElement(string path, DiContainer container)
        {
            fileSystem = container.Resolve<IFileSystemService>();
            iconExtractor = container.Resolve<IIconExtractorService>();

            SetPath(path);
        }
        public StorageElement(string path, IFileSystemService fileSystem, IIconExtractorService iconExtractor)
        {
            this.fileSystem = fileSystem;
            this.iconExtractor = iconExtractor;

            SetPath(path);
        }
        public void SetPath(string path)
        {
            Path = path.CleanUp();
            Name = System.IO.Path.GetFileName(path);
            IsFolder = File.Exists(Path) == false;

            this.RaisePropertyChanged(nameof(Path));
            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(IsFolder));

            Refresh();
        }
        public void Refresh()
        {
            RefreshIcon();
            UpdateSize(fileSystem);
        }
        public void RefreshIcon()
        {
            Icon = IsFolder ? iconExtractor.GetFolderIcon(Path) : iconExtractor.GetFileIcon(Path);
            this.RaisePropertyChanged(nameof(Icon));
        }
        public void SetUnderAction(bool underCopy, bool underCut)
        {
            IsUnderCopyAction = underCopy;
            IsUnderCutAction = underCut;

            this.RaisePropertyChanged(nameof(IsUnderCopyAction));
            this.RaisePropertyChanged(nameof(IsUnderCutAction));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void UpdateSize(IFileSystemService fileSystem)
        {
            if (Directory.Exists(Path) == false && File.Exists(Path) == false) return;

            Size = fileSystem.GetElementWeight(Path);

            if (Size == -1)
            {
                SizeString = "Нет доступа";
            }
            else
            {
                if (IsFolder)
                {
                    SizeString = Size == 0 ? "Нет элементов" : Size + " элементов";
                }
                else
                {
                    SizeString = FileSizeUtil.BytesToString(Size);
                }
            }
            
            

            LastModifyTime = fileSystem.GetModifyTime(Path);
            LastModifyTimeString = FileSizeUtil.PrettyModifyDate(LastModifyTime);


            this.RaisePropertyChanged(nameof(SizeString));
            this.RaisePropertyChanged(nameof(LastModifyTimeString));
        }
    }
}