using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public class RenameContextItem : ContextItem
    {
        public override string Text => "Переименовать";
        public override string IconPath => "Assets/Icons/rename.png";

        [Inject] private ExplorerControl explorer;

        public override bool CanBeApplied(StorageElement target)
        {
            return true;
        }
        public override void Apply(StorageElement elemet)
        {
            explorer.ShowFileCreationView(!elemet.IsFolder, FileCreationView.Action.Rename);
        }
    }
}
