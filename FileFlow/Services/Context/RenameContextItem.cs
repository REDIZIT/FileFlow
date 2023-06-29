using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public class RenameContextItem : ContextItem
    {
        public override string Text => "Переименовать";
        public override string IconPath => "Assets/Icons/rename.png";

        [Inject] private ExplorerControl explorer;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.selected != null;
        }
        public override void Apply(ContextWorkspace workspace)
        {
            explorer.ShowFileCreationView(!workspace.selected.IsFolder, FileCreationView.Action.Rename);
        }
    }
}
