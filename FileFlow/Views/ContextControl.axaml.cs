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
        public event PropertyChangedEventHandler? PropertyChanged;

        [Inject] private ExplorerControl explorer;
        [Inject] private ContextService contextService;
        [Inject] private DiContainer container;

        private ContextWorkspace workspace;

        public ContextControl()
        {
            DataContext = this;
            InitializeComponent();
            newFileButton.Click += (_, _) => explorer.ShowFileCreationView(true, FileCreationView.Action.Create);
            newFolderButton.Click += (_, _) => explorer.ShowFileCreationView(false, FileCreationView.Action.Create);
        }

        public void Open(ContextWorkspace workspace)
        {
            this.workspace = workspace;
            Items = new(contextService.GetContextItems(container, this, workspace));
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
            item.Apply(workspace);
            Close();
        }
    }
}
