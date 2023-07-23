using Avalonia.Controls;
using FileFlow.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            newFileButton.Click += (_, _) => explorer.ShowFileCreationWindow(true);
            newFolderButton.Click += (_, _) => explorer.ShowFileCreationWindow(false);
        }

        public void Open(ContextWorkspace workspace)
        {
            this.workspace = workspace;

            if (workspace.mainSelected != null && workspace.selected.Any(e => e.Path == workspace.mainSelected.Path) == false)
            {
                workspace.selected.Add(workspace.mainSelected);
            }

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
