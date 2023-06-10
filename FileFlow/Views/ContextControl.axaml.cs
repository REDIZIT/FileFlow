using Avalonia.Controls;

namespace FileFlow.Views
{
    public partial class ContextControl : UserControl
    {
        private ExplorerControl explorer;

        public ContextControl()
        {
            InitializeComponent();
        }

        public void Setup(ExplorerControl explorer)
        {
            this.explorer = explorer;
            newFileButton.Click += (_, _) => explorer.ShowFileCreationView(true, FileCreationView.Action.Create);
            newFolderButton.Click += (_, _) => explorer.ShowFileCreationView(false, FileCreationView.Action.Create);
        }

        public void Open()
        {
            contextMenu.PlacementMode = PlacementMode.Pointer;
            contextMenu.Open();
        }
        public void Close()
        {
            contextMenu.Close();
        }
    }
}
