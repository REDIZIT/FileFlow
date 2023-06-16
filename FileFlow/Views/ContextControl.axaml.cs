using Avalonia.Controls;
using FileFlow.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Zenject;

namespace FileFlow.Views
{
    public partial class ContextControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ContextItem> Items { get; set; }

        [Inject] public ContextService ContextService { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private DiContainer kernel;
        private ExplorerControl explorer;
        private StorageElement selectedElement;

        public ContextControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void Setup(DiContainer kernel, ExplorerControl explorer)
        {
            this.kernel = kernel;
            this.explorer = explorer;
            newFileButton.Click += (_, _) => explorer.ShowFileCreationView(true, FileCreationView.Action.Create);
            newFolderButton.Click += (_, _) => explorer.ShowFileCreationView(false, FileCreationView.Action.Create);
        }

        public void Open(StorageElement selectedElement)
        {
            this.selectedElement = selectedElement;
            Items = new(ContextService.GetContextItems(kernel, this, explorer, selectedElement));
            this.RaisePropertyChanged(nameof(Items));

            contextMenu.PlacementMode = PlacementMode.Pointer;
            contextMenu.Open();
        }
        public void Close()
        {
            contextMenu.Close();
        }
        public void OnClick(ContextItem item)
        {
            item.Apply(selectedElement);
        }
    }
}
