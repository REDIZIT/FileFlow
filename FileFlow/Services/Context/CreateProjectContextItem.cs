using Zenject;

namespace FileFlow.ViewModels
{
    public class CreateProjectContextItem : ContextItem
    {
        public override string Text => "Создать проект";
        public override string IconPath => "Assets/Icons/setting.png";

        [Inject] private Settings settings;

        public override bool CanBeApplied(StorageElement target)
        {
            return target != null && target.IsFolder && settings.Projects.TryGetProjectAt(target.Path) == null;
        }
        public override void Apply(StorageElement element)
        {
            settings.Projects.CreateFromFolder(element);
            settings.Save();
        }
    }
}
