﻿using Avalonia.Input;
using Avalonia.Threading;
using FileFlow.Enums;
using FileFlow.Misc;
using FileFlow.Providers;
using FileFlow.Services;
using FileFlow.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Zenject;
using static FileFlow.Views.FileCreationView;

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
        [Inject] private Settings settings;

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

        public void Open(StorageElement storageElement, string nameToSelect = null)
        {
            if (storageElement.IsFolder || ArchiveProvider.IsArchive(storageElement.Path))
            {
                history.Add(storageElement);
                SetPath(storageElement.Path);

                if (string.IsNullOrWhiteSpace(nameToSelect) == false)
                {
                    foreach (StorageElement e in StorageElementsValues)
                    {
                        if (e.Name == nameToSelect)
                        {
                            explorer.SelectElement(e);
                            break;
                        }
                    }
                }
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
                element.Refresh();

                StorageElements.Remove(args.OldName);
                StorageElements.Add(args.Name, element);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                StorageElements.Remove(e.Name);
                if (StorageElements.Count == 0) status = LoadStatus.Empty;
            }
            else if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                StorageElement element = StorageElements[e.Name];
                element.IsModified = true;
                element.Refresh();
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
            else
            {
                Project = null;
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
            DragDropEffects effects = ClipboardUtils.GetEffects();
            IEnumerable<string> copiedFiles = ClipboardUtils.EnumerateFiles();

            StorageElements.Clear();
            foreach (StorageElement element in provider.GetElements(folderPath, out status))
            {
                StorageElements.Add(element.Name, element);

                if (copiedFiles != null && copiedFiles.Any(c => c == element.Path))
                {
                    element.SetUnderAction(effects == DragDropEffects.Copy, effects == DragDropEffects.Move);
                }
            }
            SortElements();
            explorer.onFolderLoaded?.Invoke(status);
        }
        public void SortElements()
        {
            Sort type = settings.SortData.GetSort(FolderPath);
            IEnumerable<StorageElement> sorted = EnumerateSortElements(StorageElements.Values, type);

            if (type == Sort.CreationDate || type == Sort.CreationDataRev)
            {
                StorageElementsValues = new(sorted);
            }
            else
            {
                StorageElementsValues = new(sorted.OrderByDescending(e => e.IsFolder));
            }

            this.RaisePropertyChanged(nameof(StorageElementsValues));
        }
        public void RefreshElements()
        {
            foreach (StorageElement element in StorageElementsValues)
            {
                element.Refresh();
                element.IsAdded = false;
                element.IsModified = false;
            }
        }
        public void RefreshIcons()
        {
            foreach (StorageElement element in StorageElementsValues)
            {
                element.RefreshIcon();
            }
        }

        private IEnumerable<StorageElement> EnumerateSortElements(IEnumerable<StorageElement> source, Sort type)
        {
            if (source == null)
            {
                return Enumerable.Empty<StorageElement>();
            }

            return type switch
            {
                Sort.Name => source.OrderBy(e => e.Name),
                Sort.NameRev => source.OrderByDescending(e => e.Name),
                Sort.CreationDate => source.OrderByDescending(e => e.LastModifyTime),
                Sort.CreationDataRev => source.OrderBy(e => e.LastModifyTime),
                Sort.Size => source.OrderByDescending(e => e.Size),
                Sort.SizeRev => source.OrderBy(e => e.Size),
                _ => throw new NotImplementedException(),
            };
        }
        private FileSystemWatcher SetupWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            watcher.Created += OnFilesChanged;
            watcher.Deleted += OnFilesChanged;
            watcher.Renamed += OnFilesChanged;
            watcher.Changed += OnFilesChanged;
            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        private FileSystemWatcher SetupFolderWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            watcher.Created += OnFoldersChanged;
            watcher.Deleted += OnFoldersChanged;
            watcher.Changed += OnFoldersChanged;

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