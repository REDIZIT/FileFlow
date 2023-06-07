using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using FileFlow.Extensions;
using FileFlow.Services;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class StorageElement : INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public string LastModifyTime { get; private set; }
        public string Size { get; private set; }
        public Bitmap Icon { get; private set; }
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

        public bool IsFolder { get; private set; }

        private IFileSystemService fileSystem;
        private IIconExtractorService iconExtractor;

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
            Icon = IsFolder ? iconExtractor.GetFolderIcon(Path) : iconExtractor.GetFileIcon(Path);
            this.RaisePropertyChanged(nameof(Icon));

            UpdateSize(fileSystem);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void UpdateSize(IFileSystemService fileSystem)
        {
            if (Directory.Exists(Path) == false && File.Exists(Path) == false) return;

            Size = await fileSystem.GetElementWeight(Path);
            LastModifyTime = await fileSystem.GetModifyTime(Path);

            this.RaisePropertyChanged(nameof(Size));
            this.RaisePropertyChanged(nameof(LastModifyTime));
        }
    }
}