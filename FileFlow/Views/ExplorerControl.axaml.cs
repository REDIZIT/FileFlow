using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private ExplorerViewModel model;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(IFileSystemService fileSystem, string path)
        {
            model = new(fileSystem);
            DataContext = model;
            model.SetPath(path);

            InitializeComponent();
        }

        public void Click(object sender, PointerPressedEventArgs el)
        {
            if (el.ClickCount == 2 && el.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                StorageElement storageElement = (StorageElement)((Control)el.Source).Tag;
                model.SetPath(storageElement.Path);
            }
        }
    }
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public ObservableCollection<StorageElement> StorageElements { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private IFileSystemService fileSystem;

        public ExplorerViewModel(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void SetPath(string path)
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
