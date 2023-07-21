using FileFlow.Views;
using FileFlow.Views.Popups;
using Zenject;

namespace FileFlow.ViewModels
{
    public class CreateArchiveItem : ContextItem
    {
        public override string Text => "Создать архив";
        public override string IconPath => "Assets/Icons/new_archive.png";
        public override int Order => 50;

        [Inject] private CreateArchiveControl window;

        public override void Apply(ContextWorkspace workspace)
        {
            window.Show(workspace.parent.Path, workspace.mainSelected.Name, workspace.selected);
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            if (workspace.mainSelected == null) return false;

            return true;
        }
    }
}
