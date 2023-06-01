using FileFlow.Services;
using FileFlow.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace FileFlow.Views
{
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public ObservableCollection<StorageElement> StorageElements { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private History<StorageElement> history = new(HistoryPointerType.TargetFrame);
        private IFileSystemService fileSystem;


        public ExplorerViewModel(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;
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
            StorageElements = new(fileSystem.GetStorageElements(path));
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(StorageElements));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
