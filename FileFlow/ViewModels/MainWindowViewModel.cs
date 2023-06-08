using Avalonia.Controls;
using Avalonia.Input;
using DynamicData.Experimental;
using FileFlow.Services;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private FileSystemWatcher watcher;

        public IFileSystemService fileSystem;
        public IIconExtractorService iconExtractor;

        public MainWindowViewModel(IFileSystemService fileSystem, IIconExtractorService iconExtractor)
        {
            this.fileSystem = fileSystem;
            this.iconExtractor = iconExtractor;

            //DownloadItems.Add(new("C:/Tests/123.txt", fileSystem, iconExtractor));
            //DownloadItems.Add(new("C:/Tests/123.txt", fileSystem, iconExtractor));
            //DownloadItems.Add(new("C:/Tests/123.txt", fileSystem, iconExtractor));

            UpdateDownloadsWatcher();
        }
        public void CloseDownloadItem(StorageElement element)
        {
            DownloadItems.Remove(element);
            this.RaisePropertyChanged(nameof(DownloadItems));
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