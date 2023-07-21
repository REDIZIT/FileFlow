using Zenject;

namespace FileFlow.ViewModels
{
    public class CreateProjectContextItem : ContextItem
    {
        public override string Text => "Создать проект";
        public override string IconPath => "Assets/Icons/setting.png";
        public override int Order => 100;

        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.mainSelected != null && workspace.mainSelected.IsFolder && settings.Projects.TryGetProjectAt(workspace.mainSelected.Path) == null;
        }
        public override void Apply(ContextWorkspace workspace)
        {
            settings.Projects.CreateFromFolder(workspace.mainSelected);
            settings.Save();
        }
    }
}
