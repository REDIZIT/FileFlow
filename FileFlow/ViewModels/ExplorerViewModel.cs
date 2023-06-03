using Avalonia.Controls;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace FileFlow.Views
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public ObservableCollection<MenuItemViewModel> SubMenuItems { get; set; }
    }
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public ObservableCollection<StorageElement> StorageElements { get; set; }
        public ObservableCollection<PathBarHintViewModel> PathBarHints { get; set; }
        public ObservableCollection<MenuItemViewModel> ContextMenuItems { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Action<LoadStatus> onFolderLoaded;

        private History<StorageElement> history = new(HistoryPointerType.TargetFrame);
        private IFileSystemService fileSystem;


        public ExplorerViewModel(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;


            PathBarHints = new()
            {
                new() { DisplayText = "123", TypeText = "System" },
                new() { DisplayText = "234", TypeText = "App" },
                new() { DisplayText = "345", TypeText = "System" }
            };

            //ContextMenuItems = new()
            //{
            //    new() { Header = "1", Items = new List<MenuItem>()
            //    {
            //        new() { Header = "a" },
            //        new() { Header = "b" },
            //        new() { Header = "c" },
            //    } },
            //    new() { Header = "2" },
            //    new() { Header = "3" },
            //    new() { Header = "4" },
            //    new() { Header = "5" },
            //};

            //ContextMenuItems = new()
            //{
            //    "a", "b", "c"
            //};
            ContextMenuItems = new()
            {
                new() { Header = "1" },
                new() { Header = "2" },
                new() { Header = "3" },
            };
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

        private void SetPath(string path)
        {
            Path = path;
            StorageElements = new(fileSystem.GetStorageElements(path, out LoadStatus status));
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(StorageElements));
            onFolderLoaded?.Invoke(status);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
