using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileFlow.Views
{
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public TabViewModel ActiveTab { get; private set; }
        public ObservableCollection<PathBarHintViewModel> PathBarHints { get; set; }
        public ObservableCollection<TabViewModel> Tabs { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public Action<LoadStatus> onFolderLoaded;

        private FileSystemWatcher watcher;
        private IFileSystemService fileSystem;

        public ExplorerViewModel(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;

            Tabs.Add(new TabViewModel(this, fileSystem, "C:\\Users\\REDIZIT\\Documents\\GitHub\\FileFlow"));
            Tabs.Add(new TabViewModel(this, fileSystem, "C:\\Users\\REDIZIT"));
            Tabs.Add(new TabViewModel(this, fileSystem, "C:\\Users\\REDIZIT"));
            Tabs.Add(new TabViewModel(this, fileSystem, "C:\\Users\\REDIZIT"));
            Tabs.Add(new TabViewModel(this, fileSystem, "C:\\Users\\REDIZIT"));
            OnTabClicked(Tabs[0]);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tabs)));

            PathBarHints = new()
            {
                new() { DisplayText = "123", TypeText = "System" },
                new() { DisplayText = "234", TypeText = "App" },
                new() { DisplayText = "345", TypeText = "System" }
            };
        }
        
        public void Open(StorageElement storageElement)
        {
            ActiveTab.Open(storageElement);
            UpdateFileWatcher();
        }
        public void Back()
        {
            ActiveTab.Back();
            UpdateFileWatcher();
        }
        public void Next()
        {
            ActiveTab.Next();
            UpdateFileWatcher();
        }
        public void CreateTab(StorageElement storageElement)
        {
            Tabs.Add(new TabViewModel(this, fileSystem, storageElement.Path));
            OnTabClicked(Tabs.Last());
        }
        public void OnTabClicked(TabViewModel tab)
        {
            ActiveTab = tab;
            this.RaisePropertyChanged(nameof(ActiveTab));
            UpdateFileWatcher();

            foreach (TabViewModel item in Tabs)
            {
                item.SetActive(item == ActiveTab);
            }
        }
        public void OnTabClose(TabViewModel tab)
        {
            if (Tabs.Count <= 1) return;

            if (tab == ActiveTab)
            {
                int index = Tabs.IndexOf(ActiveTab);
                Tabs.RemoveAt(index);
                int indexToOpen = Math.Min(index, Tabs.Count - 1);
                OnTabClicked(Tabs[indexToOpen]);
            }
            else
            {
                Tabs.Remove(tab);
            }
            this.RaisePropertyChanged(nameof(Tabs));
        }
        private void UpdateFileWatcher()
        {
            string path = ActiveTab.FolderPath;
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(path);
                watcher.Filter = "*.*";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Created += FilesChanged;
                watcher.Deleted += FilesChanged;
                watcher.Renamed += FilesChanged;
                watcher.EnableRaisingEvents = true;
            }
            else
            {
                watcher.Path = path;
            }
        }
        private void FilesChanged(object sender, FileSystemEventArgs e)
        {
            ActiveTab.OnFilesChanged();          
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
