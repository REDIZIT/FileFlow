using Zenject;

namespace FileFlow.ViewModels
{
    public class DeleteProjectContextItem : ContextItem
    {
        public override string Text => "Удалить проект";
        public override string IconPath => "Assets/Icons/setting.png";
        public override int Order => 100;

        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.selected != null && workspace.selected.IsFolder && settings.Projects.TryGetProjectRootAt(workspace.selected.Path) != null;
        }
        public override void Apply(ContextWorkspace workspace)
        {
            settings.Projects.RemoveFromFolder(workspace.selected);
            settings.Save();
        }
    }
}
