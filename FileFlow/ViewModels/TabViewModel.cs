using Avalonia.Threading;
using FileFlow.Services;
using FileFlow.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        public string FolderPath
        {
            get { return folderPath; }
            set
            {
                folderPath = value;
                Title = new DirectoryInfo(folderPath).Name;
                this.RaisePropertyChanged(nameof(Title));
            }
        }
        public string Title { get; private set; }
        public bool IsActiveTab { get; private set; }

        public ObservableCollection<StorageElement> StorageElements { get; set; }

        private string folderPath;
        private History<StorageElement> history = new(HistoryPointerType.TargetFrame);
        private bool isLoaded;

        private ExplorerViewModel explorer;
        private IFileSystemService fileSystem;
        private LoadStatus status;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabViewModel(ExplorerViewModel explorer, IFileSystemService fileSystem, string folderPath)
        {
            this.explorer = explorer;
            this.fileSystem = fileSystem;
            FolderPath = folderPath;
        }

        public void OnClick()
        {
            explorer.OnTabClicked(this);
        }
        public void Close()
        {
            explorer.OnTabClose(this);
        }
        public void SetActive(bool active)
        {
            IsActiveTab = active;
            this.RaisePropertyChanged(nameof(IsActiveTab));

            if (active)
            {
                if (isLoaded == false)
                {
                    isLoaded = true;
                    Open(new StorageElement(folderPath, fileSystem));
                }
                explorer.onFolderLoaded?.Invoke(status);
            }
        }

        public void Open(StorageElement storageElement)
        {
            if (storageElement.IsFolder)
            {
                history.Add(storageElement);
                SetPath(storageElement.Path);
            }
            else
            {
                fileSystem.Run(storageElement.Path);
            }
        }
        public void Back()
        {
            if (history.TryUndo(out StorageElement storageElement))
            {
                SetPath(storageElement.Path);
            }
        }
        public void Next()
        {
            if (history.TryRedo(out StorageElement storageElement))
            {
                SetPath(storageElement.Path);
            }
        }
        public void OnFilesChanged()
        {
            StorageElements = new(fileSystem.GetStorageElements(FolderPath, out LoadStatus status));

            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(StorageElements));
                explorer.onFolderLoaded?.Invoke(status);
            });
        }
        private void SetPath(string path)
        {
            FolderPath = path;
            StorageElements = new(fileSystem.GetStorageElements(path, out status));

            this.RaisePropertyChanged(nameof(FolderPath));
            this.RaisePropertyChanged(nameof(StorageElements));

            explorer.onFolderLoaded?.Invoke(status);
        }
    }
}