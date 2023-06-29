using Avalonia.Threading;
using FileFlow.Extensions;
using FileFlow.Providers;
using FileFlow.Services;
using FileFlow.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Zenject;

namespace FileFlow.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        public string FolderPath
        {
            get { return folderPath; }
            private set
            {
                folderPath = value;

                Title = new DirectoryInfo(folderPath).Name.CleanUp();
                if (Project != null) Title = Project.Name + ": " + Title;

                this.RaisePropertyChanged(nameof(Title));
            }
        }
        public string Title { get; private set; }
        public bool IsActiveTab { get; private set; }

        public ObservableCollection<StorageElement> StorageElementsValues { get; private set; }
        public Dictionary<string, StorageElement> StorageElements { get; set; } = new();
        public Project Project { get; private set; }


        private string folderPath;
        private History<StorageElement> history = new(HistoryPointerType.TargetFrame);
        private bool isLoaded;
        private StorageProdiver provider;

        [Inject] private IFileSystemService fileSystem;
        [Inject] private IIconExtractorService iconExtractor;
        [Inject] private ProjectService projectService;
        [Inject] private StorageProdiverFactory providerFactory;

        private ExplorerViewModel explorer;

        private LoadStatus status;
        private FileSystemWatcher watcher;

        private List<FileSystemWatcher> foldersWatchers = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabViewModel(ExplorerViewModel explorer, string folderPath)
        {
            FolderPath = folderPath;
            this.explorer = explorer;
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
            if (storageElement.IsFolder || ArchiveProvider.IsArchive(storageElement.Path))
            {
                history.Add(storageElement);
                SetPath(storageElement.Path);
            }
            else
            {
                provider.Run(storageElement.Path);
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
        public void MoveUp()
        {
            string path = Path.GetDirectoryName(folderPath);
            if (provider.Exists(path))
            {
                Open(new(path, fileSystem, iconExtractor));
            }
        }
        public void OnFilesChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                StorageElement element = new(e.FullPath, fileSystem, iconExtractor);
                element.IsAdded = true;
                StorageElements.Add(element.Name, element);
                status = LoadStatus.Ok;
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
                if (StorageElements.Count == 0) status = LoadStatus.Empty;
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
            provider = providerFactory.Create(path);

            // Update project
            if (provider is ProjectProvider projectProvider)
            {
                Project = projectProvider.Project;
                projectService.IndexProject(Project);
            }
            this.RaisePropertyChanged(nameof(Project));

            FolderPath = path;
            this.RaisePropertyChanged(nameof(FolderPath));

            ReloadElements();

            

            // Recreate (TODO: Make a pool for that) openned folder's folders watchers
            foreach (FileSystemWatcher folderWatcher in foldersWatchers)
            {
                folderWatcher.Dispose();
            }
            foldersWatchers.Clear();


            if (status is not (LoadStatus.Ok or LoadStatus.Empty))
            {
                return;
            }

            if (provider is LogicDiskProvider)
            {
                // Setup or update openned folder watcher
                if (watcher == null) watcher = SetupWatcher(path);
                else watcher.Path = path;

                foreach (StorageElement element in StorageElements.Values)
                {
                    if (element.IsFolder)
                    {
                        foldersWatchers.Add(SetupFolderWatcher(element.Path));
                    }
                }
            }

            Dispatcher.UIThread.Post(explorer.OnPathChanged);
        }
        private void ReloadElements()
        {
            StorageElements.Clear();
            foreach (StorageElement element in provider.GetElements(null, out status))
            {
                StorageElements.Add(element.Name, element);
            }
            SortElements();
            explorer.onFolderLoaded?.Invoke(status);
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

            // Check if we have access to the folder
            try
            {
                Directory.EnumerateFileSystemEntries(path).Any();
                watcher.EnableRaisingEvents = true;
            }
            catch (UnauthorizedAccessException)
            {
            }

            return watcher;
        }
        private void OnFoldersChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                string watcherFolderName = Path.GetFileName(((FileSystemWatcher)sender).Path);
                if (StorageElements.TryGetValue(watcherFolderName, out StorageElement element))
                {
                    if (Directory.Exists(element.Path))
                    {
                        element.Refresh();
                        element.IsModified = true;
                    }
                }
            });
        }
    }
}