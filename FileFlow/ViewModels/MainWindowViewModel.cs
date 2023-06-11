using Avalonia.Controls;
using Avalonia.Input;
using DynamicData.Experimental;
using FileFlow.Services;
using FileFlow.Views;
using Ninject;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<StorageElement> DownloadItems { get; set; } = new();
        public SidebarViewModel SidebarModel { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private FileSystemWatcher watcher;
        public ExplorerControl activeExplorer;

        public IFileSystemService fileSystem;
        public IIconExtractorService iconExtractor;

        public MainWindowViewModel(IKernel kernel)
        {
            fileSystem = kernel.Get<IFileSystemService>();
            iconExtractor = kernel.Get<IIconExtractorService>();

            SidebarModel = new SidebarViewModel(this, kernel);

            UpdateDownloadsWatcher();
        }
        public void CloseDownloadItem(StorageElement element)
        {
            DownloadItems.Remove(element);
            this.RaisePropertyChanged(nameof(DownloadItems));
        }
        public void SetActiveExplorer(ExplorerControl explorer)
        {
            activeExplorer = explorer;
        }
        private void UpdateDownloadsWatcher()
        {
            string path = KnownFolders.GetPath(KnownFolder.Downloads);
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(path);
                watcher.Filter = "*.*";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Created += OnDownloadsChanged;
                watcher.Renamed += OnDownloadsChanged;
                watcher.Deleted += OnDownloadsChanged;
                watcher.EnableRaisingEvents = true;
            }
            else
            {
                watcher.Path = path;
            }
        }
        private void OnDownloadsChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                DownloadItems.Add(new(e.FullPath, fileSystem, iconExtractor));
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                RenamedEventArgs args = (RenamedEventArgs)e;
                StorageElement element = DownloadItems.FirstOrDefault(e => e.Name == args.OldName);
                if (element != null)
                {
                    element.SetPath(args.FullPath);
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                foreach (var element in DownloadItems.ToArray())
                {
                    if (element.Name == e.Name)
                    {
                        DownloadItems.Remove(element);
                    }
                }
            }

            this.RaisePropertyChanged(nameof(DownloadItems));
        }
    }
}