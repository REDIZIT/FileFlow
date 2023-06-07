using Avalonia.Threading;
using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

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

        public ObservableCollection<StorageElement> StorageElementsValues { get; private set; }
        public Dictionary<string, StorageElement> StorageElements { get; set; } = new();

        private string folderPath;
        private History<StorageElement> history = new(HistoryPointerType.TargetFrame);
        private bool isLoaded;

        private ExplorerViewModel explorer;
        private IFileSystemService fileSystem;
        private IIconExtractorService iconExtractor;
        private LoadStatus status;
        private FileSystemWatcher watcher;

        private List<FileSystemWatcher> foldersWatchers = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabViewModel(ExplorerViewModel explorer, IFileSystemService fileSystem, IIconExtractorService iconExtractor, string folderPath)
        {
            this.explorer = explorer;
            this.fileSystem = fileSystem;
            this.iconExtractor = iconExtractor;

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
                    Open(new StorageElement(folderPath, fileSystem, iconExtractor));
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
        public void OnFilesChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                StorageElement element = new(e.FullPath, fileSystem, iconExtractor);
                element.IsAdded = true;
                StorageElements.Add(element.Name, element);
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                RenamedEventArgs args = (RenamedEventArgs)e;
                StorageElement element = StorageElements[args.OldName];
                element.SetPath(args.FullPath.CleanUp());
                element.IsModified = true;

                StorageElements.Remove(args.OldName);
                StorageElements.Add(args.Name, element);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                StorageElements.Remove(e.Name);
            }

            Dispatcher.UIThread.Post(() =>
            {
                SortElements();
                this.RaisePropertyChanged(nameof(StorageElementsValues));
                explorer.onFolderLoaded?.Invoke(status);
            });
        }
        private void SetPath(string path)
        {
            FolderPath = path;
            this.RaisePropertyChanged(nameof(FolderPath));

            ReloadElements();

            explorer.onFolderLoaded?.Invoke(status);

            // Setup or update openned folder watcher
            if (watcher == null) watcher = SetupWatcher(path);
            else watcher.Path = path;

            // Recreate (TODO: Make a pool for that) openned folder's folders watchers
            foreach (FileSystemWatcher folderWatcher in foldersWatchers)
            {
                folderWatcher.Dispose();
            }
            foldersWatchers.Clear();

            foreach (StorageElement element in StorageElements.Values)
            {
                if (element.IsFolder)
                {
                    foldersWatchers.Add(SetupFolderWatcher(element.Path));
                }
            }
        }
        private void ReloadElements()
        {
            StorageElements.Clear();
            foreach (StorageElement element in fileSystem.GetStorageElements(FolderPath, out status))
            {
                StorageElements.Add(element.Name, element);
            }
            SortElements();
        }
        private void SortElements()
        {
            StorageElementsValues = new(StorageElements.Values.OrderBy(e => e.Name).OrderByDescending(e => e.IsFolder));
            this.RaisePropertyChanged(nameof(StorageElementsValues));
        }
        private FileSystemWatcher SetupWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Created += OnFilesChanged;
            watcher.Deleted += OnFilesChanged;
            watcher.Renamed += OnFilesChanged;
            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        private FileSystemWatcher SetupFolderWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Created += OnFoldersChanged;
            watcher.Deleted += OnFoldersChanged;
            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        private void OnFoldersChanged(object sender, FileSystemEventArgs e)
        {
            string watcherFolderName = Path.GetFileName(((FileSystemWatcher)sender).Path);
            StorageElement element = StorageElements[watcherFolderName];
            element.Refresh();
            element.IsModified = true;
        }
    }
}