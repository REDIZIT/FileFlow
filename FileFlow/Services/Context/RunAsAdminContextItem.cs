using FileFlow.Services;
using Zenject;

namespace FileFlow.ViewModels
{
    public class RunAsAdminContextItem : ContextItem
    {
        public override string Text => "Запустить от админа";
        public override string IconPath => "Assets/Icons/cmd.png";
        public override int Order => 0;

        [Inject] private IFileSystemService fileSystem;

        public override void Apply(ContextWorkspace workspace)
        {
            fileSystem.Run(workspace.mainSelected.Path, true);
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.mainSelected != null && workspace.mainSelected.IsFolder == false;
        }
    }
}
