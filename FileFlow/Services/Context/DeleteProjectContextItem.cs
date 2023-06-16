using Zenject;

namespace FileFlow.ViewModels
{
    public class DeleteProjectContextItem : ContextItem
    {
        public override string Text => "Удалить проект";
        public override string IconPath => "Assets/Icons/setting.png";

        [Inject] private Settings settings;

        public override bool CanBeApplied(StorageElement target)
        {
            return target != null && target.IsFolder && settings.Projects.TryGetProjectRootAt(target.Path) != null;
        }
        public override void Apply(StorageElement element)
        {
            settings.Projects.RemoveFromFolder(element);
            settings.Save();
        }
    }
}
