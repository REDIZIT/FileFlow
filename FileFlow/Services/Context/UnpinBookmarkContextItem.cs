using Zenject;

namespace FileFlow.ViewModels
{
    public class UnpinBookmarkContextItem : ContextItem
    {
        public override string Text => "Убрать из закладок";
        public override string IconPath => "Assets/Icons/pin.png";
        public override int Order => 5;

        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return settings.Bookmarks.Contains(workspace.mainSelected.Path);
        }
        public override void Apply(ContextWorkspace workspace)
        {
            settings.Bookmarks.Remove(workspace.mainSelected.Path);
            settings.Save();
        }
    }
}
